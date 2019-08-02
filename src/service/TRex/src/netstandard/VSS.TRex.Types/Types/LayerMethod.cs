namespace VSS.TRex.Types
{
  /// <summary>
  /// The layer detection mode to be used for layers analysis of cell passes within a cell.
  /// Note: This is the FilterLayerMethod from Raptor, not the LayerMethod which was not used and had a different enum order.
  /// Invalid was -1 but now 8.
  /// </summary>
  public enum LayerMethod : byte
  {
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
    /// Layers are determined based on the map reset instructions recorded by the operator of a machine.
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
    /// Layers are defined by an offset and thickness from a road alignment profile. Note: This layer method is currently not supported.
    /// </summary>
    OffsetFromProfile = 6,

    /// <summary>
    /// Layers are defined according to the LAYER_NUMBER recorded by the operator of a machine.
    /// </summary>
    TagfileLayerNumber = 7,

    /// <summary>
    /// The null layer method
    /// </summary>
    Invalid = 8
  }
}
