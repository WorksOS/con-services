using System;
using System.Collections;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Events.Models;
using VSS.TRex.Filters.Models;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
// ReSharper disable IdentifierTypo

namespace VSS.TRex.SubGridTrees.Client
{
  public class ClientCellProfileLeafSubgrid : GenericClientLeafSubGrid<ClientCellProfileLeafSubgridRecord>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<ClientCellProfileLeafSubgrid>();

    /// <summary>
    /// Initialise the null cell values for the client sub grid
    /// </summary>
    static ClientCellProfileLeafSubgrid()
    {
      var nullRecord = ClientCellProfileLeafSubgridRecord.Null();
      SubGridUtilities.SubGridDimensionalIterator((x, y) => NullCells[x, y] = nullRecord);
    }

    public override bool WantsLiftProcessingResults() => true;

    private void Initialise()
    {
      _gridDataType = GridDataType.CellProfile;

      EventPopulationFlags |= 
        PopulationControlFlags.WantsTargetPassCountValues |
        PopulationControlFlags.WantsTargetCCVValues |
        PopulationControlFlags.WantsTargetMDPValues |
        PopulationControlFlags.WantsEventGPSModeValues |
        PopulationControlFlags.WantsEventGPSAccuracyValues |
        PopulationControlFlags.WantsTargetThicknessValues |
        PopulationControlFlags.WantsEventVibrationStateValues |
        PopulationControlFlags.WantsEventMachineGearValues |
        PopulationControlFlags.WantsEventDesignNameValues;
    }

    public ClientCellProfileLeafSubgrid()
    {
      Initialise();
    }

    /*
    /// <summary>
    /// Constructor. Set the grid to CellProfile.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    public ClientCellProfileLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    {
      Initialise();
    }
    */

    public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue)
    {
      switch (ProfileDisplayMode)
      {
        case DisplayMode.CCVPercent:
        case DisplayMode.CCVSummary:
        case DisplayMode.CCVPercentSummary:
          return filteredValue.FilteredPass.CCV == CellPassConsts.NullCCV;

        case DisplayMode.MDPPercent:
        case DisplayMode.MDPSummary:
        case DisplayMode.MDPPercentSummary:
          return filteredValue.FilteredPass.MDP == CellPassConsts.NullMDP;

        default:
          return filteredValue.FilteredPass.Time == Consts.MIN_DATETIME_AS_UTC;
      }
    }

    public override void Clear()
    {
      Array.Copy(NullCells, Cells, SubGridTreeConsts.CellsPerSubGrid);

      TopLayerOnly = false;
      ProfileDisplayMode = DisplayMode.Height;
    }

    public override void FillWithTestPattern()
    {
      ForEach((x, y) =>
      {
        Cells[x, y] = new ClientCellProfileLeafSubgridRecord
        {
          LastPassTime = DateTime.SpecifyKind(new DateTime(x * 1000 + y + 1), DateTimeKind.Utc),
          PassCount = x + y
        };
      });
    }

    public override bool LeafContentEquals(IClientLeafSubGrid other)
    {
      var result = true;

      // ReSharper disable once InconsistentNaming
      var _other = (IGenericClientLeafSubGrid<ClientCellProfileLeafSubgridRecord>)other;
      ForEach((x, y) => result &= Cells[x, y].Equals(_other.Cells[x, y]));

      return result;
    }

    public override ClientCellProfileLeafSubgridRecord NullCell()
    {
      var nullRecord = new ClientCellProfileLeafSubgridRecord();
      nullRecord.Clear();
      return nullRecord;
    }

    public override bool CellHasValue(byte cellX, byte cellY) => Cells[cellX, cellY].LastPassTime != DateTime.MinValue;

    /// <summary>
    /// Assign filtered height value from a filtered pass to a cell
    /// </summary>
    public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext context)
    {
      void CalculateCmvChange(IProfileCell profileCell)
      {
        profileCell.CellCCV = CellPassConsts.NullCCV;
        profileCell.CellTargetCCV = CellPassConsts.NullCCV;
        profileCell.CellPreviousMeasuredCCV = CellPassConsts.NullCCV;
        profileCell.CellPreviousMeasuredTargetCCV = CellPassConsts.NullCCV;

        var dataStillRequiredForCcv = true;

        for (var i = profileCell.Layers.Count() - 1; i >= 0; i--)
        {
          if (profileCell.Layers[i].FilteredPassCount > 0)
          {
            if ((profileCell.Layers[i].Status & LayerStatus.Superseded) != 0 && !context.LiftParams.IncludeSuperseded)
              continue;

            if (dataStillRequiredForCcv && profileCell.CellCCV == CellPassConsts.NullCCV && profileCell.Layers[i].CCV != CellPassConsts.NullCCV)
            {
              profileCell.CellCCV = profileCell.Layers[i].CCV;
              profileCell.CellCCVElev = profileCell.Layers[i].CCV_Elev;

              var passSearchIdx = profileCell.Layers[i].CCV_CellPassIdx - 1;
              while (passSearchIdx >= 0)
              {
                if (context.LiftParams.CCVSummarizeTopLayerOnly && (passSearchIdx < profileCell.Layers[i].StartCellPassIdx || passSearchIdx > profileCell.Layers[i].EndCellPassIdx))
                  break;

                if (!profileCell.Layers.IsCellPassInSupersededLayer(passSearchIdx) || context.LiftParams.IncludeSuperseded)
                {
                  profileCell.CellPreviousMeasuredCCV = profileCell.Passes.FilteredPassData[passSearchIdx].FilteredPass.CCV;
                  if (context.Overrides?.OverrideMachineCCV ?? false)
                    profileCell.CellPreviousMeasuredTargetCCV = context.Overrides.OverridingMachineCCV;
                  else
                    profileCell.CellPreviousMeasuredTargetCCV = profileCell.Passes.FilteredPassData[passSearchIdx].TargetValues.TargetCCV;
                  break;
                }

                passSearchIdx--;
              }

              dataStillRequiredForCcv = false;
            }


            if (!dataStillRequiredForCcv)
              break;

            if (context.LiftParams.CCVSummarizeTopLayerOnly)
              dataStillRequiredForCcv = false;
          }
        }
      }

      if (context.CellProfile == null)
      {
        _log.LogError($"{nameof(AssignFilteredValue)}: Error=CellProfile not assigned.");
        return;
      }

      if (!(context.CellProfile is IProfileCell cellProfileFromContext))
      {
        _log.LogError($"{nameof(AssignFilteredValue)}: Error=CellProfile does not implement {nameof(IProfileCell)}.");
        return;
      }

      FilteredPassData lastPass;
      if (context.LowestPassIdx != Consts.NullLowestPassIdx)
        lastPass = cellProfileFromContext.Passes.FilteredPassData[context.LowestPassIdx]; // take the pass from lowest pass due to mapping mode 
      else
        lastPass = cellProfileFromContext.Passes.FilteredPassData[cellProfileFromContext.Passes.PassCount - 1];

      Cells[cellX, cellY].CellXOffset = context.ProbePositions[cellX, cellY].XOffset;
      Cells[cellX, cellY].CellYOffset = context.ProbePositions[cellX, cellY].YOffset;

      Cells[cellX, cellY].LastPassTime = cellProfileFromContext.Passes.LastPassTime();
      Cells[cellX, cellY].PassCount = context.FilteredValue.PassCount;
      Cells[cellX, cellY].LastPassValidRadioLatency = cellProfileFromContext.Passes.LastPassValidRadioLatency();
      Cells[cellX, cellY].EventDesignNameID = lastPass.EventValues.EventDesignNameID;
      Cells[cellX, cellY].InternalSiteModelMachineIndex = lastPass.FilteredPass.InternalSiteModelMachineIndex;
      Cells[cellX, cellY].MachineSpeed = lastPass.FilteredPass.MachineSpeed;
      Cells[cellX, cellY].LastPassValidGPSMode = cellProfileFromContext.Passes.LastPassValidGPSMode();
      Cells[cellX, cellY].GPSTolerance = lastPass.EventValues.GPSTolerance;
      Cells[cellX, cellY].GPSAccuracy = lastPass.EventValues.GPSAccuracy;
      Cells[cellX, cellY].TargetPassCount = lastPass.TargetValues.TargetPassCount;
      Cells[cellX, cellY].TotalWholePasses = cellProfileFromContext.TotalNumberOfWholePasses(true); // include superseded layers
      Cells[cellX, cellY].LayersCount = cellProfileFromContext.Layers.Count();

      cellProfileFromContext.Passes.LastPassValidCCVDetails(out var lastPassValidCcv, out var targetCcv); // get details from last VALID pass
      Cells[cellX, cellY].LastPassValidCCV = lastPassValidCcv;
      Cells[cellX, cellY].TargetCCV = targetCcv;

      cellProfileFromContext.Passes.LastPassValidMDPDetails(out var lastPassValidMdp, out var targetMdp); // get details from last VALID pass
      Cells[cellX, cellY].LastPassValidMDP = lastPassValidMdp;
      Cells[cellX, cellY].TargetMDP = targetMdp;

      cellProfileFromContext.Passes.LastPassValidCCADetails(out var lastPassValidCca, out var targetCca); // get details from last VALID pass
      Cells[cellX, cellY].LastPassValidCCA = lastPassValidCca;
      Cells[cellX, cellY].TargetCCA = targetCca;

      Cells[cellX, cellY].LastPassValidRMV = cellProfileFromContext.Passes.LastPassValidRMV();
      Cells[cellX, cellY].LastPassValidFreq = cellProfileFromContext.Passes.LastPassValidFreq();
      Cells[cellX, cellY].LastPassValidAmp = cellProfileFromContext.Passes.LastPassValidAmp();
      Cells[cellX, cellY].TargetThickness = lastPass.TargetValues.TargetLiftThickness;
      Cells[cellX, cellY].EventMachineGear = lastPass.EventValues.EventMachineGear;
      Cells[cellX, cellY].EventVibrationState = lastPass.EventValues.EventVibrationState;
      Cells[cellX, cellY].LastPassValidTemperature = lastPass.FilteredPass.MaterialTemperature; // Bug32323 show only last pass temp.  Passes.LastPassValidTemperature;
      Cells[cellX, cellY].TempWarningLevelMin = lastPass.TargetValues.TempWarningLevelMin;
      Cells[cellX, cellY].TempWarningLevelMax = lastPass.TargetValues.TempWarningLevelMax;
      Cells[cellX, cellY].Height = lastPass.FilteredPass.Height;

      CalculateCmvChange(cellProfileFromContext);
      Cells[cellX, cellY].CCVChange = 0;
      var v2 = cellProfileFromContext.CellCCV;
      var v1 = cellProfileFromContext.CellPreviousMeasuredCCV;

      if (v2 == CellPassConsts.NullCCV)
        Cells[cellX, cellY].CCVChange = CellPassConsts.NullCCV; // will force no result to show
      else if (v1 == CellPassConsts.NullCCV)
        Cells[cellX, cellY].CCVChange = 100; // %100 diff
      else
      {
        if (v1 == 0) // avoid div by 0 error
          Cells[cellX, cellY].CCVChange = 100; // %100 diff
        else
          Cells[cellX, cellY].CCVChange = (v2 - v1) / (float)v1 * 100;
      }
    }

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// </summary>
    /// <param name="writer"></param>
    public override void Write(BinaryWriter writer)
    {
      base.Write(writer);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Write(writer));
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// </summary>
    /// <param name="reader"></param>
    public override void Read(BinaryReader reader)
    {
      base.Read(reader);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Read(reader));
    }

    public override bool UpdateProcessingMapForSurveyedSurfaces(SubGridTreeBitmapSubGridBits processingMap, IList filteredSurveyedSurfaces, bool returnEarliestFilteredCellPass)
    {
      if (!(filteredSurveyedSurfaces is ISurveyedSurfaces surveyedSurfaces))
      {
        return false;
      }

      processingMap.Assign(FilterMap);

      // If we're interested in a particular cell, but we don't have any
      // surveyed surfaces later (or earlier) than the cell production data
      // pass time (depending on PassFilter.ReturnEarliestFilteredCellPass)
      // then there's no point in asking the Design Profiler service for an elevation
      processingMap.ForEachSetBit((x, y) =>
        {
          // ReSharper disable once CompareOfFloatsByEqualityOperator
          if (Cells[x, y].Height == Consts.NullHeight)
            return;

          if (returnEarliestFilteredCellPass)
          {
            if (!surveyedSurfaces.HasSurfaceEarlierThan(Cells[x, y].LastPassTime))
              processingMap.ClearBit(x, y);
          }
          else
          {
            if (!surveyedSurfaces.HasSurfaceLaterThan(Cells[x, y].LastPassTime))
              processingMap.ClearBit(x, y);
          }
        });

      return true;
    }

    public override bool PerformHeightAnnotation(SubGridTreeBitmapSubGridBits processingMap, IList filteredSurveyedSurfaces, bool returnEarliestFilteredCellPass,
                                                 IClientLeafSubGrid surfaceElevationsSource, Func<int, int, float, bool> elevationRangeFilterLambda)
    {
      if (!(surfaceElevationsSource is ClientHeightAndTimeLeafSubGrid surfaceElevations))
      {
        _log.LogError($"{nameof(ClientCellProfileLeafSubgrid)}.{nameof(PerformHeightAnnotation)} not supplied a ClientHeightAndTimeLeafSubGrid instance, but an instance of {surfaceElevationsSource?.GetType().FullName}");

        return false;
      }

      // For all cells we wanted to request a surveyed surface elevation for,
      // update the cell elevation if a non null surveyed surface of appropriate time was computed
      // Note: The surveyed surface will return all cells in the requested sub grid, not just the ones indicated in the processing map
      // IE: It is unsafe to test for null top indicate not-filtered, use the processing map iterators to cover only those cells required
      processingMap.ForEachSetBit((x, y) =>
      {
        var surveyedSurfaceCellHeight = surfaceElevations.Cells[x, y];

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (surveyedSurfaceCellHeight == Consts.NullHeight)
        {
          return;
        }

        // If we got back a surveyed surface elevation...
        var surveyedSurfaceCellTime = surfaceElevations.Times[x, y];
        var prodHeight = Cells[x, y].Height;
        var prodTime = Cells[x, y].LastPassTime.Ticks;

        // Determine if the elevation from the surveyed surface data is required based on the production data elevation being null, and
        // the relative age of the measured surveyed surface elevation compared with a non-null production data height
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (!(prodHeight == Consts.NullHeight || (returnEarliestFilteredCellPass ? surveyedSurfaceCellTime < prodTime : surveyedSurfaceCellTime > prodTime)))
        {
          // We didn't get a surveyed surface elevation, so clear the bit in the processing map to indicate there is no surveyed surface information present for it
          processingMap.ClearBit(x, y);
          return;
        }

        // Check if there is an elevation range filter in effect and whether the surveyed surface elevation data matches it
        if (elevationRangeFilterLambda != null)
        {
          if (!(elevationRangeFilterLambda(x, y, surveyedSurfaceCellHeight)))
          {
            // We didn't get a surveyed surface elevation, so clear the bit in the processing map to indicate there is no surveyed surface information present for it
            processingMap.ClearBit(x, y);
            return;
          }
        }

        Cells[x, y].Height = surveyedSurfaceCellHeight;
      });

      return true;
    }
  }
}
