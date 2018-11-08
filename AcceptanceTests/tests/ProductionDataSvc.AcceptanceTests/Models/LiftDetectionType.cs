namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The defined types of automatic lift detection that use information reported from the machine to group cell passes into layers.
  /// This is copied from ...\RaptorServicesCommon\Models\LiftDetectionType.cs 
  /// </summary>
  public enum LiftDetectionType
  {
    /// <summary>
    /// Use the elevations of cell passes with dead band limits to determine new layer transitions
    /// </summary>
    Automatic = 0,

    /// <summary>
    /// Use map reset commands made on the machine to determine new layer transitions
    /// </summary>
    MapReset = 1,

    /// <summary>
    /// Use a combination of automatic and map reset based methods to determine new layer transitions
    /// </summary>
    AutoMapReset = 2,

    /// <summary>
    /// Use layer numbers specified by the machine (in the TAG file) to determine new layer transitions
    /// </summary>
    Tagfile = 3,

    /// <summary>
    /// No automatic layer detection is done. This means superseded layers are included.
    /// </summary>
    None = 4
  }
}