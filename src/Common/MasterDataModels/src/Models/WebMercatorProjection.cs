using System;
using VSS.MasterData.Models.Converters;

namespace VSS.MasterData.Models.Models
{
  public class WebMercatorProjection
  {
    //see http://gis.stackexchange.com/questions/66247/what-is-the-formula-for-calculating-world-coordinates-for-a-given-latlng-in-goog/66357#66357

    public const int TILE_SIZE = 256;
    private const int ONE_HALF_CIRCLE = 180;
    private const int ONE_FULL_CIRCLE = 360;

    public static Point FromLatLngToPoint(Point latLng)
    {
      var pt = new Point
      {
        x = (latLng.Longitude + ONE_HALF_CIRCLE) / ONE_FULL_CIRCLE * TILE_SIZE
      };

      double latRad = latLng.Latitude.LatDegreesToRadians();
      pt.y = ((1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * Math.Pow(2, 0)) * TILE_SIZE;

      return pt;
    }

    public static Point FromPointToLatLng(Point point)
    {
      double lng = point.x / TILE_SIZE * ONE_FULL_CIRCLE - ONE_HALF_CIRCLE;
      double n = Math.PI - 2 * Math.PI * point.y / TILE_SIZE;
      double lat = (Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)))).LatRadiansToDegrees();

      return new Point(lat, lng);
    }

    public static Point LatLngToTile(Point latLng, long numTiles)
    {
      return PixelToTile(LatLngToPixel(latLng, numTiles));
    }

    public static Point PixelToTile(Point pixelPt)
    {
      return new Point
      {
        x = Math.Round(pixelPt.x / TILE_SIZE), //was Math.Floor but gave wrong result in some cases
        y = Math.Round(pixelPt.y / TILE_SIZE)
      };
    }

    public static Point TileToPixel(Point tilePt)
    {
      return new Point
      {
        x = tilePt.x * TILE_SIZE,
        y = tilePt.y * TILE_SIZE
      };
    }

    public static Point LatLngToPixel(Point latLng, long numTiles)
    {
      Point worldPt = FromLatLngToPoint(latLng);
      Point pixelPt = WorldToPixel(worldPt, numTiles);
      return pixelPt;
    }

    public static Point PixelToLatLng(Point pixelPt, long numTiles)
    {
      Point worldPt = PixelToWorld(pixelPt, numTiles);
      Point latLng = FromPointToLatLng(worldPt);
      return latLng;
    }

    public static Point WorldToPixel(Point worldPt, long numTiles)
    {
      return new Point
      {
        x = worldPt.x * numTiles,
        y = worldPt.y * numTiles
      };
    }

    public static Point PixelToWorld(Point pixelPt, long numTiles)
    {
      return new Point
      {
        x = pixelPt.x / numTiles,
        y = pixelPt.y / numTiles
      };
    }

    //see http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
    //Calculations below are equivalent to above but have the rounding problem also

    /// <summary>
    /// Calculates x tile coordinate from longitude
    /// </summary>
    /// <param name="longitude">longitude in radians</param>
    /// <param name="numTiles">number of tiles (calcuated from zoom level)</param>
    /// <returns>x tile coordinate</returns>
    public static int LongitudeToTile(double longitude, long numTiles)
    {
      var columnIndex = longitude;
      var columnNormalized = (1.0 + columnIndex / Math.PI) / 2.0;
      var column = columnNormalized * numTiles;
      var columnInt = Math.Round(column);
      return (int) columnInt;
    }

    /// <summary>
    /// Calculates the y tile coordinate from latitude
    /// </summary>
    /// <param name="latitude">latitude in radians</param>
    /// <param name="numTiles">number of tiles (calcuated from zoom level)</param>
    /// <returns>y tile coordinate</returns>
    public static int LatitudeToTile(double latitude, long numTiles)
    {
      var rowIndex = Math.Log(Math.Tan(latitude) + (1.0 / Math.Cos(latitude)));
      var rowNormalized = (1.0 - rowIndex / Math.PI) / 2.0;
      var row = rowNormalized * numTiles;
      var rowInt = Math.Round(row);
      return (int) rowInt;
    }
  }

  public static class LatLonExtensions
  {
    /// <summary>
    /// Latitude(y) must be in range -pi/2 to pi/2
    /// </summary>
    /// <returns>converted lat else throws exception</returns>
    public static double LatDegreesToRadians(this double lat)
    {
      lat = lat * Coordinates.DEGREES_TO_RADIANS;
      if (lat < -Math.PI / 2)
      {
        lat = lat + Math.PI;
      }
      else if (lat > Math.PI / 2)
      {
        lat = lat - Math.PI;
      }

      return lat;
    }

    /// <summary>
    /// Longitude(x) must be in the range -pi to pi
    /// </summary>
    /// <returns>converted lon else throws exception</returns>
    public static double LonDegreesToRadians(this double lon)
    {
      lon = lon * Coordinates.DEGREES_TO_RADIANS;
      if (lon < -Math.PI)
      {
        lon = lon + 2 * Math.PI;
      }
      else if (lon > Math.PI)
      {
        lon = lon - 2 * Math.PI;
      }
      return lon;
    }

    /// <summary>
    /// Convert latitude in radians to degrees
    /// </summary>
    public static double LatRadiansToDegrees(this double lat)
    {
      return lat.RadiansToDegrees();
    }

    /// <summary>
    /// Convert longitude in radians to degrees
    /// </summary>
    public static double LonRadiansToDegrees(this double lon)
    {
      return lon.RadiansToDegrees();
    }

    /// <summary>
    /// Convert a value in radians to degrees
    /// </summary>
    private static double RadiansToDegrees(this double rad)
    {
      return rad * Coordinates.RADIANS_TO_DEGREES;
    }
  }
}
