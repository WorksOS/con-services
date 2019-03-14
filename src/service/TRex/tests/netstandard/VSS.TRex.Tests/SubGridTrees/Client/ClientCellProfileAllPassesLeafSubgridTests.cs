using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Tests.BinaryReaderWriter;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class ClientCellProfileAllPassesLeafSubgridTests : IClassFixture<DILoggingFixture>
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

    [Fact]
    public void ReadWriteBinary()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CellPasses) as ClientCellProfileAllPassesLeafSubgrid;

      clientGrid.Cells[10, 10] = new ClientCellProfileAllPassesLeafSubgridRecord
      {
        TotalPasses = 2,
        CellPasses = new []
        {
          new ClientCellProfileLeafSubgridRecord
          {
            Height = 11.1f
          },
          new ClientCellProfileLeafSubgridRecord
          {
            Height = 12.2f
          }
        }
      };

      TestBinary_ReaderWriterBufferedHelper.RoundTripSerialise(clientGrid);
    }

    [Fact]
    public void DumpToLog()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CellPasses) as ClientCellProfileAllPassesLeafSubgrid;
      clientGrid.DumpToLog(clientGrid.ToString());
    }
  }
}
