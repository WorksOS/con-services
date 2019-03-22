using System;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Filters.Models;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  /// <summary>
  /// Includes tests not covered in GenericClientLeafSubgridTests
  /// </summary>
  public class HeightClientLeafSubGridTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_NullCells()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y] == Consts.NullHeight, "Cell not set to correct null value"));
    }

    [Fact]
    public void Test_NullCell()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;

      clientGrid.Cells[0, 0] = clientGrid.NullCell();
      Assert.False(clientGrid.CellHasValue(0, 0), "Cell not set to correct null value");
    }

    /// <summary>
    /// Tests the assignation of a height and time leaf sub grid to a height sub grid
    /// </summary>
    [Fact]
    public void Test_HeightClientLeafSubGridTests_Assign_FromHeightAndTime()
    {
      var clientGridHeight = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      var clientGridHeightAndTime = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.HeightAndTime) as ClientHeightAndTimeLeafSubGrid;

      // Fill in the height and time grid
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        clientGridHeightAndTime.Times[x, y] = DateTime.UtcNow.Ticks;
        clientGridHeightAndTime.Cells[x, y] = x + y;
      });

      clientGridHeight.Assign(clientGridHeightAndTime);

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        clientGridHeight.Cells[x, y].Should().Be(x + y);
      });
    }

    [Fact]
    public void Test_HeightClientLeafSubGridTests_Assign_FromHeight()
    {
      var clientGridHeight = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      var clientGridHeight2 = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;

      // Fill in the height and time grid
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        clientGridHeight.Cells[x, y] = x + y;
      });

      clientGridHeight.Assign(clientGridHeight);

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        clientGridHeight.Cells[x, y].Should().Be(x + y);
      });
    }

    [Fact]
    public void Test_HeightClientLeafSubGridTests_AssignableFilteredValueIsNull_True()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      var passData = new FilteredPassData {FilteredPass = new CellPass {Height = Consts.NullHeight}};

      Assert.True(clientGrid.AssignableFilteredValueIsNull(ref passData), "Filtered value stated as not null when it is");
    }

    [Fact]
    public void Test_HeightClientLeafSubGridTests_AssignableFilteredValueIsNull_False()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      var passData = new FilteredPassData { FilteredPass = new CellPass { Height = 42.0f } };

      Assert.False(clientGrid.AssignableFilteredValueIsNull(ref passData), "Filtered value stated as null when it is not");
    }

    [Fact]
    public void Test_HeightClientLeafSubGridTests_SetToZero()
    {
      ClientHeightLeafSubGrid subgrid = new ClientHeightLeafSubGrid(null, null, SubGridTreeConsts.SubGridTreeLevels, 1, SubGridTreeConsts.DefaultIndexOriginOffset);

      subgrid.SetToZeroHeight();

      Assert.Equal((uint) subgrid.CountNonNullCells(), SubGridTreeConsts.CellsPerSubGrid);

      int NumEqualZero = 0;
      ClientHeightLeafSubGrid.ForEachStatic((x, y) =>
      {
        if (subgrid.Cells[x, y] == 0.0) NumEqualZero++;
      });

      Assert.Equal((uint) NumEqualZero, SubGridTreeConsts.CellsPerSubGrid);
    }

    [Fact]
    public void DumpToLog()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      clientGrid.DumpToLog(clientGrid.ToString());
    }
  }
}
