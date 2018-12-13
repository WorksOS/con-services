using System;
using System.Diagnostics;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Filters.Models;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Profiling
{
  /// <summary>
  ///  Responsible for the business logic relating to constructing a series of layers/lifts from a stack of cell passes measured for a cell
  /// </summary>
  public class CellLiftBuilder : ICellLiftBuilder
  {
    private int CurrentPassIndex;
    private IProfileLayer CurrentLayer;
    private int CurrentLayerRecycledIndex;

    private FilteredPassData CurrentPass;

    private float LastPassElev;
    private float ElevationOfLastProcessedCellPass;
    private int LayerIDOfLastProcessedCellPass;

    private short LastPassCCV;
    private DateTime LastPassCCVTime;
    private short LastPassCCVMachine;
    private float LastPassCCVElev;
    private int LastPassCCVIdx;

    private short LastPassMDP;
    private DateTime LastPassMDPTime;
    private short LastPassMDPMachine;
    private float LastPassMDPElev;

    private byte LastPassCCA;
    private DateTime LastPassCCATime;
    private short LastPassCCAMachine;
    private float LastPassCCAElev;

    private ushort LastPassMaterialTemperature;
    private DateTime LastPassMaterialTemperature_Time;
    private short LastPassMaterialTemperature_Machine;
    private float LastPassMaterialTemperature_Elev;

    private byte LastPassRadioLatency;
    private short LastPassRMV;
    private ushort LastPassFrequency;
    private ushort LastPassAmplitude;
    private DateTime LastPassTime;
    private ushort LastPassMachineSpeed;

    private float LastPassInPreviousLayerElev;
    private float FirstPassInPreviousLayerElev;

    private float CurrentLayerMaxElev;
    private float CurrentLayerMinElev;
    private int PassCountForLayer; // What for?
    private int FilteredPassCountForLayer;
    private int FilteredHalfPassCountForLayer;

    private bool NewLayer;
    private bool ValidPassesExist;
    private bool CurrentLayerNotAddedToCell;

    private DateTime LastMRDate;
    private int LastMRDesignNameID;

    private bool CompactionSummaryInLiftBuildSettings;
    private bool WorkInProgressSummaryInLiftBuildSettings;
    private bool ThicknessInProgressInLiftBuildSettings;

    private int FirstCellPassIndex;
    private int LastCellPassIndex;
    private int PassIndexDirectionIncrement;
    private int FinishCellPassIndex;

    private FilteredPassData[] TempPasses;
    private int TempPassesSize, TempPassesSizeDiv2;
    private int TempPassCount;
    private FilteredPassData TempPass;

    private bool[] TempFilteredPassFlags = new bool[0];

    private bool ReadCellPassIntoTempList;

    private bool ComputedSupercededStatusForLayers;
    private int NumCellPassesRemainingToFetch;

    private CellPass Pass;
    private bool FilteredValuePopulationComplete;
    private bool FilterAppliedToCellPasses;

    private ushort LastLayerID;
    private bool LayerContainsAFilteredPass;

    private bool MainValueAquired;

    private ISiteModel SiteModel;
    private ProfileCell Cell;
    private readonly GridDataType ProfileTypeRequired;
    private readonly IFilteredValuePopulationControl PopulationControl;
    private ICellPassAttributeFilter PassFilter;
    private ICellPassFastEventLookerUpper CellPassFastEventLookerUpper;
    private ISubGridSegmentCellPassIterator CellPassIterator;

    /// <summary>
    /// The count of filtered call passes used to construct the top most (latest) layer
    /// </summary>
    public int FilteredPassCountOfTopMostLayer { get; set; }

    // FilteredHalfCellPassCountOfTopMostLayer tracks 'half cell passes'.
    // A half cell pass is recorded when a Quattro four drum compactor drives over the ground.
    // Other machines, like single drum compactors, record two half cell pass counts to form a single cell pass.
    public int FilteredHalfCellPassCountOfTopMostLayer { get; set; }

    /// <summary>
    /// Construct a cell lift builder ready to provide profile layer analysis support on a cell by cell basis.
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="profileTypeRequired"></param>
    /// <param name="populationControl"></param>
    /// <param name="passFilter"></param>
    /// <param name="cellPassFastEventLookerUpper"></param>
    public CellLiftBuilder(ISiteModel siteModel,
      GridDataType profileTypeRequired,
      IFilteredValuePopulationControl populationControl,
      ICellPassAttributeFilter passFilter,
      ICellPassFastEventLookerUpper cellPassFastEventLookerUpper
    )
    {
      SiteModel = siteModel;
      ProfileTypeRequired = profileTypeRequired;
      PopulationControl = populationControl;
      PassFilter = passFilter;
      CellPassFastEventLookerUpper = cellPassFastEventLookerUpper;
    }

    /// <summary>
    /// Retrieve the event/target values list for a machine in the site model
    /// </summary>
    /// <param name="forMachineID"></param>
    /// <returns></returns>
    private IProductionEventLists GetTargetValues(short forMachineID) => SiteModel.MachinesTargetValues[forMachineID];

    /// <summary>
    /// Initializes the 'last' tracking variables for a layer
    /// </summary>
    private void InitLastValueVars()
    {
      LastPassCCV = CellPassConsts.NullCCV;
      LastPassMDP = CellPassConsts.NullMDP;
      LastPassCCA = CellPassConsts.NullCCA;
      LastPassRMV = CellPassConsts.NullRMV;
      LastPassRadioLatency = CellPassConsts.NullRadioLatency;
      LastPassFrequency = CellPassConsts.NullFrequency;
      LastPassAmplitude = CellPassConsts.NullAmplitude;
      LastPassMaterialTemperature = CellPassConsts.NullMaterialTemperatureValue;
      LastPassMachineSpeed = CellPassConsts.NullMachineSpeed;
      LastPassCCVIdx = 0;
    }

    /// <summary>
    /// Initializes layer tracking state ready to analyze a new cell profile
    /// </summary>
    private void InitLayerTrackingVars()
    {
      LastPassInPreviousLayerElev = Consts.NullHeight;
      FirstPassInPreviousLayerElev = Consts.NullHeight;
      NewLayer = true;
      ValidPassesExist = false;
      PassCountForLayer = 0;
      FilteredPassCountForLayer = 0;
      FilteredHalfPassCountForLayer = 0;
      CurrentLayerNotAddedToCell = false;
      CurrentLayerMaxElev = 0;
      CurrentLayerMinElev = 1E10f;
      CurrentLayer = null;
      CurrentLayerRecycledIndex = -1;
      LastMRDesignNameID = Consts.kNoDesignNameID;
      LastMRDate = DateTime.MinValue;
      LastLayerID = 0;
      ElevationOfLastProcessedCellPass = Consts.NullHeight;
    }

    /// <summary>
    /// Determines if the compaction observed in the cell is within the acceptable limits
    /// </summary>
    /// <param name="lift"></param>
    private void CheckProfileCellLiftCompaction(ProfileLayer lift)
    {
      if (lift.CCV == CellPassConsts.NullCCV && lift.MDP == CellPassConsts.NullMDP && lift.CCA == CellPassConsts.NullCCA)
        return;

      Cell.CheckLiftCompaction(lift, /* todo LiftBuildSettings,*/ ProfileTypeRequired);
    }

    /// <summary>
    /// Initializes cell pass iteration state ready to analyze the cell passes in this cell
    /// </summary>
    private void SetCellIterationParameters()
    {
      int _PassCount = Cell.Passes.PassCount;

      FirstCellPassIndex = 0;
      LastCellPassIndex = _PassCount - 1;
      PassIndexDirectionIncrement = 1;
      FinishCellPassIndex = _PassCount;
    }

    /// <summary>
    /// Begins iterating through the cell passes in this cell
    /// </summary>
    private void BeginCellPassIteration()
    {
      if (CellPassIterator != null)
      {
        CellPassIterator.Initialise();
        CurrentPassIndex = -1;
      }
      else
        CurrentPassIndex = FirstCellPassIndex;
    }

    /// <summary>
    /// Locates and selects the next pass to be processed according to filter criteria from set of cell passes for the cell 
    /// </summary>
    /// <returns></returns>
    private bool SelectCurrentCellPassInScan()
    {
      // if (Debug_ExtremeLogSwitchG)
      //   Log.LogDebug("In SelectCurrentCellPassInScan");

      //Select only those cell passes which satisfy applied filter
      bool Result;

      if (FilterAppliedToCellPasses)
      {
        //search for the first cell pass which satisfies filter
        Result = false;
        while (CurrentPassIndex != FinishCellPassIndex)
        {
          if (Cell.FilteredPassFlags[CurrentPassIndex])
          {
            CurrentPass = Cell.Passes.FilteredPassData[CurrentPassIndex];
            if (!FilteredValuePopulationComplete)
              FiltersValuePopulation.PopulateFilteredValues(GetTargetValues(CurrentPass.FilteredPass.InternalSiteModelMachineIndex),
                PopulationControl, ref CurrentPass);
            Result = true;
            break;
          }

          CurrentPassIndex++;
        }
      }
      else
      {
        Result = CurrentPassIndex != FinishCellPassIndex;
        if (Result)
        {
          CurrentPass = Cell.Passes.FilteredPassData[CurrentPassIndex];
          if (!FilteredValuePopulationComplete)
            FiltersValuePopulation.PopulateFilteredValues(GetTargetValues(CurrentPass.FilteredPass.InternalSiteModelMachineIndex),
              PopulationControl, ref CurrentPass);
        }
      }

      // if (Debug_ExtremeLogSwitchG)
      //   Log.LogDebug("Out SelectCurrentCellPassInScan");

      return Result;
    }

    /// <summary>
    /// Move the iterator state to the next cell in the iteration
    /// </summary>
    private void MoveToNextCurrentPass()
    {
      // This is really only relevant to processing that does not use the cell pass in segment iterator
      if (CellPassIterator == null)
        CurrentPassIndex += PassIndexDirectionIncrement;
    }

    /// <summary>
    /// Adds a cell pass which has passed all validation criteria for including in the current layer, to that layer.
    /// </summary>
    private void AddValidatedPassToLayer()
    {
      PassCountForLayer++;

      if (FilterAppliedToCellPasses)
      {
        if (Cell.FilteredPassFlags[CurrentPassIndex])
        {
          FilteredPassCountForLayer++;
          if (CurrentPass.FilteredPass.HalfPass ||
              MachineTypeUtilities.IsHalfPassCompactorMachine(CurrentPass.MachineType))
            FilteredHalfPassCountForLayer++;
          else
            FilteredHalfPassCountForLayer += 2; // For rest its 2 half passes recorded
        }
      }
      else
      {
        FilteredPassCountForLayer++;
        if (CurrentPass.FilteredPass.HalfPass ||
            MachineTypeUtilities.IsHalfPassCompactorMachine(CurrentPass.MachineType))
          FilteredHalfPassCountForLayer++;
        else
          FilteredHalfPassCountForLayer += 2; // For rest its 2 half passes recorded
        Cell.FilteredPassFlags[CurrentPassIndex] = true;
      }

      ValidPassesExist = true;

      if (CurrentLayer == null)
        CurrentLayer = Cell.RequestNewLayer(out CurrentLayerRecycledIndex);

      // Workout max/min/first/last elevation values if a filtered pass
      if (Cell.FilteredPassFlags[CurrentPassIndex])
      {
        if (CurrentPass.FilteredPass.Height != Consts.NullHeight)
        {
          if ((CurrentLayer.MinimumPassHeight == Consts.NullHeight) ||
              (CurrentPass.FilteredPass.Height < CurrentLayer.MinimumPassHeight))
            CurrentLayer.MinimumPassHeight = CurrentPass.FilteredPass.Height;
          if ((CurrentLayer.MaximumPassHeight == Consts.NullHeight) ||
              (CurrentPass.FilteredPass.Height > CurrentLayer.MaximumPassHeight))
            CurrentLayer.MaximumPassHeight = CurrentPass.FilteredPass.Height;
          if (CurrentLayer.FirstPassHeight == Consts.NullHeight)
            CurrentLayer.FirstPassHeight = CurrentPass.FilteredPass.Height;
          if (CurrentLayer.FirstPassHeight != Consts.NullHeight)
            CurrentLayer.LastPassHeight = CurrentPass.FilteredPass.Height;
        }
      }

      CurrentLayer.AddPass(CurrentPassIndex);

      NewLayer = false;
      CurrentLayerNotAddedToCell = true;
    }

    /// <summary>
    /// Takes the current layer state and adds it as a newly created layer to the set of processed layers for this cell.
    /// </summary>
    private void AddFinishedLayer()
    {
      CurrentLayer.Height = LastPassElev;
      CurrentLayer.Thickness = LastPassElev - LastPassInPreviousLayerElev;

      CurrentLayer.CCV = LastPassCCV;
      CurrentLayer.CCV_Time = LastPassCCVTime;
      CurrentLayer.CCV_MachineID = LastPassCCVMachine;
      CurrentLayer.CCV_Elev = LastPassCCVElev;
      CurrentLayer.CCV_CellPassIdx = LastPassCCVIdx;

      CurrentLayer.RMV = LastPassRMV;
      CurrentLayer.Frequency = LastPassFrequency;
      CurrentLayer.Amplitude = LastPassAmplitude;

      CurrentLayer.RadioLatency = LastPassRadioLatency;
      CurrentLayer.LastLayerPassTime = LastPassTime;

      CurrentLayer.MDP = LastPassMDP;
      CurrentLayer.MDP_Time = LastPassMDPTime;
      CurrentLayer.MDP_MachineID = LastPassMDPMachine;
      CurrentLayer.MDP_Elev = LastPassMDPElev;

      CurrentLayer.CCA = LastPassCCA;
      CurrentLayer.CCA_Time = LastPassCCATime;
      CurrentLayer.CCA_MachineID = LastPassCCAMachine;
      CurrentLayer.CCA_Elev = LastPassCCAElev;

      CurrentLayer.MaterialTemperature = LastPassMaterialTemperature;
      CurrentLayer.MaterialTemperature_Time = LastPassMaterialTemperature_Time;
      CurrentLayer.MaterialTemperature_MachineID = LastPassMaterialTemperature_Machine;
      CurrentLayer.MaterialTemperature_Elev = LastPassMaterialTemperature_Elev;

      CurrentLayer.Status = LayerStatus.None;
      CurrentLayer.MaxThickness = CurrentLayerMaxElev - LastPassInPreviousLayerElev;

      CurrentLayer.FilteredPassCount = FilteredPassCountForLayer;
      CurrentLayer.FilteredHalfPassCount = FilteredHalfPassCountForLayer;

      Cell.AddLayer(CurrentLayer, CurrentLayerRecycledIndex);

      CurrentLayer = null;
      CurrentLayerRecycledIndex = -1;

      NewLayer = true;
      PassCountForLayer = 0;
      FilteredPassCountForLayer = 0;
      FilteredHalfPassCountForLayer = 0;
      CurrentLayerMaxElev = 0;
      CurrentLayerMinElev = 1E10f;

      InitLastValueVars();

      CurrentLayerNotAddedToCell = false;
    }

    private bool FirstCellInLayerProcessing() =>
      CellPassIterator != null ? CurrentPassIndex == 0 : CurrentPassIndex == FirstCellPassIndex;

    /// <summary>
    /// Determines if the given cell passes the criteria for inclusion in the current lift being constructed.
    /// </summary>
    /// <param name="pass"></param>
    /// <returns></returns>
    private bool IsStillInCurrentLift(FilteredPassData pass)
    {
      bool Result = false;

      if (FirstCellInLayerProcessing())
        return true;

      if (Dummy_LiftBuildSettings.LiftDetectionType == LiftDetectionType.MapReset ||
          Dummy_LiftBuildSettings.LiftDetectionType == LiftDetectionType.AutoMapReset)
      {
        if ((pass.EventValues.MapReset_PriorDate != DateTime.MinValue &&
             pass.EventValues.MapReset_PriorDate > LastMRDate) ||
            (LayerIDOfLastProcessedCellPass != pass.EventValues.LayerID))
          return false;

        if (Dummy_LiftBuildSettings.LiftDetectionType == LiftDetectionType.MapReset)
          return true;
      }

      // If we haven't hit an explicit Map Reset, optionally check for an auto detected lift
      if (Dummy_LiftBuildSettings.LiftDetectionType == LiftDetectionType.MapReset ||
          Dummy_LiftBuildSettings.LiftDetectionType == LiftDetectionType.AutoMapReset)
      {
        // We'll force LowerDeadBandBoundary to always be subtracted from the ElevationOfLastProcessedCellPass.
        Result = Range.InRange(pass.FilteredPass.Height,
          ElevationOfLastProcessedCellPass - Math.Abs(Dummy_LiftBuildSettings.DeadBandLowerBoundary),
          ElevationOfLastProcessedCellPass + Dummy_LiftBuildSettings.DeadBandUpperBoundary);

        if (Result)
          return true;

        // If we're not in the dead band, then check for a partial lift
        if (pass.FilteredPass.Height > ElevationOfLastProcessedCellPass)
        {
          double TargetThickness;

          if (Dummy_LiftBuildSettings.OverrideMachineThickness)
            TargetThickness = Dummy_LiftBuildSettings.OverridingLiftThickness;
          else
            TargetThickness = pass.TargetValues.TargetLiftThickness;

          if (TargetThickness != CellTargets.NullOverridingTargetLiftThicknessValue)
            if ((pass.FilteredPass.Height > LastPassInPreviousLayerElev) &&
                (pass.FilteredPass.Height - LastPassInPreviousLayerElev < TargetThickness))
              Result = true;
        }
      }

      // If layer detection is via layer id has the layer id changed
      if (Dummy_LiftBuildSettings.LiftDetectionType == LiftDetectionType.Tagfile)
      {
        if (pass.EventValues.LayerID == LastLayerID || pass.EventValues.LayerID == CellEvents.NullLayerID)
          return true; // still in same layer
      }

      return Result;
    }

    /// <summary>
    /// Updates tracking state to take into account the state of the last cell pass that was analyzed
    /// </summary>
    private void UpdateLastPassTrackingVars()
    {
      // If we're not on the first pass over the cell, but the passes for the current lift are empty,
      // then store the elevation of the last pass over the lift we've just finished,
      // so that we can determine the thickness of this lift once we find the last pass over it

      if (!FirstCellInLayerProcessing() && NewLayer)
      {
        if (ElevationOfLastProcessedCellPass == Consts.NullHeight)
          LastPassInPreviousLayerElev = CurrentPass.FilteredPass.Height;
        else if (CurrentPass.FilteredPass.Height > ElevationOfLastProcessedCellPass)
          LastPassInPreviousLayerElev = ElevationOfLastProcessedCellPass;
        else
          LastPassInPreviousLayerElev = CurrentPass.FilteredPass.Height;
      }

      LastPassTime = CurrentPass.FilteredPass.Time;
      LastPassElev = CurrentPass.FilteredPass.Height;

      ElevationOfLastProcessedCellPass = CurrentPass.FilteredPass.Height;

      if (ElevationOfLastProcessedCellPass > CurrentLayerMaxElev)
        CurrentLayerMaxElev = ElevationOfLastProcessedCellPass;
      if (ElevationOfLastProcessedCellPass < CurrentLayerMinElev)
        CurrentLayerMinElev = ElevationOfLastProcessedCellPass;

      // Don't update attribute tracking vars if the cell pass was not filtered. The elevations are tracked above
      // to permit accurate determination of the layer thickness

      if (!Cell.FilteredPassFlags[CurrentPassIndex])
        return;

      if (CurrentPass.FilteredPass.CCV != CellPassConsts.NullCCV)
      {
        LastPassCCV = CurrentPass.FilteredPass.CCV;
        LastPassCCVTime = CurrentPass.FilteredPass.Time;
        LastPassCCVMachine = CurrentPass.FilteredPass.InternalSiteModelMachineIndex;
        LastPassCCVElev = CurrentPass.FilteredPass.Height;
        LastPassCCVIdx = CurrentPassIndex;
      }

      if (CurrentPass.FilteredPass.MDP != CellPassConsts.NullMDP)
      {
        LastPassMDP = CurrentPass.FilteredPass.MDP;
        LastPassMDPTime = CurrentPass.FilteredPass.Time;
        LastPassMDPMachine = CurrentPass.FilteredPass.InternalSiteModelMachineIndex;
        LastPassMDPElev = CurrentPass.FilteredPass.Height;
      }

      if (CurrentPass.FilteredPass.CCA != CellPassConsts.NullCCA)
      {
        LastPassCCA = CurrentPass.FilteredPass.CCA;
        LastPassCCATime = CurrentPass.FilteredPass.Time;
        LastPassCCAMachine = CurrentPass.FilteredPass.InternalSiteModelMachineIndex;
        LastPassCCAElev = CurrentPass.FilteredPass.Height;
      }

      if (CurrentPass.FilteredPass.MaterialTemperature != CellPassConsts.NullMaterialTemperatureValue)
      {
        LastPassMaterialTemperature = CurrentPass.FilteredPass.MaterialTemperature;
        LastPassMaterialTemperature_Time = CurrentPass.FilteredPass.Time;
        LastPassMaterialTemperature_Machine = CurrentPass.FilteredPass.InternalSiteModelMachineIndex;
        LastPassMaterialTemperature_Elev = CurrentPass.FilteredPass.Height;
      }

      if (CurrentPass.FilteredPass.RadioLatency != CellPassConsts.NullRadioLatency)
        LastPassRadioLatency = CurrentPass.FilteredPass.RadioLatency;

      if (CurrentPass.FilteredPass.RMV != CellPassConsts.NullRMV)
        LastPassRMV = CurrentPass.FilteredPass.RMV;

      if (CurrentPass.FilteredPass.Frequency != CellPassConsts.NullFrequency)
        LastPassFrequency = CurrentPass.FilteredPass.Frequency;

      if (CurrentPass.FilteredPass.Amplitude != CellPassConsts.NullAmplitude)
        LastPassAmplitude = CurrentPass.FilteredPass.Amplitude;

      if (CurrentPass.FilteredPass.MachineSpeed != CellPassConsts.NullMachineSpeed)
        LastPassMachineSpeed = CurrentPass.FilteredPass.MachineSpeed;

      if (Dummy_LiftBuildSettings.LiftDetectionType == LiftDetectionType.MapReset ||
          Dummy_LiftBuildSettings.LiftDetectionType == LiftDetectionType.AutoMapReset)
      {
        if (CurrentPass.EventValues.MapReset_PriorDate > LastMRDate)
        {
          LastMRDate = CurrentPass.EventValues.MapReset_PriorDate;
          LastMRDesignNameID = CurrentPass.EventValues.MapReset_DesignNameID;
        }

        LayerIDOfLastProcessedCellPass = CurrentPass.EventValues.LayerID;
      }

      if (Dummy_LiftBuildSettings.LiftDetectionType == LiftDetectionType.Tagfile)
        LastLayerID = CurrentPass.EventValues.LayerID;
    }

    /// <summary>
    /// Determines if the cell pass being analyzed triggers completion of the layer currently being constructed
    /// </summary>
    /// <returns></returns>
    private bool CheckLayerCompleted()
    {
      bool Result = false;

      // If this is the first pass over this cell to process, then we don't know the elevation prior to this first pass.
      if (FirstCellInLayerProcessing())
        LastPassInPreviousLayerElev = CurrentPass.FilteredPass.Height - Dummy_LiftBuildSettings.FirstPassThickness;

      if (!IsStillInCurrentLift(CurrentPass) && !NewLayer &&
          Dummy_LiftBuildSettings.LiftDetectionType != LiftDetectionType.None)
      {
        AddFinishedLayer();
        Result = true;
      }

      UpdateLastPassTrackingVars();

      return Result;
    }

    /// <summary>
    /// Examines the set of layers for a cell and determines if any of them are superseded by activity subsequent
    /// to the time the cell passes in the layer were measured
    /// </summary>
    private void ComputeSupercededStatusForLayers()
    {
      if (ComputedSupercededStatusForLayers)
        return;

      ComputedSupercededStatusForLayers = true;

      for (int LayerIndex = Cell.Layers.Count() - 1; LayerIndex >= 0; LayerIndex--)
      {
        float TestLayerHeight = Cell.Layers[LayerIndex].Height;

        if ((Cell.Layers[LayerIndex].Status & LayerStatus.Superseded) != 0)
          continue;

        int J = LayerIndex - 1;

        while (J >= 0)
        {
          if (TestLayerHeight < Cell.Layers[J].Height)
          {
            Cell.Layers[J].Status |= LayerStatus.Superseded;
            Cell.Layers[J].Thickness = 0;

            J--;
          }
          else
            break;
        }
      }
    }

    /// <summary>
    /// from final passes only return passes in pass count range
    /// </summary>
    private void ApplyPassCountRangeFilter()
    {
      if (PassFilter == null || !PassFilter.HasPassCountRangeFilter)
        return;

      if (Cell.Layers.Count() < 0)
         return;

    // Note this filter only applies to processing with no layer analysis
      int passStartIdx   = Cell.Layers[0].StartCellPassIdx;
      int endPassIdx     = Cell.Layers[0].EndCellPassIdx;
      int currentPassIdx = 0;
      bool prevHalfPass = false;

      for (int passIndex = passStartIdx; passIndex < endPassIdx; passIndex++)
      {

        if (Cell.FilteredPassFlags[passIndex]) // if marked as non filtered pass (good pass)
        {
          // if we have the second half pass or a full pass inc CurrentPassIdx. God I hate half passes
          if ((prevHalfPass == false) || (Cell.Passes.FilteredPassData[passIndex].FilteredPass.HalfPass == false))
          {
            ++currentPassIdx;
          }
          
          if (!Range.InRange(currentPassIdx, PassFilter.PasscountRangeMin, PassFilter.PasscountRangeMax))
          {
            Cell.FilteredPassFlags[passIndex] = false; // flag for removal
          } 

          // if halfpass look for twin
          if (Cell.Passes.FilteredPassData[passIndex].FilteredPass.HalfPass)
          {
            prevHalfPass = !prevHalfPass; // basically keeps CurrentPassIdx at same value until two half passes are detected
          }

        }
      }
    }



    /// <summary>
    /// Apples the 'elevation type' filter aspect to determine which cell pass of a set of filtered cell passes
    /// in a layer should be used to provide the required attribute 
    /// </summary>
    private void ApplyElevationTypeFilter()
    {
      if (PassFilter == null || !PassFilter.HasElevationTypeFilter)
        return;

      DateTime TempDateTime = DateTime.MinValue;
      float TempHeight = 0;

      // Loop through the layer and mark unwanted passes as unfiltered...
      switch (PassFilter.ElevationType)
      {
        case ElevationType.Last:
          TempDateTime = DateTime.MinValue;
          break;
        case ElevationType.First:
          TempDateTime = DateTime.MaxValue;
          break;
        case ElevationType.Highest:
          TempHeight = 0;
          break;
        case ElevationType.Lowest:
          TempHeight = Consts.NullHeight;
          break;
      }

      int FirstPassIdx = -1;
      int LastPassIdx = -1;
      int TempIndex = -1;

      for (int LayerIndex = 0; LayerIndex < Cell.Layers.Count(); LayerIndex++)
      {
        bool SetInThisLayer = false;
        int PassStartIdx = Cell.Layers[LayerIndex].StartCellPassIdx;
        int EndPassIdx = Cell.Layers[LayerIndex].EndCellPassIdx;

        if (LayerIndex == 0)
          FirstPassIdx = PassStartIdx;

        if (LayerIndex == Cell.Layers.Count() - 1)
          LastPassIdx = EndPassIdx;

        if (!Dummy_LiftBuildSettings.IncludeSuperseded &&
            (Cell.Layers[LayerIndex].Status & LayerStatus.Superseded) != 0)
          continue;

        for (int PassIndex = PassStartIdx; PassIndex < EndPassIdx; PassIndex++)
        {
          switch (PassFilter.ElevationType)
          {
            case ElevationType.Last:
            {
              if (Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Time > TempDateTime)
              {
                TempDateTime = Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Time;
                TempIndex = PassIndex;
                SetInThisLayer = true;
              }

              break;
            }
            case ElevationType.First:
            {
              if (Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Time < TempDateTime)
              {
                TempDateTime = Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Time;
                TempIndex = PassIndex;
                SetInThisLayer = true;
              }

              break;
            }
            case ElevationType.Highest:
            {
              if (Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Height > TempHeight)
              {
                TempHeight = Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Height;
                TempIndex = PassIndex;
                SetInThisLayer = true;
              }

              break;
            }
            case ElevationType.Lowest:
            {
              if (Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Height < TempHeight)
              {
                TempHeight = Cell.Passes.FilteredPassData[PassIndex].FilteredPass.Height;
                TempIndex = PassIndex;
                SetInThisLayer = true;
              }

              break;
            }
          }
        }

        // because we are dealing with only one record we need to reset the details at the layer level
        // if a elevation index was set in this layer update this layers details with the matching index
        if (SetInThisLayer)
        {
          Cell.Layers[LayerIndex].CCV_CellPassIdx = TempIndex;
          Cell.Layers[LayerIndex].CCV = Cell.Passes.FilteredPassData[TempIndex].FilteredPass.CCV;
          Cell.Layers[LayerIndex].CCV_Elev = Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Height;
          Cell.Layers[LayerIndex].CCV_MachineID =
            Cell.Passes.FilteredPassData[TempIndex].FilteredPass.InternalSiteModelMachineIndex;
          Cell.Layers[LayerIndex].CCV_Time = Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Time;

          Cell.Layers[LayerIndex].MDP = Cell.Passes.FilteredPassData[TempIndex].FilteredPass.MDP;
          Cell.Layers[LayerIndex].MDP_Elev = Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Height;
          Cell.Layers[LayerIndex].MDP_MachineID =
            Cell.Passes.FilteredPassData[TempIndex].FilteredPass.InternalSiteModelMachineIndex;
          Cell.Layers[LayerIndex].MDP_Time = Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Time;

          Cell.Layers[LayerIndex].CCA = Cell.Passes.FilteredPassData[TempIndex].FilteredPass.CCA;
          Cell.Layers[LayerIndex].CCA_Elev = Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Height;
          Cell.Layers[LayerIndex].CCA_MachineID =
            Cell.Passes.FilteredPassData[TempIndex].FilteredPass.InternalSiteModelMachineIndex;
          Cell.Layers[LayerIndex].CCA_Time = Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Time;

          Cell.Layers[LayerIndex].MaterialTemperature =
            Cell.Passes.FilteredPassData[TempIndex].FilteredPass.MaterialTemperature;
          Cell.Layers[LayerIndex].MaterialTemperature_Elev =
            Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Height;
          Cell.Layers[LayerIndex].MaterialTemperature_MachineID =
            Cell.Passes.FilteredPassData[TempIndex].FilteredPass.InternalSiteModelMachineIndex;
          Cell.Layers[LayerIndex].MaterialTemperature_Time = Cell.Passes.FilteredPassData[TempIndex].FilteredPass.Time;

          // if profiling does other data types in future you probably will need to update those types here as well
        }
      }

      if (TempIndex > -1)
        for (int PassIndex = FirstPassIdx; PassIndex < LastPassIdx; PassIndex++)
          Cell.FilteredPassFlags[PassIndex] = PassIndex == TempIndex;
    }

    /// <summary>
    /// Excludes cell passes from the cell pass stack that did not pass the filter criteria.
    /// </summary>
    private void RemoveNonFilteredPasses()
    {
      int Count = 0;
      int HalfPassCount = 0;
      int PrevIdx = 0;

      for (int LayerIndex = 0; LayerIndex < Cell.Layers.Count(); LayerIndex++)
      {
        int PassStartIdx = Cell.Layers[LayerIndex].StartCellPassIdx;
        int CCVIdx = Cell.Layers[LayerIndex].CCV_CellPassIdx;
        if (PassStartIdx > PrevIdx)  // Alter the CCV_CellPassIdx if starting index was set higher due to invalid passes
           Cell.Layers[LayerIndex].CCV_CellPassIdx  -= (PassStartIdx - PrevIdx);
        int EndPassIdx = Cell.Layers[LayerIndex].EndCellPassIdx;
        PrevIdx = EndPassIdx + 1; // position at possible start location for next layer

        if (LayerIndex > 0)
        {
          // this code makes sure the indexing is correct for following layers. if more is removed in this layer, end index is adjusted below
          int TempCount = Cell.Layers[LayerIndex].PassCount;
          Cell.Layers[LayerIndex].StartCellPassIdx = Cell.Layers[LayerIndex - 1].EndCellPassIdx + 1;
          Cell.Layers[LayerIndex].EndCellPassIdx = Cell.Layers[LayerIndex].StartCellPassIdx + TempCount - 1;
        }

        int LayerHalfPassCount = 0;

        // loop through layer and remove unwanted passes
        for (int PassIndex = PassStartIdx; PassIndex <= EndPassIdx; PassIndex++)
        {
          if (Cell.FilteredPassFlags[PassIndex])
          {
            // every good pass gets re-added here using the count index
            // count makes sure we always start at index 0 for for first layer
            if (Count != PassIndex)
            {
              Cell.Passes.FilteredPassData[Count] = Cell.Passes.FilteredPassData[PassIndex];
              Cell.FilteredPassFlags[Count] = Cell.FilteredPassFlags[PassIndex];
            }

            if (MachineTypeUtilities.IsHalfPassCompactorMachine(Cell.Passes.FilteredPassData[Count].MachineType) ||
                Cell.Passes.FilteredPassData[Count].FilteredPass.HalfPass)
            {
              HalfPassCount++;
              LayerHalfPassCount++;
            }
            else
            {
              HalfPassCount += 2;
              LayerHalfPassCount += 2;
            }

            Count++;
          }
          else
          {
            // move end index up one as pass failed validation
            Cell.Layers[LayerIndex].EndCellPassIdx = Cell.Layers[LayerIndex].EndCellPassIdx - 1;
            if (PassIndex < CCVIdx)
              Cell.Layers[LayerIndex].CCV_CellPassIdx--; // adjust CCV_CellPassIdx accordingly
          }
        }

        // for first layer we may need to reset indexes. It use to assume start index was always 0 for first layer but that has changed since
        // Bug31595
        if (LayerIndex == 0)
        {
          // make sure first layer indexes are correct
          Cell.Layers[LayerIndex].StartCellPassIdx = 0;
          Cell.Layers[LayerIndex].EndCellPassIdx = Count - 1;
        }

        Cell.Layers[LayerIndex].FilteredPassCount = Cell.Layers[LayerIndex].PassCount;
        Cell.Layers[LayerIndex].FilteredHalfPassCount = LayerHalfPassCount;
      }

      Cell.FilteredPassCount = Count;
      Cell.FilteredHalfPassCount = HalfPassCount;
      Cell.SetFilteredPassCount(Count);

      // Remove any layers at the top of the stack that do not have any cell passes in them
      for (int LayerIndex = Cell.Layers.Count() - 1; LayerIndex >= 0; LayerIndex--)
        if (Cell.Layers[LayerIndex].EndCellPassIdx < Cell.Layers[LayerIndex].StartCellPassIdx)
          Cell.Layers.RemoveLastLayer();
        else
          break;
    }

    /// <summary>
    /// Performs extensive business rule logic analysis to the cell passes in a cell to derive layer breakdown
    /// and summary analytics for the cell
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="ClientGrid"></param>
    /// <param name="AssignmentContext"></param>
    /// <param name="cellPassIterator"></param>
    /// <param name="returnIndividualFilteredValueSelection"></param>
    /// <returns></returns>
    public bool Build(IProfileCell cell,
      // todo const LiftBuildSettings: TICLiftBuildSettings;
      IClientLeafSubGrid ClientGrid,
      FilteredValueAssignmentContext AssignmentContext, 
      ISubGridSegmentCellPassIterator cellPassIterator,
      bool returnIndividualFilteredValueSelection)
    {
      // if (Debug_ExtremeLogSwitchE)
      //  Log.LogDebug($"In BuildLiftsForCell at {Cell.OTGCellX}x{Cell.OTGCellY}");

      bool Result = false;
      NumCellPassesRemainingToFetch = 1000;

      FilteredValuePopulationControl.CalculateFlags(ProfileTypeRequired,
        // todo ... LiftBuildSettings,
        out CompactionSummaryInLiftBuildSettings,
        out WorkInProgressSummaryInLiftBuildSettings,
        out ThicknessInProgressInLiftBuildSettings);
      Cell = (ProfileCell)cell;
      Cell.ClearLayers();

      // Initialise the full and half pass counters for the client to inspect after the call
      FilteredPassCountOfTopMostLayer = 0;
      FilteredHalfCellPassCountOfTopMostLayer = 0;

      ComputedSupercededStatusForLayers = false;
      FilteredValuePopulationComplete = false;
      FilterAppliedToCellPasses = false;

      CellPassIterator = cellPassIterator;

      InitLastValueVars();
      InitLayerTrackingVars();

      if (cellPassIterator != null)
      {
        Cell.Passes.Clear();
        NumCellPassesRemainingToFetch = cellPassIterator.MaxNumberOfPassesToReturn;
      }

      if (cellPassIterator == null)
      {
        NumCellPassesRemainingToFetch = 1000; // TODO: = VLPDSvcLocations.VLPDPSNode_MaxCellPassIterationDepth_PassCountDetailAndSummary;

        SetCellIterationParameters();
        if (CellPassFastEventLookerUpper != null)
        {
          CellPassFastEventLookerUpper.PopulateFilteredValues(Cell.Passes.FilteredPassData, 0,
            Cell.Passes.PassCount - 1, PopulationControl, false);
          FilteredValuePopulationComplete = true;
        }
        else
          Debug.Assert(false, "CellPassFastEventLookerUpper not available");
      }
      else
      {
        // PopulateFilteredValues is used on a cell by cell basis to determine the required values
      }

      // This is the main loop in the layer analysis processing. It moves through each
      // cell pass in a forwards in time direction and determines
      // if each cell pass should be included in the layer being constructed from the
      // cell passes
      // Optionally a client grid and a filter context may be provided which indicates that
      // a single filtered value is the required result (ie: the lift building is used to
      // determine the superseded status of cell pass layers to aid in the filtered value
      // selection).

      // if (Debug_ExtremeLogSwitchE)
      //  Log.LogDebug("In BuildLiftsForCell at {Cell.OTGCellX}x{Cell.OTGCellY}");

      if (cellPassIterator != null)
      {
        /*
        1.	When assembling passes in ICProfile apply a filter that contains those filter
            aspects that do not require any of the events to be looked up (time, machine ID etc).
            Only then look up event.
        2.	When assembling passes in ICProfile that pass the above test, assemble all passes
            into a single list and then apply an optimized LookerUpper across all passes to
            compute the events, then run the filter checking the machine event relevant
            filter settings across those cell passes.
        3.	If there is a restriction in the number of cell passes to be returned, construct
            blocks of the cell passes assembled in #2 above and run the looker upper and
            filter over those until the maximum number of cell passes is achieved.
        4.	Perform layer analysis etc on the result of optimizations 1 to 3 above.
        */

        // Build the list of cell passes to be processed into layers

        // Create a singleton/temporary array of cell passes and filtered pass flags to support the 
        // first stage of call pass collation ready for analysis.
        if (TempPasses == null)
        {
          TempPassesSize = Math.Min(1000, NumCellPassesRemainingToFetch);
          TempPassesSizeDiv2 = TempPassesSize / 2;

          TempPasses = new FilteredPassData[TempPassesSize];
          TempFilteredPassFlags = new bool[TempPassesSize];
        }

        // Override the cell pass iteration direction to backwards as this allows
        // stopping reading of cell passes once the maximum number of passes has been read
        cellPassIterator.SegmentIterator.IterationDirection = IterationDirection.Backwards;

        BeginCellPassIteration();

        //with Cell, Passes, PassFilter do
        //{
        if (Cell.FilteredPassFlags.Length < 100)
          Array.Resize(ref Cell.FilteredPassFlags, 100);

        Cell.FilteredPassCount = 0;
        Cell.FilteredHalfPassCount = 0;

        do
        {
          TempPassCount = 0;
          ReadCellPassIntoTempList = false;

          //with CellPassIterator do
          while (CellPassIterator.MayHaveMoreFilterableCellPasses())
          {
            ReadCellPassIntoTempList = CellPassIterator.GetNextCellPass(ref Pass);

            if (!ReadCellPassIntoTempList)
              break;

            TempFilteredPassFlags[TempPassCount] = PassFilter.FilterPass_NoMachineEvents(Pass);
            TempPasses[TempPassCount].FilteredPass = Pass;

            TempPassCount++;

            if (TempPassCount >= TempPassesSizeDiv2)
              break;
          }

          // Extract all events for pre-filtered list
          // if (Debug_ExtremeLogSwitchF)
          //   Log.LogDebug("In BuildLiftsForCell at {Cell.OTGCellX}x{Cell.OTGCellY}: About to PopulateFilteredValues");

          CellPassFastEventLookerUpper.PopulateFilteredValues(TempPasses, TempPassCount - 1, 0, PopulationControl, false);

          // if (Debug_ExtremeLogSwitchF)
          //   Log.LogDebug("In BuildLiftsForCell at {Cell.OTGCellX}x{Cell.OTGCellY}: PopulateFilteredValues complete");

          // Construct the list of filtered cell passes ready to have layer analysis performed on them
          // filter them on any required machine events in the process
          for (int I = 0; I < TempPassCount; I++)
          {
            Cell.Passes.AddPass(TempPasses[I], false);

            if (Cell.Passes.PassCount == Cell.FilteredPassFlags.Length)
              Array.Resize(ref Cell.FilteredPassFlags, Cell.FilteredPassFlags.Length + 100);

            if (TempFilteredPassFlags[I]) // if valid pass
            {
              Cell.FilteredPassFlags[Cell.Passes.PassCount - 1] =
                PassFilter.FilterPass_MachineEvents(ref TempPasses[I]);
              if (Cell.FilteredPassFlags[Cell.Passes.PassCount - 1])
              {
                Cell.FilteredPassCount++;
                if (TempPasses[I].FilteredPass.HalfPass ||
                    MachineTypeUtilities.IsHalfPassCompactorMachine(TempPasses[I].MachineType))
                  Cell.FilteredHalfPassCount++;
                else
                  Cell.FilteredHalfPassCount += 2; // record as a whole pass
              }
            }
            else
              Cell.FilteredPassFlags[Cell.Passes.PassCount - 1] = false;

            if (Cell.FilteredPassCount >= NumCellPassesRemainingToFetch)
              break;
          }
        } while (!(!ReadCellPassIntoTempList || Cell.FilteredPassCount >= NumCellPassesRemainingToFetch));

        // if Debug_ExtremeLogSwitchE
        // Log.LogDebug($"In BuildLiftsForCell TotalPasses:{TempPassCount} Valid:{FilteredHalfPassCount div 2");

        // Reverse the order of the selected passes to allow layer analysis to proceed in the
        // standard fashion
        if (Cell.Passes.PassCount > 0)
        { 
          for (int I = 0; I < Cell.Passes.PassCount / 2; I++)
          {
            TempPass = Cell.Passes.FilteredPassData[I];

            Cell.Passes.FilteredPassData[I] = Cell.Passes.FilteredPassData[Cell.Passes.PassCount - I - 1];
            Cell.Passes.FilteredPassData[Cell.Passes.PassCount - I - 1] = TempPass;

            MinMax.Swap(ref Cell.FilteredPassFlags[I], ref Cell.FilteredPassFlags[Cell.Passes.PassCount - I - 1]);
          }
        }

        // Todo... if (VLPDSvcLocations.Debug_LogLiftAnalysisCellPassIteratorRestriction)
        //  if (Cell.FilteredPassCount >= NumCellPassesRemainingToFetch) && ReadCellPassIntoTempList)
        //    SIGLogMessage.PublishNoODS(Nil, Format( 'BuldLiftsForCell: Terminating scan as max filtered cell pass limit reached (%d vs %d) after scanning %d segments and %d passes in total', [Cell.FilteredPassCount, NumCellPassesRemainingToFetch, 
        //                           CellPassIterator.SegmentIterator.NumberOfSegmentsScanned, Cell.Passes.PassCount]), slmcDebug);

        // Discard the cell pass iterator - it is not longer needed
        CellPassIterator = null;
        FilteredValuePopulationComplete = true;
        FilterAppliedToCellPasses = true;

        // Set up cell iteration based on the list of cell passes that have been assembled
        SetCellIterationParameters();
      }
      else
      {
        if (Cell.Passes.PassCount > Cell.FilteredPassFlags.Length)
          Array.Resize(ref Cell.FilteredPassFlags, Cell.Passes.PassCount);
      }

      BeginCellPassIteration();

      // if (Debug_ExtremeLogSwitchF)
      //   Log.LogDebug("In BuildLiftsForCell at {Cell.OTGCellX}x{Cell.OTGCellY}: Entering main loop");

      while (SelectCurrentCellPassInScan())
      {
        // if (Debug_ExtremeLogSwitchF)
        //   Log.LogDebug("In BuildLiftsForCell at {Cell.OTGCellX}x{Cell.OTGCellY}: CheckLayerCompleted");

        CheckLayerCompleted(); // closes layer if true

        // if (Debug_ExtremeLogSwitchF)
        //   Log.LogDebug("In BuildLiftsForCell at {Cell.OTGCellX}x{Cell.OTGCellY}: AddValidatedPassToLayer");

        AddValidatedPassToLayer();

        // if (Debug_ExtremeLogSwitchF)
        //   Log.LogDebug("In BuildLiftsForCell at {Cell.OTGCellX}x{Cell.OTGCellY}: MoveToNextCurrentPass");

        MoveToNextCurrentPass();
      }

      // if (Debug_ExtremeLogSwitchE)
      //   Log.LogDebug($" BuildLiftsForCell at {Cell.OTGCellX}x{Cell.OTGCellY}: Adding final layer");

      // If we haven't added the lift we've been working on to the profile cell,  do so now
      if (CurrentLayerNotAddedToCell)
        AddFinishedLayer();

      // if (Debug_ExtremeLogSwitchE)
      //   Log.LogDebug($" BuildLiftsForCell at {Cell.OTGCellX}x{Cell.OTGCellY}: Checking if need to select filtered pass");

      MainValueAquired = false;

      // Check to see if we still need to select a filtered pass... Skip back through the
      // layers avoiding superceded layers for value selection
      if (!Result && returnIndividualFilteredValueSelection && ClientGrid != null)
      {
        // ReSharper disable once UseMethodAny.0
        if (ValidPassesExist && Cell.Layers.Count() > 0 && AssignmentContext != null)
        {
          AssignmentContext.FilteredValue.FilteredPassData.Clear();
          AssignmentContext.PreviousFilteredValue.FilteredPassData.Clear();

          //Reinitialize speed values to search for min&max speed
          Cell.CellMinSpeed = CellPassConsts.NullMachineSpeed;
          Cell.CellMaxSpeed = 0;

          // Calculate the superceded and layer thickness information for the computed layers
          if (Dummy_LiftBuildSettings.IncludeSuperseded == false)
            ComputeSupercededStatusForLayers();

          for (int I = Cell.Layers.Count() - 1; I >= 0; I--)
          {
            if ((Cell.Layers[I].Status & LayerStatus.Superseded) != 0)
              continue;

            int FilteredPassIndex = Cell.Layers[I].EndCellPassIdx;

            LayerContainsAFilteredPass = false;

            // Work through all passes in layer starting at EndCellPassIdx
            do
            {
              if ((FilterAppliedToCellPasses ||
                   PassFilter.FilterPass(ref Cell.Passes.FilteredPassData[FilteredPassIndex])) &&
                  (!FilterAppliedToCellPasses || Cell.FilteredPassFlags[FilteredPassIndex]))
              {
                LayerContainsAFilteredPass = true;
                if (!ClientGrid.AssignableFilteredValueIsNull(ref Cell.Passes.FilteredPassData[FilteredPassIndex]))
                  Result = true;
              }

              if (Result)
              {
                if (!MainValueAquired)
                {
                  AssignmentContext.FilteredValue.FilteredPassData = Cell.Passes.FilteredPassData[FilteredPassIndex];
                  MainValueAquired = true;
                  if (ClientGrid.GridDataType == GridDataType.CCVPercentChange ||
                      ClientGrid.GridDataType == GridDataType.CellProfile ||
                      ClientGrid.GridDataType == GridDataType.CCVPercentChangeIgnoredTopNullValue)
                  {
                    Result = false;

                    if (ClientGrid.GridDataType == GridDataType.CCVPercentChange ||
                        ClientGrid.GridDataType == GridDataType.CCVPercentChangeIgnoredTopNullValue)
                      ((ClientCMVLeafSubGrid) ClientGrid).IgnoresNullValueForLastCMV = false;
                    FilteredPassIndex++;
                  }
                }
                else
                {
                  if (ClientGrid.GridDataType == GridDataType.CCVPercentChange ||
                      ClientGrid.GridDataType == GridDataType.CellProfile ||
                      ClientGrid.GridDataType == GridDataType.CCVPercentChangeIgnoredTopNullValue)
                  {
                    AssignmentContext.PreviousFilteredValue.FilteredPassData.Assign(
                      Cell.Passes.FilteredPassData[FilteredPassIndex]);
                    FilteredPassIndex--;
                  }
                }

                if (ClientGrid.GridDataType == GridDataType.MachineSpeedTarget)
                {
                  Cell.AnalyzeSpeedTargets(Cell.Passes.FilteredPassData[FilteredPassIndex].FilteredPass.MachineSpeed);

                  //Force spinning here till we reach end of cellpasses
                  Result = false;
                  FilteredPassIndex--;
                }
              }
              else
                FilteredPassIndex--;
            } while (!(Result || (FilteredPassIndex == Cell.Layers[I].StartCellPassIdx - 1)));

            if (Result)
              break;

            if (LayerContainsAFilteredPass) // CCA not catered for here with settings
            {
              if ((ClientGrid.GridDataType == GridDataType.CCV && Dummy_LiftBuildSettings.CCVSummarizeTopLayerOnly &&
                   Dummy_LiftBuildSettings.CCVSummaryTypes != 0)
                  ||
                  (ClientGrid.GridDataType == GridDataType.MDP && ProfileTypeRequired == GridDataType.MDP &&
                   Dummy_LiftBuildSettings.MDPSummarizeTopLayerOnly && Dummy_LiftBuildSettings.MDPSummaryTypes != 0)
                  ||
                  (ClientGrid.TopLayerOnly)
                  ||
                  ((ClientGrid.GridDataType == GridDataType.CCVPercentChangeIgnoredTopNullValue ||
                    ClientGrid.GridDataType == GridDataType.CCVPercentChange) &&
                   Dummy_LiftBuildSettings.CCVSummarizeTopLayerOnly))
              {
                // For CCV and MDP, if we are calculating summary information and the current settings
                // are to examine only the top most layer, then there is no need to examine any further layers in the stack
                break;
              }
            }
          }
        }
      }

      if (ClientGrid != null)
        if (ClientGrid.GridDataType == GridDataType.CCVPercentChange ||
            ClientGrid.GridDataType == GridDataType.CCVPercentChangeIgnoredTopNullValue)
          (ClientGrid as ClientCMVLeafSubGrid).RestoreInitialSettings();

      if (MainValueAquired)
        Result = true;

      // if (Debug_ExtremeLogSwitchE)
      //   Log.LogDebug($" BuildLiftsForCell at {Cell.OTGCellX}x{Cell.OTGCellY}: Calcing superceded etc");

      // Result is not true (ie: earlier processing has not decided that everything is all good)
      // then check to see if there were valid passes in the computed profile. This is tempered
      // if the caller has requested a filtered value rather than the analysis of the layers as
      // the desired result. In this case, if the requested value is a pass count then this is OK
      // (as we are not after an attribute but rather information about the cell pass stack),
      // but not if an attribute value has been requested, and none was selected during the cell
      // layers processing.)

      if (!Result)
        Result = ValidPassesExist &&
                 ((ProfileTypeRequired == GridDataType.All || ProfileTypeRequired == GridDataType.PassCount) ||
                  !returnIndividualFilteredValueSelection);

      if (Result)
      {
        // Calculate the superseded and layer thickness information for the computed layers
        if (Dummy_LiftBuildSettings.IncludeSuperseded == false)
          ComputeSupercededStatusForLayers();

        // Todo: See NormalizeLayersMaxThickness() in SVOICProfileCell.pas in Raptor source
        // Cell.NormalizeLayersMaxThickness(Dummy_LiftBuildSettings.FirstPassThickness);

        // Todo ... layer thickness computation not included in TRex yet
        // ComputeLayerThicknessForLayers();
      }

      // if (Debug_ExtremeLogSwitchE)
      //   Log.LogDebug($" BuildLiftsForCell at {Cell.OTGCellX}x{Cell.OTGCellY}: Handling pass count check");


      ApplyPassCountRangeFilter();

      // Apply the Elevation Type filter if any...
      ApplyElevationTypeFilter();

      // Remove all the non-filtered passes from the passes that were used to perform
      // the layer analysis and make sure indexing is correct in layers
      RemoveNonFilteredPasses();

      // If the caller is really just interested in the pass count of the topmost (most
      // recent) layer in the processed lifts, then count the number of cell passes in
      // the layer that match the filter and return that value to the caller.
      // Note: If there are no filters selections, or only a time filter is configured,
      // then the number of filtered passes in the top layer is the cell pass count for
      // the top layer
      if (Result &&
          (ProfileTypeRequired == GridDataType.All ||
           ProfileTypeRequired == GridDataType.PassCount ||
           ProfileTypeRequired == GridDataType.CellProfile ||
           ProfileTypeRequired == GridDataType.CellPasses))
      {
        // ReSharper disable once UseMethodAny.0
        if (Cell.Layers.Count() > 0)
        {
          if (FilterAppliedToCellPasses || PassFilter == null)
          {
            FilteredPassCountOfTopMostLayer = Cell.Layers.Last().FilteredPassCount;
            FilteredHalfCellPassCountOfTopMostLayer = Cell.Layers.Last().FilteredHalfPassCount;
          }
          else
          {
            for (int PassIndex = Cell.Layers.Last().StartCellPassIdx;
              PassIndex < Cell.Layers.Last().EndCellPassIdx;
              PassIndex++)
              if (PassFilter.FilterPass(ref Cell.Passes.FilteredPassData[PassIndex]))
              {
                FilteredPassCountOfTopMostLayer++;
                if (Cell.Passes.FilteredPassData[PassIndex].FilteredPass.HalfPass ||
                    MachineTypeUtilities.IsHalfPassCompactorMachine(Cell.Passes.FilteredPassData[PassIndex].MachineType))
                  FilteredHalfCellPassCountOfTopMostLayer++;
                else
                  FilteredHalfCellPassCountOfTopMostLayer += 2;
              }

            Cell.Layers.Last().FilteredPassCount = FilteredPassCountOfTopMostLayer;
            Cell.Layers.Last().FilteredHalfPassCount = FilteredHalfCellPassCountOfTopMostLayer;
          }
        }
      }

      /* todo ... use VLPDSvcLocations.VLPDPSNode_EmitSubgridCellPassCounts
      if (VLPDSvcLocations.VLPDPSNode_EmitSubgridCellPassCounts)
      {
        if (MaxCellPassListAggregationSize < Cell.Passes.PassCount)
        {
          MaxCellPassListAggregationSize = Cell.Passes.PassCount;
          Log.LogDebug($"Max cell pass aggregation count: {MaxCellPassListAggregationSize}");
        }
      }
      */

      // if Debug_ExtremeLogSwitchE
      //   Log.LogDebug($"Out BuildLiftsForCell at {Cell.OTGCellX}x{Cell.OTGCellY}");

      return true;
    }
  }
}
