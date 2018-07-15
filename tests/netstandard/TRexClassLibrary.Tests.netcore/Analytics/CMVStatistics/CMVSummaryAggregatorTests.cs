using System;
using VSS.TRex.Tests.netcore.Analytics.Common;
using VSS.TRex.Analytics.CMVStatistics.Summary;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CMVStatistics
{
  public class CMVSummaryAggregatorTests : BaseTests
  {
    [Fact]
    public void Test_CMVSummaryAggregator_Creation()
    {
      var aggregator = new CMVSummaryAggregator();

      Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
      Assert.True(aggregator.CellSize < Consts.TOLERANCE_DIMENSION, "Invalid initial value for CellSize.");
      Assert.True(aggregator.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
      Assert.True(aggregator.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
      Assert.True(!aggregator.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
      Assert.True(!aggregator.OverrideMachineCMV, "Invalid initial value for OverrideTemperatureWarningLevels.");
      Assert.True(aggregator.OverridingMachineCMV == CellPass.NullCCV, "Invalid initial value for OverridingMachineCMV.");
      Assert.True(aggregator.LastTargetCMV == CellPass.NullCCV, "Invalid initial value for LastTargetCMV.");
    }

    [Fact]
    public void Test_CMVSummaryAggregator_ProcessResult_NoAggregation()
    {
      var aggregator = new CMVSummaryAggregator();

      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.CCV) as ClientCMVLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short) Math.Sqrt(dLength);
      aggregator.CellSize = CELL_SIZE;
      aggregator.OverrideMachineCMV = true;
      aggregator.OverridingMachineCMV = (short) (length - 1);
      aggregator.CMVPercentageRange = new CMVRangePercentageRecord(100, 100);

      IClientLeafSubGrid[][] subGrids = new [] { new [] { clientGrid } };
      
      aggregator.ProcessSubgridResult(subGrids);

      Assert.True(aggregator.SummaryCellsScanned == dLength, "Invalid value for SummaryCellsScanned.");
      Assert.True(Math.Abs(aggregator.SummaryProcessedArea - dLength * Math.Pow(aggregator.CellSize, 2)) < Consts.TOLERANCE_DIMENSION, "Invalid value for SummaryProcessedArea.");
      Assert.True(aggregator.CellsScannedAtTarget == length, "Invalid value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == dLength - length, "Invalid value for CellsScannedUnderTarget.");
    }

    [Fact]
    public void Test_CMVSummaryAggregator_ProcessResult_WithAggregation()
    {
      var aggregator = new CMVSummaryAggregator();

      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.CCV) as ClientCMVLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = CELL_SIZE;
      aggregator.OverrideMachineCMV = true;
      aggregator.OverridingMachineCMV = (short)(length - 1);
      aggregator.CMVPercentageRange = new CMVRangePercentageRecord(100, 100);

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubgridResult(subGrids);

      // Other aggregator...
      var otherAggregator = new CMVSummaryAggregator();

      otherAggregator.CellSize = CELL_SIZE;
      otherAggregator.OverrideMachineCMV = true;
      otherAggregator.OverridingMachineCMV = (short)(length - 1);
      otherAggregator.CMVPercentageRange = new CMVRangePercentageRecord(100, 100);

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
