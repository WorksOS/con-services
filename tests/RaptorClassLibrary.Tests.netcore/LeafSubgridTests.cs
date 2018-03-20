using System;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
        public class LeafSubgridTests
    {
        [Fact]
        public void Test_LeafSubgrid_Creation()
        {
            ISubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            ILeafSubGrid leaf = null;

            // Test creation of a leaf node without an owner tree
            try
            {
                leaf = new LeafSubGrid(null, null, (byte)(tree.NumLevels + 1));
                Assert.True(false,"Was able to create a leaf subgrid with no owning tree");
            }
            catch (Exception)
            {
                // As expected
            }

            // Test creation of a leaf node at an inappropriate level
            try
            {
                leaf = new LeafSubGrid(tree, null, (byte)(tree.NumLevels + 1));
                Assert.True(false,"Was able to create a leaf subgrid at an inappropriate level");
            }
            catch (Exception)
            {
                // As expected
            }

            leaf = new LeafSubGrid(tree, null, tree.NumLevels);

            Assert.True(leaf != null && leaf.Level == tree.NumLevels);
        }

        [Fact]
        public void Test_LeafSubgrid_IsEmpty()
        {
            ISubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());
            ILeafSubGrid leaf = new LeafSubGrid(tree, null, tree.NumLevels);

            // Base leaf classes don't implement CellHasValue(), so this call should fail with an exception
            try
            {
                bool isEmpty = leaf.IsEmpty();

                Assert.True(false,"Base LeafSubGrid class did not throw an exception due to unimplemented CellHasValu()");
            } catch (Exception)
            {
                // As expected
            }
        }
    }
}
