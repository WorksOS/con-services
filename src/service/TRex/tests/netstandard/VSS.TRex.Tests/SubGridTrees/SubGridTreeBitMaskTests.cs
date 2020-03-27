using VSS.TRex.SubGridTrees;
using FluentAssertions;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
  public class SubGridTreeBitMaskTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_SubGridTreeBitMask_SubGridTreeBitMask()
    {
      var mask = new SubGridTreeBitMask();
      mask.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(0, SubGridTreeConsts.SubGridTreeDimension - 1)]
    [InlineData(SubGridTreeConsts.SubGridTreeDimension - 1, 0)]
    [InlineData(SubGridTreeConsts.SubGridTreeDimension - 1, SubGridTreeConsts.SubGridTreeDimension - 1)]
    [InlineData(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension)]
    [InlineData(100, 100)]
    [InlineData(1000, 1000)]
    [InlineData(10000, 10000)]
    [InlineData(100000, 100000)]
    [InlineData(1000000, 1000000)]
    [InlineData(10000000, 10000000)]
    [InlineData(100000000, 100000000)]
    [InlineData(1000000000, 1000000000)]
    public void Test_SubGridTreeBitMask_GetCellAndSetCell(int x, int y)
    {
      var mask = new SubGridTreeBitMask();

      mask.GetCell(x, y).Should().BeFalse();
      mask.SetCell(x, y, true);
      mask.GetCell(x, y).Should().BeTrue();
      mask.SetCell(x, y, false);
      mask.GetCell(x, y).Should().BeFalse();
    }

    [Fact]
    public void Test_SubGridTreeBitMask_RemoveLeafOwningCell()
    {
      var mask = new SubGridTreeBitMask();

      // Check removing non-existing leaf is a null op
      mask.RemoveLeafOwningCell(0, 0);

      // Add a cell (causing a leaf to be added), then remove it
      mask[0, 0] = true;
      mask.CountBits().Should().Be(1);
      mask.CountLeafSubGridsInMemory().Should().Be(1);

      mask.RemoveLeafOwningCell(0, 0);
      mask.CountBits().Should().Be(0);
      mask.CountLeafSubGridsInMemory().Should().Be(0);
    }

    [Fact]
    public void Test_SubGridTreeBitMask_CountBits()
    {
      var mask = new SubGridTreeBitMask();

      for (int i = 0; i < 100; i++)
      for (int j = 0; j < 100; j++)
      {
        int x = i * 10;
        int y = j * 10;

        mask.SetCell(x, y, true);
      }

      mask.CountBits().Should().Be(10000);
    }

    [Fact]
    public void Test_SubGridTreeBitMask_ComputeCellsWorldExtents_EmptyMask()
    {
      var mask = new SubGridTreeBitMask();
      mask.ComputeCellsWorldExtents().Should().BeEquivalentTo(BoundingWorldExtent3D.Inverted());
    }


    [Fact]
    public void Test_SubGridTreeBitMask_ComputeCellsWorldExtents_SingleCellAtOrigin()
    {
      var mask = new SubGridTreeBitMask();
      mask[SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset] = true;

      var extent = mask.ComputeCellsWorldExtents();

      extent.MinX.Should().BeApproximately(0, 0.00001);
      extent.MaxX.Should().BeApproximately(mask.CellSize, 0.00001);
      extent.MinY.Should().BeApproximately(0, 0.00001);
      extent.MaxY.Should().BeApproximately(mask.CellSize, 0.00001);
    }

    [Fact]
    public void Test_SubGridTreeBitMask_ComputeCellsWorldExtents_TwoCells()
    {
      var mask = new SubGridTreeBitMask();
      mask[SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset] = true;
      mask[SubGridTreeConsts.DefaultIndexOriginOffset + 1000, SubGridTreeConsts.DefaultIndexOriginOffset + 1000] = true;

      var extent = mask.ComputeCellsWorldExtents();

      extent.MinX.Should().BeApproximately(0, 0.00001);
      extent.MaxX.Should().BeApproximately(1001 * mask.CellSize, 0.00001);
      extent.MinY.Should().BeApproximately(0, 0.00001);
      extent.MaxY.Should().BeApproximately(1001 * mask.CellSize, 0.00001);
    }

    [Fact]
    public void Test_SubGridTreeBitMask_LeafExists_Positive()
    {
      var mask = new SubGridTreeBitMask();

      for (int i = 0; i < 100; i++)
      for (int j = 0; j < 100; j++)
      {
        int x = i * 10;
        int y = j * 10;

        mask.SetCell(x, y, true);
      }

      for (int i = 0; i < 100; i++)
      for (int j = 0; j < 100; j++)
      {
        int x = i * 10;
        int y = j * 10;

        mask.LeafExists(x, y).Should().BeTrue();
      }
    }

    [Fact]
    public void Test_SubGridTreeBitMask_LeafExists_Negative()
    {
      var mask = new SubGridTreeBitMask();

      for (int i = 0; i < 100; i++)
      for (int j = 0; j < 100; j++)
      {
        int x = i * 10;
        int y = j * 10;

        mask.SetCell(x, y, true);
      }

      for (int i = 0; i < 100; i++)
      for (int j = 0; j < 100; j++)
      {
        int x = (i + 1) * 1000000;
        int y = (j + 1) * 1000000;

        mask.LeafExists(x, y).Should().BeFalse();
      }
    }

    [Fact]
    public void Test_SubGridTreeBitMask_SetOp_OR()
    {
      var mask = new SubGridTreeBitMask();

      for (int i = 0; i < 100; i++)
      for (int j = 0; j < 100; j++)
      {
        int x = i * 10;
        int y = j * 10;

        mask.SetCell(x, y, true);
      }

      int expectedBitCount = 10000;
      mask.CountBits().Should().Be(expectedBitCount);

      // Make a copy of mask
      var secondMask = new SubGridTreeBitMask();
      secondMask.SetOp_OR(mask);
      secondMask.CountBits().Should().Be(expectedBitCount);

      var thirdMask = new SubGridTreeBitMask();
      secondMask.SetOp_OR(thirdMask);
      secondMask.CountBits().Should().Be(expectedBitCount);

      for (int i = 0; i < 100; i++)
      for (int j = 0; j < 100; j++)
      {
        int x = i * 10;
        int y = j * 10;

        secondMask[x, y].Should().BeTrue();
      }
    }

    [Fact]
    public void Test_SubGridTreeBitMask_SetOp_AND()
    {
      var mask = new SubGridTreeBitMask();

      for (int i = 0; i < 100; i++)
      for (int j = 0; j < 100; j++)
      {
        int x = i * 10;
        int y = j * 10;

        mask.SetCell(x, y, true);
      }

      int expectedBitCount = 10000;
      mask.CountBits().Should().Be(expectedBitCount);

      // Make a copy of mask
      var secondMask = new SubGridTreeBitMask();
      secondMask.SetOp_OR(mask);
      secondMask.CountBits().Should().Be(expectedBitCount);

      // Check ANDing mask and second mask results in the same bit count
      secondMask.SetOp_AND(mask);
      secondMask.CountBits().Should().Be(expectedBitCount);

      for (int i = 0; i < 100; i++)
      for (int j = 0; j < 100; j++)
      {
        int x = i * 10;
        int y = j * 10;

        mask[x, y].Should().BeTrue();
      }

      // Check ANDing with empty mask clears all bits in mask
      var emptyMask = new SubGridTreeBitMask();
      emptyMask.CountBits().Should().Be(0);
      mask.SetOp_AND(emptyMask);

      mask.CountBits().Should().Be(0);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(0, SubGridTreeConsts.SubGridTreeDimension - 1)]
    [InlineData(SubGridTreeConsts.SubGridTreeDimension - 1, 0)]
    [InlineData(SubGridTreeConsts.SubGridTreeDimension - 1, SubGridTreeConsts.SubGridTreeDimension - 1)]
    [InlineData(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension)]
    [InlineData(100, 100)]
    [InlineData(1000, 1000)]
    [InlineData(10000, 10000)]
    [InlineData(100000, 100000)]
    [InlineData(1000000, 1000000)]
    [InlineData(10000000, 10000000)]
    [InlineData(100000000, 100000000)]
    [InlineData(1000000000, 1000000000)]
    public void Test_SubGridTreeBitMask_ClearCellIfSet_SingleCellAtLocation(int x, int y)
    {
      var mask = new SubGridTreeBitMask();
      mask[x, y].Should().BeFalse();
      mask.ClearCellIfSet(x, y);
      mask[x, y].Should().BeFalse();

      mask.SetCell(x, y, true);
      mask[x, y].Should().BeTrue();

      mask.ClearCellIfSet(x, y);
      mask[x, y].Should().BeFalse();
    }

    [Fact]
    public void Test_SubGridTreeBitMask_ClearCellIfSet_ManyCellsWidelyDispersed()
    {
      var mask = new SubGridTreeBitMask();

      for (int i = 0; i < 100; i++)
      for (int j = 0; j < 100; j++)
      {
        int x = i * 10;
        int y = j * 10;

        mask[x, y].Should().BeFalse();
        mask.ClearCellIfSet(x, y);
        mask[x, y].Should().BeFalse();

        mask.SetCell(x, y, true);
        mask[x, y].Should().BeTrue();

        mask.ClearCellIfSet(x, y);
        mask[x, y].Should().BeFalse();
        }
    }
  }
}
