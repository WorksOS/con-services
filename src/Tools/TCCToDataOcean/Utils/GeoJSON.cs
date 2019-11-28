using System;
using System.Collections.Generic;
using System.Linq;

namespace TCCToDataOcean.Utils
{
  public static class GeoJSON
  {
    public static double Area(string wktString, int precision = 15)
    {
      var points = ConvertWKTToPoints(wktString);

      points.Add(points[0]);

      var result = Math.Abs(points.Take(points.Count - 1)
                                  .Select((p, i) => (points[i + 1].X - p.X) * (points[i + 1].Y + p.Y))
                                  .Sum() / 2);

      return Math.Round(result, precision);
    }

    private static List<Point> ConvertWKTToPoints(string wktString)
    {
      var result = new List<Point>();

      var points = wktString.Substring(9)
                            .TrimEnd(')')
                            .Split(',');

      foreach (var pointStr in points)
      {
        var tmp = pointStr.Split(' ');
		
        result.Add(new Point(float.Parse(tmp[0]), float.Parse(tmp[1])));

      }
	
      return result;
    }

    internal struct Point
    {
      public float X;
      public float Y;

      public Point(float x, float y)
      {
        X = x;
        Y = y;
      }
    }
  }
}
