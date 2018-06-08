using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Tests.netcore.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class CMVClientLeafSubGridTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_CMVClientLeafSubgridGridTests_Creation()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.CCV) as ClientCMVLeafSubGrid;
      Assert.NotNull(clientGrid);
    }

    [Fact]
    public void Test_NullCells()
    {
      var cell = new SubGridCellPassDataCMVEntryRecord();
      cell.Clear();

      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.CCV) as ClientCMVLeafSubGrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y].Equals(cell)));
    }
  }
}
