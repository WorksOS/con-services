using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace VSS.TRex.Designs.TTM.Tests
{
    public class TriangleTests
    {
      [Fact]
      public void TTM_TrianglesTests_Creation()
      {
        Triangles triangles = new Triangles();

        Assert.NotNull(triangles);
      }

      [Fact]
      public void TTM_TrianglesTests_Pack()
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
  }
}
