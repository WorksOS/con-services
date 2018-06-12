using System;
using VSS.TRex.Tests.netcore.Analytics.Common;
using VSS.TRex.Analytics.CMVStatistics;
using VSS.TRex.Cells;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CMVStatistics
{
  public class CMVAggregatorTests : BaseTests
  {
    [Fact]
    public void Test_CMVAggregator_Creation()
    {
      var aggregator = new CMVAggregator();

      Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
      Assert.True(aggregator.CellSize < TOLERANCE, "Invalid initial value for CellSize.");
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
  }
}
