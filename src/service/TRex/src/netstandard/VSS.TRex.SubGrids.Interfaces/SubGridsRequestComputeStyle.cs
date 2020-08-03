namespace VSS.TRex.SubGrids.Interfaces
{
  public enum SubGridsRequestComputeStyle
  {
    /// <summary>
    /// Sub grids are requested 'as is' from the origin requests, with only intermediate transforms used
    /// </summary>
    Normal,

    /// <summary>
    /// The sub grids are being requested to satisfy 'simple volumes' requests where two origin filters have been 
    /// augmented with a thiird 'interemediary' filter to completely capture the origin surface.
    /// </summary>
    SimpleVolumeThreeWayCoalescing
  }
}
