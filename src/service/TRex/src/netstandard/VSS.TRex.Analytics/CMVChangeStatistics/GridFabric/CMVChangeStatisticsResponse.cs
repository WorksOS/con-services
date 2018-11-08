using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Analytics.CMVChangeStatistics.GridFabric
{
  /// <summary>
  /// The response state returned from a CMV change statistics request.
  /// The CMV change is exposed on the client as CMV % change.
  /// </summary>
  public class CMVChangeStatisticsResponse : StatisticsAnalyticsResponse, IAggregateWith<CMVChangeStatisticsResponse>, IAnalyticsOperationResponseResultConversion<CMVChangeStatisticsResult>
  {
    /// <summary>
    /// Aggregate a set of CMV change statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public CMVChangeStatisticsResponse AggregateWith(CMVChangeStatisticsResponse other)
    {
      return base.AggregateWith(other) as CMVChangeStatisticsResponse;
    }

    /// <summary>
    /// Constructs the CMV change result
    /// </summary>
    /// <returns></returns>
    public CMVChangeStatisticsResult ConstructResult()
    {
      return new CMVChangeStatisticsResult
      {
        ResultStatus = ResultStatus,
        Counts = Counts,
        TotalAreaCoveredSqMeters = SummaryProcessedArea
      };
    }
  }
}
