using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using VSS.Map3D.Models;

namespace VSS.Map3D.Common
{
  public class CesiumMerc
  {
    // Crap dont use

    public static PointD _rectangleSouthwestInMeters = new PointD(-20037508.342789244, -20037508.342789244);
    public static PointD _rectangleNortheastInMeters = new PointD(20037508.342789244, 20037508.342789244);
    public static double WebMercatorProjection_MaximumLatitude = 1.4844222297453322;

    public static int GetNumberOfXTilesAtLevel(int level)
    {
      return 2 << level;
    }

    public static int GetNumberOfYTilesAtLevel(int level)
    {
      return 1 << level; // ???
    }

    public static LLBoundingBox TileXYToNativeRectangle(int x, int y, int level)
    {
      var xTiles = GetNumberOfXTilesAtLevel(level);
      var yTiles = GetNumberOfYTilesAtLevel(level);
      
      var xTileWidth = (_rectangleNortheastInMeters.X - _rectangleSouthwestInMeters.Y) / xTiles;

      var west = _rectangleSouthwestInMeters.X + x * xTileWidth;
      var east = _rectangleSouthwestInMeters.X + (x + 1) * xTileWidth;

      var yTileHeight = (_rectangleNortheastInMeters.Y - _rectangleSouthwestInMeters.Y) / yTiles;
      var north = _rectangleNortheastInMeters.Y - y * yTileHeight;
      var south = _rectangleNortheastInMeters.Y - (y + 1) * yTileHeight;

      return new LLBoundingBox(west, south, east, north,true);
    }

    public static LLBoundingBox TileXYToRectangle(int x, int y, int level)
    {
        var nativeRectangle = TileXYToNativeRectangle(x, y, level);

     //   var projection = _projection;
        var southwest = UnProject(new Vector3(nativeRectangle.West, nativeRectangle.South,0));
        var northeast = UnProject(new Vector3(nativeRectangle.East, nativeRectangle.North,0));

        nativeRectangle.West = southwest.X;
        nativeRectangle.South = southwest.Y;
        nativeRectangle.East = northeast.X;
        nativeRectangle.North = northeast.Y;
        return nativeRectangle;
      
      
    }


  /**
   * Converts Web Mercator X, Y coordinates, expressed in meters, to a {@link Cartographic}
   * containing geodetic ellipsoid coordinates.  The Z coordinate is copied unmodified to the
   * height.
   *
   * @param {Cartesian3} cartesian The web mercator Cartesian position to unrproject with height (z) in meters.
   * @param {Cartographic} [result] The instance to which to copy the result, or undefined if a
   *        new instance should be created.
   * @returns {Cartographic} The equivalent cartographic coordinates.
   */

  public static Vector3 UnProject(Vector3 pt)
  {

    double oneOverEarthSemimajorAxis = 1.567855942887398e-7;
    double longitude =  pt.X * oneOverEarthSemimajorAxis;

    var latitude = MercatorAngleToGeodeticLatitude(pt.Y * oneOverEarthSemimajorAxis);
    var height = pt.Z;
    return new Vector3(longitude, latitude, height);

  }

  private static double  MercatorAngleToGeodeticLatitude(double mercatorAngle)
  {
    double CesiumMath_PI_OVER_TWO = 1.5707963267948966;

    return CesiumMath_PI_OVER_TWO - (2.0 * Math.Atan(Math.Exp(-mercatorAngle)));
  }


  /**
   * Calculates the tile x, y coordinates of the tile containing
   * a given cartographic position.
   *
   * @param {Cartographic} position The position.
   * @param {Number} level The tile level-of-detail.  Zero is the least detailed.
   * @param {Cartesian2} [result] The instance to which to copy the result, or undefined if a new instance
   *        should be created.
   * @returns {Cartesian2} The specified 'result', or a new object containing the tile x, y coordinates
   *          if 'result' is undefined.
   */
  public static PointD PositionToTileXY(PointD pt, int level)
    {
      // lat lon in radians

  //    var rectangle = new LLBoundingBox();
      /*
      if (!Rectangle.contains(rectangle, position))
      {
        // outside the bounds of the tiling scheme
        return undefined;
      }
      */

      var xTiles = GetNumberOfXTilesAtLevel(level);
      var yTiles = GetNumberOfYTilesAtLevel(level);

      var overallWidth = _rectangleNortheastInMeters.X - _rectangleSouthwestInMeters.X;
      var xTileWidth = overallWidth / xTiles;
      var overallHeight = _rectangleNortheastInMeters.Y - _rectangleSouthwestInMeters.Y;
      var yTileHeight = overallHeight / yTiles;

   //   var projection = _projection;

      var webMercatorPosition = ConvertToWebMerc(pt);

      var distanceFromWest = webMercatorPosition.X - _rectangleSouthwestInMeters.X;
      var distanceFromNorth = _rectangleNortheastInMeters.Y - webMercatorPosition.Y;

      var xTileCoordinate = distanceFromWest / xTileWidth; // else 0

      if (xTileCoordinate >= xTiles)
      {
        xTileCoordinate = xTiles - 1;
      }
      var yTileCoordinate = distanceFromNorth / yTileHeight; // | 0;
      if (yTileCoordinate >= yTiles)
      {
        yTileCoordinate = yTiles - 1;
      }

      return new PointD(xTileCoordinate, yTileCoordinate);
   

    }






    /**
     * Converts geodetic ellipsoid coordinates, in radians, to the equivalent Web Mercator
     * X, Y, Z coordinates expressed in meters and returned in a {@link Cartesian3}.  The height
     * is copied unmodified to the Z coordinate.
     *
     * @param {Cartographic} cartographic The cartographic coordinates in radians.
     * @param {Cartesian3} [result] The instance to which to copy the result, or undefined if a
     *        new instance should be created.
     * @returns {Cartesian3} The equivalent web mercator X, Y, Z coordinates, in meters.
     */
    public static Vector3 ConvertToWebMerc(PointD pt)
    { 
   // WebMercatorProjection.prototype.project = function(cartographic, result)
    //{
      double semimajorAxis = 6378137;
      var x = pt.X * semimajorAxis;
      var y = GeodeticLatitudeToMercatorAngle(pt.X) * semimajorAxis;
      var z = 0;
      return new Vector3(x, y, z);
    }



    /**
     * Converts a geodetic latitude in radians, in the range -PI/2 to PI/2, to a Mercator
     * angle in the range -PI to PI.
     *
     * @param {Number} latitude The geodetic latitude in radians.
     * @returns {Number} The Mercator angle.
     */
    public static double GeodeticLatitudeToMercatorAngle(double latitude)
    { 
      // Clamp the latitude coordinate to the valid Mercator bounds.
      if (latitude > WebMercatorProjection_MaximumLatitude)
      {
        latitude = WebMercatorProjection_MaximumLatitude;
      }
      else if (latitude < -WebMercatorProjection_MaximumLatitude)
      {
        latitude = -WebMercatorProjection_MaximumLatitude;
      }
      var sinLatitude = Math.Sin(latitude);
      return 0.5 * Math.Log((1.0 + sinLatitude) / (1.0 - sinLatitude));
    }



    /**
     * Returns true if the cartographic is on or inside the rectangle, false otherwise.
     *
     * @param {Rectangle} rectangle The rectangle
     * @param {Cartographic} cartographic The cartographic to test.
     * @returns {Boolean} true if the provided cartographic is inside the rectangle, false otherwise.
     */
    /*
   Rectangle.contains = function(rectangle, cartographic)
   {
     Check.typeOf.object('rectangle', rectangle);
     Check.typeOf.object('cartographic', cartographic);

     var longitude = cartographic.longitude;
     var latitude = cartographic.latitude;

     var west = rectangle.west;
     var east = rectangle.east;

     if (east < west)
     {
       east += CesiumMath.TWO_PI;
       if (longitude < 0.0)
       {
         longitude += CesiumMath.TWO_PI;
       }
     }
     return (longitude > west || CesiumMath.equalsEpsilon(longitude, west, CesiumMath.EPSILON14)) &&
            (longitude < east || CesiumMath.equalsEpsilon(longitude, east, CesiumMath.EPSILON14)) &&
            latitude >= rectangle.south &&
            latitude <= rectangle.north;
   };     
    */



  }
}
