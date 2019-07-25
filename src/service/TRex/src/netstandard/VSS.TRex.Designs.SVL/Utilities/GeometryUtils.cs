using System;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Designs.SVL.Utilities
{
  public static class GeometryUtils
  {
    public static bool BetweenAngle(double x1, double x2, double x3)
    {
      const double tiny_angle = 0.000001;

      if (x1 > x3) //{ set order }
        MinMax.Swap(ref x1, ref x3);

      var Res = (x1 - tiny_angle <= x2) && (x2 <= x3 + tiny_angle);
      //  if angle includes vertical(0/2*pi) - check small/big angles too

      if (!Res && x3 > 2 * Math.PI)
      {
        x3 = x3 - 2 * Math.PI;
        x1 = x1 - 2 * Math.PI;
        Res = x1 - tiny_angle <= x2 && x2 <= x3 + tiny_angle;
      }
      else if (!Res && x1 < 0)
      {
        x3 = x3 + 2 * Math.PI;
        x1 = x1 + 2 * Math.PI;
        Res = x1 - tiny_angle <= x2 && x2 <= x3 + tiny_angle;
      }

      return Res;
    }

    public static void clean_angle(ref double radians)
    {
      //  Returns angle in interval[0, 2pi) 
      while (radians < 0)
        radians += 2 * Math.PI;
      while (radians >= 2 * Math.PI)
        radians -= 2 * Math.PI;
    }

    public static double DistToLine(double x, double y, double x1, double y1, double x2, double y2,
      double AspectRatio = 1.0)
    {
      double intersect_x, intersect_y;
      double a, b, c, p;

      //  length of line squared 
      c = (y2 - y1) * (y2 - y1) + (x2 - x1) * (x2 - x1);

      if (c == 0)
        //  line is only a point so easy from here on 
        p = 0;
      else
      {
        // length of point to both ends of the line 
        a = (y - y2) * (y - y2) + (x - x2) * (x - x2);
        b = (y - y1) * (y - y1) + (x - x1) * (x - x1);

        // calculate the intersection ratio along the base line 
        p = 0.5 * (1 + (b - a) / c);
      }

      if (p <= 0)
      {
        //  intersection is outside the line segment on the side of x1 
        intersect_x = x1;
        intersect_y = y1;
      }
      else if (p >= 1)
      {
        // intersection is outside the line segment on the side of x2 
        intersect_x = x2;
        intersect_y = y2;
      }
      else
      {
        //  find the perpendicular intersection point
        intersect_x = x1 + p * (x2 - x1);
        intersect_y = y1 + p * (y2 - y1);
      }

      return MathUtilities.Hypot((intersect_y - y) / AspectRatio, intersect_x - x);
    }

    public static double Distance(double x0, double y0, double x1, double y1) => MathUtilities.Hypot(x1 - x0, y1 - y0);

    public static void LineClosestPoint(double ptx, double pty, double x0, double y0, double x1, double y1,
      out double x, out double y, out double Chainage, out double Offset)
    {
      double Dist0p = Distance(x0, y0, ptx, pty);
      double Dist1p = Distance(x1, y1, ptx, pty);
      double Dist01 = Distance(x0, y0, x1, y1);
      if (Dist01 < 1e-8)
      {
        x = x0;
        y = y0;
        Chainage = 0;
        Offset = Dist0p;
        return;
      }

      if (Dist0p < 1e-8)
        Chainage = 0;
      else
      {
        double Cos1 = (Dist0p * Dist0p + Dist01 * Dist01 - Dist1p * Dist1p)
                      / (2 * Dist0p * Dist01);
        Chainage = Dist0p * Cos1;
      }

      x = x0 + Chainage / Dist01 * (x1 - x0);
      y = y0 + Chainage / Dist01 * (y1 - y0);
      Offset = Distance(x, y, ptx, pty);

      // Offsets to the left are negative }
      if (((y1 - y0) * (ptx - x0) + (x0 - x1) * (pty - y0)) < 0)
        Offset = -Offset;
    }
  }
}
