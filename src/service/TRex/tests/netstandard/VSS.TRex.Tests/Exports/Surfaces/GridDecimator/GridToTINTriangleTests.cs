using VSS.TRex.Designs.TTM;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces.GridDecimator
{
    public class GridToTINTriangleTests
    {
      [Fact]
      public void GridToTINTriangle_Creation()
      {
        GridToTINTriangle tri = new GridToTINTriangle(null, null, null);

        Assert.NotNull(tri);
      }

      [Fact]
      public void GridToTINTriangle_Creation2()
      {
        GridToTINTriangle tri = new GridToTINTriangle(new TTMVertex(0, 0, 0), new TTMVertex(1, 0, 0), new TTMVertex(1, 1, 0));

        Assert.NotNull(tri);
        Assert.NotNull(tri.Vertices[0]);
        Assert.NotNull(tri.Vertices[1]);
        Assert.NotNull(tri.Vertices[2]);
      }

      [Fact]
      public void GridToTINTriangle_Creation3()
      {
        GridToTINTriangle tri = new GridToTINTriangle(new TriVertex(0, 0, 0), new TriVertex(1, 0, 0), new TriVertex(1, 1, 0));

        Assert.NotNull(tri);
        Assert.NotNull(tri.Vertices[0]);
        Assert.NotNull(tri.Vertices[1]);
        Assert.NotNull(tri.Vertices[2]);
      }
    }
}
