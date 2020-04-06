using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Records;
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
  public class MachineTargetSpeedClientLeafSubGridTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_NullCells()
    {
      var cell = new MachineSpeedExtendedRecord();
      cell.Clear();

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MachineSpeedTarget) as ClientMachineTargetSpeedLeafSubGrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y].Equals(cell)));
    }

    [Fact]
    public void Test_NullCell()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MachineSpeedTarget) as ClientMachineTargetSpeedLeafSubGrid;

      clientGrid.Cells[0, 0] = clientGrid.NullCell();
      Assert.False(clientGrid.CellHasValue(0, 0), "Cell not set to correct null value");
    }


    [Fact]
    public void DumpToLog()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MachineSpeedTarget) as ClientMachineTargetSpeedLeafSubGrid;
      clientGrid.DumpToLog(clientGrid.ToString());
    }


    [Fact]
    public void AssignableFilteredValueIsNull()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MachineSpeedTarget) as ClientMachineTargetSpeedLeafSubGrid;

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
