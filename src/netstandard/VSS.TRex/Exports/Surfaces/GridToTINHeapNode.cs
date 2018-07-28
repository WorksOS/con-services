namespace VSS.TRex.Tests.Exports.Surfaces
{
  public class GridToTINHeapNode
  {
    public static int NOT_IN_HEAP = -12345;

    /// <summary>
    /// Import is a measure of how important this item is. In the context
    /// of the grid decimator, this is a measure of the largest height difference
    /// between the plan of the triangle and a grid position within it.
    /// </summary>
    public double Import { get; set; }

    public GridToTINTriangle Tri { get; set; }

    public int sx, sy;
    public double sz;

    public GridToTINHeapNode()
    {
      Import = 0;
      Tri = null;

      sx = int.MaxValue;
      sy = int.MaxValue;
      sz = 0;
    }

    public GridToTINHeapNode(GridToTINTriangle tri, double import) : this()
    {
      Tri = tri;
      Import = import;
    }

    public GridToTINHeapNode(GridToTINHeapNode node) : this()
    {
      Tri = node.Tri;
      Import = node.Import;
    }
  }
}
