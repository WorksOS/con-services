using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.TTM
{
  public class DesignTriangleEdge
  {
    public bool Stamped { get; set; }

    public TriVertex Vertex1 { get; set; }

    public TriVertex Vertex2 { get; set; }

    public DesignTriangleEdge(TriVertex vertex1, TriVertex vertex2)
    {
      Stamped = false;
      Vertex1 = vertex1;
      Vertex2 = vertex2;
    }
  }
}
