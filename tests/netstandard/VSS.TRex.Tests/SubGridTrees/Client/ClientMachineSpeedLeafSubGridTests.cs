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
    public void Test_IndicativeSize()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.MachineSpeed) as ClientMachineSpeedLeafSubGrid;

      Assert.True(ClientMachineSpeedLeafSubGrid.LayoutSize == 0, $"LayoutSize incorrect, = {ClientMachineSpeedLeafSubGrid.LayoutSize}");

      int expectedSize = 3 * (4 * 32) + ClientMachineSpeedLeafSubGrid.LayoutSize; // 3 bit mask subgrids in this and parent class

      Assert.True(clientGrid.IndicativeSizeInBytes() == expectedSize,
        $"IndicativeSize() incorrect, = {clientGrid.IndicativeSizeInBytes()}, expect = {expectedSize}");
    }

  }
}
