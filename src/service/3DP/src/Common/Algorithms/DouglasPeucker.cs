using System;
using System.Collections.Generic;
using System.Linq;
using C5;
using VLPDDecls;

namespace VSS.Productivity3D.Common.Algorithms
{
  public class DouglasPeucker
  {
    /// <summary>
    /// Uses the Douglas Peucker alogrithm to reduce a polyline to a specifc number of points.
    /// 
    /// The Original Code is 'psimpl - generic n-dimensional polyline simplification'.
    /// The Initial Developer of the Original Code is Elmar de Koning.
    /// </summary>
    /// <remarks>
    /// This is not a pure implementation of Douglas Peucker because it lacks the tolerance input that is normally used to
    /// create point reduction based on the maximum distance between the original curve and simplified curve.
    /// </remarks>
    /// <returns>Returns coorindate array reduced to the target point count.</returns>
    public static List<double[]> DouglasPeuckerByCount(TWGS84Point[] coordinates, int maxVertexLimit)
    {
      if (maxVertexLimit < 0) maxVertexLimit = coordinates.GetLength(0);
      if (maxVertexLimit > 1500) maxVertexLimit = 1500;

      // Zero out keys and mark the first and last 'to keep'.
      var keys = Enumerable.Repeat(false, coordinates.Length).ToList();
      keys[0] = true;
      keys[coordinates.Length - 1] = true;

      var keyCount = 2;

      // Sorted (max dist2) queue containing sub-polylines.
      IPriorityQueue<SubPolyAlt> queue = new IntervalHeap<SubPolyAlt>();

      // Add initial polys
      SubPolyAlt subPoly;
      for (var i = 1; i < keyCount; i++)
      {
        subPoly = new SubPolyAlt(0, coordinates.Length - 1);
        subPoly.subPolyKeyInfo = FindKey(coordinates, subPoly.FirstPointCoordIndex, subPoly.LastPointCoordIndex);
        queue.Add(subPoly);
      }

      while (!queue.IsEmpty)
      {
        // Take a sub poly
        subPoly = queue.DeleteMax();
        // And store the key
        keys[subPoly.subPolyKeyInfo.CoordinateKeyIndex] = true;

        // check point count tolerance
        keyCount++;
        if (keyCount == maxVertexLimit) break;

        // split the polyline at the key and recurse
        var subPolyLeft = new SubPolyAlt(subPoly.FirstPointCoordIndex, subPoly.subPolyKeyInfo.CoordinateKeyIndex);
        subPolyLeft.subPolyKeyInfo = FindKey(coordinates, subPolyLeft.FirstPointCoordIndex, subPolyLeft.LastPointCoordIndex);

        if (subPolyLeft.subPolyKeyInfo.CoordinateKeyIndex > 0) queue.Add(subPolyLeft);

        var subPolyRight = new SubPolyAlt(subPoly.subPolyKeyInfo.CoordinateKeyIndex, subPoly.LastPointCoordIndex);
        subPolyRight.subPolyKeyInfo = FindKey(coordinates, subPolyRight.FirstPointCoordIndex, subPolyRight.LastPointCoordIndex);

        if (subPolyRight.subPolyKeyInfo.CoordinateKeyIndex > 0) queue.Add(subPolyRight);
      }

      var fencePoints = new List<double[]>(keys.Count);

      for (var i = 0; i < coordinates.Length; i++)
      {
        if (keys[i]) fencePoints.Add(new[] { coordinates[i].Lon, coordinates[i].Lat });
      }

      return fencePoints;
    }

    /// <summary>
    /// Uses the Douglas Peucker alogrithm to reduce a polyline to a specifc number of points.
    /// </summary>
    public static List<double[]> DouglasPeuckerByCount(double[,] coordinates, int maxVertexLimit)
    {
      var pointCount = coordinates.GetLength(0);
      var wgsPointArray = new TWGS84Point[pointCount];

      for (var i = 0; i < pointCount; i++)
      {
        wgsPointArray[i].Lon = coordinates[i, 0];
        wgsPointArray[i].Lat = coordinates[i, 1];
      }

      return DouglasPeuckerByCount(wgsPointArray, maxVertexLimit);
    }

    /// <summary>
    /// Compute the dot product AB . AC
    /// </summary>
    private static double ComputeDotProduct(TWGS84Point firstPoint, TWGS84Point lastPoint, TWGS84Point currentPoint)
    {
      double abx = lastPoint.Lon - firstPoint.Lon;
      double aby = lastPoint.Lat - firstPoint.Lat;
      double bcx = currentPoint.Lon - lastPoint.Lon;
      double bcy = currentPoint.Lat - lastPoint.Lat;
      double dot = abx * bcx + aby * bcy;

      return dot;
    }

    /// <summary>
    /// Compute the cross product AB x AC
    /// </summary>
    private static double ComputeCrossProduct(TWGS84Point firstPoint, TWGS84Point lastPoint, TWGS84Point currentPoint)
    {
      double abx = lastPoint.Lon - firstPoint.Lon;
      double aby = lastPoint.Lat - firstPoint.Lat;
      double acx = currentPoint.Lon - firstPoint.Lon;
      double acy = currentPoint.Lat - firstPoint.Lat;
      double cross = abx * acy - aby * acx;

      return cross;
    }

    /// <summary>
    /// Compute the distance from A to B
    /// </summary>
    private static double ComputeDistance(TWGS84Point firstPoint, TWGS84Point lastPoint) => Math.Sqrt(ComputeSquaredDistance(firstPoint, lastPoint));

    private static double ComputeSquaredDistance(TWGS84Point firstPoint, TWGS84Point lastPoint)
    {
      double d1 = firstPoint.Lon - lastPoint.Lon;
      double d2 = firstPoint.Lat - lastPoint.Lat;

      return d1 * d1 + d2 * d2;
    }

    /// <summary>
    /// Compute the distance from AB to C.
    /// If isSegment is true, AB is a segment, not a line.
    /// </summary>
    private static double LineToPointDistance2D(TWGS84Point firstPoint, TWGS84Point lastPoint, TWGS84Point currentPoint, bool isSegment)
    {
      double dist = ComputeCrossProduct(firstPoint, lastPoint, currentPoint) / ComputeDistance(firstPoint, lastPoint);

      if (isSegment)
      {
        double dot1 = ComputeDotProduct(firstPoint, lastPoint, currentPoint);

        if (dot1 > 0) return ComputeDistance(lastPoint, currentPoint);

        double dot2 = ComputeDotProduct(lastPoint, firstPoint, currentPoint);

        if (dot2 > 0) return ComputeDistance(firstPoint, currentPoint);
      }

      return Math.Abs(dist);
    }

    private static KeyInfo FindKey(TWGS84Point[] coords, int first, int last)
    {
      var keyInfo = new KeyInfo();

      for (var current = first + 1; current < last; current++)
      {
        double d2 = LineToPointDistance2D(coords[first], coords[last], coords[current], true);

        if (d2 < keyInfo.SquaredDistanceOfKeyToSegment) continue;

        // Update maximum squared distance and the point it belongs to
        keyInfo.CoordinateKeyIndex = current;
        keyInfo.SquaredDistanceOfKeyToSegment = d2;
      }

      return keyInfo;
    }

    private class KeyInfo
    {
      public int CoordinateKeyIndex;
      public double SquaredDistanceOfKeyToSegment;

      public KeyInfo(int coordinateKeyIndex = 0, double squaredDistanceOfKeyToSegment = 0)
      {
        CoordinateKeyIndex = coordinateKeyIndex;
        SquaredDistanceOfKeyToSegment = squaredDistanceOfKeyToSegment;
      }
    }

    private class SubPolyAlt : IComparable<SubPolyAlt>
    {
      public readonly int FirstPointCoordIndex;
      public readonly int LastPointCoordIndex;
      public KeyInfo subPolyKeyInfo;

      public SubPolyAlt(int firstPointCoordIndex = 0, int lastPointCoordIndex = 0)
      {
        FirstPointCoordIndex = firstPointCoordIndex;
        LastPointCoordIndex = lastPointCoordIndex;
      }

      public int CompareTo(SubPolyAlt other) => subPolyKeyInfo.SquaredDistanceOfKeyToSegment.CompareTo(other.subPolyKeyInfo.SquaredDistanceOfKeyToSegment);
    }
  }
}
