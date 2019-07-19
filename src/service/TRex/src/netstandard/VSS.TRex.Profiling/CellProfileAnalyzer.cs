using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Responsible for orchestrating analysis of identified cells along the path of a profile line
  /// and deriving the profile related analytics for each cell
  /// </summary>
  public class CellProfileAnalyzer : CellProfileAnalyzerBase<ProfileCell>
  {
    private static ILogger Log = Logging.Logger.CreateLogger<CellProfileAnalyzer>();

    /// <summary>
    /// The number of passes identified in the top-most (most recent) layer
    /// </summary>
    public int TopMostLayerPassCount;

    /// <summary>
    /// The number of half-passes (recorded by machine that report passes as such)
    /// identified in the top-most (most recent) layer
    /// </summary>
    public int TopMostLayerCompactionHalfPassCount;

    private readonly ICellPassAttributeFilter PassFilter;
    private readonly ICellPassAttributeFilterProcessingAnnex PassFilterAnnex;

    private readonly ICellSpatialFilter CellFilter;

    /// <summary>
    /// Cell lift builder reference to the engine that performs detailed analytics on individual cells in the profile.
    /// </summary>
    private readonly ICellLiftBuilder CellLiftBuilder;

    private ProfileCell ProfileCell;
    private IOverrideParameters Overrides;

    private CellProfileAnalyzer()
    {}

    /// <summary>
    /// Constructs a cell profile analyzer that analyzes cells in a cell profile vector
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="pDExistenceMap"></param>
    /// <param name="filterSet"></param>
    /// <param name="cellPassFilter_ElevationRangeDesignWrapper"></param>
    /// <param name="cellLiftBuilder"></param>
    /// <param name="overrides"></param>
    public CellProfileAnalyzer(ISiteModel siteModel,
      ISubGridTreeBitMask pDExistenceMap,
      IFilterSet filterSet,
      IDesignWrapper cellPassFilter_ElevationRangeDesignWrapper,
      ICellLiftBuilder cellLiftBuilder,
      IOverrideParameters overrides) 
      : base(siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesignWrapper)
    {
      CellLiftBuilder = cellLiftBuilder;

      PassFilter = filterSet.Filters[0].AttributeFilter;
      PassFilterAnnex = new CellPassAttributeFilterProcessingAnnex();
      CellFilter = filterSet.Filters[0].SpatialFilter;
      Overrides = overrides;
    }

    /// <summary>
    /// Gets the material temperature warning limits for a machine at a given time
    /// </summary>
    /// <param name="machineID"></param>
    /// <param name="time"></param>
    /// <param name="minWarning"></param>
    /// <param name="maxWarning"></param>
    private void GetMaterialTemperatureWarningLevelsTarget(short machineID,
      DateTime time,
      out ushort minWarning,
      out ushort maxWarning)
    {
      minWarning = SiteModel.MachinesTargetValues[machineID].TargetMinMaterialTemperature.GetValueAtDate(time, out int _, CellPassConsts.NullMaterialTemperatureValue);
      maxWarning = SiteModel.MachinesTargetValues[machineID].TargetMaxMaterialTemperature.GetValueAtDate(time, out int _, CellPassConsts.NullMaterialTemperatureValue);
    }

    /// <summary>
    /// Gets the target CCV for a machine at a given time
    /// </summary>
    /// <param name="machineID"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private short GetTargetCCV(short machineID, DateTime time) =>
      SiteModel.MachinesTargetValues[machineID].TargetCCVStateEvents.GetValueAtDate(time, out int _, CellPassConsts.NullCCV);

    /// <summary>
    /// Gets the target MDP for a machine at a given time
    /// </summary>
    /// <param name="machineID"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private short GetTargetMDP(short machineID, DateTime time) =>
      SiteModel.MachinesTargetValues[machineID].TargetMDPStateEvents.GetValueAtDate(time, out int _, CellPassConsts.NullMDP);

    /// <summary>
    /// Gets the target CCA for a machine at a given time
    /// </summary>
    /// <param name="machineID"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private short GetTargetCCA(short machineID, DateTime time) =>
      SiteModel.MachinesTargetValues[machineID].TargetCCAStateEvents.GetValueAtDate(time, out int _, CellPassConsts.NullCCA);

    /// <summary>
    /// Gets the target pass count for a machine at a given time
    /// </summary>
    /// <param name="machineID"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private ushort GetTargetPassCount(short machineID, DateTime time) =>
      SiteModel.MachinesTargetValues[machineID].TargetPassCountStateEvents.GetValueAtDate(time, out int _, CellPassConsts.NullPassCountValue);

    /// <summary>
    /// Determines a set of summary attributes for the cell being analyzed
    /// </summary>
    private void CalculateSummaryCellAttributeData()
    {
      PassCountRangeRecord PassCountTargetRange = new PassCountRangeRecord();

      ProfileCell.CellCCV = CellPassConsts.NullCCV;
      ProfileCell.CellTargetCCV = CellPassConsts.NullCCV;

      ProfileCell.CellMDP = CellPassConsts.NullMDP;
      ProfileCell.CellTargetMDP = CellPassConsts.NullMDP;

      ProfileCell.CellCCA = CellPassConsts.NullCCA;
      ProfileCell.CellTargetCCA = CellPassConsts.NullCCA;

      ProfileCell.CellMaterialTemperature = CellPassConsts.NullMaterialTemperatureValue;
      ProfileCell.CellMaterialTemperatureWarnMin = CellPassConsts.NullMaterialTemperatureValue;
      ProfileCell.CellMaterialTemperatureWarnMax = CellPassConsts.NullMaterialTemperatureValue;

      ProfileCell.CellPreviousMeasuredCCV = CellPassConsts.NullCCV;
      ProfileCell.CellPreviousMeasuredTargetCCV = CellPassConsts.NullCCV;

      ProfileCell.CellTopLayerThickness = Consts.NullHeight;

      ProfileCell.TopLayerPassCount = 0;
      PassCountTargetRange.SetMinMax(0, 0);

      ProfileCell.CellMaxSpeed = 0;
      ProfileCell.CellMinSpeed = CellPassConsts.NullMachineSpeed;

      ProfileCell.TopLayerPassCountTargetRangeMin = ProfileCell.TopLayerPassCount;
      ProfileCell.TopLayerPassCountTargetRangeMax = ProfileCell.TopLayerPassCount;

      // Work Out Speed Min Max
      // ReSharper disable once UseMethodAny.0
      if (ProfileCell.Layers.Count() > 0)
      {
        for (int I = ProfileCell.Layers.Count() - 1; I >= 0; I--)
        {
          if (ProfileCell.Layers[I].FilteredPassCount > 0)
          {
            if ((LayerStatus.Superseded & ProfileCell.Layers[I].Status) != 0)
              continue;

            for (int PassIndex = ProfileCell.Layers[I].StartCellPassIdx;
              PassIndex <= ProfileCell.Layers[I].EndCellPassIdx;
              PassIndex++)
            {
              if (ProfileCell.Passes.FilteredPassData[PassIndex].FilteredPass.MachineSpeed < ProfileCell.CellMinSpeed)
                ProfileCell.CellMinSpeed = ProfileCell.Passes.FilteredPassData[PassIndex].FilteredPass.MachineSpeed;
              if (ProfileCell.Passes.FilteredPassData[PassIndex].FilteredPass.MachineSpeed > ProfileCell.CellMaxSpeed)
                ProfileCell.CellMaxSpeed = ProfileCell.Passes.FilteredPassData[PassIndex].FilteredPass.MachineSpeed;
            }
          }
        }
      }

      // ReSharper disable once UseMethodAny.0
      if (ProfileCell.Layers.Count() > 0)
        for (int I = ProfileCell.Layers.Count() - 1; I >= 0; I--)
          if (ProfileCell.FilteredPassCount > 0)
          {
            if ((LayerStatus.Superseded & ProfileCell.Layers[I].Status) != 0)
              continue;

            ProfileCell.TopLayerPassCount = (ushort) (ProfileCell.FilteredHalfPassCount / 2);

            if (Overrides.OverrideTargetPassCount)
            {
              ProfileCell.TopLayerPassCountTargetRangeMin = Overrides.OverridingTargetPassCountRange.Min;
              ProfileCell.TopLayerPassCountTargetRangeMax = Overrides.OverridingTargetPassCountRange.Max;
            }
            else if (ProfileCell.Layers[I].TargetPassCount == 0)
            {
              ushort TempPassCountTarget =
                GetTargetPassCount(
                  ProfileCell.Passes.FilteredPassData[ProfileCell.Layers[I].EndCellPassIdx].FilteredPass.InternalSiteModelMachineIndex,
                  ProfileCell.Passes.FilteredPassData[ProfileCell.Layers[I].EndCellPassIdx].FilteredPass.Time);
              PassCountTargetRange.SetMinMax(TempPassCountTarget, TempPassCountTarget);
              ProfileCell.TopLayerPassCountTargetRangeMin = PassCountTargetRange.Min;
              ProfileCell.TopLayerPassCountTargetRangeMax = PassCountTargetRange.Max;
            }
            else
            {
              PassCountTargetRange.SetMinMax(ProfileCell.Layers[I].TargetPassCount, ProfileCell.Layers[I].TargetPassCount);
              ProfileCell.TopLayerPassCountTargetRangeMin = PassCountTargetRange.Min;
              ProfileCell.TopLayerPassCountTargetRangeMax = PassCountTargetRange.Max;
            }

            break; // we have top layer
          }

      bool DataStillRequiredForCCV = (ProfileCell.AttributeExistenceFlags & ProfileCellAttributeExistenceFlags.HasCCAData) != 0;
      bool DataStillRequiredForMDP = (ProfileCell.AttributeExistenceFlags & ProfileCellAttributeExistenceFlags.HasMDPData) != 0;
      bool DataStillRequiredForCCA = (ProfileCell.AttributeExistenceFlags & ProfileCellAttributeExistenceFlags.HasCCAData) != 0;
      bool DataStillRequiredForTMP = (ProfileCell.AttributeExistenceFlags & ProfileCellAttributeExistenceFlags.HasTemperatureData) != 0;

      for (int I = ProfileCell.Layers.Count() - 1; I >= 0; I--)
        if (ProfileCell.FilteredPassCount > 0)
        {
          if ((LayerStatus.Superseded & ProfileCell.Layers[I].Status) != 0 &&
              !Dummy_LiftBuildSettings.IncludeSuperseded)
            continue;

          if (DataStillRequiredForCCV && ProfileCell.CellCCV == CellPassConsts.NullCCV &&
              ProfileCell.Layers[I].CCV != CellPassConsts.NullCCV)
          {
            ProfileCell.CellCCV = ProfileCell.Layers[I].CCV;
            ProfileCell.CellCCVElev = ProfileCell.Layers[I].CCV_Elev;

            int PassSearchIdx = ProfileCell.Layers[I].CCV_CellPassIdx - 1;
            while (PassSearchIdx >= 0)
            {
              if (Dummy_LiftBuildSettings.CCVSummarizeTopLayerOnly &&
                  PassSearchIdx < ProfileCell.Layers[I].StartCellPassIdx ||
                  PassSearchIdx > ProfileCell.Layers[I].EndCellPassIdx)
                break;

              if (!ProfileCell.Layers.IsCellPassInSupersededLayer(PassSearchIdx) ||
                  Dummy_LiftBuildSettings.IncludeSuperseded)
              {
                ProfileCell.CellPreviousMeasuredCCV = ProfileCell.Passes.FilteredPassData[PassSearchIdx].FilteredPass.CCV;
                ProfileCell.CellPreviousMeasuredTargetCCV = Overrides.OverrideMachineCCV 
                  ? Overrides.OverridingMachineCCV 
                  : ProfileCell.Passes.FilteredPassData[PassSearchIdx].TargetValues.TargetCCV;
                break;
              }

              PassSearchIdx--;
            }

            if (Overrides.OverrideMachineCCV)
              ProfileCell.CellTargetCCV = Overrides.OverridingMachineCCV;
            else if (ProfileCell.Layers[I].TargetCCV == CellPassConsts.NullCCV)
              ProfileCell.CellTargetCCV = GetTargetCCV(ProfileCell.Layers[I].CCV_MachineID, ProfileCell.Layers[I].CCV_Time);
            else
              ProfileCell.CellTargetCCV = ProfileCell.Layers[I].TargetCCV;

            DataStillRequiredForCCV = false;
          }

          if (DataStillRequiredForMDP && ProfileCell.CellMDP == CellPassConsts.NullMDP &&
              ProfileCell.Layers[I].MDP != CellPassConsts.NullMDP)
          {
            ProfileCell.CellMDP = ProfileCell.Layers[I].MDP;
            ProfileCell.CellMDPElev = ProfileCell.Layers[I].MDP_Elev;
            if (Overrides.OverrideMachineMDP)
              ProfileCell.CellTargetMDP = Overrides.OverridingMachineMDP;
            else if (ProfileCell.Layers[I].TargetMDP == CellPassConsts.NullMDP)
              ProfileCell.CellTargetMDP = GetTargetMDP(ProfileCell.Layers[I].MDP_MachineID, ProfileCell.Layers[I].MDP_Time);
            else
              ProfileCell.CellTargetMDP = ProfileCell.Layers[I].TargetMDP;

            DataStillRequiredForMDP = false;
          }

          if (DataStillRequiredForCCA && ProfileCell.CellCCA == CellPassConsts.NullCCA &&
              ProfileCell.Layers[I].CCA != CellPassConsts.NullCCA)
          {
            ProfileCell.CellCCA = ProfileCell.Layers[I].CCA;
            ProfileCell.CellCCAElev = ProfileCell.Layers[I].CCA_Elev;
            ProfileCell.CellTargetCCA = ProfileCell.Layers[I].TargetCCA == CellPassConsts.NullCCA 
              ? GetTargetCCA(ProfileCell.Layers[I].CCA_MachineID, ProfileCell.Layers[I].CCA_Time) 
              : ProfileCell.Layers[I].TargetCCA;

            DataStillRequiredForCCA = false;
          }

          if (DataStillRequiredForTMP &&
              ProfileCell.CellMaterialTemperature == CellPassConsts.NullMaterialTemperatureValue &&
              ProfileCell.Layers[I].MaterialTemperature != CellPassConsts.NullMaterialTemperatureValue)
          {
            ProfileCell.CellMaterialTemperature = ProfileCell.Layers[I].MaterialTemperature;
            ProfileCell.CellMaterialTemperatureElev = ProfileCell.Layers[I].MaterialTemperature_Elev;

            if (Overrides.OverrideTemperatureWarningLevels)
            {
              ProfileCell.CellMaterialTemperatureWarnMin =
                Overrides.OverridingTemperatureWarningLevels.Min;
              ProfileCell.CellMaterialTemperatureWarnMax =
                Overrides.OverridingTemperatureWarningLevels.Max;
            }
            else if (ProfileCell.CellMaterialTemperatureWarnMin == CellPassConsts.NullMaterialTemperatureValue &&
                     ProfileCell.CellMaterialTemperatureWarnMax == CellPassConsts.NullMaterialTemperatureValue)
              GetMaterialTemperatureWarningLevelsTarget(ProfileCell.Layers[I].MaterialTemperature_MachineID,
                ProfileCell.Layers[I].MaterialTemperature_Time,
                out ProfileCell.CellMaterialTemperatureWarnMin, out ProfileCell.CellMaterialTemperatureWarnMax);
            else
            {
              // Currently no tracking of temperature min/max warnings on a per layer basis.
            }

            DataStillRequiredForTMP = false;
          }

          if (!DataStillRequiredForCCV && !DataStillRequiredForMDP && !DataStillRequiredForCCA &&
              !DataStillRequiredForTMP)
            break;

// CCA not part of legacy setup as yet
          if (Dummy_LiftBuildSettings.CCVSummarizeTopLayerOnly)
            DataStillRequiredForCCV = false;
          if (Dummy_LiftBuildSettings.MDPSummarizeTopLayerOnly)
            DataStillRequiredForMDP = false;

          DataStillRequiredForTMP = false; // last pass only
        }

      for (int I = ProfileCell.Layers.Count() - 1; I >= 0; I--)
        if (ProfileCell.FilteredPassCount > 0)
        {
          if ((LayerStatus.Superseded & ProfileCell.Layers[I].Status) != 0)
            continue;

          if (ProfileCell.Layers[I].Thickness != Consts.NullSingle)
          {
            ProfileCell.CellTopLayerThickness = ProfileCell.Layers[I].Thickness;
            break;
          }
        }

      ProfileCell.SetFirstLastHighestLowestElevations(PassFilter.HasElevationTypeFilter, PassFilter.ElevationType);

// are coords set right?
      int CellX = ProfileCell.OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask;
      int CellY = ProfileCell.OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask;
      bool HaveCompositeSurfaceForCell = CompositeHeightsGrid?.ProdDataMap.BitSet(CellX, CellY) ?? false;

      if (HaveCompositeSurfaceForCell)
      {
        if ((CompositeHeightsGrid.Cells[CellX, CellY].LastHeightTime == 0) ||
            ((ProfileCell.Passes.PassCount > 0) &&
             (ProfileCell.Passes.LastPassTime() >
              DateTime.FromBinary(CompositeHeightsGrid.Cells[CellX, CellY].LastHeightTime))))
          ProfileCell.CellLastCompositeElev = ProfileCell.CellLastElev;
        else
          ProfileCell.CellLastCompositeElev = CompositeHeightsGrid.Cells[CellX, CellY].LastHeight;

        if ((CompositeHeightsGrid.Cells[CellX, CellY].LowestHeightTime == 0) ||
            ((ProfileCell.Passes.PassCount > 0) &&
             (ProfileCell.Passes.LowestPassTime() >
              DateTime.FromBinary(CompositeHeightsGrid.Cells[CellX, CellY].LowestHeightTime))))
          ProfileCell.CellLowestCompositeElev = ProfileCell.CellLowestElev;
        else
          ProfileCell.CellLowestCompositeElev =
            CompositeHeightsGrid.Cells[CellX, CellY].LowestHeight;

        if ((CompositeHeightsGrid.Cells[CellX, CellY].HighestHeightTime == 0) ||
            ((ProfileCell.Passes.PassCount > 0) &&
             (ProfileCell.Passes.HighestPassTime() >
              DateTime.FromBinary(CompositeHeightsGrid.Cells[CellX, CellY].HighestHeightTime))))
          ProfileCell.CellHighestCompositeElev = ProfileCell.CellHighestElev;
        else
          ProfileCell.CellHighestCompositeElev =
            CompositeHeightsGrid.Cells[CellX, CellY].HighestHeight;

        if ((CompositeHeightsGrid.Cells[CellX, CellY].FirstHeightTime == 0) ||
            ((ProfileCell.Passes.PassCount > 0) &&
             (ProfileCell.Passes.FirstPassTime >
              DateTime.FromBinary(CompositeHeightsGrid.Cells[CellX, CellY].FirstHeightTime))))
          ProfileCell.CellFirstCompositeElev = ProfileCell.CellFirstElev;
        else
          ProfileCell.CellFirstCompositeElev =
            CompositeHeightsGrid.Cells[CellX, CellY].FirstHeight;
      }
      else
      {
        ProfileCell.CellLastCompositeElev = ProfileCell.CellLastElev;
        ProfileCell.CellLowestCompositeElev = ProfileCell.CellLowestElev;
        ProfileCell.CellHighestCompositeElev = ProfileCell.CellHighestElev;
        ProfileCell.CellFirstCompositeElev = ProfileCell.CellFirstElev;
      }
    }

    /// <summary>
    /// Builds a fully analyzed vector of profiled cells from the list of cell passed to it
    /// </summary>
    /// <param name="profileCells"></param>
    /// <param name="cellPassIterator"></param>
    /// <returns></returns>
    public override async Task<bool> Analyze(List<ProfileCell> profileCells, ISubGridSegmentCellPassIterator cellPassIterator)
    {
      //{$IFDEF DEBUG}
      //SIGLogMessage.PublishNoODS(Self, Format('BuildLiftProfileFromInitialLayer: Processing %d cells', [FProfileCells.Count]), ...);
      //{$ENDIF}

      var currentSubGridOrigin = new SubGridCellAddress(int.MaxValue, int.MaxValue);
      ISubGrid subGrid = null;
      IServerLeafSubGrid _subGridAsLeaf = null;
      ProfileCell = null;
//      FilterDesignElevations = null;
      var ignoreSubGrid = false;

      for (var i = 0; i < profileCells.Count; i++)
      {
        ProfileCell = profileCells[i];

        // get sub grid setup iterator and set cell address
        // get sub grid origin for cell address
        var thisSubGridOrigin = new SubGridCellAddress(ProfileCell.OTGCellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
          ProfileCell.OTGCellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel);

        if (!currentSubGridOrigin.Equals(thisSubGridOrigin)) // if we have a new sub grid to fetch
        {
          ignoreSubGrid = false;
          currentSubGridOrigin = thisSubGridOrigin;
          subGrid = null;

          // Does the sub grid tree contain this node in it's existence map?
          if (PDExistenceMap[currentSubGridOrigin.X, currentSubGridOrigin.Y])
            subGrid = SubGridTrees.Server.Utilities.SubGridUtilities.LocateSubGridContaining
              (SiteModel.PrimaryStorageProxy, SiteModel.Grid, ProfileCell.OTGCellX, ProfileCell.OTGCellY, SiteModel.Grid.NumLevels, false, false);

          _subGridAsLeaf = subGrid as ServerSubGridTreeLeaf;
          if (_subGridAsLeaf == null)
            continue;

          cellPassIterator.SegmentIterator.SubGrid = _subGridAsLeaf;
          cellPassIterator.SegmentIterator.Directory = _subGridAsLeaf.Directory;

          if (CompositeHeightsGrid != null)
          {
            ClientLeafSubGridFactory.ReturnClientSubGrid(ref CompositeHeightsGridIntf);
            CompositeHeightsGrid = null;
          }

          if (!LiftFilterMask<ProfileCell>.ConstructSubGridCellFilterMask(SiteModel.Grid, currentSubGridOrigin,
            profileCells, FilterMask, i, CellFilter, SurfaceDesignMaskDesign))
            continue;

          if (FilteredSurveyedSurfaces != null)
          {
            // Hand client grid details, a mask of cells we need surveyed surface elevations for, and a temp grid to the Design Profiler
            SurfaceElevationPatchArg.SetOTGBottomLeftLocation(_subGridAsLeaf.OriginX, _subGridAsLeaf.OriginY);
            SurfaceElevationPatchArg.ProcessingMap.Assign(FilterMask);

            CompositeHeightsGridIntf = SurfaceElevationPatchRequest.Execute(SurfaceElevationPatchArg);
            CompositeHeightsGrid = CompositeHeightsGridIntf as ClientCompositeHeightsLeafSubgrid;

            if (CompositeHeightsGrid == null)
            {
              Log.LogError("Call(B) to SurfaceElevationPatchRequest failed to return a composite profile grid.");
              continue;
            }
          }

          var initialiseFilterContextResult = await LiftFilterMask<ProfileCell>.InitialiseFilterContext(SiteModel, PassFilter, PassFilterAnnex, ProfileCell, CellPassFilter_ElevationRangeDesignWrapper?.Design);
          if (!initialiseFilterContextResult.executionResult)
          {
            if (initialiseFilterContextResult.filterDesignErrorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
              ignoreSubGrid = true;
            else
              Log.LogError("Call to RequestDesignElevationPatch in TICServerProfiler for filter failed to return an elevation patch.");
            continue;
          }
        }

        if (subGrid != null && !ignoreSubGrid)
        {
          if (_subGridAsLeaf != null)
          {
            if (_subGridAsLeaf.Directory.GlobalLatestCells.HasCCVData())
              ProfileCell.AttributeExistenceFlags |= ProfileCellAttributeExistenceFlags.HasCCVData;

            if (_subGridAsLeaf.Directory.GlobalLatestCells.HasMDPData())
              ProfileCell.AttributeExistenceFlags |= ProfileCellAttributeExistenceFlags.HasMDPData;

            if (_subGridAsLeaf.Directory.GlobalLatestCells.HasCCAData())
              ProfileCell.AttributeExistenceFlags |= ProfileCellAttributeExistenceFlags.HasCCAData;

            if (_subGridAsLeaf.Directory.GlobalLatestCells.HasTemperatureData())
              ProfileCell.AttributeExistenceFlags |= ProfileCellAttributeExistenceFlags.HasTemperatureData;
          }

          // get cell address relative to sub grid and SetCellCoordinatesInSubGrid
          cellPassIterator.SetCellCoordinatesInSubgrid(
            (byte) (profileCells[i].OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask),
            (byte) (profileCells[i].OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask));
          PassFilterAnnex.InitializeFilteringForCell(PassFilter, cellPassIterator.CellX, cellPassIterator.CellY);

          if (CellLiftBuilder.Build(ProfileCell, /*todo Dummy_LiftBuildSettings, */ null, null, cellPassIterator, false))
          {
            TopMostLayerPassCount = CellLiftBuilder.FilteredPassCountOfTopMostLayer;
            TopMostLayerCompactionHalfPassCount = CellLiftBuilder.FilteredHalfCellPassCountOfTopMostLayer;
            ProfileCell.IncludesProductionData = true;
          }
          else
            ProfileCell.ClearLayers();
        }
        else
          ProfileCell.ClearLayers();

        CalculateSummaryCellAttributeData();
      }

      return true;
    }
  }
}
