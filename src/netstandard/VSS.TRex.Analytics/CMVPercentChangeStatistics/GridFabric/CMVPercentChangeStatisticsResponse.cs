using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Analytics.CMVPercentChangeStatistics.GridFabric
{
  /// <summary>
  /// The response state returned from a CMV % change statistics request
  /// </summary>
  public class CMVPercentChangeStatisticsResponse : StatisticsAnalyticsResponse, IAggregateWith<CMVPercentChangeStatisticsResponse>, IAnalyticsOperationResponseResultConversion<CMVPercentChangeStatisticsResult>
  {
    /// <summary>
    /// Aggregate a set of CMV % change statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public CMVPercentChangeStatisticsResponse AggregateWith(CMVPercentChangeStatisticsResponse other)
    {
      return base.AggregateWith(other) as CMVPercentChangeStatisticsResponse;
    }

    /// <summary>
    /// Constructs the CMV % change result
    /// </summary>
    /// <returns></returns>
    public CMVPercentChangeStatisticsResult ConstructResult()
    {
      return new CMVPercentChangeStatisticsResult
      {
        ResultStatus = ResultStatus,
        Counts = Counts
      };
    }
  }
}
