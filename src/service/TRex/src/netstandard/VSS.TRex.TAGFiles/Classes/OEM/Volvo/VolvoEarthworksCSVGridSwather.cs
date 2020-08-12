using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;
using VSS.TRex.TAGFiles.Classes.Swather;

namespace VSS.TRex.TAGFiles.Classes.OEM.Volvo
{
  public class VolvoEarthworksCSVGridSwather : SwatherBase
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<VolvoEarthworksCSVGridSwather>();

    private int _processedEpochNumber;
    public int ProcessedEpochNumber { get => _processedEpochNumber; set => _processedEpochNumber = value; }

    public VolvoEarthworksCSVGridSwather(TAGProcessorBase processor,
                          IProductionEventLists machineTargetValueChanges,
                          ISiteModel siteModel,
                          IServerSubGridTree grid,
                          Fence interpolationFence) : base(processor, machineTargetValueChanges, siteModel, grid, interpolationFence)
    {
    }

    public bool SwathSingleCell(bool halfPass, PassType passType, 
                                double cellCenterX, double cellCenterY, double cellWidth, VolvoEarthworksCSVRecord cellPass)
    {
      _processedEpochNumber++;

      // Determine the rectangle of cells that overlap the given cell
      var _swathBounds = new BoundingWorldExtent3D(cellCenterX - cellWidth, cellCenterY - cellWidth, cellCenterX + cellWidth, cellCenterY + cellWidth);
      Grid.CalculateRegionGridCoverage(_swathBounds, out var cellExtent);

      // Scan the rectangle of grid cells, checking which of those fall within the quadrilateral
      for (var I = cellExtent.MinX; I <= cellExtent.MaxX; I++)
      {
        for (var J = cellExtent.MinY; J <= cellExtent.MaxY; J++)
        {
          Grid.GetCellCenterPosition(I, J, out var gridX, out var gridY);

          var theTime = DateTime.SpecifyKind(cellPass.Time, DateTimeKind.Utc);

          // Volvo CSV files do not have height available yet, just use 0.0 for now
          var theHeight = 0.0f;

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

            CommitCellPassToModel(I, J, gridX, gridY, processedCellPass);
          }
        }
      }

      return true;
    }

    public override bool PerformSwathing(SimpleTriangle heightInterpolator1,
                                         SimpleTriangle heightInterpolator2,
                                         SimpleTriangle timeInterpolator1,
                                         SimpleTriangle timeInterpolator2,
                                         bool halfPass,
                                         PassType passType,
                                         MachineSide machineSide)
    {
      // True swathing not support for Volvo machines - gridded CSV only
      return false;
    }
  }
}
