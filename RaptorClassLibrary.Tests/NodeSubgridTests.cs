using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.SubGridTrees.Types;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class NodeSubgridTests
    {
        [TestMethod]
        public void Test_NodeSubGrid_Creation()
        {
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = null;

            // Test creation of a leaf node without an owner tree
            try
            {
                subgrid = new NodeSubGrid(null, null, SubGridTree.SubGridTreeLevels - 1);
                Assert.Fail("Was able to create a node subgrid with no owning tree");
            }
            catch (Exception)
            {
                // As expected
            }

            subgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1);

            Assert.IsTrue(subgrid != null, "Failed to create node subgrid");
        }

        [TestMethod]
        public void Test_NodeSubGrid_Clear_Single()
        {
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1);
            INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 2);

            Assert.IsTrue(subgrid.IsEmpty(), "Node subgrid not empty after creation");
            Assert.IsTrue(parentSubgrid.IsEmpty(), "Parent node subgrid not empty after creation");

            parentSubgrid.SetSubGrid(0, 0, subgrid);
            Assert.IsFalse(parentSubgrid.IsEmpty(), "Parent node subgrid is empty after adding subgrid to parent");

            parentSubgrid.Clear();
            Assert.IsTrue(parentSubgrid.IsEmpty(), "Parent node subgrid not empty after calling Clear()");
        }

        [TestMethod]
        public void Test_NodeSubGrid_Clear_Many()
        {
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 2);
            Assert.IsTrue(parentSubgrid.IsEmpty(), "Parent node subgrid not empty after creation");

            // Fill the entirety of the parent subgrid with new child subgrids
            for (int i = 0; i < SubGridTree.CellsPerSubgrid; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTree.SubGridTreeDimension), (byte)(i % SubGridTree.SubGridTreeDimension),
                                         new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1));
            }

            Assert.IsFalse(parentSubgrid.IsEmpty(), "Parent node subgrid is empty after adding subgrids to parent");

            parentSubgrid.Clear();
            Assert.IsTrue(parentSubgrid.IsEmpty(), "Parent node subgrid not empty after calling Clear() to remove all subgrids");
        }

        [TestMethod]
        public void Test_NodeSubGrid_DeleteSubGrid()
        {
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1);
            INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 2);

            // Fill the entirety of the parent subgrid with new child subgrids using SetSubGrid
            for (int i = 0; i < SubGridTree.CellsPerSubgrid; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTree.SubGridTreeDimension), (byte)(i % SubGridTree.SubGridTreeDimension),
                                         new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1));
            }

            // Iterate over all subgrids deleting them one at a time
            for (int i = 0; i < SubGridTree.CellsPerSubgrid; i++)
            {
                parentSubgrid.DeleteSubgrid((byte)(i / SubGridTree.SubGridTreeDimension), (byte)(i % SubGridTree.SubGridTreeDimension), false);
            }

            Assert.IsTrue(parentSubgrid.CountChildren() == 0, "Parent not empty after deletion of subgrids (count = {0}", parentSubgrid.CountChildren());
            Assert.IsTrue(parentSubgrid.IsEmpty(), "Parent not empty after deletion of subgrids");
        }

        [TestMethod]
        public void Test_NodeSubGrid_GetSubGrid()
        {
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1);
            INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 2);

            parentSubgrid.SetSubGrid(1, 1, subgrid);
            Assert.IsTrue(parentSubgrid.CountChildren() == 1, "Parent does not report  CountChildren() == 1");

            // Get the subgrid and verify it is the same as the one set into it
            Assert.IsTrue(parentSubgrid.GetSubGrid(1, 1) == subgrid, "Subgrid retrieved from parent is not the same as the one set into it");
        }

        [TestMethod]
        public void Test_NodeSubGrid_SetSubgrid()
        {
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1);
            INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 2);


            // Fill the entirety of the parent subgrid with new child subgrids using SetSubGrid
            for (int i = 0; i < SubGridTree.CellsPerSubgrid; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTree.SubGridTreeDimension), (byte)(i % SubGridTree.SubGridTreeDimension),
                                         new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1));
            }

            Assert.IsFalse(parentSubgrid.IsEmpty(), "Parent node subgrid is empty after adding subgrids to parent");
            Assert.IsTrue(parentSubgrid.CountChildren() == SubGridTree.CellsPerSubgrid, "Parent subgrid is not full of non-null cells");
        }

        [TestMethod]
        public void Test_NodeSubGrid_IsEmpty()
        {
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1);

            Assert.IsTrue(subgrid.IsEmpty(), "Node subgrid not empty after creation");
            Assert.IsTrue(subgrid.CountChildren() == 0, "Node subgrid not empty after creation");
        }

        [TestMethod]
        public void Test_NodeSubGrid_ForEachSubgrid_FullScans()
        {
            int count;
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1);
            INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 2);

            // Fill half of the parent subgrid with new child subgrids using SetSubGrid
            for (int i = 0; i < SubGridTree.CellsPerSubgrid / 2; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTree.SubGridTreeDimension), (byte)(i % SubGridTree.SubGridTreeDimension),
                                         new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1));
            }

            // Iterate over all subgrids counting them
            count = 0;
            parentSubgrid.ForEachSubGrid(x => { count++; return SubGridProcessNodeSubGridResult.OK; });

            Assert.IsTrue(count == SubGridTree.CellsPerSubgrid / 2, "ForEachSubgrid did not count all cells");

            // Fill all of the parent subgrid with new child subgrids using SetSubGrid
            for (int i = 0; i < SubGridTree.CellsPerSubgrid; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTree.SubGridTreeDimension), (byte)(i % SubGridTree.SubGridTreeDimension),
                                         new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1));
            }

            // Iterate over all subgrids counting them
            count = 0;
            parentSubgrid.ForEachSubGrid(x => { count++; return SubGridProcessNodeSubGridResult.OK; });

            Assert.IsTrue(count == SubGridTree.CellsPerSubgrid, "ForEachSubgrid did not count all cells");
        }

        [TestMethod]
        public void Test_NodeSubGrid_ForEachSubgrid_SpatialSubsetScans()
        {
            int count;
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1);
            INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 2);

            // Fill all of the parent subgrid with new child subgrids using SetSubGrid
            for (int i = 0; i < SubGridTree.CellsPerSubgrid; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTree.SubGridTreeDimension), (byte)(i % SubGridTree.SubGridTreeDimension),
                                         new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1));
            }

            // Iterate over a subset of the subgrids counting them (from the cell at (10, 10) to the cell at (29, 29)
            // an interval of 20 cells in the X and Y dimensions
            count = 0;
            parentSubgrid.ForEachSubGrid(x => { count++; return SubGridProcessNodeSubGridResult.OK; }, 10, 10, 29, 29); // ==> Should scan 400 cells

            Assert.IsTrue(count == 400, "ForEachSubgrid did not scan 400 cells (from 10, 10 --> 29, 29), count = {0}", count);
        }

        [TestMethod]
        public void Test_NodeSubGrid_ScanSubGrids()
        {
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1);
            INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 2);

            // Fill all of the parent subgrid with new child subgrids using SetSubGrid
            for (int i = 0; i < SubGridTree.CellsPerSubgrid; i++)
            {
                parentSubgrid.SetSubGrid((byte)(i / SubGridTree.SubGridTreeDimension), (byte)(i % SubGridTree.SubGridTreeDimension),
                                         new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1));
            }

            // Test all child subgrids are visited (count should be zero as there are no leaf subgrids
            int leafCount, nodeCount;

            leafCount = 0;
            parentSubgrid.ScanSubGrids(tree.FullCellExtent(),
                                       leafSubgrid => { leafCount++; return true; },
                                       null);
            Assert.IsTrue(leafCount == 0, "Leaf count is not correct ({0})", leafCount);

            // Test all node child subgrids are visited (count should be zero as there are no leaf subgrids)
            leafCount = 0;
            nodeCount = 0;
            parentSubgrid.ScanSubGrids(tree.FullCellExtent(),
                                       leafSubgrid => { leafCount++; return true; },
                                       nodeSubgrid => { nodeCount++; return SubGridProcessNodeSubGridResult.OK; });
            Assert.IsTrue(leafCount == 0, "Leaf count is not correct ({0})", leafCount);

            // Note, count is 1025 as the parent node counts as a node subgrid that was visited
            Assert.IsTrue(nodeCount == (1 + 1024), "node count is not correct ({0})", nodeCount);

            // Test retrieval using invalid bounds
            leafCount = 0;
            parentSubgrid.ScanSubGrids(tree.FullCellExtent(),
                                       leafSubgrid => { leafCount++; return true; },
                                       null);
            Assert.IsTrue(leafCount == 0, "Leaf count is not correct ({0}), should be one (0)", leafCount);
        }

        [TestMethod]
        public void Test_NodeSubGrid_CountChildren()
        {
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            INodeSubGrid subgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1);
            INodeSubGrid parentSubgrid = new NodeSubGrid(tree, null, SubGridTree.SubGridTreeLevels - 2);

            Assert.IsTrue(parentSubgrid.IsEmpty(), "Parent node subgrid is empty after adding subgrids to parent");

            Assert.IsTrue(parentSubgrid.CountChildren() == 0, "Parent subgrid children count is incorrect, should be zero (0)");

            parentSubgrid.SetSubGrid(0, 0, subgrid);

            Assert.IsTrue(parentSubgrid.CountChildren() == 1, "Parent subgrid children count is incorrect, should be one (1)");
        }
    }
}
