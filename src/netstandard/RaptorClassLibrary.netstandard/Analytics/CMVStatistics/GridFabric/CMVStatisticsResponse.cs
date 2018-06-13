using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Requests.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric
{
  /// <summary>
  /// The response state returned from a CMV statistics request
  /// </summary>
  public class CMVStatisticsResponse : SummaryAnalyticsResponse, IAggregateWith<CMVStatisticsResponse>, IAnalyticsOperationResponseResultConversion<CMVResult>
  {
    /// <summary>
    /// Holds last known good target CMV value.
    /// </summary>
    public short LastTargetCMV { get; set; }

    /// <summary>
    /// Aggregate a set of CMV statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected override void AggregateBaseDataWith(SummaryAnalyticsResponse other)
    {
      base.AggregateBaseDataWith(other);

      LastTargetCMV = ((CMVStatisticsResponse)other).LastTargetCMV;
    }

    public CMVStatisticsResponse AggregateWith(CMVStatisticsResponse other)
    {
      return base.AggregateWith(other) as CMVStatisticsResponse;
    }

    public CMVResult ConstructResult()
    {
      return new CMVResult
      {
        IsTargetCMVConstant = IsTargetValueConstant,
        ConstantTargetCMV = LastTargetCMV,
        AboveTargetPercent = ValueOverTargetPercent,
        WithinTargetPercent = ValueAtTargetPercent,
        BelowTargetPercent = ValueUnderTargetPercent,
        TotalAreaCoveredSqMeters = SummaryProcessedArea,

        ReturnCode = MissingTargetValue ? SummaryCellsScanned == 0 ? MissingTargetDataResultType.NoResult : MissingTargetDataResultType.PartialResult : MissingTargetDataResultType.NoProblems,

        ResultStatus = ResultStatus
      };
    }
  }
}
