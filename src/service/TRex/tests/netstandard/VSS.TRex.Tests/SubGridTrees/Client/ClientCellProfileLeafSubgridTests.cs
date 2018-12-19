using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class ClientCellProfileLeafSubgridTests
  {
    [Fact]
    public void Test_NullCells()
    {
      var cell = new ClientCellProfileLeafSubgridRecord();
      cell.Clear();

      var clientGrid = ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CellProfile) as ClientCellProfileLeafSubgrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y].Equals(cell)));
    }

    [Fact]
    public void Test_NullCell()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CellProfile) as ClientCellProfileLeafSubgrid;

      clientGrid.Cells[0, 0] = clientGrid.NullCell();
      Assert.False(clientGrid.CellHasValue(0, 0), "Cell not set to correct null value");
    }
  }
}
