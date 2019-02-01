namespace VSS.TRex.Common.Types
{
  /// <summary>
  /// Notes the mapping mode state of the Earthwork machine control system.
  /// Currently the only implemented states on Earthworks are LatestElevation and
  /// MinimumElevation.
  /// </summary>
  public enum ElevationMappingMode : byte
  {
    /// <summary>
    /// The recorded elevation is used to create a new cell pass at that point in time on the Earthworks system
    /// </summary>
    LatestElevation = 0,

    /// <summary>
    /// The recorded elevation is used to create a new cell pass at that point in time on the Earthworks system only
    /// if that elevation is lower than the currently tracked elevation for that cell on Earthworks
    /// </summary>
    MinimumElevation = 1,
  }
}
