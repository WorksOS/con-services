using System;
using VSS.TRex.Analytics.MDPStatistics;
using VSS.TRex.Cells;
using VSS.TRex.Tests.netcore.Analytics.Common;
using Xunit;

namespace VSS.TRex.Tests.Analytics.MDPStatistics
{
  public class MDPAggregatorTests : BaseTests
  {
    [Fact]
    public void Test_TemperatureAggregator_Creation()
    {
      var aggregator = new MDPAggregator();

      Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
      Assert.True(aggregator.CellSize < TOLERANCE, "Invalid initial value for CellSize.");
      Assert.True(aggregator.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
      Assert.True(aggregator.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
      Assert.True(!aggregator.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
      Assert.True(!aggregator.OverrideMachineMDP, "Invalid initial value for OverrideTemperatureWarningLevels.");
      Assert.True(aggregator.OverridingMachineMDP == CellPass.NullMDP, "Invalid initial value for OverridingMachineMDP.");
      Assert.True(aggregator.LastTargetMDP == CellPass.NullMDP, "Invalid initial value for LastTargetMDP.");
    }
  }
}
