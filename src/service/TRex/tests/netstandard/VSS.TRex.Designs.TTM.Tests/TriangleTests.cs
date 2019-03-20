using System.IO;
using FluentAssertions;
using VSS.TRex.Geometry;
using Xunit;

namespace VSS.TRex.Designs.TTM.Tests
{
  public class TriangleTests
  {
    [Fact]
    public void TriangleS_Creation()
    {
      Triangles triangles = new Triangles();

      Assert.NotNull(triangles);
    }

    [Fact]
    public void Triangles_Pack()
    {
      Triangles triangles = new Triangles();

      triangles.AddTriangle(new TriVertex(0, 0, 0), new TriVertex(100, 0, 0), new TriVertex(0, 100, 0));
      triangles.AddTriangle(new TriVertex(0, 100, 5), new TriVertex(100, 100, 5), new TriVertex(100, 0, 5));

      Assert.True(triangles.Count == 2);
      triangles[0] = null;

      triangles.Pack();
      Assert.True(triangles.Count == 1);
      Assert.True(triangles[0].Vertices[0].X == 0.0);
      Assert.True(triangles[0].Vertices[0].Y == 100.0);
      Assert.True(triangles[0].Vertices[0].Z == 5.0);
      Assert.True(triangles[0].Vertices[1].X == 100.0);
      Assert.True(triangles[0].Vertices[1].Y == 100.0);
      Assert.True(triangles[0].Vertices[1].Z == 5.0);
      Assert.True(triangles[0].Vertices[2].X == 100.0);
      Assert.True(triangles[0].Vertices[2].Y == 0.0);
      Assert.True(triangles[0].Vertices[2].Z == 5.0);
    }

    [Fact]
    public void Triangle_GetSideIndex_Clockwise()
    {
      // Three clock wise coordinates
      var v1 = new TriVertex(0, 0, 0);
      var v2 = new TriVertex(0, 1, 0);
      var v3 = new TriVertex(1, 1, 0);

      var tri = new Triangle(v1, v2, v3);

      tri.GetSideIndex(v1, v2).Should().Be(0);
      tri.GetSideIndex(v2, v3).Should().Be(1);
      tri.GetSideIndex(v3, v1).Should().Be(2);
    }

    [Fact]
    public void Triangle_GetSideIndex_AntiClockwise()
    {
      // Three clock wise coordinates
      var v1 = new TriVertex(0, 0, 0);
      var v2 = new TriVertex(0, 1, 0);
      var v3 = new TriVertex(1, 1, 0);

      var tri = new Triangle(v3, v2, v1);

      tri.GetSideIndex(v1, v2).Should().Be(1);
      tri.GetSideIndex(v2, v3).Should().Be(0);
      tri.GetSideIndex(v3, v1).Should().Be(2);
    }

    [Fact]
    public void Triangle_Clockwise()
    {
      // Three clock wise coordinates
      var v1 = new TriVertex(0, 0, 0);
      var v2 = new TriVertex(0, 1, 0);
      var v3 = new TriVertex(1, 1, 0);

      var tri = new Triangle(v1, v2, v3);
      tri.IsClockwise().Should().BeTrue();

      var tri2 = new Triangle(v3, v2, v1);
      tri2.IsClockwise().Should().BeFalse();
    }

    [Fact]
    public void Triangle_Area()
    {
      // Three clock wise coordinates
      var v1 = new TriVertex(0, 0, 0);
      var v2 = new TriVertex(0, 1, 0);
      var v3 = new TriVertex(1, 1, 0);

      var tri = new Triangle(v1, v2, v3);
      tri.Area().Should().Be(0.5);
    }

    [Fact]
    public void Triangle_Centroid()
    {
      var v1 = new TriVertex(0, 0, 0);
      var v2 = new TriVertex(0, 1, 0);
      var v3 = new TriVertex(1, 1, 0);

      var tri = new Triangle(v1, v2, v3);

      tri.Centroid().Should().BeEquivalentTo(new XYZ((v1.X + v2.X + v3.X) / 3,
        (v1.Y + v2.Y + v3.Y) / 3,
        (v1.Z + v2.Z + v3.Z) / 3));
    }

    [Fact]
    public void Triangle_PointInTriangle()
    {
      var v1 = new TriVertex(0, 0, 0);
      var v2 = new TriVertex(0, 1, 0);
      var v3 = new TriVertex(1, 1, 0);

      var tri = new Triangle(v1, v2, v3);

      tri.PointInTriangle(0.5, 0.5).Should().Be(XYZ.PointInTriangle(v1.XYZ, v2.XYZ, v3.XYZ, 0.5, 0.5));
      tri.PointInTriangle(0, 0).Should().Be(XYZ.PointInTriangle(v1.XYZ, v2.XYZ, v3.XYZ, 0, 0));
      tri.PointInTriangle(0, 1.0).Should().Be(XYZ.PointInTriangle(v1.XYZ, v2.XYZ, v3.XYZ, 0, 1.0));
      tri.PointInTriangle(1.0, 0).Should().Be(XYZ.PointInTriangle(v1.XYZ, v2.XYZ, v3.XYZ, 1.0, 0));
    }

    [Fact]
    public void Triangle_PointInTriangleInclusive()
    {
      var v1 = new TriVertex(0, 0, 0);
      var v2 = new TriVertex(0, 1, 0);
      var v3 = new TriVertex(1, 1, 0);

      var tri = new Triangle(v1, v2, v3);

      tri.PointInTriangleInclusive(0.5, 0.5).Should().Be(XYZ.PointInTriangleInclusive(v1.XYZ, v2.XYZ, v3.XYZ, 0.5, 0.5));
      tri.PointInTriangleInclusive(0, 0).Should().Be(XYZ.PointInTriangleInclusive(v1.XYZ, v2.XYZ, v3.XYZ, 0, 0));
      tri.PointInTriangleInclusive(0, 1.0).Should().Be(XYZ.PointInTriangleInclusive(v1.XYZ, v2.XYZ, v3.XYZ, 0, 1.0));
      tri.PointInTriangleInclusive(1.0, 0).Should().Be(XYZ.PointInTriangleInclusive(v1.XYZ, v2.XYZ, v3.XYZ, 1.0, 0));
    }

    [Fact]
    public void Triangles_RemoveTriangle()
    {
      var ttm = new TrimbleTINModel();

      ttm.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));

      ttm.Triangles.RemoveTriangle(ttm.Triangles[0]);

      ttm.Triangles[0].Should().BeNull();
    }

    [Fact]
    public void Test_Triangle_ToString()
    {
      var v1 = new TriVertex(0, 0, 0);
      var v2 = new TriVertex(0, 1, 0);
      var v3 = new TriVertex(1, 1, 0);

      var tri = new Triangle(v1, v2, v3);

      tri.ToString().Should().ContainAll(new[] { "Vertices:", "Neighbours:"});
    }
  }
}
