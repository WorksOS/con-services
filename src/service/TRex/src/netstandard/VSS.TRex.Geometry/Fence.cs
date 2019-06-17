using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using VSS.TRex.Common;

namespace VSS.TRex.Geometry
{
  /// <summary>
  /// A simple polygon describing a fence and including tests for different geometry elements
  /// </summary>
  public class Fence
  {
    /// <summary>
    /// No-arg constructor. Created a fence with no vertices
    /// </summary>
    public Fence()
    {
      Initialise();
    }

    /// <summary>
    /// Constructor that creates a rectangular fence given the min/max x/y points
    /// </summary>
    /// <param name="MinX"></param>
    /// <param name="MinY"></param>
    /// <param name="MaxX"></param>
    /// <param name="MaxY"></param>
    public Fence(double MinX, double MinY, double MaxX, double MaxY) : this()
    {
      SetExtents(MinX, MinY, MaxX, MaxY);
    }

    /// <summary>
    /// Constructor that creates a rectangular fence from a world coordinate bounding extent
    /// </summary>
    /// <param name="extent"></param>
    public Fence(BoundingWorldExtent3D extent) : this(extent.MinX, extent.MinY, extent.MaxX, extent.MaxY)
    {
    }

    /// <summary>
    /// Default indexer for the list of fence points
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public FencePoint this[int index] => Points[index];     

    /// <summary>
    /// The list of the points taking part in the fence
    /// </summary>
    public List<FencePoint> Points = new List<FencePoint>();

    /// <summary>
    /// Determine if any of the vertices in the Fence are null
    /// </summary>
    /// <returns></returns>
    public bool IsNull()
    {
      if (Points.Count == 0)
      {
        return true;
      }

      foreach (FencePoint fp in Points)
      {
        if (fp.X == Consts.NullDouble || fp.Y == Consts.NullDouble)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Minimum X ordinate for all points in the fence
    /// </summary>
    public double MinX { get; private set; }

    /// <summary>
    /// Maximum X ordinate for all points in the fence
    /// </summary>
    public double MaxX { get; private set; }

    /// <summary>
    /// Minimum Y ordinate for all points in the fence
    /// </summary>
    public double MinY { get; private set; }


    /// <summary>
    /// Maximum Y ordinate for all points in the fence
    /// </summary>
    public double MaxY { get; private set; }


    /// <summary>
    /// Is the fence intrinsically a rectangle?
    /// </summary>
    public bool IsRectangle { get; set; }

    /// <summary>
    /// Set the min/max x/y values to inverted (invalid) values
    /// </summary>
    private void InitialiseMaxMins()
    {
      MinX = 1E10;
      MinY = 1E10;
      MaxX = -1E10;
      MaxY = -1E10;
    }

    /// <summary>
    /// Update the local max/min x/y bounding box for all the points in the fence
    /// </summary>
    protected void UpdateMaxMins()
    {
      InitialiseMaxMins();

      foreach(var pt in Points)
      {
        if (pt.X < MinX) MinX = pt.X;
        if (pt.Y < MinY) MinY = pt.Y;
        if (pt.X > MaxX) MaxX = pt.X;
        if (pt.Y > MaxY) MaxY = pt.Y;
      }
    }

    /// <summary>
    /// Determine if a given point (x, y) lies inside the boundary defined by the fence points
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IncludesPoint(double x, double y)
    {
      if (x < MinX || x > MaxX || y < MinY || y > MaxY)
        return false;

      if (IsRectangle) // The point lies in the known rectangular area, so is contained in the filter
        return true;

      var points_Count = Points.Count;

      if (points_Count < 3)
        return false;

      bool result = false;
      var pt1 = Points[points_Count - 1];

      for (int i = 0; i < points_Count; i++)
      {
        var pt2 = Points[i];
        if (pt2.X == x && pt2.Y == y)
          return true;

        if (pt2.Y == pt1.Y && y == pt1.Y && pt1.X <= x && x <= pt2.X)
          return true;

        if ((pt2.Y < y) && (pt1.Y >= y) || (pt1.Y < y) && (pt2.Y >= y))
        {
          if (pt2.X + (y - pt2.Y) / (pt1.Y - pt2.Y) * (pt1.X - pt2.X) <= x)
            result = !result;
        }
        pt1 = pt2;
      }
      return result;     
    }

    /// <summary>
    /// Determine if the fence includes the given line
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    /// <returns></returns>
    public bool IncludesLine(double x1, double y1, double x2, double y2)
    {
      if (IncludesPoint(x1, y1) || IncludesPoint(x2, y2))
      {
        return true;
      }

      int pointsCount = Points.Count;

      var pt2 = Points[pointsCount - 1];
      for (int i = 0; i < pointsCount; i++)
      {
        var pt1 = pt2;
        pt2 = Points[i];

        if (LineIntersection.LinesIntersect(x1, y1, x2, y2,
          pt1.X, pt1.Y, pt2.X, pt2.Y,
          out _, out _, true, out _))
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Determine if the fence strictly intersects with the given line
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    /// <returns></returns>
    public bool BoundaryIntersectsLine(double x1, double y1, double x2, double y2)
    {
      int pointsCount = Points.Count;

      var pt2 = Points[pointsCount - 1];
      for (int i = 0; i < pointsCount; i++)
      {
        var pt1 = pt2;
        pt2 = Points[i];

        if (LineIntersection.LinesIntersect(x1, y1, x2, y2,
          pt1.X, pt1.Y, pt2.X, pt2.Y,
          out _, out _, true, out _))
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Determines if the fence intersects a supplied world coordinate bounding extent
    /// </summary>
    /// <param name="extent"></param>
    /// <returns></returns>
    public bool IntersectsExtent(BoundingWorldExtent3D extent)
    {
      // Check extent vertex inclusion in the fence
      if (IncludesPoint(extent.MinX, extent.MinY) ||
          IncludesPoint(extent.MinX, extent.MaxY) ||
          IncludesPoint(extent.MaxX, extent.MinY) ||
          IncludesPoint(extent.MaxX, extent.MaxY))
      {
        return true;
      }

      // Check fence vertex inclusion in Extents
      foreach (FencePoint pt in Points)
      {
        if (extent.Includes(pt.X, pt.Y))
        {
          return true;
        }
      }

      // Check for intersecting lines
      if (IncludesLine(extent.MinX, extent.MinY, extent.MinX, extent.MaxY) ||
          IncludesLine(extent.MinX, extent.MaxY, extent.MaxX, extent.MaxY) ||
          IncludesLine(extent.MaxX, extent.MaxY, extent.MaxX, extent.MinY) ||
          IncludesLine(extent.MaxX, extent.MinY, extent.MinX, extent.MinY))
      {
        return true;
      }

      // The fence and the square do not intersect
      return false;
    }

    /// <summary>
    /// Determines if the fence includes a supplied world coordinate bounding extent
    /// </summary>
    /// <param name="extent"></param>
    /// <returns></returns>
    public bool IncludesExtent(BoundingWorldExtent3D extent)
    {
      // Check extent vertex inclusion in the fence
      if (!(IncludesPoint(extent.MinX, extent.MinY) &&
            IncludesPoint(extent.MinX, extent.MaxY) &&
            IncludesPoint(extent.MaxX, extent.MinY) &&
            IncludesPoint(extent.MaxX, extent.MaxY)))
      {
        return false;
      }

      if (IsRectangle || IsSquare)
      {
        // No Further checks are necessary, it is included
        return true;
      }

      // Check fence vertex inclusion in Extents
      foreach (FencePoint pt in Points)
      {
        if (extent.Includes(pt.X, pt.Y))
        {
          // There must be some area of the extent that does not reside in the filter
          return false;
        }
      }

      // Check for intersecting lines. Intersection means at least part of the given extent must lie outside the fence
      if (BoundaryIntersectsLine(extent.MinX, extent.MinY, extent.MinX, extent.MaxY) ||
          BoundaryIntersectsLine(extent.MinX, extent.MaxY, extent.MaxX, extent.MaxY) ||
          BoundaryIntersectsLine(extent.MaxX, extent.MaxY, extent.MaxX, extent.MinY) ||
          BoundaryIntersectsLine(extent.MaxX, extent.MinY, extent.MinX, extent.MinY))
      {
        return false;
      }

      // The fence must include the square
      return true;
    }

    /// <summary>
    /// Initialise all elements of the Fence
    /// </summary>
    public void Initialise()
    {
      IsRectangle = false;
      Points.Clear();
      InitialiseMaxMins();
    }

    /// <summary>
    /// Clear the fence to an initialized state
    /// </summary>
    public void Clear()
    {
      Initialise();
    }

    /// <summary>
    /// Determine if the shape of the fence is a square
    /// </summary>
    /// <returns></returns>
    public bool IsSquare => IsRectangle && (Math.Abs((MaxX - MinX) - (MaxY - MinY)) < 0.0001);

    /// <summary>
    /// Retrieve the bounding extents of the fence previously calculate with UpdateMaxMins()
    /// </summary>
    /// <param name="AMinX"></param>
    /// <param name="AMinY"></param>
    /// <param name="AMaxX"></param>
    /// <param name="AMaxY"></param>
    public void GetExtents(out double AMinX, out double AMinY, out double AMaxX, out double AMaxY)
    {
      AMinX = MinX;
      AMinY = MinY;
      AMaxX = MaxX;
      AMaxY = MaxY;
    }

    /// <summary>
    /// Create a rectangle fence from the min/max x/y points
    /// </summary>
    /// <param name="AMinX"></param>
    /// <param name="AMinY"></param>
    /// <param name="AMaxX"></param>
    /// <param name="AMaxY"></param>
    public void SetExtents(double AMinX, double AMinY, double AMaxX, double AMaxY)
    {
      Clear();

      Points.Add(new FencePoint(AMinX, AMinY));
      Points.Add(new FencePoint(AMinX, AMaxY));
      Points.Add(new FencePoint(AMaxX, AMaxY));
      Points.Add(new FencePoint(AMaxX, AMinY));

      UpdateExtents();

      IsRectangle = true;
    }

    /// <summary>
    /// Determine if there are any vertices in the Fence
    /// </summary>
    /// <returns></returns>
    public bool HasVertices => Points.Count > 0;

    /// <summary>
    /// Return the number of vertices in the fence
    /// </summary>
    public int NumVertices => Points.Count;

    /// <summary>
    /// Calculate the ares in square meters encompassed by the Fence
    /// </summary>
    /// <returns></returns>
    public double Area()
    {
      if (Points.Count == 0)
        return 0;

      // Calc the area by summing the trapeziums to a base line
      double BaseY = Points.Last().Y;
      double LastX = Points.Last().X;
      double LastY = Points.Last().Y - BaseY;
      double result = 0.0;

      foreach (FencePoint pt in Points)
      {
        double X = pt.X;
        double Y = pt.Y - BaseY;

        result += (LastY + Y) / 2.0 * (X - LastX);

        LastX = X;
        LastY = Y;
      }

      return Math.Abs(result);
    }

    /// <summary>
    /// Force an update of the min/max x/y values for the fence
    /// </summary>
    public void UpdateExtents() => UpdateMaxMins();

    /// <summary>
    /// Assigned (copies) the vertices from another fence to this fence
    /// </summary>
    /// <param name="source"></param>
    public void Assign(Fence source)
    {
      Points = source.Points.Select(pt => new FencePoint(pt)).ToList();
    }

    /// <summary>
    /// Clears all vertices in the fence and replaces them with a rectangle
    ///  of points as per the two coordinates given. The coordinates may be any
    /// two diagonally opposite corners of the rectangle.
    /// </summary>
    /// <param name="X1"></param>
    /// <param name="Y1"></param>
    /// <param name="X2"></param>
    /// <param name="Y2"></param>
    public void SetRectangleFence(double X1, double Y1, double X2, double Y2)
    {
      SetExtents(Math.Min(X1, X2), Math.Min(Y1, Y2), Math.Max(X1, X2), Math.Max(Y1, Y2));
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(Points.Count);

      foreach (var point in Points)
        point.Write(writer);
    }

    public void Read(BinaryReader reader)
    {
      var pointsCount = reader.ReadInt32();

      for (var i = 0; i < pointsCount; i++)
      {
        var point = new FencePoint();
        point.Read(reader);

        Points.Add(point);
      }
    }

    /// <summary>
    /// Compress the line chain to the specified tolerance using the Douglas and Peucker algorithm - always include one interior point
    /// </summary>
    /// <param name="tolerance"></param>
    public void Compress(double tolerance)
    {
      const double TOLERANCE_MIN = 0.0;
      const int POLYGON_POINTS_MIN = 3;

      if (tolerance < TOLERANCE_MIN || Points.Count < POLYGON_POINTS_MIN)
        return;

      var first = true;
      var includePoints = new bool[Points.Count];
      includePoints[0] = true;
      includePoints[Points.Count - 1] = true;

      var toleranceSquared = Math.Sqrt(tolerance);

      for (var i = 1; i < Points.Count - 2; i++)
        includePoints[i] = false;

      CompressLine(0, Points.Count - 1);

      for (var i = Points.Count - 1; i >= 0; i--)
      {
        if (!includePoints[i])
          Points[i] = null;
      }

      Points.RemoveAll(p => p == null);

      #region Local functions
      //===================================================================
      // Compress the line segment from point i to point j.
      void CompressLine(int i, int j)
      {
        if (i - j == 1)
          return;

        FindFurthest(i, j, out var maxDistanceSquared, out var furthest);

        if (maxDistanceSquared > toleranceSquared || first)
        {
          first = false;
          includePoints[furthest] = true;
          CompressLine(i, furthest);
          CompressLine(furthest, j);
        }
      }
      //===================================================================
      #endregion
    }

    /// <summary>
    /// Find the perpendicularly furthest point from the partial line chain from i to j and return the square of the distance and the index.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="d2Max"></param>
    /// <param name="index"></param>
    private void FindFurthest(int i, int j, out double d2Max, out int index)
    {
      d2Max = -1.0;
      index = -1;

      for (var k = i + 1; k <= j - 1; k++)
      {
        var d2 = TDistance2(Points[k], Points[i], Points[j]);
        if (d2 > d2Max)
        {
          d2Max = d2;
          index = k;
        }
      }
    }

    /// <summary>
    /// Returns the square of the perpendicular distance of pt1 from the straight line connecting pt2 and pt3
    /// </summary>
    /// <param name="pt1"></param>
    /// <param name="pt2"></param>
    /// <param name="pt3"></param>
    /// <returns></returns>
    private double TDistance2(FencePoint pt1, FencePoint pt2, FencePoint pt3)
    {
      var d1x = pt3.X - pt2.X;
      var d1y = pt3.Y - pt2.Y;
      var d2x = pt1.X - pt3.X;
      var d2y = pt1.Y - pt3.Y;
      var d3x = pt2.X - pt1.X;
      var d3y = pt2.Y - pt1.Y;
      var s1 = d1x * d1x + d1y * d1y;
      var s2 = d2x * d2x + d2y * d2y;
      var s3 = d3x * d3x + d3y * d3y;

      if (s1 + s2 <= s3)
        return s2;

      if (s1 + s3 == s2)
        return s3;

      var d = d1y * d2x - d1x * d2y;
      return d * d / s1;
    }

    /// <summary>
    /// Based on the Area calucation.
    /// </summary>
    /// <returns></returns>
    public bool IsWindingClockwise()
    {
      const double INITIAL_AREA = 0.0;

      if (Points.Count == 0)
        return false;

      // Calc the area by suming the trapeziums to a base line...
      var lastPoint = Points[Points.Count - 1];  // The last point...
      var baseY = lastPoint.Y;

      var lastX = lastPoint.X;
      var lastY = lastPoint.Y - baseY;

      var area = INITIAL_AREA;

      for (var i = 0; i < Points.Count; i++)
      {
        var x = Points[i].X;
        var y = Points[i].Y - baseY;

        area += (lastY + y) / 2.0 * (x - lastX);

        lastX = x;
        lastY = y;
      }

      return area >= INITIAL_AREA;
    }
  }
}
