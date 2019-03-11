using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.Tests.BinarizableSerialization;
using Xunit;

namespace VSS.TRex.Tests.Geometry
{
  public class BoundingIntegerExtent2DTests
  {
    [Fact]
    public void Creation()
    {
      var bound = new BoundingIntegerExtent2D();

      bound.IsValidExtent.Should().Be(true);
      bound.MinX.Should().Be(0);
      bound.MinY.Should().Be(0);
      bound.MaxX.Should().Be(0);
      bound.MaxY.Should().Be(0);
    }

    [Fact]
    public void Creation2()
    {
      var bound = new BoundingIntegerExtent2D(1, 2, 3, 4);

      bound.IsValidExtent.Should().Be(true);
      bound.MinX.Should().Be(1);
      bound.MinY.Should().Be(2);
      bound.MaxX.Should().Be(3);
      bound.MaxY.Should().Be(4);
    }

    [Fact]
    public void IsValidExtent()
    {
      var bound = new BoundingIntegerExtent2D();

      bound.IsValidExtent.Should().BeTrue();

      bound.MinX = 100;

      bound.IsValidExtent.Should().BeFalse();
    }

    [Fact]
    public void Area()
    {
      var bound = new BoundingIntegerExtent2D(0, 0, 100, 100);

      bound.SizeX.Should().Be(10);
      bound.SizeY.Should().Be(10);
      bound.Area().Should().Be(10000);
    }

    [Fact]
    public void Test_ToStrng()
    {
      var bound = new BoundingIntegerExtent2D(1, 2, 100, 101);

      bound.ToString().Should().Be($"MinX: {1}, MinY:{2}, MaxX: {100}, MaxY:{101}");
    }

    [Fact]
    public void Include()
    {
      var bound = new BoundingIntegerExtent2D(0, 0, 0, 0);

      bound.Include(0, 1000);
      bound.Should().BeEquivalentTo(new BoundingIntegerExtent2D(0, 0, 0, 1000));
      bound.Include(1000, 0);
      bound.Should().BeEquivalentTo(new BoundingIntegerExtent2D(0, 0, 1000, 1000));
      bound.Include(0, -1000);
      bound.Should().BeEquivalentTo(new BoundingIntegerExtent2D(0, -1000, 1000, 1000));
      bound.Include(-1000, 0);
      bound.Should().BeEquivalentTo(new BoundingIntegerExtent2D(-1000, -1000, 1000, 1000));
    }

    [Fact]
    public void IncludeExtent()
    {
      var bound = new BoundingIntegerExtent2D(0, 0, 0, 0);

      bound.Include(new BoundingIntegerExtent2D(0, 0, 0, 1000));
      bound.Should().BeEquivalentTo(new BoundingIntegerExtent2D(0, 0, 0, 1000));

      bound.Include(new BoundingIntegerExtent2D(0, 0, 1000, 0));
      bound.Should().BeEquivalentTo(new BoundingIntegerExtent2D(0, 0, 1000, 1000));

      bound.Include(new BoundingIntegerExtent2D(0, -1000, 0, 0));
      bound.Should().BeEquivalentTo(new BoundingIntegerExtent2D(0, -1000, 1000, 1000));

      bound.Include(new BoundingIntegerExtent2D(-1000, 0, 0, 0));
      bound.Should().BeEquivalentTo(new BoundingIntegerExtent2D(-1000, -1000, 1000, 1000));
    }

    [Fact]
    public void Includes()
    {
      var bound = new BoundingIntegerExtent2D(1, 1, 100, 100);

      bound.Includes(101, 101).Should().BeFalse();
      bound.Includes(0, 0).Should().BeFalse();

      // Int version
      bound.Includes(1, 1).Should().BeTrue();
      bound.Includes(1, 100).Should().BeTrue();
      bound.Includes(100, 100).Should().BeTrue();
      bound.Includes(100, 1).Should().BeTrue();

      // UInt version
      bound.Includes(1U, 1U).Should().BeTrue();
      bound.Includes(1U, 100U).Should().BeTrue();
      bound.Includes(100U, 100U).Should().BeTrue();
      bound.Includes(100U, 1U).Should().BeTrue();
    }

    [Fact]
    public void Assign()
    {
      var bound = new BoundingIntegerExtent2D(1, 1, 100, 100);

      var bound2 = new BoundingIntegerExtent2D();
      bound2.Assign(bound);

      bound2.Should().BeEquivalentTo(bound);
    }

    [Fact]
    public void Test_Equals()
    {
      var bound = new BoundingIntegerExtent2D(0, 0, 100, 100);
      var bound2 = new BoundingIntegerExtent2D(0, 0, 100, 100);
      var bound3 = new BoundingIntegerExtent2D(0, 0, 101, 101);

      bound.Equals((object)bound2).Should().BeTrue();
      bound.Equals((object)bound3).Should().BeFalse();

      bound.Equals(bound2).Should().BeTrue();
      bound.Equals(bound3).Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 1, 1, 1, true)]
    [InlineData(100, 100, 100, 100, true)]
    [InlineData(1, 100, 1, 100, true)]
    [InlineData(100, 1, 100, 1, true)]
    [InlineData(50, 50, 75, 75, true)]
    [InlineData(0, 0, 1, 1, false)]
    [InlineData(-1000, -1000, 1000, 1000, false)]
    public void Encloses(int minX, int minY, int maxX, int maxY, bool result)
    {
      var bound = new BoundingIntegerExtent2D(1, 1, 100, 100);

      bound.Encloses(new BoundingIntegerExtent2D(minX, minY, maxX, maxY)).Should().Be(result);
    }

    [Fact]
    public void Inverted()
    {
      var bound = new BoundingIntegerExtent2D(0, 0, 100, 100);

      bound.SetInverted();

      bound.Should().BeEquivalentTo(new BoundingIntegerExtent2D(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue));
    }

    [Fact]
    public void Expand()
    {
      var bound = new BoundingIntegerExtent2D(0, 0, 100, 100);

      bound.Expand(1);
      bound.Should().BeEquivalentTo(new BoundingIntegerExtent2D(-1, -1, 101, 101));
      bound.Expand(-1);
      bound.Should().BeEquivalentTo(new BoundingIntegerExtent2D(0, 0, 100, 100));
    }

    [Fact]
    public void Offset()
    {
      var bound = new BoundingIntegerExtent2D(0, 0, 100, 100);

      bound.Offset(1, 2);
      bound.Should().BeEquivalentTo(new BoundingIntegerExtent2D(1, 2, 101, 102));

      bound.Offset(-1, -2);
      bound.Should().BeEquivalentTo(new BoundingIntegerExtent2D(0, 0, 100, 100));
    }
  }
}
