using System;
using VSS.VisionLink.Raptor.SubGridTrees.Types;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.SubGridTrees;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
        public class SubGridTreeSparseCellRecordTests
    {
        [Fact]
        public void Test_SubGridTreeSparseCellRecord_Creation()
        {
            ISubGrid leafSubgrid = null;
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new VSS.VisionLink.Raptor.SubGridTrees.SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels);

            SubgridTreeSparseCellRecord sparseCell = new SubgridTreeSparseCellRecord(15, 15, leafSubgrid);

            Assert.True(sparseCell.CellX == 15 && sparseCell.CellY == 15 && sparseCell.Cell == leafSubgrid,
                          "Sparce subgrid tree cell record failed to initialise");
        }
    }
}
