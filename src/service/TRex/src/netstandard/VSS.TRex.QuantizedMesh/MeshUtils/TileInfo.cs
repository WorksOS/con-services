using System;
using VSS.TRex.QuantizedMesh.Models;

namespace VSS.TRex.QuantizedMesh.MeshUtils
{
  public class TileInfo
  {
    public BBSphere BoundingSphere;

    public TileInfo()
    {
      BoundingSphere = new BBSphere();
    }

    public TerrainTileHeader CalculateHeaderInfo(ref Vector3[] ecefPoints)
    {
      BoundingSphere.FromPoints(ecefPoints);
      var hdr = new TerrainTileHeader();
      hdr.CenterX = BoundingSphere.Center.X;
      hdr.CenterY = BoundingSphere.Center.Y;
      hdr.CenterZ = BoundingSphere.Center.Z;
      hdr.BoundingSphereRadius = BoundingSphere.Radius;
      return hdr;
    }
  }
}
