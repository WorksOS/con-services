using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Proxies;
using CommonModels=VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// Provides geofence tile functionality for reports
  /// </summary>
  public class GeofenceTileService : IGeofenceTileService
  {
    private readonly ILogger log;
    private readonly ILoggerFactory logger;

    const int DEFAULT_BOUNDARY_COLOR = 0xF4511E;

    public GeofenceTileService(ILoggerFactory logger)
    {
      log = logger.CreateLogger<GeofenceTileService>();
      this.logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="filterPolygons">List of filter polygons</param>
    /// <returns></returns>
    public byte[] GetFilterBoundaryBitmap(MapParameters parameters, List<List<CommonModels.WGSPoint>> filterPolygons)
    {
      byte[] sitesImage = null;

      if (filterPolygons != null && filterPolygons.Any())
      {
        using (Bitmap bitmap = new Bitmap(parameters.mapWidth, parameters.mapHeight))
        using (Graphics g = Graphics.FromImage(bitmap))
        {
          foreach (var polygonPoints in filterPolygons)
          {
            if (polygonPoints != null && polygonPoints.Any())
            {
              DrawGeofence(parameters, g, "Filter Boundary", polygonPoints, DEFAULT_BOUNDARY_COLOR, true);
            }
          }
          sitesImage = bitmap.BitmapToByteArray();
        }
      }
      return sitesImage;
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

        using (Bitmap bitmap = new Bitmap(parameters.mapWidth, parameters.mapHeight))
        using (Graphics g = Graphics.FromImage(bitmap))
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

            var sitePoints = RaptorConverters.geometryToPoints(site.GeometryWKT);

            //Exclude site if outside bbox
            bool outside = sitePoints.Min(p => p.Lat) < parameters.bbox.minLat || 
                           sitePoints.Max(p => p.Lat) > parameters.bbox.maxLat ||
                           sitePoints.Min(p => p.Lon) < parameters.bbox.minLng || 
                           sitePoints.Max(p => p.Lon) > parameters.bbox.maxLng;

            if (outside)
            {
              log.LogDebug($"GetGeofencesBitmap excluding site {site.GeofenceUID} outside bbox");
            }
            else
            {
              int siteColor = site.FillColor > 0 ? site.FillColor : (isSites ? DEFAULT_SITE_COLOR : DEFAULT_BOUNDARY_COLOR);
              bool transparent = isSites ? site.IsTransparent : true;
              DrawGeofence(parameters, g, site.GeofenceUID.ToString(), sitePoints, siteColor, transparent);
            }
          }

          sitesImage = bitmap.BitmapToByteArray();
        }
      }

      return sitesImage;
    }

    private void DrawGeofence(MapParameters parameters, Graphics g, string uid, IEnumerable<CommonModels.WGSPoint> points, int color, bool isTransparent)
    {
      const int FILL_TRANSPARENCY = 0x40; //0.25 of FF
      const int STROKE_TRANSPARENCY = 0x73; //0.45 of FF
      const int SITE_OUTLINE_WIDTH = 2;

      log.LogDebug($"DrawGeofence drawing site or boundary {uid}");
      PointF[] pixelPoints = TileServiceUtils.LatLngToPixelOffset(points, parameters.pixelTopLeft, parameters.numTiles);
      if (!isTransparent)
      {
        Brush brush = new SolidBrush(Color.FromArgb(FILL_TRANSPARENCY, Color.FromArgb(color)));
        g.FillPolygon(brush, pixelPoints, FillMode.Alternate);
      }
      Pen pen = new Pen(Color.FromArgb(STROKE_TRANSPARENCY, Color.FromArgb(color)), SITE_OUTLINE_WIDTH);
      g.DrawPolygon(pen, pixelPoints);
    }
  }

  public interface IGeofenceTileService
  {
    byte[] GetSitesBitmap(MapParameters parameters, IEnumerable<GeofenceData> sites);
    byte[] GetBoundariesBitmap(MapParameters parameters, IEnumerable<GeofenceData> customBoundaries);
    byte[] GetFilterBoundaryBitmap(MapParameters parameters, List<List<CommonModels.WGSPoint>> filterPoints);
  }
}
