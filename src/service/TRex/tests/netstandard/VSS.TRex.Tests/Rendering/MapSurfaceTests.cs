using System;
using FluentAssertions;
using VSS.TRex.Rendering.Displayers;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Rendering
{
  public class MapSurfaceTests : IClassFixture<DIRenderingFixture>
  {
    [Fact]
    public void Creation()
    {
      var map = new MapSurface();
      map.Should().NotBeNull();
    }

    [Fact]
    public void RotatePoint()
    {
      var map = new MapSurface();
      map.SetRotation(Math.PI / 2);

      // Rotate about center point 500, 500 -> 0, 0
      map.Rotate_point(1000.0, 0.0, out var toX, out var toY);
      toX.Should().BeApproximately(0.0, 0.0000001);
      toY.Should().BeApproximately(0.0, 0.0000001);
    }

    [Fact]
    public void UnRotatePoint()
    {
      var map = new MapSurface();
      map.SetRotation(Math.PI / 2);

      // Rotate about center point 500, 500 -> 1000, 0
      map.Un_rotate_point(0.0, 0.0, out var toX, out var toY);

      toX.Should().BeApproximately(1000.0, 0.0000001);
      toY.Should().BeApproximately(0.0, 0.0000001);
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(1000.0, 0.0)]
    public void RotatePointRoundtrip(double testX, double testY)
    {
      var map = new MapSurface();
      map.SetRotation(Math.PI / 2);

      // Rotate/unrotate about center point 500, 500 -> 1000, 0

      map.Rotate_point(testX, testY, out var toX, out var toY);
      map.Un_rotate_point(toX, toY, out var toX2, out var toY2);

      toX2.Should().BeApproximately(testX, 0.0000001);
      toY2.Should().BeApproximately(testY, 0.0000001);
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(1000.0, 0.0)]
    public void ZeroRotationDoesNotMovepoint(double testX, double testY)
    {
      var map = new MapSurface();
      map.SetRotation(0);

      // Rotate/unrotate about center point 500, 500

      map.Rotate_point(testX, testY, out var toX, out var toY);

      toX.Should().BeApproximately(testX, 0.0000001);
      toY.Should().BeApproximately(testY, 0.0000001);
    }
  }
}
