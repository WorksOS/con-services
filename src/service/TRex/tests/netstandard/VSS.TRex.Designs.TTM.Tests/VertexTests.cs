using FluentAssertions;
using Xunit;

namespace VSS.TRex.Designs.TTM.Tests
{
  public class VertexTests
  {
    [Fact]
    public void Creation_FromCoordinates()
    {
      var vertex = new TriVertex(1, 2, 3);

      vertex.X = 1;
      vertex.Y = 2;
      vertex.Z = 3;
    }

    [Fact]
    public void Creation_FromXYZ()
    {
      var vertex = new TriVertex(1, 2, 3);
      var vertex2 = new TriVertex(vertex.XYZ);

      vertex2.X = 1;
      vertex2.Y = 2;
      vertex2.Z = 3;
    }

    [Fact]
    public void Creation_SetFomXYZ()
    {
      var vertex = new TriVertex(0, 0, 0);
      var vertex2 = new TriVertex(vertex.XYZ);

      vertex.XYZ = vertex2.XYZ;

      vertex2.X = 1;
      vertex2.Y = 2;
      vertex2.Z = 3;
    }

    [Fact]
    public void IsEqual_Vertex()
    {
      var vertex1 = new TriVertex(1, 2, 3);
      var vertex2 = new TriVertex(1, 2, 3);
      var vertex3 = new TriVertex(2, 3, 4);

      vertex1.IsEqual(vertex2, 0.001).Should().BeTrue();
      vertex1.IsEqual(vertex3, 0.001).Should().BeFalse();
    }

    [Fact]
    public void IsEqual_WithTolerance()
    {
      var vertex1 = new TriVertex(1, 2, 3);
      var vertex2 = new TriVertex(1.1, 2.1, 3.1);

      vertex1.IsEqual(1, 2, 3, 0.0001).Should().BeTrue();
      vertex2.IsEqual(1, 2, 3, 0.0001).Should().BeFalse();
      vertex2.IsEqual(1, 2, 3, 0.2).Should().BeTrue();
    }


    [Fact]
    public void AdjustLimits()
    {
      var vertex1 = new TriVertex(1, 2, 3);
      var vertex2 = new TriVertex(1.1, 2.1, 3.1);
      var vertex3 = new TriVertex(0.9, 1.9, 2.9);

      double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
      double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

      vertex1.AdjustLimits(ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);

      minX.Should().Be(1);
      minY.Should().Be(2);
      minZ.Should().Be(3);
      maxX.Should().Be(1);
      maxY.Should().Be(2);
      maxZ.Should().Be(3);

      vertex2.AdjustLimits(ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);

      minX.Should().Be(1);
      minY.Should().Be(2);
      minZ.Should().Be(3);
      maxX.Should().Be(1.1);
      maxY.Should().Be(2.1);
      maxZ.Should().Be(3.1);

      vertex3.AdjustLimits(ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);

      minX.Should().Be(0.9);
      minY.Should().Be(1.9);
      minZ.Should().Be(2.9);
      maxX.Should().Be(1.1);
      maxY.Should().Be(2.1);
      maxZ.Should().Be(3.1);
    }

    [Fact]
    public void Test_ToString()
    {
      var vertex = new TriVertex(1, 2, 3);

      vertex.ToString().Should().Be("Tag:0, X=1.000, Y=2.000, Z=3.000");
    }
  }
}
