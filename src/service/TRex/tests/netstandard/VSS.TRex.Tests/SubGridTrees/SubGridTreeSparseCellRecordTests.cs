using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.SubGridTrees.Core;
using Xunit;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.Tests.TestFixtures;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class SubGridTreeSparseCellRecordTests : IClassFixture<DILoggingAndStorageProxyFixture>
  {
        [Fact]
        public void Test_SubGridTreeSparseCellRecord_Creation()
        {
            SubGridTree tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            ISubGrid leafSubgrid = new SubGrid(tree, null, SubGridTreeConsts.SubGridTreeLevels);

            SubgridTreeSparseCellRecord sparseCell = new SubgridTreeSparseCellRecord(15, 15, leafSubgrid);

            Assert.True(sparseCell.CellX == 15 && sparseCell.CellY == 15 && sparseCell.Cell == leafSubgrid,
                          "Sparse subgrid tree cell record failed to initialise");
        }
    }
}
