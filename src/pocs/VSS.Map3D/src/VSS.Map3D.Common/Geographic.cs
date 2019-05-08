using System;
using VSS.Map3D.Models;

/// <summary>
/// Cesium terrain tile requests use Geographic tiling scheme. Use these functions for terrain tiles
/// </summary>
namespace VSS.Map3D.Common
{
  public class Geographic
  {

    public static LLBoundingBox world = new LLBoundingBox(-3.141592653589793, -1.5707963267948966, 3.141592653589793, 1.5707963267948966,true); // world coords in radians 

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
      int ytile = (int)Math.Floor((1 - Math.Log(Math.Tan(MapUtil.Deg2Rad(lat)) + 1 / Math.Cos(MapUtil.Deg2Rad(lat))) / Math.PI) / 2 * (1 << zoom));
      if (xtile < 0)
        xtile = 0;
      if (xtile >= (1 << zoom))
        xtile = ((1 << zoom) - 1);
      if (ytile < 0)
        ytile = 0;
      if (ytile >= (1 << zoom))
        ytile = ((1 << zoom) - 1);
      return ("ZXY " + zoom +" / " +  xtile + " / " + ytile);
    }

    public static int GetNumberOfXTilesAtLevel(int level)
    {
      return 2 << level; // 2 at level 0
    }

    public static int GetNumberOfYTilesAtLevel(int level)
    {
      return 1 << level; // 1 at level 0
    }

    /*
    public static double tile2lon(int x, int zoom)
    {
      return x / Math.Pow(2, zoom) * 360.0 - 180;
    }

    public static double tile2lat(int y, int zoom)
    {
      double n = Math.PI - (2.0 * Math.PI * y) / Math.Pow(2.0, zoom);
      return (180.0 / Math.PI) * (Math.Atan(Math.Sinh(n)));
    }
    
    public static BoundingRect GetLatLonBoundsForCesiumTile(int zoom, int x, int y)
    {
      BoundingRect bb = new BoundingRect();
      bb.North = tile2lat(y, zoom);
      bb.South = tile2lat(y + 1, zoom);
      bb.West = tile2lon(x, zoom + 1);
      bb.East = tile2lon(x + 1, zoom + 1);
      return bb;
    }
    */


    public static LLBoundingBox TileXYToRectangleRad(int x, int y, int level)
    {

      var xTiles = GetNumberOfXTilesAtLevel(level);
      var yTiles = GetNumberOfYTilesAtLevel(level);

      var xTileWidth = (world.East - world.West) / xTiles;
      var west = x * xTileWidth + world.West;
      var east = (x + 1) * xTileWidth + world.West;

      var yTileHeight = (world.North - world.South) / yTiles;
      var north = world.North - y * yTileHeight;
      var south = world.North - (y + 1) * yTileHeight;
      return new LLBoundingBox(west, south, east, north,true);
    }

    public static LLBoundingBox TileXYToRectangleLL(int x, int y, int level)
    {

      var xTiles = GetNumberOfXTilesAtLevel(level);
      var yTiles = GetNumberOfYTilesAtLevel(level);

      var xTileWidth = (world.East - world.West) / xTiles;
      var west = x * xTileWidth + world.West;
      var east = (x + 1) * xTileWidth + world.West;

      var yTileHeight = (world.North - world.South) / yTiles;
      var north = world.North - y * yTileHeight;
      var south = world.North - (y + 1) * yTileHeight;
      return new LLBoundingBox(MapUtil.Rad2Deg(west), MapUtil.Rad2Deg(south), MapUtil.Rad2Deg(east), MapUtil.Rad2Deg(north),false);
    }


    // special Cesium Code

    public static double ComputeWidth(BoundingRect2 rectangle)
    {
      var east = rectangle.East;
      var west = rectangle.West;
      if (east < west)
      {
        east += Math.PI * 2;
      }
      return east - west;
    }

    public static double ComputeHeight(BoundingRect2 rectangle)
    {
      return rectangle.North - rectangle.South;
    }

    // same as above
    public static BoundingRect2 GeographicTilingSchemeTileXYToRectangle(int x, int y, int level)
    {
      double southE = -1.4844222297453324;
      double northE = 1.4844222297453322;
      double eastE = 3.141592653589793;
      double westE = -3.141592653589793;

  //    y = MapUtil.FlipY(y, level);
      var rectangle = new BoundingRect2(){East = eastE,North = northE, South = southE, West = westE};

      var xTiles = GetNumberOfXTilesAtLevel(level);
      var yTiles = GetNumberOfYTilesAtLevel(level);

      var xTileWidth = ComputeWidth(rectangle) / xTiles;
      var west = x * xTileWidth + rectangle.West;
      var east = (x + 1) * xTileWidth + rectangle.West;

      var yTileHeight = ComputeHeight(rectangle) / yTiles;
      var north = rectangle.North - y * yTileHeight;
      var south = rectangle.North - (y + 1) * yTileHeight;
      // Y is south to north in this case
 //     var south = rectangle.South + y * yTileHeight;
  //    var north = rectangle.South + (y + 1) * yTileHeight;

      return new BoundingRect2(west, south, east, north);
    }

  }
}
