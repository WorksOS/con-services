using System;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Common;
using VSS.TRex.Tests.netcore.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.PassCountStatistics.GridFabric
{
  public class PassCountResponseTests : BaseTests
  {
    private PassCountStatisticsResponse _response => new PassCountStatisticsResponse()
    {
      ResultStatus = RequestErrorStatus.OK,
      CellSize = CELL_SIZE,
      CellsScannedOverTarget = CELLS_OVER_TARGET,
      CellsScannedAtTarget = CELLS_AT_TARGET,
      CellsScannedUnderTarget = CELLS_UNDER_TARGET,
      SummaryCellsScanned = CELLS_OVER_TARGET + CELLS_AT_TARGET + CELLS_UNDER_TARGET,
      IsTargetValueConstant = true,
      LastPassCountTargetRange = new PassCountRangeRecord(3, 10)
    };

    [Fact]
    public void Test_PassCountResponse_Creation()
    {
      var response = new PassCountStatisticsResponse();

      Assert.True(response.ResultStatus == RequestErrorStatus.Unknown, "ResultStatus invalid after creation.");
      Assert.True(response.CellSize < Consts.TOLERANCE_DIMENSION, "CellSize invalid after creation.");
      Assert.True(response.SummaryCellsScanned == 0, "Invalid initial value for SummaryCellsScanned.");
      Assert.True(response.LastPassCountTargetRange.Min == 0, "Invalid initial value for LastPassCountTargetRange.Min.");
      Assert.True(response.LastPassCountTargetRange.Max == 0, "Invalid initial value for LastPassCountTargetRange.Max.");
      Assert.True(response.CellsScannedOverTarget == 0, "Invalid initial value for CellsScannedOverTarget.");
      Assert.True(response.CellsScannedAtTarget == 0, "Invalid initial value for CellsScannedAtTarget.");
      Assert.True(response.CellsScannedUnderTarget == 0, "Invalid initial value for CellsScannedUnderTarget.");
      Assert.True(response.IsTargetValueConstant, "Invalid initial value for IsTargetValueConstant.");
      Assert.True(!response.MissingTargetValue, "Invalid initial value for MissingTargetValue.");
    }

    [Fact]
    public void Test_PassCountResponse_ConstructResult_Successful()
    {
      Assert.True(_response.ResultStatus == RequestErrorStatus.OK, "Invalid initial result status");

      var result = _response.ConstructResult();

      Assert.True(result.ResultStatus == RequestErrorStatus.OK, "Result status invalid, not propagaged from aggregation state");

      Assert.True(result.ConstantTargetPassCountRange.Min ==_response.LastPassCountTargetRange.Min, "Invalid initial result value for ConstantTargetPassCountRange.Min.");
      Assert.True(result.ConstantTargetPassCountRange.Max == _response.LastPassCountTargetRange.Max, "Invalid initial result value for ConstantTargetPassCountRange.Max.");
      Assert.True(Math.Abs(result.AboveTargetPercent - _response.ValueOverTargetPercent) < Consts.TOLERANCE_PERCENTAGE, "Invalid initial result value for AbovePassCountPercent.");
      Assert.True(Math.Abs(result.WithinTargetPercent - _response.ValueAtTargetPercent) < Consts.TOLERANCE_PERCENTAGE, "Invalid initial result value for WithinPassCountPercent.");
      Assert.True(Math.Abs(result.BelowTargetPercent - _response.ValueUnderTargetPercent) < Consts.TOLERANCE_PERCENTAGE, "Invalid initial result value for BelowPassCountPercent.");
      Assert.True(Math.Abs(result.TotalAreaCoveredSqMeters - _response.SummaryProcessedArea) < Consts.TOLERANCE_DIMENSION, "Invalid initial result value for TotalAreaCoveredSqMeters.");
      Assert.True(result.IsTargetPassCountConstant == _response.IsTargetValueConstant, "Invalid initial result value for IsTargetPassCountConstant.");
    }

    [Fact]
    public void Test_PassCountResponse_AgregateWith_Successful()
    {
      var responseClone = new PassCountStatisticsResponse()
      {
        ResultStatus = _response.ResultStatus,
        CellSize = _response.CellSize,
        CellsScannedOverTarget = _response.CellsScannedOverTarget,
        CellsScannedAtTarget = _response.CellsScannedAtTarget,
        CellsScannedUnderTarget = _response.CellsScannedUnderTarget,
        SummaryCellsScanned = _response.SummaryCellsScanned,
        IsTargetValueConstant = _response.IsTargetValueConstant,
        LastPassCountTargetRange = _response.LastPassCountTargetRange
      };

      var response = _response.AggregateWith(responseClone);

      Assert.True(Math.Abs(response.CellSize - _response.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after aggregation.");
      Assert.True(response.SummaryCellsScanned == _response.SummaryCellsScanned * 2, "Invalid aggregated value for SummaryCellsScanned.");
      Assert.True(response.LastPassCountTargetRange.Min == _response.LastPassCountTargetRange.Min, "Invalid aggregated value for LastPassCountTargetRange.Min.");
      Assert.True(response.LastPassCountTargetRange.Max == _response.LastPassCountTargetRange.Max, "Invalid aggregated value for LastPassCountTargetRange.Max.");
      Assert.True(response.CellsScannedOverTarget == _response.CellsScannedOverTarget * 2, "Invalid aggregated value for CellsScannedOverTarget.");
      Assert.True(response.CellsScannedAtTarget == _response.CellsScannedAtTarget * 2, "Invalid aggregated value for CellsScannedAtTarget.");
      Assert.True(response.CellsScannedUnderTarget == _response.CellsScannedUnderTarget * 2, "Invalid aggregated value for CellsScannedUnderTarget.");
      Assert.True(response.IsTargetValueConstant == _response.IsTargetValueConstant, "Invalid aggregated value for IsTargetValueConstant.");
      Assert.True(response.MissingTargetValue == _response.MissingTargetValue, "Invalid aggregated value for MissingTargetValue.");
    }
  }
}
