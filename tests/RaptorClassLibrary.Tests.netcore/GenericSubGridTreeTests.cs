using System;
using VSS.TRex.SubGridTrees;
using Xunit;

namespace VSS.TRex.RaptorClassLibrary.Tests
{
        public class GenericSubGridTreeTests
    {
        [Fact]
        public void Test_GenericSubGridTree_Creation()
        {
           var tree1 = new GenericSubGridTree<bool>(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<bool>>());

            Assert.True(tree1 != null && tree1.NumLevels == SubGridTree.SubGridTreeLevels && tree1.CellSize == 1.0,
                "Generic sub grid tree not created as expected with 3 arg constructor");

            var tree2 = new GenericSubGridTree<bool>(SubGridTree.SubGridTreeLevels, 1.0);

            Assert.True(tree2 != null && tree2.NumLevels == SubGridTree.SubGridTreeLevels && tree2.CellSize == 1.0,
                "Generic sub grid tree not created as expected with  arg constructor");
        }

        [Fact]
        public void Test_GenericSubGridTree_GetCell()
        {
            var tree = new GenericSubGridTree<bool>(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<bool>>());

            Assert.False(tree[0, 0]);
            tree[0, 0] = true;
            Assert.True(tree[0, 0]);
        }

        [Fact]
        public void Test_GenericSubGridTree_SetCell()
        {
            var tree = new GenericSubGridTree<bool>(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<bool>>());

            tree[0, 0] = true;
            Assert.True(tree[0, 0]);
        }

        [Fact]
        public void Test_GenericSubGridTree_NullCellValue()
        {
            var tree1 = new GenericSubGridTree<bool> (SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<bool>>());
            Assert.False(tree1.NullCellValue);

            var tree2 = new GenericSubGridTree<long> (SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<long>>());
            Assert.Equal(0, tree2.NullCellValue);

            var tree3 = new GenericSubGridTree<object>(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<long>>());
            Assert.Null(tree3.NullCellValue);
        }

        [Fact]
        public void Test_GenericSubGridTree_ForEach()
        {
            var tree = new GenericSubGridTree<bool>(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<bool>>());
            int count;

            // Count 'true' cells (should be none yet)
            count = 0;
            tree.ForEach(x => { count = x ? count++ : count; return true; });
            Assert.Equal(0, count);

            // Add some true cells to the tree and count them
            for (uint x = 0; x < 10; x++)
            {
                for (uint y = 0; y < 10; y++)
                {
                    tree[x * 10, y * 10] = true;
                }
            }

            count = 0;
            tree.ForEach(x => { count = x ? count + 1 : count; return true; });
            Assert.Equal(100, count);
        }
    }
}
