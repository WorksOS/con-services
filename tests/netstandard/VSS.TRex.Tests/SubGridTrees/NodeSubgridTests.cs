using System;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex;
using Xunit;
using VSS.TRex.SubGridTrees.Factories;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class NodeSubgridTests
    {
        [Fact]
        public void Test_NodeSubGrid_Creation()
        {
            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = null;

            // Test creation of a leaf node without an owner tree
            try
            {
                subgrid = new NodeSubGrid(null, null, SubGridTreeConsts.SubGridTreeLevels - 1);
                Assert.True(false,"Was able to create a node subgrid with no owning tree");
            }
            catch (Exception)
            {
                // As expected
            }

            subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);

            Assert.NotNull(subgrid);
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
            for (int i = 0; i < SubGridTreeConsts.CellsPerSubgrid; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTreeConsts.SubGridTreeDimension), (byte)(i % SubGridTreeConsts.SubGridTreeDimension),
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
            INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
            INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

            // Fill the entirety of the parent subgrid with new child subgrids using SetSubGrid
            for (int i = 0; i < SubGridTreeConsts.CellsPerSubgrid; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTreeConsts.SubGridTreeDimension), (byte)(i % SubGridTreeConsts.SubGridTreeDimension),
                                         new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
            }

            // Iterate over all subgrids deleting them one at a time
            for (int i = 0; i < SubGridTreeConsts.CellsPerSubgrid; i++)
            {
                parentSubgrid.DeleteSubgrid((byte)(i / SubGridTreeConsts.SubGridTreeDimension), (byte)(i % SubGridTreeConsts.SubGridTreeDimension));
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
        public void Test_NodeSubGrid_SetSubgrid()
        {
            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
            INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);


            // Fill the entirety of the parent subgrid with new child subgrids using SetSubGrid
            for (int i = 0; i < SubGridTreeConsts.CellsPerSubgrid; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTreeConsts.SubGridTreeDimension), (byte)(i % SubGridTreeConsts.SubGridTreeDimension),
                                         new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
            }

            Assert.False(parentSubgrid.IsEmpty(), "Parent node subgrid is empty after adding subgrids to parent");
            Assert.Equal((uint)parentSubgrid.CountChildren(), SubGridTreeConsts.CellsPerSubgrid);
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
            for (int i = 0; i < SubGridTreeConsts.CellsPerSubgrid / 2; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTreeConsts.SubGridTreeDimension), (byte)(i % SubGridTreeConsts.SubGridTreeDimension),
                                         new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
            }

            // Iterate over all subgrids counting them
            count = 0;
            parentSubgrid.ForEachSubGrid(x => { count++; return SubGridProcessNodeSubGridResult.OK; });

            Assert.Equal((uint)count, SubGridTreeConsts.CellsPerSubgrid / 2);

            // Fill all of the parent subgrid with new child subgrids using SetSubGrid
            for (int i = 0; i < SubGridTreeConsts.CellsPerSubgrid; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTreeConsts.SubGridTreeDimension), (byte)(i % SubGridTreeConsts.SubGridTreeDimension),
                                         new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
            }

            // Iterate over all subgrids counting them
            count = 0;
            parentSubgrid.ForEachSubGrid(x => { count++; return SubGridProcessNodeSubGridResult.OK; });

            Assert.Equal((uint)count, SubGridTreeConsts.CellsPerSubgrid);
        }

        [Fact]
        public void Test_NodeSubGrid_ForEachSubgrid_SpatialSubsetScans()
        {
            int count;
            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
            INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

            // Fill all of the parent subgrid with new child subgrids using SetSubGrid
            for (int i = 0; i < SubGridTreeConsts.CellsPerSubgrid; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTreeConsts.SubGridTreeDimension), (byte)(i % SubGridTreeConsts.SubGridTreeDimension),
                                         new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
            }

            // Iterate over a subset of the subgrids counting them (from the cell at (10, 10) to the cell at (29, 29)
            // an interval of 20 cells in the X and Y dimensions
            count = 0;
            parentSubgrid.ForEachSubGrid(x => { count++; return SubGridProcessNodeSubGridResult.OK; }, 10, 10, 29, 29); // ==> Should scan 400 cells

            Assert.Equal(400, count);
        }

        [Fact]
        public void Test_NodeSubGrid_ScanSubGrids()
        {
            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1);
            INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 2);

            // Fill all of the parent subgrid with new child subgrids using SetSubGrid
            for (int i = 0; i < SubGridTreeConsts.CellsPerSubgrid; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTreeConsts.SubGridTreeDimension), (byte)(i % SubGridTreeConsts.SubGridTreeDimension),
                                         new NodeSubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels - 1));
            }

            // Test all child subgrids are visited (count should be zero as there are no leaf subgrids
            int leafCount, nodeCount;

            leafCount = 0;
            parentSubgrid.ScanSubGrids(tree.FullCellExtent(),
                                       leafSubgrid => { leafCount++; return true; },
                                       null);
            Assert.Equal(0, leafCount);

            // Test all node child subgrids are visited (count should be zero as there are no leaf subgrids)
            leafCount = 0;
            nodeCount = 0;
            parentSubgrid.ScanSubGrids(tree.FullCellExtent(),
                                       leafSubgrid => { leafCount++; return true; },
                                       nodeSubgrid => { nodeCount++; return SubGridProcessNodeSubGridResult.OK; });
            Assert.Equal(0, leafCount);

            // Note, count is 1025 as the parent node counts as a node subgrid that was visited
            Assert.Equal(nodeCount, (1 + 1024));

            // Test retrieval using invalid bounds
            leafCount = 0;
            parentSubgrid.ScanSubGrids(tree.FullCellExtent(),
                                       leafSubgrid => { leafCount++; return true; },
                                       null);
            Assert.Equal(0, leafCount);
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
    }
}
