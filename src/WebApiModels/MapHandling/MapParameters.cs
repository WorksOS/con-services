using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// All the data required to generate overlayed map tiles
  /// </summary>
  public class MapParameters
  {
    public MapBoundingBox bbox;
    public int zoomLevel;
    public long numTiles;
    public Point pixelTopLeft;
    public int mapWidth;
    public int mapHeight;
    public bool addMargin;
  }
}
