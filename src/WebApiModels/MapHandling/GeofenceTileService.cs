using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// Provides geofence tile functionality for reports
  /// </summary>
  public class GeofenceTileService : IGeofenceTileService
  {
    private readonly ILogger log;
    private readonly ILoggerFactory logger;

    public GeofenceTileService(ILoggerFactory logger)
    {
      log = logger.CreateLogger<GeofenceTileService>();
      this.logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="sites">List of geofences for the customer</param>
    /// <returns>A bitmap</returns>
    public byte[] GetSitesBitmap(MapParameters parameters, IEnumerable<GeofenceData> sites)
    {
      log.LogInformation("GetSitesBitmap");

      const int DEFAULT_SITE_COLOR = 0x0055FF;
      const int FILL_TRANSPARENCY = 0x40; //0.25 of FF
      const int STROKE_TRANSPARENCY = 0x73; //0.45 of FF
      const int SITE_OUTLINE_WIDTH = 2;

      // Exclude sites that are too small to be displayed in the current viewport. 
      double viewPortArea = Math.Abs(parameters.bbox.minLatDegrees - parameters.bbox.maxLatDegrees) * Math.Abs(parameters.bbox.minLngDegrees - parameters.bbox.maxLngDegrees);
      double minArea = viewPortArea / 10000;

      byte[] sitesImage = null;

      if (sites.Any())
      {
        using (Bitmap bitmap = new Bitmap(parameters.mapWidth, parameters.mapHeight))
        using (Graphics g = Graphics.FromImage(bitmap))
        {
          foreach (var site in sites)
          {
            log.LogDebug($"GetSitesBitmap examining site {site.GeofenceUID}");
            //Old geofences may not have AreaSqMeters set.
            if (site.AreaSqMeters > 0 && site.AreaSqMeters < minArea)
            {
              log.LogDebug($"GetSitesBitmap excluding site {site.GeofenceUID} due to area");
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
              log.LogDebug($"GetSitesBitmap excluding site {site.GeofenceUID} outside bbox");
            }
            else
            {
              log.LogDebug($"GetSitesBitmap drawing site {site.GeofenceUID}");
              PointF[] pixelPoints = TileServiceUtils.LatLngToPixelOffset(sitePoints, parameters.pixelTopLeft, parameters.numTiles);
              int siteColor = site.FillColor > 0 ? site.FillColor : DEFAULT_SITE_COLOR;
              if (!site.IsTransparent)
              {
                Brush brush = new SolidBrush(Color.FromArgb(FILL_TRANSPARENCY, Color.FromArgb(siteColor)));
                g.FillPolygon(brush, pixelPoints, FillMode.Alternate);
              }
              Pen pen = new Pen(Color.FromArgb(STROKE_TRANSPARENCY, Color.FromArgb(siteColor)), SITE_OUTLINE_WIDTH);
              g.DrawPolygon(pen, pixelPoints);
            }
          }

          sitesImage = bitmap.BitmapToByteArray();
        }
      }

      return sitesImage;
    }
  }

  public interface IGeofenceTileService
  {
    byte[] GetSitesBitmap(MapParameters parameters, IEnumerable<GeofenceData> sites);
  }
}
