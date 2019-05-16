using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VSS.MasterData.Models.Models;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Types;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Filters.Models;
using VSS.TRex.Machines;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities;
using ElevationType = VSS.TRex.Common.Types.ElevationType;

namespace VSS.TRex.Filters
{
  /*
  This unit defines support for filtering information stored in the data grid.

   There are two varieties of filtering used.
     - Cell selection filtering

       Based on:
         Spatial: Arbitrary fence specifying inclusion area
         Positional: Point and radius for inclusion area


       The result of ElevationRangeOffset filter is <YES> the cell may be used for cell pass
       filtering, or<NO> the cell should not be considered for cell pass
       filtering.

     - Cell pass filtering
       Intended to choose a single cell pass from the cell passes collected for a
       cell.

         Based on:
            - Machine(s)
            - Time ranges
            - many other attributes

       The result of this filter is either <NOTHING>, or a single cell pass.

   Filtering is accomplished via the use of filters (a set of parameters that
   govern how cells are to be included/excluded from the filter).
   */

  /// <summary>
  /// CellPassAttributeFilter provides filtering support for grid data requested by the client
  /// </summary>
  public class CellPassAttributeFilter : CellPassAttributeFilterModel, ICellPassAttributeFilter
  {
    private ISiteModel siteModel;

    /// <summary>
    /// Owner is the SiteModel instance to which this filter relates and is used in cases where machine related
    /// attributes are included in the filter
    /// </summary>
    public object /*ISiteModel*/ SiteModel
    {
      get => siteModel; 
      set => siteModel = (ISiteModel) value; 
    }

    /// <summary>
    /// The machines present in the filter represented as a bit set
    /// </summary>
    protected BitArray MachineIDSet { get; private set; }

    private bool _anyFilterSelections;
    public bool AnyFilterSelections => _prepared ? _anyFilterSelections : Prepare() && _anyFilterSelections;

    private bool _anyMachineEventFilterSelections;
    public bool AnyMachineEventFilterSelections => _prepared ? _anyMachineEventFilterSelections : Prepare() && _anyMachineEventFilterSelections;

    private bool _anyNonMachineEventFilterSelections;
    public bool AnyNonMachineEventFilterSelections => _prepared ? _anyNonMachineEventFilterSelections : Prepare() && _anyNonMachineEventFilterSelections;

    /// <summary>
    /// Default no-arg constructor the produces a filter with all aspects set to their defaults
    /// </summary>
    public CellPassAttributeFilter()
    {
      ClearFilter();
    }

    /// <summary>
    /// Performs operations that prepares the filter for active use. Prepare() must be called prior to
    /// active use of the filter.
    /// </summary>
    private bool Prepare()
    {
      _anyFilterSelections =
        HasCompactionMachinesOnlyFilter ||
        HasDesignFilter ||
        HasElevationRangeFilter ||
        HasElevationTypeFilter ||
        HasGCSGuidanceModeFilter ||
        HasGPSAccuracyFilter ||
        HasGPSToleranceFilter ||
        HasLayerIDFilter ||
        HasLayerStateFilter ||
        HasMachineDirectionFilter ||
        HasMachineFilter ||
        HasElevationMappingModeFilter ||
        HasPassTypeFilter ||
        HasPositioningTechFilter ||
        HasTimeFilter ||
        HasVibeStateFilter ||
        HasTemperatureRangeFilter ||
        HasPassCountRangeFilter;

      _anyMachineEventFilterSelections =
        HasDesignFilter ||
        HasVibeStateFilter ||
        HasMachineDirectionFilter ||
        HasElevationMappingModeFilter ||
        HasGCSGuidanceModeFilter ||
        HasGPSAccuracyFilter ||
        HasGPSToleranceFilter ||
        HasPositioningTechFilter ||
        HasLayerIDFilter ||
        HasPassTypeFilter;

      _anyNonMachineEventFilterSelections =
        HasTimeFilter ||
        HasMachineFilter ||
        HasElevationRangeFilter ||
        HasCompactionMachinesOnlyFilter ||
        HasTemperatureRangeFilter;

      _ = GetMachineIDsSet();

      _prepared = true;
      return true;
    }

    // Clear all the elements of the filter to a null state
    public void ClearFilter()
    {
      ClearDesigns();
      ClearMachines();
      ClearTime();
      ClearVibeState();
      ClearLayerState();
      ClearMachineDirection();
      ClearPassType();
      ClearMinElevationMapping();
      ClearElevationType();
      ClearGuidanceMode();
      ClearElevationRange();
      ClearCompactionMachineOnlyRestriction();
      ClearLayerID();
      ClearGPSAccuracy();
      ClearGPSTolerance();
      ClearTemperatureRange();
      ClearPassCountRange();
      ClearPositioningTech();

      ReturnEarliestFilteredCellPass = false;
      FilterTemperatureByLastPass = false;

      _anyFilterSelections = false;
      _anyMachineEventFilterSelections = false;
      _anyNonMachineEventFilterSelections = false;

      _prepared = false;
    }

    public void ClearVibeState()
    {
      HasVibeStateFilter = false;

      VibeState = VibrationState.Invalid;
    }

    /* Possibly obsolete functionality due to filter fingerprinting for cache support
    /// <summary>
    /// Compares left and right boolean expressions and returns a -1, 0, -1 relative comparison indicator
    /// </summary>
    /// <param name="Left"></param>
    /// <param name="Right"></param>
    /// <returns></returns>
    private static int FlagCheck(bool Left, bool Right) => Left ? Right ? 0 : -1 : Right ? 1 : 0;

    /// <summary>
    /// Compare two lists of machine IDs for ordering
    /// </summary>
    /// <param name="list1"></param>
    /// <param name="list2"></param>
    /// <returns></returns>
    public static int MachineIDListsComparison_Obsolete(short[] list1, short[] list2)
    {
      if (list1 == null || list2 == null)
        return 0;

      var list1Length = list1.Length;
      var list2Length = list2.Length;

      // Check list lengths
      int result = list1Length.CompareTo(list2Length);

      // If the lengths are the same check individual items
      if (result == 0)
      {
        for (int i = 0; i < list1Length; i++)
        {
          result = list1[i].CompareTo(list2[i]);

          if (result != 0)
            break;
        }
      }

      return result;
    }

    /// <summary>
    /// Compare one filter with another for the purpose of ordering them in caching lists
    /// </summary>
    /// <param name="AFilter"></param>
    /// <returns></returns>
    public int CompareTo_Obsolete(ICellPassAttributeFilter AFilter)
    {
      // Time
      int Result = FlagCheck(HasTimeFilter, AFilter.HasTimeFilter);
      if (Result != 0)
      {
        return Result;
      }

      if (HasTimeFilter) // Check the contents of the time filter
      {
        Result = StartTime.CompareTo(AFilter.StartTime);
        if (Result == 0)
          Result = EndTime.CompareTo(AFilter.EndTime);
      }

      if (Result != 0)
        return Result;

      // Designs
      Result = FlagCheck(HasDesignFilter, AFilter.HasDesignFilter);
      if (Result != 0)
        return Result;

      if (HasDesignFilter) // Check the contents of the design filter
        Result = DesignNameID.CompareTo(AFilter.DesignNameID);

      if (Result != 0)
        return Result;

      // Machines
      Result = FlagCheck(HasMachineFilter, AFilter.HasMachineFilter);
      if (Result != 0)
        return Result;

      if (HasMachineFilter) // Check the contents of the machine filter
        Result = MachineIDListsComparison_Obsolete(MachineIDs, AFilter.MachineIDs);

      if (Result != 0)
        return Result;

      // Machine direction filter
      Result = FlagCheck(HasMachineDirectionFilter, AFilter.HasMachineDirectionFilter);
      if (Result != 0)
        return Result;

      if (HasMachineDirectionFilter) // Check the contents of the machine direction filter
        Result = MachineDirection.CompareTo(AFilter.MachineDirection); // CompareValue(Ord(MachineDirection), Ord(AFilter.MachineDirection));

      if (Result != 0)
        return Result;

      // Pass Type filter
      Result = FlagCheck(HasPassTypeFilter, AFilter.HasPassTypeFilter);
      if (Result != 0)
        return Result;

      if (HasPassTypeFilter) // Check the contents of the pass type filter
        if (PassTypeSet == AFilter.PassTypeSet)
          Result = 0;
        else
          Result = -1;

      if (Result != 0)
        return Result;

      // Vibe state filter
      Result = FlagCheck(HasVibeStateFilter, AFilter.HasVibeStateFilter);
      if (Result != 0)
        return Result;

      if (HasVibeStateFilter) // Check the contents of the machine filter
        Result = VibeState.CompareTo(AFilter.VibeState); // CompareValue(Ord(VibeState), Ord(AFilter.VibeState));

      if (Result != 0)
        return Result;

      // Min elev mapping
      Result = FlagCheck(HasElevationMappingModeFilter, AFilter.HasElevationMappingModeFilter);
      if (Result != 0)
        return Result;

      if (HasElevationMappingModeFilter) // Check the contents of the min elevation filter
        Result = ElevationMappingMode.CompareTo(AFilter.ElevationMappingMode); // CompareValue(Ord(ElevationMappingMode), Ord(AFilter.ElevationMappingMode));

      if (Result != 0)
        return Result;

      // Elevation type
      Result = FlagCheck(HasElevationTypeFilter, AFilter.HasElevationTypeFilter);
      if (Result != 0)
        return Result;

      if (HasElevationTypeFilter) // Check the contents of the elevation type filter
        Result = ElevationType.CompareTo(AFilter.ElevationType); // CompareValue(Ord(ElevationType), Ord(AFilter.ElevationType));

      if (Result != 0)
        return Result;

      // Exclusion of surveyed surfaces from query
      Result = FlagCheck(ExcludeSurveyedSurfaces(), AFilter.ExcludeSurveyedSurfaces());
      if (Result != 0)
        return Result;

      // GCS Guidance mode
      Result = FlagCheck(HasGCSGuidanceModeFilter, AFilter.HasGCSGuidanceModeFilter);
      if (Result != 0)
        return Result;

      if (HasGCSGuidanceModeFilter) // Check the contents of the GPS guidance mode
        Result = GCSGuidanceMode.CompareTo(AFilter.GCSGuidanceMode); // CompareValue(Ord(GCSGuidanceMode), Ord(AFilter.GCSGuidanceMode));

      if (Result != 0)
        return Result;

      // GPS Accuracy
      Result = FlagCheck(HasGPSAccuracyFilter, AFilter.HasGPSAccuracyFilter);
      if (Result != 0)
        return Result;

      if (HasGPSAccuracyFilter) // Check the contents of the GPS accuracy filter
      {
        Result = FlagCheck(GPSAccuracyIsInclusive, AFilter.GPSAccuracyIsInclusive); // CompareValue(Ord(GPSAccuracyIsInclusive), Ord(AFilter.GPSAccuracyIsInclusive));
        if (Result == 0)
          Result = GPSAccuracy.CompareTo(AFilter.GPSAccuracy); // CompareValue(Ord(GPSAccuracy), Ord(AFilter.GPSAccuracy));
      }

      if (Result != 0)
        return Result;

      // GPS Tolerance
      Result = FlagCheck(HasGPSToleranceFilter, AFilter.HasGPSToleranceFilter);
      if (Result != 0)
        return Result;

      if (HasGPSToleranceFilter) // Check the contents of the GPS tolerance filter
      {
        Result = FlagCheck(GPSToleranceIsGreaterThan, AFilter.GPSToleranceIsGreaterThan); // CompareValue(Ord(GPSToleranceIsGreaterThan), Ord(AFilter.GPSToleranceIsGreaterThan));
        if (Result != 0)
          Result = GPSTolerance.CompareTo(AFilter.GPSTolerance); // CompareValue(GPSTolerance, AFilter.GPSTolerance);
      }

      if (Result != 0)
        return Result;

      // Positioning Tech
      Result = FlagCheck(HasPositioningTechFilter, AFilter.HasPositioningTechFilter);
      if (Result != 0)
        return Result;
      if (HasPositioningTechFilter) // Check the contents of the positioning tech filter
        Result = PositioningTech.CompareTo(AFilter.PositioningTech); //  CompareValue(Ord(PositioningTech), Ord(AFilter.PositioningTech));
      if (Result != 0)
        return Result;

      // Elevation Range
      Result = FlagCheck(HasElevationRangeFilter, AFilter.HasElevationRangeFilter);
      if (Result != 0)
        return Result;

      if (HasElevationRangeFilter) // Check the contents of the elevation range filter
        if (ElevationRangeDesignUID != Guid.Empty)
        {
          Result = ElevationRangeDesignUID.CompareTo(AFilter.ElevationRangeDesignUID);
          if (Result == 0)
            Result = ElevationRangeOffset.CompareTo(AFilter.ElevationRangeOffset);
          if (Result == 0)
            Result = ElevationRangeThickness.CompareTo(AFilter.ElevationRangeThickness);
        }
        else
        {
          Result = ElevationRangeLevel.CompareTo(AFilter.ElevationRangeLevel);
          if (Result == 0)
            Result = ElevationRangeOffset.CompareTo(AFilter.ElevationRangeOffset);
          if (Result == 0)
            Result = ElevationRangeThickness.CompareTo(AFilter.ElevationRangeThickness);
        }

      if (Result != 0)
        return Result;

      Result = FlagCheck(HasLayerStateFilter, AFilter.HasLayerStateFilter);
      if (Result != 0)
        return Result;
      if (HasLayerStateFilter)
        Result = LayerState.CompareTo(AFilter.LayerState); // CompareValue(Ord(LayerState), Ord(AFilter.LayerState));
      if (Result != 0)
        return Result;

      Result = FlagCheck(HasCompactionMachinesOnlyFilter, AFilter.HasCompactionMachinesOnlyFilter);
      // Note: The compaction machines only filter is fully described by having
      // that state in the filter - there are no additional attributes to check
      if (Result != 0)
        return Result;

      // LayerID
      Result = FlagCheck(HasLayerIDFilter, AFilter.HasLayerIDFilter);
      if (Result != 0)
        return Result;
      if (HasLayerIDFilter)
        Result = LayerID.CompareTo(AFilter.LayerID); // CompareValue(Ord(LayerID), Ord(AFilter.LayerID));
      if (Result != 0)
        return Result;

      // TemperatureRangeFilter
      Result = FlagCheck(HasTemperatureRangeFilter, AFilter.HasTemperatureRangeFilter);
      if (Result != 0)
        return Result;
      if (HasTemperatureRangeFilter)
      {
        Result = MaterialTemperatureMin.CompareTo(AFilter.MaterialTemperatureMin);
        if (Result != 0)
          return Result;
        Result = MaterialTemperatureMax.CompareTo(AFilter.MaterialTemperatureMax);
        if (Result != 0)
          return Result;
        Result = FilterTemperatureByLastPass.CompareTo(AFilter.FilterTemperatureByLastPass);
        if (Result != 0)
          return Result;
      }

      // PassCountRangeFilter
      Result = FlagCheck(HasPassCountRangeFilter, AFilter.HasPassCountRangeFilter);
      if (Result != 0)
        return Result;
      if (HasPassCountRangeFilter)
      {
        Result = PassCountRangeMin.CompareTo(AFilter.PassCountRangeMin);
        if (Result != 0)
          return Result;
        Result = PassCountRangeMax.CompareTo(AFilter.PassCountRangeMax);
        if (Result != 0)
          return Result;
      }

      // Everything is equal!
      Result = 0;

      return Result;
    }
    */

    public void ClearDesigns()
    {
      HasDesignFilter = false;
      DesignNameID = Consts.kNoDesignNameID;
    }

    public void ClearElevationRange()
    {
      HasElevationRangeFilter = false;

      ElevationRangeLevel = Consts.NullDouble;
      ElevationRangeOffset = Consts.NullDouble;
      ElevationRangeThickness = Consts.NullDouble;
      ElevationRangeDesign.DesignID = Guid.Empty;
      ElevationRangeDesign.Offset = 0;
    }

    public void ClearElevationType()
    {
      HasElevationTypeFilter = false;
      ElevationType = ElevationType.Last;
    }

    public void ClearGPSAccuracy()
    {
      HasGPSAccuracyFilter = false;

      GPSAccuracy = GPSAccuracy.Unknown;
      GPSAccuracyIsInclusive = false;
    }

    public void ClearTemperatureRange()
    {
      HasTemperatureRangeFilter = false;
      MaterialTemperatureMin = CellPassConsts.NullMaterialTemperatureValue;
      MaterialTemperatureMax = CellPassConsts.NullMaterialTemperatureValue;
    }

    public void ClearPassCountRange()
    {
      HasPassCountRangeFilter = false;
      PassCountRangeMin = 0;
      PassCountRangeMax = 0;
    }

    public void ClearGPSTolerance()
    {
      HasGPSToleranceFilter = false;
      GPSTolerance = Consts.kMaxGPSAccuracyErrorLimit;
    }

    public void ClearGuidanceMode()
    {
      HasGCSGuidanceModeFilter = false;
      GCSGuidanceMode = AutomaticsType.Unknown;
    }

    public void ClearLayerID()
    {
      HasLayerIDFilter = false;
      LayerID = CellEvents.NullLayerID;
    }

    public void ClearLayerState()
    {
      HasLayerStateFilter = false;
      LayerState = LayerState.Invalid;
    }

    public void Assign(ICellPassAttributeFilter Source)
    {
      SiteModel = Source.SiteModel;

      // Time based filtering members
      StartTime = Source.StartTime;
      EndTime = Source.EndTime;

      // Machine based filtering members
      var machinesCount = Source.MachinesList?.Length ?? 0;
      MachinesList = new Guid[machinesCount];
      if (machinesCount > 0)
        Array.Copy(Source.MachinesList, MachinesList, machinesCount);

      MachineIDSet = null;

      // Design based filtering member
      DesignNameID = Source.DesignNameID;

      // Auto Vibe state filtering member
      VibeState = Source.VibeState;

      // how to build layers
      LayerState = Source.LayerState;

      MachineDirection = Source.MachineDirection;

      PassTypeSet = Source.PassTypeSet;
      ElevationMappingMode = Source.ElevationMappingMode;

      PositioningTech = Source.PositioningTech;
      GPSTolerance = Source.GPSTolerance;
      GPSAccuracy = Source.GPSAccuracy;
      GPSAccuracyIsInclusive = Source.GPSAccuracyIsInclusive;
      GPSToleranceIsGreaterThan = Source.GPSToleranceIsGreaterThan;

      ElevationType = Source.ElevationType;

      GCSGuidanceMode = Source.GCSGuidanceMode;

      // FReturnEarliestFilteredCellPass details how we choose a cell pass from a set of filtered
      // cell passes within a cell. If set, then the first cell pass is chosen. If not set, then
      // the latest cell pass is chosen
      ReturnEarliestFilteredCellPass = Source.ReturnEarliestFilteredCellPass;

      ElevationRangeLevel = Source.ElevationRangeLevel;
      ElevationRangeOffset = Source.ElevationRangeOffset;
      ElevationRangeThickness = Source.ElevationRangeThickness;
      ElevationRangeDesign.DesignID = Source.ElevationRangeDesign.DesignID;
      ElevationRangeDesign.Offset = Source.ElevationRangeDesign.Offset;

      LayerID = Source.LayerID;

      MaterialTemperatureMin = Source.MaterialTemperatureMin;
      MaterialTemperatureMax = Source.MaterialTemperatureMax;
      FilterTemperatureByLastPass = Source.FilterTemperatureByLastPass;
      PassCountRangeMin = Source.PassCountRangeMin;
      PassCountRangeMax = Source.PassCountRangeMax;

      int SurveyedSurfaceExclusionCount = Source.SurveyedSurfaceExclusionList?.Length ?? 0;
      SurveyedSurfaceExclusionList = new Guid[SurveyedSurfaceExclusionCount];
      if (Source.SurveyedSurfaceExclusionList != null)
        Array.Copy(Source.SurveyedSurfaceExclusionList, SurveyedSurfaceExclusionList, SurveyedSurfaceExclusionCount);

      HasTimeFilter = Source.HasTimeFilter;
      HasMachineFilter = Source.HasMachineFilter;
      HasMachineDirectionFilter = Source.HasMachineDirectionFilter;
      HasDesignFilter = Source.HasDesignFilter;
      HasVibeStateFilter = Source.HasVibeStateFilter;
      HasLayerStateFilter = Source.HasLayerStateFilter;
      HasElevationMappingModeFilter = Source.HasElevationMappingModeFilter;
      HasElevationTypeFilter = Source.HasElevationTypeFilter;
      HasGCSGuidanceModeFilter = Source.HasGCSGuidanceModeFilter;
      HasGPSAccuracyFilter = Source.HasGPSAccuracyFilter;
      HasGPSToleranceFilter = Source.HasGPSToleranceFilter;
      HasPositioningTechFilter = Source.HasPositioningTechFilter;
      HasLayerIDFilter = Source.HasLayerIDFilter;
      HasElevationRangeFilter = Source.HasElevationRangeFilter;
      HasPassTypeFilter = Source.HasPassTypeFilter;
      HasCompactionMachinesOnlyFilter = Source.HasCompactionMachinesOnlyFilter;
      HasTemperatureRangeFilter = Source.HasTemperatureRangeFilter;
      HasPassCountRangeFilter = Source.HasPassCountRangeFilter;

      Prepare();
    }

    public void ClearCompactionMachineOnlyRestriction()
    {
      HasCompactionMachinesOnlyFilter = false;
    }

    public void ClearMachineDirection()
    {
      HasMachineDirectionFilter = false;
      MachineDirection = MachineDirection.Unknown;
    }

    public void ClearMachines()
    {
      HasMachineFilter = false;
      MachinesList = new Guid[0];
      MachineIDSet = null;
    }

    public void ClearMinElevationMapping()
    {
      HasElevationMappingModeFilter = false;
      ElevationMappingMode = ElevationMappingMode.LatestElevation;
    }

    public void ClearPassType()
    {
      HasPassTypeFilter = false;
      PassTypeSet = PassTypeSet.None;
    }

    public void ClearPositioningTech()
    {
      HasPositioningTechFilter = false;
      PositioningTech = PositioningTech.Unknown;
    }

    public void ClearSurveyedSurfaceExclusionList()
    {
      SurveyedSurfaceExclusionList = new Guid[0];
    }

    public void ClearTime()
    {
      HasTimeFilter = false;

      StartTime = Consts.MIN_DATETIME_AS_UTC;
      EndTime = Consts.MAX_DATETIME_AS_UTC;
    }

    private static readonly GPSAccuracyAndTolerance NullGPSAccuracyAndToleranceValue = GPSAccuracyAndTolerance.Null();

    // Returns true if the specified pass meets the set filter (if any)
    // FilterPass determines if a single pass conforms to the current filtering configuration
    public bool FilterPass(ref CellPass PassValue, ICellPassAttributeFilterProcessingAnnex filterAnnex)
    {
      if (!AnyFilterSelections)
      {
        // There are no constrictive filter criteria - all cell passes pass the filter
        return false;
      }

      if (HasTimeFilter)
      {
        if (!FilterPassUsingTimeOnly(ref PassValue))
          return false;
      }

      if (HasElevationRangeFilter)
      {
        if (!filterAnnex.FilterPassUsingElevationRange(ref PassValue))
          return false;
      }

      if (HasMachineFilter)
      {
        // Check the machine identified by PassValue.InternalSiteModelMachineIndex is in our site model Machine
        // list based on the index of the machine in that list

        if (PassValue.InternalSiteModelMachineIndex < MachineIDSet.Count && !MachineIDSet[PassValue.InternalSiteModelMachineIndex])
          return false;
      }

      if (HasCompactionMachinesOnlyFilter)
      {
        IMachine Machine = siteModel.Machines[PassValue.InternalSiteModelMachineIndex];

        if (Machine != null && !Machine.MachineIsCompactorType())
          return false;
      }

      if (AnyMachineEventFilterSelections)
      {
        // Extract the list of events for the machine referred to in the cell pass. Use this
        // reference for all filter criteria that depend on machine events
        var machineTargetValues = siteModel.MachinesTargetValues[PassValue.InternalSiteModelMachineIndex];

        if (HasDesignFilter)
        {
          int DesignNameIDValue = machineTargetValues.MachineDesignNameIDStateEvents.GetValueAtDate(PassValue.Time, out _, Consts.kNoDesignNameID);

          if (DesignNameIDValue != Consts.kAllDesignsNameID && DesignNameID != DesignNameIDValue)
            return false;
        }

        if (HasVibeStateFilter)
        {
          VibrationState VibeStateValue = machineTargetValues.VibrationStateEvents.GetValueAtDate(PassValue.Time, out _, VibrationState.Invalid);

          if (VibeState != VibeStateValue)
            return false;
        }

        if (HasGCSGuidanceModeFilter)
        {
          AutomaticsType GCSGuidanceModeValue = machineTargetValues.MachineAutomaticsStateEvents.GetValueAtDate(PassValue.Time, out _, AutomaticsType.Unknown);

          if (GCSGuidanceMode != GCSGuidanceModeValue)
            return false;
        }

        if (HasMachineDirectionFilter)
        {
          MachineGear MachineGearValue = machineTargetValues.MachineGearStateEvents.GetValueAtDate(PassValue.Time, out _, MachineGear.Null);

          if ((MachineDirection == MachineDirection.Forward && !Machine.MachineGearIsForwardGear(MachineGearValue)) ||
              (MachineDirection == MachineDirection.Reverse && !Machine.MachineGearIsReverseGear(MachineGearValue)))
            return false;
        }

        if (HasElevationMappingModeFilter)
        {
          ElevationMappingMode ElevationMappingModeValue = machineTargetValues.ElevationMappingModeStateEvents.GetValueAtDate(PassValue.Time, out _, ElevationMappingMode.LatestElevation);

          if (ElevationMappingMode != ElevationMappingModeValue)
            return false;
        }

        if (HasGPSAccuracyFilter || HasGPSToleranceFilter)
        {
          var GPSAccuracyAndToleranceValue = machineTargetValues.GPSAccuracyAndToleranceStateEvents.GetValueAtDate(PassValue.Time, out _, NullGPSAccuracyAndToleranceValue);

          if (HasGPSAccuracyFilter && GPSAccuracy != GPSAccuracyAndToleranceValue.GPSAccuracy && !GPSAccuracyIsInclusive)
            return false;

          if (HasGPSAccuracyFilter && GPSAccuracyIsInclusive && GPSAccuracy < GPSAccuracyAndToleranceValue.GPSAccuracy)
            return false;

          if (HasGPSToleranceFilter &&
              !(GPSAccuracyAndToleranceValue.GPSTolerance != CellPassConsts.NullGPSTolerance &&
                ((!GPSToleranceIsGreaterThan && GPSAccuracyAndToleranceValue.GPSTolerance < GPSTolerance) ||
                 (GPSToleranceIsGreaterThan && GPSAccuracyAndToleranceValue.GPSTolerance >= GPSTolerance))))
            return false;
        }

        if (HasPositioningTechFilter)
        {
          PositioningTech PositioningTechStateValue = machineTargetValues.PositioningTechStateEvents.GetValueAtDate(PassValue.Time, out _, PositioningTech.Unknown);

          if (PositioningTech != PositioningTechStateValue)
            return false;
        }

        // Filter on LayerID
        if (HasLayerIDFilter)
        {
          ushort LayerIDStateValue = machineTargetValues.LayerIDStateEvents.GetValueAtDate(PassValue.Time, out _, ushort.MaxValue);
          if (LayerID != LayerIDStateValue)
            return false;
        }
      }

      // Filter on PassType
      if (HasPassTypeFilter)
      {
        if (!PassTypeHelper.PassTypeSetContains(PassTypeSet, PassValue.PassType))
          return false;
      }

      // TemperatureRange
      if (HasTemperatureRangeFilter & !FilterTemperatureByLastPass)
      {
        if (!FilterPassUsingTemperatureRange(ref PassValue))
          return false;
      }

      return true;
    }

    public bool FilterPass(ref FilteredPassData PassValue, ICellPassAttributeFilterProcessingAnnex filterAnnex)
    {
      if (!AnyFilterSelections)
        return true;

      if (HasTimeFilter)
      {
        if (!FilterPassUsingTimeOnly(ref PassValue.FilteredPass))
          return false;
      }

      if (HasElevationRangeFilter)
        if (!filterAnnex.FilterPassUsingElevationRange(ref PassValue.FilteredPass))
          return false;

      if (HasMachineFilter)
      {
        // Check the machine identified by PassValue.InternalSiteModelMachineIndex is in our site model Machine
        // list based on the index of the machine in that list

        if (PassValue.FilteredPass.InternalSiteModelMachineIndex < MachineIDSet.Count && !MachineIDSet[PassValue.FilteredPass.InternalSiteModelMachineIndex])
          return false;
      }

      if (HasCompactionMachinesOnlyFilter)
      {
        IMachine Machine = siteModel.Machines[PassValue.FilteredPass.InternalSiteModelMachineIndex];
        if (Machine != null && !Machine.MachineIsCompactorType())
          return false;
      }

      if (HasDesignFilter)
        if (DesignNameID != Consts.kAllDesignsNameID && DesignNameID != PassValue.EventValues.EventDesignNameID)
          return false;

      if (HasVibeStateFilter)
      {
        if (VibeState != PassValue.EventValues.EventVibrationState)
          return false;
      }

      if (HasMachineDirectionFilter)
      {
        if ((MachineDirection == MachineDirection.Forward && !Machines.Machine.MachineGearIsForwardGear(PassValue.EventValues.EventMachineGear)) ||
            (MachineDirection == MachineDirection.Reverse && !Machines.Machine.MachineGearIsReverseGear(PassValue.EventValues.EventMachineGear)))
          return false;
      }

      if (HasElevationMappingModeFilter)
      {
        if (ElevationMappingMode != PassValue.EventValues.EventElevationMappingMode)
          return false;
      }

      if (HasGCSGuidanceModeFilter)
      {
        if (GCSGuidanceMode != PassValue.EventValues.EventMachineAutomatics)
          return false;
      }

      if (HasGPSAccuracyFilter)
      {
        if (GPSAccuracy != PassValue.EventValues.GPSAccuracy && !GPSAccuracyIsInclusive)
          return false;

        if (GPSAccuracyIsInclusive && GPSAccuracy < PassValue.EventValues.GPSAccuracy)
          return false;
      }

      if (HasGPSToleranceFilter)
      {
        if (!((PassValue.EventValues.GPSTolerance != CellPassConsts.NullGPSTolerance) &&
              ((GPSToleranceIsGreaterThan && PassValue.EventValues.GPSTolerance >= GPSTolerance) ||
               (!GPSToleranceIsGreaterThan && PassValue.EventValues.GPSTolerance < GPSTolerance))))
          return false;
      }

      if (HasPositioningTechFilter)
      {
        if (PositioningTech != PassValue.EventValues.PositioningTechnology)
          return false;
      }

      // Filter on LayerID
      if (HasLayerIDFilter)
      {
        if (LayerID != PassValue.EventValues.LayerID)
          return false;
      }

      // Filter on PassType
      if (HasPassTypeFilter)
      {
        if (!PassTypeHelper.PassTypeSetContains(PassTypeSet, PassValue.FilteredPass.PassType)) // maybe if noting set you may want ptFront as a default pass
          return false;
      }

      // TemperatureRange
      if (HasTemperatureRangeFilter & !FilterTemperatureByLastPass)
      {
        if (!FilterPassUsingTemperatureRange(ref PassValue.FilteredPass))
          return false;
      }

      return true;
    }

    public bool FilterPassUsingTemperatureRange(ref CellPass PassValue)
    {
      Debug.Assert(HasTemperatureRangeFilter, "Temperature range filter being used without the temperature range data being initialized");
      return (PassValue.MaterialTemperature != CellPassConsts.NullMaterialTemperatureValue) &&
             Range.InRange(PassValue.MaterialTemperature, MaterialTemperatureMin, MaterialTemperatureMax);
    }

    public bool FilterPassUsingTimeOnly(ref CellPass PassValue)
    {
      if (StartTime == Consts.MIN_DATETIME_AS_UTC)
      {
        // It's an End/As At time filter
        if (PassValue.Time > EndTime)
          return false;
      }

      // In that case it's a time range filter
      if (PassValue.Time < StartTime || PassValue.Time > EndTime)
        return false;

      // The pass made it past the filtering criteria, accept it
      return true;
    }

    public bool FilterPass_MachineEvents(ref FilteredPassData PassValue)
    {
      if (!AnyMachineEventFilterSelections)
      {
        // There are no constrictive machine events filter criteria - all cell passes pass the filter
        return true;
      }

      if (HasDesignFilter)
      {
        if (DesignNameID != Consts.kAllDesignsNameID && DesignNameID != PassValue.EventValues.EventDesignNameID)
          return false;
      }

      if (HasVibeStateFilter)
      {
        if (VibeState != PassValue.EventValues.EventVibrationState)
          return false;
      }

      if (HasMachineDirectionFilter)
      {
        if (((MachineDirection == MachineDirection.Forward && !Machine.MachineGearIsForwardGear(PassValue.EventValues.EventMachineGear))) ||
            ((MachineDirection == MachineDirection.Reverse && !Machine.MachineGearIsReverseGear(PassValue.EventValues.EventMachineGear))))
          return false;
      }

      if (HasElevationMappingModeFilter)
      {
        if (ElevationMappingMode != PassValue.EventValues.EventElevationMappingMode)
          return false;
      }

      if (HasGCSGuidanceModeFilter)
      {
        if (GCSGuidanceMode != PassValue.EventValues.EventMachineAutomatics)
          return false;
      }

      if (HasGPSAccuracyFilter)
      {
        if (GPSAccuracy != PassValue.EventValues.GPSAccuracy && !GPSAccuracyIsInclusive)
          return false;

        if (GPSAccuracyIsInclusive && GPSAccuracy < PassValue.EventValues.GPSAccuracy)
          return false;
      }

      if (HasGPSToleranceFilter)
      {
        if (!(PassValue.EventValues.GPSTolerance != CellPassConsts.NullGPSTolerance &&
              ((GPSToleranceIsGreaterThan && PassValue.EventValues.GPSTolerance >= GPSTolerance) ||
               (!GPSToleranceIsGreaterThan && PassValue.EventValues.GPSTolerance < GPSTolerance))))
          return false;
      }

      if (HasPositioningTechFilter)
      {
        if (PositioningTech != PassValue.EventValues.PositioningTechnology)
          return false;
      }

      if (HasLayerIDFilter)
      {
        if (LayerID != PassValue.EventValues.LayerID)
          return false;
      }

      // Filter on PassType
      if (HasPassTypeFilter)
      {
        if (!PassTypeHelper.PassTypeSetContains(PassTypeSet, PassValue.FilteredPass.PassType)) // maybe if noting set you may want ptFront as a default pass
          return false;
      }

      return true;
    }

    public bool FilterPass_NoMachineEvents(ref CellPass PassValue, ICellPassAttributeFilterProcessingAnnex filterAnnex)
    {
      if (!AnyNonMachineEventFilterSelections)
        return true;

      if (HasTimeFilter)
      {
        if (!FilterPassUsingTimeOnly(ref PassValue))
          return false;
      }

      if (HasElevationRangeFilter)
      {
        if (!filterAnnex.FilterPassUsingElevationRange(ref PassValue))
          return false;
      }

      if (HasMachineFilter)
      {
        // Check the machine identified by PassValue.InternalSiteModelMachineIndex is in our site model Machine
        // list based on the index of the machine in that list

        if (PassValue.InternalSiteModelMachineIndex < MachineIDSet.Count && !MachineIDSet[PassValue.InternalSiteModelMachineIndex])
          return false;
      }

      if (HasCompactionMachinesOnlyFilter)
      {
        IMachine Machine = siteModel.Machines[PassValue.InternalSiteModelMachineIndex];

        if (Machine != null && !Machine.MachineIsCompactorType())
          return false;
      }

      if (HasTemperatureRangeFilter && !FilterTemperatureByLastPass) // Note temperature filter has two behaviors depending on display or grid type etc
      {
        // filtering on every cell here
        if (!FilterPassUsingTemperatureRange(ref PassValue))
          return false;
      }

      return true;
    }

    // FilterSinglePass selects a single passes from the list of passes in
    // <PassValues> where <PassValues> contains the entire list of passes for
    // a cell in the database.
    public bool FilterSinglePass(CellPass[] passValues,
                                 int passValueCount,
                                 bool wantEarliestPass,
                                 ref FilteredSinglePassInfo filteredPassInfo,
                                 object /* IProfileCell */ profileCell,
                                 bool performAttributeSubFilter,
                                 ICellPassAttributeFilterProcessingAnnex filterAnnex)
    {
      bool Accept;
      bool Result = false;

      if (passValueCount == 0)
        return false;

      bool CheckAttributes = performAttributeSubFilter && AnyFilterSelections;
      int AcceptedIndex = -1;

      if (wantEarliestPass)
      {
        for (int I = 0; I < passValueCount; I++)
        {
          if (CheckAttributes && !FilterPass(ref passValues[I], filterAnnex))
            return false;

          Accept = profileCell == null
            ? PassIsAcceptable(ref passValues[I])
            : PassIsAcceptable(ref passValues[I]) && !((IProfileCell)profileCell).IsInSupersededLayer(passValues[I]);

          if (Accept)
          {
            AcceptedIndex = I;
            Result = true;
            break;
          }
        }
      }
      else
      {
        for (int I = passValueCount - 1; I >= 0; I--)
        {
          if (CheckAttributes && !FilterPass(ref passValues[I], filterAnnex))
            return false;

          Accept = profileCell == null
            ? PassIsAcceptable(ref passValues[I])
            : PassIsAcceptable(ref passValues[I]) && !((IProfileCell)profileCell).IsInSupersededLayer(passValues[I]);

          if (Accept)
          {
            AcceptedIndex = I;
            Result = true;
            break;
          }
        }
      }

      if (Result)
      {
        filteredPassInfo.FilteredPassData.FilteredPass = passValues[AcceptedIndex];
        filteredPassInfo.FilteredPassData.TargetValues.TargetCCV = 1;
        filteredPassInfo.FilteredPassData.TargetValues.TargetMDP = 1;
        filteredPassInfo.FilteredPassData.TargetValues.TargetPassCount = 1;
        filteredPassInfo.PassCount = passValueCount;
      }

      return Result;
    }

    /// <summary>
    /// FilterSinglePass selects a single passes from the list of passes in
    /// PassValues where PassValues contains the entire list of passes for
    /// a cell in the database.
    /// </summary>
    /// <param name="filteredPassValues"></param>
    /// <param name="passValueCount"></param>
    /// <param name="wantEarliestPass"></param>
    /// <param name="filteredPassInfo"></param>
    /// <param name="profileCell"></param>
    /// <param name="performAttributeSubFilter"></param>
    /// <param name="filterAnnex"></param>
    /// <returns></returns>
    public bool FilterSinglePass(FilteredPassData[] filteredPassValues,
                                 int passValueCount,
                                 bool wantEarliestPass,
                                 ref FilteredSinglePassInfo filteredPassInfo,
                                 object /*IProfileCell*/ profileCell,
                                 bool performAttributeSubFilter,
                                 ICellPassAttributeFilterProcessingAnnex filterAnnex)
    {
      bool Accept;
      bool Result = false;

      if (passValueCount == 0)
      {
        return false;
      }

      bool CheckAttributes = performAttributeSubFilter && AnyFilterSelections;
      int AcceptedIndex = -1;

      if (wantEarliestPass)
      {
        for (int I = 0; I < passValueCount; I++)
        {
          if (CheckAttributes && !FilterPass(ref filteredPassValues[I], filterAnnex))
          {
            return false;
          }

          Accept = profileCell == null
            ? PassIsAcceptable(ref filteredPassValues[I].FilteredPass)
            : PassIsAcceptable(ref filteredPassValues[I].FilteredPass) && !((IProfileCell)profileCell).IsInSupersededLayer(filteredPassValues[I].FilteredPass);

          if (Accept)
          {
            AcceptedIndex = I;
            Result = true;
            break;
          }
        }
      }
      else
      {
        for (int I = passValueCount - 1; I >= 0; I--)
        {
          if (CheckAttributes && !FilterPass(ref filteredPassValues[I], filterAnnex))
          {
            return false;
          }

          Accept = profileCell == null
            ? PassIsAcceptable(ref filteredPassValues[I].FilteredPass)
            : PassIsAcceptable(ref filteredPassValues[I].FilteredPass) && !((IProfileCell)profileCell).IsInSupersededLayer(filteredPassValues[I].FilteredPass);

          if (Accept)
          {
            AcceptedIndex = I;
            Result = true;
            break;
          }
        }
      }

      if (Result)
      {
        filteredPassInfo.FilteredPassData = filteredPassValues[AcceptedIndex];
        filteredPassInfo.PassCount = passValueCount;
      }
      return Result;
    }

    /// <summary>
    /// Constructs a fingerprint based on the features of a filter that can influence the outcome of analyzed sub grids stored in a cache
    /// </summary>
    /// <returns></returns>
    public string SpatialCacheFingerprint()
    {
      var sb = new StringBuilder();

      // TIme
      if (HasTimeFilter)
        sb.Append($"TF:{StartTime.Ticks}-{EndTime.Ticks}");

      // Designs
      if (HasDesignFilter)
        sb.Append($"DF:{DesignNameID}");

      // Machines
      if (HasMachineFilter)
      {
        sb.Append("MF:");

        var machineIDBitArray = GetMachineIDsSet();
        for (int i = 0; i <  MachineIDSet.Length; i++)
          if (machineIDBitArray[i])
            sb.Append($"-{i}");
      }

      // Machine direction filter
      if (HasMachineDirectionFilter)
        sb.Append($"MD:{MachineDirection}");

      // Pass Type filter
      if (HasPassTypeFilter)
        sb.Append($"PT:{PassTypeSet}");

      // Vibe state filter
      if (HasVibeStateFilter)
        sb.Append($"VS:{VibeState}");

      // Min elev mapping
      if (HasElevationMappingModeFilter)
        sb.Append($"EMM:{(int)ElevationMappingMode}");

      // Elevation type
      if (HasElevationTypeFilter)
        sb.Append($"ET:{ElevationType}");

      // Exclusion of surveyed surfaces from query
      if (ExcludeSurveyedSurfaces())
        sb.Append("ESS:1");

      // GCS Guidance mode
      if (HasGCSGuidanceModeFilter)
        sb.Append($"GM:{GCSGuidanceMode}");

      // GPS Accuracy
      if (HasGPSAccuracyFilter)
        sb.Append($"GA:{(GPSAccuracyIsInclusive?1:0)}-{GPSAccuracy}");

      // GPS Tolerance
      if (HasGPSToleranceFilter)
        sb.Append($"GT:{(GPSToleranceIsGreaterThan?1:0)}-{GPSTolerance}");

      // Positioning Tech
      if (HasPositioningTechFilter)
        sb.Append($"PT:{PositioningTech}");

      // Elevation Range
      if (HasElevationRangeFilter)
      {
        if ((ElevationRangeDesign?.DesignID ?? Guid.Empty) != Guid.Empty)
          sb.Append($"ER:{ElevationRangeDesign.DesignID}-{ElevationRangeDesign.Offset}-{ElevationRangeOffset:F3}-{ElevationRangeThickness:F3}");
        else
          sb.Append($"ER:{ElevationRangeLevel:F3}-{ElevationRangeOffset:F3}-{ElevationRangeThickness:F3}");
      }

      // Layer state filter
      if (HasLayerStateFilter)
        sb.Append($"LS:{LayerState}");

      // Compaction machines only
      if (HasCompactionMachinesOnlyFilter)
        sb.Append("CMO:1");

      // Layer ID filter
      if (HasLayerIDFilter)
        sb.Append($"LID:{LayerID}");

      // TemperatureRangeFilter
      if (HasTemperatureRangeFilter)
        sb.Append($"TR:{MaterialTemperatureMin}-{MaterialTemperatureMax}-{(FilterTemperatureByLastPass?1:0)}");

      // PassCountRangeFilter
      if (HasPassCountRangeFilter)
        sb.Append($"PC:{PassCountRangeMin}-{PassCountRangeMax}");

      if (ReturnEarliestFilteredCellPass)
        sb.Append("REFCP:1");

      return sb.ToString();
    }

    /// <summary>
    /// Converts an array of GUIDs representing machine identifiers into a BitArray encoding a bit set of
    /// internal machine IDs relative to this site model
    /// </summary>
    private short[] GetMachineIDs()
    {
      return (siteModel?.Machines?.Count ?? 0) > 0
        ? MachinesList.Where(x => siteModel.Machines.Locate(x) != null).Select(x => siteModel.Machines.Locate(x).InternalSiteModelMachineIndex).ToArray()
        : new short[0];
    }

    /// <summary>
    /// Converts an array of GUIDs representing machine identifiers into a BitArray encoding a bit set of
    /// internal machine IDs relative to this site model
    /// </summary>
    public BitArray GetMachineIDsSet()
    {
      if (MachineIDSet != null)
        return MachineIDSet;

      var machineIDs = GetMachineIDs();
      MachineIDSet = machineIDs.Length > 0 ? new BitArray(machineIDs.Max() + 1) : new BitArray(0);

      foreach (var internalID in machineIDs)
        MachineIDSet[internalID] = true;

      return MachineIDSet;
    }

    /// <summary>
    /// LastRecordedCellPassSatisfiesFilter denotes whether the settings in the filter
    /// may be satisfied by examining the last recorded value wrt the sub grid information
    /// currently being requested. This allows the cached latest recorded values slice
    /// stored
    /// </summary>
    public bool LastRecordedCellPassSatisfiesFilter => !AnyFilterSelections && !ReturnEarliestFilteredCellPass;

    /// <summary>
    /// FilterMultiplePasses selects a set of passes from the list of passes
    /// in passValues where passValues contains the entire
    /// list of passes for a cell in the database.
    /// </summary>
    /// <param name="passValues"></param>
    /// <param name="passValueCount"></param>
    /// <param name="filteredPassInfo"></param>
    /// <param name="filterAnnex"></param>
    /// <returns></returns>
    public bool FilterMultiplePasses(CellPass[] passValues,
      int passValueCount,
      FilteredMultiplePassInfo filteredPassInfo,
      ICellPassAttributeFilterProcessingAnnex filterAnnex)
    {
      if (!AnyFilterSelections)
      {
        if (passValueCount == 0) // There's nothing to do
          return true;

        // Note: We make an independent copy of the Passes array for the cell.
        // Simply doing CellPasses = Passes just copies a reference to the
        // array of passes.

        for (int I = 0; I < passValueCount; I++)
          filteredPassInfo.AddPass(passValues[I]);

        return true;
      }

      bool Result = false;

      for (int i = 0; i < passValueCount; i++)
      {
        CellPass PassValue = passValues[i];

        if (FilterPass(ref PassValue, filterAnnex))
        {
          filteredPassInfo.AddPass(PassValue);
          Result = true;
        }
      }

      return Result;
    }

    public bool ExcludeSurveyedSurfaces()
    {
      return HasDesignFilter || HasMachineFilter || HasMachineDirectionFilter ||
             HasVibeStateFilter || HasCompactionMachinesOnlyFilter ||
             HasGPSAccuracyFilter || HasPassTypeFilter || HasTemperatureRangeFilter;
    }

    public string ActiveFiltersText() => "Not implemented";

    private bool PassIsAcceptable(ref CellPass PassValue)
    {
      // Certain types of grid attribute data requests may need us to select
      // a pass that is not the latest pass in the pass list. Such an instance is
      // when requesting CCV values where null CCV values are passed over in favor of
      // non-null CCV values in passes that are older in the pass list for the cell.

      // Important: Also see the CalculateLatestPassDataForPassStack() function in
      // TICServerSubGridTreeLeaf.CalculateLatestPassGrid() to ensure that the logic
      // here is consistent (or at least not contradictory) with the logic here.
      // The checks are duplicated as there may be different logic applied to the
      // selection of the 'latest' pass from a cell pass state versus selection of
      // an appropriate filtered pass given other filtering criteria in play.

      switch (RequestedGridDataType)
      {
        case GridDataType.CCV:
          return PassValue.CCV != CellPassConsts.NullCCV;
        case GridDataType.MDP:
          return PassValue.MDP != CellPassConsts.NullMDP;
        case GridDataType.RMV:
          return PassValue.RMV != CellPassConsts.NullRMV;
        case GridDataType.Frequency:
          return PassValue.Frequency != CellPassConsts.NullFrequency;
        case GridDataType.Amplitude:
          return PassValue.Amplitude != CellPassConsts.NullAmplitude;
        case GridDataType.Temperature:
          return PassValue.MaterialTemperature != CellPassConsts.NullMaterialTemperatureValue;
        case GridDataType.TemperatureDetail:
          return PassValue.MaterialTemperature != CellPassConsts.NullMaterialTemperatureValue;
        case GridDataType.GPSMode:
          return PassValue.gpsMode != CellPassConsts.NullGPSMode;
        default:
          return true;
      }
    }
  }
}
