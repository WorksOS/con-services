using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Analytics.CutFillStatistics.GridFabric
{
  /// <summary>
  /// The response state returned from a cut/fill statistics request
  /// </summary>
  public class CutFillStatisticsResponse : StatisticsAnalyticsResponse, IAggregateWith<CutFillStatisticsResponse>, IAnalyticsOperationResponseResultConversion<CutFillStatisticsResult>
  {
    /// <summary>
    /// Aggregate a set of cut fill statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public CutFillStatisticsResponse AggregateWith(CutFillStatisticsResponse other)
    {
      return base.AggregateWith(other) as CutFillStatisticsResponse;
    }

    /// <summary>
    /// Constructs the Cut/Fill result
    /// </summary>
    /// <returns></returns>
    public CutFillStatisticsResult ConstructResult()
    {
      return new CutFillStatisticsResult
      {
        ResultStatus = ResultStatus,
        Counts = Counts
      };
    }
  }
}
