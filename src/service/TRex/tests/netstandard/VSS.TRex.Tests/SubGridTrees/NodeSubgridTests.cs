using System;
using FluentAssertions;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.SubGridTrees.Core;
using Xunit;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.Tests.TestFixtures;

namespace VSS.TRex.Tests.SubGridTrees
{
  public class NodeSubgridTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_NodeSubGrid_Creation()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);

      Assert.NotNull(subgrid);
    }

    [Fact]
    public void Test_NodeSubGrid_CellHsValue()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
      INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

      Assert.True(subgrid.IsEmpty(), "Node subgrid not empty after creation");
      Assert.True(parentSubgrid.IsEmpty(), "Parent node subgrid not empty after creation");

      parentSubgrid.SetSubGrid(0, 0, subgrid);
      Assert.True(parentSubgrid.CellHasValue(0, 0), "Cell at 0, 0 does not indicate it has a value");
      Assert.False(parentSubgrid.CellHasValue(0, 1), "Cell at 0, 1 does indicate it has a value");
    }

    [Fact]
    public void Test_NodeSubGrid_Clear_Single()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
      INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

      Assert.True(subgrid.IsEmpty(), "Node subgrid not empty after creation");
      Assert.True(parentSubgrid.IsEmpty(), "Parent node subgrid not empty after creation");

      parentSubgrid.SetSubGrid(0, 0, subgrid);
      Assert.False(parentSubgrid.IsEmpty(), "Parent node subgrid is empty after adding subgrid to parent");

      parentSubgrid.Clear();
      Assert.True(parentSubgrid.IsEmpty(), "Parent node subgrid not empty after calling Clear()");
    }

    [Fact]
    public void Test_NodeSubGrid_Clear_Many()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

      INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);
      Assert.True(parentSubgrid.IsEmpty(), "Parent node subgrid not empty after creation");

      // Fill the entirety of the parent subgrid with new child subgrids
      for (int i = 0; i < SubGridTreeConsts.CellsPerSubGrid; i++)
      {
        parentSubgrid.SetSubGrid((byte) (i / SubGridTreeConsts.SubGridTreeDimension), (byte) (i % SubGridTreeConsts.SubGridTreeDimension),
          new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
      }

      Assert.False(parentSubgrid.IsEmpty(), "Parent node subgrid is empty after adding subgrids to parent");

      parentSubgrid.Clear();
      Assert.True(parentSubgrid.IsEmpty(), "Parent node subgrid not empty after calling Clear() to remove all subgrids");
    }

    [Fact]
    public void Test_NodeSubGrid_DeleteSubGrid()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

      // Fill the entirety of the parent subgrid with new child subgrids using SetSubGrid
      for (int i = 0; i < SubGridTreeConsts.CellsPerSubGrid; i++)
      {
        parentSubgrid.SetSubGrid((byte) (i / SubGridTreeConsts.SubGridTreeDimension), (byte) (i % SubGridTreeConsts.SubGridTreeDimension),
          new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
      }

      // Iterate over all subgrids deleting them one at a time
      for (int i = 0; i < SubGridTreeConsts.CellsPerSubGrid; i++)
      {
        parentSubgrid.DeleteSubGrid((byte) (i / SubGridTreeConsts.SubGridTreeDimension), (byte) (i % SubGridTreeConsts.SubGridTreeDimension));
      }

      Assert.Equal(0, parentSubgrid.CountChildren());
      Assert.True(parentSubgrid.IsEmpty(), "Parent not empty after deletion of subgrids");
    }

    [Fact]
    public void Test_NodeSubGrid_GetSubGrid()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
      INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

      parentSubgrid.SetSubGrid(1, 1, subgrid);
      Assert.Equal(1, parentSubgrid.CountChildren());

      // Get the subgrid and verify it is the same as the one set into it
      Assert.Equal(parentSubgrid.GetSubGrid(1, 1), subgrid);
    }

    [Fact]
    public void Test_NodeSubGrid_SetSubGrid_Sparcity_SetNullSubGridWithSingleExistingChild()
    {
      var tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      var parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);
      var subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);

      parentSubgrid.SetSubGrid(0, 0, subgrid);
      parentSubgrid.GetSubGrid(0, 0).Should().NotBeNull();

      // Test setting an existing null entry to null when there are non-zero entries in sparcity list
      parentSubgrid.SetSubGrid(1, 1, null);
    }

    [Fact]
    public void Test_NodeSubGrid_SetSubgrid()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

      // Fill the entirety of the parent subgrid with new child subgrids using SetSubGrid
      for (int i = 0; i < SubGridTreeConsts.CellsPerSubGrid; i++)
      {
        parentSubgrid.SetSubGrid((byte) (i / SubGridTreeConsts.SubGridTreeDimension), (byte) (i % SubGridTreeConsts.SubGridTreeDimension),
          new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
      }

      Assert.False(parentSubgrid.IsEmpty(), "Parent node subgrid is empty after adding subgrids to parent");
      Assert.Equal((int) parentSubgrid.CountChildren(), SubGridTreeConsts.CellsPerSubGrid);
    }

    [Fact]
    public void Test_NodeSubGrid_IsEmpty()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);

      Assert.True(subgrid.IsEmpty(), "Node subgrid not empty after creation");
      Assert.Equal(0, subgrid.CountChildren());
    }

    [Fact]
    public void Test_NodeSubGrid_ForEachSubgrid_FullScans()
    {
      int count;
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
      INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

      // Fill half of the parent subgrid with new child subgrids using SetSubGrid
      for (int i = 0; i < SubGridTreeConsts.CellsPerSubGrid / 2; i++)
      {
        parentSubgrid.SetSubGrid((byte) (i / SubGridTreeConsts.SubGridTreeDimension), (byte) (i % SubGridTreeConsts.SubGridTreeDimension),
          new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
      }

      // Iterate over all subgrids counting them
      count = 0;
      parentSubgrid.ForEachSubGrid(x =>
      {
        count++;
        return SubGridProcessNodeSubGridResult.OK;
      });

      Assert.Equal((int) count, SubGridTreeConsts.CellsPerSubGrid / 2);

      // Fill all of the parent subgrid with new child subgrids using SetSubGrid
      for (int i = 0; i < SubGridTreeConsts.CellsPerSubGrid; i++)
      {
        parentSubgrid.SetSubGrid((byte) (i / SubGridTreeConsts.SubGridTreeDimension), (byte) (i % SubGridTreeConsts.SubGridTreeDimension),
          new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
      }

      // Iterate over all subgrids counting them
      count = 0;
      parentSubgrid.ForEachSubGrid(x =>
      {
        count++;
        return SubGridProcessNodeSubGridResult.OK;
      });

      Assert.Equal((int) count, SubGridTreeConsts.CellsPerSubGrid);
    }

    [Fact]
    public void Test_NodeSubGrid_ForEachSubgrid_SpatialSubsetScans()
    {
      int count;
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
      INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

      // Fill all of the parent subgrid with new child subgrids using SetSubGrid
      for (int i = 0; i < SubGridTreeConsts.CellsPerSubGrid; i++)
      {
        parentSubgrid.SetSubGrid((byte) (i / SubGridTreeConsts.SubGridTreeDimension), (byte) (i % SubGridTreeConsts.SubGridTreeDimension),
          new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
      }

      // Iterate over a subset of the subgrids counting them (from the cell at (10, 10) to the cell at (29, 29)
      // an interval of 20 cells in the X and Y dimensions
      count = 0;
      parentSubgrid.ForEachSubGrid(x =>
      {
        count++;
        return SubGridProcessNodeSubGridResult.OK;
      }, 10, 10, 29, 29); // ==> Should scan 400 cells

      Assert.Equal(400, count);
    }

    [Fact]
    public void Test_NodeSubGrid_ForEachSubgrid_NodeFunctor_InvalidCellRange()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid subGrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);

      Action act = () => subGrid.ForEachSubGrid(x => SubGridProcessNodeSubGridResult.TerminateProcessing, SubGridTreeConsts.SubGridTreeDimension, 0, 2 * SubGridTreeConsts.SubGridTreeDimension, 2 * SubGridTreeConsts.SubGridTreeDimension);
      act.Should().Throw<ArgumentException>().WithMessage("Minimum sub grid cell X/Y bounds are out of range");

      act = () => subGrid.ForEachSubGrid(x => SubGridProcessNodeSubGridResult.TerminateProcessing, 0, SubGridTreeConsts.SubGridTreeDimension, 2 * SubGridTreeConsts.SubGridTreeDimension, 2 * SubGridTreeConsts.SubGridTreeDimension);
      act.Should().Throw<ArgumentException>().WithMessage("Minimum sub grid cell X/Y bounds are out of range");
    }

    [Fact]
    public void Test_NodeSubGrid_ForEachSubgrid_NodeFunctorWithIndices_InvalidCellRange()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid subGrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);

      Action act = () => subGrid.ForEachSubGrid((x, y, s) => SubGridProcessNodeSubGridResult.TerminateProcessing, SubGridTreeConsts.SubGridTreeDimension, 0, 2 * SubGridTreeConsts.SubGridTreeDimension, 2 * SubGridTreeConsts.SubGridTreeDimension);
      act.Should().Throw<ArgumentException>().WithMessage("Minimum sub grid cell X/Y bounds are out of range");

      act = () => subGrid.ForEachSubGrid((x, y, s) => SubGridProcessNodeSubGridResult.TerminateProcessing, 0, SubGridTreeConsts.SubGridTreeDimension, 2 * SubGridTreeConsts.SubGridTreeDimension, 2 * SubGridTreeConsts.SubGridTreeDimension);
      act.Should().Throw<ArgumentException>().WithMessage("Minimum sub grid cell X/Y bounds are out of range");
    }

    [Fact]
    public void Test_NodeSubGrid_ScanSubGrids()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
      INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

      // Fill all of the parent subgrid with new child subgrids using SetSubGrid
      for (int i = 0; i < SubGridTreeConsts.CellsPerSubGrid; i++)
      {
        parentSubgrid.SetSubGrid((byte) (i / SubGridTreeConsts.SubGridTreeDimension), (byte) (i % SubGridTreeConsts.SubGridTreeDimension),
          new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
      }

      // Test all child subgrids are visited (count should be zero as there are no leaf subgrids
      int leafCount, nodeCount;

      leafCount = 0;
      parentSubgrid.ScanSubGrids(tree.FullCellExtent(),
        leafSubgrid =>
        {
          leafCount++;
          return true;
        },
        null);
      Assert.Equal(0, leafCount);

      // Test all node child subgrids are visited (count should be zero as there are no leaf subgrids)
      leafCount = 0;
      nodeCount = 0;
      parentSubgrid.ScanSubGrids(tree.FullCellExtent(),
        leafSubgrid =>
        {
          leafCount++;
          return true;
        },
        nodeSubgrid =>
        {
          nodeCount++;
          return SubGridProcessNodeSubGridResult.OK;
        });
      Assert.Equal(0, leafCount);

      // Note, count is 1025 as the parent node counts as a node subgrid that was visited
      Assert.Equal(nodeCount, (1 + 1024));
    }

    [Fact]
    public void Test_NodeSubGrid_ScanSubGrids_InvalidBounds()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

      // Fill all of the parent subgrid with new child subgrids using SetSubGrid
      for (int i = 0; i < SubGridTreeConsts.CellsPerSubGrid; i++)
      {
        parentSubgrid.SetSubGrid((byte)(i / SubGridTreeConsts.SubGridTreeDimension), (byte)(i % SubGridTreeConsts.SubGridTreeDimension),
          new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
      }

      int leafCount;

      leafCount = 0;
      parentSubgrid.ScanSubGrids(tree.FullCellExtent(),
        leafSubgrid =>
        {
          leafCount++;
          return true;
        },
        null);
      Assert.Equal(0, leafCount);
    }

    [Fact]
    public void Test_NodeSubGrid_ScanSubGrids_TerminateProcessing()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

      // Fill all of the parent subgrid with new child subgrids using SetSubGrid
      for (int i = 0; i < SubGridTreeConsts.CellsPerSubGrid; i++)
      {
        parentSubgrid.SetSubGrid((byte)(i / SubGridTreeConsts.SubGridTreeDimension), (byte)(i % SubGridTreeConsts.SubGridTreeDimension),
          new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
      }

      int leafCount = 0, nodeCount = 0;

      parentSubgrid.ScanSubGrids(tree.FullCellExtent(),
        leafSubgrid =>
        {
          leafCount++;
          return true;
        },
        nodeSubgrid =>
        {
          nodeCount++;
          return SubGridProcessNodeSubGridResult.TerminateProcessing;
        });

      Assert.Equal(0, leafCount);
      Assert.Equal(1, nodeCount);
    }

    [Fact]
    public void Test_NodeSubGrid_CountChildren()
    {
      SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
      INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

      Assert.True(parentSubgrid.IsEmpty(), "Parent node subgrid is empty after adding subgrids to parent");

      Assert.Equal(0, parentSubgrid.CountChildren());

      parentSubgrid.SetSubGrid(0, 0, subgrid);

      Assert.Equal(1, parentSubgrid.CountChildren());
    }

    [Fact]
    public void Test_NodeSubGrid_WithinSparcityLimit()
    {
      var tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      var parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);
      var sparcityLimit = NodeSubGrid.SubGridTreeNodeCellSparcityLimit;

      // Add sparcity limit - 1 child node sub grids to the parent
      for (int i = 0; i < sparcityLimit; i++)
      {
        var subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
        parentSubgrid.SetSubGrid(i % SubGridTreeConsts.SubGridTreeDimension, i / SubGridTreeConsts.SubGridTreeDimension, subgrid);
      }

      parentSubgrid.CountNonNullCells().Should().Be((int) sparcityLimit);

      // Read through the sub grids added, plus another one to cover access failure
      for (int i = 0; i < sparcityLimit; i++)
      {
        var subGrid2 = parentSubgrid.GetSubGrid(i % SubGridTreeConsts.SubGridTreeDimension, i / SubGridTreeConsts.SubGridTreeDimension);
        subGrid2.Should().NotBeNull();
      }

      var subGrid = parentSubgrid.GetSubGrid((int)sparcityLimit % SubGridTreeConsts.SubGridTreeDimension,(int)sparcityLimit / SubGridTreeConsts.SubGridTreeDimension);
      subGrid.Should().BeNull();

      // Drain the sub grids back out of the node
      for (int i = 0; i < sparcityLimit; i++)
        parentSubgrid.SetSubGrid(i % SubGridTreeConsts.SubGridTreeDimension, i / SubGridTreeConsts.SubGridTreeDimension, null);

      parentSubgrid.CountNonNullCells().Should().Be(0);
    }

    [Fact]
    public void Test_NodeSubGrid_ExceedSparcityLimit()
    {
      var tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
      var parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);
      var sparcityLimit = NodeSubGrid.SubGridTreeNodeCellSparcityLimit;

      // Add sparcity limit + 1 child node subgrids to the parent
      for (int i = 0; i < sparcityLimit + 1; i++)
      {
        var subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
        parentSubgrid.SetSubGrid(i % SubGridTreeConsts.SubGridTreeDimension, i / SubGridTreeConsts.SubGridTreeDimension, subgrid);
      }

      parentSubgrid.CountNonNullCells().Should().Be((int)sparcityLimit + 1);

      // Read through the sub grids added, plus another one rto cover access failure
      for (int i = 0; i < sparcityLimit + 2; i++)
      {
        var subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);

        subgrid.Should().NotBeNull();
      }

      var subGrid = parentSubgrid.GetSubGrid((int)(sparcityLimit + 2) % SubGridTreeConsts.SubGridTreeDimension, (int)(sparcityLimit + 2) / SubGridTreeConsts.SubGridTreeDimension);
      subGrid.Should().BeNull();

      // Drain the sub grids back out of the node
      for (int i = 0; i < sparcityLimit + 1; i++)
        parentSubgrid.SetSubGrid(i % SubGridTreeConsts.SubGridTreeDimension, i / SubGridTreeConsts.SubGridTreeDimension, null);

      parentSubgrid.CountNonNullCells().Should().Be(0);
    }
  }
}
