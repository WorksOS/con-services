using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  /// <summary>
  /// Includes tests not covered in GenericClientLeafSibgriTests
  /// </summary>
  public class PassClountClientLeafSubGridTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_NullCells()
    {
      var cell = new SubGridCellPassDataPassCountEntryRecord();
      cell.Clear();

      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.PassCount) as ClientPassCountLeafSubGrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y].Equals(cell)));
    }
  }
}
