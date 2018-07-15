using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Tests.netcore.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class TemperatureClientLeafSubGridTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_NullCells()
    {
      var cell = new SubGridCellPassDataTemperatureEntryRecord();
      cell.Clear();

      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.Temperature) as ClientTemperatureLeafSubGrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y].Equals(cell)));
    }
  }
}
