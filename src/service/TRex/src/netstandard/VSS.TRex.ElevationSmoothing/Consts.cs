namespace VSS.TRex.DataSmoothing
{
  public static class Consts
  {
    /// <summary>
    /// Default state for application of null infill
    /// </summary>
    public const bool SURFACE_EXPORT_DATA_SMOOTHING_ACTIVE = true;

    /// <summary>
    /// Default state for application of null infill
    /// </summary>
    public const NullInfillMode SURFACE_EXPORT_DATA_SMOOTHING_NULL_INFILL_MODE = NullInfillMode.NoInfill;

    /// <summary>
    /// Size of mask to use for surface export
    /// </summary>
    public const ConvolutionMaskSize SURFACE_EXPORT_DATA_SMOOTHING_MASK_SIZE = ConvolutionMaskSize.Mask3X3;

    /// <summary>
    /// Default state for application of null infill
    /// </summary>
    public const bool TILE_RENDERING_DATA_SMOOTHING_ACTIVE = true;

    /// <summary>
    /// Default state for application of null infill
    /// </summary>
    public const NullInfillMode TILE_RENDERING_DATA_SMOOTHING_NULL_INFILL_MODE = NullInfillMode.NoInfill;

    /// <summary>
    /// Size of mask to use for surface export
    /// </summary>
    public const ConvolutionMaskSize TILE_RENDERING_DATA_SMOOTHING_MASK_SIZE = ConvolutionMaskSize.Mask3X3;

  }
}
