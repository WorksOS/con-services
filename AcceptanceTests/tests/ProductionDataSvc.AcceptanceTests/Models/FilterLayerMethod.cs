namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The available filter-layer analysis methods
  /// This is copied from ...\RaptorServicesCommon\Models\FilterLayerMethod.cs 
  /// </summary>
  public enum FilterLayerMethod
  {
    /// <summary>
    /// The null layer method
    /// </summary>
    Invalid = -1,

    /// <summary>
    /// No layer filtering is applied
    /// </summary>
    None = 0,

    /// <summary>
    /// A combination of 'Auto' and 'MapReset'
    /// </summary>       
    AutoMapReset = 1,

    /// <summary>
    /// Layers are calculated using the observed pass-to-pass elevation changes with respect to the configured elevation dead bands.
    /// </summary>
    Automatic = 2,

    /// <summary>
    /// Layers are determined based on the map reset instructions recorded by the operatof of a machine.
    /// </summary>
    MapReset = 3,

    /// <summary>
    /// Layers are defined by an offset and thickness from a design
    /// </summary>
    OffsetFromDesign = 4,

    /// <summary>
    /// Layers are defined by an offset and thickness from a bench elevation
    /// </summary>
    OffsetFromBench = 5,

    /// <summary>
    /// Layers are defined by an offset and thickness from a road alignment profile
    /// </summary>
    OffsetFromProfile = 6,

    /// <summary>
    /// Layers are defined according to the LAYER_NUMBER recorded by the operatof of a machine.
    /// </summary>
    TagfileLayerNumber = 7
  }
}