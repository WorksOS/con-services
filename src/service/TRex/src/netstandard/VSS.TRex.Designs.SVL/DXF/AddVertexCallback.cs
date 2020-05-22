using System.Collections.Generic;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL.Utilities;

namespace VSS.TRex.Designs.SVL.DXF
{
  public class AddVertexCallback
  {
    public List<AlignmentGeometryVertex> Vertices { get; } = new List<AlignmentGeometryVertex>();
    public int VertexCount { get; set; }

    private void Add(AlignmentGeometryVertex vertex)
    {
      if (VertexCount == 0 || !Vertices[VertexCount - 1].Equals(vertex))
      {
        if (VertexCount > Vertices.Count)
        {
          Vertices.Add(vertex);
          VertexCount++;
        }
        else
        {
          Vertices[VertexCount - 1] = vertex;
          VertexCount++;
        }
      }
    }

    public void AddVertex(double vx, double vy, DecompositionVertexLocation vertexLocation)
    {
      Add(new AlignmentGeometryVertex(vx, vy, Consts.NullDouble, Consts.NullDouble));
    }

    public void AddVertex(double vx, double vy, double vz, double vstn, DecompositionVertexLocation vertexLocation)
    {
      Add(new AlignmentGeometryVertex(vx, vy, vz, vstn));
    }
  }
}
