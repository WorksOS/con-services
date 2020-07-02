using System.Collections.Generic;
using VSS.Common.Abstractions.Clients.CWS.Models;
using Xunit;

namespace CCSS.Geometry.UnitTests
{
  public class GeometryConversionTests
  {
    private const string GeometryWKT =
      "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154))";

    [Fact]
    public void PolygonWKTNewBoundaryClosed()
    {
      var wkt = GeometryConversion.GetPolygonWKT(GeometryWKT);
      Assert.Equal(GeometryWKT, wkt);
    }

    [Fact]
    public void PolygonWKTNewBoundaryNotClosed()
    {
      var boundary = GeometryWKT.Substring(0, GeometryWKT.LastIndexOf(',')) + "))";
      var wkt = GeometryConversion.GetPolygonWKT(boundary);
      Assert.Equal(GeometryWKT, wkt);
    }

    [Fact]
    public void MapWKTToCWSFormat()
    {
      var cwsCoordinates = new List<List<double[]>> { new List<double[]> { new[] { 150.3, 1.2 }, new[] { 150.4, 1.2 }, new[] { 150.4, 1.3 }, new[] { 150.4, 1.4 }, new[] { 150.3, 1.2 } } };
      var expectedProjectBoundary = new ProjectBoundary() { type = "Polygon", coordinates = cwsCoordinates };
      var wktPolygon = "POLYGON((150.3 1.2,150.4 1.2,150.4 1.3,150.4 1.4,150.3 1.2))";

      var cwsBoundary = GeometryConversion.MapProjectBoundary(wktPolygon);
      Assert.Equal(expectedProjectBoundary.type, cwsBoundary.type);
      Assert.Equal(expectedProjectBoundary.coordinates, cwsBoundary.coordinates);
    }

    [Fact]
    public void MapCWSFormatToWkt()
    {
      var cwsCoordinates = new List<List<double[]>> { new List<double[]> { new[] { 150.3, 1.2 }, new[] { 150.4, 1.2 }, new[] { 150.4, 1.3 }, new[] { 150.4, 1.4 }, new[] { 150.3, 1.2 } } };
      var ProjectBoundary = new ProjectBoundary() { type = "Polygon", coordinates = cwsCoordinates };
      var expectedWktPolygon = "POLYGON((150.3 1.2,150.4 1.2,150.4 1.3,150.4 1.4,150.3 1.2))";

      var wktBoundary = GeometryConversion.ProjectBoundaryToWKT(ProjectBoundary);
      Assert.Equal(expectedWktPolygon, wktBoundary);
    }
  }
}
