using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using VSS.MasterData.Models.Models;
using VSS.Tile.Service.Common.Extensions;
using VSS.Tile.Service.Common.Helpers;
using VSS.Tile.Service.Common.Models;
using PointF = SixLabors.Primitives.PointF;

namespace VSS.Tile.Service.Common.Services
{
  /// <summary>
  /// Provides geofence tile functionality for reports
  /// </summary>
  public class GeofenceTileService : IGeofenceTileService
  {
    private readonly ILogger log;

    const int DEFAULT_CUSTOM_BOUNDARY_COLOR = 0xF4511E;
    const int DEFAULT_DESIGN_BOUNDARY_COLOR = 0x008DBD;
    const int DEFAULT_ALIGNMENT_BOUNDARY_COLOR = 0xFF0000;

    public GeofenceTileService(ILoggerFactory logger)
    {
      log = logger.CreateLogger<GeofenceTileService>();
    }

    /// <summary>
    /// Gets a map tile with spatial filter boundaries drawn on it
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="filterPolygons">List of filter polygons</param>
    /// <param name="boundaryType">Type of filter boundary which determines the color</param>
    /// <returns>A bitmap</returns>
    public byte[] GetFilterBoundaryBitmap(MapParameters parameters, List<List<WGSPoint>> filterPolygons, FilterBoundaryType boundaryType)
    {
      byte[] geofenceImage = null;

      if (filterPolygons != null && filterPolygons.Any())
      {
        using (Image<Rgba32> bitmap = new Image<Rgba32>(parameters.mapWidth, parameters.mapHeight))
        {
          foreach (var polygonPoints in filterPolygons)
          {
            if (polygonPoints != null && polygonPoints.Any())
            {
              int color = 0;
              switch (boundaryType)
              {
                  case FilterBoundaryType.Alignment:
                    color = DEFAULT_ALIGNMENT_BOUNDARY_COLOR;
                    break;
                case FilterBoundaryType.Design:
                  color = DEFAULT_DESIGN_BOUNDARY_COLOR;
                  break;
                default:
                  color = DEFAULT_CUSTOM_BOUNDARY_COLOR;
                  break;
              }
              DrawGeofence(parameters, bitmap, $"{boundaryType} Filter Boundary", polygonPoints, color, true);
            }
          }
          geofenceImage = bitmap.BitmapToByteArray();
        }
      }
      return geofenceImage;
    }
    /// <summary>
    /// Gets a map tile with geofences drawn on it.
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="sites">List of geofences for the customer</param>
    /// <returns>A bitmap</returns>
    public byte[] GetSitesBitmap(MapParameters parameters, IEnumerable<GeofenceData> sites)
    {
      return GetGeofencesBitmap(parameters, sites, true);
    }

    /// <summary>
    /// Gets a map tile with custom boundaries drawn on it.
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="customBoundaries">List of custom boundaries for the project</param>
    /// <returns>A bitmap</returns>
    public byte[] GetBoundariesBitmap(MapParameters parameters, IEnumerable<GeofenceData> customBoundaries)
    {
      return GetGeofencesBitmap(parameters, customBoundaries, false);
    }

    /// <summary>
    /// Gets a map tile with geofences drawn on it.
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="sites">List of geofences for the customer</param>
    /// <returns>A bitmap</returns>
    private byte[] GetGeofencesBitmap(MapParameters parameters, IEnumerable<GeofenceData> sites, bool isSites)
    {
      log.LogInformation("GetGeofencesBitmap");

      const int DEFAULT_SITE_COLOR = 0x0055FF;

      byte[] sitesImage = null;

      if (sites != null && sites.Any())
      {
        // Exclude sites that are too small to be displayed in the current viewport. 
        double viewPortArea = Math.Abs(parameters.bbox.minLatDegrees - parameters.bbox.maxLatDegrees) * Math.Abs(parameters.bbox.minLngDegrees - parameters.bbox.maxLngDegrees);
        double minArea = viewPortArea / 10000;

        using (Image<Rgba32> bitmap = new Image<Rgba32>(parameters.mapWidth, parameters.mapHeight))
        {
          foreach (var site in sites)
          {
            log.LogDebug($"GetGeofencesBitmap examining site {site.GeofenceUID}");
            //Old geofences may not have AreaSqMeters set.
            if (site.AreaSqMeters > 0 && site.AreaSqMeters < minArea)
            {
              log.LogDebug($"GetGeofencesBitmap excluding site {site.GeofenceUID} due to area");
              continue;
            }

            var sitePoints = site.GeometryWKT.GeometryToPoints().ToList();

            //Exclude site if outside bbox
            bool outside = TileServiceUtils.Outside(parameters.bbox, sitePoints);

            if (outside)
            {
              log.LogDebug($"GetGeofencesBitmap excluding site {site.GeofenceUID} outside bbox");
            }
            else
            {
              int siteColor = site.FillColor > 0 ? site.FillColor : (isSites ? DEFAULT_SITE_COLOR : DEFAULT_CUSTOM_BOUNDARY_COLOR);
              bool transparent = isSites ? site.IsTransparent : true;
              DrawGeofence(parameters, bitmap, site.GeofenceUID.ToString(), sitePoints, siteColor, transparent);
            }
          }

          sitesImage = bitmap.BitmapToByteArray();
        }
      }

      return sitesImage;
    }

    private void DrawGeofence(MapParameters parameters, Image<Rgba32> bitmap, string uid, IEnumerable<WGSPoint> points, int color, bool isTransparent)
    {
      const byte FILL_TRANSPARENCY = 0x40; //0.25 of FF
      const byte STROKE_TRANSPARENCY = 0x73; //0.45 of FF
      const int SITE_OUTLINE_WIDTH = 2;

      log.LogDebug($"DrawGeofence drawing site or boundary {uid}");
      var red = (byte)((color & 0xFF0000) >> 16);
      var green = (byte)((color & 0x00FF00) >> 8);
      var blue = (byte)(color & 0x0000FF);
      PointF[] pixelPoints = TileServiceUtils.LatLngToPixelOffset(points, parameters.pixelTopLeft, parameters.numTiles);
      if (!isTransparent)
      {
        var fillColor = new Rgba32(red, green, blue, FILL_TRANSPARENCY);
        bitmap.Mutate(ctx => ctx.FillPolygon(fillColor, pixelPoints));
      }
      var lineColor = new Rgba32(red, green, blue, STROKE_TRANSPARENCY);
      bitmap.Mutate(ctx => ctx.DrawPolygon(lineColor, SITE_OUTLINE_WIDTH, pixelPoints));
    }
  }

  public interface IGeofenceTileService
  {
    byte[] GetSitesBitmap(MapParameters parameters, IEnumerable<GeofenceData> sites);
    byte[] GetBoundariesBitmap(MapParameters parameters, IEnumerable<GeofenceData> customBoundaries);
    byte[] GetFilterBoundaryBitmap(MapParameters parameters, List<List<WGSPoint>> filterPoints, FilterBoundaryType boundaryType);
  }
}
