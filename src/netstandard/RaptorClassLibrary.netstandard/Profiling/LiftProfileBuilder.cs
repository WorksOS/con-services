using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.DesignProfiling;
using VSS.TRex.Designs.Storage;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.Surfaces;
using VSS.TRex.Surfaces.GridFabric.Arguments;
using VSS.TRex.Surfaces.GridFabric.Requests;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling
{
  public class LiftProfileBuilder
  {
    private static ILogger Log = Logging.Logger.CreateLogger<LiftProfileBuilder>();

    private int TopMostLayerPassCount;
    private int TopMostLayerCompactionHalfPassCount;
    private ProfileCell ProfileCell;
    private SubGridCellAddress CurrentSubgridOrigin;
    private SubGridCellAddress ThisSubgridOrigin;
    private ISubGrid SubGrid;

    private IServerLeafSubGrid _SubGridAsLeaf;

    //FLockToken              : Integer;
    //FLockTokenName          : String;
    private ClientCompositeHeightsLeafSubgrid CompositeHeightsGrid;

    private SubGridTreeBitmapSubGridBits FilterMask =
      new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    private ClientHeightLeafSubGrid FilterDesignElevations;
    private DesignProfilerRequestResult FilterDesignErrorCode;
    private bool IgnoreSubgrid;

    private ISiteModel SiteModel;
    private FilteredMultiplePassInfo Passes;
    private CellPassAttributeFilter PassFilter;

    private bool PopulationControl_AnySet;
    private FilteredValuePopulationControl PopulationControl;
    private CellSpatialFilter CellFilter;
    private List<ProfileCell> ProfileCells;
    private SubGridTreeBitMask PDExistenceMap;

    private ISubGridSegmentCellPassIterator CellPassIterator;
    private SurveyedSurfaces FilteredSurveyedSurfaces;

    /* TODO: Profile patch requests not implemented yet...
private SurfaceElevationPatchArgument SurfaceElevationPatchArg;
private SurfaceElevationPatchRequest SurfaceElevationPatchRequest;
*/

    /// <summary>
    /// The design supplied as a result of an independent lookup outside the scope of this builder
    /// to find the design identified by the cellPassFilter.ElevationRangeDesignID
    /// </summary>
    private Design CellPassFilter_ElevationRangeDesign;

    public LiftProfileBuilder()
    {
    }

    public LiftProfileBuilder(ISiteModel siteModel,
      SubGridTreeBitMask pDExistenceMap,
      FilteredMultiplePassInfo passes,
      CellPassAttributeFilter passFilter,
      CellSpatialFilter cellFilter,
      FilteredValuePopulationControl populationControl,
      List<ProfileCell> profileCells,
      Design cellPassFilter_ElevationRangeDesign,
      ISubGridSegmentCellPassIterator cellPassIterator)
    {
      SiteModel = siteModel;
      PDExistenceMap = pDExistenceMap;
      Passes = passes;
      PassFilter = passFilter;
      CellFilter = cellFilter;
      PopulationControl = populationControl;
      PopulationControl_AnySet = PopulationControl.AnySet();
      ProfileCells = profileCells;
      CellPassFilter_ElevationRangeDesign = cellPassFilter_ElevationRangeDesign;
      CellPassIterator = cellPassIterator;

      if (SiteModel.SurveyedSurfaces?.Count > 0)
      {
// Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here

        FilteredSurveyedSurfaces = new SurveyedSurfaces();

        SiteModel.SurveyedSurfaces?.FilterSurveyedSurfaceDetails(PassFilter.HasTimeFilter, PassFilter.StartTime,
          PassFilter.EndTime, PassFilter.ExcludeSurveyedSurfaces(), FilteredSurveyedSurfaces,
          PassFilter.SurveyedSurfaceExclusionList);

        if (FilteredSurveyedSurfaces?.Count == 0)
          FilteredSurveyedSurfaces = null;
      }

      /* TODO: Profile patch requests not implemented yet...
    SurfaceElevationPatchRequest = new SurfaceElevationPatchRequest();
    
    // Instantiate a single instance of the argument object for the surface elevation patch requests and populate it with 
    // the common elements for this set of subgrids being requested. We always want to request all surface elevations to 
    // promote cacheability.
    SurfaceElevationPatchArg = new SurfaceElevationPatchArgument()
    {
    SiteModelID = SiteModel.ID,
    IncludedSurveyedSurfaces = FilteredSurveyedSurfaces,
    EarliestSurface = PassFilter.ReturnEarliestFilteredCellPass,
    ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled)
    };
    */
    }

    private ProductionEventLists GetTargetValues(short forMachineID) =>
      SiteModel.Machines[forMachineID].TargetValueChanges;

    private void GetMaterialTemperatureWarningLevelsTarget(short machineID,
      DateTime time,
      out ushort minWarning,
      out ushort maxWarning)
    {
      minWarning = GetTargetValues(machineID).TargetMinMaterialTemperature.GetValueAtDate(time, out int _);
      maxWarning = GetTargetValues(machineID).TargetMinMaterialTemperature.GetValueAtDate(time, out int _);
    }

    private short GetTargetCCV(short machineID, DateTime time) =>
      GetTargetValues(machineID).TargetCCVStateEvents.GetValueAtDate(time, out int _);

    private short GetTargetMDP(short machineID, DateTime time) =>
      GetTargetValues(machineID).TargetMDPStateEvents.GetValueAtDate(time, out int _);

    private short GetTargetCCA(short machineID, DateTime time) =>
      GetTargetValues(machineID).TargetCCAStateEvents.GetValueAtDate(time, out int _);

    private ushort GetTargetPassCount(short machineID, DateTime time) =>
      GetTargetValues(machineID).TargetPassCountStateEvents.GetValueAtDate(time, out int _);

    private void CalculateSummaryCellAttributeData()
    {
      bool DataStillRequiredForCCV;
      bool DataStillRequiredForMDP;
      bool DataStillRequiredForCCA;
      bool DataStillRequiredForTMP;
      bool HaveCompositeSurfaceForCell;
      uint CellX, CellY;
      TargetPassCountRange PassCountTargetRange = new TargetPassCountRange();
      ushort TempPassCountTarget;
      int PassSearchIdx;

//with ProfileCell, LiftBuildSettings do
      {
        ProfileCell.CellCCV = CellPass.NullCCV;
        ProfileCell.CellTargetCCV = CellPass.NullCCV;

        ProfileCell.CellMDP = CellPass.NullMDP;
        ProfileCell.CellTargetMDP = CellPass.NullMDP;

        ProfileCell.CellCCA = CellPass.NullCCA;
        ProfileCell.CellTargetCCA = CellPass.NullCCA;

        ProfileCell.CellMaterialTemperature = CellPass.NullMaterialTemperatureValue;
        ProfileCell.CellMaterialTemperatureWarnMin = CellPass.NullMaterialTemperatureValue;
        ProfileCell.CellMaterialTemperatureWarnMax = CellPass.NullMaterialTemperatureValue;

        ProfileCell.CellPreviousMeasuredCCV = CellPass.NullCCV;
        ProfileCell.CellPreviousMeasuredTargetCCV = CellPass.NullCCV;

        ProfileCell.CellTopLayerThickness = Consts.NullHeight;

        ProfileCell.TopLayerPassCount = 0;
        PassCountTargetRange.SetMinMax(0, 0);

        ProfileCell.CellMaxSpeed = 0;
        ProfileCell.CellMinSpeed = CellPass.NullMachineSpeed;

        ProfileCell.TopLayerPassCountTargetRangeMin = ProfileCell.TopLayerPassCount;
        ProfileCell.TopLayerPassCountTargetRangeMax = ProfileCell.TopLayerPassCount;

// WorkOut Speed Min Max
        if (ProfileCell.Layers.Count() > 0)
        {
          for (int I = ProfileCell.Layers.Count() - 1; I >= 0; I--)
          {
            if (ProfileCell.Layers[I].FilteredPassCount > 0)
            {
              if ((LayerStatus.Superseded & ProfileCell.Layers[I].Status) != 0)
                continue;

              for (int PassIndex = ProfileCell.Layers[I].StartCellPassIdx;
                PassIndex < ProfileCell.Layers[I].EndCellPassIdx;
                PassIndex++)
              {
                if (Passes.FilteredPassData[PassIndex].FilteredPass.MachineSpeed < ProfileCell.CellMinSpeed)
                  ProfileCell.CellMinSpeed = Passes.FilteredPassData[PassIndex].FilteredPass.MachineSpeed;
                if (Passes.FilteredPassData[PassIndex].FilteredPass.MachineSpeed > ProfileCell.CellMaxSpeed)
                  ProfileCell.CellMaxSpeed = Passes.FilteredPassData[PassIndex].FilteredPass.MachineSpeed;
              }
            }
          }
        }

        if (ProfileCell.Layers.Count() > 0)
          for (int I = ProfileCell.Layers.Count() - 1; I >= 0; I--)
//with ProfileCell.Layers[i] do
            if (ProfileCell.FilteredPassCount > 0)
            {
              if ((LayerStatus.Superseded & ProfileCell.Layers[I].Status) != 0)
                continue;

              ProfileCell.TopLayerPassCount = (ushort) (ProfileCell.FilteredHalfPassCount / 2);

              if (Dummy_LiftBuildSettings.OverrideTargetPassCount)
              {
                ProfileCell.TopLayerPassCountTargetRangeMin =
                  Dummy_LiftBuildSettings.OverridingTargetPassCountRange.Min;
                ProfileCell.TopLayerPassCountTargetRangeMax =
                  Dummy_LiftBuildSettings.OverridingTargetPassCountRange.Max;
              }
              else if (ProfileCell.Layers[I].TargetPassCount == 0)
              {
                //with Passes.FilteredPassData[EndCellPassIdx].FilteredPass do
                //  {
                TempPassCountTarget =
                  GetTargetPassCount(
                    Passes.FilteredPassData[ProfileCell.Layers[I].EndCellPassIdx].FilteredPass
                      .InternalSiteModelMachineIndex,
                    Passes.FilteredPassData[ProfileCell.Layers[I].EndCellPassIdx].FilteredPass.Time);
                PassCountTargetRange.SetMinMax(TempPassCountTarget, TempPassCountTarget);
                ProfileCell.TopLayerPassCountTargetRangeMin = PassCountTargetRange.Min;
                ProfileCell.TopLayerPassCountTargetRangeMax = PassCountTargetRange.Max;
                //   }
              }
              else
              {
                PassCountTargetRange.SetMinMax(ProfileCell.Layers[I].TargetPassCount,
                  ProfileCell.Layers[I].TargetPassCount);
                ProfileCell.TopLayerPassCountTargetRangeMin = PassCountTargetRange.Min;
                ProfileCell.TopLayerPassCountTargetRangeMax = PassCountTargetRange.Max;
              }

              break; // we have top layer
            }

        DataStillRequiredForCCV =
          (ProfileCell.AttributeExistenceFlags & ProfileCellAttributeExistenceFlags.HasCCAData) != 0;
        DataStillRequiredForMDP =
          (ProfileCell.AttributeExistenceFlags & ProfileCellAttributeExistenceFlags.HasMDPData) != 0;
        DataStillRequiredForCCA =
          (ProfileCell.AttributeExistenceFlags & ProfileCellAttributeExistenceFlags.HasCCAData) != 0;
        DataStillRequiredForTMP =
          (ProfileCell.AttributeExistenceFlags & ProfileCellAttributeExistenceFlags.HasTemperatureData) != 0;

        for (int I = ProfileCell.Layers.Count() - 1; I >= 0; I--)
// with Layers[i] do
          if (ProfileCell.FilteredPassCount > 0)
          {
            if ((LayerStatus.Superseded & ProfileCell.Layers[I].Status) != 0 &&
                !Dummy_LiftBuildSettings.IncludeSuperseded)
              continue;

            if (DataStillRequiredForCCV && ProfileCell.CellCCV == CellPass.NullCCV &&
                ProfileCell.Layers[I].CCV != CellPass.NullCCV)
            {
              ProfileCell.CellCCV = ProfileCell.Layers[I].CCV;
              ProfileCell.CellCCVElev = ProfileCell.Layers[I].CCV_Elev;

              PassSearchIdx = ProfileCell.Layers[I].CCV_CellPassIdx - 1;
              while (PassSearchIdx >= 0)
              {
                if (Dummy_LiftBuildSettings.CCVSummarizeTopLayerOnly &&
                    PassSearchIdx < ProfileCell.Layers[I].StartCellPassIdx ||
                    PassSearchIdx > ProfileCell.Layers[I].EndCellPassIdx)
                  break;

                if (!ProfileCell.Layers.IsCellPassInSupersededLayer(PassSearchIdx) ||
                    Dummy_LiftBuildSettings.IncludeSuperseded)
                {
                  ProfileCell.CellPreviousMeasuredCCV = ProfileCell.Passes.FilteredPassData[PassSearchIdx]
                    .FilteredPass.CCV;
                  if (Dummy_LiftBuildSettings.OverrideMachineCCV)
                    ProfileCell.CellPreviousMeasuredTargetCCV = Dummy_LiftBuildSettings.OverridingMachineCCV;
                  else
                    ProfileCell.CellPreviousMeasuredTargetCCV = ProfileCell.Passes
                      .FilteredPassData[PassSearchIdx].TargetValues.TargetCCV;
                  break;
                }

                PassSearchIdx--;
              }

              if (Dummy_LiftBuildSettings.OverrideMachineCCV)
                ProfileCell.CellTargetCCV = Dummy_LiftBuildSettings.OverridingMachineCCV;
              else if (ProfileCell.Layers[I].TargetCCV == CellPass.NullCCV)
                ProfileCell.CellTargetCCV =
                  GetTargetCCV(ProfileCell.Layers[I].CCV_MachineID, ProfileCell.Layers[I].CCV_Time);
              else
                ProfileCell.CellTargetCCV = ProfileCell.Layers[I].TargetCCV;

              DataStillRequiredForCCV = false;
            }

            if (DataStillRequiredForMDP && ProfileCell.CellMDP == CellPass.NullMDP &&
                ProfileCell.Layers[I].MDP != CellPass.NullMDP)
            {
              ProfileCell.CellMDP = ProfileCell.Layers[I].MDP;
              ProfileCell.CellMDPElev = ProfileCell.Layers[I].MDP_Elev;
              if (Dummy_LiftBuildSettings.OverrideMachineMDP)
                ProfileCell.CellTargetMDP = Dummy_LiftBuildSettings.OverridingMachineMDP;
              else if (ProfileCell.Layers[I].TargetMDP == CellPass.NullMDP)
                ProfileCell.CellTargetMDP =
                  GetTargetMDP(ProfileCell.Layers[I].MDP_MachineID, ProfileCell.Layers[I].MDP_Time);
              else
                ProfileCell.CellTargetMDP = ProfileCell.Layers[I].TargetMDP;

              DataStillRequiredForMDP = false;
            }

            if (DataStillRequiredForCCA && ProfileCell.CellCCA == CellPass.NullCCA &&
                ProfileCell.Layers[I].CCA != CellPass.NullCCA)
            {
              ProfileCell.CellCCA = ProfileCell.Layers[I].CCA;
              ProfileCell.CellCCAElev = ProfileCell.Layers[I].CCA_Elev;
              if (ProfileCell.Layers[I].TargetCCA == CellPass.NullCCA)
                ProfileCell.CellTargetCCA =
                  GetTargetCCA(ProfileCell.Layers[I].CCA_MachineID, ProfileCell.Layers[I].CCA_Time);
              else
                ProfileCell.CellTargetCCA = ProfileCell.Layers[I].TargetCCA;

              DataStillRequiredForCCA = false;
            }

            if (DataStillRequiredForTMP &&
                ProfileCell.CellMaterialTemperature == CellPass.NullMaterialTemperatureValue &&
                ProfileCell.Layers[I].MaterialTemperature != CellPass.NullMaterialTemperatureValue)
            {
              ProfileCell.CellMaterialTemperature = ProfileCell.Layers[I].MaterialTemperature;
              ProfileCell.CellMaterialTemperatureElev = ProfileCell.Layers[I].MaterialTemperature_Elev;

              if (Dummy_LiftBuildSettings.OverrideTemperatureWarningLevels)
              {
                ProfileCell.CellMaterialTemperatureWarnMin =
                  Dummy_LiftBuildSettings.OverridingTemperatureWarningLevels.Min;
                ProfileCell.CellMaterialTemperatureWarnMax =
                  Dummy_LiftBuildSettings.OverridingTemperatureWarningLevels.Max;
              }
              else if (ProfileCell.CellMaterialTemperatureWarnMin == CellPass.NullMaterialTemperatureValue &&
                       ProfileCell.CellMaterialTemperatureWarnMax == CellPass.NullMaterialTemperatureValue)
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
//with Layers[i] do
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
        CellX = ProfileCell.OTGCellX & SubGridTree.SubGridLocalKeyMask;
        CellY = ProfileCell.OTGCellY & SubGridTree.SubGridLocalKeyMask;
        HaveCompositeSurfaceForCell = CompositeHeightsGrid.ProdDataMap.BitSet(CellX, CellY);

        if (HaveCompositeSurfaceForCell)
        {
//with CompositeHeightsGrid.Cells[CellX, CellY] do
          {
            if ((CompositeHeightsGrid.Cells[CellX, CellY].LastHeightTime == DateTime.MinValue) ||
                ((ProfileCell.Passes.PassCount > 0) &&
                 (ProfileCell.Passes.LastPassTime() >
                  CompositeHeightsGrid.Cells[CellX, CellY].LastHeightTime)))
              ProfileCell.CellLastCompositeElev = ProfileCell.CellLastElev;
            else
              ProfileCell.CellLastCompositeElev = CompositeHeightsGrid.Cells[CellX, CellY].LastHeight;

            if ((CompositeHeightsGrid.Cells[CellX, CellY].LowestHeightTime == DateTime.MinValue) ||
                ((ProfileCell.Passes.PassCount > 0) &&
                 (ProfileCell.Passes.LowestPassTime() >
                  CompositeHeightsGrid.Cells[CellX, CellY].LowestHeightTime)))
              ProfileCell.CellLowestCompositeElev = ProfileCell.CellLowestElev;
            else
              ProfileCell.CellLowestCompositeElev =
                CompositeHeightsGrid.Cells[CellX, CellY].LowestHeight;

            if ((CompositeHeightsGrid.Cells[CellX, CellY].HighestHeightTime == DateTime.MinValue) ||
                ((ProfileCell.Passes.PassCount > 0) &&
                 (ProfileCell.Passes.HighestPassTime() >
                  CompositeHeightsGrid.Cells[CellX, CellY].HighestHeightTime)))
              ProfileCell.CellHighestCompositeElev = ProfileCell.CellHighestElev;
            else
              ProfileCell.CellHighestCompositeElev =
                CompositeHeightsGrid.Cells[CellX, CellY].HighestHeight;

            if ((CompositeHeightsGrid.Cells[CellX, CellY].FirstHeightTime == DateTime.MinValue) ||
                ((ProfileCell.Passes.PassCount > 0) &&
                 (ProfileCell.Passes.FirstPassTime >
                  CompositeHeightsGrid.Cells[CellX, CellY].FirstHeightTime)))
              ProfileCell.CellFirstCompositeElev = ProfileCell.CellFirstElev;
            else
              ProfileCell.CellFirstCompositeElev =
                CompositeHeightsGrid.Cells[CellX, CellY].FirstHeight;
          }
        }
        else
        {
          ProfileCell.CellLastCompositeElev = ProfileCell.CellLastElev;
          ProfileCell.CellLowestCompositeElev = ProfileCell.CellLowestElev;
          ProfileCell.CellHighestCompositeElev = ProfileCell.CellHighestElev;
          ProfileCell.CellFirstCompositeElev = ProfileCell.CellFirstElev;
        }
      }
    }


    public bool BuildLiftProfileFromInitialLayer()
    {
//{$IFDEF DEBUG}
//SIGLogMessage.PublishNoODS(Self, Format('BuildLiftProfileFromInitialLayer: Processing %d cells', [FProfileCells.Count]), slmcDebug);
//{$ENDIF}

      CurrentSubgridOrigin = new SubGridCellAddress(int.MaxValue, int.MaxValue);
      SubGrid = null;
      _SubGridAsLeaf = null;
      ProfileCell = null;
      FilterDesignElevations = null;
      IgnoreSubgrid = false;

//FLockTokenName = kProfilerLockToken + IntToStr(GetCurrentThreadID);
//FLockToken = LockTokenManager.AcquireToken(FLockTokenName);

      CompositeHeightsGrid = new ClientCompositeHeightsLeafSubgrid(SiteModel.Grid, null, SubGridTree.SubGridTreeLevels,
        SiteModel.Grid.CellSize, SubGridTree.DefaultIndexOriginOffset);

      try
      {
        try
        {
          for (int I = 0; I < ProfileCells.Count; I++)
          {
            ProfileCell = ProfileCells[I];

// get subgrid setup iterator and set cell address
// get sugbrid origin for cell address
            ThisSubgridOrigin = new SubGridCellAddress(ProfileCell.OTGCellX >> SubGridTree.SubGridIndexBitsPerLevel,
              ProfileCell.OTGCellY >> SubGridTree.SubGridIndexBitsPerLevel);

            if (!CurrentSubgridOrigin.Equals(ThisSubgridOrigin)) // if we have a new subgrid to fetch
            {
              IgnoreSubgrid = false;
              CurrentSubgridOrigin = ThisSubgridOrigin;

// release previous subgrid
// if (SubGrid != null && SubGrid.Locked && SubGrid.LockToken = FLockToken)
//   SubGrid.ReleaseLock(FLockToken);

              SubGrid = null;
              CompositeHeightsGrid.Clear();

// Does the subgridtree contain this node in it's existance map?
              if (PDExistenceMap[CurrentSubgridOrigin.X, CurrentSubgridOrigin.Y])
                SubGrid = SiteModel.Grid.LocateSubGridContaining(ProfileCell.OTGCellX, ProfileCell.OTGCellY,
                  SiteModel.Grid.NumLevels);

              if (SubGrid != null)
              {
                _SubGridAsLeaf = SubGrid as ServerSubGridTreeLeaf;
                CellPassIterator.SegmentIterator.SubGrid = _SubGridAsLeaf;
                CellPassIterator.SegmentIterator.Directory = _SubGridAsLeaf.Directory;
              }

              CompositeHeightsGrid.SetAbsoluteOriginPosition(
                (uint) (ProfileCell.OTGCellX & ~SubGridTree.SubGridLocalKeyMask),
                (uint) (ProfileCell.OTGCellY & ~SubGridTree.SubGridLocalKeyMask));
              CompositeHeightsGrid.SetAbsoluteLevel(SiteModel.Grid.NumLevels);

              if (LiftFilterMask.ConstructSubgridCellFilterMask(SiteModel.Grid, _SubGridAsLeaf, CurrentSubgridOrigin,
                ProfileCells, ref FilterMask, I, CellFilter))
                continue;

              if (FilteredSurveyedSurfaces != null)
              {
                /* TODO: Profile patch requests not implemented yet...
                // Hand client grid details, a mask of cells we need surveyed surface elevations for, and a temp grid to the Design Profiler
                SurfaceElevationPatchArg.CellSize = SiteModel.Grid.CellSize;
                SurfaceElevationPatchArg.OTGCellBottomLeftX = _SubGridAsLeaf.OriginX;
                SurfaceElevationPatchArg.OTGCellBottomLeftY = _SubGridAsLeaf.OriginY;
  
                SurfaceElevations = surfaceElevationPatchRequest.Execute(SurfaceElevationPatchArg);
  
                if (SurfaceElevations == null)
                {
                  Log.LogError(
                    "Call(B) to RequestSurveyedSurfacesProfilePatch in TICServerProfiler failed to return a composite profile grid.");
                  continue;
                }
  
                if (DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService
                      .RequestSurveyedSurfacesProfilePatch
                      (Construct_CalculateSurveyedSurfacesProfilePatch_Args(SiteModel.ID,
                          ProfileCell.OTGCellX, ProfileCell.OTGCellY,
                          SiteModel.Grid.CellSize, FilterMask),
                        FilteredSurveyedSurfaces, CompositeHeightsGrid) != dppiOK)
                {
                  Log.LogError(
                    "Call(B) to RequestSurveyedSurfacesProfilePatch in TICServerProfiler failed to return a composite profile grid.");
                  continue;
                }
                */
              }

              if (!LiftFilterMask.InitialiseFilterContext(SiteModel, PassFilter, ProfileCell,
                CellPassFilter_ElevationRangeDesign))
              {
                if (FilterDesignErrorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
                  IgnoreSubgrid = true;
                else
                  Log.LogError(
                    "Call to RequestDesignElevationPatch in TICServerProfiler for filter failed to return a elevation patch.");
                continue;
              }
            }

            if (SubGrid != null && !IgnoreSubgrid)
            {
              if (_SubGridAsLeaf != null)
                //with ProfileCell, _SubGridAsLeaf.Directory.GlobalLatestCells do
              {
                if (_SubGridAsLeaf.Directory.GlobalLatestCells.HasCCVData())
                  ProfileCell.AttributeExistenceFlags |= ProfileCellAttributeExistenceFlags.HasCCVData;

                if (_SubGridAsLeaf.Directory.GlobalLatestCells.HasMDPData())
                  ProfileCell.AttributeExistenceFlags |= ProfileCellAttributeExistenceFlags.HasMDPData;

                if (_SubGridAsLeaf.Directory.GlobalLatestCells.HasCCAData())
                  ProfileCell.AttributeExistenceFlags |= ProfileCellAttributeExistenceFlags.HasCCAData;

                if (_SubGridAsLeaf.Directory.GlobalLatestCells.HasTemperatureData())
                  ProfileCell.AttributeExistenceFlags |= ProfileCellAttributeExistenceFlags.HasTemperatureData;
              }

              // get cell address relative to subgrid and SetCellCoordinatesInSubgrid
              CellPassIterator.SetCellCoordinatesInSubgrid(
                (byte) (ProfileCells[I].OTGCellX & SubGridTree.SubGridLocalKeyMask),
                (byte) (ProfileCells[I].OTGCellY & SubGridTree.SubGridLocalKeyMask));
              PassFilter.InitaliaseFilteringForCell(CellPassIterator.CellX, CellPassIterator.CellY);

              /* TODO ... not supporting layer/lift analysis at this time
              // if (BuildLiftsForCell(cidBuildLiftProfileFromInitialLayer,
                ProfileCell,
                false,
                // Dummy_LiftBuildSettings,
                null, null, CellPassIterator, false, PassFilter,
                TopMostLayerPassCount,
                TopMostLayerCompactionHalfPassCount))
              {
                PopulationControl_AnySet = true;
                ProfileCell.IncludesProductionData = true;
                CalculateSummaryCellAttributeData();
              } 
              else End todo */
              {
                ProfileCell.ClearLayers();
                CalculateSummaryCellAttributeData();
              }
            }
            else
            {
              ProfileCell.ClearLayers();
              CalculateSummaryCellAttributeData();
            }
          }
        }
        finally
        {
          // release last subgrid
          //if (SubGrid != null && SubGrid.Locked && SubGrid.LockToken == FLockToken)
          //SubGrid.ReleaseLock(FLockToken);
        }
      }
      finally
      {
        //LockTokenManager.ReleaseToken(FLockTokenName);
      }

      return true;
    }
  }
}
