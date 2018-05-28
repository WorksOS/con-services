using System.Collections.Generic;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
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

    private List<ProfileCell> ProfileCells = new List<ProfileCell>();
    private ISiteModel SiteModel;

    private bool PopulationControl_AnySet;
    public bool Aborted;
    public double GridDistanceBetweenProfilePoints;
    public FilteredValuePopulationControl PopulationControl;

    public CellPassFastEventLookerUpper CellPassFastEventLookerUpper { get; set; }
    private SubGridTreeBitMask PDExistenceMap;

    public ProductionDataProfiler(ISiteModel siteModel,
      IServerSubGridTree subGridTree,
      GridDataType profileTypeRequired,
      FilteredValuePopulationControl populationControl,
      SubGridTreeBitMask pDExistenceMap)
    {
      SiteModel = siteModel;
      SubGridTree = subGridTree;
      ProfileTypeRequired = profileTypeRequired;
      PopulationControl = populationControl;
      PDExistenceMap = pDExistenceMap;
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
*/