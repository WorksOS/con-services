using System.Collections.Generic;
using VSS.TRex.Designs.SVL.Utilities;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL.DXF
{
  public class AddVertexCallback
  {
    public List<XYZ> Vertices { get; } = new List<XYZ>();
    public int VertexCount { get; set; }

    public void AddVertex(double vx, double vy, DecompositionVertexLocation vertexLocation)
    {
      VertexCount++;
      if (VertexCount > Vertices.Count)
        Vertices.Add(new XYZ(vx, vy));
      else
        Vertices[VertexCount - 1] = new XYZ(vx, vy);
    }
  }
}
