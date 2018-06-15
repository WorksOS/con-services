using System;
using VSS.TRex.Analytics.SpeedStatistics;
using VSS.TRex.Tests.netcore.Analytics.Common;
using VSS.TRex.Analytics.TemperatureStatistics;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.netcore.Analytics.TemperatureStatistics
{
  public class TemperatureAggregatorTests : BaseTests
  {
		[Fact]
		public void Test_TemperatureAggregator_Creation()
		{
			var aggregator = new TemperatureAggregator();

			Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
			Assert.True(aggregator.CellSize < Consts.TOLERANCE_DIMENSION, "Invalid initial value for CellSize.");
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

    [Fact]
    public void Test_TemperatureAggregator_ProcessResult_NoAggregation()
    {
      var aggregator = new TemperatureAggregator();

      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.Temperature) as ClientTemperatureLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = CELL_SIZE;
      aggregator.OverrideTemperatureWarningLevels = true;
      aggregator.OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord((ushort)(length - 1), (ushort)(length - 1));

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubgridResult(subGrids);

      Assert.True(aggregator.SummaryCellsScanned == dLength, "Invalid value for SummaryCellsScanned.");
      Assert.True(Math.Abs(aggregator.SummaryProcessedArea - dLength * Math.Pow(aggregator.CellSize, 2)) < Consts.TOLERANCE_DIMENSION, "Invalid value for SummaryProcessedArea.");
      Assert.True(aggregator.CellsScannedAtTarget == length, "Invalid value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == dLength - length, "Invalid value for CellsScannedUnderTarget.");
    }

    [Fact]
    public void Test_TemperatureAggregator_ProcessResult_WithAggregation()
    {
      var aggregator = new TemperatureAggregator();

      var clientGrid = ClientLeafSubgridFactoryFactory.Factory().GetSubGrid(GridDataType.Temperature) as ClientTemperatureLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = CELL_SIZE;
      aggregator.OverrideTemperatureWarningLevels = true;
      aggregator.OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord((ushort)(length - 1), (ushort)(length - 1));


      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubgridResult(subGrids);

      // Other aggregator...
      var otherAggregator = new TemperatureAggregator();

      otherAggregator.CellSize = CELL_SIZE;
      otherAggregator.OverrideTemperatureWarningLevels = true;
      otherAggregator.OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord((ushort)(length - 1), (ushort)(length - 1));


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
