using System;

namespace VSS.Productivity3D.WebApi.Models.Notification.Helpers
{
  public class WebMercatorProjection
  {
    //see http://gis.stackexchange.com/questions/66247/what-is-the-formula-for-calculating-world-coordinates-for-a-given-latlng-in-goog/66357#66357

    public const int TILE_SIZE = 256;
    private const int ONE_HALF_CIRCLE = 180;
    private const int ONE_FULL_CIRCLE = 360;

    public static double DegreesToRadians(double deg)
    {
      return deg * (Math.PI / ONE_HALF_CIRCLE);
    }

    public static double RadiansToDegrees(double rad)
    {
      return rad / (Math.PI / ONE_HALF_CIRCLE);
    }

    public static Point FromLatLngToPoint(Point latLng)
    {
      var pt = new Point {
        x = (latLng.Longitude + ONE_HALF_CIRCLE) / ONE_FULL_CIRCLE * TILE_SIZE
      };

      double latRad = DegreesToRadians(latLng.Latitude);
      pt.y = ((1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * Math.Pow(2, 0)) * TILE_SIZE;

      return pt;
    }

    public static Point LatLngToTile(Point latLng, int numTiles)
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

    public static Point LatLngToPixel(Point latLng, int numTiles)
    {
      Point worldPt = FromLatLngToPoint(latLng);
      Point pixelPt = WorldToPixel(worldPt, numTiles);
      return pixelPt;
    }

    public static Point WorldToPixel(Point worldPt, int numTiles)
    {
      return new Point
      {
        x = worldPt.x * numTiles,
        y = worldPt.y * numTiles
      };
    }
  }
}