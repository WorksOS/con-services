using VSS.TRex.Tests.Exports.Surfaces.GridDecimator;
using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces.GridDecimator
{
    public class GridToTINHeapNodeTests
    {
      [Fact]
      public void GridToTINHeapNode_Creation()
      {
        GridToTINHeapNode node = new GridToTINHeapNode();
        Assert.True(node.Import == 0.0);
        Assert.Null(node.Tri);
        Assert.True(node.sx == int.MaxValue);
        Assert.True(node.sy == int.MaxValue);
        Assert.True(node.sz == 0);
    }

      [Fact]
      public void GridToTINHeapNode_Creation2()
      {
        GridToTINHeapNode node = new GridToTINHeapNode();

        GridToTINHeapNode node2 = new GridToTINHeapNode(node);

        Assert.True(node2.Import == 0.0);
        Assert.Null(node2.Tri);
        Assert.True(node2.sx == int.MaxValue);
        Assert.True(node2.sy == int.MaxValue);
        Assert.True(node2.sz == 0);
      }

      [Fact]
      public void GridToTINHeapNode_Creation3()
      {
        GridToTINHeapNode node = new GridToTINHeapNode()
        {
          Tri = new GridToTINTriangle(null, null, null),
          Import = 1.1,
          sx = 10,
          sy = 22,
          sz = 35
        };

        GridToTINHeapNode node2 = new GridToTINHeapNode(node);

        Assert.True(node2.Import == 1.1);
        Assert.NotNull(node2.Tri);
        Assert.True(node2.sx == int.MaxValue);
        Assert.True(node2.sy == int.MaxValue);
        Assert.True(node2.sz == 0);
      }
  }
}
