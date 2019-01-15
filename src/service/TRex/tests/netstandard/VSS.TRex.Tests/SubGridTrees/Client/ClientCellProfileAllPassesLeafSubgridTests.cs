using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class ClientCellProfileAllPassesLeafSubgridTests
  {
    [Fact]
    public void Test_NullCells()
    {
      var cell = new ClientCellProfileAllPassesLeafSubgridRecord();
      cell.Clear();

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CellPasses) as ClientCellProfileAllPassesLeafSubgrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y].GetHashCode() == cell.GetHashCode()));
    }

    [Fact]
    public void Test_NullCell()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CellPasses) as ClientCellProfileAllPassesLeafSubgrid;

      clientGrid.Cells[0, 0] = clientGrid.NullCell();
      Assert.False(clientGrid.CellHasValue(0, 0), "Cell not set to correct null value");
    }
  }
}
