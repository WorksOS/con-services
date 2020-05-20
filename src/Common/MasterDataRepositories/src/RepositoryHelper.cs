using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.MasterData.Repositories
{
  public static class RepositoryHelper
  {
    private static string POLYGON = "POLYGON";

    public static string WKTToSpatial(string geometryWKT)
    {
      return string.IsNullOrEmpty(geometryWKT) ? "null" : $"ST_GeomFromText('{geometryWKT}')";
    }

    public static string GetPolygonWKT(string boundary)
    {
      return GetPoints(boundary)?.ToPolygonWKT();
    }

    /// <summary>
    /// Map a 3dp project wkt boundary to the format required for cws Project API
    /// </summary>
    public static ProjectBoundary MapProjectBoundary(string boundary)
    {
      var boundaryPoints = GetPoints(boundary);
      if (boundaryPoints == null)
        return null;

      var pointsAsDoubleList = new List<double[,]>();
      foreach (var point in boundaryPoints)
      {       
        pointsAsDoubleList.Add(item: new double[,] { { point.X, point.Y } });
      }

      var cwsProjectBoundary = new ProjectBoundary();
      cwsProjectBoundary.type = "Polygon";
      cwsProjectBoundary.coordinates = pointsAsDoubleList;     

      return cwsProjectBoundary;
    }

    private static List<Point> GetPoints(string boundary)
    {
      if (!string.IsNullOrEmpty(boundary))
      {
        // Check whether the ProjectBoundary is in WKT format. Convert to the WKT format if it is not. 
        if (!boundary.Contains(POLYGON))
        {
          boundary =
            boundary.Replace(",", " ").Replace(";", ",").TrimEnd(',');
          boundary =
            string.Concat(POLYGON + "((", boundary, "))");
        }
        //Polygon must start and end with the same point
        return boundary.ParseGeometryData().ClosePolygonIfRequired();
      }

      return null;
    }

    /// <summary>
    /// Maps a CWS project boundary to a 3dpm project WKT boundary
    /// </summary>
    public static string ProjectBoundaryToWKT(ProjectBoundary boundary)
    {
      //Should always be a boundary but just in case
      if (boundary == null || boundary.coordinates.Count == 0)
        return null;

      // CWS boundary is always closed ?
      return boundary.coordinates.ToPolygonWKT();
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

    public static string ToPolygonWKT(this List<double[,]> list)
    {
      // Always just a single 2D array in the list which is the CWS polygon coordinates
      var coords = list[0];
      var rowCount = coords.GetLength(0);
      var wktCoords = new List<string>();
      for (var i = 0; i < rowCount; i++)
      {
        wktCoords.Add($"{coords[i, 0]} {coords[i, 1]}");
      }
     
      var internalString = wktCoords.Aggregate((i, j) => $"{i},{j}");
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
