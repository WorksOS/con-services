using VSS.MasterData.Models.Models;


namespace VSS.Tile.Service.Common.Models
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
