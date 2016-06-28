using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLib;
using Common.Utilities;
using LandfillService.Common.Models;

namespace VSS.MasterData.Common.Helpers
{
  public static class Geometry
  {
    public static bool GeofencesOverlap(string projectGeometry, string geofenceGeometry)
    {
      List<IntPoint> geofencePolygon = ClipperPolygon(geofenceGeometry);
      return GeofencesOverlap(projectGeometry, geofencePolygon);
    }

    public static bool GeofencesOverlap(string projectGeometry, List<IntPoint> geofencePolygon)
    {
      List<List<IntPoint>> intersectingPolys = ClipperIntersects(
        ClipperPolygon(projectGeometry), geofencePolygon);
      return intersectingPolys != null && intersectingPolys.Count > 0;
    }

    public static List<IntPoint> ClipperPolygon(string geofenceGeometry)
    {
      const float SCALE = 100000;

      //TODO: Make a common utility method for GeometryToPoints
      IEnumerable<WGSPoint> points = ConversionUtil.GeometryToPoints(geofenceGeometry);
      return points.Select(p => new IntPoint { X = (Int64)(p.Lon * SCALE), Y = (Int64)(p.Lat * SCALE) }).ToList();
    }

    private static List<List<IntPoint>> ClipperIntersects(List<IntPoint> oldPolygon, List<IntPoint> newPolygon)
    {
      //Note: the clipper library uses 2D geometry while we have spherical coordinates. But it should be near enough for our purposes.

      Clipper c = new Clipper();
      c.AddPath(oldPolygon, PolyType.ptSubject, true);
      c.AddPath(newPolygon, PolyType.ptClip, true);
      List<List<IntPoint>> solution = new List<List<IntPoint>>();
      bool succeeded = c.Execute(ClipType.ctIntersection, solution, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
      return succeeded ? solution : null;
    }


  }
}
