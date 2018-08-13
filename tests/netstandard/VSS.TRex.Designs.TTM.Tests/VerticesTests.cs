using Xunit;

namespace VSS.TRex.Designs.TTM.Tests
{
    public class VerticesTests
    {
      [Fact]
      public void TTM_VerticesTests_Creation()
      {
        TriVertices vertices = new TriVertices();

        Assert.NotNull(vertices);
      }

      [Fact]
      public void TTM_VerticesTests_InitPointSearch()
      {
        TriVertices vertices = new TriVertices();
        vertices.InitPointSearch(-1, -1, 101, 101, 100);

        Assert.True(true);
      }

    [Fact]
      public void TTM_VerticesTests_Pack()
      {
        TriVertices vertices = new TriVertices();
        vertices.InitPointSearch(-1, -1, 101, 101, 100);

        vertices.AddPoint(0, 0, 0);
        vertices.AddPoint(100.0, 90.0, 5.0);

        Assert.True(vertices.Count == 2);
        vertices[0] = null;

        vertices.Pack();
        Assert.True(vertices.Count == 1);
        Assert.True(vertices[0].X == 100.0);
        Assert.True(vertices[0].Y == 90.0);
        Assert.True(vertices[0].Z == 5.0);
      }
  }
}
