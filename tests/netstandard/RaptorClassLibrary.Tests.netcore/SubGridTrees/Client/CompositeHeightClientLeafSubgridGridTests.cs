using VSS.TRex;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Tests.netcore.TestFixtures;
using Xunit;

namespace RaptorClassLibrary.Tests.netcore.SubGridTrees.Client
{
  public class CompositeHeightClientLeafSubgridGridTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_CompositeHeightClientLeafSubgridGridTests_Creation()
    {
      var clientGrid = new ClientCompositeHeightsLeafSubgrid(null, null, SubGridTree.SubGridTreeLevels, 1.0, SubGridTree.DefaultIndexOriginOffset);
      Assert.NotNull(clientGrid);
    }

    [Fact]
    public void Test_NullCells()
    {
      var cell = new SubGridCellCompositeHeightsRecord();
      cell.Clear();

      var clientGrid = new ClientCompositeHeightsLeafSubgrid(null, null, SubGridTree.SubGridTreeLevels, 1.0, SubGridTree.DefaultIndexOriginOffset);
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y].Equals(cell)));
    }
  }
}
