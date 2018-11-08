using System;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Filters.Interfaces
{
  public interface ICellPassAttributeFilterModel : IFromToBinary
  {
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
    bool FilterTemperatureByLastPass { get; set; }
    bool HasPassCountRangeFilter { get; set; }

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
    /// Elevation parameters have been initialized in preparation for elevation range filtering, either
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
    /// Denotes whether analysis of cell passes in a cell are analysed into separate layers according to 
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
    /// The list of surveyed surface identifiers to be excluded from the filtered result
    /// </summary>
    Guid[] SurveyedSurfaceExclusionList { get; set; } // note this is not saved in the database and must be set in the server

    /// <summary>
    /// Only permit cell passes for temperature values within min max range
    /// </summary>
    ushort MaterialTemperatureMin { get; set; }

    /// <summary>
    /// Only permit cell passes for temperature values within min max range
    /// </summary>
    ushort MaterialTemperatureMax { get; set; }

    /// <summary>
    /// takes final filtered passes and reduces to the set to passes within the min max pass count range
    /// </summary>
    ushort PasscountRangeMin { get; set; }

    /// <summary>
    ///  takes final filtered passes and reduces to the set to passes within the min max pass count range
    /// </summary>
    ushort PasscountRangeMax { get; set; }

    bool IsTimeRangeFilter();
  }
}
