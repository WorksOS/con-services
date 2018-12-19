using System;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using Point = VSS.MasterData.Models.Models.Point;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// Utilities for map tiles for reports
  /// </summary>
  public class TileServiceUtils
  {
    /// <summary>
    /// Converts the lat/lng point to pixels
    /// </summary>
    /// <param name="latitude">The latitude to convert in radians</param>
    /// <param name="longitude">The longitude to convert in radians</param>
    /// <param name="numTiles">The number of tiles</param>
    /// <returns>Pixel point</returns>
    public static Point LatLngToPixel(double latitude, double longitude, long numTiles)
    {
      var point = new Point(latitude.LatRadiansToDegrees(), longitude.LonRadiansToDegrees());
      return WebMercatorProjection.LatLngToPixel(point, numTiles);
    }

    /// <summary>
    /// Calculates the zoom level from the bounding box
    /// </summary>
    /// <param name="deltaLat">The height (maximum latitude - minimum latitude) of the bounding box in radians</param>
    /// <param name="deltaLng">The width (maximum longitude - minimum longitude) of the bounding box in radians</param>
    /// <returns>The zoom level</returns>
    public static int CalculateZoomLevel(double deltaLat, double deltaLng)
    {
      const int MAXZOOM = 24;

      double selectionLatSize = Math.Abs(deltaLat);
      double selectionLongSize = Math.Abs(deltaLng);

      //Google maps zoom level starts at 0 for whole world (-90.0 to 90.0, -180.0 to 180.0)
      //and doubles the precision both horizontally and vertically for each suceeding level.
      int zoomLevel = 0;
      double latSize = Math.PI; //180.0;
      double longSize = 2 * Math.PI; //360.0;


      while (CompareWithPrecision(latSize,selectionLatSize) > 0 && CompareWithPrecision(longSize,selectionLongSize) > 0 && zoomLevel < MAXZOOM)
      {
        zoomLevel++;
        latSize /= 2;
        longSize /= 2;
      }
      return zoomLevel;
    }

    private static int CompareWithPrecision(double d1, double d2)
    {
      var d1_rounded = Math.Round(d1, 9);
      var d2_rounded = Math.Round(d2, 9);
      return d1_rounded.CompareTo(d2_rounded);
    }

    /// <summary>
    /// Calculates the number of tiles for the specified zoom level.
    /// </summary>
    /// <param name="zoomLevel"></param>
    /// <returns></returns>
    public static int NumberOfTiles(int zoomLevel)
    {
      return  1 << zoomLevel; //equivalent to 2 to the power of zoomLevel
    }

    /// <summary>
    /// Get a description of the design for logging
    /// </summary>
    /// <param name="designDescriptor">The design</param>
    /// <returns>A descriptive string of the design properties</returns>
    public static string DesignDescriptionForLogging(DesignDescriptor designDescriptor)
    {
      if (designDescriptor == null)
        return string.Empty;

      if (designDescriptor.File != null)
        return $"{designDescriptor.File.filespaceId}:{designDescriptor.File.path}/{designDescriptor.File.fileName}";

      return designDescriptor.Id.ToString();
    }
  }

}
