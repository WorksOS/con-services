using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLib;

namespace CCSS.Geometry
{
  public static class PolygonUtils
  {

    const double EPSILON = 0.00000001;  // for comparing floating points numbers for equality

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
      //If the test point is on the border of the polygon, the ray casting algorithm will deliver unpredictable results;
      //i.e. the result may be “inside” or “outside” depending on arbitrary factors such as how the polygon is oriented with respect to the coordinate system.
      //Therefore check if the point lies on any edge of the polygon. This will include the case of the point being a polygon vertex.
      if (PointOnPolygonEdge(points, longitude, latitude))
        return true;

      // ray casting algorithm
      // from http://alienryderflex.com/polygon/
      var oddNodes = false;

      var count = points.Count;
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
          if (LineSegmentsIntersect(
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
    /// Determines if the line segment defined by points A and B
    /// intersects with the line segment defined by points C and D.
    /// 
    /// public domain function by Darel Rex Finley, 2006
    /// </summary>
    private static bool LineSegmentsIntersect(
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

      //Simple bounding box check first.
      if (!BoundingBoxesOverlap(points1, points2))
        return false;

      // The clipper library doesn't work as expected for touching polygons so check.
      // Check both ways as a vertex of one polygon may be touching an edge of the other.
      foreach (var point in points1)
      {
        if (PointOnPolygonEdge(points2, point.X, point.Y))
          return true;
      }
      foreach (var point in points2)
      {
        if (PointOnPolygonEdge(points1, point.X, point.Y))
          return true;
      }

      // Now do the clipper polygon intersection check.
      var polygon1 = ClipperPolygon(points1);
      var polygon2 = ClipperPolygon(points2);
      var clipper = new Clipper();
      clipper.AddPolygon(polygon1, PolyType.ptSubject);
      clipper.AddPolygon(polygon2, PolyType.ptClip);
      var intersectingPolygons = new List<List<IntPoint>>();
      var succeeded = clipper.Execute(ClipType.ctIntersection, intersectingPolygons);
      return succeeded && intersectingPolygons.Count > 0;
    }

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
      if (count <= 3) // at least 3 points + closing point for valid polygon
      {
        throw new InvalidOperationException("invalid polygon");
      }

      return points;
    }

    /// <summary>
    /// Determines if the bounding boxes of the two polygons overlap.
    /// </summary>
    private static bool BoundingBoxesOverlap(List<Point> points1, List<Point> points2)
    {
      var xmin1 = points1.Min(p => p.X);
      var xmax1 = points1.Max(p => p.X);
      var ymin1 = points1.Min(p => p.Y);
      var ymax1 = points1.Max(p => p.Y);
      var xmin2 = points2.Min(p => p.X);
      var xmax2 = points2.Max(p => p.X);
      var ymin2 = points2.Min(p => p.Y);
      var ymax2 = points2.Max(p => p.Y);
      return IsOverlapping(xmin1, xmax1, xmin2, xmax2) &&
             IsOverlapping(ymin1, ymax1, ymin2, ymax2);
    }

    /// <summary>
    /// Determines if the two intervals overlap.
    /// </summary>
    private static bool IsOverlapping(double min1, double max1, double min2, double max2)
    {
      return max1 >= min2 && max2 >= min1;
    }

    /// <summary>
    /// Determines if the point lies on any edge of the polygon, including coinciding with a polygon vertex.
    /// </summary>
    private static bool PointOnPolygonEdge(List<Point> points, double longitude, double latitude)
    {
      var count = points.Count;
      for (var i = 1; i < count; i++)
      {
        if (PointOnLine(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y, longitude, latitude))
          return true;
      }

      return false;
    }

    /// <summary>
    /// Determines if the point (cx,cy) lies on the line segment (ax,ay) to (bx,by)
    /// </summary>
    public static bool PointOnLine(double ax, double ay, double bx, double by, double cx, double cy)
    {
      var crossProduct = (cy - ay) * (bx - ax) - (cx - ax) * (by - ay);

      if (Math.Abs(crossProduct) > EPSILON)
        return false;

      var dotProduct = (cx - ax) * (bx - ax) + (cy - ay) * (by - ay);
      if (dotProduct < 0)
        return false;

      var squaredLineLength = (bx - ax) * (bx - ax) + (by -ay) * (by - ay);
      if (dotProduct > squaredLineLength)
        return false;

      return true;
    }

  }
}
