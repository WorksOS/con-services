using System;

namespace VSS.TRex.QuantizedMesh.MeshUtils
{
  public static class MapGeo
  {
    public static LLBoundingBox world = new LLBoundingBox(-3.141592653589793, -1.5707963267948966,
                                                           3.141592653589793, 1.5707963267948966, true); // world boundary in radians 

    /// <summary>
    /// Get tile number from decimal degrees passed in 
    /// </summary>
    /// <param name="lat"></param>
    /// <param name="lon"></param>
    /// <param name="zoom"></param>
    /// <returns></returns>
    public static String GetTileNumber(double lat, double lon, int zoom)
    {
      int xtile = (int)Math.Floor((lon + 180) / 360 * (1 << zoom));
      int ytile = (int)Math.Floor((1 - Math.Log(Math.Tan(MapUtils.Deg2Rad(lat)) + 1 / Math.Cos(MapUtils.Deg2Rad(lat))) / Math.PI) / 2 * (1 << zoom));
      if (xtile < 0)
        xtile = 0;
      if (xtile >= (1 << zoom))
        xtile = ((1 << zoom) - 1);
      if (ytile < 0)
        ytile = 0;
      if (ytile >= (1 << zoom))
        ytile = ((1 << zoom) - 1);
      return ("ZXY " + zoom + " / " + xtile + " / " + ytile);
    }

    public static int GetNumberOfXTilesAtLevel(int level)
    {
      return 2 << level; // 2 tiles at level 0
    }

    public static int GetNumberOfYTilesAtLevel(int level)
    {
      return 1 << level; // 1 tile at level 0
    }

    /// <summary>
    /// Return lat long bounding rect in degrees
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static LLBoundingBox TileXYZToRectLL(int x, int y, int level)
    {

      y = (int)Math.Pow(2, level) - y - 1; // Very important to flip the Y coord so it goes from bottom to top for our TMS server
      var xTiles = GetNumberOfXTilesAtLevel(level);
      var yTiles = GetNumberOfYTilesAtLevel(level);

      var xTileWidth = (world.East - world.West) / xTiles;
      var west = x * xTileWidth + world.West;
      var east = (x + 1) * xTileWidth + world.West;

      var yTileHeight = (world.North - world.South) / yTiles;
      var north = world.North - y * yTileHeight;
      var south = world.North - (y + 1) * yTileHeight;
      //return new LLBoundingBox(west, south, east, north);
      return new LLBoundingBox(MapUtils.Rad2Deg(west), MapUtils.Rad2Deg(south), MapUtils.Rad2Deg(east), MapUtils.Rad2Deg(north), false);
    }
  }
}
