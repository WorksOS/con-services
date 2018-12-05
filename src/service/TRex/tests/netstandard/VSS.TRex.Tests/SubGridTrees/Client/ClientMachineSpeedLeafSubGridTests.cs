using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  /// <summary>
  /// Includes tests not covered in GenericClientLeafSubgridTests
  /// </summary>
  public class ClientMachineSpeedLeafSubGridTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_NullCells()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MachineSpeed) as ClientMachineSpeedLeafSubGrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y] == Consts.NullMachineSpeed, "Cell not set to correct null value"));
    }

    [Fact]
    public void Test_NullCell()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MachineSpeed) as ClientMachineSpeedLeafSubGrid;

      clientGrid.Cells[0, 0] = clientGrid.NullCell();
      Assert.False(clientGrid.CellHasValue(0, 0), "Cell not set to correct null value");
    }

    [Fact]
    public void Test_IndicativeSize()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MachineSpeed) as ClientMachineSpeedLeafSubGrid;

      int expectedSize = 2 * (4 * 32); // 2 bit mask subgrids in this and parent class
      int actualSize = clientGrid.IndicativeSizeInBytes();

      Assert.True(actualSize == expectedSize, $"IndicativeSize() incorrect, = {clientGrid.IndicativeSizeInBytes()}, expected = {expectedSize}");
    }    
  }
}
