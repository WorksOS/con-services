using System.Collections.Generic;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace CCSS.Geometry
{
  public static class GeometryConversion
  {
    private static string POLYGON = "Polygon";

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

      var pointsAsDoubleList = new List<double[]>(boundaryPoints.Count);
      for (var i = 0; i < boundaryPoints.Count; i++)
        pointsAsDoubleList.Add(item: new[] { boundaryPoints[i].X, boundaryPoints[i].Y });

      return new ProjectBoundary
      {
        type = POLYGON,
        coordinates = new List<List<double[]>> { pointsAsDoubleList }
      };
    }

    private static List<Point> GetPoints(string boundary)
    {
      //Polygon must start and end with the same point
      return string.IsNullOrEmpty(boundary) ? null : boundary.ParseGeometryData().ClosePolygonIfRequired();
    }

    /// <summary>
    /// Maps a CWS project boundary to a project WKT boundary
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
}
