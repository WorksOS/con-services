using System;
using VSS.TRex.Analytics.MDPStatistics;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.MDPStatistics
{
  public class MDPAggregatorTests : BaseTests
  {
    [Fact]
    public void Test_MDPAggregator_Creation()
    {
      var aggregator = new MDPStatisticsAggregator();

      Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
      Assert.True(aggregator.CellSize < Consts.TOLERANCE_DIMENSION, "Invalid initial value for CellSize.");
      Assert.True(aggregator.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
      Assert.True(aggregator.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
      Assert.True(!aggregator.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
      Assert.True(!aggregator.OverrideMachineMDP, "Invalid initial value for OverrideTemperatureWarningLevels.");
      Assert.True(aggregator.OverridingMachineMDP == CellPassConsts.NullMDP, "Invalid initial value for OverridingMachineMDP.");
      Assert.True(aggregator.LastTargetMDP == CellPassConsts.NullMDP, "Invalid initial value for LastTargetMDP.");
    }

    [Fact]
    public void Test_MDPAggregator_ProcessResult_NoAggregation()
    {
      var aggregator = new MDPStatisticsAggregator();

      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.MDP) as ClientMDPLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = CELL_SIZE;
      aggregator.OverrideMachineMDP = true;
      aggregator.OverridingMachineMDP = (short)(length - 1);
      aggregator.MDPPercentageRange = new MDPRangePercentageRecord(100, 100);

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubgridResult(subGrids);

      Assert.True(aggregator.SummaryCellsScanned == dLength, "Invalid value for SummaryCellsScanned.");
      Assert.True(Math.Abs(aggregator.SummaryProcessedArea - dLength * Math.Pow(aggregator.CellSize, 2)) < Consts.TOLERANCE_DIMENSION, "Invalid value for SummaryProcessedArea.");
      Assert.True(aggregator.CellsScannedAtTarget == length, "Invalid value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == dLength - length, "Invalid value for CellsScannedUnderTarget.");
    }

    [Fact]
    public void Test_MDPAggregator_ProcessResult_WithAggregation()
    {
      var aggregator = new MDPStatisticsAggregator();

      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.MDP) as ClientMDPLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = CELL_SIZE;
      aggregator.OverrideMachineMDP = true;
      aggregator.OverridingMachineMDP = (short)(length - 1);
      aggregator.MDPPercentageRange = new MDPRangePercentageRecord(100, 100);

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubgridResult(subGrids);

      // Other aggregator...
      var otherAggregator = new MDPStatisticsAggregator();

      otherAggregator.CellSize = CELL_SIZE;
      otherAggregator.OverrideMachineMDP = true;
      otherAggregator.OverridingMachineMDP = (short)(length - 1);
      otherAggregator.MDPPercentageRange = new MDPRangePercentageRecord(100, 100);

      otherAggregator.ProcessSubgridResult(subGrids);

      aggregator.AggregateWith(otherAggregator);

      Assert.True(aggregator.SummaryCellsScanned == dLength * 2, "Invalid value for SummaryCellsScanned.");
      Assert.True(Math.Abs(aggregator.SummaryProcessedArea - 2 * dLength * Math.Pow(aggregator.CellSize, 2)) < Consts.TOLERANCE_DIMENSION, "Invalid value for SummaryProcessedArea.");
      Assert.True(aggregator.CellsScannedAtTarget == length * 2, "Invalid value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == (dLength - length) * 2, "Invalid value for CellsScannedUnderTarget.");
    }
  }
}
