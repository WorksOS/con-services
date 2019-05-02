using System;
using System.Threading.Tasks;
using VSS.Map3D.Models;
using VSS.Map3D.Models.QMTile;

namespace VSS.Map3D.Mesh
{
  public interface IMesh
  {
    VertexData MakeQuantizedMesh(ref ElevationData evlData);
    VertexData MakeFakeMesh(ref ElevationData evlData);
  }
}
