using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.Analytics.PassCountStatistics.Summary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric.Summary
{
  /// <summary>
  /// The response state returned from a Pass Count summary request
  /// </summary>
  public class PassCountSummaryResponse : SummaryAnalyticsResponse, IAggregateWith<PassCountSummaryResponse>, IAnalyticsOperationResponseResultConversion<PassCountSummaryResult>
  {
    /// <summary>
    /// Holds last known good target Pass Count range values.
    /// </summary>
    public PassCountRangeRecord LastPassCountTargetRange;

    /// <summary>
    /// Aggregate a set of Pass Count summary into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected override void AggregateBaseDataWith(StatisticAnalyticsResponse other)
    {
      base.AggregateBaseDataWith(other);

      LastPassCountTargetRange = ((PassCountSummaryResponse) other).LastPassCountTargetRange;
    }

    public PassCountSummaryResponse AggregateWith(PassCountSummaryResponse other)
    {
      return base.AggregateWith(other) as PassCountSummaryResponse;
    }

    public PassCountSummaryResult ConstructResult()
    {
      return new PassCountSummaryResult
      {
        IsTargetPassCountConstant = IsTargetValueConstant,
        ConstantTargetPassCountRange = LastPassCountTargetRange,
        AboveTargetPercent = ValueOverTargetPercent,
        WithinTargetPercent = ValueAtTargetPercent,
        BelowTargetPercent = ValueUnderTargetPercent,
        TotalAreaCoveredSqMeters = SummaryProcessedArea,

        ReturnCode = MissingTargetValue ?
          (!(ValueOverTargetPercent < Consts.TOLERANCE_PERCENTAGE) && ValueAtTargetPercent < Consts.TOLERANCE_PERCENTAGE && ValueUnderTargetPercent < Consts.TOLERANCE_PERCENTAGE) ? MissingTargetDataResultType.PartialResult : MissingTargetDataResultType.PartialResultMissingTarget :
          SummaryCellsScanned == 0 ? MissingTargetDataResultType.NoProblems : MissingTargetDataResultType.NoResult,

        ResultStatus = ResultStatus
      };
    }

  }
}
