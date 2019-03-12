using System;
using VSS.TRex.Analytics.ElevationStatistics;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.ElevationStatistics
{
  public class ElevationStatisticsAggregatorTests : BaseTests
  {
    private const byte MAX_ELEVATION = 62;

    [Fact]
    public void Test_ElevationStatisticsAggregator_Creation()
    {
      var aggregator = new ElevationStatisticsAggregator();

      Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
      Assert.True(aggregator.CellSize < Consts.TOLERANCE_DIMENSION, "Invalid initial value for CellSize.");
      Assert.True(Math.Abs(aggregator.MinElevation - Consts.INITIAL_ELEVATION) < Consts.TOLERANCE_HEIGHT, "Invalid initial value for MinElevation.");
      Assert.True(Math.Abs(aggregator.MaxElevation + Consts.INITIAL_ELEVATION) < Consts.TOLERANCE_HEIGHT, "Invalid initial value for MaxElevation.");
      Assert.True(aggregator.CellsUsed == 0, "Invalid initial value for CellsUsed.");
      Assert.True(aggregator.CellsScanned == 0, "Invalid initial value for CellsScanned.");
      Assert.True(Math.Abs(aggregator.BoundingExtents.MinX - Consts.MAX_RANGE) < Consts.TOLERANCE_DIMENSION, "Invalid initial value for BoundingExtents.MinX.");
      Assert.True(Math.Abs(aggregator.BoundingExtents.MinY - Consts.MAX_RANGE) < Consts.TOLERANCE_DIMENSION, "Invalid initial value for BoundingExtents.MinY.");
      Assert.True(Math.Abs(aggregator.BoundingExtents.MinZ - Consts.MAX_RANGE) < Consts.TOLERANCE_HEIGHT, "Invalid initial value for BoundingExtents.MinZ.");
      Assert.True(Math.Abs(aggregator.BoundingExtents.MaxX - Consts.MIN_RANGE) < Consts.TOLERANCE_DIMENSION, "Invalid initial value for BoundingExtents.MaxX.");
      Assert.True(Math.Abs(aggregator.BoundingExtents.MaxY - Consts.MIN_RANGE) < Consts.TOLERANCE_DIMENSION, "Invalid initial value for BoundingExtents.MaxY.");
      Assert.True(Math.Abs(aggregator.BoundingExtents.MaxZ - Consts.MIN_RANGE) < Consts.TOLERANCE_HEIGHT, "Invalid initial value for BoundingExtents.MaxZ.");
    }

    [Fact]
    public void Test_ElevationStatisticsAggregator_ProcessResult_NoAggregation()
    {
      var aggregator = new ElevationStatisticsAggregator();

      var clientGrid = new ClientHeightLeafSubGrid();

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      aggregator.CellSize = CELL_SIZE;

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubGridResult(subGrids);

      Assert.True(aggregator.CellsScanned == dLength, "Invalid value for CellsScanned.");
      Assert.True(aggregator.CellsUsed == dLength, "Invalid value for CellsUsed.");
      Assert.True(Math.Abs(aggregator.CoverageArea - dLength * Math.Pow(aggregator.CellSize, 2)) < Consts.TOLERANCE_AREA, "Invalid value for CoverageArea.");
      Assert.True(Math.Abs(aggregator.MinElevation) < Consts.TOLERANCE_HEIGHT, "Invalid value for MinElevation.");
      Assert.True(Math.Abs(aggregator.MaxElevation - MAX_ELEVATION) < Consts.TOLERANCE_HEIGHT, "Invalid value for MaxElevation.");
    }

    [Fact]
    public void Test_ElevationAggregator_ProcessResult_WithAggregation()
    {
      var aggregator = new ElevationStatisticsAggregator();

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      aggregator.CellSize = CELL_SIZE;

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubGridResult(subGrids);

      // Other aggregator...
      var otherAggregator = new ElevationStatisticsAggregator();

      otherAggregator.CellSize = CELL_SIZE;
      
      otherAggregator.ProcessSubGridResult(subGrids);

      aggregator.AggregateWith(otherAggregator);

      Assert.True(aggregator.CellsScanned == dLength * 2, "Invalid value for CellsScanned.");
      Assert.True(aggregator.CellsUsed == dLength * 2, "Invalid value for CellsUsed.");
      Assert.True(Math.Abs(aggregator.CoverageArea - 2 * dLength * Math.Pow(aggregator.CellSize, 2)) < Consts.TOLERANCE_AREA, "Invalid value for CoverageArea.");
      Assert.True(Math.Abs(aggregator.MinElevation) < Consts.TOLERANCE_HEIGHT, "Invalid value for MinElevation.");
      Assert.True(Math.Abs(aggregator.MaxElevation - MAX_ELEVATION) < Consts.TOLERANCE_HEIGHT, "Invalid value for MaxElevation.");
    }
  }
}
