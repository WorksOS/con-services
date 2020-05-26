using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLib;

namespace CCSS.Geometry
{
  public static class PolygonUtils
  {
    /// <summary>
    /// Determines if the point is inside the polygon.
    /// If the point lies on the edge of the polygon it is considered inside.
    /// If the point coincides with a vertex it is considered inside.
    /// </summary>
    public static bool PointInPolygon(string polygonWkt, double latitude, double longitude)
    {
      var points = ConvertAndValidatePolygon(polygonWkt);
      return PointInPolygon(points, latitude, longitude);
    }

    private static bool PointInPolygon(List<Point> points, double latitude, double longitude)
    {
      // Check if the point matches any vertex of the polygon.
      // This is because the algorithm returns false when we want true in this case.
      var count = points.Count;
      for (var i = 0; i < count; i++)
      {
        if (EqualPoints(points[i].X, points[i].Y, longitude, latitude))
          return true;
      }

      // from http://alienryderflex.com/polygon/
      // ray casting algorithm
      var oddNodes = false;

      var j = count - 1;
      for (var i = 0; i < count; i++)
      {
        // Get the points only once.  Even though the accessor is fast, multiple gets are a measured culprit of poor performance
        var pointILatitude = points[i].Y;
        var pointJLatitude = points[j].Y;
        if (pointILatitude < latitude && pointJLatitude >= latitude
            || pointJLatitude < latitude && pointILatitude >= latitude)
        {
          var pointILongitude = points[i].X;
          var pointJLongitude = points[j].X;
          if (pointILongitude + (latitude - pointILatitude) /
            (pointJLatitude - pointILatitude) * (pointJLongitude - pointILongitude) < longitude)
          {
            oddNodes = !oddNodes;
          }
        }
        j = i;
      }

      return oddNodes;
    }

    /// <summary>
    /// Determines if the polygon is self intersecting.
    /// </summary>
    public static bool SelfIntersectingPolygon(string polygonWkt)
    {
      var points = ConvertAndValidatePolygon(polygonWkt);

      //Brute force approach. Compare each line segment with every other line segment.
      //Could use Shamos-Hoey if brute force approach too inefficient.

      var count = points.Count;

      for (var i = 0; i < count - 1; i++)
      {
        for (int j = i + 1; j < count - 1; j++)
        {
          if (LineSegmentIntersection(
            points[i].X, points[i].Y,
            points[i + 1].X, points[i + 1].Y,
            points[j].X, points[j].Y,
            points[j + 1].X, points[j + 1].Y))
          {
            return true;
          }
        }
      }

      return false;
    }

    /// <summary>
    /// Determines the intersection point of the line segment defined by points A and B
    /// with the line segment defined by points C and D.
    /// 
    /// public domain function by Darel Rex Finley, 2006
    /// </summary>
    private static bool LineSegmentIntersection(
        double Ax, double Ay,
        double Bx, double By,
        double Cx, double Cy,
        double Dx, double Dy)
    {
      //  Fail if either line segment is zero-length.
      if (EqualPoints(Ax, Ay, Bx, By) || EqualPoints(Cx, Cy, Dx, Dy)) return false;

      //  Fail if the segments share an end-point.
      if (EqualPoints(Ax, Ay, Cx, Cy) || EqualPoints(Bx, By, Cx, Cy) ||
      EqualPoints(Ax, Ay, Dx, Dy) || EqualPoints(Bx, By, Dx, Dy))
      {
        return false;
      }

      //  (1) Translate the system so that point A is on the origin.
      Bx -= Ax; By -= Ay;
      Cx -= Ax; Cy -= Ay;
      Dx -= Ax; Dy -= Ay;

      //  Discover the length of segment A-B.
      var distAB = Math.Sqrt(Bx * Bx + By * By);

      //  (2) Rotate the system so that point B is on the positive X axis.
      var theCos = Bx / distAB;
      var theSin = By / distAB;
      var newX = Cx * theCos + Cy * theSin;
      Cy = Cy * theCos - Cx * theSin; Cx = newX;
      newX = Dx * theCos + Dy * theSin;
      Dy = Dy * theCos - Dx * theSin; Dx = newX;

      //  Fail if segment C-D doesn't cross line A-B.
      if (Cy < 0 && Dy < 0 || Cy >= 0 && Dy >= 0) return false;

      //  (3) Discover the position of the intersection point along line A-B.
      var ABpos = Dx + (Cx - Dx) * Dy / (Dy - Cy);

      //  Fail if segment C-D crosses line A-B outside of segment A-B.
      if (ABpos < 0 || ABpos > distAB) return false;

      //  Success.
      return true;
    }

    /// <summary>
    /// Determines if the two points are considered equal.
    /// </summary>
    private static bool EqualPoints(double x1, double y1, double x2, double y2)
    {
      const double EPSILON = 0.000001;
      return Math.Abs(x1 - x2) < EPSILON && Math.Abs(y1 - y2) < EPSILON;
    }

    /// <summary>
    /// Determines if the two polygons overlap.
    /// Polygons that touch at a vertex or along an edge are considered overlapping.
    /// </summary>
    public static bool OverlappingPolygons(string polygonWkt1, string polygonWkt2)
    {
      //Note: the clipper library uses 2D geometry while we have spherical coordinates. But it should be near enough for our purposes.

      var points1 = ConvertAndValidatePolygon(polygonWkt1);
      var points2 = ConvertAndValidatePolygon(polygonWkt2);
      // The clipper library considers polygons touching at a vertex or edge to be non-overlapping but we want them to be overlapping.
      // Check this first.
      foreach (var point in points1)
      {
        if (PointInPolygon(points2, point.Y, point.X))
          return true;
      }

      // Now do the intersection check.
      var polygon1 = ClipperPolygon(points1);
      var polygon2 = ClipperPolygon(points2);
      var clipper = new Clipper();
      clipper.AddPolygon(polygon1, PolyType.ptSubject);
      clipper.AddPolygon(polygon2, PolyType.ptClip);
      var intersectingPolygons = new List<List<IntPoint>>();
      var succeeded = clipper.Execute(ClipType.ctIntersection, intersectingPolygons);
      return succeeded && intersectingPolygons.Count > 0;
    }

    /*
    /// <summary>
    /// Converts the polygon WKT to the polygon model for the clipper library.
    /// </summary>
    private static List<IntPoint> ClipperPolygon(string polygonWkt)
    {
      const float SCALE = 100000;
      var points = ConvertAndValidatePolygon(polygonWkt);
      var clipperPolygon = points.Select(p => new IntPoint { X = (long)(p.X * SCALE), Y = (long)(p.Y * SCALE) }).ToList();
      return clipperPolygon;
    }
    */

    /// <summary>
    /// Converts the polygon points to the polygon model for the clipper library.
    /// </summary>
    private static List<IntPoint> ClipperPolygon(List<Point> points)
    {
      const float SCALE = 100000;
      var clipperPolygon = points.Select(p => new IntPoint { X = (long)(p.X * SCALE), Y = (long)(p.Y * SCALE) }).ToList();
      return clipperPolygon;
    }

    /// <summary>
    /// Converts the polygon WKT to a list of points. Closes the polygon if it is not already closed. Validates there are at least 3 points. 
    /// </summary>
    private static List<Point> ConvertAndValidatePolygon(string polygonWkt)
    {
      var points = polygonWkt?.ParseGeometryData().ClosePolygonIfRequired();
      var count = points?.Count ?? 0;
      if (count < 3)
      {
        throw new InvalidOperationException("invalid polygon");
      }

      return points;
    }
  }
}
