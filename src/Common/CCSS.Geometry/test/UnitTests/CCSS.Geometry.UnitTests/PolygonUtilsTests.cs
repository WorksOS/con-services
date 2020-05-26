using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CCSS.Geometry.UnitTests
{
  public class PolygonUtilsTests
  {
    private const string _validBoundary = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965,172.595831670724 -43.5427038560109))";
    private const string _invalidBoundary_FewPoints = "POLYGON((172.595831670724 -43.5427038560109))";

    #region point in polygon
    [Fact]
    public void PointInPolygon_Inside()
    {
      var result = PolygonUtils.PointInPolygon(_validBoundary, -43.5427, 172.5946);
      Assert.True(result);
    }

    [Fact]
    public void PointInPolygon_Outside()
    {
      var result = PolygonUtils.PointInPolygon(_validBoundary, -43.544, 172.596);
      Assert.False(result);
    }

    [Fact]
    public void PointInPolygon_OnEdge()
    {
      // point is half way between first and second vertex
      var result = PolygonUtils.PointInPolygon(_validBoundary, -43.5432948958441, 172.5952308559065);
      Assert.True(result);
    }

    [Fact]
    public void PointInPolygon_AtVertex()
    {
      // point is at second vertex
      var result = PolygonUtils.PointInPolygon(_validBoundary, -43.5438859356773, 172.594630041089);
      Assert.True(result);
    }

    [Fact]
    public void PointInPolygon_InvalidPolygon()
    {
      Assert.Throws<InvalidOperationException>(() => PolygonUtils.PointInPolygon(_invalidBoundary_FewPoints, -43.5, 172.6));
    }
    #endregion

    #region self intersecting polygon

    [Fact]
    public void SelfIntersectingPolygon_NoPolygon()
    {
      Assert.Throws<InvalidOperationException>(() => PolygonUtils.SelfIntersectingPolygon(null));
    }

    [Fact]
    public void SelfIntersectingPolygon_EmptyPolygon()
    {
      Assert.Throws<InvalidOperationException>(() => PolygonUtils.SelfIntersectingPolygon(string.Empty));
    }

    [Fact]
    public void SelfIntersectingPolygon_SelfIntersecting()
    {
      var result = PolygonUtils.SelfIntersectingPolygon("POLYGON((1 2,7 5,1 6, 5 2))");
      Assert.True(result);
    }

    [Fact]
    public void SelfIntersectingPolygon_HappyPath()
    {
      var result = PolygonUtils.SelfIntersectingPolygon(_validBoundary);
      Assert.False(result);
    }
    #endregion

    #region overlapping polygon
    [Fact]
    public void OverlappingPolygons_MissingPolygon()
    {
      Assert.Throws<InvalidOperationException>(() => PolygonUtils.OverlappingPolygons(_validBoundary, null));
    }

    [Fact]
    public void OverlappingPolygons_NotPolygon()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var testBoundary = "LINESTRING(30 10, 10 30, 40 40)";

      var result = PolygonUtils.OverlappingPolygons(projectBoundary, testBoundary);
      Assert.False(result);
    }

    [Fact]
    public void OverlappingPolygons_InvalidPolygon()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var testBoundary = "POLYGON((1 3,3 2,1 1,3 0,1 0,1 3))";
      var result = PolygonUtils.OverlappingPolygons(projectBoundary, testBoundary);
      Assert.False(result);
    }

    [Fact]
    public void OverlappingPolygons_Inside()
    {
      // test polygon is completely inside project polygon
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var testBoundary = "POLYGON((175 15, 185 15, 185 35, 175 35, 175 15))";
      var result = PolygonUtils.OverlappingPolygons(projectBoundary, testBoundary);
      Assert.True(result);
    }

    [Fact]
    public void OverlappingPolygons_Outside()
    {
      // test polygon is completely outside project polygon
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var testBoundary = "POLYGON((200 10, 202 10, 202 20, 200 20, 200 10))";
      var result = PolygonUtils.OverlappingPolygons(projectBoundary, testBoundary);
      Assert.False(result);
    }

    [Fact]
    public void OverlappingPolygons_Vertex()
    {
      // test polygon touches project polygon at a vertex
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var testBoundary = "POLYGON((200 10, 202 10, 202 20, 190 20, 200 10))";
      //var result = PolygonUtils.OverlappingPolygons(projectBoundary, testBoundary);
      var result = PolygonUtils.OverlappingPolygons(testBoundary, projectBoundary);
      Assert.True(result);
    }

    [Fact]
    public void OverlappingPolygons_Edge()
    {
      // test polygon touches project polygon at an edge
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var testBoundary = "POLYGON((190 10, 202 10, 202 20, 190 20, 190 10))";
      var result = PolygonUtils.OverlappingPolygons(projectBoundary, testBoundary);
      Assert.True(result);
    }

    [Fact]
    public void OverlappingPolygons_Overlapping()
    {
      // test polygon is completely overlapping project polygon
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var testBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var result = PolygonUtils.OverlappingPolygons(projectBoundary, testBoundary);
      Assert.True(result);
    }

    [Fact]
    public void OverlappingPolygons_Intersecting()
    {
      // test polygon is intersecting project polygon
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var testBoundary = "POLYGON((180 20, 210 20, 210 50, 180 50, 180 20))";
      var result = PolygonUtils.OverlappingPolygons(projectBoundary, testBoundary);
      Assert.True(result);
    }

    #endregion
  }
}
