using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Filters.Models;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  /// <summary>
  /// Includes tests not covered in GenericClientLeafSubGridTests
  /// </summary>
  public class ClientMachineSpeedLeafSubGridTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_NullCells()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MachineSpeed) as ClientMachineSpeedLeafSubGrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y] == Consts.NullMachineSpeed, "Cell not set to correct null value"));
    }

    [Fact]
    public void Test_NullCell()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MachineSpeed) as ClientMachineSpeedLeafSubGrid;

      clientGrid.Cells[0, 0] = clientGrid.NullCell();
      Assert.False(clientGrid.CellHasValue(0, 0), "Cell not set to correct null value");
    }

    [Fact]
    public void Test_IndicativeSize()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MachineSpeed) as ClientMachineSpeedLeafSubGrid;

      int expectedSize = 2 * (4 * 32); // 2 bit mask subgrids in this and parent class
      int actualSize = clientGrid.IndicativeSizeInBytes();

      Assert.True(actualSize == expectedSize, $"IndicativeSize() incorrect, = {clientGrid.IndicativeSizeInBytes()}, expected = {expectedSize}");
    }

    [Fact]
    public void DumpToLog()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MachineSpeed) as ClientMachineSpeedLeafSubGrid;
      clientGrid.DumpToLog(clientGrid.ToString());
    }

    [Fact]
    public void AssignableFilteredValueIsNull()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MachineSpeed) as ClientMachineSpeedLeafSubGrid;
      var passData = new FilteredPassData
      {
        FilteredPass = new CellPass
        {
          MachineSpeed = CellPassConsts.NullMachineSpeed
        }
      };

      clientGrid.AssignableFilteredValueIsNull(ref passData).Should().BeTrue();

      passData.FilteredPass.MachineSpeed = 100;

      clientGrid.AssignableFilteredValueIsNull(ref passData).Should().BeFalse();
    }
  }
}
