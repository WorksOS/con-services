using System;
using VSS.MasterData.Models.Models;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Types;
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
    bool HasElevationMappingModeFilter { get; set; }
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
    int DesignNameID { get; set; } 
    VibrationState VibeState { get; set; }
    MachineDirection MachineDirection { get; set; }
    PassTypeSet PassTypeSet { get; set; }
    ElevationMappingMode ElevationMappingMode { get; set; }
    PositioningTech PositioningTech { get; set; }
    ushort GPSTolerance { get; set; }
    bool GPSAccuracyIsInclusive { get; set; }
    GPSAccuracy GPSAccuracy { get; set; }

    /// <summary>
    /// The filter will select cell passes with a measure GPS tolerance value greater than the limit specified
    /// in GPSTolerance
    /// </summary>
    bool GPSToleranceIsGreaterThan { get; set; }

    Common.Types.ElevationType ElevationType { get; set; }

    /// <summary>
    /// The machine automatics guidance mode to be in used to record cell passes that will meet the filter.
    /// </summary>
    AutomaticsType GCSGuidanceMode { get; set; }

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
    Guid ElevationRangeDesignUID { get; set; }

    /// <summary>
    /// The offset from the benchmark design for a reference surface
    /// </summary>
    double ElevationRangeDesignOffset { get; set; }

    /// <summary>
    /// Denotes whether analysis of cell passes in a cell are analyzed into separate layers according to 
    /// LayerMethod or if extracted cell passes are wrapped into a single containing layer.
    /// </summary>
    LayerState LayerState { get; set; }

    /// <summary>
    /// ID of layer we are only interested in
    /// </summary>
    int LayerID { get; set; }

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
    ushort PassCountRangeMin { get; set; }

    /// <summary>
    ///  takes final filtered passes and reduces to the set to passes within the min max pass count range
    /// </summary>
    ushort PassCountRangeMax { get; set; }

    bool IsTimeRangeFilter();
  }
}
