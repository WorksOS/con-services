using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  public class TileServiceUtils
  {
    /// <summary>
    /// Converts the lat/lng points to pixels and offsets them from the top left corner of the tile.
    /// </summary>
    /// <param name="latLngs"></param>
    /// <param name="pixelTopLeft"></param>
    /// <param name="numTiles"></param>
    /// <returns>The points in pixels relative to the top left corner of the tile.</returns>
    public static PointF[] LatLngToPixelOffset(IEnumerable<Point> latLngs, Point pixelTopLeft, int numTiles)
    {
      List<PointF> pixelPoints = new List<PointF>();
      foreach (Point ll in latLngs)
      {
        Point pixelPt = WebMercatorProjection.LatLngToPixel(ll, numTiles);
        pixelPoints.Add(new PointF((float) (pixelPt.x - pixelTopLeft.x), (float) (pixelPt.y - pixelTopLeft.y)));
      }
      return pixelPoints.ToArray();
    }

    /// <summary>
    /// Overlays the collection of tiles on top of each other and returns a single tile
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="tileList"></param>
    /// <returns></returns>
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
    /// Converts a WKT polygon to points (latitude/longitude)
    /// </summary>
    /// <param name="geometry">The WKT</param>
    /// <returns>A list of latitude/longitude points in degrees</returns>
    public static IEnumerable<Point> GeometryToPoints(string geometry)
    {
      List<Point> latlngs = new List<Point>();
      //Trim off the "POLYGON((" and "))"
      geometry = geometry.Substring(9, geometry.Length - 11);
      var points = geometry.Split(',');
      foreach (var point in points)
      {
        var parts = point.Trim().Split(' ');
        var lng = double.Parse(parts[0]);
        var lat = double.Parse(parts[1]);
        latlngs.Add(new Point
        {
          y = lat,
          x = lng
        });
      }
      return latlngs;
    }

    /// <summary>
    /// Calculates the zoom level from the bounding box
    /// </summary>
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
      while (latSize > selectionLatSize && longSize > selectionLongSize && zoomLevel < MAXZOOM)
      {
        zoomLevel++;
        latSize /= 2;
        longSize /= 2;
      }
      return zoomLevel;
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
  }

}
