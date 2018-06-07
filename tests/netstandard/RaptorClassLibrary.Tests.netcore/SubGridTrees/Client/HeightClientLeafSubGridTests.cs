using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Tests.netcore.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests
{
  public class HeightClientLeafSubGridTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_HeightClientLeafSubGridTests_Creation()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      Assert.NotNull(clientGrid);
    }

    [Fact]
    public void Test_NullCells()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y] == Consts.NullHeight, "Cell not set to correct null value"));
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_HeightClientLeafSubGridTests_Assign()
    {
      Assert.True(false, "Not implemented");
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_HeightClientLeafSubGridTests_AssignableFilteredValueIsNull()
    {
      Assert.True(false, "Not implemented");
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_HeightClientLeafSubGridTests_CellHasValue()
    {
      Assert.True(false, "Not implemented");
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_HeightClientLeafSubGridTests_Clear()
    {
      Assert.True(false, "Not implemented");
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_HeightClientLeafSubGridTests_DumpToLog()
    {
      Assert.True(false, "Not implemented");
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_HeightClientLeafSubGridTests_Read()
    {
      Assert.True(false, "Not implemented");
    }

    [Fact(Skip = "Not Implemented")]
    public void Test_HeightClientLeafSubGridTests_Write()
    {
      Assert.True(false, "Not implemented");
    }

    [Fact]
    public void Test_HeightClientLeafSubGridTests_SetToZero()
    {
      ClientHeightLeafSubGrid subgrid = new ClientHeightLeafSubGrid(null, null, SubGridTree.SubGridTreeLevels, 1, SubGridTree.DefaultIndexOriginOffset);

      subgrid.SetToZeroHeight();

      Assert.Equal((uint) subgrid.CountNonNullCells(), SubGridTree.CellsPerSubgrid);

      int NumEqualZero = 0;
      ClientHeightLeafSubGrid.ForEach((x, y) =>
      {
        if (subgrid.Cells[x, y] == 0.0) NumEqualZero++;
      });

      Assert.Equal((uint) NumEqualZero, SubGridTree.CellsPerSubgrid);
    }
  }
}
