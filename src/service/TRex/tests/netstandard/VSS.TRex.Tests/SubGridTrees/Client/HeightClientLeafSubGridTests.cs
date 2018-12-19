using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Filters.Models;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  /// <summary>
  /// Includes tests not covered in GenericClientLeafSubgridTests
  /// </summary>
  public class HeightClientLeafSubGridTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_NullCells()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y] == Consts.NullHeight, "Cell not set to correct null value"));
    }

    [Fact]
    public void Test_NullCell()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;

      clientGrid.Cells[0, 0] = clientGrid.NullCell();
      Assert.False(clientGrid.CellHasValue(0, 0), "Cell not set to correct null value");
    }

    /// <summary>
    /// Tests the assignation of a height and time leaf subgrid to a height subgrid
    /// </summary>
    [Fact(Skip = "Not Implemented")]
    public void Test_HeightClientLeafSubGridTests_Assign()
    {
      Assert.True(false, "Not implemented");
    }

    [Fact]
    public void Test_HeightClientLeafSubGridTests_AssignableFilteredValueIsNull_True()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      var passData = new FilteredPassData {FilteredPass = new CellPass {Height = Consts.NullHeight}};

      Assert.True(clientGrid.AssignableFilteredValueIsNull(ref passData), "Filtered value stated as not null when it is");
    }

    [Fact]
    public void Test_HeightClientLeafSubGridTests_AssignableFilteredValueIsNull_False()
    {
      var clientGrid = ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      var passData = new FilteredPassData { FilteredPass = new CellPass { Height = 42.0f } };

      Assert.False(clientGrid.AssignableFilteredValueIsNull(ref passData), "Filtered value stated as null when it is not");
    }

    [Fact]
    public void Test_HeightClientLeafSubGridTests_SetToZero()
    {
      ClientHeightLeafSubGrid subgrid = new ClientHeightLeafSubGrid(null, null, SubGridTreeConsts.SubGridTreeLevels, 1, SubGridTreeConsts.DefaultIndexOriginOffset);

      subgrid.SetToZeroHeight();

      Assert.Equal((uint) subgrid.CountNonNullCells(), SubGridTreeConsts.CellsPerSubgrid);

      int NumEqualZero = 0;
      ClientHeightLeafSubGrid.ForEachStatic((x, y) =>
      {
        if (subgrid.Cells[x, y] == 0.0) NumEqualZero++;
      });

      Assert.Equal((uint) NumEqualZero, SubGridTreeConsts.CellsPerSubgrid);
    }
  }
}
