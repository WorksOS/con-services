using VSS.TRex.Analytics.Aggregators;

namespace VSS.TRex.Analytics.Foundation.Aggregators
{
  /// <summary>
  /// Base class used by data analytics aggregators supporting funcitons such as pass count summary/details, cut/fill details, speed summary etc
  /// where the analytics are calculated at the cluster compute layer and reduced at the application service layer.
  /// </summary>
  public class DataStatisticsAggregator : AggregatorBase
  {
    /// <summary>
    /// Aggregator state is now single threaded in the context of processing subgrid
    /// information into it as the processing threads access independent substate aggregators which
    /// are aggregated together to form the final aggregation result. However, in contexts that do support
    /// threaded access to this sturcture the FRequiresSerialisation flag should be set
    /// </summary>
    public bool RequiresSerialisation { get; set; }

    /// <summary>
    /// Combine this aggregator with another aggregator and store the result in this aggregator
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public DataStatisticsAggregator AggregateWith(DataStatisticsAggregator other)
    {
      AggregateBaseDataWith(other);

      DataCheck(other);

      return this;
    }

    /// <summary>
    /// Aggregate a set of generic data statistics into this set.
    /// </summary>
    /// <param name="other"></param>
    protected virtual void AggregateBaseDataWith(DataStatisticsAggregator other)
    {
      CellSize = other.CellSize;
    }

    protected virtual void DataCheck(DataStatisticsAggregator other)
    {
      // Nothing to implement...
    }
  }
}
