using System;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.netcore.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.PassCountStatistics
{
  public class PassCountAggregatorTests : BaseTests
  {
    [Fact]
    public void Test_PassCountAggregator_Creation()
    {
      var aggregator = new PassCountAggregator();

      Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
      Assert.True(aggregator.CellSize < Consts.TOLERANCE_DIMENSION, "Invalid initial value for CellSize.");
      Assert.True(aggregator.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
      Assert.True(aggregator.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
      Assert.True(!aggregator.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
      Assert.True(!aggregator.OverrideTargetPassCount, "Invalid initial value for OverrideTargetPassCount.");
      Assert.True(aggregator.OverridingTargetPassCountRange.Min == CellPass.NullPassCountValue, "Invalid initial value for OverridingTargetPassCountRange.Min.");
      Assert.True(aggregator.OverridingTargetPassCountRange.Max == CellPass.NullPassCountValue, "Invalid initial value for OverridingTargetPassCountRange.Max.");
      Assert.True(aggregator.LastPassCountTargetRange.Min == CellPass.NullPassCountValue, "Invalid initial value for LastPassCountTargetRange.Min.");
      Assert.True(aggregator.LastPassCountTargetRange.Max == CellPass.NullPassCountValue, "Invalid initial value for LastPassCountTargetRange.Max.");
    }

    [Fact]
    public void Test_PassCountAggregator_ProcessResult_NoAggregation()
    {
      var aggregator = new PassCountAggregator();

      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.PassCount) as ClientPassCountLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = CELL_SIZE;
      aggregator.OverrideTargetPassCount = true;
      aggregator.OverridingTargetPassCountRange = new PassCountRangeRecord((ushort)length, (ushort)length);

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubgridResult(subGrids);

      Assert.True(aggregator.SummaryCellsScanned == dLength, "Invalid value for SummaryCellsScanned.");
      Assert.True(Math.Abs(aggregator.SummaryProcessedArea - dLength * Math.Pow(aggregator.CellSize, 2)) < Consts.TOLERANCE_DIMENSION, "Invalid value for SummaryProcessedArea.");
      Assert.True(aggregator.CellsScannedAtTarget == length, "Invalid value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == dLength - length, "Invalid value for CellsScannedUnderTarget.");
    }

    [Fact]
    public void Test_PassCountAggregator_ProcessResult_WithAggregation()
    {
      var aggregator = new PassCountAggregator();

      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.PassCount) as ClientPassCountLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = CELL_SIZE;
      aggregator.OverrideTargetPassCount = true;
      aggregator.OverridingTargetPassCountRange = new PassCountRangeRecord((ushort)length, (ushort)length);

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubgridResult(subGrids);

      // Other aggregator...
      var otherAggregator = new PassCountAggregator();

      otherAggregator.CellSize = CELL_SIZE;
      otherAggregator.OverrideTargetPassCount = true;
      otherAggregator.OverridingTargetPassCountRange = new PassCountRangeRecord((ushort)length, (ushort)length);

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
