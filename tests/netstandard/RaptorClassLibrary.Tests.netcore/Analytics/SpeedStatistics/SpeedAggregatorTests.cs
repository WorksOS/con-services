using System;
using RaptorClassLibrary.Tests.netcore.Analytics.Common;
using VSS.TRex.Analytics.SpeedStatistics;
using VSS.TRex.Cells;
using Xunit;

namespace RaptorClassLibrary.Tests.netcore.Analytics.SpeedStatistics
{
  public class SpeedAggregatorTests : BaseTests
  {
    [Fact]
    public void Test_SpeedAggregator_Creation()
    {
      var aggregator = new SpeedAggregator();

      Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
      Assert.True(aggregator.CellSize < TOLERANCE, "Invalid initial value for CellSize.");
      Assert.True(aggregator.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
      Assert.True(aggregator.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
      Assert.True(!aggregator.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
      Assert.True(aggregator.TargetMachineSpeed.Max == CellPass.NullMachineSpeed, "Invalid initial value for TargetMachineSpeed.Max.");
      Assert.True(aggregator.TargetMachineSpeed.Min == CellPass.NullMachineSpeed, "Invalid initial value for TargetMachineSpeed.Min.");
    }
  }
}
