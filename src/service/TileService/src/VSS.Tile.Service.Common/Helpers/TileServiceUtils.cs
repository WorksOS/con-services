using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using VSS.Tile.Service.Common.Extensions;
using VSS.Tile.Service.Common.Models;
using VSS.MasterData.Models.Models;
using MasterDataModels = VSS.MasterData.Models.Models;

namespace VSS.Tile.Service.Common.Helpers
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
    public static MasterDataModels.Point LatLngToPixel(double latitude, double longitude, long numTiles)
    {
      var point = new MasterDataModels.Point(latitude.LatRadiansToDegrees(), longitude.LonRadiansToDegrees());
      return WebMercatorProjection.LatLngToPixel(point, numTiles);
    }

    /// <summary>
    /// Converts the lat/lng points to pixels and offsets them from the top left corner of the tile.
    /// </summary>
    /// <param name="latLngs">The list of points to convert in radians</param>
    /// <param name="pixelTopLeft">The top left corner of the tile in pixels</param>
    /// <param name="numTiles">The number of tiles for the zoom level</param>
    /// <returns>The points in pixels relative to the top left corner of the tile.</returns>
    public static PointF[] LatLngToPixelOffset(IEnumerable<WGSPoint> latLngs, MasterDataModels.Point pixelTopLeft, long numTiles)
    {
      List<PointF> pixelPoints = new List<PointF>();
      foreach (WGSPoint ll in latLngs)
      {
        MasterDataModels.Point pixelPt = LatLngToPixel(ll.Lat, ll.Lon, numTiles);
        pixelPoints.Add(new PointF((float) (pixelPt.x - pixelTopLeft.x), (float) (pixelPt.y - pixelTopLeft.y)));
      }
      return pixelPoints.ToArray();
    }

    /// <summary>
    /// Returns an empty tile
    /// </summary>
    public static byte[] EmptyTile(int width, int height)
    {
      using (Image<Rgba32> bitmap = new Image<Rgba32>(width, height))
      {
        return bitmap.BitmapToByteArray();
      }
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
      using (Image<Rgba32> bitmap = new Image<Rgba32>(parameters.mapWidth, parameters.mapHeight))
      {
        foreach (byte[] tileData in tileList)
        {
          if (tileData != null && tileData.Length > 0)
          {
            using (var tileStream = new MemoryStream(tileData))
            {
              var image = Image.Load<Rgba32>(tileStream);
              bitmap.Mutate(ctx => ctx.DrawImage(image, 1f));
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
        TileOverlayType.BaseMap, TileOverlayType.ProjectBoundary, TileOverlayType.Geofences, TileOverlayType.GeofenceBoundary,
        TileOverlayType.ProductionData, TileOverlayType.LoadDumpData,
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

    /*
    /// <summary>
    /// Get a description of the design for logging
    /// </summary>
    /// <param name="designDescriptor">The design</param>
    /// <returns>A descriptive string of the design properties</returns>
    public static string DesignDescriptionForLogging(DesignDescriptor designDescriptor)
    {
      if (designDescriptor == null)
        return string.Empty;

      if (designDescriptor.file != null)
        return $"{designDescriptor.file.filespaceId}:{designDescriptor.file.path}/{designDescriptor.file.fileName}";

      return designDescriptor.id.ToString();
    }
    */

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
