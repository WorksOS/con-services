using VSS.TRex.Designs.TTM;

namespace VSS.TRex.Tests.Exports.Surfaces
{
  /// <summary>
  /// Decorates the standard triangle class with a HeapIndex tracking member
  /// </summary>
  public class GridToTINTriangle : TTMTriangle
  {
    public int HeapIndex { get; set; } = GridToTINHeapNode.NOT_IN_HEAP;

    public GridToTINTriangle(TriVertex Vertex1, TriVertex Vertex2, TriVertex Vertex3) : base(Vertex1, Vertex2, Vertex3)
    {
    }
  }
}
