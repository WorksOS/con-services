using Xunit;

namespace VSS.TRex.Designs.TTM.Optimised.Tests
{
    public class VerticesTests
    {
      [Fact]
      public void TTM_VerticesTests_Creation()
      {
        TriVertices vertices = new TriVertices();

        Assert.NotNull(vertices);
      }
  }
}
