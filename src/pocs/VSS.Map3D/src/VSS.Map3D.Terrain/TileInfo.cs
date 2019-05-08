using System;
using VSS.Map3D.Common;
using VSS.Map3D.Models.QMTile;

namespace VSS.Map3D.Terrain
{
  public class TileInfo
  {
    public BBSphere BoundingSphere;

    public TileInfo()
    {
      BoundingSphere = new BBSphere();
    }

    public TerrainTileHeader CalculateHeaderInfo(ref Vector3[] ecefPoints, Boolean lightingRequired)
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
