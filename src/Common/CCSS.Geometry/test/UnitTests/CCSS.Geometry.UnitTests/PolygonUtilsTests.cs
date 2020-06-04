using System;
using Xunit;

namespace CCSS.Geometry.UnitTests
{
  public class PolygonUtilsTests
  {
    #region point in polygon
    [Fact]
    public void PointInPolygon_Inside()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var result = PolygonUtils.PointInPolygon(projectBoundary, 15, 180);
      Assert.True(result);
    }

    [Fact]
    public void PointInPolygon_Outside()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var result = PolygonUtils.PointInPolygon(projectBoundary, 50, 180);
      Assert.False(result);
    }

    [Fact]
    public void PointInPolygon_OnEdge()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var result = PolygonUtils.PointInPolygon(projectBoundary, 20, 190);
      Assert.True(result);
    }

    [Fact]
    public void PointInPolygon_AtVertex()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var result = PolygonUtils.PointInPolygon(projectBoundary, 40, 170);
      Assert.True(result);
    }

    [Fact]
    public void PointInPolygon_InvalidPolygon()
    {
      var projectBoundary = "POLYGON((170 10, 190 10))";
      Assert.Throws<InvalidOperationException>(() => PolygonUtils.PointInPolygon(projectBoundary, 15, 180));
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
      var result = PolygonUtils.SelfIntersectingPolygon("POLYGON((10 20,70 50,10 60, 50 20))");
      Assert.True(result);
    }

    [Fact]
    public void SelfIntersectingPolygon_HappyPath()
    {
      // Not self intersecting
      var result = PolygonUtils.SelfIntersectingPolygon("POLYGON((10 20,10 60,70 50, 50 20))");
      Assert.False(result);
    }
    #endregion

    #region overlapping polygon
    [Fact]
    public void OverlappingPolygons_MissingPolygon()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      Assert.Throws<InvalidOperationException>(() => PolygonUtils.OverlappingPolygons(projectBoundary, null));
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
      // test polygon touches project polygon at a point
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var testBoundary = "POLYGON((200 10, 202 10, 202 20, 190 20, 200 10))";
      var result = PolygonUtils.OverlappingPolygons(projectBoundary, testBoundary);
      Assert.True(result);
    }

    [Fact]
    public void OverlappingPolygons_Edge()
    {
      // test polygon touches project polygon along an edge
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
