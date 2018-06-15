using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Requests.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric
{
  /// <summary>
  /// The response state returned from a Pass Count statistics request
  /// </summary>
  public class PassCountStatisticsResponse : SummaryAnalyticsResponse, IAggregateWith<PassCountStatisticsResponse>, IAnalyticsOperationResponseResultConversion<PassCountResult>
  {
    /// <summary>
    /// Holds last known good target Pass Count range values.
    /// </summary>
    public PassCountRangeRecord LastPassCountTargetRange;

    /// <summary>
    /// Aggregate a set of Pass Count statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected override void AggregateBaseDataWith(SummaryAnalyticsResponse other)
    {
      base.AggregateBaseDataWith(other);

      LastPassCountTargetRange = ((PassCountStatisticsResponse) other).LastPassCountTargetRange;
    }

    public PassCountStatisticsResponse AggregateWith(PassCountStatisticsResponse other)
    {
      return base.AggregateWith(other) as PassCountStatisticsResponse;
    }

    public PassCountResult ConstructResult()
    {
      return new PassCountResult
      {
        IsTargetPassCountConstant = IsTargetValueConstant,
        ConstantTargetPassCountRange = LastPassCountTargetRange,
        AboveTargetPercent = ValueOverTargetPercent,
        WithinTargetPercent = ValueAtTargetPercent,
        BelowTargetPercent = ValueUnderTargetPercent,
        TotalAreaCoveredSqMeters = SummaryProcessedArea,

        ReturnCode = MissingTargetValue ?
          (!(ValueOverTargetPercent < Consts.TOLERANCE) && ValueAtTargetPercent < Consts.TOLERANCE && ValueUnderTargetPercent < Consts.TOLERANCE) ? MissingTargetDataResultType.PartialResult : MissingTargetDataResultType.PartialResultMissingTarget :
          SummaryCellsScanned == 0 ? MissingTargetDataResultType.NoProblems : MissingTargetDataResultType.NoResult,

        ResultStatus = ResultStatus
      };
    }

  }
}
