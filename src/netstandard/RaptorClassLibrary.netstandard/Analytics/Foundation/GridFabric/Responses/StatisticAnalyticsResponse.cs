using VSS.TRex.Analytics.GridFabric.Responses;
using VSS.TRex.GridFabric.Requests.Interfaces;

namespace VSS.TRex.Analytics.Foundation.GridFabric.Responses
{
  /// <summary>
  /// Base class for statistic analytics response.
  /// </summary>
  public class StatisticAnalyticsResponse : BaseAnalyticsResponse, IAggregateWith<StatisticAnalyticsResponse>
  {
    /// <summary>
    /// Aggregate a set of data statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public StatisticAnalyticsResponse AggregateWith(StatisticAnalyticsResponse other)
    {
      AggregateBaseDataWith(other);

      return this;
    }

    /// <summary>
    /// Aggregate a set of generic data statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    protected virtual void AggregateBaseDataWith(StatisticAnalyticsResponse other)
    {
      // ...
    }
  }
}
