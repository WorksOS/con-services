using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Filters;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Tests.netcore.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests
{
  /// <summary>
  /// Includes tests not covered in GenericClientLeafSibgriTests
  /// </summary>
  public class HeightClientLeafSubGridTests : IClassFixture<DILoggingFixture>
  {
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

    [Fact]
    public void Test_HeightClientLeafSubGridTests_AssignableFilteredValueIsNull_True()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      var passData = new FilteredPassData {FilteredPass = new CellPass {Height = Consts.NullHeight}};

      Assert.True(clientGrid.AssignableFilteredValueIsNull(ref passData), "Filtered value stated as not null when it is");
    }

    [Fact]
    public void Test_HeightClientLeafSubGridTests_AssignableFilteredValueIsNull_False()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      var passData = new FilteredPassData { FilteredPass = new CellPass { Height = 42.0f } };

      Assert.False(clientGrid.AssignableFilteredValueIsNull(ref passData), "Filtered value stated as null when it is not");
    }

    [Fact]
    public void Test_HeightClientLeafSubGridTests_CellHasValue_False()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;

      // Initial state of cell should be null
      Assert.False(clientGrid.CellHasValue(0, 0), "Cell has a value when it should not");
    }

    [Fact]
    public void Test_HeightClientLeafSubGridTests_CellHasValue_True()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;

      clientGrid.Cells[0, 0] = 42.0f;
      // Initial state of cell should be null
      Assert.True(clientGrid.CellHasValue(0, 0), "Cell has no value when it should");
    }

    [Fact]
    public void Test_HeightClientLeafSubGridTests_Clear()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;

      SubGridUtilities.SubGridDimensionalIterator((x, y) => clientGrid.Cells[x, y] = 42.0f);

      clientGrid.Clear();

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y] == Consts.NullHeight, "Cell not cleared"));
    }

    [Fact]
    public void Test_HeightClientLeafSubGridTests_SetToZero()
    {
      ClientHeightLeafSubGrid subgrid = new ClientHeightLeafSubGrid(null, null, SubGridTree.SubGridTreeLevels, 1, SubGridTree.DefaultIndexOriginOffset);

      subgrid.SetToZeroHeight();

      Assert.Equal((uint) subgrid.CountNonNullCells(), SubGridTree.CellsPerSubgrid);

      int NumEqualZero = 0;
      ClientHeightLeafSubGrid.ForEachStatic((x, y) =>
      {
        if (subgrid.Cells[x, y] == 0.0) NumEqualZero++;
      });

      Assert.Equal((uint) NumEqualZero, SubGridTree.CellsPerSubgrid);
    }
  }
}
