using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Performs profiling operations across the grid of cells that describe th eproduction data held in datamodel
  /// </summary>
  public class ProductionDataProfiler
  {
    private IServerSubGridTree SubGridTree;

    private GridDataType ProfileTypeRequired;

    // USED ==> public TICProfileCellList ProfileCells;
    private ISiteModel SiteModel;

//    private bool PopulationControl_AnySet;
    public bool Aborted;
    public double GridDistanceBetweenProfilePoints;
    public FilteredValuePopulationControl PopulationControl;

    public CellPassFastEventLookerUpper CellPassFastEventLookerUpper { get; set; }

    public ProductionDataProfiler(ISiteModel siteModel,
      IServerSubGridTree subGridTree,
    GridDataType profileTypeRequired,
    FilteredValuePopulationControl populationControl)
    {
      SiteModel = siteModel;
      SubGridTree = subGridTree;
      ProfileTypeRequired = profileTypeRequired;
      PopulationControl = populationControl;
    }

    private bool ReadCellPassIntoTempList;

    // FLastGetTargetValues_MachineID : TICMachineID; <== Should not be needed due to Ignite based lock free implementation

    private ProductionEventLists MachineTargetValues; // : TICSiteModelMachineTargetValues;

    /// <summary>
    /// Aborts the current profiling computation
    /// </summary>
    public void Abort() => Aborted = true;

    /// <summary>
    /// Obtains a references to the collection of event lists in the currnet sitemodel belonging to
    /// a designated machine within the sitemodel
    /// </summary>
    /// <param name="forMachineID"></param>
    /// <returns></returns>
    private ProductionEventLists GetTargetValues(short forMachineID) //TICSiteModelMachineTargetValues;
    {
      /*
        //if Debug_ExtremeLogSwitchH then
        //  SIGLogMessage.PublishNoODS(Nil, Format('In GetTargetValues', []), slmcDebug);

        if ForMachineID = FLastGetTargetValues_MachineID then
          begin
            Result := FMachineTargetValues;
            Exit;
          end;

        // Locate the machine target values of the machine we want to lock
        FMachineTargetValues := FSiteModel.MachinesTargetValues.LocateByMachineID(ForMachineID);
        Result := FMachineTargetValues;

        // If necessary, acquire the interlock on this set of machine target values
        if FPopulationControl_AnySet and Assigned(FMachineTargetValues) then
          FLastGetTargetValues_MachineID := ForMachineID;
        else
          FLastGetTargetValues_MachineID := -1;

        //if Debug_ExtremeLogSwitchH then
          SIGLogMessage.PublishNoODS(Nil, Format('Out GetTargetValues', []), slmcDebug);
      */

      // Note: The commment out implementation above is entirely concerned with l;ocking semantics around the 
      // machine events for the site model in question. TRex provides a no-lock metaphor that means these accesses
      // don't require this logic
      return SiteModel.Machines[forMachineID].TargetValueChanges;
    }

    /*
      TempPasses : TICFilteredPassDataArray;
      TempPassesSize, TempPassesSizeDiv2 : Integer;
      TempPassCount : Integer;
      TempPass : TICFilteredPassData;
      TempFilteredPassFlags : Array of Boolean;

    //  public
      function BuildCellPassProfile(const CellFilter : TICGridDataCellSelectionFilter;
                                    const GridDataCache : TICDataStoreCache;
                                    const NEECoords     : TCSConversionCoordinates;
                                    const Design : TVLPDDesignDescriptor) : Boolean;

      function BuildLiftProfileFromInitialLayer(const PassFilter       : TICGridDataPassFilter;
                                                const CellFilter : TICGridDataCellSelectionFilter;
                                                const LiftBuildSettings: TICLiftBuildSettings;
                                                const GridDataCache    : TICDataStoreCache;
                                                CellPassIterator       : TSubGridSegmentCellPassIterator): Boolean;

      function BuildLiftsForCell(const CallerID : TCallerIDs;
                                 const Cell: TICProfileCell;
                                 const ReturnPasses: Boolean;
                                 const LiftBuildSettings: TICLiftBuildSettings;
                                 const ClientGrid : TICSubGridTreeLeafSubGridBase;
                                 const AssignmentContext : TICSubGridFilteredValueAssignmentContext;
                                 CellPassIterator : TSubGridSegmentCellPassIterator;
                                 const ReturnIndividualFilteredValueSelection : Boolean;
                                 const PassFilter : TICGridDataPassFilter;
                                 var FilteredPassCountOfTopMostLayer : Integer;
                                 var FilteredHalfCellPassCountOfTopMostLayer : Integer): Boolean;
    */

  }
}



/*
var
  MaxCellPassListAggregationSize : Integer = 0;

{ TICServerProfiler }

function TICServerProfiler.BuildLiftProfileFromInitialLayer(const PassFilter        : TICGridDataPassFilter;
                                                            const CellFilter        : TICGridDataCellSelectionFilter;
                                                            const LiftBuildSettings : TICLiftBuildSettings;
                                                            const GridDataCache     : TICDataStoreCache;
                                                            CellPassIterator        : TSubGridSegmentCellPassIterator): Boolean;
var
  I                       : Integer;
  TopMostLayerPassCount   : Integer;
  TopMostLayerCompactionHalfPassCount : Integer;
  ProfileCell             : TICProfileCell;
  CurrentSubgridOrigin    : TSubgridCellAddress;
  ThisSubgridOrigin       : TSubgridCellAddress;
  SubGrid                 : TSubGridTreeSubGridFunctionalBase;
  SubGridAsLeaf           : TICServerSubGridTreeLeaf;
  FLockToken              : Integer;
  FLockTokenName          : String;
  CompositeHeightsGrid    : TICClientSubGridTreeLeaf_CompositeHeights;
  FilteredGroundSurfaces  : TICGroundSurfaceDetailsList;
  FilterMask              : TSubGridTreeLeafBitmapSubGridBits;
  FilterDesignElevations  : TICClientSubGridTreeLeaf_Height;
  FilterDesignErrorCode   : TDesignProfilerRequestResult;
  IgnoreSubgrid           : Boolean;

  Procedure GetMaterialTemperatureWarningLevelsTarget(const MachineID : Int64;
                                                         const Time      : TICPassTime;
                                                         var MinWarning  : TICMaterialTemperature;
                                                         var MaxWarning  : TICMaterialTemperature);
  begin
    with FSiteModel.MachinesTargetValues do
      begin
        LocateTempWarningLevelMinValueAtDate(GetTargetValues(MachineID), Time, MinWarning);
        LocateTempWarningLevelMaxValueAtDate(GetTargetValues(MachineID), Time, MaxWarning);
      end;
  end;

  Function GetTargetCCV(const MachineID : Int64;
                        const Time : TICPassTime) : TICCCVValue;
  begin
    FSiteModel.MachinesTargetValues.LocateTargetCCVValueAtDate(GetTargetValues(MachineID), Time, Result);
  end;

  Function GetTargetMDP(const MachineID : Int64;
                        const Time : TICPassTime) : TICMDPValue;
  begin
    FSiteModel.MachinesTargetValues.LocateTargetMDPValueAtDate(GetTargetValues(MachineID), Time, Result);
  end;

  Function GetTargetCCA(const MachineID : Int64; const Time : TICPassTime) : TICCCAMinPassesValue;
  begin
    FSiteModel.MachinesTargetValues.LocateTargetCCAValueAtDate(GetTargetValues(MachineID), Time, Result);
  end;


  Function GetTargetPassCount(const MachineID : Int64;
                              const Time : TICPassTime) : TICPassCountValue;
  begin
    FSiteModel.MachinesTargetValues.LocateTargetPassCountValueAtDate(GetTargetValues(MachineID), Time, Result);
  end;

  procedure CalculateSummaryCellAttributeData;
  var
    I,PassIndex                 :Integer;
    DataStillRequiredForCCV     :Boolean;
    DataStillRequiredForMDP     :Boolean;
    DataStillRequiredForCCA     :Boolean;
    DataStillRequiredForTMP     :Boolean;
    HaveCompositeSurfaceForCell :Boolean;
    CellX, CellY                :Integer;
    PassCountTargetRange        :TTargetPassCountRange;
    TempPassCountTarget         :TICPassCountValue;
    PassSearchIdx               :LongInt;
  begin
// RCE 36155    try
      with ProfileCell, LiftBuildSettings do
        begin
          CellCCV := kICNullCCVValue;
          CellTargetCCV := kICNullCCVValue;

          CellMDP := kICNullMDPValue;
          CellTargetMDP := kICNullMDPValue;

          CellCCA := kICNullCCA;
          CellTargetCCA := kICNullCCATarget;

          CellMaterialTemperature := kICNullMaterialTempValue;
          CellMaterialTemperatureWarnMin := kICNullMaterialTempValue;
          CellMaterialTemperatureWarnMax := kICNullMaterialTempValue;

          CellPreviousMeasuredCCV := kICNullCCVValue;
          CellPreviousMeasuredTargetCCV := kICNullCCVValue;

          CellTopLayerThickness := NullSingle;

          TopLayerPassCount := 0;
          PassCountTargetRange.SetMinMax(0, 0);

          CellMaxSpeed := 0;
          CellMinSpeed := kICNullMachineSpeed;

          TopLayerPassCountTargetRangeMin := TopLayerPassCount;
          TopLayerPassCountTargetRangeMax := TopLayerPassCount;

          // WorkOut Speed Min Max
          if Layers.Count > 0 then
            for I := Layers.Count - 1 downto 0 do
              with ProfileCell.Layers[i] do
                begin
                  if FilteredPassCount > 0 then
                  begin
                    if TICLayerStatus.iclsSuperseded in Status then
                      Continue;
                     for PassIndex := StartCellPassIdx to EndCellPassIdx do
                       begin
                         with Passes.FilteredPassData[PassIndex].FilteredPass do
                           begin
                             if (MachineSpeed < CellMinSpeed) then
                               CellMinSpeed := MachineSpeed;
                             if (MachineSpeed > CellMaxSpeed) then
                               CellMaxSpeed := MachineSpeed;
                           end;

                       end;
                  end;
                end;

          if Layers.Count > 0 then
            for I := Layers.Count - 1 downto 0 do
              with ProfileCell.Layers[i] do
               if FilteredPassCount > 0 then
                begin
                  if TICLayerStatus.iclsSuperseded in Status then
                    Continue;

                //  TopLayerPassCount := FilteredPassCount;
                  TopLayerPassCount := FilteredHalfPassCount div 2;

                  if OverrideTargetPassCount then
                    begin
                      TopLayerPassCountTargetRangeMin := OverridingTargetPassCountRange.Min;
                      TopLayerPassCountTargetRangeMax := OverridingTargetPassCountRange.Max;
                    end
                  else
                    if TargetPassCount = 0 then
                      begin
                        with Passes.FilteredPassData[EndCellPassIdx].FilteredPass do
                          begin
                            TempPassCountTarget := GetTargetPassCount(MachineID, Time);
                            PassCountTargetRange.SetMinMax(TempPassCountTarget, TempPassCountTarget);
                            TopLayerPassCountTargetRangeMin := PassCountTargetRange.Min;
                            TopLayerPassCountTargetRangeMax := PassCountTargetRange.Max;
                          end;
                      end
                    else
                      begin
                        PassCountTargetRange.SetMinMax(TargetPassCount, TargetPassCount);
                        TopLayerPassCountTargetRangeMin := PassCountTargetRange.Min;
                        TopLayerPassCountTargetRangeMax := PassCountTargetRange.Max;
                      end;

                  Break; // we have top layer
                end;

          DataStillRequiredForCCV := AttributeExistenceFlags.HasCCVData;
          DataStillRequiredForMDP := AttributeExistenceFlags.HasMDPData;
          DataStillRequiredForCCA := AttributeExistenceFlags.HasCCAData;
          DataStillRequiredForTMP := AttributeExistenceFlags.HasTemperatureData;

          for I := Layers.Count - 1 downto 0 do
            with Layers[i] do
             if FilteredPassCount > 0 then
              begin

                if (TICLayerStatus.iclsSuperseded in Status) and not LiftBuildSettings.IncludeSuperseded then
                  Continue;

                if DataStillRequiredForCCV then
                  if CellCCV = kICNullCCVValue then
                    if CCV <> kICNullCCVValue then
                      begin
                        CellCCV := CCV;
                        CellCCVElev := CCV_Elev;

                        PassSearchIdx := CCV_CellPassIdx-1;
                        while PassSearchIdx >= 0  do
                        begin
                          if (LiftBuildSettings.CCVSummarizeTopLayerOnly) and ((PassSearchIdx<StartCellPassIdx) or (PassSearchIdx>EndCellPassIdx)) then
                            break;
                          if (not Layers.IsCellPassInSupersededLayer(PassSearchIdx) or LiftBuildSettings.IncludeSuperseded) then
                          begin
                            CellPreviousMeasuredCCV := ProfileCell.Passes.FilteredPassData[PassSearchIdx].FilteredPass.CCV;
                            if OverrideMachineCCV then
                              CellPreviousMeasuredTargetCCV := OverridingMachineCCV
                            else
                              CellPreviousMeasuredTargetCCV := ProfileCell.Passes.FilteredPassData[PassSearchIdx].TargetValues.TargetCCV;
                            break;
                          end;
                          Dec(PassSearchIdx);
                        end;

                        if OverrideMachineCCV then
                          CellTargetCCV := OverridingMachineCCV
                        else
                          if TargetCCV = kICNullCCVValue then
                            CellTargetCCV := GetTargetCCV(CCV_MachineID, CCV_Time)
                          else
                            CellTargetCCV := TargetCCV;
                        DataStillRequiredForCCV := False;
                      end;

                if DataStillRequiredForMDP then
                  if CellMDP = kICNullMDPValue then
                    if MDP <> kICNullMDPValue then
                      begin
                        CellMDP := MDP;
                        CellMDPElev := MDP_Elev;
                        if OverrideMachineMDP then
                          CellTargetMDP := OverridingMachineMDP
                        else
                          if TargetMDP = kICNullMDPValue then
                            CellTargetMDP := GetTargetMDP(MDP_MachineID, MDP_Time)
                          else
                            CellTargetMDP := TargetMDP;
                        DataStillRequiredForMDP := False;
                      end;

                if DataStillRequiredForCCA then
                  if CellCCA = kICNullCCA then
                    if CCA <> kICNullCCA then
                      begin
                        CellCCA := CCA;
                        CellCCAElev := CCA_Elev;
                        if TargetCCA = kICNullCCATarget then
                          CellTargetCCA := GetTargetCCA(CCA_MachineID, CCA_Time)
                        else
                          CellTargetCCA := TargetCCA;
                        DataStillRequiredForCCA := False;
                      end;


                if DataStillRequiredForTMP then
                  if CellMaterialTemperature = kICNullMaterialTempValue then
                    if MaterialTemperature <> kICNullMaterialTempValue then
                      begin
                        CellMaterialTemperature := MaterialTemperature;
                        CellMaterialTemperatureElev := MaterialTemperature_Elev;

                        if OverrideTemperatureWarningLevels then
                          begin
                            CellMaterialTemperatureWarnMin := OverridingTemperatureWarningLevels.Min;
                            CellMaterialTemperatureWarnMax := OverridingTemperatureWarningLevels.Max;
                          end
                        else
                          if (CellMaterialTemperatureWarnMin = kICNullMaterialTempValue) and
                             (CellMaterialTemperatureWarnMax = kICNullMaterialTempValue) then
                            GetMaterialTemperatureWarningLevelsTarget(MaterialTemperature_MachineID, MaterialTemperature_Time,
                                                                         CellMaterialTemperatureWarnMin, CellMaterialTemperatureWarnMax)
                          else
                            begin
                              // Currently no tracking of temperature min/max warnings on a per layer basis.
                            end;

                        DataStillRequiredForTMP := False;
                      end;

                if not DataStillRequiredForCCV and not DataStillRequiredForMDP and not DataStillRequiredForCCA and not DataStillRequiredForTMP then
                  Break;

                // CCA not part of legacy setup as yet
                if CCVSummarizeTopLayerOnly then
                  DataStillRequiredForCCV := False;
                if MDPSummarizeTopLayerOnly then
                  DataStillRequiredForMDP := False;

                DataStillRequiredForTMP := False; // last pass only

              end;

            for I := Layers.Count - 1 downto 0 do
              with Layers[i] do
                if FilteredPassCount > 0 then
                  begin
                    if TICLayerStatus.iclsSuperseded in Status then
                      Continue;

                    if Thickness <> NullSingle then
                      begin
                        CellTopLayerThickness := Thickness;
                        Break;
                      end;
                  end;

             ProfileCell.SetFirstLastHighestLowestElevations(PassFilter.HasElevationTypeFilter,PassFilter.ElevationType);

             // are coords set right?
             CellX := ProfileCell.OTGCellX AND kSubGridLocalKeyMask;
             CellY := ProfileCell.OTGCellY AND kSubGridLocalKeyMask;
             HaveCompositeSurfaceForCell := CompositeHeightsGrid.ProdDataMap.BitSet(CellX,CellY);

             if HaveCompositeSurfaceForCell then
               begin
                 with CompositeHeightsGrid.Cells[CellX, CellY] do
                   begin
                     if (LastHeightTime = 0) or ((ProfileCell.Passes.PassCount > 0) and (ProfileCell.Passes.LastPassTime > LastHeightTime)) then
                       ProfileCell.CellLastCompositeElev := ProfileCell.CellLastElev
                     else
                       ProfileCell.CellLastCompositeElev := LastHeight;

                     if (LowestHeightTime = 0) or ((ProfileCell.Passes.PassCount > 0) and (ProfileCell.Passes.LowestPassTime > LowestHeightTime)) then
                       ProfileCell.CellLowestCompositeElev := ProfileCell.CellLowestElev
                     else
                       ProfileCell.CellLowestCompositeElev := LowestHeight;

                     if (HighestHeightTime = 0) or ((ProfileCell.Passes.PassCount > 0) and (ProfileCell.Passes.HighestPassTime > HighestHeightTime)) then
                       ProfileCell.CellHighestCompositeElev := ProfileCell.CellHighestElev
                     else
                       ProfileCell.CellHighestCompositeElev := HighestHeight;

                     if (FirstHeightTime = 0) or ((ProfileCell.Passes.PassCount > 0) and (ProfileCell.Passes.FirstPassTime > FirstHeightTime)) then
                       ProfileCell.CellFirstCompositeElev := ProfileCell.CellFirstElev
                     else
                       ProfileCell.CellFirstCompositeElev := FirstHeight;
                   end;
               end
             else
               begin
                 ProfileCell.CellLastCompositeElev := ProfileCell.CellLastElev;
                 ProfileCell.CellLowestCompositeElev := ProfileCell.CellLowestElev;
                 ProfileCell.CellHighestCompositeElev := ProfileCell.CellHighestElev;
                 ProfileCell.CellFirstCompositeElev := ProfileCell.CellFirstElev;
               end;

        end;
  end;


  Procedure ConstructSubgridSpatialAndPositionalMask(var Mask : TSubGridTreeLeafBitmapSubGridBits);
  var
    CellIdx : Integer;
    CellX, CellY : Integer;
    CellCenterX, CellCenterY: Double;
    ThisSubgridOrigin : TSubgridCellAddress;

  begin
    Mask.Clear;

    with CellFilter, FSubGridTree do
      for CellIdx := I to FProfileCells.Count - 1 do // from current position to end
        begin

          ThisSubgridOrigin := TSubgridCellAddress.CreateSimple(FProfileCells[CellIdx].OTGCellX SHR kSubGridIndexBitsPerLevel,
                                                                FProfileCells[CellIdx].OTGCellY SHR kSubGridIndexBitsPerLevel);

          if CurrentSubgridOrigin.IsSameAs(ThisSubgridOrigin) then
            begin
              CompositeHeightsGrid.GetSubGridCellIndex(FProfileCells[CellIdx].OTGCellX, FProfileCells[CellIdx].OTGCellY, CellX, CellY);
              if FilterSelections <> [] then
                begin
                  GetCellCenterPosition(FProfileCells[CellIdx].OTGCellX, FProfileCells[CellIdx].OTGCellY, CellCenterX, CellCenterY);
                  if IsCellInSelection(CellCenterX, CellCenterY) then
                    Mask.SetBit(CellX, CellY);
                end
              else
                Mask.SetBit(CellX, CellY);
            end
          else
            Break;
        end;
  end;

  Function ConstructSubgridCellFilterMask(var Mask : TSubGridTreeLeafBitmapSubGridBits) : Boolean;
  var
    OriginX, OriginY  : Double;
    DesignMask        : TSubGridTreeLeafBitmapSubGridBits;
    DesignFilterMask  : TSubGridTreeLeafBitmapSubGridBits;
    RequestResult     : TDesignProfilerRequestResult;

  begin

    Result := True;

    ConstructSubgridSpatialAndPositionalMask(Mask);

    // If the filter contains a design mask filter then compute this and AND it with the
    // mask calculated in the step above to derive the final required filter mask

    with CellFilter do
    begin

      if HasAlignmentDesignMask then
        begin
          // Query the design profiler service for the corresponding filter mask given the
          // alignment design configured in the cell selection filter.

          CompositeHeightsGrid.CalculateWorldOrigin(OriginX, OriginY);
          with DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService do
            begin
              RequestResult := RequestDesignMaskFilterPatch(Construct_ComputeDesignFilterPatch_Args(FSiteModel.ID,
                                                                                                    OriginX, OriginY,
                                                                                                    FSiteModel.Grid.CellSize,
                                                                                                    ReferenceDesign,
                                                                                                    Mask,
                                                                                                    StartStation, EndStation,
                                                                                                    LeftOffset, RightOffset),
                                                                                                    DesignMask);

              if RequestResult = dppiOK then
                Mask := Mask AND DesignMask
              else
                begin
                  Result := False;
                  SIGLogMessage.PublishNoODS(Nil, Format('Call(B1) to RequestDesignMaskFilterPatch in TICServerProfiler returned error result %s for %s.',
                                                         [DesignProfilerErrorStatusName(RequestResult), CellFilter.ReferenceDesign.ToString]), slmcError);
                end;
            end;
        end;

        if HasDesignFilter then
        begin
          // Query the design profiler service for the corresponding filter mask given the
          // tin design configured in the cell selection filter.

          with DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService do
            begin
              RequestResult := RequestDesignMaskFilterPatch(Construct_ComputeDesignFilterPatch_Args(FSiteModel.ID,
                                                                                                    OriginX, OriginY,
                                                                                                    FSiteModel.Grid.CellSize,
                                                                                                    DesignFilter,
                                                                                                    Mask,
                                                                                                    StartStation, EndStation,
                                                                                                    LeftOffset, RightOffset),
                                                            DesignFilterMask);

              if RequestResult = dppiOK then
                Mask := Mask AND DesignFilterMask
              else
                begin
                  Result := False;
                  SIGLogMessage.PublishNoODS(Nil, Format('Call (B2) to RequestDesignMaskFilterPatch in TICServerProfiler returned error result %s for %s.',
                                                         [DesignProfilerErrorStatusName(RequestResult), CellFilter.DesignFilter.ToString]), slmcError);
                end;
            end;
        end;
    end;
  end;


  Function InitialiseFilterContext : Boolean;
  begin
    Result := True;

    if PassFilter.HasElevationRangeFilter then
      begin
        // If the elevation range filter uses a design then the design elevations
        // for the subgrid need to be calculated and supplied to the filter

        if not PassFilter.ElevationRangeDesign.IsNull then
          begin
            // Query the DesignProfiler service to get the patch of elevations calculated
            FilterDesignErrorCode := PSNodeImplInstance.DesignProfilerService.RequestDesignElevationPatch
                         (Construct_CalculateDesignElevationPatch_Args(FSiteModel.ID,
                                                                       ProfileCell.OTGCellX, ProfileCell.OTGCellY,
                                                                       FSubGridTree.CellSize,
                                                                       PassFilter.ElevationRangeDesign,
                                                                       TSubGridTreeLeafBitmapSubGridBits.FullMask),
                         FilterDesignElevations);

            if (FilterDesignErrorCode <> dppiOK) or not Assigned(FilterDesignElevations) then
              begin
                if FilterDesignErrorCode = dppiNoElevationsInRequestedPatch then
                  SIGLogMessage.PublishNoODS(nil, 'Lift filter by design. Call to RequestDesignElevationPatch failed due to no elevations in requested patch.', slmcMessage)
                else
                  SIGLogMessage.PublishNoODS(nil, Format('Lift filter by design. Call to RequestDesignElevationPatch failed due to no TDesignProfilerRequestResult return code %d.',[Integer(FilterDesignErrorCode)]), slmcWarning);
                Result := False;
                Exit;
              end;
          end;

        PassFilter.InitialiseElevationRangeFilter(FilterDesignElevations)
      end;
  end;

begin

  FPopulationControl_AnySet := FPopulationControl.AnySet;

  {$IFDEF DEBUG}
  SIGLogMessage.PublishNoODS(Self, Format('BuildLiftProfileFromInitialLayer: Processing %d cells', [FProfileCells.Count]), slmcDebug);
  {$ENDIF}

  CurrentSubgridOrigin := TSubgridCellAddress.CreateSimple(MaxInt, MaxInt);
  SubGrid := Nil;
  SubGridAsLeaf := Nil;
  ProfileCell := nil;
  FilteredGroundSurfaces := Nil;
  FilterDesignElevations := nil;
  IgnoreSubgrid := False;

  FLockTokenName := kProfilerLockToken + IntToStr(GetCurrentThreadID);
  FLockToken := LockTokenManager.AcquireToken(FLockTokenName);

  if not FSiteModel.GroundSurfacesLoaded then
    FSiteModel.ReadGroundSurfacesFromDataModel;

  FSiteModel.GroundSurfaces.AcquireReadAccessInterlock;
  try
    if FSiteModel.GroundSurfacesLoaded and (FSiteModel.GroundSurfaces.Count > 0) then
      begin
        FilteredGroundSurfaces := TICGroundSurfaceDetailsList.Create;
        // Filter out any ground surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
        FSiteModel.GroundSurfaces.FilterGroundSurfaceDetails(PassFilter.HasTimeFilter, PassFilter.StartTime, PassFilter.EndTime,PassFilter.ExcludeSurveyedSurfaces, FilteredGroundSurfaces,PassFilter.SurveyedSurfaceExclusionList);
      end;
  finally
    FSiteModel.GroundSurfaces.ReleaseReadAccessInterlock;
  end;

  if Assigned(FilteredGroundSurfaces) and (FilteredGroundSurfaces.Count = 0) then
    FreeAndNil(FilteredGroundSurfaces);


  CompositeHeightsGrid := TICClientSubGridTreeLeaf_CompositeHeights.Create(FSubGridTree, nil, kSubGridTreeLevels, FSubGridTree.CellSize, TICClientSubGridTreeLeaf_CompositeHeights.DefaultIndexOriginOffset);

  try
    try
      for I := 0 to FProfileCells.Count - 1 do
        begin
          ProfileCell := FProfileCells[I];


          // get subgrid setup iterator and set cell address

          // get sugbrid origin for cell address
   //       ThisSubgridOrigin := TSubgridCellAddress.CreateSimple(FProfileCells[I].OTGCellX SHR kSubGridIndexBitsPerLevel,
     //                                                           FProfileCells[I].OTGCellY SHR kSubGridIndexBitsPerLevel);
          ThisSubgridOrigin := TSubgridCellAddress.CreateSimple(ProfileCell.OTGCellX SHR kSubGridIndexBitsPerLevel,
                                                                ProfileCell.OTGCellY SHR kSubGridIndexBitsPerLevel);

          if not CurrentSubgridOrigin.IsSameAs(ThisSubgridOrigin) then // if we have a new subgrid to fetch
            begin
              IgnoreSubgrid := False;
              CurrentSubgridOrigin := ThisSubgridOrigin;

              // release previous subgrid
              if Assigned(SubGrid) and Subgrid.Locked and (SubGrid.LockToken = FLockToken) then
               SubGrid.ReleaseLock(FLockToken);

              SubGrid := nil;
              CompositeHeightsGrid.Clear;

                  // Does the subgridtree contain this node in it's existance map?
              if FPDExistenceMap.Cells[CurrentSubgridOrigin.X, CurrentSubgridOrigin.Y] then
                SubGrid := LocateSubGridContaining(FSubGridTree,
                                                   GridDataCache,
                                                   ProfileCell.OTGCellX, ProfileCell.OTGCellY,
                                                   FSubGridTree.NumLevels,
                                                   FLockToken, False, False);
              if Assigned(SubGrid) then
                begin
                  SubGridAsLeaf := SubGrid as TICServerSubGridTreeLeaf;
                  CellPassIterator.SegmentIterator.SubGrid := SubGridAsLeaf;
                  CellPassIterator.SegmentIterator.Directory := SubGridAsLeaf.Directory;
                end;

              CompositeHeightsGrid.SetAbsoluteOriginPosition(ProfileCell.OTGCellX and not kSubGridLocalKeyMask, ProfileCell.OTGCellY and not kSubGridLocalKeyMask);
              CompositeHeightsGrid.SetAbsoluteLevel(FSubGridTree.NumLevels);

              if not ConstructSubgridCellFilterMask(FilterMask) then
                Continue;

              if Assigned(FilteredGroundSurfaces) then
                if DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService.RequestSurveyedSurfacesProfilePatch
                  (Construct_CalculateSurveyedSurfacesProfilePatch_Args(FSiteModel.ID,
                                                                        ProfileCell.OTGCellX, ProfileCell.OTGCellY,
                                                                        FSubGridTree.CellSize, FilterMask),
                                                                        FilteredGroundSurfaces, CompositeHeightsGrid) <> dppiOK then
                begin
                  SIGLogMessage.PublishNoODS(nil, 'Call(B) to RequestSurveyedSurfacesProfilePatch in TICServerProfiler failed to return a composite profile grid.', slmcError);
                  Continue;
                end;

              if not InitialiseFilterContext then
                begin
                  if FilterDesignErrorCode = dppiNoElevationsInRequestedPatch then
                    IgnoreSubgrid := true
                  else
                    SIGLogMessage.PublishNoODS(nil, 'Call to RequestDesignElevationPatch in TICServerProfiler for filter failed to return a elevation patch.', slmcError);
                  Continue;
                end;
            end;

          if Assigned(SubGrid) and not IgnoreSubgrid then
            begin
              if Assigned(SubGridAsLeaf) then
                with ProfileCell, SubGridAsLeaf.Directory.GlobalLatestCells do
                  begin
                    AttributeExistenceFlags.HasCCVData := HasCCVData;
                    AttributeExistenceFlags.HasMDPData := HasMDPData;
                    AttributeExistenceFlags.HasCCAData := HasCCAData;
                    AttributeExistenceFlags.HasTemperatureData := HasTemperatureData;
                  end;

              // get cell address relative to subgrid and SetCellCoordinatesInSubgrid
              CellPassIterator.SetCellCoordinatesInSubgrid(FProfileCells[I].OTGCellX AND kSubGridLocalKeyMask,
                                                           FProfileCells[I].OTGCellY AND kSubGridLocalKeyMask);
              PassFilter.InitaliaseFilteringForCell(CellPassIterator.CellX, CellPassIterator.CellY);

              if BuildLiftsForCell(cidBuildLiftProfileFromInitialLayer,
                                   ProfileCell,
                                   False,
                                   LiftBuildSettings,
                                   Nil, Nil, CellPassIterator, False, PassFilter,
                                   TopMostLayerPassCount,
                                   TopMostLayerCompactionHalfPassCount) then
                begin
                  FPopulationControl_AnySet := True;
                  ProfileCell.IncludesProductionData := True;
                  CalculateSummaryCellAttributeData;
                end
              else
                begin
                  ProfileCell.ClearLayers;
                  CalculateSummaryCellAttributeData;
                end;
            end
          else
            begin
              ProfileCell.ClearLayers;
              CalculateSummaryCellAttributeData;
            end;

        end;

    finally
      if Assigned(FilteredGroundSurfaces) then
        FreeAndNil(FilteredGroundSurfaces);

      // release last subgrid
      if Assigned(SubGrid) and Subgrid.Locked and (SubGrid.LockToken = FLockToken) then
        SubGrid.ReleaseLock(FLockToken);
     end;

  finally
    CompositeHeightsGrid.Free;
    LockTokenManager.ReleaseToken(FLockTokenName);
  end;

  Result := True;
end;

function TICServerProfiler.BuildLiftsForCell(const CallerID : TCallerIDs; // allows us to identfy caller
                                             const Cell: TICProfileCell;
                                             const ReturnPasses: Boolean;
                                             const LiftBuildSettings: TICLiftBuildSettings;
                                             const ClientGrid : TICSubGridTreeLeafSubGridBase;
                                             const AssignmentContext : TICSubGridFilteredValueAssignmentContext;
                                             CellPassIterator : TSubGridSegmentCellPassIterator;
                                             const ReturnIndividualFilteredValueSelection : Boolean;
                                             const PassFilter : TICGridDataPassFilter;
                                             var FilteredPassCountOfTopMostLayer : Integer;

                                            // FilteredHalfCellPassCountOfTopMostLayer tracks 'half cell passes'.
                                            // A half cell pass is recorded whan a Quattro four drum compactor drives over the ground.
                                            // Other machines, like single drum compactors, record two half cell pass counts to form a single cell pass.
                                             var FilteredHalfCellPassCountOfTopMostLayer : Integer
                                             ): Boolean;
{$region 'Local variables'}
var
  CurrentPassIndex: Integer;
  CurrentLayer: TICProfileLayer;
  CurrentLayerRecycledIndex : Integer;

  LastCreatedLayer : TICProfileLayer;
  CurrentPass: TICFilteredPassData;

  LastPassElev: TICCellHeight;
  ElevationOfLastProcessedCellPass : TICCellHeight;
  LayerIDOfLastProcessedCellPass : TICLayerIDValue;

  LastPassCCV: TICCCVValue;
  LastPassCCVTime : TICPassTime;
  LastPassCCVMachine : TICMachineID;
  LastPassCCVElev : TICCellHeight;
  LastPassCCVIdx  : Integer;

  LastPassMDP: TICMDPValue;
  LastPassMDPTime : TICPassTime;
  LastPassMDPMachine : TICMachineID;
  LastPassMDPElev : TICCellHeight;

  LastPassCCA : TICCCAValue;
  LastPassCCATime : TICPassTime;
  LastPassCCAMachine : TICMachineID;
  LastPassCCAElev : TICCellHeight;

  LastPassMaterialTemperature: TICMaterialTemperature;
  LastPassMaterialTemperature_Time : TICPassTime;
  LastPassMaterialTemperature_Machine : TICMachineID;
  LastPassMaterialTemperature_Elev : TICCellHeight;

  LastPassRadioLatency: TICRadioLatency;
  LastPassRMV: TICRMVValue;
  LastPassFrequency: TICVibrationFrequency;
  LastPassAmplitude: TICVibrationAmplitude;
  LastPassTime: TICPassTime;
  LastPassMachineSpeed : TICMachineSpeed;

  LastPassInPreviousLayerElev: TICCellHeight;
  FirstPassInPreviousLayerElev: TICCellHeight;

  CurrentLayerMaxElev: TICCellHeight;
  CurrentLayerMinElev: TICCellHeight;
  PassCountForLayer: Integer; // What for?
  FilteredPassCountForLayer: Integer;
  FilteredHalfPassCountForLayer: Integer;

  NewLayer: Boolean;
  ValidPassesExist: Boolean;
  CurrentLayerNotAddedToCell: Boolean;

  LastMRDate: TICPassTime;
  LastMRDesignNameID: TICDesignNameID;

  CompactionSummaryInLiftBuildSettings : Boolean;
  WorkInProgressSummaryInLiftBuildSettings : Boolean;
  ThicknessInProgressInLiftBuildSettings : Boolean;

  FirstCellPassIndex,
  LastCellPassIndex,
  PassIndexDirectionIncrement,
  FinishCellPassIndex : Integer;

  FilteredPassIndex : Integer;
  I: Integer;

  FComputedSupercededStatusForLayers : Boolean;
  NumCellPassesRemainingToFetch : Integer;

  Pass : TICCellPassValue;
  FilteredValuePopulationComplete : Boolean;
  FilterAppliedToCellPasses : Boolean;

  LastLayerID: TICLayerIDValue;
  LayerContainsAFilteredPass : Boolean;

  MainValueAquired : Boolean;

{$endregion}

{$region 'Nested methods'}

  //============================================================================
  procedure InitLastValueVars;
  begin
    LastPassCCV := kICNullCCVValue;
    LastPassMDP := kICNullMDPValue;
    LastPassCCA := kICNullCCA;
    LastPassRMV := kICNullRMVValue;
    LastPassRadioLatency := kICNullRadioLatency;
    LastPassFrequency := kICNullFrequencyValue;
    LastPassAmplitude := kICNullAmplitudeValue;
    LastPassMaterialTemperature := kICNullMaterialTempValue;
    LastPassMachineSpeed := kICNullMachineSpeed;
    LastPassCCVIdx := 0;
  end;

  procedure InitLayerTrackingVars;
  begin
    LastPassInPreviousLayerElev := kICNullHeight;
    FirstPassInPreviousLayerElev := kICNullHeight;
    NewLayer            := True;
    ValidPassesExist    := False;
    PassCountForLayer   := 0;
    FilteredPassCountForLayer := 0;
    FilteredHalfPassCountForLayer := 0;
    CurrentLayerNotAddedToCell := False;
    CurrentLayerMaxElev := 0;
    CurrentLayerMinElev := 1E100;
    CurrentLayer        := Nil;
    CurrentLayerRecycledIndex := -1;
    LastCreatedLayer    := Nil;
    LastMRDesignNameID  := kNoDesignNameID;
    LastMRDate          := 0;
    LastLayerID         := kICNullLayerIDValue;
    ElevationOfLastProcessedCellPass := kICNullHeight;
  end;

  procedure AddFinishedLayer;
  begin
    with CurrentLayer do
      begin
        Height            := LastPassElev;
        Thickness         := LastPassElev - LastPassInPreviousLayerElev;

        CCV               := LastPassCCV;
        CCV_Time          := LastPassCCVTime;
        CCV_MachineID     := LastPassCCVMachine;
        CCV_Elev          := LastPassCCVElev;
        CCV_CellPassIdx   := LastPassCCVIdx;

        RMV               := LastPassRMV;
        Frequency         := LastPassFrequency;
        Amplitude         := LastPassAmplitude;

        RadioLatency      := LastPassRadioLatency;
        LastLayerPassTime := LastPassTime;

        MDP               := LastPassMDP;
        MDP_Time          := LastPassMDPTime;
        MDP_MachineID     := LastPassMDPMachine;
        MDP_Elev          := LastPassMDPElev;

        CCA               := LastPassCCA;
        CCA_Time          := LastPassCCATime;
        CCA_MachineID     := LastPassCCAMachine;
        CCA_Elev          := LastPassCCAElev;

        MaterialTemperature           := LastPassMaterialTemperature;
        MaterialTemperature_Time      := LastPassMaterialTemperature_Time;
        MaterialTemperature_MachineID := LastPassMaterialTemperature_Machine;
        MaterialTemperature_Elev      := LastPassMaterialTemperature_Elev;

        Status            := [];
        MaxThickness      := CurrentLayerMaxElev - LastPassInPreviousLayerElev;

        FilteredPassCount := FilteredPassCountForLayer;
        FilteredHalfPassCount := FilteredHalfPassCountForLayer;
      end;

    Cell.AddLayer(CurrentLayer, CurrentLayerRecycledIndex);
    LastCreatedLayer := CurrentLayer;

    CurrentLayer := Nil;
    CurrentLayerRecycledIndex := -1;

    NewLayer := True;
    PassCountForLayer := 0;
    FilteredPassCountForLayer := 0;
    FilteredHalfPassCountForLayer := 0;
    CurrentLayerMaxElev := 0;
    CurrentLayerMinElev := 1E100;

    InitLastValueVars;

    CurrentLayerNotAddedToCell := False;
  end;

  procedure CheckProfileCellLiftCompaction(const Lift: TICProfileLayer); // ajr15167
  begin
    if (Lift.CCV = kICNullCCVValue) and (Lift.MDP = kICNullMDPValue) and (Lift.CCA = kICNullCCA) then
      Exit;

    Cell.CheckLiftCompaction(Lift, LiftBuildSettings, FProfileTypeRequired);
  end;

  procedure CheckProfileCellLiftThickness(const Lift: TICProfileLayer);
  var
    TargetThickness :TICLiftThickness;
    ThicknessToTest :TICLiftThickness;
  begin
    if LiftBuildSettings.OverrideMachineThickness then
      TargetThickness := LiftBuildSettings.OverridingLiftThickness
    else
      begin
        if Lift.PassCount > 0 then
          TargetThickness := Cell.TopPassTargetThicknessByCompactor(Lift)
        else
          TargetThickness := kDefaultTargetLiftThickness;
      end;

    case LiftBuildSettings.LiftThicknessType of
      lttCompacted    : ThicknessToTest := Lift.Thickness;
      lttUncompacted  : ThicknessToTest := Lift.MaxThickness;
    else
      ThicknessToTest := 0;
      Assert(False, 'Unknown LiftBuildSettings.LiftThicknessType'); {SKIP}
    end;

    if (TargetThickness <> kICNullOverridingTargetLiftThicknessValue) and (ThicknessToTest > TargetThickness) then
      Include(Lift.Status, iclsTooThick);
  end;

  function FirstCellInLayerProcessing : Boolean;
  begin
    if Assigned(CellPassIterator) then
      Result := CurrentPassIndex = 0
    else
      Result := CurrentPassIndex = FirstCellPassIndex;
  end;

  function IsStillInCurrentLift(const Pass: TICFilteredPassData): Boolean;
  var
    TargetThickness: TICLiftThickness;
  begin
    Result := False;

    if FirstCellInLayerProcessing then
      begin
        Result := True;
        Exit;
      end;

    with LiftBuildSettings do
      begin
        if LiftDetectionType in [icldtMapReset, icldtAutoMapReset] then
          with Pass.EventValues do
            if ((MapReset_PriorDate <> 0) and
                (MapReset_PriorDate > LastMRDate)) or (LayerIDOfLastProcessedCellPass <> LayerID) then
              Exit
            else
              if LiftDetectionType = icldtMapReset then
                begin
                  Result := True;
                  Exit;
                end;

        // If we haven't hit an explicit Map Reset, optionally check for an auto detected lift
        if LiftDetectionType in [icldtAutomatic, icldtAutoMapReset] then
          with Pass.FilteredPass do
            begin
              // We'll force LowerDeadBandBoundary to always be subtracted from the ElevationOfLastProcessedCellPass.
              Result := InRange(Height,
                                ElevationOfLastProcessedCellPass - Abs(DeadBandLowerBoundary),
                                ElevationOfLastProcessedCellPass + DeadBandUpperBoundary);

              if Result then
                Exit;

              // If we're not in the deadband, then check for a partial lift
              if Height > ElevationOfLastProcessedCellPass then
                begin
                  if OverrideMachineThickness then
                    TargetThickness := OverridingLiftThickness
                  else
                    TargetThickness := Pass.TargetValues.TargetThickness;

                  if TargetThickness <> kICNullOverridingTargetLiftThicknessValue then
                    if (Height > LastPassInPreviousLayerElev) and (Height - LastPassInPreviousLayerElev < TargetThickness) then
                      Result := True;
                end;
            end;

        // If layer detction is via layerid has the layer id changed
        if LiftDetectionType = icldtTagfile then
          with Pass.EventValues do
            begin
              if (LayerID = LastLayerID) or  (LayerID = kICNullLayerIDValue) then
                begin
                  Result := True;  // still in same layer
                  Exit;
                end;
            end;
      end;
  end;

  procedure UpdateLastPassTrackingVars;
  begin
    with CurrentPass.FilteredPass do
      begin
        // If we're not on the first pass over the cell, but the passes for the current lift are empty,
        // then store the elevation of the last pass over the lift we've just finished,
        // so that we can determine the thickness of this lift once we find the last pass over it

        if not FirstCellInLayerProcessing and NewLayer then
        begin
          try
            if (ElevationOfLastProcessedCellPass = kICNullHeight) then
              LastPassInPreviousLayerElev := Height
            else if Height > ElevationOfLastProcessedCellPass then
              LastPassInPreviousLayerElev := ElevationOfLastProcessedCellPass
            else
              LastPassInPreviousLayerElev := Height;
          Except
            On E:Exception do
            begin
              if IsNAN(Height) then
                SIGLogMessage.PublishNoODS(Nil, 'Height in UpdateLastPassTrackingVars CurrentPass.FilteredPass is a Nan', slmcWarning);
              if IsNAN(ElevationOfLastProcessedCellPass) then
                SIGLogMessage.PublishNoODS(Nil, 'ElevationOfLastProcessedCellPass in UpdateLastPassTrackingVars is a Nan', slmcWarning);
              SIGLogMessage.PublishNoODS(Nil, Format('Unexpected error in ElevationOfLastProcessedCellPass. Error=%s',[e.message]), slmcError);
            end;
          end;
        end;

        LastPassTime := Time;
        LastPassElev := Height;

        ElevationOfLastProcessedCellPass := Height;

        if ElevationOfLastProcessedCellPass > CurrentLayerMaxElev then
          CurrentLayerMaxElev := ElevationOfLastProcessedCellPass;
        if ElevationOfLastProcessedCellPass < CurrentLayerMinElev then
          CurrentLayerMinElev := ElevationOfLastProcessedCellPass;

        // Don't update attribute tracking vars if the cell pass was not filtered. The elevations are tracked above
        // to permit accurate determination of the layer thickness

        if not Cell.FilteredPassFlags[CurrentPassIndex] then
          Exit;

        if CCV <> kICNullCCVValue then
          begin
            LastPassCCV := CCV;
            LastPassCCVTime := Time;
            LastPassCCVMachine := MachineID;
            LastPassCCVElev := Height;
            LastPassCCVIdx := CurrentPassIndex;
          end;

        if MDP <> kICNullMDPValue then
          begin
            LastPassMDP := MDP;
            LastPassMDPTime := Time;
            LastPassMDPMachine := MachineID;
            LastPassMDPElev := Height;
          end;

        if CCA <> kICNullCCA then
          begin
            LastPassCCA := CCA;
            LastPassCCATime := Time;
            LastPassCCAMachine := MachineID;
            LastPassCCAElev := Height;
          end;

        if MaterialTemperature <> kICNullMaterialTempValue then
          begin
            LastPassMaterialTemperature := MaterialTemperature;
            LastPassMaterialTemperature_Time := Time;
            LastPassMaterialTemperature_Machine := MachineID;
            LastPassMaterialTemperature_Elev := Height;
          end;

        if RadioLatency <> kICNullRadioLatency then
          LastPassRadioLatency := RadioLatency;
        if RMV <> kICNullRMVValue then
          LastPassRMV := RMV;
        if Frequency <> kICNullFrequencyValue then
          LastPassFrequency := Frequency;
        if Amplitude <> kICNullAmplitudeValue then
          LastPassAmplitude := Amplitude;

        if MachineSpeed <> kICNullMachineSpeed then
           LastPassMachineSpeed := MachineSpeed;
      end;

    with CurrentPass.EventValues do
    begin
      if LiftBuildSettings.LiftDetectionType in [icldtMapReset, icldtAutoMapReset] then
      begin
        if MapReset_PriorDate > LastMRDate then
          begin
            LastMRDate := MapReset_PriorDate;
            LastMRDesignNameID := MapReset_DesignNameID;
          end;
         LayerIDOfLastProcessedCellPass := CurrentPass.EventValues.LayerID;
      end;
      if LiftBuildSettings.LiftDetectionType  = icldtTagfile then
         LastLayerId := LayerId;
    end;

  end;

  Procedure ComputeSupercededStatusForLayers;
  var
    TestLayerHeight : TICCellHeight;
    J : Integer;
    LayerIndex: Integer;
  begin
    if FComputedSupercededStatusForLayers then
      Exit;

    FComputedSupercededStatusForLayers := True;

    with Cell do
      for LayerIndex := Layers.Count - 1 downto 0 do
        begin
          with Layers[LayerIndex] do
            begin
              TestLayerHeight := Height;

              if iclsSuperseded in Status then
                Continue;
            end;

          J := LayerIndex - 1;

          while (J >= 0) do
            with Layers[j] do
              if (TestLayerHeight < Height) then
                begin
                  Include(Status, iclsSuperseded);
                  Thickness := 0;

                  Dec(J);
                end
              else
                Break;
        end;
  end;

  Procedure ComputeLayerThicknessForLayers;
  var
    PreviousTopMostNonSupersededLayerPassHeight : Double;
    LayerIndex: Integer;
    Layer: TICProfileLayer;
  begin
    PreviousTopMostNonSupersededLayerPassHeight := NullReal;

    with Cell, Layers do
      for LayerIndex := 0 to Count - 1 do
        begin
          Layer := Layers[LayerIndex];

          with Layer do
            begin
              if not (iclsSuperseded in Status) then
                begin
                  if PreviousTopMostNonSupersededLayerPassHeight = NullReal then
                    PreviousTopMostNonSupersededLayerPassHeight := Owner.Passes.FilteredPassData[StartCellPassIdx].FilteredPass.Height - LiftBuildSettings.FirstPassThickness;

                  if PassCount > 0 then
                    Thickness := Height - PreviousTopMostNonSupersededLayerPassHeight
                  else
                    Thickness := 0;

                  if CompactionSummaryInLiftBuildSettings or
                     (WorkInProgressSummaryInLiftBuildSettings and (LayerIndex = Count - 1)) then
                    CheckProfileCellLiftCompaction(Layer);

                  if ThicknessInProgressInLiftBuildSettings or
                     (WorkInProgressSummaryInLiftBuildSettings and (LayerIndex = Count - 1)) then
                    CheckProfileCellLiftThickness(Layer);

                  PreviousTopMostNonSupersededLayerPassHeight := Owner.Passes.FilteredPassData[EndCellPassIdx].FilteredPass.Height;
                end;

              Thickness := Max(Thickness, 0);
            end;
        end;
  end;

  Procedure SetCellIterationParameters;
  var
    _PassCount : Integer;
  begin
    _Passcount := Cell.Passes.PassCount;

    FirstCellPassIndex := 0;
    LastCellPassIndex := _PassCount - 1;
    PassIndexDirectionIncrement := 1;
    FinishCellPassIndex := _PassCount;
  end;

  Function CheckLayerCompleted : Boolean;
  begin
    Result := False;

    // If this is the first pass over this cell to process, then we don't know the elevation prior to this first pass.
    if FirstCellInLayerProcessing then
      LastPassInPreviousLayerElev := CurrentPass.FilteredPass.Height - LiftBuildSettings.FirstPassThickness;

    if not IsStillInCurrentLift(CurrentPass) and not NewLayer and (LiftBuildSettings.LiftDetectionType <> icldtNone) then
      begin
        AddFinishedLayer;
        Result := True;
      end;

    UpdateLastPassTrackingVars;
  end;

  procedure AddValidatedPassToLayer;
  begin
    inc(PassCountForLayer);

    if FilterAppliedToCellPasses then
      begin
        if Cell.FilteredPassFlags[CurrentPassIndex] then
          begin
            Inc(FilteredPassCountForLayer);
            if CurrentPass.FilteredPass.HalfPass or (CurrentPass.MachineType in kICHalfPassCompactorMachineTypes) then
              Inc(FilteredHalfPassCountForLayer)
            else
              Inc(FilteredHalfPassCountForLayer, 2); // For rest its 2 half passes recorded
          end;
      end
    else
      begin
        Inc(FilteredPassCountForLayer);
        if CurrentPass.FilteredPass.HalfPass or (CurrentPass.MachineType in kICHalfPassCompactorMachineTypes) then
          Inc(FilteredHalfPassCountForLayer)
        else
          Inc(FilteredHalfPassCountForLayer, 2); // For rest its 2 half passes recorded
        Cell.FilteredPassFlags[CurrentPassIndex] := True;
      end;

    ValidPassesExist := True;

    if not Assigned(CurrentLayer) then
      CurrentLayer := Cell.RequestNewLayer(CurrentLayerRecycledIndex);

    // Workout max/min/first/last elevation values if a filtered pass
    if Cell.FilteredPassFlags[CurrentPassIndex] then
      begin
        if (CurrentPass.FilteredPass.Height <> kICNullHeight) then
          begin
            if (CurrentLayer.MinimumPassHeight = kICNullHeight) or (CurrentPass.FilteredPass.Height < CurrentLayer.MinimumPassHeight) then
              CurrentLayer.MinimumPassHeight := CurrentPass.FilteredPass.Height;
            if (CurrentLayer.MaximumPassHeight = kICNullHeight) or (CurrentPass.FilteredPass.Height > CurrentLayer.MaximumPassHeight) then
              CurrentLayer.MaximumPassHeight := CurrentPass.FilteredPass.Height;
            if (CurrentLayer.FirstPassHeight = kICNullHeight) then
              CurrentLayer.FirstPassHeight := CurrentPass.FilteredPass.Height;
            if (CurrentLayer.FirstPassHeight <> kICNullHeight) then
              CurrentLayer.LastPassHeight := CurrentPass.FilteredPass.Height;
          end;
      end;

    CurrentLayer.AddPass(CurrentPassIndex);

    NewLayer := False;
    CurrentLayerNotAddedToCell := True;
  end;

  Procedure BeginCellPassIteration;
  begin
    if Assigned(CellPassIterator) then
      begin
        CellPassIterator.Initialise;
        CurrentPassIndex := -1;
      end
    else
      CurrentPassIndex := FirstCellPassIndex;
  end;

  Function SelectCurrentCellPassInScan : Boolean;
  begin
    if Debug_ExtremeLogSwitchG then
      SIGLogMessage.PublishNoODS(Nil, Format('In SelectCurrentCellPassInScan', []), slmcDebug);

    //Select only those cellpasses which satisfy applied filter
    if (FilterAppliedToCellPasses)  then
      begin
        //search for the first cell pass which satisfies filter
        Result := false;
        while CurrentPassIndex <> FinishCellPassIndex do
        begin
          if (Cell.FilteredPassFlags[CurrentPassIndex]) then
            begin
              CurrentPass := Cell.Passes.FilteredPassData[CurrentPassIndex];
              if not FilteredValuePopulationComplete then
                 PopulateFilteredValues(FSiteModel.MachinesTargetValues,
                                        GetTargetValues(CurrentPass.FilteredPass.MachineID),
                                        FPopulationControl, CurrentPass);
              Result := true;
              break;
            end;
          Inc(CurrentPassIndex);
        end;
      end
    else
      begin
        Result := CurrentPassIndex <> FinishCellPassIndex;
        if Result then
          begin
            CurrentPass := Cell.Passes.FilteredPassData[CurrentPassIndex];
            if not FilteredValuePopulationComplete then
              PopulateFilteredValues(FSiteModel.MachinesTargetValues,
                                     GetTargetValues(CurrentPass.FilteredPass.MachineID),
                                     FPopulationControl, CurrentPass);
          end;
      end;

    if Debug_ExtremeLogSwitchG then
      SIGLogMessage.PublishNoODS(Nil, Format('Out SelectCurrentCellPassInScan', []), slmcDebug);
  end;

  procedure MoveToNextCurrentPass;
  begin
    // This is really only relevant to processing that does not use the cell pass
    // in segment iterator
    if not Assigned(CellPassIterator) then
      Inc(CurrentPassIndex, PassIndexDirectionIncrement);
  end;

  Procedure RemoveNonFilteredPasses;
  var
    LayerIndex : Integer;
    PassIndex  : Integer;
    TempCount  : Integer;
    Count      : Integer;
    HalfPassCount : Integer;
    LayerHalfPassCount : Integer;
    PassStartIdx, EndPassIdx : Integer;

  begin
    Count := 0;
    HalfPassCount := 0;

    for LayerIndex := 0 to Cell.Layers.Count - 1 do
      begin
        PassStartIdx := Cell.Layers[LayerIndex].StartCellPassIdx;
        EndPassIdx := Cell.Layers[LayerIndex].EndCellPassIdx;

        if LayerIndex > 0 then
          begin // this code makes sure the indexing is correct for following layers. if more is removed in this layer, end index is adjusted below
            TempCount := Cell.Layers[LayerIndex].PassCount;
            Cell.Layers[LayerIndex].StartCellPassIdx := Cell.Layers[LayerIndex - 1].EndCellPassIdx + 1;
            Cell.Layers[LayerIndex].EndCellPassIdx := Cell.Layers[LayerIndex].StartCellPassIdx + TempCount - 1;
          end;

        LayerHalfPassCount := 0;

        // loop through layer and remove unwanted passes
        for PassIndex := PassStartIdx to EndPassIdx do
        begin
          if Cell.FilteredPassFlags[PassIndex] then
            begin
              // every good pass gets readded here using the count index
              // count makes sure we always start at index 0 for for first layer
              if Count <> PassIndex then
                begin
                  Cell.Passes.FilteredPassData[Count] := Cell.Passes.FilteredPassData[PassIndex];
                  Cell.FilteredPassFlags[Count] := Cell.FilteredPassFlags[PassIndex];
                end;

              if (Cell.Passes.FilteredPassData[Count].MachineType in kICHalfPassCompactorMachineTypes) or
                  Cell.Passes.FilteredPassData[Count].FilteredPass.HalfPass then
                begin
                  Inc(HalfPassCount);
                  Inc(LayerHalfPassCount);
                end
              else
                begin
                  Inc(HalfPassCount, 2);
                  Inc(LayerHalfPassCount, 2);
                end;

              Inc(Count);
            end
          else
            begin
              // move end index up one as pass failed validation
              Cell.Layers[LayerIndex].EndCellPassIdx := Cell.Layers[LayerIndex].EndCellPassIdx - 1;
            end;
        end;

        // for first layer we may need to reset indexes. It use to assume startindex was always 0 for first layer but that has changed since
        // Bug31595
        if (LayerIndex = 0) then
          begin // make sure first layer indexes are correct
            Cell.Layers[LayerIndex].StartCellPassIdx := 0;
            Cell.Layers[LayerIndex].EndCellPassIdx := Count - 1;
          end;

        Cell.Layers[LayerIndex].FilteredPassCount     := Cell.Layers[LayerIndex].PassCount;
        Cell.Layers[LayerIndex].FilteredHalfPassCount := LayerHalfPassCount;
      end;

    Cell.FilteredPassCount := Count;
    Cell.FilteredHalfPassCount := HalfPassCount;
    Cell.Passes.PassCount := Count;

    // Remove any layers at the top of the stack that do not have any cell passes in them
    for LayerIndex := Cell.Layers.Count - 1 downto 0 do
      with Cell.Layers[LayerIndex] do
        if EndCellPassIdx < StartCellPassIdx then
          Cell.Layers.RemoveLastLayer
        else
          Break;

  end;
  //============================================================================
  procedure ApplyElevationTypeFilter;
  const
    CompareEpsilon = 1e-7; // to 7 decimal places

  var
    LayerIndex    :Integer;
    PassIndex     :Integer;
    PassStartIdx  :Integer;
    EndPassIdx    :Integer;
    TempIndex     :Integer;
    TempDateTime  :TDateTime;
    TempHeight    :TICCellHeight;
    FirstPassIdx  :Integer;
    LastPassIdx   :Integer;
    SetInThisLayer : Boolean;

  begin
    if not Assigned(PassFilter) or not PassFilter.HasElevationTypeFilter then
      Exit;

    TempDateTime  := 0;
    TempHeight    := 0;

    // Loop through the layer and mark unwanted passes as unfiltered...
    case PassFilter.ElevationType of
      etLast   : TempDateTime := 0;
      etFirst  : TempDateTime := NullReal;
      etHighest: TempHeight   := 0;
      etLowest : TempHeight   := NullSingle;
    else
      Assert(False, 'Undefined Elevation Type!!!'); {SKIP}
    end;

    FirstPassIdx  := -1;
    LastPassIdx   := -1;
    TempIndex     := -1;

    for LayerIndex := 0 to Cell.Layers.Count - 1 do
      begin
        SetInThisLayer := False;
        PassStartIdx := Cell.Layers[LayerIndex].StartCellPassIdx;
        EndPassIdx := Cell.Layers[LayerIndex].EndCellPassIdx;

        if LayerIndex = 0 then
          FirstPassIdx := PassStartIdx;

        if LayerIndex = Cell.Layers.Count - 1 then
          LastPassIdx := EndPassIdx;

        if not LiftBuildSettings.IncludeSuperseded and (iclsSuperseded in Cell.Layers[LayerIndex].Status) then
          Continue;

        for PassIndex := PassStartIdx to EndPassIdx do
          begin
            case PassFilter.ElevationType of
              etLast    :
                begin
                  if CompareValue(Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Time, TempDateTime,CompareEpsilon) = GreaterThanValue then
                    begin
                      TempDateTime:= Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Time;
                      TempIndex   := PassIndex;
                      SetInThisLayer := True;
                    end;
                end;
              etFirst  :
                begin
                  if  CompareValue(Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Time, TempDateTime,CompareEpsilon) = LessThanValue then
                    begin
                      TempDateTime:= Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Time;
                      TempIndex   := PassIndex;
                      SetInThisLayer := True;
                    end;
                  end;
              etHighest:
                begin
                  if  CompareValue(Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Height, TempHeight,CompareEpsilon) = GreaterThanValue then
                    begin
                      TempHeight:= Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Height;
                      TempIndex := PassIndex;
                      SetInThisLayer := True;
                    end;
                end;
              etLowest :
                begin
                  if  CompareValue(Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Height, TempHeight,CompareEpsilon) = LessThanValue then
                    begin
                      TempHeight:= Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Height;
                      TempIndex := PassIndex;
                      SetInThisLayer := True;
                    end;
                end
            else
              Assert(False, 'Undefined Elevation Type!!!'); {SKIP}
            end;
          end;
        // because we are delaing with only one record we need to reset the details at the layer level
        if SetInThisLayer then // if a elevation index was set in this layer update this layers details with the matching index
        begin
          Cell.Layers[LayerIndex].CCV_CellPassIdx := TempIndex;
          Cell.Layers[LayerIndex].CCV := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.CCV;
          Cell.Layers[LayerIndex].CCV_Elev := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Height;
          Cell.Layers[LayerIndex].CCV_MachineID := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.MachineID;
          Cell.Layers[LayerIndex].CCV_Time := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Time;

          Cell.Layers[LayerIndex].MDP := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.MDP;
          Cell.Layers[LayerIndex].MDP_Elev := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Height;
          Cell.Layers[LayerIndex].MDP_MachineID := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.MachineID;
          Cell.Layers[LayerIndex].MDP_Time := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Time;

          Cell.Layers[LayerIndex].CCA := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.CCA;
          Cell.Layers[LayerIndex].CCA_Elev := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Height;
          Cell.Layers[LayerIndex].CCA_MachineID := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.MachineID;
          Cell.Layers[LayerIndex].CCA_Time := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Time;

          Cell.Layers[LayerIndex].MaterialTemperature := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.MaterialTemperature;
          Cell.Layers[LayerIndex].MaterialTemperature_Elev := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Height;
          Cell.Layers[LayerIndex].MaterialTemperature_MachineID := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.MachineID;
          Cell.Layers[LayerIndex].MaterialTemperature_Time := Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Time;
          // if profiling does other data tpes in future you probably will need to update those types here as well
        end;

      end;

    if TempIndex > -1 then
      begin
        for PassIndex := FirstPassIdx to LastPassIdx do
          Cell.FilteredPassFlags[PassIndex] := PassIndex = TempIndex;
      end;
  end;
  //============================================================================

{$endregion}

begin
  if Debug_ExtremeLogSwitchE then
    SIGLogMessage.PublishNoODS(Nil, Format('In BuildLiftsForCell at %dx%d callerID:%s', [Cell.OTGCellX, Cell.OTGCellY, GetEnumName(TypeInfo(TCallerIDs), ord(CallerId))]), slmcDebug);

  Result := False;
  FPopulationControl_AnySet := FPopulationControl.AnySet;
  NumCellPassesRemainingToFetch := 1000;

  CalculateFlags(FProfileTypeRequired,
                 LiftBuildSettings,
                 CompactionSummaryInLiftBuildSettings,
                 WorkInProgressSummaryInLiftBuildSettings,
                 ThicknessInProgressInLiftBuildSettings);

  Cell.ClearLayers;

  FilteredPassCountOfTopMostLayer := 0;
  FilteredHalfCellPassCountOfTopMostLayer := 0;

  FComputedSupercededStatusForLayers := False;
  FilteredValuePopulationComplete := False;
  FilterAppliedToCellPasses := False;

  InitLastValueVars;
  InitLayerTrackingVars;

  if Assigned(CellPassIterator) then
    begin
      Cell.Passes.Clear;
      NumCellPassesRemainingToFetch := CellPassIterator.MaxNumberOfPassesToReturn;
    end;

  if not Assigned(CellPassIterator) then
    begin
      NumCellPassesRemainingToFetch := VLPDSvcLocations.VLPDPSNode_MaxCellPassIterationDepth_PassCountDetailAndSummary;
      SetCellIterationParameters;
      if Assigned(FCellPassFastEventLookerUpper) then
        begin
          FCellPassFastEventLookerUpper.PopulateFilteredValues(Cell.Passes.FilteredPassData, 0, Cell.Passes.PassCount - 1, FPopulationControl, False);
          FilteredValuePopulationComplete := True;
        end
      else
        ; //Assert(False, 'FCellPassFastEventLookerUpper not available');
    end
  else
    begin
      // PopulateFilteredValues is used on a cell by cell basis to detemine the required values
    end;

  // This is the main loop in the layer analysis processing. It moves through each
  // cell pass in a formards in time direction and determines
  // if each cell pass should be included in the layer being constructed from the
  // cell passes
  // Optionally a client grid and a filter context may be provided which indicates that
  // a single filtered value is the required result (ie: the lift building is used to
  // determine the superced status of cell pass layers to aid in the filtered value
  // selection).

  if Debug_ExtremeLogSwitchE then
    SIGLogMessage.PublishNoODS(Nil, Format('In BuildLiftsForCell at %dx%d: BeginCellPassIteration', [Cell.OTGCellX, Cell.OTGCellY]), slmcDebug);

  if Assigned(CellPassIterator) then
    begin
(*
1.	When assembling passes in ICProfile apply a filter that contains those filter
    aspects that do not require any of the events to be looked up (time, machine ID etc).
    Only then look up event.
2.	When assembling passes in ICProfile that pass the above test, assemble all passes
    into a single list and then apply an optimised LookerUpper across all passes to
    compute the events, then run the filter checking the machine event relevant
    filter settings across those cell passes.
3.	If there is a restriction in the number of cell passes to be returned, construct
    blocks of the cell passes assembled in #2 above and run the looker upper and
    filter over those until the maximum number of cell passes is achieved.
4.	Perform layer analysis etc on the result of optimisations 1 to 3 above.
*)
      // Build the list of cell passes to be processed into layers
      TempPassesSize := Min_I(1000, NumCellPassesRemainingToFetch);
      TempPassesSizeDiv2 := TempPassesSize DIV 2;

      SetLength(TempPasses, TempPassesSize);
      SetLength(TempFilteredPassFlags, TempPassesSize);

      // Override the cell pass iteration direction to backwards as this allows
      // stopping reading of cell passes once the maximum number of passes has been read
      CellPassIterator.SegmentIterator.IterationDirection := ssidBackwards;

      BeginCellPassIteration;

      with Cell, Passes, PassFilter do
        begin
          if Length(FilteredPassFlags) < 100 then
            SetLength(FilteredPassFlags, 100);
          FilteredPassCount := 0;
          FilteredHalfPassCount := 0;

          repeat
            TempPassCount := 0;
            ReadCellPassIntoTempList := False;
            with CellPassIterator do
              while MayHaveMoreFilterableCellPasses do
                begin
                  ReadCellPassIntoTempList := GetNextCellPass(Pass);

                  if not ReadCellPassIntoTempList then
                    Break;

                  TempFilteredPassFlags[TempPassCount] := FilterPass_NoMachineEvents(Pass);
                  TempPasses[TempPassCount].FilteredPass := Pass;

                  Inc(TempPassCount);

                  if TempPassCount >= TempPassesSizeDIV2 then
                    Break;
                end;

            // Extract all events for pre-filtered list
//            if Debug_ExtremeLogSwitchF then
//              SIGLogMessage.PublishNoODS(Nil, Format('In BuildLiftsForCell at %dx%d: About to PopulateFilteredValues', [Cell.OTGCellX, Cell.OTGCellY]), slmcDebug);

            FCellPassFastEventLookerUpper.PopulateFilteredValues(TempPasses, TempPassCount - 1, 0, FPopulationControl, False);

//            if Debug_ExtremeLogSwitchF then
//              SIGLogMessage.PublishNoODS(Nil, Format('In BuildLiftsForCell at %dx%d: PopulateFilteredValues complete', [Cell.OTGCellX, Cell.OTGCellY]), slmcDebug);

            // Construct the list of filtered cell passes ready to have layer analysis performed on them
            // filter them on any required machine events in the process
            for I := 0 to TempPassCount - 1 do
              begin
                AddPass(TempPasses[I], False);

                if PassCount = Length(FilteredPassFlags) then
                  SetLength(FilteredPassFlags, Length(FilteredPassFlags) + 100);

                if TempFilteredPassFlags[I] then // if valid pass
                  begin
                    FilteredPassFlags[PassCount-1] := FilterPass_MachineEvents(TempPasses[I]);
                    if FilteredPassFlags[PassCount-1] then
                      begin
                        Inc(FilteredPassCount);
                        if TempPasses[I].FilteredPass.HalfPass or (TempPasses[I].MachineType in kICHalfPassCompactorMachineTypes) then
                          Inc(FilteredHalfPassCount)
                        else
                          Inc(FilteredHalfPassCount, 2);  // record as a whole pass
                      end;
                  end
                else
                  FilteredPassFlags[PassCount-1] := False;

                if FilteredPassCount >= NumCellPassesRemainingToFetch then
                  Break;
              end;
          until not ReadCellPassIntoTempList or (FilteredPassCount >= NumCellPassesRemainingToFetch);

         if Debug_ExtremeLogSwitchE then
           SIGLogMessage.PublishNoODS(Nil, Format('In BuildLiftsForCell TotalPasses:%d Valid:%d', [TempPassCount,FilteredHalfPassCount div 2 ]), slmcDebug);

          // Reverse the order of the selected passes to allow layer analysis to proceed in the
          // standard fashion
          if PassCount > 0 then
            for I := 0 to (PassCount DIV 2) - 1 do
              begin
                TempPass := FilteredPassData[I];

                FilteredPassData[I] := FilteredPassData[PassCount - I - 1];
                FilteredPassData[PassCount - I - 1] := TempPass;

                Swap_b(FilteredPassFlags[I], FilteredPassFlags[PassCount - I - 1]);
              end;
        end;

      if VLPDSvcLocations.Debug_LogLiftAnalysisCellPassIteratorRestriction then
        if (Cell.FilteredPassCount >= NumCellPassesRemainingToFetch) and ReadCellPassIntoTempList then
          SIGLogMessage.PublishNoODS(Nil, Format('BuldLiftsForCell: Terminating scan as max filtered cell pass limit reached (%d vs %d) after scanning %d segments and %d passes in total',
                                                 [Cell.FilteredPassCount, NumCellPassesRemainingToFetch, CellPassIterator.SegmentIterator.NumberOfSegmentsScanned, Cell.Passes.PassCount]), slmcDebug);

      // Discard the cell pass iterator - it is not longer needed
      CellPassIterator := Nil;
      FilteredValuePopulationComplete := True;
      FilterAppliedToCellPasses := True;

      // Set up cell iteration based on the list of cell passes that have been assembled
      SetCellIterationParameters;
    end
  else
    begin
      SetLength(Cell.FilteredPassFlags, Cell.Passes.PassCount);
    end;

  BeginCellPassIteration;

    if Debug_ExtremeLogSwitchF then
      SIGLogMessage.PublishNoODS(Nil, Format('In BuildLiftsForCell at %dx%d: Entering main loop', [Cell.OTGCellX, Cell.OTGCellY]), slmcDebug);

  //  LayerIsGenerated := false;

    while SelectCurrentCellPassInScan do
      begin
        if Debug_ExtremeLogSwitchF then
          SIGLogMessage.PublishNoODS(Nil, Format('In BuildLiftsForCell at %dx%d: CheckLayerCompleted', [Cell.OTGCellX, Cell.OTGCellY]), slmcDebug);

        CheckLayerCompleted; // closes layer if true

        if Debug_ExtremeLogSwitchF then
          SIGLogMessage.PublishNoODS(Nil, Format('In BuildLiftsForCell at %dx%d: AddValidatedPassToLayer', [Cell.OTGCellX, Cell.OTGCellY]), slmcDebug);

        AddValidatedPassToLayer;

        if Debug_ExtremeLogSwitchF then
          SIGLogMessage.PublishNoODS(Nil, Format('In BuildLiftsForCell at %dx%d: MoveToNextCurrentPass', [Cell.OTGCellX, Cell.OTGCellY]), slmcDebug);

        MoveToNextCurrentPass;
      end;

  if Debug_ExtremeLogSwitchE then
    SIGLogMessage.PublishNoODS(Nil, Format('In BuildLiftsForCell at %dx%d: Adding final layer', [Cell.OTGCellX, Cell.OTGCellY]), slmcDebug);

  // If we haven't added the lift we've been working on to the profile cell, then do so now
  if CurrentLayerNotAddedToCell then
    AddFinishedLayer;

  if Debug_ExtremeLogSwitchE then
    SIGLogMessage.PublishNoODS(Nil, Format('In BuildLiftsForCell at %dx%d: Checking if need to select filtered pass', [Cell.OTGCellX, Cell.OTGCellY]), slmcDebug);

  MainValueAquired := false;

  // Check to see if we still need to select a filtered pass... Skip back through the
  // layers avoiding superceded layers for value selection
  if not Result and ReturnIndividualFilteredValueSelection and Assigned(ClientGrid) then
    with Cell do
      if ValidPassesExist and (Layers.Count > 0) and Assigned(AssignmentContext) then
        begin

         AssignmentContext.FilteredValue.FilteredPassData.Clear;
         AssignmentContext.PreviousFilteredValue.FilteredPassData.Clear;

         //Reinitialize speed values to search for min&max speed
         Cell.CellMinSpeed := kICNullMachineSpeed;
         Cell.CellMaxSpeed := 0;

          // Calculate the superceded and layer thickness information for the computed layers
          if (LiftBuildSettings.IncludeSuperseded = false) then
            ComputeSupercededStatusForLayers;

          for I := Layers.Count - 1 downto 0 do
            with Layers[I] do
              begin
                if iclsSuperseded in Status then
                  Continue;

                FilteredPassIndex := EndCellPassIdx;

                LayerContainsAFilteredPass := False;

                // Work through all passes in layer starting at EndCellPassIdx
                Repeat
                  if (FilterAppliedToCellPasses or PassFilter.FilterPass(Passes.FilteredPassData[FilteredPassIndex])) and
                      (not FilterAppliedToCellPasses or FilteredPassFlags[FilteredPassIndex]) then
                    begin
                      LayerContainsAFilteredPass := True;
                      if not ClientGrid.AssignableFilteredValueIsNull(Passes.FilteredPassData[FilteredPassIndex]) then
                        Result := True;
                    end;

                  if Result then
                    begin
                      if not MainValueAquired then
                        begin
                          AssignmentContext.FilteredValue.FilteredPassData := Passes.FilteredPassData[FilteredPassIndex];
                          MainValueAquired := true;
                          if (ClientGrid.GridDataType in [icdtCCVPercentChange, icdtCellProfile, icdtCCVPercentChangeIgnoredTopNullValue] )then
                          begin
                            Result := false;
                            if ((ClientGrid.GridDataType in [icdtCCVPercentChange, icdtCCVPercentChangeIgnoredTopNullValue])) then
                              (ClientGrid as TICClientSubGridTreeLeaf_CCV).IgnoresNullValueForLastCMV := false;
                            Dec(FilteredPassIndex);
                          end;
                        end
                      else
                        begin
                          if (ClientGrid.GridDataType in [icdtCCVPercentChange, icdtCellProfile, icdtCCVPercentChangeIgnoredTopNullValue]) then
                          begin
                            Result := True;
                            AssignmentContext.PreviousFilteredValue.FilteredPassData.Assign(Passes.FilteredPassData[FilteredPassIndex]);
                            Dec(FilteredPassIndex);
                          end;
                        end;

                      if (ClientGrid.GridDataType = icdtMachineSpeedTarget) then
                        begin
                          AnalyzeSpeedTargets(Passes.FilteredPassData[FilteredPassIndex].FilteredPass.MachineSpeed);
                          //Force spinning here till we reach end of cellpasses
                          Result := false;
                          Dec(FilteredPassIndex);
                        end;

                    end
                  else
                    Dec(FilteredPassIndex);
                Until Result or (FilteredPassIndex = StartCellPassIdx - 1);

                if Result then
                  Break;

                if LayerContainsAFilteredPass then   // CCA not catered for here with settings
                  if ((ClientGrid.GridDataType = icdtCCV) and LiftBuildSettings.CCVSummarizeTopLayerOnly and (LiftBuildSettings.CCVSummaryTypes <> [])) or
                    ((ClientGrid.GridDataType = icdtMDP) and LiftBuildSettings.MDPSummarizeTopLayerOnly and (LiftBuildSettings.MDPSummaryTypes <> [])) or
                     (ClientGrid.TopLayerOnly) or
                    ((ClientGrid.GridDataType in [icdtCCVPercentChangeIgnoredTopNullValue, icdtCCVPercentChange]) and LiftBuildSettings.CCVSummarizeTopLayerOnly)
                    then
                    begin
                      // For CCV and MDP, if we are calculating summary information and the current settings
                      // are to examine only the top most layer, then there is no need to examine any further layers in the stack
                      Break;
                    end;
              end;
        end;

   if Assigned(ClientGrid) then
    if ((ClientGrid.GridDataType in [icdtCCVPercentChange, icdtCCVPercentChangeIgnoredTopNullValue])) then
      (ClientGrid as TICClientSubGridTreeLeaf_CCV).RestoreInitialSettings();

  if (MainValueAquired) then
    Result := true;

  if Debug_ExtremeLogSwitchE then
    SIGLogMessage.PublishNoODS(Nil, Format('In BuildLiftsForCell at %dx%d: Calcing superceded etc', [Cell.OTGCellX, Cell.OTGCellY]), slmcDebug);

  // Result is not true (ie: earlier processing has not decided that everything is all good)
  // then check to see if there were valid passes in the computed profile. This is tempered
  // if the caller has requested a filtered value rather than the analysis of the layers as
  // the desired result. In this case, if the requested value is a pass count then this is OK
  // (as we are not after an attribute but rather information about the cell pass stack),
  // but not if an attribute value has been requested, and none was selected during the cell
  // layers processing.)

  if not Result then
    Result := ValidPassesExist and ((FProfileTypeRequired in [icdtAll, icdtPassCount]) or (ReturnIndividualFilteredValueSelection = False) );

  if Result then
    begin
      // Calculate the superceded and layer thickness information for the computed layers
          // Calculate the superceded and layer thickness information for the computed layers
          if (LiftBuildSettings.IncludeSuperseded = false) then
            ComputeSupercededStatusForLayers;

      Cell.NormalizeLayersMaxThickness(LiftBuildSettings.FirstPassThickness);

      ComputeLayerThicknessForLayers;
    end;

  if Debug_ExtremeLogSwitchE then
    SIGLogMessage.PublishNoODS(Nil, Format('In BuildLiftsForCell at %dx%d: Handling passcount check', [Cell.OTGCellX, Cell.OTGCellY]), slmcDebug);

  // Apply the Elevation Type filter if any...
  ApplyElevationTypeFilter;

  // Remove all the non-filtered passes from the passes that were used to perform
  // the layer analysis and make sure indexing is correct in layers
  RemoveNonFilteredPasses;

  // If the caller is really just interested in the passcount of the topmost (most
  // recent) layer in the processed lifts, then count the number of cell passes in
  // the layer that match the filter and return that value to the caller.
  // Note: If there are no filters selections, or only a time filter is configured,
  // then the number of filtered passes in the top layer is the cell pass count for
  // the top layer
  if Result and // ReturnIndividualFilteredValueSelection and
     (FProfileTypeRequired in [icdtAll, icdtPassCount, icdtCellProfile, icdtCellPasses]) then
    with Cell.Layers do
      if Count > 0 then
        if FilterAppliedToCellPasses or not Assigned(PassFilter) then
          begin
            FilteredPassCountOfTopMostLayer := Last.FilteredPassCount;
            FilteredHalfCellPassCountOfTopMostLayer := Last.FilteredHalfPassCount;
          end
        else
          begin
            for CurrentPassIndex := Last.StartCellPassIdx to Last.EndCellPassIdx do
              if PassFilter.FilterPass(Cell.Passes.FilteredPassData[CurrentPassIndex]) then
                begin
                  Inc(FilteredPassCountOfTopMostLayer);
                  if Cell.Passes.FilteredPassData[CurrentPassIndex].FilteredPass.HalfPass or
                     (Cell.Passes.FilteredPassData[CurrentPassIndex].MachineType in kICHalfPassCompactorMachineTypes) then
                    Inc(FilteredHalfCellPassCountOfTopMostLayer)
                  else
                    Inc(FilteredHalfCellPassCountOfTopMostLayer, 2);
                end;

            Last.FilteredPassCount := FilteredPassCountOfTopMostLayer;
            Last.FilteredHalfPassCount := FilteredHalfCellPassCountOfTopMostLayer;
          end;

  if VLPDSvcLocations.VLPDPSNode_EmitSubgridCellPassCounts then
    if MaxCellPassListAggregationSize < Cell.Passes.PassCount then
      begin
        MaxCellPassListAggregationSize := Cell.Passes.PassCount;
        SIGLogMessage.PublishNoODS(Nil, Format('Max cell pass aggregation count: %d', [MaxCellPassListAggregationSize]), slmcDebug);
      end;

  if Debug_ExtremeLogSwitchE then
    SIGLogMessage.PublishNoODS(Nil, Format('Out BuildLiftsForCell at %dx%d', [Cell.OTGCellX, Cell.OTGCellY]), slmcDebug);
end;

function TICServerProfiler.BuildCellPassProfile(const CellFilter : TICGridDataCellSelectionFilter;
                                                const GridDataCache : TICDataStoreCache;
                                                const NEECoords     : TCSConversionCoordinates;
                                                const Design : TVLPDDesignDescriptor) : Boolean;
const
  kMaxHzVtGridInterceptsToCalculate = 8000;
var
  VtIntercepts, HzIntercepts, VtHzIntercepts    : TInterceptList;
  NVtIntercepts, NHzIntercepts, NVtHzIntercepts : Integer;
  I                     : Integer;
  ArrayCount            : Integer;
  SlicerToolUsed        : Boolean;
  ReturnDesignElevation : Boolean;
  OTGCellX, OTGCellY    : Integer;
  CurrentSubgridOrigin  : TSubgridCellAddress;
  ThisSubgridOrigin     : TSubgridCellAddress;
  FilterMask            : TSubGridTreeLeafBitmapSubGridBits;
  StartX, StartY, StartStation : Double;
  EndX, EndY, EndStation : Double;
  CurrStationPos        : double;
  Distance              : Double;
  DesignElevations      : TICClientSubGridTreeLeaf_Height;
  DesignResult          : TDesignProfilerRequestResult;
  HavePreviousRecord    : Boolean;
  SubgridOriginX,
  SubgridOriginY        : Double;

  {$region 'Nested methods'}

  procedure CalculateHorizontalIntercepts(const StartStation : double);
  var
    VGridLineStartIndex, VGridLineEndIndex: Integer;
    IntersectionCount: Integer;
    Increment : Integer;
    IntersectX, IntersectY : Float;
    HGridStartX, HGridEndX: Double;
    CoLinear: Boolean;
  begin
    // Find intersections of all horizontal grid rows
    VGridLineStartIndex := Trunc(StartY/FSubGridTree.CellSize);
    VGridLineEndIndex := Trunc(EndY/FSubGridTree.CellSize);

    Increment := Sign(VGridLineEndIndex - VGridLineStartIndex);

    // To find a match, tell the intersection matching method that each
    // horizontal gridline starts 'before' the StartX parameter and 'ends'
    // after the EndX parameter - use the CellSize as an arbitrary value for this.
    // This gets around an issue where with a perfectly vertical profile line,
    // no intersections were being determined.
    HGridStartX := StartX;
    HGridEndX := EndX;
    if HGridStartX > HGridEndX then
      swap_f(HGridEndX, HGridStartX);
    HGridStartX := HGridStartX - FSubGridTree.CellSize;
    HGridEndX := HGridEndX + FSubGridTree.CellSize;

    IntersectionCount := abs(VGridLineEndIndex - VGridLineStartIndex) + 1;
    While (IntersectionCount > 0) and
          (NHzIntercepts < kMaxHzVtGridInterceptsToCalculate) and
          not FAborted do
      begin
        if linesIntersect(StartX, StartY, EndX, EndY,
                          HGridStartX, VGridLineStartIndex * FSubGridTree.CellSize,
                          HGridEndX, VGridLineStartIndex * FSubGridTree.CellSize,
                          IntersectX, IntersectY, True, CoLinear) then
          begin
            AddPointToInterceptList(IntersectX, IntersectY,
                                    StartStation + HyPot(StartX - IntersectX, StartY - IntersectY),
                                    HzIntercepts, NHzIntercepts)
          end;
        Inc(VGridLineStartIndex, Increment);
        Dec(IntersectionCount);
      end;
  end;

  procedure CalculateVerticalIntercepts(const StartStation : double);
  var
    HGridLineStartIndex, HGridLineEndIndex: Integer;
    IntersectionCount: Integer;
    Increment : Integer;
    IntersectX, IntersectY : Float;
    VGridStartX, VGridEndX: Double;
    CoLinear: Boolean;
  begin
    // Find intersections of all vertical grid columns
    HGridLineStartIndex := Trunc(StartX/FSubGridTree.CellSize);
    HGridLineEndIndex := Trunc(EndX/FSubGridTree.CellSize);

    Increment := Sign(HGridLineEndIndex - HGridLineStartIndex);

    // To find a match, tell the intersection matching method that each
    // vertical gridline starts 'before' the StartX parameter and 'ends'
    // after the EndX parameter - use the CellSize as an arbitrary value for this.
    // This gets around an issue where with a perfectly horizontal profile line,
    // no intersections were being determined.
    VGridStartX := StartY;
    VGridEndX := EndY;
    if VGridStartX > VGridEndX then
      swap_f(VGridEndX, VGridStartX);
    VGridStartX := VGridStartX - FSubGridTree.CellSize;
    VGridEndX := VGridEndX + FSubGridTree.CellSize;

    IntersectionCount := abs(HGridLineEndIndex - HGridLineStartIndex) + 1;
    While (IntersectionCount > 0) and
          (NVtIntercepts < kMaxHzVtGridInterceptsToCalculate) and
          not FAborted do
      begin
        if linesIntersect(StartX, StartY, EndX, EndY,
                            HGridLineStartIndex * FSubGridTree.CellSize, VGridStartX,
                            HGridLineStartIndex * FSubGridTree.CellSize, VGridEndX,
                            IntersectX, IntersectY, True, CoLinear) then
          begin
            AddPointToInterceptList(IntersectX, IntersectY,
                           StartStation + HyPot(StartX - IntersectX, StartY - IntersectY),
                           VtIntercepts, NVtIntercepts);
          end;
        Inc(HGridLineStartIndex, Increment);
        Dec(IntersectionCount);
      end;
  end;

  Procedure ConstructSubgridSpatialAndPositionalMask(var Mask : TSubGridTreeLeafBitmapSubGridBits);
  var
    InterceptIdx : Integer;
    CellX, CellY,
    OTGCellX, OTGCellY: Integer;
    CellCenterX, CellCenterY: Double;
    ThisSubgridOrigin : TSubgridCellAddress;
  begin
    Mask.Clear;

    with CellFilter, FSubGridTree do
      for InterceptIdx := I to NVtHzIntercepts - 1 do
        begin
          // Determine the on-the-ground cell underneath the midpoint of each cell on the intercept line
          with VtHzIntercepts[InterceptIdx] do
            FSubGridTree.CalculateIndexOfCellContainingPosition(MidPointX, MidPointY, OTGCellX, OTGCellY);

          ThisSubgridOrigin := TSubgridCellAddress.CreateSimple(OTGCellX SHR kSubGridIndexBitsPerLevel,
                                                                OTGCellY SHR kSubGridIndexBitsPerLevel);

          if CurrentSubgridOrigin.IsSameAs(ThisSubgridOrigin) then
            begin
              CellX := OTGCellX AND kSubGridLocalKeyMask;
              CellY := OTGCellY AND kSubGridLocalKeyMask;

              if FilterSelections <> [] then
                begin
                  GetCellCenterPosition(OTGCellX, OTGCellY, CellCenterX, CellCenterY);

                  if IsCellInSelection(CellCenterX, CellCenterY) then
                    Mask.SetBit(CellX, CellY);
                end
              else
                Mask.SetBit(CellX, CellY);
            end
          else
            Break;
        end;
  end;

  Function ConstructSubgridCellFilterMask(var Mask : TSubGridTreeLeafBitmapSubGridBits) : Boolean;
  var
    DesignMask       : TSubGridTreeLeafBitmapSubGridBits;
    DesignFilterMask : TSubGridTreeLeafBitmapSubGridBits;
    RequestResult    : TDesignProfilerRequestResult;
  begin

    Result := True;

    ConstructSubgridSpatialAndPositionalMask(Mask);

    // If the filter contains a design mask filter then compute this and AND it with the
    // mask calculated in the step above to derive the final required filter mask

    with CellFilter do
    begin
      if HasAlignmentDesignMask then
        begin
          // Query the design profiler service for the corresponding filter mask given the
          // alignment design configured in the cell selection filter.

          with DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService do
            begin
              RequestResult := RequestDesignMaskFilterPatch(Construct_ComputeDesignFilterPatch_Args(FSiteModel.ID,
                                                                                                    SubgridOriginX, SubgridOriginY,
                                                                                                    FSiteModel.Grid.CellSize,
                                                                                                    ReferenceDesign,
                                                                                                    Mask,
                                                                                                    StartStation, EndStation,
                                                                                                    LeftOffset, RightOffset),
                                                            DesignMask);

              if RequestResult = dppiOK then
                Mask := Mask AND DesignMask
              else
                begin
                  Result := False;
                  SIGLogMessage.PublishNoODS(Nil, Format('Call (A1) to RequestDesignMaskFilterPatch in TICServerProfiler returned error result %s for %s.',
                                                         [DesignProfilerErrorStatusName(RequestResult), CellFilter.ReferenceDesign.ToString]), slmcError);
                end;
            end;
        end;

        if HasDesignFilter then
        begin
          // Query the design profiler service for the corresponding filter mask given the
          // alignment design configured in the cell selection filter.

          with DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService do
            begin
              RequestResult := RequestDesignMaskFilterPatch(Construct_ComputeDesignFilterPatch_Args(FSiteModel.ID,
                                                                                                    SubgridOriginX, SubgridOriginY,
                                                                                                    FSiteModel.Grid.CellSize,
                                                                                                    DesignFilter,
                                                                                                    Mask,
                                                                                                    StartStation, EndStation,
                                                                                                    LeftOffset, RightOffset),
                                                            DesignFilterMask);

              if RequestResult = dppiOK then
                Mask := Mask AND DesignFilterMask
              else
                begin
                  Result := False;
                  SIGLogMessage.PublishNoODS(Nil, Format('Call (A2) to RequestDesignMaskFilterPatch in TICServerProfiler returned error result %s for %s.',
                                                         [DesignProfilerErrorStatusName(RequestResult), CellFilter.DesignFilter.ToString]), slmcError);
                end;
            end;
        end;

    end;
  end;

  function AddCellPassesDataToList(const OTGX, OTGY: Integer; const Station, InterceptLength: Float) : Boolean;
  var
    ProfileCell : TICProfileCell;
  begin
    Result := False;
    try
      ProfileCell := TICProfileCell.Create;
      ProfileCell.OTGCellX := OTGX;
      ProfileCell.OTGCellY := OTGY;
      ProfileCell.Station := Station;
      ProfileCell.InterceptLength := InterceptLength;

      if Assigned(DesignElevations) then
        ProfileCell.DesignElev := DesignElevations.Cells[OTGX and kSubGridLocalKeyMask, OTGY and kSubGridLocalKeyMask]
      else
        ProfileCell.DesignElev := NullSingle;

      FProfileCells.Add(ProfileCell);
      Result := True;
    except
      On E:Exception do
        SIGLogMessage.PublishNoODS(nil, 'ICProfile.BuildCellPassProfile.AddCellPassesDataToList Exception. ' + e.message, slmcException)
    end;
  end;

{$endregion}

begin
  Result := False;
  CurrStationPos := 0;
  SlicerToolUsed := False;
  HavePreviousRecord := False;
  NVtIntercepts := 0;
  NHzIntercepts := 0;
  NVtHzIntercepts := 0;

  CurrentSubgridOrigin := TSubgridCellAddress.CreateSimple(MaxInt, MaxInt);

  FGridDistanceBetweenProfilePoints := 0;
  ArrayCount := Length(NEECoords);

  ReturnDesignElevation := Design.FileName <> '';

  DesignElevations := nil;

  // 26-June-2012 US14653 we now process an array of coordinates. Previously just a start and end point
  for I := 0 to Arraycount -2 do
  begin
    StartX := NEECoords[I].X;
    StartY := NEECoords[I].Y;
    StartStation := NEECoords[I].Z;
    EndX   := NEECoords[I+1].X;
    EndY   := NEECoords[I+1].Y;
    EndStation := NEECoords[I+1].Z;

    if I = 0 then // Add start point of profile line to intercept list
    begin
     // this could be improved by passing a is slicertoolused variable but this method should be reliable
      SlicerToolUsed := (Arraycount = 2) and ((NEECoords[0].Z = 0) and (NEECoords[1].Z = 0));
      if SlicerToolUsed then
        CurrStationPos := 0
      else
        CurrStationPos := NEECoords[I].Z; // alignment profiles pass in chainage for more accuracy
      AddPointToInterceptList(StartX, StartY, CurrStationPos, VtHzIntercepts, NVtHzIntercepts);
    end;

    if SlicerToolUsed then
      Distance := Hypot(EndX - StartX, EndY - StartY) // chainage is not passed so compute
    else
      Distance := EndStation - StartStation; // use precise chainage passed

    if Distance = 0 then // if we have two points the same
       Continue;


   // Get all intercepts between profile line and cell boundaries for this segment
    CalculateHorizontalIntercepts(CurrStationPos); // pass the distance down alignment this segment starts
    CalculateVerticalIntercepts(CurrStationPos);

    FGridDistanceBetweenProfilePoints := FGridDistanceBetweenProfilePoints + Distance; // add actual distance along long
    CurrStationPos := CurrStationPos + Distance; // add distance to current station
  end;

  // Merge vertical and horizontal cell boundary/profile line intercepts
  MergeInterceptLists(VtIntercepts, HzIntercepts,
                      NVtIntercepts, NHzIntercepts,
                      VtHzIntercepts, NVtHzIntercepts);

  // Add end point of profile line to intercept list
  AddPointToInterceptList(EndX, EndY, CurrStationPos, VtHzIntercepts, NVtHzIntercepts);

  // Update each intercept with it's midpoint and intercept length
  // i.e. the midpoint on the line between one intercept and the next one
  // and the length between those intercepts
  UpdateMergedListInterceptMidPoints(VtHzIntercepts, NVtHzIntercepts);

  try
    if NVtHzIntercepts > FProfileCells.Capacity then
      FProfileCells.Capacity := NVtHzIntercepts;

    // Iterate over all intercepts calculating the results for each cell that lies in
    // a subgrid handled by this node
    for i := 0 to NVtHzIntercepts - 1 do
      begin
        if FAborted then
          Exit;

        // Determine the on-the-ground cell underneath the midpoint of each intercept line
        with VtHzIntercepts[i] do
          FSubGridTree.CalculateIndexOfCellContainingPosition(MidPointX, MidPointY, OTGCellX, OTGCellY);

        ThisSubgridOrigin := TSubgridCellAddress.CreateSimple(OTGCellX SHR kSubGridIndexBitsPerLevel,
                                                              OTGCellY SHR kSubGridIndexBitsPerLevel);

        if not CurrentSubgridOrigin.IsSameAs(ThisSubgridOrigin) then
          begin
            HavePreviousRecord := False;  // new subgrid

            with VLPDDynamicSvcLocations do
              if PSNodeServicingSubgrid(ThisSubgridOrigin.ToSkipInterleavedDescriptor).Descriptor <> PSNodeDescriptors.ThisNodeDescriptor then
                Continue;

            CurrentSubgridOrigin := ThisSubgridOrigin;

            FSubGridTree.GetCellOriginPosition(OTGCellX and not kSubGridLocalKeyMask, OTGCellY and not kSubGridLocalKeyMask,
                                               SubgridOriginX, SubgridOriginY);

            if not ConstructSubgridCellFilterMask(FilterMask) then
              Continue;

            if ReturnDesignElevation then // cutfill profile request then get elevation at same spot along design
              begin
                if Assigned(DesignElevations) then
                  FreeAndNil(DesignElevations);

                DesignResult := DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService.RequestDesignElevationPatch(
                      Construct_CalculateDesignElevationPatch_Args(FSiteModel.ID, OTGCellX, OTGCellY, FSubGridTree.CellSize, Design, TSubGridTreeLeafBitmapSubGridBits.FullMask), DesignElevations);

                if (DesignResult <> dppiOK) and (DesignResult <> dppiNoElevationsInRequestedPatch) then
                  begin
                    SIGLogMessage.PublishNoODS(nil, 'Call to RequestDesignElevationPatch in TICServerProfiler failed to return a elevation patch.', slmcError);
                    Continue;
                  end;

                if (DesignResult = dppiNoElevationsInRequestedPatch) then
                  begin
                    if Assigned(DesignElevations) then
                      FreeAndNil(DesignElevations); // force a nullsingle to be written
                  end
              end;
          end;

        if FilterMask.BitSet(OTGCellX AND kSubGridLocalKeyMask, OTGCellY AND kSubGridLocalKeyMask) then
          begin
            // old method use to fetch passes here, but now its done in buildliftsforcell method so profiling matches mapview after filters applied

            // This routine checks if there is gap caused by no data and if so it makes sure there is a point
            // at the end of the last valid station + Intercept. This ensures whats drawn in profile matched whats drawn in map view
            // previously a single cell was not drawn
            if AddCellPassesDataToList(OTGCellX, OTGCellY, VtHzIntercepts[I].ProfileItemIndex, VtHzIntercepts[I].InterceptLength) then
              HavePreviousRecord := True
            else
              if HavePreviousRecord then
                begin // there is no data and last record was good so make sure previous cell gets drawn to its border
                  // add record at last good cell border
                  AddCellPassesDataToList(OTGCellX, OTGCellY, VtHzIntercepts[I-1].ProfileItemIndex + VtHzIntercepts[I-1].InterceptLength,0);
                  HavePreviousRecord := False; // do only once after a good cell and gap
                end;
          end;
      end;
  finally
    if Assigned(DesignElevations) then
      FreeAndNil(DesignElevations);
  end;

  Result := True;
end;
*/