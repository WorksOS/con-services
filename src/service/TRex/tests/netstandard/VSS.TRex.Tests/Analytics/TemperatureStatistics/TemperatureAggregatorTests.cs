using System;
using VSS.TRex.Analytics.TemperatureStatistics;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Records;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.TemperatureStatistics
{
  public class TemperatureAggregatorTests : IClassFixture<DILoggingFixture>
  {
		[Fact]
		public void Test_TemperatureAggregator_Creation()
		{
			var aggregator = new TemperatureStatisticsAggregator();

			Assert.True(aggregator.SiteModelID == Guid.Empty, "Invalid initial value for SiteModelID.");
			Assert.True(aggregator.CellSize < Consts.TOLERANCE_DIMENSION, "Invalid initial value for CellSize.");
			Assert.True(aggregator.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
			Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
			Assert.True(aggregator.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
			Assert.True(aggregator.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
			Assert.True(aggregator.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
			Assert.True(!aggregator.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
			Assert.True(!aggregator.OverrideTemperatureWarningLevels, "Invalid initial value for OverrideTemperatureWarningLevels.");
			Assert.True(aggregator.OverridingTemperatureWarningLevels.Max == CellPassConsts.NullMaterialTemperatureValue, "Invalid initial value for OverridingTemperatureWarningLevels.Max.");
			Assert.True(aggregator.OverridingTemperatureWarningLevels.Min == CellPassConsts.NullMaterialTemperatureValue, "Invalid initial value for OverridingTemperatureWarningLevels.Min.");
			Assert.True(aggregator.LastTempRangeMax == CellPassConsts.NullMaterialTemperatureValue, "Invalid initial value for LastTempRangeMax.");
			Assert.True(aggregator.LastTempRangeMin == CellPassConsts.NullMaterialTemperatureValue, "Invalid initial value for LastTempRangeMin.");
		}

    [Fact]
    public void Test_TemperatureAggregator_ProcessResult_NoAggregation()
    {
      var aggregator = new TemperatureStatisticsAggregator();

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Temperature) as ClientTemperatureLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = TestConsts.CELL_SIZE;
      aggregator.OverrideTemperatureWarningLevels = true;
      aggregator.OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord((ushort)(length - 1), (ushort)(length - 1));

      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubGridResult(subGrids);

      Assert.True(aggregator.SummaryCellsScanned == dLength, "Invalid value for SummaryCellsScanned.");
      Assert.True(Math.Abs(aggregator.SummaryProcessedArea - dLength * Math.Pow(aggregator.CellSize, 2)) < Consts.TOLERANCE_DIMENSION, "Invalid value for SummaryProcessedArea.");
      Assert.True(aggregator.CellsScannedAtTarget == length, "Invalid value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == dLength - length, "Invalid value for CellsScannedUnderTarget.");
    }

    [Fact]
    public void Test_TemperatureAggregator_ProcessResult_WithAggregation()
    {
      var aggregator = new TemperatureStatisticsAggregator();

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Temperature) as ClientTemperatureLeafSubGrid;

      clientGrid.FillWithTestPattern();

      var dLength = clientGrid.Cells.Length;
      var length = (short)Math.Sqrt(dLength);
      aggregator.CellSize = TestConsts.CELL_SIZE;
      aggregator.OverrideTemperatureWarningLevels = true;
      aggregator.OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord((ushort)(length - 1), (ushort)(length - 1));


      IClientLeafSubGrid[][] subGrids = new[] { new[] { clientGrid } };

      aggregator.ProcessSubGridResult(subGrids);

      // Other aggregator...
      var otherAggregator = new TemperatureStatisticsAggregator();

      otherAggregator.CellSize = TestConsts.CELL_SIZE;
      otherAggregator.OverrideTemperatureWarningLevels = true;
      otherAggregator.OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord((ushort)(length - 1), (ushort)(length - 1));


      otherAggregator.ProcessSubGridResult(subGrids);

      aggregator.AggregateWith(otherAggregator);

      Assert.True(aggregator.SummaryCellsScanned == dLength * 2, "Invalid value for SummaryCellsScanned.");
      Assert.True(Math.Abs(aggregator.SummaryProcessedArea - 2 * dLength * Math.Pow(aggregator.CellSize, 2)) < Consts.TOLERANCE_DIMENSION, "Invalid value for SummaryProcessedArea.");
      Assert.True(aggregator.CellsScannedAtTarget == length * 2, "Invalid value for CellsScannedAtTarget.");
      Assert.True(aggregator.CellsScannedOverTarget == 0, "Invalid value for CellsScannedOverTarget.");
      Assert.True(aggregator.CellsScannedUnderTarget == (dLength - length) * 2, "Invalid value for CellsScannedUnderTarget.");
    }
  }
}
