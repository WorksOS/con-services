using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Filters.Models;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Tests.TestFixtures;
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

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Temperature) as ClientTemperatureLeafSubGrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y].Equals(cell)));
    }

    [Fact]
    public void Test_NullCell()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Temperature) as ClientTemperatureLeafSubGrid;

      clientGrid.Cells[0, 0] = clientGrid.NullCell();
      Assert.False(clientGrid.CellHasValue(0, 0), "Cell not set to correct null value");
    }

    [Fact]
    public void DumpToLog()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Temperature) as ClientTemperatureLeafSubGrid;
      clientGrid.DumpToLog(clientGrid.ToString());
    }

    [Fact]
    public void AssignableFilteredValueIsNull()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Temperature) as ClientTemperatureLeafSubGrid;
 
      var passData = new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          MaterialTemperature = CellPassConsts.NullMaterialTemperatureValue
        }
      };

      clientGrid.AssignableFilteredValueIsNull(ref passData).Should().BeTrue();

      passData.FilteredPass.MaterialTemperature = 100;

      clientGrid.AssignableFilteredValueIsNull(ref passData).Should().BeFalse();
    }
  }
}
