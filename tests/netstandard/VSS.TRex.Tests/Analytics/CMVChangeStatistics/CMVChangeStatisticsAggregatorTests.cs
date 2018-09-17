using System;
using System.Linq;
using VSS.TRex.Analytics.CMVChangeStatistics;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CMVChangeStatistics
{
  public class CMVChangeStatisticsAggregatorTests : BaseTests
  {
    [Fact]
    public void Test_CMVStatisticsAggregator_Creation()
    {
      var aggregator = new CMVChangeStatisticsAggregator();

      Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
      Assert.True(aggregator.CellSize < Consts.TOLERANCE_DIMENSION, "Invalid initial value for CellSize.");
      Assert.True(aggregator.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
      Assert.True(aggregator.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
      Assert.True(!aggregator.MissingTargetValue, "Invalid initial value for MissingTargetValue.");

      Assert.True(aggregator.CMVChangeDetailsDataValues == null, "Invalid initial value for DetailsDataValues.");
      Assert.True(aggregator.Counts == null, "Invalid initial value for Counts.");
    }

    [Fact]
    public void Test_CMVStatisticsAggregator_ProcessResult_NoAggregation()
    {
      var aggregator = new CMVChangeStatisticsAggregator();

      var clientGrid = new ClientCMVLeafSubGrid();

      clientGrid.FillWithTestPattern();

      aggregator.CellSize = CELL_SIZE;
      aggregator.CMVChangeDetailsDataValues = new[] { -100.0, 0.0, 100.0 };
      aggregator.Counts = new long[aggregator.CMVChangeDetailsDataValues.Length];

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubgridResult(subGrids);

      Assert.True(aggregator.Counts.Length == aggregator.CMVChangeDetailsDataValues.Length, "Invalid value for CMVChangeDetailsDataValues.");

      for (int i = 0; i < aggregator.Counts.Length; i++)
        Assert.True(aggregator.Counts[i] > 0, $"Invalid value for Counts[{i}].");

      Assert.True(aggregator.Counts.Sum() == aggregator.SummaryCellsScanned, "Invalid value for total number of processed cells.");
    }

    [Fact]
    public void Test_CMVStatisticsAggregator_ProcessResult_WithAggregation_Details()
    {
      var aggregator = new CMVChangeStatisticsAggregator();

      var clientGrid = new ClientCMVLeafSubGrid();

      clientGrid.FillWithTestPattern();

      aggregator.CellSize = CELL_SIZE;
      aggregator.CMVChangeDetailsDataValues = new[] { -100.0, 0.0, 100.0 };
      aggregator.Counts = new long[aggregator.CMVChangeDetailsDataValues.Length];

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubgridResult(subGrids);

      // Other aggregator...
      var otherAggregator = new CMVChangeStatisticsAggregator();

      otherAggregator.CellSize = CELL_SIZE;
      otherAggregator.CMVChangeDetailsDataValues = new[] { -100.0, 0.0, 100.0 };
      otherAggregator.Counts = new long[aggregator.CMVChangeDetailsDataValues.Length];

      otherAggregator.ProcessSubgridResult(subGrids);

      aggregator.AggregateWith(otherAggregator);

      Assert.True(aggregator.Counts.Length == aggregator.CMVChangeDetailsDataValues.Length, "Invalid value for DetailsDataValues.");
      for (int i = 0; i < aggregator.Counts.Length; i++)
        Assert.True(aggregator.Counts[i] == otherAggregator.Counts[i] * 2, $"Invalid aggregated value for Counts[{i}].");

      Assert.True(aggregator.Counts.Sum() == aggregator.SummaryCellsScanned, "Invalid value for total number of processed cells.");
    }
  }
}
