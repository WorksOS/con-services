using System;
using RaptorClassLibrary.Tests.netcore.Analytics.Common;
using VSS.TRex.Analytics.TemperatureStatistics;
using VSS.TRex.Cells;
using Xunit;

namespace RaptorClassLibrary.Tests.netcore.Analytics.TemperatureStatistics
{
  public class TemperatureAggregatorTests : BaseTests
  {
		[Fact]
		public void Test_TemperatureAggregator_Creation()
		{
			var aggregator = new TemperatureAggregator();

			Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
			Assert.True(aggregator.CellSize < TOLERANCE, "Invalid initial value for CellSize.");
			Assert.True(aggregator.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
			Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
			Assert.True(aggregator.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
			Assert.True(aggregator.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
			Assert.True(aggregator.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
			Assert.True(!aggregator.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
			Assert.True(!aggregator.OverrideTemperatureWarningLevels, "Invalid initial value for OverrideTemperatureWarningLevels.");
			Assert.True(aggregator.OverridingTemperatureWarningLevels.Max == CellPass.NullMaterialTemperatureValue, "Invalid initial value for OverridingTemperatureWarningLevels.Max.");
			Assert.True(aggregator.OverridingTemperatureWarningLevels.Min == CellPass.NullMaterialTemperatureValue, "Invalid initial value for OverridingTemperatureWarningLevels.Min.");
			Assert.True(aggregator.LastTempRangeMax == CellPass.NullMaterialTemperatureValue, "Invalid initial value for LastTempRangeMax.");
			Assert.True(aggregator.LastTempRangeMin == CellPass.NullMaterialTemperatureValue, "Invalid initial value for LastTempRangeMin.");
		}
	}
}
