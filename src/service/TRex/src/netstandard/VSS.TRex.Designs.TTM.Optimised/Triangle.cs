namespace VSS.TRex.Designs.TTM.Optimised
{
  /// <summary>
  /// Describes a triangle in the TIN mesh
  /// </summary>
  public struct Triangle
  {
    /// <summary>
    /// Indices of the three vertices the make up this triangle. These indices are relative to the Triangle array maintained
    /// in the TrimbleTINModel instance
    /// </summary>
    public int Vertex0, Vertex1, Vertex2;

    public Triangle(int vertex0, int vertex1, int vertex2)
    {
      Vertex0 = vertex0;
      Vertex1 = vertex1;
      Vertex2 = vertex2;
    }
  }
}
