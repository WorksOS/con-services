using System;

namespace WebApiModels.Notification.Helpers
{
  public class WebMercatorProjection
  {
    //see http://gis.stackexchange.com/questions/66247/what-is-the-formula-for-calculating-world-coordinates-for-a-given-latlng-in-goog/66357#66357

    public const int TILE_SIZE = 256;

    public static double DegreesToRadians(double deg)
    {
      return deg * (Math.PI / 180);
    }

    public static double RadiansToDegrees(double rad)
    {
      return rad / (Math.PI / 180);
    }

    public static Point FromLatLngToPoint(Point latLng)
    {
      Point pt = new Point();
      pt.x = (latLng.Longitude + 180) / 360 * TILE_SIZE;
      double latRad = DegreesToRadians(latLng.Latitude);
      pt.y = ((1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * Math.Pow(2, 0)) * TILE_SIZE;
      return pt;
    }

    public static Point FromPointToLatLng(Point point)
    {
      double lng = point.x / TILE_SIZE * 360 - 180;
      double n = Math.PI - 2 * Math.PI * point.y / TILE_SIZE;
      double lat = RadiansToDegrees(Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n))));
      return new Point(lat, lng);
    }

    public static Point LatLngToTile(Point latLng, int numTiles)
    {
      return PixelToTile(LatLngToPixel(latLng, numTiles));
    }

    public static Point PixelToTile(Point pixelPt)
    {
      Point tilePt = new Point();
      tilePt.x = Math.Round(pixelPt.x / TILE_SIZE);//was Math.Floor
      tilePt.y = Math.Round(pixelPt.y / TILE_SIZE);//but gave wrong result in some cases
      return tilePt;
    }

    public static Point TileToPixel(Point tilePt)
    {
      Point pixelPt = new Point();
      pixelPt.x = tilePt.x * TILE_SIZE;
      pixelPt.y = tilePt.y * TILE_SIZE;
      return pixelPt;
    }

    public static Point LatLngToPixel(Point latLng, int numTiles)
    {
      Point worldPt = FromLatLngToPoint(latLng);
      Point pixelPt = WorldToPixel(worldPt, numTiles);
      return pixelPt;
    }

    public static Point PixelToLatLng(Point pixelPt, int numTiles)
    {
      Point worldPt = PixelToWorld(pixelPt, numTiles);
      Point latLng = FromPointToLatLng(worldPt);
      return latLng;
    }

    public static Point WorldToPixel(Point worldPt, int numTiles)
    {
      Point pixelPt = new Point();
      pixelPt.x = worldPt.x * numTiles;
      pixelPt.y = worldPt.y * numTiles;
      return pixelPt;
    }

    public static Point PixelToWorld(Point pixelPt, int numTiles)
    {
      Point worldPt = new Point();
      worldPt.x = pixelPt.x / numTiles;
      worldPt.y = pixelPt.y / numTiles;
      return worldPt;
    }

    //see http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
    //Calculations below are equivalent to above but have the rounding problem also

    /// <summary>
    /// Calculates x tile coordinate from longitude
    /// </summary>
    /// <param name="longitude">longitude in radians</param>
    /// <param name="numTiles">number of tiles (calcuated from zoom level)</param>
    /// <returns>x tile coordinate</returns>
    public static int LongitudeToTile(double longitude, int numTiles)
    {      
      var columnIndex = longitude;
      var columnNormalized = (1.0 + columnIndex / Math.PI) / 2.0;
      var column = columnNormalized * numTiles;
      var columnInt = Math.Round(column);
      return (int)columnInt;
    }

    /// <summary>
    /// Calculates the y tile coordinate from latitude
    /// </summary>
    /// <param name="latitude">latitude in radians</param>
    /// <param name="numTiles">number of tiles (calcuated from zoom level)</param>
    /// <returns>y tile coordinate</returns>
    public static int LatitudeToTile(double latitude, int numTiles)
    {    
      var rowIndex = Math.Log(Math.Tan(latitude) + (1.0 / Math.Cos(latitude)));
      var rowNormalized = (1.0 - rowIndex / Math.PI) / 2.0;
      var row = rowNormalized * numTiles;
      var rowInt = Math.Round(row);
      return (int)rowInt;
    }
  }
}
