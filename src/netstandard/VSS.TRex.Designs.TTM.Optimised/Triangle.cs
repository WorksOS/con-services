namespace VSS.TRex.Designs.TTM.Optimised
{
  /// <summary>
  /// Describes a triangle in the TIN mesh
  /// </summary>
  public struct Triangle
  {
    /// <summary>
    /// Indices of the three vertices the make up this triangle. These indices are relative to the Triangle array maintainted
    /// in the TrimbleTINModel instance
    /// </summary>
    public int Vertex0, Vertex1, Vertex2;
  }
}
