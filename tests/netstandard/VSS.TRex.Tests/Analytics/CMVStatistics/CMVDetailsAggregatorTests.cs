using System;
using VSS.TRex.Analytics.CMVStatistics.Details;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CMVStatistics
{
  public class CMVDetailsAggregatorTests : BaseTests
  {
    [Fact]
    public void Test_CMVDetailsAggregator_Creation()
    {
      var aggregator = new CMVDetailsAggregator();

      Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
      Assert.True(aggregator.DetailsDataValues == null, "Invalid initial value for DetailsDataValues.");
      Assert.True(aggregator.Counts == null, "Invalid initial value for Counts.");
    }

    [Fact]
    public void Test_CMVDetailsAggregator_ProcessResult_NoAggregation()
    {
      var aggregator = new CMVDetailsAggregator();
      var clientGrid = new ClientCMVLeafSubGrid(); 

      clientGrid.FillWithTestPattern();

      aggregator.CellSize = CELL_SIZE;
      aggregator.DetailsDataValues = new[] { 1, 5, 10, 15, 20, 25, 31 };
      aggregator.Counts = new long[aggregator.DetailsDataValues.Length];

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubgridResult(subGrids);

      Assert.True(aggregator.Counts.Length == aggregator.DetailsDataValues.Length, "Invalid value for DetailsDataValues.");
      for (int i = 0; i < aggregator.Counts.Length; i++)
        Assert.True(aggregator.Counts[i] > 0, $"Invalid aggregated value for Counts[{i}].");
    }

    [Fact]
    public void Test_CMVDetailsAggregator_ProcessResult_WithAggregation()
    {
      var aggregator = new CMVDetailsAggregator();
      var clientGrid = new ClientCMVLeafSubGrid();

      clientGrid.FillWithTestPattern();

      aggregator.CellSize = CELL_SIZE;
      aggregator.DetailsDataValues = new[] { 1, 5, 10, 15, 20, 25, 31 };
      aggregator.Counts = new long[aggregator.DetailsDataValues.Length];

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubgridResult(subGrids);

      // Other aggregator...
      var otherAggregator = new CMVDetailsAggregator();

      otherAggregator.CellSize = CELL_SIZE;
      otherAggregator.DetailsDataValues = new[] { 1, 5, 10, 15, 20, 25, 31 };
      otherAggregator.Counts = new long[aggregator.DetailsDataValues.Length];

      otherAggregator.ProcessSubgridResult(subGrids);

      aggregator.AggregateWith(otherAggregator);

      Assert.True(aggregator.Counts.Length == aggregator.DetailsDataValues.Length, "Invalid value for DetailsDataValues.");
      for (int i = 0; i < aggregator.Counts.Length; i++)
        Assert.True(aggregator.Counts[i] == otherAggregator.Counts[i] * 2, $"Invalid aggregated value for Counts[{i}].");
    }
  }
}
