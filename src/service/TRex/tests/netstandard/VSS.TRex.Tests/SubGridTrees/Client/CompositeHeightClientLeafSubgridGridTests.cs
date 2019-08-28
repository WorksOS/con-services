using System;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Filters.Models;
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

      TestBinary_ReaderWriterHelper.RoundTripSerialise(clientGrid);
    }

    [Fact]
    public void AssignableFilteredValueIsNull()
    {
      var clientGrid = TestSubGrid();
      var passData = new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          Height = CellPassConsts.NullHeight
        }
      };

      clientGrid.AssignableFilteredValueIsNull(ref passData).Should().BeTrue();

      passData.FilteredPass.Height = 10.0f;

      clientGrid.AssignableFilteredValueIsNull(ref passData).Should().BeFalse();
    }

    [Fact]
    public void AssignFilteredValue_FailAsNotSupported()
    {
      var clientGrid = TestSubGrid();
      var context = new FilteredValueAssignmentContext
      {
        FilteredValue = new FilteredSinglePassInfo
        {
          FilteredPassData = new FilteredPassData
          {
            FilteredPass = new CellPass
            {
              Height = 123.4f
            }
          }
        }
      };

      Action act = () => clientGrid.AssignFilteredValue(0, 0, context);
      act.Should().Throw<TRexSubGridProcessingException>().WithMessage("Composite height sub grids don't define a filter value assignment behaviour");
    }

    [Fact]
    public void SetHeightsToNull()
    {
      var clientGrid = TestSubGrid();
      var nonNullValue = new SubGridCellCompositeHeightsRecord
      {
        LastHeight = 12.3f,
        HighestHeight = 23.4f,
        LowestHeight = 34.5f,
        FirstHeight = 45.6f
      };

      SubGridUtilities.SubGridDimensionalIterator((x, y) => clientGrid.Cells[x, y] = nonNullValue);

      clientGrid.SetHeightsToNull();

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        clientGrid.Cells[x, y].LastHeight = CellPassConsts.NullHeight;
        clientGrid.Cells[x, y].HighestHeight = CellPassConsts.NullHeight;
        clientGrid.Cells[x, y].LowestHeight = CellPassConsts.NullHeight;
        clientGrid.Cells[x, y].FirstHeight = CellPassConsts.NullHeight;
      });
    }
  }
}
