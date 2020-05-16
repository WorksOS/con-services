using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.MasterData.Repositories
{
  public static class RepositoryHelper
  {
    public static string WKTToSpatial(string geometryWKT)
    {
      return string.IsNullOrEmpty(geometryWKT) ? "null" : $"ST_GeomFromText('{geometryWKT}')";
    }

    public static string GetPolygonWKT(string boundary)
    {
      const string polygonStr = "POLYGON";
      var boundaryWkt = string.Empty;

      if (!string.IsNullOrEmpty(boundary))
      {
        // Check whether the ProjectBoundary is in WKT format. Convert to the WKT format if it is not. 
        if (!boundary.Contains(polygonStr))
        {
          boundary =
            boundary.Replace(",", " ").Replace(";", ",").TrimEnd(',');
          boundary =
            string.Concat(polygonStr + "((", boundary, "))");
        }
        //Polygon must start and end with the same point

        boundaryWkt = boundary.ParseGeometryData().ClosePolygonIfRequired()
          .ToPolygonWKT();
      }

      return boundaryWkt;
    }

    /// <summary>
    /// Map a 3dp project wkt boundary to the format required for cws Project API
    /// </summary>
    /// <param name="boundary"></param>
    /// <returns></returns>
    public static ProjectBoundary MapProjectBoundary(string boundary)
    {
      if (string.IsNullOrEmpty(boundary))
        return null;

      // Check whether the ProjectBoundary is in WKT format. Convert to the WKT format if it is not. 
      const string polygonStr = "POLYGON";
      if (!boundary.Contains(polygonStr))
        {
          boundary =
            boundary.Replace(",", " ").Replace(";", ",").TrimEnd(',');
          boundary =
            string.Concat(polygonStr + "((", boundary, "))");
        }
        //Polygon must start and end with the same point

        var boundaryWkt = boundary.ParseGeometryData().ClosePolygonIfRequired();
      var pointsAsDoubleList = new List<double[,]>();
      foreach (var point in boundaryWkt)
      {       
        pointsAsDoubleList.Add(item: new double[,] { { point.X, point.Y } });
      }

      var cwsProjectBoundary = new ProjectBoundary();
      cwsProjectBoundary.type = "Polygon";
      cwsProjectBoundary.coordinates = pointsAsDoubleList;     

      return cwsProjectBoundary;
    }
  }

  internal static class ExtensionString
  {
    private static readonly Dictionary<string, string> _replacements = new Dictionary<string, string>();

    static ExtensionString()
    {
      _replacements["LINESTRING"] = "";
      _replacements["CIRCLE"] = "";
      _replacements["POLYGON"] = "";
      _replacements["POINT"] = "";
      _replacements["("] = "";
      _replacements[")"] = "";
    }

    public static List<Point> ClosePolygonIfRequired(this List<Point> s)
    {
      if (Equals(s.First(), s.Last()))
        return s;
      s.Add(s.First());
      return s;
    }

    public static string ToPolygonWKT(this List<Point> s)
    {
      var internalString = s.Select(p => p.WKTSubstring).Aggregate((i, j) => $"{i},{j}");
      return $"POLYGON(({internalString}))";
    }

    public static List<Point> ParseGeometryData(this string s)
    {
      var points = new List<Point>();

      foreach (string to_replace in _replacements.Keys)
      {
        s = s.Replace(to_replace, _replacements[to_replace]);
      }

      string[] pointsArray = s.Split(',').Select(str => str.Trim()).ToArray();

      IEnumerable<string[]> coordinates;

      //gets x and y coordinates split by space, trims whitespace at pos 0, converts to double array
      coordinates = pointsArray.Select(point => point.Trim().Split(null)
        .Where(v => !string.IsNullOrWhiteSpace(v)).ToArray());
      points = coordinates.Select(p => new Point() { X = double.Parse(p[0]), Y = double.Parse(p[1]) }).ToList();

      return points;
    }
  }

  internal class Point
  {
    public double X;
    public double Y;
    public string WKTSubstring => $"{X} {Y}";

    public override bool Equals(object obj)
    {
      var source = (Point)obj;
      return (source.X == X) && (source.Y == Y);
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}
