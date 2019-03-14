using FluentAssertions;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Tests.BinaryReaderWriter;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class CompositeHeightClientLeafSubgridGridTests : IClassFixture<DILoggingFixture>
  {
    private ClientCompositeHeightsLeafSubgrid TestSubGrid()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CompositeHeights) as ClientCompositeHeightsLeafSubgrid;

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        clientGrid.Cells[x, y].FirstHeight = 1.1f;
        clientGrid.Cells[x, y].HighestHeight = 1.1f;
        clientGrid.Cells[x, y].LastHeight = 1.1f;
        clientGrid.Cells[x, y].LowestHeight = 1.1f;
      });

      return clientGrid;
    }

    [Fact]
    public void Test_NullCells()
    {
      var cell = new SubGridCellCompositeHeightsRecord();
      cell.Clear();

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CompositeHeights) as ClientCompositeHeightsLeafSubgrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y].Equals(cell)));
    }

    [Fact]
    public void Test_NullCell()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CompositeHeights) as ClientCompositeHeightsLeafSubgrid;

      clientGrid.Cells[0, 0] = clientGrid.NullCell();
      Assert.False(clientGrid.CellHasValue(0, 0), "Cell not set to correct null value");
    }

    [Fact]
    public void SetToZeroHeight()
    {
      var clientGrid = TestSubGrid();

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        clientGrid.Cells[x, y].FirstHeight = 1.1f;
        clientGrid.Cells[x, y].HighestHeight = 1.1f;
        clientGrid.Cells[x, y].LastHeight = 1.1f;
        clientGrid.Cells[x, y].LowestHeight = 1.1f;
      });

      clientGrid.SetToZeroHeight();

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        clientGrid.Cells[x, y].FirstHeight.Should().Be(0);
        clientGrid.Cells[x, y].HighestHeight.Should().Be(0);
        clientGrid.Cells[x, y].LastHeight.Should().Be(0);
        clientGrid.Cells[x, y].LowestHeight.Should().Be(0);
      });
    }

    [Fact]
    public void BinaryReaderWriter()
    {
      var clientGrid = TestSubGrid();

      TestBinary_ReaderWriterBufferedHelper.RoundTripSerialise(clientGrid);
    }
  }
}
