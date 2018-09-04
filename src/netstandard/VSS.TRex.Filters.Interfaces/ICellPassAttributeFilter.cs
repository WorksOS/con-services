using System;
using System.Collections;
using VSS.TRex.Cells;
using VSS.TRex.Filters.Models;
using VSS.TRex.Types;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Filters.Interfaces
{
  public interface ICellPassAttributeFilter
  {
    object /*ISiteModel*/ SiteModel { get; set; }

    /// <summary>
    /// The earliest time that a measured cell pass must have to be included in the filter
    /// </summary>
    DateTime StartTime { get; set; }

    /// <summary>
    /// The latest time that a measured cell pass must have to be included in the filter
    /// </summary>
    DateTime EndTime { get; set; }

    Guid[] MachinesList { get; set; }
    int DesignNameID { get; set; } // DesignNameID :TICDesignNameID;
    VibrationState VibeState { get; set; }
    MachineDirection MachineDirection { get; set; }
    PassTypeSet PassTypeSet { get; set; }
    bool MinElevationMapping { get; set; } //MinElevationMapping : TICMinElevMappingState;
    PositioningTech PositioningTech { get; set; }
    ushort GPSTolerance { get; set; }
    bool GPSAccuracyIsInclusive { get; set; }
    GPSAccuracy GPSAccuracy { get; set; }

    /// <summary>
    /// The filter will select cell passes with a measure GPS tolerance value greater than the limit specified
    /// in GPSTolerance
    /// </summary>
    bool GPSToleranceIsGreaterThan { get; set; }

    ElevationType ElevationType { get; set; }

    /// <summary>
    /// The machine automatics guidance mode to be in used to record cell passes that will meet the filter.
    /// </summary>
    MachineAutomaticsMode GCSGuidanceMode { get; set; }

    /// <summary>
    /// ReturnEarliestFilteredCellPass details how we choose a cell pass from a set of filtered
    /// cell passes within a cell. If set, then the first cell pass is chosen. If not set, then
    /// the latest cell pass is chosen
    /// </summary>
    bool ReturnEarliestFilteredCellPass { get; set; }

    /// <summary>
    /// The elevation to uses as a level benchmark plane for an elevation filter
    /// </summary>
    double ElevationRangeLevel { get; set; }

    /// <summary>
    /// The vertical separation to apply from the benchmark elevation defined as a level or surface elevation
    /// </summary>
    double ElevationRangeOffset { get; set; }

    /// <summary>
    /// The thickness of the range from the level/surface benchmark + Offset to level/surface benchmark + Offset + thickness
    /// </summary>
    double ElevationRangeThickness { get; set; }

    /// <summary>
    /// The design to be used as the benchmark for a surface based elevation range filter
    /// </summary>
    Guid ElevationRangeDesignID { get; set; }

    /// <summary>
    /// Elevation parameters have been initialised in preparation for elevation range filtering, either
    /// by setting ElevationRangeBottomElevationForCell and ElevationRangeTopElevationForCell or by
    /// setting ElevationRangeDesignElevations top contain relevant benchmark elevations
    /// </summary>
    bool ElevationRangeIsInitialised { get; set; }

    /// <summary>
    /// The defined elevation range is defined only by a level plan and thickness
    /// </summary>
    bool ElevationRangeIsLevelAndThicknessOnly { get; set; }

    /// <summary>
    /// The top of the elevation range permitted for an individual cell being filtered against as
    /// elevation range filter.
    /// </summary>
    double ElevationRangeTopElevationForCell { get; set; }

    /// <summary>
    /// The bottom of the elevation range permitted for an individual cell being filtered against as
    /// elevation range filter.
    /// </summary>
    double ElevationRangeBottomElevationForCell { get; set; }

    /// <summary>
    /// Denotes whether analysis of cell passes in a cell are analysed into separate layers accodring to 
    /// LayerMethod or if extracted cell passes are wrapped into a single containing layer.
    /// </summary>
    LayerState LayerState { get; set; }

    /// <summary>
    /// ID of layer we are only interested in
    /// </summary>
    int LayerID { get; set; }

    /// <summary>
    /// Only permit cell passes recorded from a compaction type machine to be considered for filtering
    /// </summary>
    bool RestrictFilteredDataToCompactorsOnly { get; set; }

    /// <summary>
    /// The list of surveyed surface identifiers to be exluded from the filtered result
    /// </summary>
    Guid[] SurveyedSurfaceExclusionList { get; set; } // note this is not saved in the database and must be set in the server

    /// <summary>
    /// The machines present in the filter represented as an array of internal machine IDs specific to the site model the filtrer is being applied to
    /// </summary>
    short[] MachineIDs { get; set; }

    /// <summary>
    /// The machines present in the filter represented as a bitset
    /// </summary>
    BitArray MachineIDSet { get; set; }

    /// <summary>
    /// Only permit cell passes for temperature values within min max range
    /// </summary>
    ushort MaterialTemperatureMin { get; set; }

    /// <summary>
    /// Only permit cell passes for temperature values within min max range
    /// </summary>
    ushort MaterialTemperatureMax { get; set; }

    /// <summary>
    /// takes final filtered passes and reduces to the set to passes within the min max passcount range
    /// </summary>
    ushort PasscountRangeMin { get; set; }

    /// <summary>
    ///  takes final filtered passes and reduces to the set to passes within the min max passcount range
    /// </summary>
    ushort PasscountRangeMax { get; set; }

    bool LastRecordedCellPassSatisfiesFilter { get; }

    /// <summary>
    /// RequestedGridDataType stores the type of grid data being requested at
    /// the time this filter is asked filter cell passes.
    /// </summary>
    GridDataType RequestedGridDataType { get; set; }

    bool HasTimeFilter { get; set; }
    bool HasMachineFilter { get; set; }
    bool HasMachineDirectionFilter { get; set; }
    bool HasDesignFilter { get; set; }
    bool HasVibeStateFilter { get; set; }
    bool HasLayerStateFilter { get; set; }
    bool HasMinElevMappingFilter { get; set; }
    bool HasElevationTypeFilter { get; set; }
    bool HasGCSGuidanceModeFilter { get; set; }
    bool HasGPSAccuracyFilter { get; set; }
    bool HasGPSToleranceFilter { get; set; }
    bool HasPositioningTechFilter { get; set; }
    bool HasLayerIDFilter { get; set; }
    bool HasElevationRangeFilter { get; set; }
    bool HasPassTypeFilter { get; set; }
    bool HasCompactionMachinesOnlyFilter { get; set; }
    bool HasTemperatureRangeFilter { get; set; }
    bool HasPassCountRangeFilter { get; set; }
    bool AnyFilterSelections { get; set; }
    bool AnyMachineEventFilterSelections { get; set; }
    bool AnyNonMachineEventFilterSelections { get; set; }

    void Prepare();
    void ClearFilter();
    void ClearVibeState();

    /// <summary>
    /// Compare one filter with another for the purpose of ordering them in caching lists
    /// </summary>
    /// <param name="AFilter"></param>
    /// <returns></returns>
    int CompareTo(ICellPassAttributeFilter AFilter);

    void ClearDesigns();
    void ClearElevationRange();
    void ClearElevationRangeFilterInitialisation();
    void ClearElevationType();
    void ClearGPSAccuracy();
    void ClearTemperatureRange();
    void ClearPassCountRange();
    void ClearGPSTolerance();
    void ClearGuidanceMode();
    void ClearLayerID();
    void ClearLayerState();
    void Assign(ICellPassAttributeFilter Source);
    void ClearCompactionMachineOnlyRestriction();
    void ClearMachineDirection();
    void ClearMachines();
    void ClearMinElevationMapping();
    void ClearPassType();
    void ClearPositioningTech();
    void ClearSurveyedSurfaceExclusionList();
    void ClearTime();
    bool FilterPass(ref CellPass PassValue);
    bool FilterPass(ref FilteredPassData PassValue);
    bool FilterPassUsingElevationRange(ref CellPass PassValue);
    bool FilterPassUsingTemperatureRange(ref CellPass PassValue);
    bool FilterPassUsingTimeOnly(ref CellPass PassValue);
    bool FilterPass_MachineEvents(ref FilteredPassData PassValue);
    bool FilterPass_NoMachineEvents(CellPass PassValue);
    bool FiltersElevation(float Elevation);
    bool FiltersElevation(double Elevation);

    /// <summary>
    /// FilterSinglePass selects a single pass from the list of passes in
    /// PassValues where PassValues contains the entire list of passes for
    /// a cell in the database.
    /// </summary>
    /// <returns></returns>
    bool FilterSinglePass(CellPass[] PassValues,
      int PassValueCount,
      ref FilteredSinglePassInfo FilteredPassInfo,
    //             ref FilteredMultiplePassInfo FilteredPassesBuffer)
      object /*IProfileCell*/ profileCell
    );

    void InitaliaseFilteringForCell(byte ASubgridCellX, byte ASubgridCellY);
    void InitialiseElevationRangeFilter(IClientHeightLeafSubGrid DesignElevations);

    /// <summary>
    /// Converts an array of Guids representing machine identifiers into a BitArray encoding a bit set of
    /// internal machine IDs relative to this sitemodel
    /// </summary>
    void InitialiseMachineIDsSet();

    bool IsTimeRangeFilter();

    bool FilterMultiplePasses(CellPass[] passValues,
      int PassValueCount,
      ref FilteredMultiplePassInfo filteredPassInfo);

    bool ExcludeSurveyedSurfaces();
    string ActiveFiltersText();

    bool FilterSinglePass(CellPass[] passValues,
      int passValueCount,
      bool wantEarliestPass,
      ref FilteredSinglePassInfo filteredPassInfo,
      object profileCell,
      bool performAttributeSubFilter);

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
    bool FilterSinglePass(FilteredPassData[] filteredPassValues,
      int passValueCount,
      bool wantEarliestPass,
      ref FilteredSinglePassInfo filteredPassInfo,
      object /* IProfileCell*/ profileCell,
      bool performAttributeSubFilter);
  }
}
