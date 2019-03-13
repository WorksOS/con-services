using VSS.TRex.SubGridTrees;
using System.IO;
using FluentAssertions;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
  public class SubGridTreeLeafBitmapSubGridTests
  {
    [Fact]
    public void Test_SubGridTreeLeafBitmapSubGrid_Creation_OwnerParentLevel()
    {
      var subGrid = new SubGridTreeLeafBitmapSubGrid(null, null, SubGridTreeConsts.SubGridTreeLevels - 1);

      subGrid.Should().NotBeNull();
      subGrid.Owner.Should().BeNull();
      subGrid.Parent.Should().BeNull();
      subGrid.Level.Should().Be(SubGridTreeConsts.SubGridTreeLevels - 1);
    }

    [Fact]
    public void Test_SubGridTreeLeafBitmapSubGrid_ReadWrite()
    {
      var leaf = new SubGridTreeLeafBitmapSubGrid();
      leaf.Bits.SetBit(10, 10);

      var newLeaf = new SubGridTreeLeafBitmapSubGrid();

      using (var ms = new MemoryStream())
      {
        using (var bw = new BinaryWriter(ms))
        {
          leaf.Write(bw, new byte[10000]);

          ms.Position = 0;
          using (var br = new BinaryReader(ms))
          {
            newLeaf.Read(br, new byte[10000]);
          }
        }
      }

      newLeaf.Should().BeEquivalentTo(leaf);
      newLeaf.Bits[10, 10].Should().BeTrue();
      newLeaf.Bits[10, 10].Should().Be(leaf.Bits[10, 10]);
    }

    [Fact]
    public void Test_SubGridTreeLeafBitmapSubGrid_SubGridTreeLeafBitmapSubGrid()
    {
      var leaf = new SubGridTreeLeafBitmapSubGrid();
      leaf.Should().NotBeNull();
    }

    [Fact]
    public void Test_SubGridTreeLeafBitmapSubGrid_CountBits()
    {
      var leaf = new SubGridTreeLeafBitmapSubGrid();

      leaf.CountBits().Should().Be(0);
      leaf.Bits.SetBit(0, 0);
      leaf.CountBits().Should().Be(1);
      leaf.Bits.SetBit(SubGridTreeConsts.SubGridTreeDimensionMinus1, 0);
      leaf.CountBits().Should().Be(2);
      leaf.Bits.SetBit(0, SubGridTreeConsts.SubGridTreeDimensionMinus1);
      leaf.CountBits().Should().Be(3);
      leaf.Bits.SetBit(SubGridTreeConsts.SubGridTreeDimensionMinus1, SubGridTreeConsts.SubGridTreeDimensionMinus1);
      leaf.CountBits().Should().Be(4);
    }

    [Fact]
    public void Test_SubGridTreeLeafBitmapSubGrid_ComputeCellsExtents()
    {
      var leaf = new SubGridTreeLeafBitmapSubGrid();

      leaf.ComputeCellsExtents().IsValidExtent.Should().BeFalse();

      leaf.Bits.SetBit(0, 0);
      leaf.ComputeCellsExtents().IsValidExtent.Should().BeTrue();
      leaf.ComputeCellsExtents().Should().BeEquivalentTo(new BoundingIntegerExtent2D(0, 0, 0, 0));

      leaf.Bits.SetBit(SubGridTreeConsts.SubGridTreeDimensionMinus1, 0);
      leaf.ComputeCellsExtents().IsValidExtent.Should().BeTrue();
      leaf.ComputeCellsExtents().Should().BeEquivalentTo(new BoundingIntegerExtent2D(0, 0, SubGridTreeConsts.SubGridTreeDimension - 1, 0));

      leaf.Bits.SetBit(0, SubGridTreeConsts.SubGridTreeDimensionMinus1);
      leaf.ComputeCellsExtents().IsValidExtent.Should().BeTrue();
      leaf.ComputeCellsExtents().Should().BeEquivalentTo(new BoundingIntegerExtent2D(0, 0, SubGridTreeConsts.SubGridTreeDimension - 1, SubGridTreeConsts.SubGridTreeDimension - 1));
    }

    [Fact]
    public void Test_SubGridTreeLeafBitmapSubGrid_ForEachSetBit()
    {
      var leaf = new SubGridTreeLeafBitmapSubGrid();

      int count = 0;
      leaf.ForEachSetBit((x, y) => { count++; });
      count.Should().Be(0);

      leaf.Bits[0, 0] = true;
      leaf.Bits[0, SubGridTreeConsts.SubGridTreeDimensionMinus1] = true;
      leaf.Bits[SubGridTreeConsts.SubGridTreeDimensionMinus1, 0] = true;
      leaf.Bits[SubGridTreeConsts.SubGridTreeDimensionMinus1, SubGridTreeConsts.SubGridTreeDimensionMinus1] = true;

      leaf.ForEachSetBit((x, y) =>
      {
        x.Should().BeOneOf(0, SubGridTreeConsts.SubGridTreeDimensionMinus1);
        y.Should().BeOneOf(0, SubGridTreeConsts.SubGridTreeDimensionMinus1);
        count++;
      });
      count.Should().Be(4);
    }

    [Fact]
    public void Test_SubGridTreeLeafBitmapSubGrid_ForEachSetBit2()
    {
      var leaf = new SubGridTreeLeafBitmapSubGrid();

      int count = 0;
      leaf.ForEachSetBit((x, y) => 
      {
        count++;
        return true;
      });
      count.Should().Be(0);

      leaf.Bits[0, 0] = true;
      leaf.Bits[0, SubGridTreeConsts.SubGridTreeDimensionMinus1] = true;
      leaf.Bits[SubGridTreeConsts.SubGridTreeDimensionMinus1, 0] = true;
      leaf.Bits[SubGridTreeConsts.SubGridTreeDimensionMinus1, SubGridTreeConsts.SubGridTreeDimensionMinus1] = true;

      leaf.ForEachSetBit((x, y) =>
      {
        x.Should().BeOneOf(0, SubGridTreeConsts.SubGridTreeDimensionMinus1);
        y.Should().BeOneOf(0, SubGridTreeConsts.SubGridTreeDimensionMinus1);
        count++;

        return false;
      });
      count.Should().Be(1);

      count = 0;
      leaf.ForEachSetBit((x, y) =>
      {
        x.Should().BeOneOf(0, SubGridTreeConsts.SubGridTreeDimensionMinus1);
        y.Should().BeOneOf(0, SubGridTreeConsts.SubGridTreeDimensionMinus1);
        count++;

        return true;
      });
      count.Should().Be(4);
    }

    [Fact]
    public void Test_SubGridTreeLeafBitmapSubGrid_ForEachClearBit() 
    {
      var leaf = new SubGridTreeLeafBitmapSubGrid();

      uint count = 0;
      leaf.ForEachClearBit((x, y) => { count++; });
      count.Should().Be(SubGridTreeConsts.CellsPerSubGrid);

      count = 0;
      leaf.Bits[0, 0] = true;
      leaf.Bits[0, SubGridTreeConsts.SubGridTreeDimensionMinus1] = true;
      leaf.Bits[SubGridTreeConsts.SubGridTreeDimensionMinus1, 0] = true;
      leaf.Bits[SubGridTreeConsts.SubGridTreeDimensionMinus1, SubGridTreeConsts.SubGridTreeDimensionMinus1] = true;

      leaf.ForEachClearBit((x, y) => count++);
      count.Should().Be(SubGridTreeConsts.CellsPerSubGrid - 4);
    }

    [Fact]
    public void Test_SubGridTreeLeafBitmapSubGrid_ForEach() 
    {
      var leaf = new SubGridTreeLeafBitmapSubGrid();

      uint count = 0;
      leaf.ForEach((x, y) => { count++; });
      count.Should().Be(SubGridTreeConsts.CellsPerSubGrid);
    }

    [Fact]
    public void Test_SubGridTreeLeafBitmapSubGrid_ForEach_ForEach2() 
    {
      var leaf = new SubGridTreeLeafBitmapSubGrid();

      leaf.ForEach((x, y) => x == y);
      leaf.CountBits().Should().Be(SubGridTreeConsts.SubGridTreeDimension);
    }
  }
}
