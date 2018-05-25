using System;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Types;
using Xunit;

namespace RaptorClassLibrary.Tests.netcore.Analytics.TemperatureStatistics.GridFabric
{
	public class TemperatureResponseTests
	{
		private const double CELL_SIZE = 0.34;
		private const int CELLS_OVER_TARGET = 25;
		private const int CELLS_AT_TARGET = 45;
		private const int CELLS_UNDER_TARGET = 85;
		private const double TOLERANCE = 0.00001;

	  private TemperatureStatisticsResponse _response => new TemperatureStatisticsResponse()
	  {
	    ResultStatus = RequestErrorStatus.OK,
	    CellSize = CELL_SIZE,
	    CellsScannedOverTarget = CELLS_OVER_TARGET,
	    CellsScannedAtTarget = CELLS_AT_TARGET,
	    CellsScannedUnderTarget = CELLS_UNDER_TARGET,
	    SummaryCellsScanned = CELLS_OVER_TARGET + CELLS_AT_TARGET + CELLS_UNDER_TARGET,
	    IsTargetValueConstant = true,
	    LastTempRangeMax = 150,
	    LastTempRangeMin = 10
    };

	  [Fact]
	  public void Test_TemperatureResponse_Creation()
	  {
      var response = new TemperatureStatisticsResponse();

	    Assert.True(response.ResultStatus == RequestErrorStatus.OK, "ResultStatus invalid after creation.");
      Assert.True(response.CellSize < TOLERANCE, "CellSize invalid after creation.");
	    Assert.True(response.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
	    Assert.True(response.LastTempRangeMax < TOLERANCE, "Invalid initial value for LastTempRangeMax.");
	    Assert.True(response.LastTempRangeMin < TOLERANCE, "Invalid initial value for LastTempRangeMin.");
	    Assert.True(response.CellsScannedOverTarget < TOLERANCE, "Invalid initial value for CellsScannedOverTarget.");
	    Assert.True(response.CellsScannedAtTarget < TOLERANCE, "Invalid initial value for CellsScannedAtTarget.");
	    Assert.True(response.CellsScannedUnderTarget < TOLERANCE, "Invalid initial value for CellsScannedUnderTarget.");
	    Assert.True(response.IsTargetValueConstant == false, "Invalid initial value for IsTargetValueConstant.");
	    Assert.True(response.MissingTargetValue == false, "Invalid initial value for MissingTargetValue.");
    }

    [Fact]
		public void Test_TemperatureResponse_ConstructResult_Successful()
		{
			Assert.True(_response.ResultStatus == RequestErrorStatus.OK, "Invalid initial result status");

			var result = _response.ConstructResult();

			Assert.True(result.ResultStatus == RequestErrorStatus.OK, "Result status invalid, not propagaged from aggregation state");

			Assert.True(Math.Abs(result.MaximumTemperature - _response.LastTempRangeMax) < TOLERANCE, "Invalid initial result value for MaximumTemperature.");
			Assert.True(Math.Abs(result.MinimumTemperature - _response.LastTempRangeMin) < TOLERANCE, "Invalid initial result value for MinimumTemperature.");
			Assert.True(Math.Abs(result.AboveTemperaturePercent - _response.ValueOverTargetPercent) < TOLERANCE, "Invalid initial result value for AboveTemperaturePercent.");
			Assert.True(Math.Abs(result.WithinTemperaturePercent - _response.ValueAtTargetPercent) < TOLERANCE, "Invalid initial result value for WithinTemperaturePercent.");
			Assert.True(Math.Abs(result.BelowTemperaturePercent - _response.ValueUnderTargetPercent) < TOLERANCE, "Invalid initial result value for BelowTemperaturePercent.");
			Assert.True(Math.Abs(result.TotalAreaCoveredSqMeters - _response.SummaryProcessedArea) < TOLERANCE, "Invalid initial result value for TotalAreaCoveredSqMeters.");
			Assert.True(result.IsTargetTemperatureConstant == _response.IsTargetValueConstant, "Invalid initial result value for IsTargetValueConstant.");
		}

	  [Fact]
	  public void Test_TemperatureResponse_AgregateWith_Successful()
	  {
	    var responseClone = new TemperatureStatisticsResponse()
	    {
	      ResultStatus = _response.ResultStatus,
	      CellSize = _response.CellSize,
	      CellsScannedOverTarget = _response.CellsScannedOverTarget,
	      CellsScannedAtTarget = _response.CellsScannedAtTarget,
	      CellsScannedUnderTarget = _response.CellsScannedUnderTarget,
	      SummaryCellsScanned = _response.SummaryCellsScanned,
	      IsTargetValueConstant = _response.IsTargetValueConstant,
	      LastTempRangeMax = _response.LastTempRangeMax,
	      LastTempRangeMin = _response.LastTempRangeMin
      };

      var response = _response.AggregateWith(responseClone);

	    Assert.True(Math.Abs(response.CellSize - _response.CellSize) < TOLERANCE, "CellSize invalid after aggregation.");
	    Assert.True(response.SummaryCellsScanned == _response.SummaryCellsScanned * 2, "Invalid aggregated value for SummaryCellsScanned.");
      Assert.True(Math.Abs(response.LastTempRangeMax - _response.LastTempRangeMax) < TOLERANCE, "Invalid aggregated value for LastTempRangeMax.");
	    Assert.True(Math.Abs(response.LastTempRangeMin - _response.LastTempRangeMin) < TOLERANCE, "Invalid aggregated value for LastTempRangeMin.");
	    Assert.True(Math.Abs(response.CellsScannedOverTarget - _response.CellsScannedOverTarget * 2) < TOLERANCE, "Invalid aggregated value for CellsScannedOverTarget.");
      Assert.True(Math.Abs(response.CellsScannedAtTarget - _response.CellsScannedAtTarget * 2) < TOLERANCE, "Invalid aggregated value for CellsScannedAtTarget.");
	    Assert.True(Math.Abs(response.CellsScannedUnderTarget - _response.CellsScannedUnderTarget * 2) < TOLERANCE, "Invalid aggregated value for CellsScannedUnderTarget.");
	    Assert.True(response.IsTargetValueConstant == _response.IsTargetValueConstant, "Invalid aggregated value for IsTargetValueConstant.");
	    Assert.True(response.MissingTargetValue == _response.MissingTargetValue, "Invalid aggregated value for MissingTargetValue.");

    }

  }
}
