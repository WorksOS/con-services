using FluentAssertions;
using Xunit;

namespace VSS.TRex.Designs.TTM.Optimised.Tests
{
    public class TriangleTests
    {
      [Fact]
      public void TTM_TrianglesTests_Creation()
      {
        Triangles triangles = new Triangles();

        triangles.Should().NotBeNull();
      }

      [Fact]
      public void TTM_TrianglesTests_CreationWithVertexIndices()
      {
        var tri = new Triangle(1, 2, 3);

        tri.Vertex0.Should().Be(1);
        tri.Vertex1.Should().Be(2);
        tri.Vertex2.Should().Be(3);
    }
  }
}
