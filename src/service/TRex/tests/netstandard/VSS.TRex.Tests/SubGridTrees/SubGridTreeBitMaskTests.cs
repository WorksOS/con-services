using VSS.TRex.SubGridTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using VSS.TRex.SubGridTrees.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
  public class SubGridTreeBitMaskTests
  {
    [Fact(Skip = "Not Implemented")]
    public void Test_SubGridTreeBitMask_SubGridTreeBitMask()
    {
      Assert.True(false);
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_SubGridTreeBitMask_GetCell()
    {
      Assert.True(false);
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_SubGridTreeBitMask_SetCell()
    {
      Assert.True(false);
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_SubGridTreeBitMask_GetLeaf()
    {
      Assert.True(false);
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_SubGridTreeBitMask_RemoveLeafOwningCell()
    {
      Assert.True(false);
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_SubGridTreeBitMask_CountBits()
    {
      Assert.True(false);
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_SubGridTreeBitMask_ComputeCellsWorldExtents()
    {
      Assert.True(false);
    }

    [Fact]
    public void Test_SubGridTreeBitMask_LeafExists()
    {
      var mask = new SubGridTreeBitMask();

      for (uint i = 0; i < 100; i++)
      for (uint j = 0; j < 100; j++)
      {
        uint x = i * 10;
        uint y = j * 10;

        mask.SetCell(x, y, true);
      }

      for (uint i = 0; i < 100; i++)
      for (uint j = 0; j < 100; j++)
      {
        uint x = i * 10;
        uint y = j * 10;

        mask.LeafExists(x, y).Should().BeTrue();
      }

      for (uint i = 0; i < 100; i++)
      for (uint j = 0; j < 100; j++)
      {
        uint x = i * 1000000;
        uint y = j * 1000000;

        mask.LeafExists(x, y).Should().BeFalse();
      }
    }

    [Fact]
    public void Test_SubGridTreeBitMask_SetOp_OR()
    {
      var mask = new SubGridTreeBitMask();

      for (uint i = 0; i < 100; i++)
      for (uint j = 0; j < 100; j++)
      {
        uint x = i * 10;
        uint y = j * 10;

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

      for (uint i = 0; i < 100; i++)
      for (uint j = 0; j < 100; j++)
      {
        uint x = i * 10;
        uint y = j * 10;

        secondMask[x, y].Should().BeTrue();
      }
    }

    [Fact]
    public void Test_SubGridTreeBitMask_SetOp_AND()
    {
      var mask = new SubGridTreeBitMask();

      for (uint i = 0; i < 100; i++)
      for (uint j = 0; j < 100; j++)
      {
        uint x = i * 10;
        uint y = j * 10;

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

      for (uint i = 0; i < 100; i++)
      for (uint j = 0; j < 100; j++)
      {
        uint x = i * 10;
        uint y = j * 10;

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
    public void Test_SubGridTreeBitMask_ClearCellIfSet_SingleCellAtLocation(uint x, uint y)
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

      for (uint i = 0; i < 100; i++)
      for (uint j = 0; j < 100; j++)
      {
        uint x = i * 10;
        uint y = j * 10;

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
