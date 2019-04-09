using System;
using System.Collections.Generic;
using VLPDDecls;

namespace VSS.Productivity3D.Common.Algorithms
{
  public class DouglasPeucker
  {
    /// <summary>
    /// Simplify the polyline using Douglas-Peucker algorithm. The algorithm always includes (starts with) the first and last point.
    /// </summary>
    public List<TCoordPoint> SimplifyPolyline(List<TCoordPoint> nePoints, double distanceTolerance)
    {
      var included = new List<bool>();
      
      // TODO (Aaron) init better.
      for (var i = 0; i < nePoints.Count; i++)
      {
        included.Add(i == 0 || i == nePoints.Count - 1);
      }

      ReducePointCount(nePoints, 0, nePoints.Count - 1, included, distanceTolerance);
      var newPoints = new List<TCoordPoint>();

      for (var i = 0; i < nePoints.Count; i++)
      {
        if (included[i]) newPoints.Add(nePoints[i]);
      }

      return newPoints;
    }

    //public TWGS84LineworkBoundary[] SimplifyPolyline(TWGS84LineworkBoundary[] nePoints, double distanceTolerance)
    //{
    //  var included = new List<bool>();

    //  for (var i = 0; i < nePoints.Length; i++)
    //  {
    //    included.Add(i == 0 || i == nePoints.Length - 1);
    //  }
    //}
    
    private static void ReducePointCount(List<TCoordPoint> nePoints, int startIndex, int endIndex, List<bool> included, double distanceTolerance)
    {
      var maxDistanceIndex = -1;
      double maxDistance = 0;

      for (var i = startIndex + 1; i < endIndex; i++)
      {
        double distance =
          LineToPointDistance2D(nePoints[startIndex].X, nePoints[startIndex].Y, nePoints[endIndex].X, nePoints[endIndex].Y, nePoints[i].X, nePoints[i].Y, true);

        if (distance > maxDistance)
        {
          maxDistance = distance;
          maxDistanceIndex = i;
        }
      }

      if (maxDistance > distanceTolerance)
      {
        included[maxDistanceIndex] = true;

        ReducePointCount(nePoints, startIndex, maxDistanceIndex, included, distanceTolerance);
        ReducePointCount(nePoints, maxDistanceIndex, endIndex, included, distanceTolerance);
      }
    }

    /// <summary>
    /// Compute the distance from AB to C.
    /// </summary>
    /// <remarks>
    /// If isSegment is true, AB is a segment, not a line.
    /// </remarks>
    private static double LineToPointDistance2D(double ax, double ay, double bx, double by, double cx, double cy, bool isSegment)
    {
      double dist = CrossProduct(ax, ay, bx, by, cx, cy) / Distance(ax, ay, bx, by);

      if (isSegment)
      {
        double dot1 = DotProduct(ax, ay, bx, by, cx, cy);
        if (dot1 > 0) return Distance(bx, by, cx, cy);

        double dot2 = DotProduct(bx, by, ax, ay, cx, cy);
        if (dot2 > 0) return Distance(ax, ay, cx, cy);
      }

      return Math.Abs(dist);
    }

    /// <summary>
    /// Compute the dot product AB . AC
    /// </summary>
    private static double DotProduct(double ax, double ay, double bx, double by, double cx, double cy)
    {
      double abx = bx - ax;
      double aby = by - ay;
      double bcx = cx - bx;
      double bcy = cy - by;

      return abx * bcx + aby * bcy;
    }

    /// <summary>
    /// Compute the cross product AB x AC
    /// </summary>
    private static double CrossProduct(double ax, double ay, double bx, double by, double cx, double cy)
    {
      double abx = bx - ax;
      double aby = by - ay;
      double acx = cx - ax;
      double acy = cy - ay;

      return abx * acy - aby * acx;
    }

    /// <summary>
    /// Compute the distance from A to B
    /// </summary>
    public static double Distance(double ax, double ay, double bx, double by)
    {
      return Math.Sqrt(SquaredDistance(ax, ay, bx, by));
    }

    private static double SquaredDistance(double ax, double ay, double bx, double by)
    {
      double d1 = ax - bx;
      double d2 = ay - by;

      return d1 * d1 + d2 * d2;
    }
  }
}
