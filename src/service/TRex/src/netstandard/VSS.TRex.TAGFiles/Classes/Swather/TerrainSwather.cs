using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VSS.TRex.Cells;
using VSS.TRex.Common;
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

        /// <summary>
        /// The maximum number of cell passes that may be generated when swathing a single interval between
        /// two measurement epochs
        /// </summary>
        private const int kMaxNumberCellPassesPerSwathingEpoch = 25000;

        private BoundingWorldExtent3D swathBounds = new BoundingWorldExtent3D();

        public int ProcessedEpochNumber { get; set; }

        public TerrainSwather(TAGProcessorBase processor,
                              IProductionEventLists machineTargetValueChanges,
                              ISiteModel siteModel,
                              IServerSubGridTree grid,
                              Fence interpolationFence) : base(processor, machineTargetValueChanges, siteModel, grid, interpolationFence)
        {
        }

        public override bool PerformSwathing(SimpleTriangle HeightInterpolator1,
                                             SimpleTriangle HeightInterpolator2,
                                             SimpleTriangle TimeInterpolator1,
                                             SimpleTriangle TimeInterpolator2,
                                             bool HalfPass,
                                             PassType passType,
                                             MachineSide machineSide)
        {
            DateTime _TheTime = DateTime.MinValue;
            float _TheHeight = Consts.NullHeight;

            // FMinX/Y, FMaxX/Y describe the world coordinate rectangle the encompasses
            // the pair of epochs denoting a processing interval.

            try
            {
                // Calculate the grid coverage of the bounding rectangle for the
                // quadrilateral held in the fence
                InterpolationFence.UpdateExtents();
                InterpolationFence.GetExtents(out swathBounds.MinX, out swathBounds.MinY, out swathBounds.MaxX, out swathBounds.MaxY);

                // SIGLogMessage.PublishNoODS(Self,
                //                            Format('Swathing over rectangle: (%.3f, %.3f) -> (%.3f, %.3f) [%.3f wide by %.3f tall]', {SKIP}
                //                                   [fMinX, fMinY, fMaxX, fMaxY, fMaxX - fMinX, fMaxY - fMinY]),
                //                            slmcDebug);

                // We assume that we have a pair of epochs to compute IC information between
                // Determine the rectangle of cells that overlap the interval between the two epochs
                if (!Grid.CalculateRegionGridCoverage(swathBounds, out BoundingIntegerExtent2D CellExtent))
                {
                    return true;
                }

                Debug.Assert(swathBounds.IsValidPlanExtent, "Invalid rectangle for processing cell passes over");

                // Check that the swathing of this epoch will not create an inordinate number of cell passes
                // If so, prevent swathing of this epoch interval
                long CellCount = (long)(CellExtent.MaxX - CellExtent.MinX) * (long)(CellExtent.MaxY - CellExtent.MinY);
                if (CellCount > kMaxNumberCellPassesPerSwathingEpoch)
                {
                    Log.LogError($"Epoch {ProcessedEpochNumber} cell extents {CellExtent} (SizeX={CellExtent.SizeX}, SizeY={CellExtent.SizeX}) cover too many cell passes to swath ({CellCount}), limit is {kMaxNumberCellPassesPerSwathingEpoch} per epoch");
                    return true;
                }

                // Scan the rectangle of grid cells, checking which of those fall within the quadrilateral
                for (uint I = (uint)CellExtent.MinX; I < CellExtent.MaxX + 1; I++)
                {
                    for (uint J = (uint)CellExtent.MinY; J < CellExtent.MaxY + 1; J++)
                    {
                        Grid.GetCellCenterPosition((uint)I, (uint)J, out double GridX, out double GridY);

                        if (InterpolationFence.IncludesPoint(GridX, GridY))
                        {
                            bool haveInterpolation = false;

                            if (/*!haveInterpolation && */TimeInterpolator1.IncludesPoint(GridX, GridY)) 
                            {
                                double timeVal = TimeInterpolator1.InterpolateHeight(GridX, GridY);
                                double heightVal = HeightInterpolator1.InterpolateHeight(GridX, GridY);

                                haveInterpolation = timeVal != Consts.NullDouble && heightVal != Consts.NullDouble;

                                if (haveInterpolation)
                                { 
                                    _TheTime = DateTime.FromOADate(timeVal);
                                    _TheHeight = (float)heightVal;
                                }
                            }

                            if (!haveInterpolation && TimeInterpolator2.IncludesPoint(GridX, GridY)) 
                            {
                                double timeVal = TimeInterpolator2.InterpolateHeight(GridX, GridY);
                                double heightVal = HeightInterpolator2.InterpolateHeight(GridX, GridY);

                                haveInterpolation = timeVal != Consts.NullDouble && heightVal != Consts.NullDouble;

                                if (haveInterpolation)
                                {
                                    _TheTime = DateTime.FromOADate(timeVal);
                                    _TheHeight = (float)heightVal;
                                }
                            }
                            
                            if (!haveInterpolation)
                            {
                                continue;  // We do not want to record this pass in this cell
                            }

                            // Check to see if the blade-on-the-ground flag is set. if not, then we will not process this epoch.
                            // The reason for this is that there is no useful information for us while the blade is not on the ground.
                            // There is a counter-argument to this in that customers may use a supervisor system to do an initial
                            // topo survey of the site, in which case this flag may not be set. Curently this is not supported.
                            // If you want the data processed, set the blade-on-the-ground flag.

                            if (!(passType == PassType.Track || passType == PassType.Wheel))
                            {
                                if (Processor.OnGrounds.GetOnGroundAtDateTime(_TheTime) == OnGroundState.No)
                                    continue;
                            }

                            // Fill in all the details for the processed cell pass, using the tag event lookups
                            // to make sure the appropriate values at the cell pass time are used.

                            CellPass ProcessedCellPass = new CellPass();

                            if (BaseProductionDataSupportedByMachine)
                            {
                                // Prepare a processed pass record to include in the cell
                                //ProcessedCellPass.MachineID = MachineID;
                                ProcessedCellPass.InternalSiteModelMachineIndex = InternalSiteModelMachineIndex;
                                ProcessedCellPass.Time = _TheTime;
                                ProcessedCellPass.Height = _TheHeight;

                                if (CompactionDataSupportedByMachine)
                                {
                                    ProcessedCellPass.MDP = Processor.ICMDPValues.GetMDPValueAtDateTime(_TheTime);

                                   //    ProcessedCellPass.CCA = Processor.ICCCAValues.GetCCAValueAtDateTime(_TheTime);

                                  // CCA values can come from 5 different fields in the TAG files. Earlier versions had a single CCA value for all
                                  // four wheels. As of GCS v13.12, all four wheels have CCA independently reported for each wheel
                                  // (see TAG file schema for more details)
                                  ProcessedCellPass.CCA = Processor.SelectCCAValue(_TheTime, passType, machineSide);

                                  // If VibeState is not On, then any CCV info etc is invalid, and should be recorded as appropriate null values
                                  // temp bug fix AJR
                                  if (MachineTargetValueChanges.VibrationStateEvents.GetValueAtDate(ProcessedCellPass.Time, out int StateChangeIndex, VibrationState.Invalid) == VibrationState.On)
                                    {
                                        ProcessedCellPass.CCV = Processor.ICCCVValues.GetCCVValueAtDateTime(_TheTime);
                                        ProcessedCellPass.RMV = Processor.ICRMVValues.GetRMVValueAtDateTime(_TheTime);
                                        ProcessedCellPass.Frequency = Processor.ICFrequencys.GetFrequencyValueAtDateTime(_TheTime);
                                        ProcessedCellPass.Amplitude = Processor.ICAmplitudes.GetAmplitudeValueAtDateTime(_TheTime);
                                    }
                                }

                                ProcessedCellPass.RadioLatency = Processor.AgeOfCorrections.GetAgeOfCorrectionValueAtDateTime(_TheTime);
                                ProcessedCellPass.MaterialTemperature = Processor.ICTemperatureValues.GetMaterialTemperatureValueAtDateTime(_TheTime);

                                double MachineSpd = Processor.ICMachineSpeedValues.GetMachineSpeedValueAtDateTime(_TheTime);
                                if (MachineSpd == Consts.NullDouble)
                                {
                                    MachineSpd = Processor.CalculatedMachineSpeed;
                                }

                                // MachineSpeed is meters per second - we need to convert this to
                                // centimeters per seconds for the cell pass
                                if ((MachineSpd != Consts.NullDouble) &&
                                    (MachineSpd < (65535.0 / 100.0))) // Machine too fast (its > 2358 km/hr)
                                {
                                    ProcessedCellPass.MachineSpeed = (ushort)Math.Round(MachineSpd * 100);
                                }

                                ProcessedCellPass.gpsMode = Processor.GPSModes.GetGPSModeAtDateTime(_TheTime);

                                ProcessedCellPass.HalfPass = HalfPass;
                                ProcessedCellPass.PassType = passType;

                                // We have now assembled the cell pass - add it to the model
                                // SIGLogMessage.PublishNoODS(Self,
                                //                            Format('Committing cell pass at %dx%d to model', {SKIP}
                                //                                   [I, J]),
                                //                            slmcDebug);

                                CommitCellPassToModel(I, J, GridX, GridY, ProcessedCellPass);
                            }
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Log.LogError(E, "Exception in TerrainSwather.PerformSwathing:");
                return false;
            }

            return true;
        }
    }
}
