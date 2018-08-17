using System;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex;
using Xunit;
using VSS.TRex.SubGridTrees.Factories;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class SubGridTreeSparseCellRecordTests
    {
        [Fact]
        public void Test_SubGridTreeSparseCellRecord_Creation()
        {
            ISubGrid leafSubgrid = null;
            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            SubgridTreeSparseCellRecord sparseCell = new SubgridTreeSparseCellRecord(15, 15, leafSubgrid);

            Assert.True(sparseCell.CellX == 15 && sparseCell.CellY == 15 && sparseCell.Cell == leafSubgrid,
                          "Sparce subgrid tree cell record failed to initialise");
        }
    }
}
