using System;
using System.Threading.Tasks;
using VSS.Map3D.Common;
using VSS.Map3D.Models;
using VSS.Map3D.Models.QMTile;

namespace VSS.Map3D.Tiler
{
  public interface ITiler
  {
    byte[] MakeTile(VertexData vertices, TerrainTileHeader headerRec, int trianglesCount, int tileSize);

    Task<byte[]> FetchTile(string tileDir, int x, int y, int z);

    Task<byte[]> GetXYZTile(TileOptions options, int x, int y, int z);
  }
}
