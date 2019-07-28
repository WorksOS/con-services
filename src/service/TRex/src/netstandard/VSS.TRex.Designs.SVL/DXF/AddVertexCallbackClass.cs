using System.Collections.Generic;
using VSS.TRex.Designs.SVL.Utilities;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL.DXF
{
  public class AddVertexCallbackClass
  {
    public AddVertexCallbackClass()
    {
      Vertices = new List<XYZ>();
    }

    public List<XYZ> Vertices { get; private set; }
    public int VertexCount { get; set; }

    public void AddVertex(double vx, double vy, DecompositionVertexLocation VertexLocation)
    {
      VertexCount++;
      if (VertexCount > Vertices.Count)
        Vertices.Add(new XYZ(vx, vy));
      else
        Vertices[VertexCount - 1] = new XYZ(vx, vy);
    }
  }
}