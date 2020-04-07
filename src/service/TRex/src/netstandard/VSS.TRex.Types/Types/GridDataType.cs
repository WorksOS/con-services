namespace VSS.TRex.Types
{
  /// <summary>
  /// Types of grid data that client leaf sub grids may represent. These largely map to the 
  /// attribute and processing vectors supported by the TRex processing engine with the exception 
  /// of All which represents fully attributed call passes. This type is only used on server side
  /// full stack cell pass sub grids.
  /// </summary>
  public enum GridDataType
  {
    All = 0x00000000, // Could possibly remove this from the enumeration
    CCV = 0x00000001,
    Height = 0x00000002,
    Latency = 0x00000003,
    PassCount = 0x00000004,
    Frequency = 0x00000005,
    Amplitude = 0x00000006,
    Moisture = 0x00000007,
    Temperature = 0x00000008,
    RMV = 0x00000009,
    GPSMode = 0x0000000A,
    CCVPercent = 0x0000000B,
    SimpleVolumeOverlay = 0x0000000C,
    HeightAndTime = 0x0000000D,
    CompositeHeights = 0x0000000E,
    MDP = 0x0000000F,
    MDPPercent = 0x00000010,
    CellProfile = 0x00000011,
    CellPasses = 0x00000012,
    MachineSpeed = 0x00000013,
    CCVPercentChange = 0x00000014,
    MachineSpeedTarget = 0x00000015,
    CCVPercentChangeIgnoredTopNullValue = 0x00000016,
    CCA = 0x00000017,
    CCAPercent = 0x00000018,
    TemperatureDetail = 0x00000019,
    CutFill = 0x0000001A,
    DesignHeight = 0x0000001B,

    /// <summary>
    /// SurveyedSurfaceHeightAndTime is distinguished from HeightAndTime in that only surveyed surfaces are
    /// used to construct this data. Differentiating the grid types allows coherent caching in a single spatial
    /// general sub grid result cache along with HeightAndTime results that are derived from production data
    /// and SurveyedSurfaceHeightAndTime results
    /// </summary>
    SurveyedSurfaceHeightAndTime = 0x0000001C,

    ProgressiveVolumes = 0x0000001D
  }
}
