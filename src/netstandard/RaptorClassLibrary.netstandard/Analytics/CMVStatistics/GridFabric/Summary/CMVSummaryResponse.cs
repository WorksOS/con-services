using System.Diagnostics;
using VSS.TRex.Analytics.CMVStatistics.Summary;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Requests.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric.Summary
{
  /// <summary>
  /// The response state returned from a CMV summary request
  /// </summary>
  public class CMVSummaryResponse : SummaryAnalyticsResponse, IAggregateWith<CMVSummaryResponse>, IAnalyticsOperationResponseResultConversion<CMVSummaryResult>
  {
    /// <summary>
    /// Holds last known good target CMV value.
    /// </summary>
    public short LastTargetCMV { get; set; }

    /// <summary>
    /// Aggregate a set of CMV summary into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected override void AggregateBaseDataWith(StatisticAnalyticsResponse other)
    {
      base.AggregateBaseDataWith(other);

      LastTargetCMV = ((CMVSummaryResponse)other).LastTargetCMV;
    }

    public CMVSummaryResponse AggregateWith(CMVSummaryResponse other)
    {
      return base.AggregateWith(other) as CMVSummaryResponse;
    }

    public CMVSummaryResult ConstructResult()
    {
      return new CMVSummaryResult
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
