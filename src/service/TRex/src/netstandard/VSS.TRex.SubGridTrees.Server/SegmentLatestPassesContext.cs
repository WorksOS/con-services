namespace VSS.TRex.SubGridTrees.Server
{
  public enum SegmentLatestPassesContext
  {
    /// <summary>
    /// The latest passes are to be used for the global latest cell passes for a sub grid
    /// </summary>
    Global,

    /// <summary>
    /// The latest passes are to be used for the latest cell passes for a segment 
    /// </summary>
    Segment
  }
}
