using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Types;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
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
  public class SummaryVolumesCellProfileAnalyzer : CellProfileAnalyzerBase<SummaryVolumeProfileCell>
  {
    private static ILogger Log = Logging.Logger.CreateLogger<CellProfileAnalyzer>();

    private SummaryVolumeProfileCell profileCell;
    public VolumeComputationType VolumeType { get; set; } = VolumeComputationType.None;

    private SummaryVolumesCellProfileAnalyzer()
    {}

    /// <summary>
    /// Constructs a profile lift builder that analyzes cells in a cell profile vector
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="pDExistenceMap"></param>
    /// <param name="filterSet"></param>
    /// <param name="cellPassFilter_ElevationRangeDesign"></param>
    /// <param name="cellLiftBuilder"></param>
    public SummaryVolumesCellProfileAnalyzer(ISiteModel siteModel,
      ISubGridTreeBitMask pDExistenceMap,
      IFilterSet filterSet,
      IDesign cellPassFilter_ElevationRangeDesign,
      ICellLiftBuilder cellLiftBuilder) : base(siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign)
    {
    }


    public void ProcessSubGroup()
    {


      // This this subgrid get relevant data based upon VolumeType requested

      int I,J,K;
      int YCell;
      float H1, H2;
      float StationAtNextCellBorder;
      bool OKToAdd;

      if (VolumeType == VolumeComputationType.BetweenFilterAndDesign || VolumeType == VolumeComputationType.Between2Filters)
      {
        var acs = new AreaControlSet();
        // to do get subgrid
      }





    }


    /// <summary>
    /// Builds a fully analyzed vector of profiled cells from the list of cell passed to it
    /// </summary>
    /// <param name="profileCells"></param>
    /// <param name="cellPassIterator"></param>
    /// <returns></returns>
    ///
    public override bool Analyze(List<SummaryVolumeProfileCell> profileCells, ISubGridSegmentCellPassIterator cellPassIterator)
    {
      //{$IFDEF DEBUG}
      //SIGLogMessage.PublishNoODS(Self, Format('BuildLiftProfileFromInitialLayer: Processing %d cells', [FProfileCells.Count]), slmcDebug);
      //{$ENDIF}

      SubGridCellAddress CurrentSubgridOrigin = new SubGridCellAddress(int.MaxValue, int.MaxValue);
      ISubGrid SubGrid = null;
      IServerLeafSubGrid _SubGridAsLeaf = null;
      profileCell = null;
      //      FilterDesignElevations = null;
      bool IgnoreSubgrid = false;

      for (int I = 0; I < profileCells.Count; I++)
      {

        profileCell = profileCells[I];

        // get subgrid setup iterator and set cell address
        // get subgrid origin for cell address
        SubGridCellAddress thisSubgridOrigin = new SubGridCellAddress(profileCell.OTGCellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
          profileCell.OTGCellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel);

        if (!CurrentSubgridOrigin.Equals(thisSubgridOrigin)) // if we have a new subgrid to fetch
        {
          IgnoreSubgrid = false;
          CurrentSubgridOrigin = thisSubgridOrigin;
          SubGrid = null;

          // Does the subgrid tree contain this node in it's existence map?
          if (PDExistenceMap[CurrentSubgridOrigin.X, CurrentSubgridOrigin.Y])
            SubGrid = SubGridTrees.Server.Utilities.SubGridUtilities.LocateSubGridContaining
              (StorageProxy, SiteModel.Grid, profileCell.OTGCellX, profileCell.OTGCellY, SiteModel.Grid.NumLevels, false, false);

          _SubGridAsLeaf = SubGrid as ServerSubGridTreeLeaf;
          if (_SubGridAsLeaf == null)
            continue;
        }

        if (SubGrid != null && !IgnoreSubgrid)
        {
          ProcessSubGroup();
        }
      }

      return true;
    }
  }
}

/*
  TICSVServerProfiler = class     // Summary Volumes Profiler
  private
    FAborted : Boolean;
    FSubGridTree: TICServerSubGridTree;
    FProfileTypeRequired: TICGridDataType;
    FSVProfileCells: TICSummaryVolumesProfileCellList;
    FSiteModel: TICSiteModel;
    FPDExistenceMap : TSubGridTreeBitMask;
    FGridDistanceBetweenProfilePoints : Double;
    FLastGetTargetValues_MachineID : TICMachineID;
// RCE 36155    FMachineTargetValuesEventsLocked : Boolean;
    FPopulationControl : TFilteredValuePopulationControl;
    FPopulationControl_AnySet : Boolean;

    FVolumeType : TComputeICVolumesType;
    FDesignFile : TVLPDDesignDescriptor;
    FReturnDesignElevation : Boolean;
  public
    property Profile: TICSummaryVolumesProfileCellList read FSVProfileCells;
    property Aborted : Boolean read FAborted;
    property GridDistanceBetweenProfilePoints : double read FGridDistanceBetweenProfilePoints;
    property PopulationControl : TFilteredValuePopulationControl read FPopulationControl write FPopulationControl;

    constructor Create(SiteModel: TICSiteModel;
                       ASubGridTree: TICServerSubGridTree;
                       const APDExistenceMap : TSubGridTreeBitMask;
                       const AProfileTypeRequired: TICGridDataType;
                       const APopulationControl : TFilteredValuePopulationControl;
                       const AVolumeType : TComputeICVolumesType;
                       const ADesignfile : TVLPDDesignDescriptor);

    function BuildCellPassProfile(const PassFilter1   : TICGridDataPassFilter;
                                  const PassFilter2   : TICGridDataPassFilter;
                                  const CellFilter1   : TICGridDataCellSelectionFilter;
                                  const CellFilter2   : TICGridDataCellSelectionFilter;
                                  const GridDataCache : TICDataStoreCache;
                                  const NEECoords     : TCSConversionCoordinates) : Boolean;


    Procedure Abort;
  end;

const
  ListInc = 1000;
  MaxIntercepts = 10000;  // note this should give a profile at least up to 2-3km long

constructor TICSVServerProfiler.Create(SiteModel: TICSiteModel;
                                     ASubGridTree: TICServerSubGridTree;
                                     const APDExistenceMap : TSubGridTreeBitMask;
                                     const AProfileTypeRequired: TICGridDataType;
                                     const APopulationControl : TFilteredValuePopulationControl;
                                     const AVolumeType : TComputeICVolumesType;
                                     const ADesignfile : TVLPDDesignDescriptor);
begin
  FAborted := False;

  FSubGridTree := ASubGridTree;
  FProfileTypeRequired := AProfileTypeRequired;
  FSVProfileCells := TICSummaryVolumesProfileCellList.Create;

//  FOverallExistenceMap := AOverallExistenceMap;
  FPDExistenceMap := APDExistenceMap;
  FGridDistanceBetweenProfilePoints := 0;

  FSiteModel := SiteModel;

  FPopulationControl := APopulationControl;

// RCE 36155  FMachineTargetValuesEventsLocked := False;
  FLastGetTargetValues_MachineID := -1;
  FVolumeType := AVolumeType;
  FDesignFile := ADesignfile;

end;

procedure TICSVServerProfiler.Abort;
begin
  FAborted := True;
end;

function TICSVServerProfiler.BuildCellPassProfile(const PassFilter1 : TICGridDataPassFilter;
                                                  const PassFilter2 : TICGridDataPassFilter;
                                                  const CellFilter1 : TICGridDataCellSelectionFilter;
                                                  const CellFilter2 : TICGridDataCellSelectionFilter;
                                                  const GridDataCache : TICDataStoreCache;
                                                  const NEECoords     : TCSConversionCoordinates) : Boolean;
const
  kMaxHzVtGridInterceptsToCalculate = 8000;
var
  I              : Integer;
  TotalCells     : Integer;
  BasePosition   : Integer;
  ArrayCount     : Integer;
  SlicerToolUsed : Boolean;

  VtIntercepts, HzIntercepts, VtHzIntercepts    : TInterceptList;
  NVtIntercepts, NHzIntercepts, NVtHzIntercepts : Integer;
  CellX, CellY  : Integer;
  OTGCellX, OTGCellY  : Integer;

  FLockToken : Integer;
  FLockTokenName : String;
  CurrentSubgridOrigin : TSubgridCellAddress;
  ThisSubgridOrigin : TSubgridCellAddress;
  HeightsGrid1_: TICSubGridTreeLeafSubGridBase;
  HeightsGrid2_: TICSubGridTreeLeafSubGridBase;
  HeightsGrid1: TICClientSubGridTreeLeaf_HeightAndTime;
  HeightsGrid2: TICClientSubGridTreeLeaf_HeightAndTime;
  StartX, StartY, StartStation : Double;
  EndX, EndY, EndStation : Double;
  CurrStationPos : double;
  Distance : Double;

  ServerResult : TICServerRequestResult;
  LiftBuildSettings: TICLiftBuildSettings;
  CellOverrideMask : TSubGridTreeLeafBitmapSubGridBits;

  GridResults : array[0..1024, 0..1] of Integer;
  Height1 : Single;
  Height2 : Single;
  DesignHeight : Single;

  DesignElevations : TICClientSubGridTreeLeaf_Height;
  DesignResult : TDesignProfilerRequestResult;

  ProcessCount : Integer;

  function AddCellPassesDataToList(const FilteredHeight1: Single;
                                    const FilteredHeight2: Single;
                                    const DesignHeight:Single;
                                    const Station, InterceptLength: Float) : boolean;
  var
    ProfileCell : TICSummaryVolumesProfileCell;

  begin
    Result := False;
    if (FilteredHeight1 <> kICNullHeight) or (FilteredHeight2 <> kICNullHeight)  then
    begin
      ProfileCell := TICSummaryvolumesProfileCell.Create(FilteredHeight1,FilteredHeight2, DesignHeight, 0, 0, Station, InterceptLength);
      FSVProfileCells.Add(ProfileCell);
      Result := True;
    end;
  end;

  procedure ProcessSubGroup;
  Var I, J, K : Integer;
      XCell, YCell  : Integer;
      H1, H2 : Single;
      StationAtNextCellBorder : double;
      OktoAdd : Boolean;
      AreaControlSet : TAreaControlSet;
  begin
    if TotalCells = 0 then
      Exit;

    SIGLogMessage.Publish(nil, 'Summary Volumes Profile BuildCellPassProfile. In ProcesssubGroup TotalCells = ' + InTToStr(TotalCells), slmcDebug);

    ServerResult := icsrrNoError;

    // now get the compositeheights for subgrid just passed over
    if (FVolumeType in [ic_cvtBetweenFilterAndDesign, ic_cvtBetween2Filters]) then
    begin
      AreaControlSet.Init(0,0,0,0,0,True);
      HeightsGrid1.ProdDataMap := CellOverrideMask;
      ServerResult := TSubGridRequestor.RequestSubGridInternal(Nil,
                                                               PassFilter1,
                                                               VLPDSvcLocations.VLPDPSNode_MaxCellPassIterationDepth_PassCountDetailAndSummary,
                                                               CellFilter1,
                                                               False,
                                                               T2DBoundingIntegerExtent.Create(0, 0, 0, 0),
                                                               FSiteModel,
                                                               CurrentSubgridOrigin.X SHL kSubGridIndexBitsPerLevel, CurrentSubgridOrigin.Y SHL kSubGridIndexBitsPerLevel,
                                                               FSiteModel.Grid.NumLevels,
                                                               LiftBuildSettings,
                                                               FLockToken,
                                                               FPDExistenceMap.Cells[CurrentSubgridOrigin.X, CurrentSubgridOrigin.Y],
                                                               True,
                                                               HeightsGrid1_,
                                                               CellOverrideMask,
                                                               AreaControlSet);
    end;

    // FIntermediary filter is used to assist calculation of the volumetric work
    // done between two points in time. Conceptually, the from surface is defined
    // as a combination of the surface data using the latest available information
    // for an AsAt time filter, ['from', representing the start of the time period] and combining
    // it with the surface data using the earliest available information for a
    // time range filter defined as starting at the AsAt data of the from filter and
    // [representing work started after the AsAt data of the 'from' filter]
    // terminating at the end date of the 'To'. This combined surface is then
    // compared to a surface constructed from the latest available information
    // for the supplied To Filter.
    //
    // The intermediary filter is derived from the 'To' filter and is altered to be
    // an 'earliest' filter with all other attributes remaining unchanged and is
    // used to calculate the additional elevation information to be combined with
    // the 'AsAt'/latest surface information from the 'From' filter.
    //
    // The conditions for using the intermedairy filter are then:
    // 1. A summary volume requested between two filters
    // 2. The 'from' filter is defined as an 'As-At' filter, with latest data selected
    // 3. The 'to' filter is defined either as an 'As-At' or a time range filter,
    //    with latest data selected
    //
    // Note: No 'look forward' behaviour should be undertaken.

    if (ServerResult = icsrrNoError) and (FVolumeType = ic_cvtBetween2Filters) then
      begin
        // Determine if intermediary filter/surface behaviour is required to
        // support summary volumes
        if (PassFilter1.HasTimeFilter and (PassFilter1.StartTime = 0)) // 'From' has As-At Time filter
           and not PassFilter1.ReturnEarliestFilteredCellPass // Want latest cell pass in 'from'
           and (PassFilter2.HasTimeFilter and (PassFilter2.StartTime <> 0)) // 'To' has time-range filter with latest
           and not PassFilter2.ReturnEarliestFilteredCellPass // Want latest cell pass in 'to'
          then
        begin
          // Modify and use filter2 as the intermediary filter. The only modification is the
          // setting of ReturnEarliestFilteredCellPass to true, this will be reversed once the
          // request is made
          PassFilter2.ReturnEarliestFilteredCellPass := true;
          try
            // Make the query to obtain the modified filter subgrid...
            AreaControlSet.Init(0,0,0,0,0,True);
            HeightsGrid2.ProdDataMap := CellOverrideMask;
            ServerResult := TSubGridRequestor.RequestSubGridInternal
              (Nil,
               PassFilter2,
               VLPDSvcLocations.VLPDPSNode_MaxCellPassIterationDepth_PassCountDetailAndSummary,
               CellFilter2,
               False,
               T2DBoundingIntegerExtent.Create(0, 0, 0, 0),
               FSiteModel,
               CurrentSubgridOrigin.X SHL kSubGridIndexBitsPerLevel, CurrentSubgridOrigin.Y SHL kSubGridIndexBitsPerLevel,
               FSiteModel.Grid.NumLevels,
               LiftBuildSettings,
               FLockToken,
               FPDExistenceMap.Cells[CurrentSubgridOrigin.X, CurrentSubgridOrigin.Y],
               True,
               HeightsGrid2_,
               CellOverrideMask,
               AreaControlSet);

            // Combine this result with the result of the first query to obtain a modified HeightsGrid1_
            // the case then the first two subgrid results will be HeightAndTime elevation subgrids and will
            // need to be merged into a single height and time subgrid before any secondary conversion of intermediary
            // results in the logic below.
            HeightsGrid1 := HeightsGrid1_ as TICClientSubGridTreeLeaf_HeightAndTime;
            HeightsGrid2 := HeightsGrid2_ as TICClientSubGridTreeLeaf_HeightAndTime;

            // Merge the first two results to give the profile calc the correct combined 'from' surface
            // HeightsGrid1 is 'latest @ first filter', HeightsGrid1 is earliest @ second filter
            for I := 0 to kSubGridTreeDimension - 1 do
              for J := 0 to kSubGridTreeDimension - 1 do
                begin
                  if (HeightsGrid1.Cells[I, J].Height = kICNullHeight) and // Check if there is a non null candidate in the earlier @ second filter
                     (HeightsGrid2.Cells[I, J].Height <> kICNullHeight) then
                    HeightsGrid1.Cells[I, J] := HeightsGrid2.Cells[I, J];
                end;
          finally
            PassFilter2.ReturnEarliestFilteredCellPass := false;
          end;
        end;
      end;

    if (FVolumeType in [ic_cvtBetweenDesignAndFilter, ic_cvtBetween2Filters]) then
    begin
      if ServerResult = icsrrNoError then
      begin
        AreaControlSet.Init(0,0,0,0,0,True);
        HeightsGrid2.ProdDataMap := CellOverrideMask;
        ServerResult := TSubGridRequestor.RequestSubGridInternal(Nil,
                                                                 PassFilter2,
                                                                 VLPDSvcLocations.VLPDPSNode_MaxCellPassIterationDepth_PassCountDetailAndSummary,
                                                                 CellFilter2,
                                                                 False,
                                                                 T2DBoundingIntegerExtent.Create(0, 0, 0, 0),
                                                                 FSiteModel,
                                                                 CurrentSubgridOrigin.X SHL kSubGridIndexBitsPerLevel, CurrentSubgridOrigin.Y SHL kSubGridIndexBitsPerLevel,
                                                                 FSiteModel.Grid.NumLevels,
                                                                 LiftBuildSettings,
                                                                 FLockToken,
                                                                 FPDExistenceMap.Cells[CurrentSubgridOrigin.X, CurrentSubgridOrigin.Y],
                                                                 True,
                                                                 HeightsGrid2_,
                                                                 CellOverrideMask,
                                                                 AreaControlSet);
       end;
    end;

    HeightsGrid1 := HeightsGrid1_ as TICClientSubGridTreeLeaf_HeightAndTime;
    HeightsGrid2 := HeightsGrid2_ as TICClientSubGridTreeLeaf_HeightAndTime;

    if (FVolumeType in [ic_cvtBetweenDesignAndFilter, ic_cvtBetweenFilterAndDesign]) then
    begin  // if design used

      if (ServerResult = icsrrNoError) and FReturnDesignElevation then
      begin
        if Assigned(DesignElevations) then
          FreeAndNil(DesignElevations);

        DesignResult := DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService.RequestDesignElevationPatch(
                               Construct_CalculateDesignElevationPatch_Args(FSiteModel.ID, CurrentSubgridOrigin.X SHL kSubGridIndexBitsPerLevel, CurrentSubgridOrigin.Y SHL kSubGridIndexBitsPerLevel, FSubGridTree.CellSize, FDesignFile, TSubGridTreeLeafBitmapSubGridBits.FullMask), DesignElevations);

        if (DesignResult <> dppiOK) and (DesignResult <> dppiNoElevationsInRequestedPatch) then
          begin
            SIGLogMessage.PublishNoODS(nil, 'Call to RequestDesignElevationPatch in TICSVServerProfiler failed to return a elevation patch.', slmcError);
            ServerResult := icsrrUnknownError;
          end;

        if (DesignResult = dppiNoElevationsInRequestedPatch) then
          if Assigned(DesignElevations) then
            FreeAndNil(DesignElevations); // force a nullsingle to be written
      end;
    end;

    if ServerResult = icsrrNoError then
    begin
      // Process returned height grid and extract cells we are interested in
      for K := 0 to TotalCells -1 do
      begin

        CellX := GridResults[K,0]; // cell address from when we constructed bitmask
        CellY := GridResults[K,1];
        Height1 := HeightsGrid1.Cells[CellX,CellY].Height;
        Height2 := HeightsGrid2.Cells[CellX,CellY].Height;
        if Assigned(DesignElevations) then
          DesignHeight := DesignElevations.Cells[CellX, CellY]
        else
          DesignHeight := NullSingle;
        inc(ProcessCount);

        OkToAdd := AddCellPassesDataToList(Height1, Height2, DesignHeight, VtHzIntercepts[BasePosition+K].ProfileItemIndex, VtHzIntercepts[BasePosition+K].InterceptLength);
        if OkToAdd and (K < TotalCells -1) then // if next cell is a gap so add a point at end of last good cell pos + intercept so cell is drawn to its border
        begin
          // Due to the way the map draws a full cell in profile view this little trick keeps the visual appearance in sync
          XCell := GridResults[K+1,0]; // cell address from when we constructed bitmask
          YCell := GridResults[K+1,1];
          H1 := HeightsGrid1.Cells[XCell,YCell].Height;
          H2 := HeightsGrid2.Cells[XCell,YCell].Height;
          if (H1 = kICNullHeight) and (H2 = kICNullHeight) then
          begin
            // The next point is a gap so lets make sure the last cell drawn all the way to the edge
            StationAtNextCellBorder := VtHzIntercepts[BasePosition+K].ProfileItemIndex + VtHzIntercepts[BasePosition+K].InterceptLength;
            if StationAtNextCellBorder <= EndStation then
               AddCellPassesDataToList(Height1, Height2, DesignHeight, StationAtNextCellBorder,0);
          end;
        end;

      end;
    end;

    CellOverrideMask.Clear;
    HeightsGrid1.Clear;
    HeightsGrid2.Clear;

    CurrentSubgridOrigin := ThisSubgridOrigin;

    TotalCells := 0;
  end;

begin
  Result := False;

  CurrStationPos := 0;
  SlicerToolUsed := False;

  DesignElevations := Nil;

  FReturnDesignElevation := FDesignFile.FileName <> '';

  NVtHzIntercepts := 0;

  CurrentSubgridOrigin := TSubgridCellAddress.CreateSimple(MaxInt, MaxInt);
  FPopulationControl_AnySet := FPopulationControl.AnySet;

  FGridDistanceBetweenProfilePoints := 0;
  ArrayCount := Length(NEECoords);

  ProcessCount := 0;

  //............................
  if NVtHzIntercepts = 0 then // if no intercepts exit
    Exit;

  // Create an empty Height SubGrid
  HeightsGrid1 := TICClientSubGridTreeLeaf_HeightAndTime.Create(FSubGridTree, nil, kSubGridTreeLevels, FSubGridTree.CellSize, TICClientSubGridTreeLeaf_HeightAndTime.DefaultIndexOriginOffset);
  HeightsGrid2 := TICClientSubGridTreeLeaf_HeightAndTime.Create(FSubGridTree, nil, kSubGridTreeLevels, FSubGridTree.CellSize, TICClientSubGridTreeLeaf_HeightAndTime.DefaultIndexOriginOffset);

  HeightsGrid1_ := HeightsGrid1;
  HeightsGrid2_ := HeightsGrid2;

  try
    HeightsGrid1.Clear; // important as it fulls heights will null values
    HeightsGrid2.Clear;
    CellOverrideMask.Clear;

    // get Lock name required to protect data we are looking at later on
    FLockTokenName := kProfilerLockToken + IntToStr(GetCurrentThreadID);
    FLockToken := LockTokenManager.AcquireToken(FLockTokenName);

    try
      TotalCells := 0;
      BasePosition := 0;

      if NVtHzIntercepts > FSVProfileCells.Capacity then // make sure we can store our results
        FSVProfileCells.Capacity := NVtHzIntercepts;

      LiftBuildSettings := TICLiftBuildSettings.Create; // defaults are fine

      try
        // Iterate over all intercepts calculating the results for each cell that lies in
        // a subgrid handled by this node

        SIGLogMessage.Publish(nil, 'Summary Volumes Profile BuildCellPassProfile. # NVtHzIntercepts = ' + IntToStr(NVtHzIntercepts), slmcDebug);

        for I := 0 to NVtHzIntercepts - 1 do
        begin
          if FAborted then
            Exit;

          // Determine the on-the-ground cell underneath the midpoint of each intercept line
          with VtHzIntercepts[I] do
            FSubGridTree.CalculateIndexOfCellContainingPosition(MidPointX, MidPointY, OTGCellX, OTGCellY);

          ThisSubgridOrigin := TSubgridCellAddress.CreateSimple(OTGCellX SHR kSubGridIndexBitsPerLevel,
                                                                OTGCellY SHR kSubGridIndexBitsPerLevel);
          if I = 0 then
            CurrentSubgridOrigin := ThisSubgridOrigin; // first time through so set

          // Do this PSNode care about this subgrid. Each PSNode has a unique id that determines what subgrids it takes care of
          try
            with VLPDDynamicSvcLocations do
              if PSNodeServicingSubgrid(ThisSubgridOrigin.ToSkipInterleavedDescriptor).Descriptor = PSNodeDescriptors.ThisNodeDescriptor then
                begin
                  if not CurrentSubgridOrigin.IsSameAs(ThisSubgridOrigin) then
                    begin
                      ProcessSubGroup;
                      BasePosition := I;
                    end;

                  HeightsGrid1.GetSubGridCellIndex(OTGCellX, OTGCellY, CellX, CellY);
                  CellOverrideMask.SetBit(CellX,CellY);
                  GridResults[TotalCells,0] := CellX;
                  GridResults[TotalCells,1] := CellY; // used later to extract height values from subgrid
                  inc(TotalCells);
                  if not CurrentSubgridOrigin.IsSameAs(ThisSubgridOrigin) then
                    CurrentSubgridOrigin := ThisSubgridOrigin; // make sure its set

                end
              else
                begin
                  ProcessSubGroup;
                  BasePosition := I;
                end;

          except
            On E:exception do
              begin
                SIGLogMessage.Publish(nil, 'Summary Volumes Profile BuildCellPassProfile. Unexpected error pos 1. I=' + IntToStr(I)+ ' err:'+ e.message, slmcError);
                Raise;
              end;
          end;

          if (I = NVtHzIntercepts - 1) then // last point
          begin
            try
              CurrentSubgridOrigin := ThisSubgridOrigin;
              ProcessSubGroup;
              BasePosition := I;
            except
              On E:exception do
                begin
                  SIGLogMessage.Publish(nil, 'Summary Volumes Profile BuildCellPassProfile. Unexpected error pos 2. I=' + IntToStr(I)+ ' err:'+ e.message, slmcError);
                  Raise;
                end;
            end;
          end;

        end; // end NVtHzIntercepts loop

      finally
        FreeAndNil(LiftBuildSettings);
        SIGLogMessage.Publish(nil, 'Summary Volumes Profile BuildCellPassProfile. # ProcessCount= ' + IntToStr(ProcessCount), slmcDebug);
      end;

    finally
      LockTokenManager.ReleaseToken(FLockTokenName);
    end;

  finally
    HeightsGrid1.Free;
    HeightsGrid2.Free;
    if Assigned(DesignElevations) then
      FreeAndNil(DesignElevations);
  end;

  Result := True;
end;
end.

 */
