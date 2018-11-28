using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Responsible for orchestrating analysis of identified cells along the path of a profile line
  /// and deriving the profile related analytics for each cell
  /// </summary>
  public class ProfileLiftBuilder : IProfileLiftBuilder
  {
    private static ILogger Log = Logging.Logger.CreateLogger<ProfileLiftBuilder>();

    /// <summary>
    /// Local reference to the client subgrid factory
    /// </summary>
    private static IClientLeafSubgridFactory clientLeafSubGridFactory;
    private IClientLeafSubgridFactory ClientLeafSubGridFactory
      => clientLeafSubGridFactory ?? (clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubgridFactory>());

    /// <summary>
    /// The storage proxy to use when requesting subgrids for profiling operations
    /// </summary>
    private IStorageProxy storageProxy;
    private IStorageProxy StorageProxy => storageProxy ?? (storageProxy = DIContext.Obtain<ISiteModels>().StorageProxy);

    /// <summary>
    /// The number of passes identified in the top-most (most recent) layer
    /// </summary>
    public int TopMostLayerPassCount;

    /// <summary>
    /// The number of half-passes (recorded by machine that report passes as such)
    /// identified in the top-most (most recent) layer
    /// </summary>
    public int TopMostLayerCompactionHalfPassCount;

    /// <summary>
    /// The profile cell currently being analyzed
    /// </summary>
    private ProfileCell ProfileCell;

    /// <summary>
    /// The subgrid of composite elevations calculate from the collection of surveyed surfaces
    /// relevant to the profiling query
    /// </summary>
    private ClientCompositeHeightsLeafSubgrid CompositeHeightsGrid;

    private IClientLeafSubGrid CompositeHeightsGridIntf;

    /// <summary>
    /// The subgrid-by-subgrid filter mask used to control selection os surveyed surface
    /// and other cell data for each subgrid
    /// </summary>
    private SubGridTreeBitmapSubGridBits FilterMask = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    private ISiteModel SiteModel;
    private ICellPassAttributeFilter PassFilter;

    private ICellSpatialFilter CellFilter;
    private ISubGridTreeBitMask PDExistenceMap;

    /// <summary>
    /// Cell lift builder reference to the engine that performs detailed analytics on individual cells in the profile.
    /// </summary>
    private ICellLiftBuilder CellLiftBuilder;

    /// <summary>
    /// The set of surveyed surfaces that match the time constraints of the supplied filter.
    /// </summary>
    private ISurveyedSurfaces FilteredSurveyedSurfaces;

    /// <summary>
    /// The argument to be used when requesting composite elevation subgrids to support profile analysis
    /// </summary>
    public SurfaceElevationPatchArgument SurfaceElevationPatchArg;

    private SurfaceElevationPatchRequest SurfaceElevationPatchRequest;

    /// <summary>
    /// The design supplied as a result of an independent lookup outside the scope of this builder
    /// to find the design identified by the cellPassFilter.ElevationRangeDesignID
    /// </summary>
    private IDesign CellPassFilter_ElevationRangeDesign;

    /// <summary>
    /// Constructs a profile lift builder that analyzes cells in a cell profile vector
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="pDExistenceMap"></param>
    /// <param name="passFilter"></param>
    /// <param name="cellFilter"></param>
    /// <param name="cellPassFilter_ElevationRangeDesign"></param>
    /// <param name="cellLiftBuilder"></param>
    public ProfileLiftBuilder(ISiteModel siteModel,
      ISubGridTreeBitMask pDExistenceMap,
      ICellPassAttributeFilter passFilter,
      ICellSpatialFilter cellFilter,
      IDesign cellPassFilter_ElevationRangeDesign,
      ICellLiftBuilder cellLiftBuilder)
    {
      SiteModel = siteModel;
      PDExistenceMap = pDExistenceMap;
      PassFilter = passFilter;
      CellFilter = cellFilter;
      CellPassFilter_ElevationRangeDesign = cellPassFilter_ElevationRangeDesign;
      CellLiftBuilder = cellLiftBuilder;

      if (SiteModel.SurveyedSurfaces?.Count > 0)
      {
        // Filter out any surveyed surfaces which don't match current filter (if any)
        // - realistically, this is time filters we're thinking of here

        FilteredSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

        SiteModel.SurveyedSurfaces?.FilterSurveyedSurfaceDetails(PassFilter.HasTimeFilter, PassFilter.StartTime,
          PassFilter.EndTime, PassFilter.ExcludeSurveyedSurfaces(), FilteredSurveyedSurfaces,
          PassFilter.SurveyedSurfaceExclusionList);

        if (FilteredSurveyedSurfaces?.Count == 0)
          FilteredSurveyedSurfaces = null;
      }

      // Instantiate a single instance of the argument object for the surface elevation patch requests to obtain composite
      // elevation subgrids and populate it with the common elements for this set of subgrids being requested.
      SurfaceElevationPatchArg = new SurfaceElevationPatchArgument
      {
        SiteModelID = SiteModel.ID,
        CellSize = SiteModel.Grid.CellSize,
        IncludedSurveyedSurfaces = FilteredSurveyedSurfaces?.Select(x => x.ID).ToArray() ?? new Guid[0],
        SurveyedSurfacePatchType = SurveyedSurfacePatchType.CompositeElevations,
        ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled)
      };

      var _cache = DIContext.Obtain<ITRexSpatialMemoryCache>();
      var _context = _cache?.LocateOrCreateContext(SiteModel.ID, SurfaceElevationPatchArg.CacheFingerprint());

      SurfaceElevationPatchRequest = new SurfaceElevationPatchRequest(_cache, _context);
    }

    /// <summary>
    /// Returns the set of target values for given machine in the site model being processed.
    /// </summary>
    /// <param name="forMachineID"></param>
    /// <returns></returns>
    private IProductionEventLists GetTargetValues(short forMachineID) => SiteModel.MachinesTargetValues[forMachineID];

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
      minWarning = GetTargetValues(machineID).TargetMinMaterialTemperature.GetValueAtDate(time, out int _);
      maxWarning = GetTargetValues(machineID).TargetMinMaterialTemperature.GetValueAtDate(time, out int _);
    }

    /// <summary>
    /// Gets the target CCV for a machine at a given time
    /// </summary>
    /// <param name="machineID"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private short GetTargetCCV(short machineID, DateTime time) =>
      GetTargetValues(machineID).TargetCCVStateEvents.GetValueAtDate(time, out int _);

    /// <summary>
    /// Gets the target MDP for a machine at a given time
    /// </summary>
    /// <param name="machineID"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private short GetTargetMDP(short machineID, DateTime time) =>
      GetTargetValues(machineID).TargetMDPStateEvents.GetValueAtDate(time, out int _);

    /// <summary>
    /// Gets the target CCA for a machine at a given time
    /// </summary>
    /// <param name="machineID"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private short GetTargetCCA(short machineID, DateTime time) =>
      GetTargetValues(machineID).TargetCCAStateEvents.GetValueAtDate(time, out int _);

    /// <summary>
    /// Gets the target pass count for a machine at a given time
    /// </summary>
    /// <param name="machineID"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private ushort GetTargetPassCount(short machineID, DateTime time) =>
      GetTargetValues(machineID).TargetPassCountStateEvents.GetValueAtDate(time, out int _);

    /// <summary>
    /// Determines a set of summary attributes for the cell being analysed
    /// </summary>
    private void CalculateSummaryCellAttributeData()
    {
      TargetPassCountRange PassCountTargetRange = new TargetPassCountRange();

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

// WorkOut Speed Min Max
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
              PassIndex < ProfileCell.Layers[I].EndCellPassIdx;
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

            if (Dummy_LiftBuildSettings.OverrideTargetPassCount)
            {
              ProfileCell.TopLayerPassCountTargetRangeMin = Dummy_LiftBuildSettings.OverridingTargetPassCountRange.Min;
              ProfileCell.TopLayerPassCountTargetRangeMax = Dummy_LiftBuildSettings.OverridingTargetPassCountRange.Max;
            }
            else if (ProfileCell.Layers[I].TargetPassCount == 0)
            {
              //with Passes.FilteredPassData[EndCellPassIdx].FilteredPass do
              //  {
              ushort TempPassCountTarget =
                GetTargetPassCount(
                  ProfileCell.Passes.FilteredPassData[ProfileCell.Layers[I].EndCellPassIdx].FilteredPass
                    .InternalSiteModelMachineIndex,
                  ProfileCell.Passes.FilteredPassData[ProfileCell.Layers[I].EndCellPassIdx].FilteredPass.Time);
              PassCountTargetRange.SetMinMax(TempPassCountTarget, TempPassCountTarget);
              ProfileCell.TopLayerPassCountTargetRangeMin = PassCountTargetRange.Min;
              ProfileCell.TopLayerPassCountTargetRangeMax = PassCountTargetRange.Max;
              //   }
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
                if (Dummy_LiftBuildSettings.OverrideMachineCCV)
                  ProfileCell.CellPreviousMeasuredTargetCCV = Dummy_LiftBuildSettings.OverridingMachineCCV;
                else
                  ProfileCell.CellPreviousMeasuredTargetCCV = ProfileCell.Passes.FilteredPassData[PassSearchIdx].TargetValues.TargetCCV;
                break;
              }

              PassSearchIdx--;
            }

            if (Dummy_LiftBuildSettings.OverrideMachineCCV)
              ProfileCell.CellTargetCCV = Dummy_LiftBuildSettings.OverridingMachineCCV;
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
            if (Dummy_LiftBuildSettings.OverrideMachineMDP)
              ProfileCell.CellTargetMDP = Dummy_LiftBuildSettings.OverridingMachineMDP;
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
            if (ProfileCell.Layers[I].TargetCCA == CellPassConsts.NullCCA)
              ProfileCell.CellTargetCCA = GetTargetCCA(ProfileCell.Layers[I].CCA_MachineID, ProfileCell.Layers[I].CCA_Time);
            else
              ProfileCell.CellTargetCCA = ProfileCell.Layers[I].TargetCCA;

            DataStillRequiredForCCA = false;
          }

          if (DataStillRequiredForTMP &&
              ProfileCell.CellMaterialTemperature == CellPassConsts.NullMaterialTemperatureValue &&
              ProfileCell.Layers[I].MaterialTemperature != CellPassConsts.NullMaterialTemperatureValue)
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
      uint CellX = ProfileCell.OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask;
      uint CellY = ProfileCell.OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask;
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
    /// <param name="ProfileCells"></param>
    /// <param name="cellPassIterator"></param>
    /// <returns></returns>
    public bool Build(List<IProfileCell> ProfileCells, ISubGridSegmentCellPassIterator cellPassIterator)
    {
      //{$IFDEF DEBUG}
      //SIGLogMessage.PublishNoODS(Self, Format('BuildLiftProfileFromInitialLayer: Processing %d cells', [FProfileCells.Count]), slmcDebug);
      //{$ENDIF}

      SubGridCellAddress CurrentSubgridOrigin = new SubGridCellAddress(int.MaxValue, int.MaxValue);
      ISubGrid SubGrid = null;
      IServerLeafSubGrid _SubGridAsLeaf = null;
      ProfileCell = null;
//      FilterDesignElevations = null;
      bool IgnoreSubgrid = false;

      for (int I = 0; I < ProfileCells.Count; I++)
      {
        ProfileCell = (ProfileCell) ProfileCells[I];

        // get subgrid setup iterator and set cell address
        // get subgrid origin for cell address
        SubGridCellAddress ThisSubgridOrigin = new SubGridCellAddress(ProfileCell.OTGCellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
          ProfileCell.OTGCellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel);

        if (!CurrentSubgridOrigin.Equals(ThisSubgridOrigin)) // if we have a new subgrid to fetch
        {
          IgnoreSubgrid = false;
          CurrentSubgridOrigin = ThisSubgridOrigin;
          SubGrid = null;

          // Does the subgrid tree contain this node in it's existence map?
          if (PDExistenceMap[CurrentSubgridOrigin.X, CurrentSubgridOrigin.Y])
            SubGrid = SubGridTrees.Server.Utilities.SubGridUtilities.LocateSubGridContaining
              (StorageProxy, SiteModel.Grid, ProfileCell.OTGCellX, ProfileCell.OTGCellY, SiteModel.Grid.NumLevels, false, false);

          // SubGrid = SiteModel.Grid.LocateSubGridContaining(ProfileCell.OTGCellX, ProfileCell.OTGCellY, SiteModel.Grid.NumLevels);

          _SubGridAsLeaf = SubGrid as ServerSubGridTreeLeaf;
          if (_SubGridAsLeaf == null)
            continue;

          cellPassIterator.SegmentIterator.SubGrid = _SubGridAsLeaf;
          cellPassIterator.SegmentIterator.Directory = _SubGridAsLeaf.Directory;

          if (CompositeHeightsGrid != null)
          {
            ClientLeafSubGridFactory.ReturnClientSubGrid(ref CompositeHeightsGridIntf);
            CompositeHeightsGrid = null;
          }

          if (!LiftFilterMask.ConstructSubgridCellFilterMask(SiteModel.Grid, CurrentSubgridOrigin,
            ProfileCells, FilterMask, I, CellFilter))
            continue;

          if (FilteredSurveyedSurfaces != null)
          {
            // Hand client grid details, a mask of cells we need surveyed surface elevations for, and a temp grid to the Design Profiler
            SurfaceElevationPatchArg.OTGCellBottomLeftX = _SubGridAsLeaf.OriginX;
            SurfaceElevationPatchArg.OTGCellBottomLeftY = _SubGridAsLeaf.OriginY;
            SurfaceElevationPatchArg.ProcessingMap.Assign(FilterMask);

            CompositeHeightsGridIntf = SurfaceElevationPatchRequest.Execute(SurfaceElevationPatchArg);
            CompositeHeightsGrid = CompositeHeightsGridIntf as ClientCompositeHeightsLeafSubgrid;

            if (CompositeHeightsGrid == null)
            {
              Log.LogError("Call(B) to SurfaceElevationPatchRequest failed to return a composite profile grid.");
              continue;
            }
          }

          if (!LiftFilterMask.InitialiseFilterContext(SiteModel, PassFilter, ProfileCell,
            CellPassFilter_ElevationRangeDesign, out DesignProfilerRequestResult FilterDesignErrorCode))
          {
            if (FilterDesignErrorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
              IgnoreSubgrid = true;
            else
              Log.LogError("Call to RequestDesignElevationPatch in TICServerProfiler for filter failed to return an elevation patch.");
            continue;
          }
        }

        if (SubGrid != null && !IgnoreSubgrid)
        {
          if (_SubGridAsLeaf != null)
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
          cellPassIterator.SetCellCoordinatesInSubgrid(
            (byte) (ProfileCells[I].OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask),
            (byte) (ProfileCells[I].OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask));
          PassFilter.InitaliaseFilteringForCell(cellPassIterator.CellX, cellPassIterator.CellY);

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
