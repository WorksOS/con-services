using System;
using VSS.TRex.Tests.netcore.Analytics.Common;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.netcore.Analytics.TemperatureStatistics.GridFabric
{
	public class TemperatureResponseTests : BaseTests
  {
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

	    Assert.True(response.ResultStatus == RequestErrorStatus.Unknown, "ResultStatus invalid after creation.");
      Assert.True(response.CellSize < Consts.TOLERANCE, "CellSize invalid after creation.");
	    Assert.True(response.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
	    Assert.True(response.LastTempRangeMax == 0, "Invalid initial value for LastTempRangeMax.");
	    Assert.True(response.LastTempRangeMin == 0, "Invalid initial value for LastTempRangeMin.");
	    Assert.True(response.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
	    Assert.True(response.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
	    Assert.True(response.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
	    Assert.True(response.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
	    Assert.True(!response.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
    }

    [Fact]
		public void Test_TemperatureResponse_ConstructResult_Successful()
		{
			Assert.True(_response.ResultStatus == RequestErrorStatus.OK, "Invalid initial result status");

			var result = _response.ConstructResult();

			Assert.True(result.ResultStatus == RequestErrorStatus.OK, "Result status invalid, not propagaged from aggregation state");

			Assert.True(Math.Abs(result.MaximumTemperature - _response.LastTempRangeMax) < Consts.TOLERANCE, "Invalid initial result value for MaximumTemperature.");
			Assert.True(Math.Abs(result.MinimumTemperature - _response.LastTempRangeMin) < Consts.TOLERANCE, "Invalid initial result value for MinimumTemperature.");
			Assert.True(Math.Abs(result.AboveTargetPercent - _response.ValueOverTargetPercent) < Consts.TOLERANCE, "Invalid initial result value for AboveTemperaturePercent.");
			Assert.True(Math.Abs(result.WithinTargetPercent - _response.ValueAtTargetPercent) < Consts.TOLERANCE, "Invalid initial result value for WithinTemperaturePercent.");
			Assert.True(Math.Abs(result.BelowTargetPercent - _response.ValueUnderTargetPercent) < Consts.TOLERANCE, "Invalid initial result value for BelowTemperaturePercent.");
			Assert.True(Math.Abs(result.TotalAreaCoveredSqMeters - _response.SummaryProcessedArea) < Consts.TOLERANCE, "Invalid initial result value for TotalAreaCoveredSqMeters.");
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

	    Assert.True(Math.Abs(response.CellSize - _response.CellSize) < Consts.TOLERANCE, "CellSize invalid after aggregation.");
	    Assert.True(response.SummaryCellsScanned == _response.SummaryCellsScanned * 2, "Invalid aggregated value for SummaryCellsScanned.");
      Assert.True(response.LastTempRangeMax ==_response.LastTempRangeMax, "Invalid aggregated value for LastTempRangeMax.");
	    Assert.True(response.LastTempRangeMin ==_response.LastTempRangeMin, "Invalid aggregated value for LastTempRangeMin.");
	    Assert.True(response.CellsScannedOverTarget ==_response.CellsScannedOverTarget * 2, "Invalid aggregated value for CellsScannedOverTarget.");
      Assert.True(response.CellsScannedAtTarget ==_response.CellsScannedAtTarget * 2, "Invalid aggregated value for CellsScannedAtTarget.");
	    Assert.True(response.CellsScannedUnderTarget ==_response.CellsScannedUnderTarget * 2, "Invalid aggregated value for CellsScannedUnderTarget.");
	    Assert.True(response.IsTargetValueConstant == _response.IsTargetValueConstant, "Invalid aggregated value for IsTargetValueConstant.");
	    Assert.True(response.MissingTargetValue == _response.MissingTargetValue, "Invalid aggregated value for MissingTargetValue.");
    }
  }
}
