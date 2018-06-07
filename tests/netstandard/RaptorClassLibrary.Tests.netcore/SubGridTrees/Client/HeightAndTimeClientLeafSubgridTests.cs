using System;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Utilities;
using Xunit;

namespace VSS.TRex.Tests
{
  public class HeightAndTimeClientLeafSubgridTests
  {
    [Fact()]

    public void Test_HeightAndTimeClientLeafSubgrid_Creation()
    {
        var clientGrid = new ClientHeightAndTimeLeafSubGrid(null, null, SubGridTree.SubGridTreeLevels, 1.0, SubGridTree.DefaultIndexOriginOffset);
        Assert.NotNull(clientGrid);
    }

    [Fact]
    public void Test_NullCells()
    {
      long minDateTime = DateTime.MinValue.ToBinary();
      var clientGrid = new ClientHeightAndTimeLeafSubGrid(null, null, SubGridTree.SubGridTreeLevels, 1.0, SubGridTree.DefaultIndexOriginOffset);
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y] == Consts.NullHeight, "Cell not set to correct null value"));
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Times[x, y] == minDateTime, "Cell time not set to correct null value"));
    }
  }
}
