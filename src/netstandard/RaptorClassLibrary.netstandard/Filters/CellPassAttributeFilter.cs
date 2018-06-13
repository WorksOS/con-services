using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Events;
using VSS.TRex.Machines;
using VSS.TRex.SubGridTrees.Client;
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


         The result of ElevationangeOffset filter is <YES> the cell may be used for cell pass
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

     <DataPassFilter> Represents a base class from which filter classes may be derived 
     */

    /// <summary>
    /// TICGridDataFilter provides filtering support for grid data requested by the client
    /// </summary>
    [Serializable]
    public class CellPassAttributeFilter : DataPassFilter
    {
        // Time based filtering members
        /// <summary>
        /// The earliest time that a measured cell pass must have to be included in the filter
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// The latest time that a measured cell pass must have to be included in the filter
        /// </summary>
        public DateTime EndTime { get; set; } = DateTime.MaxValue;

        // Machine based filtering members
        public Guid[] MachinesList { get; set; } 

        // Design based filtering member (for designs reported by name from machine via TAG files)
        public int DesignNameID { get; set; } // DesignNameID :TICDesignNameID;

        // Auto Vibe state filtering member
        public VibrationState VibeState { get; set; } = VibrationState.Invalid;

        public MachineDirection MachineDirection { get; set; } = MachineDirection.Unknown;

        public PassTypeSet PassTypeSet { get; set; }

        public bool MinElevationMapping { get; set; } //MinElevationMapping : TICMinElevMappingState;
        public PositioningTech PositioningTech { get; set; } = PositioningTech.Unknown;

        public ushort GPSTolerance { get; set; } = CellPass.NullGPSTolerance;

        public bool GPSAccuracyIsInclusive { get; set; }

        public GPSAccuracy GPSAccuracy { get; set; } = GPSAccuracy.Unknown;

        /// <summary>
        /// The filter will select cell passes with a measure GPS tolerance value greater than the limit specified
        /// in GPSTolerance
        /// </summary>
        public bool GPSToleranceIsGreaterThan { get; set; }

        // AvoidZoneEnteringEvents : Boolean;
        // AvoidZoneExitingEvents : Boolean;
        // AvoidZone2DZones : Boolean;
        // AvoidZoneUndergroundServiceZones : Boolean;

        public ElevationType ElevationType { get; set; } = ElevationType.Last;

        /// <summary>
        /// The machine automatics guidance mode to be in used to record cell passes that will meet the filter.
        /// </summary>
        public MachineAutomaticsMode GCSGuidanceMode { get; set; } = MachineAutomaticsMode.Unknown;

        /// <summary>
        /// ReturnEarliestFilteredCellPass details how we choose a cell pass from a set of filtered
        /// cell passes within a cell. If set, then the first cell pass is chosen. If not set, then
        /// the latest cell pass is chosen
        /// </summary>
        public bool ReturnEarliestFilteredCellPass { get; set; }

        /// <summary>
        /// The elevation to uses as a level benchmark plane for an elevation filter
        /// </summary>
        public double ElevationRangeLevel { get; set; } = Consts.NullDouble;

        /// <summary>
        /// The vertical separation to apply from the benchmark elevation defined as a level or surface elevation
        /// </summary>
        public double ElevationRangeOffset { get; set; } = Consts.NullDouble;

        /// <summary>
        /// The thickness of the range from the level/surface benchmark + Offset to level/surface benchmark + Offset + thickness
        /// </summary>
        public double ElevationRangeThickness { get; set; } = Consts.NullDouble;

        /// <summary>
        /// The design to be used as the benchmark for a surface based elevation range filter
        /// </summary>
        public Guid ElevationRangeDesignID = Guid.Empty;
        //public DesignDescriptor ElevationRangeDesign = DesignDescriptor.Null();

        /// <summary>
        /// Elevation parameters have been initialised in preparation for elevation range filtering, either
        /// by setting ElevationRangeBottomElevationForCell and ElevationRangeTopElevationForCell or by
        /// setting ElevationRangeDesignElevations top contain relevant benchmark elevations
        /// </summary>
        public bool ElevationRangeIsInitialised { get; set; }

        /// <summary>
        /// The defined elevation range is defined only by a level plan and thickness
        /// </summary>
        public bool ElevationRangeIsLevelAndThicknessOnly { get; set; }

        /// <summary>
        /// The top of the elevation range permitted for an individual cell being filtered against as
        /// elevation range filter.
        /// </summary>
        public double ElevationRangeTopElevationForCell { get; set; } = Consts.NullDouble;

        /// <summary>
        /// The bottom of the elevation range permitted for an individual cell being filtered against as
        /// elevation range filter.
        /// </summary>
        public double ElevationRangeBottomElevationForCell { get; set; } = Consts.NullDouble;

        /// <summary>
        /// A subgrid containing sampled elevations from a benchmark surface defining the bench surface for
        /// an elevation range filter.
        /// </summary>
        [NonSerialized]
        public ClientHeightLeafSubGrid ElevationRangeDesignElevations;

        /// <summary>
        /// Denotes whether analysis of cell passes in a cell are analysed into separate layers accodring to 
        /// LayerMethod or if extracted cell passes are wrapped into a single containing layer.
        /// </summary>
        public LayerState LayerState { get; set; } = LayerState.Invalid;

        /// <summary>
        /// ID of layer we are only interested in
        /// </summary>
        public int LayerID { get; set; } = -1;

        /// <summary>
        /// Only permit cell passes recorded from a compaction type machine to be considered for filtering
        /// </summary>
        public bool RestrictFilteredDataToCompactorsOnly { get; set; }

        /// <summary>
        /// The list of surveyed surface identifiers to be exluded from the filtered result
        /// </summary>
        public Guid[] SurveyedSurfaceExclusionList { get; set; } = new Guid[0]; // note this is not saved in the database and must be set in the server

        /// <summary>
        /// The machines present in the filter represented as an array of internal machine IDs specific to the site model the filtrer is being applied to
        /// </summary>
        public short[] MachineIDs { get; set; }

        /// <summary>
        /// The machines present in the filter represented as a bitset
        /// </summary>
        public BitArray MachineIDSet { get; set; }

        /// <summary>
        /// Only permit cell passes for temperature values within min max range
        /// </summary>
        public ushort MaterialTemperatureMin { get; set; }

        /// <summary>
        /// Only permit cell passes for temperature values within min max range
        /// </summary>
        public ushort MaterialTemperatureMax { get; set; }

        /// <summary>
        /// Default no-arg constructor the produces a filter with all aspects set to their defaults
        /// </summary>
        public CellPassAttributeFilter()
        {
            ClearFilter();
        }

        public override void Prepare()
        {
            base.Prepare();

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
            // ClearAvoidZoneState();
            ClearElevationType();
            ClearGuidanceMode();
            ClearElevationRange();
            ClearCompactionMachineOnlyRestriction();
            ClearLayerID();
            ClearGPSAccuracy();
            ClearTemperatureRange();

            ClearElevationRangeFilterInitialisation();

            AnyFilterSelections = false;
            AnyMachineEventFilterSelections = false;
            AnyNonMachineEventFilterSelections = false;

            ReturnEarliestFilteredCellPass = false;
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
        private static int FlagCheck2(bool Left, bool Right) => Left ? Right ? 0 : -1 : Right ? 1 : 0;

        /// <summary>
        /// Compare two lists of machine IDs for ordering
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        private int MachineIDListsComparison(short[] list1, short[] list2)
        {
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
        public int CompareTo(CellPassAttributeFilter AFilter)
        {
            int Result;

            // Time
            Result = FlagCheck2(HasTimeFilter, AFilter.HasTimeFilter);
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
            Result = FlagCheck2(HasDesignFilter, AFilter.HasDesignFilter);
            if (Result != 0)
                return Result;

            if (HasDesignFilter) // Check the contents of the design filter
                Result = DesignNameID.CompareTo(AFilter.DesignNameID);

            if (Result != 0)
                return Result;

            // Machines
            Result = FlagCheck2(HasMachineFilter, AFilter.HasMachineFilter);
            if (Result != 0)
                return Result;

            if (HasMachineFilter)  // Check the contents of the machine filter
               Result = MachineIDListsComparison(MachineIDs, AFilter.MachineIDs);

            if (Result != 0)
                return Result;

            // Machine direction filter
            Result = FlagCheck2(HasMachineDirectionFilter, AFilter.HasMachineDirectionFilter);
            if (Result != 0)
                return Result;

            if (HasMachineDirectionFilter)  // Check the contents of the machine direction filter
                Result = MachineDirection.CompareTo(AFilter.MachineDirection); // CompareValue(Ord(MachineDirection), Ord(AFilter.MachineDirection));

            if (Result != 0)
                return Result;

            // Pass Type filter
            Result = FlagCheck2(HasPassTypeFilter, AFilter.HasPassTypeFilter);
            if (Result != 0)
            return Result;

            if (HasPassTypeFilter)  // Check the contents of the passtype filter
                if (PassTypeSet == AFilter.PassTypeSet)
                    Result = 0;
                else
                    Result = -1;

            if (Result != 0)
                return Result;

            // Vibe state filter
            Result = FlagCheck2(HasVibeStateFilter, AFilter.HasVibeStateFilter);
            if (Result != 0)
                return Result;

            if (HasVibeStateFilter)  // Check the contents of the machine filter
                Result = VibeState.CompareTo(AFilter.VibeState); // CompareValue(Ord(VibeState), Ord(AFilter.VibeState));

            if (Result != 0)
                return Result;

            // Min elev mapping
            Result = FlagCheck2(HasMinElevMappingFilter, AFilter.HasMinElevMappingFilter);
            if (Result != 0)
                return Result;

            if (HasMinElevMappingFilter)  // Check the contents of the min elevation filter
                Result = MinElevationMapping.CompareTo(AFilter.MinElevationMapping); // CompareValue(Ord(MinElevationMapping), Ord(AFilter.MinElevationMapping));

            if (Result != 0)
                return Result;

            // Elevation type
            Result = FlagCheck2(HasElevationTypeFilter, AFilter.HasElevationTypeFilter);
            if (Result != 0)
                return Result;

            if (HasElevationTypeFilter)  // Check the contents of the elevation type filter
                Result = ElevationType.CompareTo(AFilter.ElevationType); // CompareValue(Ord(ElevationType), Ord(AFilter.ElevationType));

            if (Result != 0)
                return Result;

            // Exclusion of surveyed surfaces from query
            Result = FlagCheck2(ExcludeSurveyedSurfaces(), AFilter.ExcludeSurveyedSurfaces());
            if (Result != 0)
              return Result;


            // GCS Guidance mode
            Result = FlagCheck2(HasGCSGuidanceModeFilter, AFilter.HasGCSGuidanceModeFilter);
            if (Result != 0)
                return Result;

            if (HasGCSGuidanceModeFilter)  // Check the contents of the GPS guidance mode
                Result = GCSGuidanceMode.CompareTo(AFilter.GCSGuidanceMode); // CompareValue(Ord(GCSGuidanceMode), Ord(AFilter.GCSGuidanceMode));

            if (Result != 0)
                return Result;

            // GPS Accuracy
            Result = FlagCheck2(HasGPSAccuracyFilter, AFilter.HasGPSAccuracyFilter);
            if (Result != 0)
                return Result;

            if (HasGPSAccuracyFilter)  // Check the contents of the GPS accuracy filter
            {
                Result = FlagCheck2(GPSAccuracyIsInclusive, AFilter.GPSAccuracyIsInclusive); // CompareValue(Ord(GPSAccuracyIsInclusive), Ord(AFilter.GPSAccuracyIsInclusive));
                if (Result == 0)
                    Result = GPSAccuracy.CompareTo(AFilter.GPSAccuracy); // CompareValue(Ord(GPSAccuracy), Ord(AFilter.GPSAccuracy));
            }

            if (Result != 0)
                return Result;

            // GPS Tolerance
            Result = FlagCheck2(HasGPSToleranceFilter, AFilter.HasGPSToleranceFilter);
            if (Result != 0)
                return Result;

            if (HasGPSToleranceFilter)  // Check the contents of the GPS tolerance filter
            {
                Result = FlagCheck2(GPSToleranceIsGreaterThan, AFilter.GPSToleranceIsGreaterThan); // CompareValue(Ord(GPSToleranceIsGreaterThan), Ord(AFilter.GPSToleranceIsGreaterThan));
                if (Result != 0)
                    Result = GPSTolerance.CompareTo(AFilter.GPSTolerance); // CompareValue(GPSTolerance, AFilter.GPSTolerance);
            }

            if (Result != 0)
                return Result;

            // Positioning Tech
            Result = FlagCheck2(HasPositioningTechFilter, AFilter.HasPositioningTechFilter);
            if (Result != 0)
                return Result;
            if (HasPositioningTechFilter)  // Check the contents of the positioning tech filter
                Result = PositioningTech.CompareTo(AFilter.PositioningTech); //  CompareValue(Ord(PositioningTech), Ord(AFilter.PositioningTech));
            if (Result != 0)
                return Result;

            // Elevation Range
            Result = FlagCheck2(HasElevationRangeFilter, AFilter.HasElevationRangeFilter);
            if (Result != 0)
                return Result;

            if (HasElevationRangeFilter)  // Check the contents of the elevation range filter
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

            Result = FlagCheck2(HasLayerStateFilter, AFilter.HasLayerStateFilter);
            if (Result != 0)
                return Result;
            if (HasLayerStateFilter)
                Result = LayerState.CompareTo(AFilter.LayerState); // CompareValue(Ord(LayerState), Ord(AFilter.LayerState));
            if (Result != 0)
                return Result;

            Result = FlagCheck2(HasCompactionMachinesOnlyFilter, AFilter.HasCompactionMachinesOnlyFilter);
            // Note: The compaction machines only filter is fully described by having
            // that state in the filter - there are no additional attributes to check
            if (Result != 0)
                return Result;

            // LayerID
            Result = FlagCheck2(HasLayerIDFilter, AFilter.HasLayerIDFilter);
            if (Result != 0)
                return Result;
            if (HasLayerIDFilter)
                Result = LayerID.CompareTo(AFilter.LayerID); // CompareValue(Ord(LayerID), Ord(AFilter.LayerID));
            if (Result != 0)
                return Result;

            // TemperatureRangeFilter
            Result = FlagCheck2(HasTemperatureRangeFilter, AFilter.HasTemperatureRangeFilter);
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
          MaterialTemperatureMin = CellPass.NullMaterialTemperatureValue;
          MaterialTemperatureMax = CellPass.NullMaterialTemperatureValue;
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

        public void Assign(CellPassAttributeFilter Source)
        {
            siteModel = Source.siteModel;
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

            /*
              AvoidZoneEnteringEvents  = Source.FAvoidZoneEnteringEvents;
              AvoidZoneExitingEvents  = Source.FAvoidZoneExitingEvents;
              AvoidZone2DZones  = Source.FAvoidZone2DZones;
              AvoidZoneUndergroundServiceZones = Source.FAvoidZoneUndergroundServiceZones;
            */

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
            

            Prepare();

        }

        /*
public void TICGridDataPassFilter.ClearAvoidZoneState;
{
Exclude(FilterSelections, icfsInAvoidZone);

FAvoidZoneEnteringEvents = false;
FAvoidZoneExitingEvents = false;

FAvoidZone2DZones = false;
FAvoidZoneUndergroundServiceZones = false;
}
*/

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
        public override bool FilterPass(ref CellPass PassValue)
        {
            int StateChangeIndex;
            int DesignNameIDValue = Consts.kNoDesignNameID;
            VibrationState VibeStateValue = VibrationState.Invalid;
            MachineGear MachineGearValue = MachineGear.Null;
            bool MinElevMappingValue = false;
            GPSAccuracyAndTolerance GPSAccuracyAndToleranceValue = GPSAccuracyAndTolerance.Null();
            PositioningTech PositioningTechStateValue = PositioningTech.Unknown;
            MachineAutomaticsMode GCSGuidanceModeValue = MachineAutomaticsMode.Unknown;
            Machines.Machine Machine;
            ushort LayerIDStateValue = ushort.MaxValue;  // ID of current layer

            /* SPR 10733: AZ filters should only apply to AZ transgression events
            InAvoidZoneStateValue: TICInAvoidZoneState;
            AvoidZoneEvent : TICEventInAvoidZoneStateValueChangeBase;
            AZ2DResult : Boolean;
            AZUSResult : Boolean;
            */

            if (!AnyFilterSelections)
            {
                // There are no constrictive filter criteria - all cell passes pass the filter
                return false;
            }

            /* TODO
              {$IFOPT C+}
              if not Assigned(FOwner) 
                {
                  SIGLogMessage.Publish(Self, 'TICGridDataPassFilter.FilterPass: SiteModel owner not set for filter', slmcError); {SKIP}
                  Result = false;
                  Exit;
                }
              {$ENDIF}
            */

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
                Machine = siteModel.Machines[PassValue.InternalSiteModelMachineIndex];
                
                if (Machine != null && !Machine.MachineIsConpactorType())
                    return false;
            }

            // Extract the list of events for the machine referred to in the cell pass. Use this
            // reference for all filter criteria that depend on machine events
            ProductionEventLists machineTargetValues = AnyMachineEventFilterSelections ? siteModel.MachinesTargetValues[PassValue.InternalSiteModelMachineIndex] : null;

            if (HasDesignFilter)
            {
                DesignNameIDValue = machineTargetValues.DesignNameIDStateEvents.GetValueAtDate(PassValue.Time, out StateChangeIndex, DesignNameIDValue);

                if ((DesignNameIDValue != Consts.kAllDesignsNameID) && (DesignNameID != DesignNameIDValue))
                    return false;
            }

            if (HasVibeStateFilter)
            {
                VibeStateValue = machineTargetValues.VibrationStateEvents.GetValueAtDate(PassValue.Time, out StateChangeIndex, VibeStateValue);

                if (VibeState != VibeStateValue)
                    return false;
            }

            if (HasGCSGuidanceModeFilter)
            {
                GCSGuidanceModeValue = machineTargetValues.MachineAutomaticsStateEvents.GetValueAtDate(PassValue.Time, out StateChangeIndex, GCSGuidanceModeValue);

                if (GCSGuidanceMode != GCSGuidanceModeValue)
                    return false;
            }

            if (HasMachineDirectionFilter)
            {
                MachineGearValue = machineTargetValues.MachineGearStateEvents.GetValueAtDate(PassValue.Time, out StateChangeIndex, MachineGearValue);

                if (((MachineDirection == MachineDirection.Forward && !TRex.Machines.Machine.MachineGearIsForwardGear(MachineGearValue))) ||
                    ((MachineDirection == MachineDirection.Reverse && !TRex.Machines.Machine.MachineGearIsReverseGear(MachineGearValue))))
                    return false;
            }

            if (HasMinElevMappingFilter)
            {
                MinElevMappingValue = machineTargetValues.MinElevMappingStateEvents.GetValueAtDate(PassValue.Time, out StateChangeIndex, MinElevMappingValue);

                if (MinElevationMapping != MinElevMappingValue)
                    return false;
            }

            /* SPR 10733: AZ filters should only apply to AZ transgression events
            if icfsInAvoidZone in FilterSelections 
            {
              if FAvoidZone2DZones 
                {
                  AvoidZoneEvent = LocateInAvoidZone2DStateValueAtDate(PassValue.MachineID, PassValue.Time, InAvoidZoneStateValue);

      AZ2DResult = Assigned(AvoidZoneEvent) and
                    (((FAvoidZoneEnteringEvents and AvoidZoneEvent.IsInsideAvoidanceZone) OR
                      (FAvoidZoneExitingEvents and AvoidZoneEvent.IsOutsideAvoidanceZone))
                                AND
                                AvoidZoneEvent.Is2DZoneAvoidanceZone);
      end
              else
                AZ2DResult = true;

              if FAvoidZoneUndergroundServiceZones 
                {
                  AvoidZoneEvent = LocateInAvoidZoneUSStateValueAtDate(PassValue.MachineID, PassValue.Time, InAvoidZoneStateValue);

      AZUSResult = Assigned(AvoidZoneEvent) and
                    (((FAvoidZoneEnteringEvents and AvoidZoneEvent.IsInsideAvoidanceZone) OR
                      (FAvoidZoneExitingEvents and AvoidZoneEvent.IsOutsideAvoidanceZone))
                                AND
                                AvoidZoneEvent.IsUndergroundServiceAvoidanceZone);
      end
              else
                AZUSResult = true;

              if not(AZ2DResult or AZUSResult) 
               Exit;
      }
            */

            if (HasGPSAccuracyFilter || HasGPSToleranceFilter)
            {
                GPSAccuracyAndToleranceValue = machineTargetValues.GPSAccuracyAndToleranceStateEvents.GetValueAtDate(PassValue.Time, out StateChangeIndex, GPSAccuracyAndToleranceValue);

                if (HasGPSAccuracyFilter && GPSAccuracy != GPSAccuracyAndToleranceValue.GPSAccuracy && !GPSAccuracyIsInclusive)
                    return false;

                if (HasGPSAccuracyFilter && GPSAccuracyIsInclusive && GPSAccuracy < GPSAccuracyAndToleranceValue.GPSAccuracy)
                    return false;

                if (HasGPSToleranceFilter &&
                    !(GPSAccuracyAndToleranceValue.GPSTolerance != CellPass.NullGPSTolerance &&
                      ((!GPSToleranceIsGreaterThan && GPSAccuracyAndToleranceValue.GPSTolerance < GPSTolerance) ||
                       (GPSToleranceIsGreaterThan && GPSAccuracyAndToleranceValue.GPSTolerance >= GPSTolerance))))
                    return false;
            }

            if (HasPositioningTechFilter)
            {
                PositioningTechStateValue = machineTargetValues.PositioningTechStateEvents.GetValueAtDate(PassValue.Time, out StateChangeIndex, PositioningTechStateValue);

                if (PositioningTech != PositioningTechStateValue)
                    return false;
            }

            // Filter on LayerID
            if (HasLayerIDFilter)
            {
                LayerIDStateValue = machineTargetValues.LayerIDStateEvents.GetValueAtDate(PassValue.Time, out StateChangeIndex, LayerIDStateValue);
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
            if (HasTemperatureRangeFilter)
            {
              if (!FilterPassUsingTemepratureRange(ref PassValue))
                return false;
            }


            return true;
        }

        public override bool FilterPass(ref FilteredPassData PassValue)
        {
            /* SPR 10733: AZ filters should only apply to AZ transgression events
            AZ2DResult : Boolean;
          AZUSResult : Boolean;
            */

            Machines.Machine Machine;

            if (!AnyFilterSelections)
            {
                return true;
            }

            /* TODO readd when ifopt C+ understood in c#
  {$IFOPT C+}
  if not Assigned(FOwner) 
    {
      SIGLogMessage.Publish(Self, 'TICGridDataPassFilter.FilterPass: SiteModel owner not set for filter', slmcError); {SKIP}
      Result = false;
      Exit;
    }
  {$ENDIF}
  */

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
                Machine = siteModel.Machines[PassValue.FilteredPass.InternalSiteModelMachineIndex];
                if (Machine != null && !Machine.MachineIsConpactorType())
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

            /* SPR 10733: AZ filters should only apply to AZ transgression events
            if icfsInAvoidZone in FilterSelections 
              {
                if FAvoidZone2DZones 
                  AZ2DResult = (FAvoidZoneEnteringEvents and((PassValue.EventValues.EventInAvoidZoneState and kICInAvoidZone2DMask) != 0)) OR
                               (FAvoidZoneExitingEvents and ((PassValue.EventValues.EventInAvoidZoneState and kICInAvoidZone2DMask) = 0))
                else
                  AZ2DResult = true;

                if FAvoidZoneUndergroundServiceZones 
                  AZUSResult = (FAvoidZoneEnteringEvents and((PassValue.EventValues.EventInAvoidZoneState and kICInAvoidZoneUndergroundServiceMask) != 0)) OR
                               (FAvoidZoneExitingEvents and ((PassValue.EventValues.EventInAvoidZoneState and kICInAvoidZoneUndergroundServiceMask) = 0))
                else
                  AZUSResult = true;

                if not(AZ2DResult or AZUSResult) 
                 Exit;
          }
            */

            if (HasGPSAccuracyFilter)
            {
                if (GPSAccuracy != PassValue.EventValues.GPSAccuracy && !GPSAccuracyIsInclusive)
                    return false;

                if (GPSAccuracyIsInclusive && GPSAccuracy < PassValue.EventValues.GPSAccuracy)
                    return false;
            }

            if (HasGPSToleranceFilter)
            {
                if (!((PassValue.EventValues.GPSTolerance != CellPass.NullGPSTolerance) &&
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
            if (HasTemperatureRangeFilter)
            {
              if (!FilterPassUsingTemepratureRange(ref PassValue.FilteredPass))
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


      public bool FilterPassUsingTemepratureRange(ref CellPass PassValue)
      {
        Debug.Assert(HasTemperatureRangeFilter, "Temperature range filter being used without the temperature range data being initialised");
        return (PassValue.MaterialTemperature != CellPass.NullMaterialTemperatureValue) &&
               Range.InRange(PassValue.MaterialTemperature, MaterialTemperatureMin, MaterialTemperatureMax);
      }




    public bool FilterPassUsingTimeOnly(ref CellPass PassValue)
        {
            // TODO readd when ifopt c+ is understood
            /*  {$IFOPT C+}
              if not Assigned(FOwner) 
                {
                  SIGLogMessage.Publish(Self, 'TICGridDataPassFilter.FilterPassUsingTimeOnly: SiteModel owner not set for filter', slmcError); {SKIP}
                  Result = false;
                  Exit;
                }
              {$ENDIF} */

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
            /*
               SPR 10733: AZ filters should only apply to AZ transgression events
              AZ2DResult : Boolean;
              AZUSResult : Boolean;
            */

            if (!AnyMachineEventFilterSelections)
            {
                // There are no constrictive machine events filter criteria - all cell passes pass the filter
                return true;
            }

            // TODO readd when IFOPT c+ understood in c#
            /*{$IFOPT C+}
            if not Assigned(FOwner) 
              {
                SIGLogMessage.Publish(Self, 'TICGridDataPassFilter.FilterPass: SiteModel owner not set for filter', slmcError); {SKIP}
                Result = false;
                Exit;
              }
            {$ENDIF} */

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

            /* SPR 10733: AZ filters should only apply to AZ transgression events
            if icfsInAvoidZone in FilterSelections 
              {
                if FAvoidZone2DZones 
                  AZ2DResult = (FAvoidZoneEnteringEvents and((PassValue.EventValues.EventInAvoidZoneState and kICInAvoidZone2DMask) != 0)) OR
                               (FAvoidZoneExitingEvents and ((PassValue.EventValues.EventInAvoidZoneState and kICInAvoidZone2DMask) = 0))
                else
                  AZ2DResult = true;

                if FAvoidZoneUndergroundServiceZones 
                  AZUSResult = (FAvoidZoneEnteringEvents and((PassValue.EventValues.EventInAvoidZoneState and kICInAvoidZoneUndergroundServiceMask) != 0)) OR
                               (FAvoidZoneExitingEvents and ((PassValue.EventValues.EventInAvoidZoneState and kICInAvoidZoneUndergroundServiceMask) = 0))
                else
                  AZUSResult = true;

                if not(AZ2DResult or AZUSResult) 
                 Exit;
          }
            */

            if (HasGPSAccuracyFilter)
            {
                if (GPSAccuracy != PassValue.EventValues.GPSAccuracy && !GPSAccuracyIsInclusive)
                    return false;

                if (GPSAccuracyIsInclusive && GPSAccuracy < PassValue.EventValues.GPSAccuracy)
                    return false;
            }

            if (HasGPSToleranceFilter)
            {
                if (!(PassValue.EventValues.GPSTolerance != CellPass.NullGPSTolerance &&
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
            Machine Machine;

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
                Machine = siteModel.Machines[PassValue.InternalSiteModelMachineIndex];

                if (Machine != null && !Machine.MachineIsConpactorType())
                    return false;
            }


            if (HasTemperatureRangeFilter)
            {
              if (!FilterPassUsingTemepratureRange(ref PassValue))
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
                                     ref FilteredSinglePassInfo FilteredPassInfo
                                                //             ref FilteredMultiplePassInfo FilteredPassesBuffer)
                                                // TODO add when cell profile available... ProfileCell CellProfile
                                                )
        {
            return base.FilterSinglePass(PassValues,
                                         PassValueCount,
                                         ReturnEarliestFilteredCellPass,
                                         ref FilteredPassInfo,
                                         // TODO readd when available CellProfile,
                                         true);
        }

        /*
    public void TICGridDataPassFilter.GetCurrentSettings(const Settings: TICFilterSettings);
    {
      if not Assigned(Settings) 
        Exit;

    Settings.StartTime    = FStartTime;
      Settings.EndTime      = FEndTime;
      Settings.Machines     = FMachines;
      Settings.DesignNameID = FDesignNameID;
      Settings.VibeState    = FVibeState;
      Settings.LayerState   = FLayerState;
      Settings.MachineDirection = FMachineDirection;
      Settings.MinElevationMapping = FMinElevationMapping;
      Settings.PositioningTech = FPositioningTech;
      Settings.GPSAccuracy = FGPSAccuracy;
      Settings.GPSTolerance = FGPSTolerance;
      Settings.GPSToleranceIsGreaterThan = FGPSToleranceIsGreaterThan;
      Settings.AvoidZoneEnteringEvents = FAvoidZoneEnteringEvents;
      Settings.AvoidZoneExitingEvents = FAvoidZoneExitingEvents;
      Settings.AvoidZone2DZones = FAvoidZone2DZones;
      Settings.AvoidZoneUndergroundServiceZones = FAvoidZoneUndergroundServiceZones;
      Settings.ElevationType   = FElevationType;
      Settings.GCSGuidanceMode = FGCSGuidanceMode;
      Settings.ReturnEarliestFilteredCellPass = FReturnEarliestFilteredCellPass;
      Settings.PassFilterSelections = FilterSelections;
      Settings.LayerID = FLayerID;
      Settings.PassTypeSelections = FPassTypeSet;
      Settings.ElevationRangeLevel = FElevationRangeLevel;
      Settings.ElevationRangeOffset =  FElevationRangeOffset;
      Settings.ElevationRangeThickness  =  FElevationRangeThickness;
      Settings.ElevationRangeDesign  = FElevationRangeDesign;
      Settings.ElevationType = FElevationType;

    }
    */

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

        /*
    public void TICGridDataPassFilter.Initialise(const Settings: TICFilterSettings);
    var i : Integer;
    {
      if not Assigned(Settings) 
        Exit;

    FStartTime                            = Settings.StartTime;
      FEndTime                              = Settings.EndTime;

      FMachines                             = Settings.Machines;
      InitialiseMachineIDsSet;

      FDesignNameID                         = Settings.DesignNameID;
      FVibeState                            = Settings.VibeState;
      FLayerState                           = Settings.LayerState;
      FMachineDirection                     = Settings.MachineDirection;
      FMinElevationMapping                  = Settings.MinElevationMapping;

      FPositioningTech                      = Settings.PositioningTech;

      FGPSTolerance                         = Settings.GPSTolerance;
      FGPSAccuracyIsInclusive               = Settings.GPSAccuracyIsInclusive;
      FGPSToleranceIsGreaterThan            = Settings.GPSToleranceIsGreaterThan;
      FGPSAccuracy                          = Settings.GPSAccuracy;

      FAvoidZoneEnteringEvents              = Settings.AvoidZoneEnteringEvents;
      FAvoidZoneExitingEvents               = Settings.AvoidZoneExitingEvents;
      FAvoidZone2DZones                     = Settings.AvoidZone2DZones;
      FAvoidZoneUndergroundServiceZones     = Settings.AvoidZoneUndergroundServiceZones;

      FElevationType                        = Settings.ElevationType;
      FGCSGuidanceMode                      = Settings.GCSGuidanceMode;

      FReturnEarliestFilteredCellPass       = Settings.ReturnEarliestFilteredCellPass;

      FElevationRangeDesign                 = Settings.ElevationRangeDesign;
      FElevationRangeLevel                  = Settings.ElevationRangeLevel;
      FElevationRangeOffset                 = Settings.ElevationRangeOffset;
      FElevationRangeThickness              = Settings.ElevationRangeThickness;

      FRestrictFilteredDataToCompactorsOnly = Settings.HasCompactionMachinesOnlyComponent;

      FilterSelections                      = Settings.PassFilterSelections;

      SetLength(FSurveyedSurfaceExclusionList, Length(Settings.SurveyedSurfaceExclusionList));
      for i = Low(Settings.SurveyedSurfaceExclusionList) to High(Settings.SurveyedSurfaceExclusionList) do
        FSurveyedSurfaceExclusionList[i]=Settings.SurveyedSurfaceExclusionList[i];

      FLayerID                              = Settings.LayerID;
      FPassTypeSet                          = Settings.PassTypeSelections;
    }
    */

        /*
         * public void TICGridDataPassFilter.Initialise(const AOwner: TObject;
    const Settings: TICFilterSettings);
    {
      SetOwner(AOwner);

      Initialise(Settings);
    }
    */

        public void InitialiseElevationRangeFilter(ClientHeightLeafSubGrid DesignElevations)
        {
            // If there is a design specified then intialise the filter using the design elevations
            // queried and supplied by the caller, otherwise the specified Elevation level, offset and thickess
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

        // LastRecordedCellPassSatisfiesFilter denotes whether the settings in the filter
        // may be satisfied by examining the last recorded value wrt the subgrid information
        // currently being requested. This allows the cached latest recorded values slice
        // stored

        public bool LastRecordedCellPassSatisfiesFilter => !AnyFilterSelections && !ReturnEarliestFilteredCellPass;

        /* FilterEvent is not used in any apparent context in current gen systems, though it may have been used by SiteVision office for timeline requests
                // Returns true if passes are returned which meet the set filter (if any)
                public bool FilterEvent(ProductionEventChanges EventList, ProductionEventChangeBase Event)
                {
                    bool EventMeetsMachineFilter;
                    //  AvoidZoneEvent                  :TICEventInAvoidZoneStateValueChangeBase;
                    //  FilteredAZEventOK               :Boolean;
                    //  ComparingIncompatibleAZs: Boolean;
                    DesignNameEvent: TICEventDesignNameValueChange;
                    VibeStateChange: TICEventVibrationStateValueChange;
                    MachinegearChange: TICEventMachineGearValueChange;
                    MinElevMappingChange: TICEventMinElevMappingStateValueChange;
                    PositioningTechStateChange: TICEventPositioningTechStateValueChange;
                    //  LayerIDStateChange              :TICLayerIDValueChangeList; ??
                    GCSGuidanceModeChange: TICEventMachineAutomaticsStateChange;
                    Machine Machine;
                    int StateChangeIndex;
                    short GPSToleranceValue;
                    GPSAccuracy GPSAccuracyValue;
                    long DesignNameEventValue;
                    VibrationState VibeStateChangeValue;
                    MachineGear MachinegearChangeValue;
                    bool MinElevMappingChangeValue;
                    PositioningTech PositioningTechStateChangeValue;
                    // LayerIDStateChangeValue         :TICLayerState; // ??
                    MachineAutomaticsMode GCSGuidanceModeChangeValue;

                    Machine = Owner.MachinesTargetValues.LocateByMachineID(EventList.MachineID);
                    if (Machine == null)
                        return false;

                    if (HasTimeFilter)
                    {
                        if (StartTime == DateTime.MinValue)
                        {
                            // It's an End/As At time filter
                            if (Event.Date > EndTime)
                                return false;
                        }

                        // In that case it's a time range filter
                        if (Event.Date < StartTime || Event.Date > EndTime)
                            return false;
                    }

                    if (HasMachineFilter)
                    {
                        EventMeetsMachineFilter = false;

                        for (int i = 0; i < Machines.Length; i++)
                        {
                            if (EventList.MachineID = Machines[i].ID)
                            {
                                EventMeetsMachineFilter = true;
                                break;
                            }
                        }

                        if (!EventMeetsMachineFilter)
                            return false;
                    }

                    // Note: If one type of avoidance zone event is being filtered against another type of
                    //       avoidance zone event, then just pass this filtering step, as avoidance zones
                    //       are opaque to each others filtering. Filtering is only applied within the
                    //       same avoidance zone type in this instance. This is actually a different set
                    //       of semantics when compared to grid filtering where combinations of avoidance
                    //       zones are applied to filter cell passes
                    //(*
                     *  if icfsInAvoidZone in FilterSelections 
                        {
                          FilteredAZEventOK = false;

                          if FAvoidZone2DZones 
                            {
                              AvoidZoneEvent = Nil;
                              ComparingIncompatibleAZs = false;

                              if EventList is TICEventInAvoidZoneStateValueChangeList 
                                {
                                  if Event is TICEventInAvoidZone2DStateValueChange 
                                    AvoidZoneEvent = TICEventInAvoidZoneStateValueChangeBase(Event)
                                  else
                                    ComparingIncompatibleAZs = true;
                                end
                              else
                                {
                                  Machine.TargetValueChanges.EventMachineInAvoidZone2DStates.GetValueAtDate(Event.EventDate, StateChangeIndex);
                                  if StateChangeIndex!= -1 
                                   AvoidZoneEvent = TICEventInAvoidZoneStateValueChangeBase(Machine.TargetValueChanges.EventMachineInAvoidZone2DStates[StateChangeIndex]);
                    }

                              if not ComparingIncompatibleAZs 
                                {
                                  if not Assigned(AvoidZoneEvent) 
                                    Exit;

                                  if not((FAvoidZoneEnteringEvents and AvoidZoneEvent.IsInsideAvoidanceZone) OR
                                         (FAvoidZoneExitingEvents and AvoidZoneEvent.IsOutsideAvoidanceZone)) 
                                   Exit;

                    FilteredAZEventOK = true;
                                }
                            }

                          if not FAvoidZoneUndergroundServiceZones and not FilteredAZEventOK 
                            Exit;

                          if FAvoidZoneUndergroundServiceZones 
                            {
                              AvoidZoneEvent = Nil;
                              ComparingIncompatibleAZs = false;

                              if EventList is TICEventInAvoidZoneStateValueChangeList 
                                {
                                  if Event is TICEventInAvoidZoneUSStateValueChange 
                                    AvoidZoneEvent = TICEventInAvoidZoneStateValueChangeBase(Event)
                                  else
                                    ComparingIncompatibleAZs = true;
                                end
                              else
                                {
                                  Machine.TargetValueChanges.EventMachineInAvoidZoneUSStates.GetValueAtDate(Event.EventDate, StateChangeIndex);
                                  if StateChangeIndex!= -1 
                                   AvoidZoneEvent = TICEventInAvoidZoneStateValueChangeBase(Machine.TargetValueChanges.EventMachineInAvoidZoneUSStates[StateChangeIndex])
                                  else
                                    AvoidZoneEvent = Nil;
                                }

                              if not ComparingIncompatibleAZs 
                                {
                                  if not Assigned(AvoidZoneEvent) 
                                    Exit;

                                  if not((FAvoidZoneEnteringEvents and AvoidZoneEvent.IsInsideAvoidanceZone) OR
                                         (FAvoidZoneExitingEvents and AvoidZoneEvent.IsOutsideAvoidanceZone)) 
                                   Exit;

                    FilteredAZEventOK = true;
                                }
                            }

                          if not FilteredAZEventOK 
                            Exit;
                    }
                    //*)

                    if (HasDesignFilter)
                    {
                        DesignNameEvent = null;
                        DesignNameEventValue = Consts.kNoDesignNameID;

                        if (EventList is TICEventDesignNameValueChangeList)
                        {
                            DesignNameEvent = TICEventDesignNameValueChange(Event);
                        }
                        else
                        {
                            DesignNameEventValue = Machine.TargetValueChanges.EventDesignNames.GetValueAtDate(Event.EventDate, StateChangeIndex);
                        }

                        if (DesignNameEvent != null)
                        {
                            DesignNameEventValue = DesignNameEvent.EventDesignNameID;
                        }

                        if (DesignNameID != Consts.kAllDesignsNameID && DesignNameID != DesignNameEventValue)
                            return false;
                    }

                    if (HasVibeStateFilter)
                    {
                        VibeStateChange = null;
                        VibeStateChangeValue = VibrationState.Invalid;

                        if (EventList is TICEventVibrationStateValueChangeList)
                        {
                            VibeStateChange = TICEventVibrationStateValueChange(Event);
                        }
                        else
                        {
                            VibeStateChangeValue = Machine.TargetValueChanges.EventVibrationStates.GetValueAtDate(Event.EventDate, StateChangeIndex);
                        }

                        if (VibeStateChange != null)
                        {
                            VibeStateChangeValue = VibeStateChange.EventVibrationState;
                        }

                        if (VibeState != VibeStateChangeValue)
                            return false;
                    }

                    if (HasMachineFilter)
                    {
                        MachinegearChangeValue = MachineGear.Null;
                        MachinegearChange = null;

                        if (EventList is TICEventMachineGearValueChangeList)
                        {
                            MachinegearChange = TICEventMachineGearValueChange(Event);
                        }
                        else
                        {
                            MachinegearChangeValue = Machine.TargetValueChanges.EventMachineGears.GetValueAtDate(Event.EventDate, StateChangeIndex);
                        }

                        if (MachinegearChange != null)
                        {
                            MachinegearChangeValue = MachinegearChange.EventMachineGear;
                        }

                        if ((MachineDirection == MachineDirection.Forward && !(Machine.MachineGearIsForwardGear(MachinegearChangeValue))) ||
                            (MachineDirection == MachineDirection.Reverse && !(Machine.MachineGearIsReverseGear(MachinegearChangeValue))))
                            return false;
                    }

                    if (HasMinElevMappingFilter)
                    {
                        MinElevMappingChangeValue = false;
                        MinElevMappingChange = null;

                        if (EventList is TICEventMachineGearValueChangeList)
                        {
                            MinElevMappingChange = TICEventMinElevMappingStateValueChange(Event);
                        }
                        else
                        {
                            MinElevMappingChangeValue = Machine.TargetValueChanges.EventMachineMinEleveMappingStates.GetValueAtDate(Event.EventDate, StateChangeIndex);
                        }

                        if (MinElevMappingChange != null)
                        {
                            MinElevMappingChangeValue = MinElevMappingChange.MinElevMappingState;
                        }

                        if (MinElevationMapping != MinElevMappingChangeValue)
                            return false;
                    }

                    if (HasGCSGuidanceModeFilter)
                    {
                        GCSGuidanceModeChange = null;
                        GCSGuidanceModeChangeValue = MachineAutomaticsMode.Unknown;

                        if (EventList is TICEventMachineAutomaticsValueChangeList)
                        {
                            GCSGuidanceModeChange = TICEventMachineAutomaticsStateChange(Event);
                        }
                        else
                        {
                            GCSGuidanceModeChangeValue = Machine.TargetValueChanges.EventMachineAutomaticsStates.GetValueAtDate(Event.EventDate, StateChangeIndex);
                        }

                        if (GCSGuidanceModeChange != null)
                        {
                            GCSGuidanceModeChangeValue = GCSGuidanceModeChange.EventMachineAutomatics;
                        }

                        if (GCSGuidanceMode != GCSGuidanceModeChangeValue)
                            return false;
                    }

                    if (HasGPSAccuracyFilter || HasGPSToleranceFilter)
                    {
                        if (EventList is TICEventPositionAccuracyValueChangeList)
                        {
                            GPSAccuracyValue = TICGPSAccuracyStateValueChange(Event).GPSAccuracyState;
                            GPSToleranceValue = TICGPSAccuracyStateValueChange(Event).GPSAccuracyErrorLimit;
                        }
                        else
                        {
                            GPSAccuracyValue = Machine.TargetValueChanges.EventGPSAccuracyStates.GetValuesAtDate(Event.EventDate, StateChangeIndex, GPSToleranceValue);
                        }

                        if (GPSAccuracyValue != GPSAccuracy.Unknown)
                        {
                            if (HasGPSAccuracyFilter)
                            {
                                if (GPSAccuracy != GPSAccuracyValue && !GPSAccuracyIsInclusive)
                                    return false;

                                if (GPSAccuracyIsInclusive && GPSAccuracy < GPSAccuracyValue)
                                    return false;
                            }

                            if (HasGPSToleranceFilter &&
                               !((GPSToleranceValue != CellPass.NullGPSTolerance) &&
                                 ((!GPSToleranceIsGreaterThan && GPSToleranceValue < GPSTolerance) ||
                                  (GPSToleranceIsGreaterThan && GPSToleranceValue >= GPSTolerance))))
                          return false;
                        }
                    }

                    if (HasPositioningTechFilter)
                    {
                        PositioningTechStateChangeValue = PositioningTech.Unknown;
                        PositioningTechStateChange = null;

                        if (EventList is TICEventPositioningTechStateValueChangeList)
                        {
                            PositioningTechStateChange = TICEventPositioningTechStateValueChange(Event);
                        }
                        else
                        {
                            PositioningTechStateChangeValue = Machine.TargetValueChanges.EventPositioningTechStates.GetValueAtDate(Event.EventDate, StateChangeIndex);
                        }

                        if (PositioningTechStateChange != null)
                        {
                            PositioningTechStateChangeValue = PositioningTechStateChange.PositioningTechState;
                        }

                        if (PositioningTech != PositioningTechStateChangeValue)
                            return false;
                    }

                    return true;
                }
        */

        // FilterMultiplePasses selects a set of passes from the list of passes
        // in <PassValues> where <PassValues> contains the entire
        // list of passes for a cell in the database.

        public override bool FilterMultiplePasses(CellPass[] passValues,
                                                  int PassValueCount,
                                                  ref FilteredMultiplePassInfo filteredPassInfo)
        {
            if (!AnyFilterSelections)
            {
                return base.FilterMultiplePasses(passValues, PassValueCount, ref filteredPassInfo);
            }

            bool Result = false;

            for (int i = 0; i < PassValueCount; i++)
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
    }
}
