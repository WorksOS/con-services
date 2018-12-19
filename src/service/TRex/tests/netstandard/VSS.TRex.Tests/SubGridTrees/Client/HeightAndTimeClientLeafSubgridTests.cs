using System;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  /// <summary>
  /// Includes tests not covered in GenericClientLeafSibgriTests
  /// </summary>
  public class HeightAndTimeClientLeafSubgridTests
  {
    [Fact]
    public void Test_NullCells()
    {
      long minDateTime = DateTime.MinValue.ToBinary();

      var clientGrid = ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.HeightAndTime) as ClientHeightAndTimeLeafSubGrid;

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y] == Consts.NullHeight, "Cell not set to correct null value"));
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Times[x, y] == minDateTime, "Cell time not set to correct null value"));
    }

    [Fact]
    public void Test_NullCell()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.HeightAndTime) as ClientHeightAndTimeLeafSubGrid;

      clientGrid.Cells[0, 0] = clientGrid.NullCell();
      Assert.False(clientGrid.CellHasValue(0, 0), "Cell not set to correct null value");
    }
  }
}
