using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Extensions;
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
    /// Converts the lat/lng points to pixels and offsets them from the top left corner of the tile.
    /// </summary>
    /// <param name="latLngs">The list of points to convert in radians</param>
    /// <param name="pixelTopLeft">The top left corner of the tile in pixels</param>
    /// <param name="numTiles">The number of tiles for the zoom level</param>
    /// <returns>The points in pixels relative to the top left corner of the tile.</returns>
    public static PointF[] LatLngToPixelOffset(IEnumerable<WGSPoint> latLngs, Point pixelTopLeft, long numTiles)
    {
      List<PointF> pixelPoints = new List<PointF>();
      foreach (var ll in latLngs)
      {
        Point pixelPt = LatLngToPixel(ll.Lat, ll.Lon, numTiles);
        pixelPoints.Add(new PointF((float) (pixelPt.x - pixelTopLeft.x), (float) (pixelPt.y - pixelTopLeft.y)));
      }
      return pixelPoints.ToArray();
    }

    /// <summary>
    /// Overlays the collection of tiles on top of each other and returns a single tile
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="tileList">The list of tiles to overlay</param>
    /// <returns>A single bitmap of the overlayed tiles</returns>
    public static byte[] OverlayTiles(MapParameters parameters, IEnumerable<byte[]> tileList)
    {
      byte[] overlayData = null;
      //Overlay the tiles. Return an empty tile if none to overlay.
      System.Drawing.Point origin = new System.Drawing.Point(0, 0);
      using (Bitmap bitmap = new Bitmap(parameters.mapWidth, parameters.mapHeight))
      using (Graphics g = Graphics.FromImage(bitmap))
      {
        foreach (byte[] tileData in tileList)
        {
          if (tileData != null)
          {
            using (var tileStream = new MemoryStream(tileData))
            {
              Image image = Image.FromStream(tileStream);
              g.DrawImage(image, origin);
            }
          }
        }
        overlayData = bitmap.BitmapToByteArray();
      }

      return overlayData;
    }

    /// <summary>
    /// Overlays the collection of tiles on top of each other and returns a single tile
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="tileList">The list of tiles to overlay</param>
    /// <returns>A single bitmap of the overlayed tiles</returns>
    public static byte[] OverlayTiles(MapParameters parameters, IDictionary<TileOverlayType,byte[]> tileList)
    {
      //Order for overlays: 
      List<TileOverlayType> orderedOverlayTypes = new List<TileOverlayType>
      {
        TileOverlayType.BaseMap, TileOverlayType.ProjectBoundary, TileOverlayType.Geofences, TileOverlayType.ProductionData,
        TileOverlayType.FilterCustomBoundary, TileOverlayType.FilterDesignBoundary, TileOverlayType.FilterAlignmentBoundary,
        TileOverlayType.CutFillDesignBoundary, TileOverlayType.DxfLinework, TileOverlayType.Alignments
      };
      //Make an orderd list
      List<byte[]> overlays = new List<byte[]>();
      foreach (var overLayType in orderedOverlayTypes)
      {
        if (tileList.ContainsKey(overLayType))
        {
          overlays.Add(tileList[overLayType]);
        }
      }
      return OverlayTiles(parameters,overlays);
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
        return $"{designDescriptor.File.FilespaceId}:{designDescriptor.File.Path}/{designDescriptor.File.FileName}";

      return designDescriptor.Id.ToString();
    }

    /// <summary>
    /// Determines if a polygon lies outside a bounding box.
    /// </summary>
    /// <param name="bbox">The bounding box</param>
    /// <param name="points">The polygon</param>
    /// <returns>True if the polygon is completely outside the bounding box otherwise false</returns>
    public static bool Outside(MapBoundingBox bbox, List<WGSPoint> points)
    {
      return points.Min(p => p.Lat) > bbox.maxLat ||
             points.Max(p => p.Lat) < bbox.minLat ||
             points.Min(p => p.Lon) > bbox.maxLng ||
             points.Max(p => p.Lon) < bbox.minLng;
    }

  }

}
