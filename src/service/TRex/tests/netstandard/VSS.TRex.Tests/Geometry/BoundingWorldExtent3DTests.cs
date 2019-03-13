using System.IO;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.TRex.Tests.BinaryReaderWriter;
using Xunit;

namespace VSS.TRex.Tests.Geometry
{
  public class BoundingWorldExtent3DTests
  {
    [Fact]
    public void Creation()
    {
      var bound = new BoundingWorldExtent3D();
      bound.IsValidHeightExtent.Should().Be(true);
      bound.IsValidPlanExtent.Should().Be(true);
    }

    [Fact]
    public void Creation_FromMinMax_XY()
    {
      var bound = new BoundingWorldExtent3D(1, 1, 100, 100);
      bound.IsValidHeightExtent.Should().Be(false);
      bound.IsValidPlanExtent.Should().Be(true);

      bound.MinX.Should().Be(1);
      bound.MinY.Should().Be(1);
      bound.MaxX.Should().Be(100);
      bound.MaxY.Should().Be(100);
    }

    [Fact]
    public void Creation_FromMinMax_XYZ()
    {
      var bound = new BoundingWorldExtent3D(1, 1, 100, 100, 12, 34);
      bound.IsValidHeightExtent.Should().Be(true);
      bound.IsValidPlanExtent.Should().Be(true);

      bound.MinX.Should().Be(1);
      bound.MinY.Should().Be(1);
      bound.MaxX.Should().Be(100);
      bound.MaxY.Should().Be(100);
      bound.MinZ.Should().Be(12);
      bound.MaxZ.Should().Be(34);
    }

    [Fact]
    public void Creation_FromSource()
    {
      var bound = new BoundingWorldExtent3D(1, 1, 100, 100);

      var bound2 = new BoundingWorldExtent3D(bound);
      bound2.Should().BeEquivalentTo(bound);
    }

    [Fact]
    public void Assign()
    {
      var bound = new BoundingWorldExtent3D(1, 1, 100, 100);

      var bound2 = new BoundingWorldExtent3D();
      bound2.Assign(bound);

      bound2.Should().BeEquivalentTo(bound);
    }

    [Fact]
    public void Area()
    {
      var bound = new BoundingWorldExtent3D(0, 0, 100, 100);
      bound.Area.Should().Be(10000);
    }

    [Fact]
    public void Size_XYZ()
    {
      var bound = new BoundingWorldExtent3D(0, 1, 100, 100, 10, 20);

      bound.SizeX.Should().Be(100);
      bound.SizeY.Should().Be(99);
      bound.SizeZ.Should().Be(10);
    }

    [Fact]
    public void Center_XYZ()
    {
      var bound = new BoundingWorldExtent3D(0, 1, 100, 100, 10, 20);

      bound.CenterX.Should().Be(50);
      bound.CenterY.Should().Be(50.5);
      bound.CenterZ.Should().Be(15);
    }

    [Fact]
    public void Test_ToString()
    {
      var bound = new BoundingWorldExtent3D(0, 0, 100, 100);
      bound.ToString().Should().Be($"MinX: {bound.MinX}, MaxX:{bound.MaxX}, MinY:{bound.MinY}, MaxY:{bound.MaxY}, MinZ: {bound.MinZ}, MaxZ:{bound.MaxZ}");
    }

    [Fact]
    public void MaximalPlanCoverage()
    {
      var bound = new BoundingWorldExtent3D();

      bound.IsMaximalPlanConverage.Should().BeFalse();

      bound.SetMaximalCoverage();

      bound.IsMaximalPlanConverage.Should().BeTrue();
    }

    [Fact]
    public void Center_LargestPlanDimension()
    {
      var bound = new BoundingWorldExtent3D();
      bound.LargestPlanDimension.Should().Be(0);

      bound = new BoundingWorldExtent3D(0, 1, 100, 100);
      bound.LargestPlanDimension.Should().Be(100);
    }

    [Fact]
    public void Includes_Point()
    {
      var bound = new BoundingWorldExtent3D(0, 0, 100, 100);
      bound.Includes(0, 0).Should().BeTrue();
      bound.Includes(100, 0).Should().BeTrue();
      bound.Includes(0, 100).Should().BeTrue();
      bound.Includes(100, 100).Should().BeTrue();

      bound.Includes(-1, -1).Should().BeFalse();
      bound.Includes(101, 0).Should().BeFalse();
      bound.Includes(0, 101).Should().BeFalse();
      bound.Includes(101, 101).Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 0, 100, 100, 0, 0, 100, 100, true)]
    [InlineData(50, 50, 150, 150, 50, 50, 100, 100, true)]
    [InlineData(-50, -50, 50, 50, 0, 0, 50, 50, true)]
    [InlineData(0, 0, 50, 100, 0, 0, 50, 100, true)]
    [InlineData(99, 99, 101, 101, 99, 99, 100, 100, true)]
    [InlineData(0, 99, 1, 101, 0, 99, 1, 100, true)]
    [InlineData(-1, -1, 1, 1, 0, 0, 1, 1, true)]
    [InlineData(99, 0, 101, 1, 99, 0, 100, 1, true)]
    [InlineData(-1, -1, -1, -1, 0, 0, 100, 100, false)]
    [InlineData(1000, 1000, 2000, 2000, 0, 0, 100, 100, false)]
    public void Intersect_BoundingExtent(double minX, double minY, double maxX, double maxY, double r_minX, double r_minY, double r_maxX, double r_maxY, bool valid)
    {
      BoundingWorldExtent3D bound = new BoundingWorldExtent3D(0, 0, 100, 100);

      void Check()
      {
        if (valid)
        {
          bound.MinX.Should().Be(r_minX);
          bound.MinY.Should().Be(r_minY);
          bound.MaxX.Should().Be(r_maxX);
          bound.MaxY.Should().Be(r_maxY);
        }
      }

      // Check variant #1
      var bound2 = new BoundingWorldExtent3D(minX, minY, maxX, maxY);

      bound.Intersect(bound2);
      bound.IsValidPlanExtent.Should().Be(valid);

      Check();

      // Check variant #2
      bound = new BoundingWorldExtent3D(0, 0, 100, 100);

      bound.Intersect(minX, minY, maxX, maxY);
      bound.IsValidPlanExtent.Should().Be(valid);

      Check();
    }

    [Fact]
    public void Test_Equals()
    {
      var bound = new BoundingWorldExtent3D(0, 0, 100, 100);
      var bound2 = new BoundingWorldExtent3D(0, 0, 100, 100);
      var bound3 = new BoundingWorldExtent3D(0, 0, 101, 101);

      bound.Equals((object) bound2).Should().BeTrue();
      bound.Equals((object) bound3).Should().BeFalse();

      bound.Equals(bound2).Should().BeTrue();
      bound.Equals(bound3).Should().BeFalse();
    }

    [Fact]
    public void Test_Null()
    {
      var bound = BoundingWorldExtent3D.Null();
      bound.Equals(new BoundingWorldExtent3D(Consts.NullDouble, Consts.NullDouble, Consts.NullDouble, Consts.NullDouble, Consts.NullDouble, Consts.NullDouble)).Should().BeTrue();
    }

    [Fact]
    public void Offset()
    {
      var bound = new BoundingWorldExtent3D(0, 0, 100, 100, 0, 0);

      bound.Offset(1, 1);

      bound.MinX.Should().Be(1);
      bound.MinY.Should().Be(1);
      bound.MaxX.Should().Be(101);
      bound.MaxY.Should().Be(101);
      bound.MinZ.Should().Be(0);
      bound.MaxZ.Should().Be(0);

      bound.Offset(1);
      bound.MinZ.Should().Be(1);
      bound.MaxZ.Should().Be(1);

      bound.Offset(-1, -1);

      bound.MinX.Should().Be(0);
      bound.MinY.Should().Be(0);
      bound.MaxX.Should().Be(100);
      bound.MaxY.Should().Be(100);
      bound.MinZ.Should().Be(1);
      bound.MaxZ.Should().Be(1);

      bound.Offset(-1);
      bound.MinZ.Should().Be(0);
      bound.MaxZ.Should().Be(0);
    }

    [Fact]
    public void Shrink()
    {
      var bound = new BoundingWorldExtent3D(0, 0, 100, 100, 0, 10);

      bound.Shrink(1, 1);

      bound.MinX.Should().Be(1);
      bound.MinY.Should().Be(1);
      bound.MaxX.Should().Be(99);
      bound.MaxY.Should().Be(99);
      bound.MinZ.Should().Be(0);
      bound.MaxZ.Should().Be(10);

      bound.Shrink(1);
      bound.MinZ.Should().Be(1);
      bound.MaxZ.Should().Be(9);

      bound.Shrink(-1, -1);

      bound.MinX.Should().Be(0);
      bound.MinY.Should().Be(0);
      bound.MaxX.Should().Be(100);
      bound.MaxY.Should().Be(100);
      bound.MinZ.Should().Be(1);
      bound.MaxZ.Should().Be(9);

      bound.Shrink(-1);
      bound.MinZ.Should().Be(0);
      bound.MaxZ.Should().Be(10);
    }

    [Fact]
    public void Expand_Plan()
    {
      var bound = new BoundingWorldExtent3D(0, 0, 100, 100, 0, 10);

      bound.Expand(1, 1);

      bound.MinX.Should().Be(-1);
      bound.MinY.Should().Be(-1);
      bound.MaxX.Should().Be(101);
      bound.MaxY.Should().Be(101);
      bound.MinZ.Should().Be(0);
      bound.MaxZ.Should().Be(10);

      bound.Expand(1);
      bound.MinZ.Should().Be(-1);
      bound.MaxZ.Should().Be(11);
    }

    [Fact]
    public void Inverted()
    {
      var bound = BoundingWorldExtent3D.Inverted();

      bound.MinX.Should().Be(1E100);
      bound.MinY.Should().Be(1E100);
      bound.MaxX.Should().Be(-1E100);
      bound.MaxY.Should().Be(-1E100);
      bound.MinZ.Should().Be(1E100);
      bound.MaxZ.Should().Be(-1E100);
    }

    [Fact]
    public void Full()
    {
      var bound = BoundingWorldExtent3D.Full();

      bound.IsMaximalPlanConverage.Should().BeTrue();
    }

    [Fact]
    public void Extract2DExtents()
    {
      var bound = new BoundingWorldExtent3D(0, 1, 100, 102);

      bound.Extract2DExtents(out var minX, out var minY, out var maxX, out var maxY);

      minX.Should().Be(0);
      minY.Should().Be(1);
      maxX.Should().Be(100);
      maxY.Should().Be(102);
    }

    [Fact]
    public void Extract3DExtents()
    {
      var bound = new BoundingWorldExtent3D(0, 1, 100, 102, 9, 11);

      bound.Extract3DExtents(out var minX, out var minY, out var maxX, out var maxY, out var minZ, out var maxZ);

      minX.Should().Be(0);
      minY.Should().Be(1);
      maxX.Should().Be(100);
      maxY.Should().Be(102);
      minZ.Should().Be(9);
      maxZ.Should().Be(11);
    }

    [Fact]
    public void Set()
    {
      var bound = new BoundingWorldExtent3D();
      bound.Set(0, 1, 100, 102);
      bound.Should().BeEquivalentTo(new BoundingWorldExtent3D(0, 1, 100, 102, 0, 0));
    }

    [Fact]
    public void Clear()
    {
      var bound = new BoundingWorldExtent3D();
      bound.Set(0, 1, 100, 102);
      var bound2 = new BoundingWorldExtent3D();

      bound.Clear();

      bound.Should().BeEquivalentTo(bound2);
    }

    [Fact]
    public void FromToBinary()
    {
      var cp1 = new BoundingWorldExtent3D(1, 2, 3, 4, 5, 6);
      var writer = new TestBinaryWriter();
      cp1.ToBinary(writer);

      var cp2 = new BoundingWorldExtent3D();
      cp2.FromBinary(new TestBinaryReader(writer._stream.BaseStream as MemoryStream));

      cp1.Should().BeEquivalentTo(cp2);
    }

    [Fact]
    public void BinaryReaderWriter()
    {
      var cp1 = new BoundingWorldExtent3D(1, 2, 3, 4, 5, 6);

      TestBinary_ReaderWriterHelper.RoundTripSerialise(cp1);
    }

    [Fact]
    public void Include_Height()
    {
      var bound = new BoundingWorldExtent3D(0, 0, 0, 0, 0, 0);

      bound.Include(10);
      bound.Should().BeEquivalentTo(new BoundingWorldExtent3D(0, 0, 0, 0, 0, 10));

      bound.Include(-10);
      bound.Should().BeEquivalentTo(new BoundingWorldExtent3D(0, 0, 0, 0, -10, 10));
    }

    [Fact]
    public void Include_3D()
    {
      var bound = new BoundingWorldExtent3D(0, 0, 0, 0, 0, 0);

      bound.Include(10, 11, 12);
      bound.Should().BeEquivalentTo(new BoundingWorldExtent3D(0, 0, 10, 11, 0, 12));

      bound.Include(-10, -11, -12);
      bound.Should().BeEquivalentTo(new BoundingWorldExtent3D(-10, -11, 10, 11, -12, 12));
    }

    [Fact]
    public void Include_2D()
    {
      var bound = new BoundingWorldExtent3D(0, 0, 0, 0, 0, 0);

      bound.Include(10, 11);
      bound.Should().BeEquivalentTo(new BoundingWorldExtent3D(0, 0, 10, 11, 0, 0));

      bound.Include(-10, -11);
      bound.Should().BeEquivalentTo(new BoundingWorldExtent3D(-10, -11, 10, 11, 0, 0));
    }

    [Fact]
    public void Include_BoundingRectangle()
    {
      var bound = BoundingWorldExtent3D.Inverted();

      bound.Include(new BoundingWorldExtent3D(10, 10, 20, 20, 0, 0));
      bound.Should().BeEquivalentTo(new BoundingWorldExtent3D(10, 10, 20, 20, 0, 0));

      bound.Include(new BoundingWorldExtent3D(10, 10, 20, 20, 10, 20));
      bound.Should().BeEquivalentTo(new BoundingWorldExtent3D(10, 10, 20, 20, 0, 20));

      bound.Include(new BoundingWorldExtent3D(-100, -100, 200, 200, -10, 0));
      bound.Should().BeEquivalentTo(new BoundingWorldExtent3D(-100, -100, 200, 200, -10, 20));

      bound.Include(new BoundingWorldExtent3D(-1000, -1000, 2000, 2000, -20, 100), true);
      bound.Should().BeEquivalentTo(new BoundingWorldExtent3D(-1000, -1000, 2000, 2000, -10, 20));
    }

    [Fact]
    public void ScalePlan()
    {
      var bound = new BoundingWorldExtent3D(1, 1, 2, 2, 0, 0);

      bound.ScalePlan(2.0);
      bound.Should().BeEquivalentTo(new BoundingWorldExtent3D(0.5, 0.5, 2.5, 2.5, 0, 0));
    }
  }
}
