using System;
using System.Collections.Generic;
using System.Linq;
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
    public FencePoint this[int index]
    {
      get { return Points[index]; }
    }

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
    protected void InitialiseMaxMins()
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
      };
    }

    /// <summary>
    /// Determine if a given point (x, y) lies inside the boundary defined by the fence points
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IncludesPoint(double x, double y)
    {
      try
      {
        if ((x < MinX) || (x > MaxX) || (y < MinY) || (y > MaxY))
        {
          return false;
        }

        if (IsRectangle)
        {
          // The point lies in the known rectangular area, so is contained in the filter
          return true;
        }

        int pointsCount = Points.Count;

        if (pointsCount < 3)
        {
          return false;
        }

        //  Count the number of segments of the fence which intersect a line
        //through the test point with constant y, and whose x value is less
        //than the test x. 
        // Get the last point of the fence. }

        int crosses = 0;
        FencePoint pt1; 
        FencePoint pt2 = Points.Last();

        //  No intersections found yet 
        for (int i = 0; i < pointsCount; i++)
        {
          // Load the segment 
          pt1 = pt2;
          pt2 = Points[i];

          double pt1X = pt1.X;
          double pt1Y = pt1.Y;
          double pt2Y = pt2.Y;

          // Does the constant y line intersect the segment? 
          if ((((y >= pt1Y) && (y < pt2Y)) || ((y <= pt1Y) && (y > pt2Y)))
              // Test if the intersection is to the left of test_x 
              && ((pt1X + (pt2.X - pt1X) * (y - pt1Y) / (pt2Y - pt1Y)) < x))
          {
            // Did we cross through a point where both neighbour points are on the
            // same side of the line with constant height, if so DO NOT increment crosses
            if (y == pt1Y)
            {
              double pt3Y = Points[(pointsCount + i - 2) % pointsCount].Y;
              if (Math.Sign(y - pt2Y) == Math.Sign(y - pt3Y))
              {
                continue;
              }
            }

            if (y == pt2Y)
            {
              double pt3Y = Points[(i + 1) % pointsCount].Y;
              if ((Math.Sign(y - pt1Y) == Math.Sign(y - pt3Y)))
              {
                continue;
              }
            }

            // Everything seems to be OK, so say we crossed... 
            crosses++;
          }
        }

        // Point is included if the number of crosses is odd 
        return (crosses % 2) == 1;
      }
      catch (Exception)
      {
        // SIGLogMessage.PublishNoODS(Nil, Format('Maths error in IncludesPoint. X:%f Y:%f MinX:%f MaxY:%f MinY:%f MaxY:%f Error:%s', [X, Y, MinX, MaxX, MinY, MaxY, e.Message]), TSigLogMessageClass.slmcError);
        return false;
      }
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
      FencePoint pt1, pt2;

      if ((IncludesPoint(x1, y1) || IncludesPoint(x2, y2)))
      {
        return true;
      }

      int pointsCount = Points.Count;

      pt2 = Points[pointsCount - 1];
      for (int i = 0; i < pointsCount; i++)
      {
        pt1 = pt2;
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
      FencePoint pt1, pt2;

      int pointsCount = Points.Count;

      pt2 = Points[pointsCount - 1];
      for (int i = 0; i < pointsCount; i++)
      {
        pt1 = pt2;
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

      // Check for intersecting lines
      if (BoundaryIntersectsLine(extent.MinX, extent.MinY, extent.MinX, extent.MaxY) ||
          BoundaryIntersectsLine(extent.MinX, extent.MaxY, extent.MaxX, extent.MaxY) ||
          BoundaryIntersectsLine(extent.MaxX, extent.MaxY, extent.MaxX, extent.MinY) ||
          BoundaryIntersectsLine(extent.MaxX, extent.MinY, extent.MinX, extent.MinY))
      {
        return true;
      }

      // The fence does not include the square
      return false;
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
    /// Clear the fence to an initialised state
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
      {
        return 0;
      }

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
  }
}
