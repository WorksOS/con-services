using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class GenericClientLeafSubgridTests : IClassFixture<DILoggingFixture>
  {
    static int GetGridDataTypeCount() => Enum.GetValues(typeof(GridDataType)).Length;

    static readonly int GridDataTypeCount = GetGridDataTypeCount();

    private const int kGridDataTypeCount_Expected = 15;
    private const int kGridDataTypeCount = 29;

    /// <summary>
    /// Which grid data types have implementations that should be tested?
    /// </summary>
    /// <param name="gridDataType"></param>
    /// <returns></returns>
    private static bool Include(GridDataType gridDataType)
    {
      return gridDataType == GridDataType.Height ||
             gridDataType == GridDataType.HeightAndTime ||
             gridDataType == GridDataType.CompositeHeights ||
             gridDataType == GridDataType.CCV ||
             gridDataType == GridDataType.CCVPercentChange ||
             gridDataType == GridDataType.MDP ||
             gridDataType == GridDataType.MachineSpeed ||
             gridDataType == GridDataType.MachineSpeedTarget ||
             gridDataType == GridDataType.Temperature ||
             gridDataType == GridDataType.TemperatureDetail ||
             gridDataType == GridDataType.PassCount ||
             gridDataType == GridDataType.CellProfile ||
             gridDataType == GridDataType.CellPasses ||
             gridDataType == GridDataType.CCA ||
             gridDataType == GridDataType.CutFill;
    }

    /// <summary>
    /// Provides the list of grid data types the tests should apply to
    /// </summary>
    /// <param name="numTests"></param>
    /// <returns></returns>
    public static IEnumerable<object[]> ClientLeafDataTypes(int numTests)
    {
      var allData = (Enum.GetValues(typeof(GridDataType)) as int[]).Select(x => new object[] {(GridDataType) x, Include((GridDataType)x) }).ToList();

      return allData.Take(numTests);
    }

    /// <summary>
    /// Selects only the grid data types with expected == true from ClientLeafDataTypes
    /// </summary>
    /// <param name="numTests"></param>
    /// <returns></returns>
    public static IEnumerable<object[]> ClientLeafDataTypes_ExpectedOnly(int numTests)
    {
      var allData = (Enum.GetValues(typeof(GridDataType)) as int[]).Where(x => Include((GridDataType)x)).Select(x => new object[] { (GridDataType)x }).ToList();

      return allData.Take(numTests);
    }

    /// <summary>
    /// Fail if a new grid data type has been added to ensure tests are created for it
    /// </summary>
    [Fact]
    public void Test_GenericClientLeafSubgrid_EnsureExpectedGridDataTypes()
    {
      Assert.True(kGridDataTypeCount == GridDataTypeCount, $"{GridDataTypeCount} grid data types found, but {kGridDataTypeCount} were expected");
    }

    /// <summary>
    /// Fail if a new grid data type has been added to ensure tests are created for it
    /// </summary>
    [Fact]
    public void Test_GenericClientLeafSubgrid_EnsureThereAreNumberOfExpectedGridDataTypes()
    {
      Assert.True(kGridDataTypeCount_Expected == ClientLeafDataTypes_ExpectedOnly(100).Count(), $"Number of expected grid data types is not {kGridDataTypeCount_Expected} as expected");
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes), parameters: kGridDataTypeCount)]
    public void Test_GenericClientLeafSubgrid_Creation_EX(GridDataType gridDataType, bool expected)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      if (expected)
        Assert.NotNull(clientGrid);
      else
        Assert.Null(clientGrid);
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_ForEach_Ex(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      int Count = 0;

      clientGrid.ForEach((x, y) => Count++);
      Assert.True(SubGridTreeConsts.SubGridTreeCellsPerSubGrid == Count, "ForEach did not iterate all cells");
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_Clear_Ex(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);
      clientGrid.FillWithTestPattern();
      clientGrid.Clear();
      clientGrid.ForEach((x, y) =>
      {
        if (gridDataType != GridDataType.CCVPercentChange)
          Assert.True(!clientGrid.CellHasValue(x, y), "Clear() did not clear all cells");
      });
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_ReadWrite_Ex(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);
      clientGrid.FillWithTestPattern();
      byte[] bytes = clientGrid.ToBytes();
      Assert.True(bytes.Length > 0);

      var clientGrid2 = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);
      clientGrid2.FromBytes(bytes);

      Assert.True(clientGrid.LeafContentEquals(clientGrid2), "Client grids not equal after read/write serialisation");
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_Read_FailWithCorruptData(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      byte[] bytes = new byte[100]; // 100 zeros, which will cause the FromBytes call to fail with a TRexSubGridIOException on the grid data type
      Action act = () => clientGrid.FromBytes(bytes);
      act.Should().Throw<TRexSubGridIOException>().WithMessage("GridDataType in stream does not match GridDataType of local sub grid instance");
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_CellHasValue_True_Ex(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);
      clientGrid.FillWithTestPattern();

      clientGrid.ForEach((x, y) => Assert.True(clientGrid.CellHasValue(x, y), "Cell does not have value when it should"));
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_CellHasValue_False_Ex(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);
      clientGrid.ForEach((x, y) =>
      {
        if (gridDataType != GridDataType.CCVPercentChange)
          Assert.False(clientGrid.CellHasValue(x, y), "Cell does have value when it should not");
        else
          Assert.True(clientGrid.CellHasValue(x, y), "Cell does not have value when it should");
      });
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_Implements_IndicativeSizeInBytes(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      Assert.True(clientGrid.IndicativeSizeInBytes() > 0, "Indicative size in bytes is <= 0!");
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_Implements_AssignFromCachedPreProcessedClientSubgrid_FullMap(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      clientGrid.FillWithTestPattern();

      var clientGrid2 = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      clientGrid2.AssignFromCachedPreProcessedClientSubgrid(clientGrid, new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled));

      clientGrid.Should().BeEquivalentTo(clientGrid2);
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_Implements_AssignFromCachedPreProcessedClientSubgrid_PartialMap(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      clientGrid.FillWithTestPattern();

      var clientGrid2 = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      var filterMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled)
      {
        [0, 0] = true
      };

      clientGrid2.AssignFromCachedPreProcessedClientSubgrid(clientGrid, filterMap);

      // If we get here it's all good!
      Assert.True(true, "");
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_Implements_AssignFromCachedPreProcessedClientSubgrid2(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      clientGrid.AssignFromCachedPreProcessedClientSubgrid(clientGrid);

      // If we get here it's all good!
      Assert.True(true, "");
    }

    [Theory]
    [MemberData(nameof(ClientLeafDataTypes_ExpectedOnly), parameters: kGridDataTypeCount_Expected)]
    public void Test_GenericClientLeafSubgrid_Implements_DumpToLog(GridDataType gridDataType)
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(gridDataType);

      clientGrid.DumpToLog();

      // If we get here it's all good!
      Assert.True(true, "");
    }

    [Fact]
    public void Clone2DArray()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;

      clientGrid.ForEach((x, y) => clientGrid.Cells[x, y] = (float)(x + y));
      SubGridUtilities.SubGridDimensionalIterator((x, y) => clientGrid.Cells[x, y] = (float) (x + y));

      var clone = clientGrid.Clone2DArray();

      clientGrid.Cells.Should().BeEquivalentTo(clone);
      clientGrid.Cells.Should().NotBeSameAs(clone);
    }

    [Fact]
    public void ForEach_Action()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      clientGrid.ForEach((x, y, value) => clientGrid.Cells[x, y] = value + (float)(x + y));

      var clientGrid2 = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => clientGrid2.Cells[x, y] = (float)(x + y));
    }

    [Fact]
    public void ForEach_Func()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;

      double sum1 = 0;
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        clientGrid.Cells[x, y] = (float) (x + y);
        sum1 += x + y;
      });

      double sum2 = 0;

      // Iterate over all elements
      clientGrid.ForEach(value =>
      {
        sum2 += value;
        return true;
      });

      sum1.Should().Be(sum2);

      // Iterate ove only the first
      clientGrid.ForEach(value =>
      {
        sum2 = value;
        return false;
      });

      sum2.Should().Be(clientGrid.Cells[0, 0]);
    }
  }
}
