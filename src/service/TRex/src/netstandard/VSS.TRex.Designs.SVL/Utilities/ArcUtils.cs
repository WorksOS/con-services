using System;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL.Utilities
{
  public static class ArcUtils
  {
    public static BoundingWorldExtent3D ArcBoundingRectangle(double x1, double y1, double x2, double y2, double cx, double cy,
      bool clockwise, bool clockwiseCoordSystem)
    {
      double radius = Math.Sqrt((x1 - cx) * (x1 - cx) + (y1 - cy) * (y1 - cy));
      double xa = x1;
      double xb = x1;
      double ya = y1;
      double yb = y1;
      if (x2 < x1) xa = x2;
      else xb = x2;
      if (y2 < y1) ya = y2;
      else yb = y2;

      if (PointOnArc(x1, y1, x2, y2, cx, cy, clockwise, clockwiseCoordSystem, cx + radius, cy))
       xb = cx + radius;
      if (PointOnArc(x1, y1, x2, y2, cx, cy, clockwise, clockwiseCoordSystem, cx - radius, cy))
       xa = cx - radius;
      if (PointOnArc(x1, y1, x2, y2, cx, cy, clockwise, clockwiseCoordSystem, cx, cy + radius))
       yb = cy + radius;
      if (PointOnArc(x1, y1, x2, y2, cx, cy, clockwise, clockwiseCoordSystem, cx, cy - radius))
       ya = cy - radius;

      return new BoundingWorldExtent3D(xa, ya, xb, yb);
    }

    public static bool PointOnArc(double x1, double y1, double x2, double y2, double cx, double cy,
      bool clockwise,
      bool ClockwiseCoordSystem,
      double int_x, double int_y)
    {
      // Assumes(x1, y1), (x2, y2) and(int_x, int_y) are all on the circle with centre(cx, cy) 

      bool greater(int h, double a, double b)
        //{ Assumes a & b are in the same(X) hemisphere and are Y values.
        //    Returns true if a is further round circle in direction given by
        //  'clockwise' }
      {
        return (((a > b) == (h != 0)) == (clockwise == ClockwiseCoordSystem)) || (a == b);
      }

      int hi = int_x < cx ? 1 : 0;
      int h1 = x1 < cx ? 1 : 0;
      int h2 = x2 < cx ? 1 : 0;

      if (h1 == h2)
      {
        if (hi == h1)
          return (greater(hi, int_y, y1) == greater(hi, int_y, y2)) == greater(h1, y1, y2);
        return greater(h1, y1, y2);
      }

      if (hi == h1)
        return greater(hi, int_y, y1);

      return greater(hi, y2, int_y);
    }

    public static double CalcIncludedAngle(double sx, double sy, double ex, double ey, double cx, double cy, bool ClockWise)
    {
      double anglediff(double angle2, double angle1)
        // Returns the anticlockwise angle from 'angle1' to 'angle2' 
      {
        if (angle1 > angle2)
          return 2 * Math.PI - angle1 + angle2;

        return angle2 - angle1;
      }

      var bearing1 = Math.Atan2(sy - cy, sx - cx);
      var bearing2 = Math.Atan2(ey - cy, ex - cx);
      GeometryUtils.CleanAngle(ref bearing1);
      GeometryUtils.CleanAngle(ref bearing2);
      var Result = anglediff(bearing1, bearing2);
      // returns clockwise ang 
      if (!ClockWise)
        Result = 2 * Math.PI - Result;
      else
        Result = -Result;

      return Result;
    }
  }
}
