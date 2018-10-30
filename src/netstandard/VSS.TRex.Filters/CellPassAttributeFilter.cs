using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Events;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Filters.Models;
using VSS.TRex.Machines;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Utilities;

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
  /// TICGridDataFilter provides filtering support for grid data requested by the client
  /// </summary>
  public class CellPassAttributeFilter : CellPassAttributeFilterModel, ICellPassAttributeFilter
  {
    [NonSerialized] private ISiteModel siteModel;

    /// <summary>
    /// Owner is the SiteModel instance to which this filter relates and is used in cases where machine related
    /// attributes are included in the filter
    /// </summary>
    public object /*ISiteModel*/ SiteModel
    {
      get { return siteModel; }
      set { siteModel = (ISiteModel) value; }
    }

    /// <summary>
    /// A subgrid containing sampled elevations from a benchmark surface defining the bench surface for
    /// an elevation range filter.
    /// </summary>
    [NonSerialized] public IClientHeightLeafSubGrid ElevationRangeDesignElevations;

    /// <summary>
    /// The machines present in the filter represented as an array of internal machine IDs specific to the site model the filter is being applied to
    /// </summary>
    public short[] MachineIDs { get; set; }

    /// <summary>
    /// The machines present in the filter represented as a bit set
    /// </summary>
    public BitArray MachineIDSet { get; set; }

    public bool AnyFilterSelections { get; set; }

    public bool AnyMachineEventFilterSelections { get; set; }

    public bool AnyNonMachineEventFilterSelections { get; set; }

    /// <summary>
    /// Default no-arg constructor the produces a filter with all aspects set to their defaults
    /// </summary>
    public CellPassAttributeFilter()
    {
      ClearFilter();
    }

    public CellPassAttributeFilter(IBinaryRawReader reader)
    {
      FromBinary(reader);
    }

    /// <summary>
    /// Performs operations that prepares the filter for active use. Prepare() must be called prior to
    /// active use of the filter.
    /// </summary>
    public void Prepare()
    {
      AnyFilterSelections =
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
        HasMinElevMappingFilter ||
        HasPassTypeFilter ||
        HasPositioningTechFilter ||
        HasTimeFilter ||
        HasVibeStateFilter ||
        HasTemperatureRangeFilter ||
        HasPassCountRangeFilter;

      AnyMachineEventFilterSelections =
        HasDesignFilter ||
        HasVibeStateFilter ||
        HasMachineDirectionFilter ||
        HasMinElevMappingFilter ||
        HasGCSGuidanceModeFilter ||
        HasGPSAccuracyFilter ||
        HasGPSToleranceFilter ||
        HasPositioningTechFilter ||
        HasLayerIDFilter ||
        HasPassTypeFilter;

      AnyNonMachineEventFilterSelections =
        HasTimeFilter ||
        HasMachineFilter ||
        HasElevationRangeFilter ||
        HasCompactionMachinesOnlyFilter ||
        HasTemperatureRangeFilter;

      InitialiseMachineIDsSet();
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
      ClearTemperatureRange();
      ClearPassCountRange();
      ClearElevationRangeFilterInitialisation();
      AnyFilterSelections = false;
      AnyMachineEventFilterSelections = false;
      AnyNonMachineEventFilterSelections = false;
      ReturnEarliestFilteredCellPass = false;
      FilterTemperatureByLastPass = false;
    }

    public void ClearVibeState()
    {
      HasVibeStateFilter = false;

      VibeState = VibrationState.Invalid;
    }

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
    private int MachineIDListsComparison(short[] list1, short[] list2)
    {
      if (list1 == null && list2 == null)
        return 0;

      // Check list lengths
      int result = list1.Length < list2.Length ? -1 : list1.Length == list2.Length ? 0 : 1;

      // If the lengths are the same check individual items
      if (result == 0)
      {
        for (int i = 0; i < list1.Length; i++)
        {
          result = list1[i] < list2[i] ? -1 : list1[i] == list2[i] ? 0 : 1;

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
    public int CompareTo(ICellPassAttributeFilter AFilter)
    {
      // Time
      int Result = FlagCheck(HasTimeFilter, AFilter.HasTimeFilter);
      if (Result != 0)
      {
        return Result;
      }
      else
      {
        if (HasTimeFilter) // Check the contents of the time filter
        {
          Result = StartTime.CompareTo(AFilter.StartTime);
          if (Result == 0)
            Result = EndTime.CompareTo(AFilter.EndTime);
        }
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
        Result = MachineIDListsComparison(MachineIDs, AFilter.MachineIDs);

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
      Result = FlagCheck(HasMinElevMappingFilter, AFilter.HasMinElevMappingFilter);
      if (Result != 0)
        return Result;

      if (HasMinElevMappingFilter) // Check the contents of the min elevation filter
        Result = MinElevationMapping.CompareTo(AFilter.MinElevationMapping); // CompareValue(Ord(MinElevationMapping), Ord(AFilter.MinElevationMapping));

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
        if (ElevationRangeDesignID != Guid.Empty)
        {
          Result = ElevationRangeDesignID.CompareTo(AFilter.ElevationRangeDesignID);
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
        Result = PasscountRangeMin.CompareTo(AFilter.PasscountRangeMin);
        if (Result != 0)
          return Result;
        Result = PasscountRangeMax.CompareTo(AFilter.PasscountRangeMax);
        if (Result != 0)
          return Result;
      }

      // Everything is equal!
      Result = 0;

      return Result;
    }

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
      ElevationRangeDesignID = Guid.Empty;

      ElevationRangeIsInitialised = false;
      ElevationRangeIsLevelAndThicknessOnly = false;
      ElevationRangeTopElevationForCell = Consts.NullDouble;
      ElevationRangeBottomElevationForCell = Consts.NullDouble;
      ElevationRangeDesignElevations = null;
    }

    public void ClearElevationRangeFilterInitialisation()
    {
      ElevationRangeIsInitialised = false;
      ElevationRangeDesignElevations = null;
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
      PasscountRangeMin = 0;
      PasscountRangeMax = 0;
    }

    public void ClearGPSTolerance()
    {
      HasGPSToleranceFilter = false;
      GPSTolerance = Consts.kMaxGPSAccuracyErrorLimit;
    }

    public void ClearGuidanceMode()
    {
      HasGCSGuidanceModeFilter = false;
      GCSGuidanceMode = MachineAutomaticsMode.Unknown;
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

      //  FilterSelections = Source.FilterSelections;

      // Time based filtering members
      StartTime = Source.StartTime;
      EndTime = Source.EndTime;

      // Machine based filtering members
      if (Source.MachinesList != null)
      {
        Array.Copy(Source.MachinesList, MachinesList, Source.MachinesList.Length);
        // all set types below HasMachineFilter = Source.HasMachineFilter;
      }
      else
      {
        ClearMachines();
      }


      if (Source.MachineIDSet != null)
      {
        MachineIDSet = new BitArray(Source.MachineIDSet);
      }
      else
      {
        MachineIDSet = null;
      }

      // Design based filtering member
      DesignNameID = Source.DesignNameID;

      // Auto Vibe state filtering member
      VibeState = Source.VibeState;

      // how to build layers
      LayerState = Source.LayerState;

      MachineDirection = Source.MachineDirection;

      PassTypeSet = Source.PassTypeSet;
      MinElevationMapping = Source.MinElevationMapping;

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
      ElevationRangeDesignID = Source.ElevationRangeDesignID;

      RestrictFilteredDataToCompactorsOnly = Source.RestrictFilteredDataToCompactorsOnly;

      LayerID = Source.LayerID;

      MaterialTemperatureMin = Source.MaterialTemperatureMin;
      MaterialTemperatureMax = Source.MaterialTemperatureMax;
      FilterTemperatureByLastPass = Source.FilterTemperatureByLastPass;
      PasscountRangeMin = Source.PasscountRangeMin;
      PasscountRangeMax = Source.PasscountRangeMax;

      Array.Copy(Source.SurveyedSurfaceExclusionList, SurveyedSurfaceExclusionList, Source.SurveyedSurfaceExclusionList.Length);

      // This assignment method consciously does not "clone" or otherwise assign Elevation Range related filter state;
      // i.e. FElevationRangeIsInitialised, FElevationRangeIsLevelAndThicknessOnly, FElevationRangeTopElevationForCell,
      //      FElevationRangeBottomElevationForCell, FElevationRangeDesignElevations


      HasTimeFilter = Source.HasTimeFilter;
      HasMachineFilter = Source.HasMachineFilter;
      HasMachineDirectionFilter = Source.HasMachineDirectionFilter;
      HasDesignFilter = Source.HasDesignFilter;
      HasVibeStateFilter = Source.HasVibeStateFilter;
      HasLayerStateFilter = Source.HasLayerStateFilter;
      HasMinElevMappingFilter = Source.HasMinElevMappingFilter;
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
      RestrictFilteredDataToCompactorsOnly = false;
    }

    public void ClearMachineDirection()
    {
      HasMachineDirectionFilter = false;
      MachineDirection = MachineDirection.Unknown;
    }

    public void ClearMachines()
    {
      HasMachineFilter = false;
      MachinesList = null;
    }

    public void ClearMinElevationMapping()
    {
      HasMinElevMappingFilter = false;
      MinElevationMapping = false;
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

      StartTime = DateTime.MinValue;
      EndTime = DateTime.MaxValue;
    }

    // Returns true if the specified pass meets the set filter (if any)
    // FilterPass determines if a single pass conforms to the current filtering configuration
    public bool FilterPass(ref CellPass PassValue)
    {
      int DesignNameIDValue = Consts.kNoDesignNameID;
      VibrationState VibeStateValue = VibrationState.Invalid;
      MachineGear MachineGearValue = MachineGear.Null;
      bool MinElevMappingValue = false;
      GPSAccuracyAndTolerance GPSAccuracyAndToleranceValue = GPSAccuracyAndTolerance.Null();
      PositioningTech PositioningTechStateValue = PositioningTech.Unknown;
      MachineAutomaticsMode GCSGuidanceModeValue = MachineAutomaticsMode.Unknown;
      ushort LayerIDStateValue = ushort.MaxValue; // ID of current layer

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
        if (!FilterPassUsingElevationRange(ref PassValue))
          return false;
      }

      if (HasMachineFilter)
      {
        // Check the machine identified by PassValue.MachineID is in our Sitemodel Machine
        // list based on the index of the machine in that list

        if (PassValue.InternalSiteModelMachineIndex < MachineIDSet.Count && !MachineIDSet[PassValue.InternalSiteModelMachineIndex])
          return false;
      }

      if (HasCompactionMachinesOnlyFilter)
      {
        //Machine = siteModel.Machines.Locate(PassValue.MachineID);
        IMachine Machine = siteModel.Machines[PassValue.InternalSiteModelMachineIndex];

        if (Machine != null && !Machine.MachineIsCompactorType())
          return false;
      }

      // Extract the list of events for the machine referred to in the cell pass. Use this
      // reference for all filter criteria that depend on machine events
      ProductionEventLists machineTargetValues = AnyMachineEventFilterSelections ? (ProductionEventLists) siteModel.MachinesTargetValues[PassValue.InternalSiteModelMachineIndex] : null;

      if (HasDesignFilter)
      {
        DesignNameIDValue = machineTargetValues.MachineDesignNameIDStateEvents.GetValueAtDate(PassValue.Time, out _, DesignNameIDValue);

        if ((DesignNameIDValue != Consts.kAllDesignsNameID) && (DesignNameID != DesignNameIDValue))
          return false;
      }

      if (HasVibeStateFilter)
      {
        VibeStateValue = machineTargetValues.VibrationStateEvents.GetValueAtDate(PassValue.Time, out _, VibeStateValue);

        if (VibeState != VibeStateValue)
          return false;
      }

      if (HasGCSGuidanceModeFilter)
      {
        GCSGuidanceModeValue = machineTargetValues.MachineAutomaticsStateEvents.GetValueAtDate(PassValue.Time, out _, GCSGuidanceModeValue);

        if (GCSGuidanceMode != GCSGuidanceModeValue)
          return false;
      }

      if (HasMachineDirectionFilter)
      {
        MachineGearValue = machineTargetValues.MachineGearStateEvents.GetValueAtDate(PassValue.Time, out _, MachineGearValue);

        if (((MachineDirection == MachineDirection.Forward && !TRex.Machines.Machine.MachineGearIsForwardGear(MachineGearValue))) ||
            ((MachineDirection == MachineDirection.Reverse && !TRex.Machines.Machine.MachineGearIsReverseGear(MachineGearValue))))
          return false;
      }

      if (HasMinElevMappingFilter)
      {
        MinElevMappingValue = machineTargetValues.MinElevMappingStateEvents.GetValueAtDate(PassValue.Time, out _, MinElevMappingValue);

        if (MinElevationMapping != MinElevMappingValue)
          return false;
      }

      if (HasGPSAccuracyFilter || HasGPSToleranceFilter)
      {
        GPSAccuracyAndToleranceValue = machineTargetValues.GPSAccuracyAndToleranceStateEvents.GetValueAtDate(PassValue.Time, out _, GPSAccuracyAndToleranceValue);

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
        PositioningTechStateValue = machineTargetValues.PositioningTechStateEvents.GetValueAtDate(PassValue.Time, out _, PositioningTechStateValue);

        if (PositioningTech != PositioningTechStateValue)
          return false;
      }

      // Filter on LayerID
      if (HasLayerIDFilter)
      {
        LayerIDStateValue = machineTargetValues.LayerIDStateEvents.GetValueAtDate(PassValue.Time, out _, LayerIDStateValue);
        if (LayerID != LayerIDStateValue)
          return false;
      }

      // Filter on PassType
      if (HasPassTypeFilter)
      {
        if (!CellPass.PassTypeHelper.PassTypeSetContains(PassTypeSet, PassValue.PassType))
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

    public bool FilterPass(ref FilteredPassData PassValue)
    {
      if (!AnyFilterSelections)
      {
        return true;
      }

      if (HasTimeFilter)
      {
        if (!FilterPassUsingTimeOnly(ref PassValue.FilteredPass))
          return false;
      }

      if (HasElevationRangeFilter)
        if (!FilterPassUsingElevationRange(ref PassValue.FilteredPass))
          return false;

      if (HasMachineFilter)
      {
        // Check the machine identified by PassValue.MachineID is in our Sitemodel Machine
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
        if (((MachineDirection == MachineDirection.Forward && !Machines.Machine.MachineGearIsForwardGear(PassValue.EventValues.EventMachineGear))) ||
            ((MachineDirection == MachineDirection.Reverse && !Machines.Machine.MachineGearIsReverseGear(PassValue.EventValues.EventMachineGear))))
          return false;
      }

      if (HasMinElevMappingFilter)
      {
        if (MinElevationMapping != PassValue.EventValues.EventMinElevMapping)
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
        if (!CellPass.PassTypeHelper.PassTypeSetContains(PassTypeSet, PassValue.FilteredPass.PassType)) // maybe if noting set you may want ptFront as a default pass
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

    public bool FilterPassUsingElevationRange(ref CellPass PassValue)
    {
      Debug.Assert(ElevationRangeIsInitialised, "Elevation range filter being used without the elevation range data being initialised");
      return (ElevationRangeBottomElevationForCell != Consts.NullDouble) &&
             Range.InRange(PassValue.Height, ElevationRangeBottomElevationForCell, ElevationRangeTopElevationForCell);
    }

    public bool FilterPassUsingTemperatureRange(ref CellPass PassValue)
    {
      Debug.Assert(HasTemperatureRangeFilter, "Temperature range filter being used without the temperature range data being initialised");
      return (PassValue.MaterialTemperature != CellPassConsts.NullMaterialTemperatureValue) &&
             Range.InRange(PassValue.MaterialTemperature, MaterialTemperatureMin, MaterialTemperatureMax);
    }

    public bool FilterPassUsingTimeOnly(ref CellPass PassValue)
    {
      if (StartTime == DateTime.MinValue)
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

      if (HasMinElevMappingFilter)
      {
        if (MinElevationMapping != PassValue.EventValues.EventMinElevMapping)
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
        if (!CellPass.PassTypeHelper.PassTypeSetContains(PassTypeSet, PassValue.FilteredPass.PassType)) // maybe if noting set you may want ptFront as a default pass
          return false;
      }

      return true;
    }

    public bool FilterPass_NoMachineEvents(CellPass PassValue)
    {
      if (!AnyNonMachineEventFilterSelections)
      {
        return true;
      }

      if (HasTimeFilter)
      {
        if (!FilterPassUsingTimeOnly(ref PassValue))
          return false;
      }

      if (HasElevationRangeFilter)
      {
        if (!FilterPassUsingElevationRange(ref PassValue))
          return false;
      }

      if (HasMachineFilter)
      {
        // Check the machine identified by PassValue.MachineID is in our Sitemodel Machine
        // list based on the index of the machine in that list

        if (PassValue.InternalSiteModelMachineIndex < MachineIDSet.Count && !MachineIDSet[PassValue.InternalSiteModelMachineIndex])
          return false;
      }

      if (HasCompactionMachinesOnlyFilter)
      {
        //Machine = siteModel.Machines.Locate(PassValue.MachineID);
        IMachine Machine = siteModel.Machines[PassValue.InternalSiteModelMachineIndex];

        if (Machine != null && !Machine.MachineIsCompactorType())
          return false;
      }


      if (HasTemperatureRangeFilter && !FilterTemperatureByLastPass) // Note temperature filter has two behavours depending on display or grid type etc
      {
        // filtering on every cell here
        if (!FilterPassUsingTemperatureRange(ref PassValue))
          return false;
      }

      return true;
    }

    public bool FiltersElevation(float Elevation)
    {
      Debug.Assert(ElevationRangeIsInitialised, "Elevation range filter being used without the elevation range data being initialised");
      return ElevationRangeBottomElevationForCell != Consts.NullDouble &&
             Range.InRange(Elevation, ElevationRangeBottomElevationForCell, ElevationRangeTopElevationForCell);
    }

    public bool FiltersElevation(double Elevation)
    {
      Debug.Assert(ElevationRangeIsInitialised, "Elevation range filter being used without the elevation range data being initialised");
      return ElevationRangeBottomElevationForCell != Consts.NullDouble &&
             Range.InRange(Elevation, ElevationRangeBottomElevationForCell, ElevationRangeTopElevationForCell);
    }

    /// <summary>
    /// FilterSinglePass selects a single pass from the list of passes in
    /// PassValues where PassValues contains the entire list of passes for
    /// a cell in the database.
    /// </summary>
    /// <returns></returns>
    public bool FilterSinglePass(CellPass[] PassValues,
      int PassValueCount,
      ref FilteredSinglePassInfo FilteredPassInfo,
      object /*IProfileCell*/ profileCell)
    {
      return FilterSinglePass(PassValues,
        PassValueCount,
        ReturnEarliestFilteredCellPass,
        ref FilteredPassInfo,
        (IProfileCell) profileCell,
        true);
    }

    // FilterSinglePass selects a single passes from the list of passes in
    // <PassValues> where <PassValues> contains the entire list of passes for
    // a cell in the database.
    public bool FilterSinglePass(CellPass[] passValues,
                                 int passValueCount,
                                 bool wantEarliestPass,
                                 ref FilteredSinglePassInfo filteredPassInfo,
                                 object /* IProfileCell */ profileCell,
                                 bool performAttributeSubFilter)
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
          if (CheckAttributes && !FilterPass(ref passValues[I]))
          {
            return false;
          }

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
          if (CheckAttributes && !FilterPass(ref passValues[I]))
          {
            return false;
          }

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
    /// <returns></returns>
    public bool FilterSinglePass(FilteredPassData[] filteredPassValues,
                                 int passValueCount,
                                 bool wantEarliestPass,
                                 ref FilteredSinglePassInfo filteredPassInfo,
                                 object /*IProfileCell*/ profileCell,
                                 bool performAttributeSubFilter)
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
          if (CheckAttributes && !FilterPass(ref filteredPassValues[I]))
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
          if (CheckAttributes && !FilterPass(ref filteredPassValues[I]))
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
        foreach (var id in MachineIDs)
          sb.Append($"-{id}");
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
      if (HasMinElevMappingFilter)
        sb.Append($"VS:{(MinElevationMapping ? 1 : 0)}");

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
        sb.Append($"GA:{GCSGuidanceMode}-{(GPSAccuracyIsInclusive?1:0)}-{GPSAccuracy}");

      // GPS Tolerance
      if (HasGPSToleranceFilter)
        sb.Append($"GT:{(GPSToleranceIsGreaterThan?1:0)}-{GPSTolerance}");

      // Positioning Tech
      if (HasPositioningTechFilter)
        sb.Append($"PT:{PositioningTech}");

      // Elevation Range
      if (HasElevationRangeFilter)
      {
        if (ElevationRangeDesignID != Guid.Empty)
          sb.Append($"ER-{ElevationRangeDesignID}-{ElevationRangeOffset:F3}-{ElevationRangeThickness:F3}");
        else
          sb.Append($"ER-{ElevationRangeLevel:F3}-{ElevationRangeOffset:F3}-{ElevationRangeThickness:F3}");
      }

      // Layer state filter
      if (HasLayerStateFilter)
        sb.Append($"LS:{LayerState}");

      // Compaction machines only
      if (HasLayerStateFilter)
        sb.Append("CMO:1");

      // Layer ID filter
      if (HasLayerIDFilter)
        sb.Append($"LF:{LayerID}");

      // TemperatureRangeFilter
      if (HasTemperatureRangeFilter)
        sb.Append($"LF:{MaterialTemperatureMin}-{MaterialTemperatureMax}-{(FilterTemperatureByLastPass?1:0)}");

      // PassCountRangeFilter
      if (HasPassCountRangeFilter)
        sb.Append($"PC:{PasscountRangeMin}-{PasscountRangeMax}");

      return sb.ToString();
    }


    public void InitaliaseFilteringForCell(byte ASubgridCellX, byte ASubgridCellY)
    {
      if (!HasElevationRangeFilter)
        return;

      if (ElevationRangeDesignElevations != null)
      {
        if (ElevationRangeDesignElevations.Cells[ASubgridCellX, ASubgridCellY] == Consts.NullHeight)
        {
          ElevationRangeTopElevationForCell = Consts.NullDouble;
          ElevationRangeBottomElevationForCell = Consts.NullDouble;
          return;
        }
        else
        {
          ElevationRangeTopElevationForCell = ElevationRangeDesignElevations.Cells[ASubgridCellX, ASubgridCellY] + ElevationRangeOffset;
        }
      }
      else
      {
        ElevationRangeTopElevationForCell = ElevationRangeLevel + ElevationRangeOffset;
      }

      ElevationRangeBottomElevationForCell = ElevationRangeTopElevationForCell - ElevationRangeThickness;
    }

    public void InitialiseElevationRangeFilter(IClientHeightLeafSubGrid DesignElevations)
    {
      // If there is a design specified then intialise the filter using the design elevations
      // queried and supplied by the caller, otherwise the specified Elevation level, offset and thickness
      // are used to calculate an elevation bracket.

      ElevationRangeIsLevelAndThicknessOnly = DesignElevations == null;
      if (ElevationRangeIsLevelAndThicknessOnly)
      {
        ElevationRangeTopElevationForCell = ElevationRangeLevel + ElevationRangeOffset;
        ElevationRangeBottomElevationForCell = ElevationRangeTopElevationForCell - ElevationRangeThickness;
      }
      else
      {
        ElevationRangeDesignElevations = DesignElevations;
      }

      ElevationRangeIsInitialised = true;
    }

    /// <summary>
    /// Converts an array of Guids representing machine identifiers into a BitArray encoding a bit set of
    /// internal machine IDs relative to this sitemodel
    /// </summary>
    public void InitialiseMachineIDsSet()
    {
      if (siteModel == null)
        return;

      short[] internalMachineIDs = MachinesList.Where(x => siteModel.Machines.Locate(x) != null).Select(x => siteModel.Machines.Locate(x).InternalSiteModelMachineIndex).ToArray();

      if (internalMachineIDs.Length == 0)
      {
        MachineIDSet = null;
      }
      else
      {
        MachineIDSet = new BitArray(internalMachineIDs.Max() + 1);

        foreach (var internalID in internalMachineIDs)
          MachineIDSet[internalID] = true;
      }
    }

    public override bool IsTimeRangeFilter() => HasTimeFilter && StartTime > DateTime.MinValue;

    /// <summary>
    /// LastRecordedCellPassSatisfiesFilter denotes whether the settings in the filter
    /// may be satisfied by examining the last recorded value wrt the subgrid information
    /// currently being requested. This allows the cached latest recorded values slice
    /// stored
    /// </summary>
    public bool LastRecordedCellPassSatisfiesFilter => !AnyFilterSelections && !ReturnEarliestFilteredCellPass;

    /// <summary>
    /// FilterMultiplePasses selects a set of passes from the list of passes
    /// in <passValues> where <passValues> contains the entire
    /// list of passes for a cell in the database.
    /// </summary>
    /// <param name="passValues"></param>
    /// <param name="passValueCount"></param>
    /// <param name="filteredPassInfo"></param>
    /// <returns></returns>
    public bool FilterMultiplePasses(CellPass[] passValues,
      int passValueCount,
      ref FilteredMultiplePassInfo filteredPassInfo)
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

        if (FilterPass(ref PassValue))
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
      // when request CCV value where null CCV values are passed over in favour of
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
