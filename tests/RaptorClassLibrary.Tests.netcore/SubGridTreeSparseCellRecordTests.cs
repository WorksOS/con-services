using System;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex;
using Xunit;

namespace VSS.TRex.Tests
{
        public class SubGridTreeSparseCellRecordTests
    {
        [Fact]
        public void Test_SubGridTreeSparseCellRecord_Creation()
        {
            ISubGrid leafSubgrid = null;
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels);

            SubgridTreeSparseCellRecord sparseCell = new SubgridTreeSparseCellRecord(15, 15, leafSubgrid);

            Assert.True(sparseCell.CellX == 15 && sparseCell.CellY == 15 && sparseCell.Cell == leafSubgrid,
                          "Sparce subgrid tree cell record failed to initialise");
        }
    }
}
