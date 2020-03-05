using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.DI;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.Swather
{
    public class TerrainSwather : SwatherBase
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<TerrainSwather>();

        private static readonly ICell_NonStatic_MutationHook Hook = DIContext.Obtain<ICell_NonStatic_MutationHook>();

        /// <summary>
        /// The maximum number of cell passes that may be generated when swathing a single interval between
        /// two measurement epochs
        /// </summary>
        private const int MaxNumberCellPassesPerSwathingEpoch = 25000;

        private readonly BoundingWorldExtent3D _swathBounds = new BoundingWorldExtent3D();

        private int _processedEpochNumber;
        public int ProcessedEpochNumber { get => _processedEpochNumber; set => _processedEpochNumber = value; }

        public TerrainSwather(TAGProcessorBase processor,
                              IProductionEventLists machineTargetValueChanges,
                              ISiteModel siteModel,
                              IServerSubGridTree grid,
                              Fence interpolationFence) : base(processor, machineTargetValueChanges, siteModel, grid, interpolationFence)
        {
        }

        public override bool PerformSwathing(SimpleTriangle heightInterpolator1,
                                             SimpleTriangle heightInterpolator2,
                                             SimpleTriangle timeInterpolator1,
                                             SimpleTriangle timeInterpolator2,
                                             bool halfPass,
                                             PassType passType,
                                             MachineSide machineSide)
        {
            _processedEpochNumber++;

            // MinX/Y, MaxX/Y describe the world coordinate rectangle the encompasses
            // the pair of epochs denoting a processing interval.
            // Calculate the grid coverage of the bounding rectangle for the
            // quadrilateral held in the fence
            InterpolationFence.UpdateExtents();
            InterpolationFence.GetExtents(out _swathBounds.MinX, out _swathBounds.MinY, out _swathBounds.MaxX, out _swathBounds.MaxY);

            Hook?.EmitNote($"Interpolation extents: {_swathBounds.MinX:F3},{_swathBounds.MinY:F3} --> {_swathBounds.MaxX:F3},{_swathBounds.MaxY:F3}");
 
            // SIGLogMessage.PublishNoODS(Self,
            //                            Format('Swathing over rectangle: (%.3f, %.3f) -> (%.3f, %.3f) [%.3f wide by %.3f tall]', {SKIP}
            //                                   [fMinX, fMinY, fMaxX, fMaxY, fMaxX - fMinX, fMaxY - fMinY]),
            //                            slmcDebug);

            // We assume that we have a pair of epochs to compute IC information between
            // Determine the rectangle of cells that overlap the interval between the two epochs
            Grid.CalculateRegionGridCoverage(_swathBounds, out var cellExtent);

            // Check that the swathing of this epoch will not create an inordinate number of cell passes
            // If so, prevent swathing of this epoch interval
            var swatchCellCount = (long)cellExtent.SizeX * (long)cellExtent.SizeY;
            if (swatchCellCount > MaxNumberCellPassesPerSwathingEpoch)
            {
                Log.LogError($"Epoch {ProcessedEpochNumber} cell extents {cellExtent} (SizeX={cellExtent.SizeX}, SizeY={cellExtent.SizeX}) cover too many cell passes to swath ({swatchCellCount}), limit is {MaxNumberCellPassesPerSwathingEpoch} per epoch");
                return true;
            }

            if (Hook != null)
            {
              Hook.EmitNote($"Swathing: {cellExtent.MinX},{cellExtent.MinY} --> {cellExtent.MaxX},{cellExtent.MaxY}");

              // Emit count of cells matching quad boundary
              var cellCount = 0;

              // Scan the rectangle of grid cells, checking which of those fall within the quadrilateral
              for (var I = cellExtent.MinX; I <= cellExtent.MaxX; I++)
              {
                for (var J = cellExtent.MinY; J <= cellExtent.MaxY; J++)
                {
                  Grid.GetCellCenterPosition(I, J, out var gridX, out var gridY);

                  if (InterpolationFence.IncludesPoint(gridX, gridY))
                    cellCount++;
                }
              }

              Hook.EmitNote($"Potential CellCount={cellCount}");
            }

            // Scan the rectangle of grid cells, checking which of those fall within the quadrilateral
            for (var I = cellExtent.MinX; I <= cellExtent.MaxX; I++)
            {
                for (var J = cellExtent.MinY; J <= cellExtent.MaxY; J++)
                {
                    Grid.GetCellCenterPosition(I, J, out var gridX, out var gridY);

                    if (InterpolationFence.IncludesPoint(gridX, gridY))
                    {
                        var timeVal = Consts.NullDouble;
                        var heightVal = Consts.NullDouble;

                        if (timeInterpolator1.IncludesPoint(gridX, gridY)) 
                        {
                            timeVal = timeInterpolator1.InterpolateHeight(gridX, gridY);
                            heightVal = heightInterpolator1.InterpolateHeight(gridX, gridY);
                        }
                        else if (timeInterpolator2.IncludesPoint(gridX, gridY)) 
                        {
                            timeVal = timeInterpolator2.InterpolateHeight(gridX, gridY);
                            heightVal = heightInterpolator2.InterpolateHeight(gridX, gridY);
                        }

                        var haveInterpolation = timeVal != Consts.NullDouble && heightVal != Consts.NullDouble;
                        if (!haveInterpolation)
                        {
                            continue;  // We do not want to record this pass in this cell
                        }

                        var theTime = DateTime.SpecifyKind(DateTime.FromOADate(timeVal), DateTimeKind.Utc);
                        var theHeight = (float)heightVal;

                        //if (_TheTime.ToString("yyyy-MM-dd HH-mm-ss.fff") == "2012-11-07 00-12-38.330")
                        //{
                        //  double d = TimeInterpolator2.InterpolateHeight(GridX, GridY);                                   
                        //}
                        
                        // Check to see if the blade-on-the-ground flag is set. if not, then we will not process this epoch.
                        // The reason for this is that there is no useful information for us while the blade is not on the ground.
                        // There is a counter-argument to this in that customers may use a supervisor system to do an initial
                        // topo survey of the site, in which case this flag may not be set. Currently this is not supported.
                        // If you want the data processed, set the blade-on-the-ground flag.

                        if (!(passType == PassType.Track || passType == PassType.Wheel))
                        {
                            if (Processor.OnGrounds.GetValueAtDateTime(theTime, OnGroundState.No) == OnGroundState.No)
                                continue;
                        }

                        // Fill in all the details for the processed cell pass, using the tag event lookups
                        // to make sure the appropriate values at the cell pass time are used.

                        var processedCellPass = Cells.CellPass.CLEARED_CELL_PASS;

                        if (BaseProductionDataSupportedByMachine)
                        {
                            // Prepare a processed pass record to include in the cell
                            processedCellPass.InternalSiteModelMachineIndex = MachineTargetValueChanges.InternalSiteModelMachineIndex;
                            processedCellPass.Time = theTime;
                            processedCellPass.Height = theHeight;

                            if (CompactionDataSupportedByMachine)
                            {
                                processedCellPass.MDP = Processor.ICMDPValues.GetValueAtDateTime(theTime, CellPassConsts.NullMDP);

                                //    ProcessedCellPass.CCA = Processor.ICCCAValues.GetCCAValueAtDateTime(_TheTime);
                            
                                // CCA values can come from 5 different fields in the TAG files. Earlier versions had a single CCA value for all
                                // four wheels. As of GCS v13.12, all four wheels have CCA independently reported for each wheel
                                // (see TAG file schema for more details)
                                processedCellPass.CCA = Processor.SelectCCAValue(theTime, passType, machineSide);
                            
                                // If VibeState is not On, then any CCV info etc is invalid, and should be recorded as appropriate null values
                                // temp bug fix AJR
                                if (MachineTargetValueChanges.VibrationStateEvents.GetValueAtDate(processedCellPass.Time, out _, VibrationState.Invalid) == VibrationState.On)
                                {
                                    processedCellPass.CCV = Processor.ICCCVValues.GetValueAtDateTime(theTime, CellPassConsts.NullCCV);
                                    processedCellPass.RMV = Processor.ICRMVValues.GetValueAtDateTime(theTime, CellPassConsts.NullRMV);
                                    processedCellPass.Frequency = Processor.ICFrequencys.GetValueAtDateTime(theTime, CellPassConsts.NullFrequency);
                                    processedCellPass.Amplitude = Processor.ICAmplitudes.GetValueAtDateTime(theTime, CellPassConsts.NullAmplitude);
                                }
                            }

                            processedCellPass.RadioLatency = Processor.AgeOfCorrections.GetValueAtDateTime(theTime, CellPassConsts.NullRadioLatency);
                            processedCellPass.MaterialTemperature = Processor.ICTemperatureValues.GetValueAtDateTime(theTime, CellPassConsts.NullMaterialTemperatureValue);

                            var machineSpd = Processor.ICMachineSpeedValues.GetValueAtDateTime(theTime, Consts.NullDouble);
                            if (machineSpd == Consts.NullDouble)
                            {
                                machineSpd = Processor.CalculatedMachineSpeed;
                            }

                            // MachineSpeed is meters per second - we need to convert this to
                            // centimeters per seconds for the cell pass
                            if (machineSpd != Consts.NullDouble &&
                                machineSpd < 65535.0 / 100.0) // Machine too fast (its > 2358 km/hr)
                            {
                                processedCellPass.MachineSpeed = (ushort)Math.Round(machineSpd * 100);
                            }

                            processedCellPass.gpsMode = Processor.GPSModes.GetValueAtDateTime(theTime, CellPassConsts.NullGPSMode);

                            processedCellPass.HalfPass = halfPass;
                            processedCellPass.PassType = passType;

                            // We have now assembled the cell pass - add it to the model
                            // SIGLogMessage.PublishNoODS(Self,
                            //                            Format('Committing cell pass at %dx%d to model', {SKIP}
                            //                                   [I, J]),
                            //                            slmcDebug);

                            //if (ProcessedCellPass.Time.ToString("yyyy-MM-dd HH-mm-ss.fff") == "2012-11-07 00-12-38.330")
                            //  ProcessedCellPass = ProcessedCellPass;

                            CommitCellPassToModel(I, J, gridX, gridY, processedCellPass);
                        }
                    }
                }
            }

            return true;
        }
    }
}
