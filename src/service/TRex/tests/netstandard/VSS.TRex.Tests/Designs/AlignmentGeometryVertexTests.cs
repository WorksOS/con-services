using FluentAssertions;
using VSS.TRex.Designs.Models;
using Xunit;

namespace VSS.TRex.Tests.Designs
{
  public class AlignmentGeometryVertexTests
  {
    [Fact]
    public void Creation()
    {
      var vertex = new AlignmentGeometryVertex(1, 2, 3, 4);

      vertex.X.Should().Be(1);
      vertex.Y.Should().Be(2);
      vertex.Z.Should().Be(3);
      vertex.Station.Should().Be(4);
    }

    [Fact]
    public void Creation2()
    {
      var originalVertex = new AlignmentGeometryVertex(1, 2, 3, 4);
      var vertex = new AlignmentGeometryVertex(originalVertex);

      vertex.X.Should().Be(1);
      vertex.Y.Should().Be(2);
      vertex.Z.Should().Be(3);
      vertex.Station.Should().Be(4);
    }

    [Fact]
    public void Test_ToString()
    {
      var vertex = new AlignmentGeometryVertex(1, 2, 3, 4);
      vertex.ToString().Should().Be($"X:{1:F3}, Y:{2:F3}, Z:{3:F3} Station:{4:F3}");
    }

    [Fact]
    public void Test_Equals()
    {
      var vertex = new AlignmentGeometryVertex(1, 2, 3, 4);
      var vertex2 = new AlignmentGeometryVertex(1, 2, 3, 4);
      var vertex3 = new AlignmentGeometryVertex(2, 3, 4, 5);

      vertex.Equals(vertex2).Should().BeTrue();
      vertex.Equals(vertex3).Should().BeFalse();
    }
  }
}
