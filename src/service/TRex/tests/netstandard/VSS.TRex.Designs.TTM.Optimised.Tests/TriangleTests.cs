using Xunit;

namespace VSS.TRex.Designs.TTM.Optimised.Tests
{
    public class TriangleTests
    {
      [Fact]
      public void TTM_TrianglesTests_Creation()
      {
        Triangles triangles = new Triangles();

        Assert.NotNull(triangles);
      }
  }
}
