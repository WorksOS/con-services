using System.Diagnostics;

namespace VSS.TRex.Analytics.Foundation.Aggregators
{
  /// <summary>
  /// Base class used by details analytics aggregators supporting funcitons such as pass count details, speed details etc
  /// where the analytics are calculated at the cluster compute layer and reduced at the application service layer.
  /// </summary>
  public class DetailsDataAggregator : DataStatisticsAggregator
  {
    /// <summary>
    /// Details data values.
    /// </summary>
    public int[] DetailsDataValues { get; set; }

    /// <summary>
    /// An array values representing the counts of cells within each of the CMV details bands defined in the request.
    /// The array's size is the same as the number of the CMV details bands.
    /// </summary>
    public long[] Counts { get; set; }

    /// <summary>
    /// Combine this aggregator with another aggregator and store the result in this aggregator
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public DetailsDataAggregator AggregateWith(DetailsDataAggregator other)
    {
      return base.AggregateWith(other) as DetailsDataAggregator;
    }

    /// <summary>
    /// Aggregate a set of CMV details into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected override void AggregateBaseDataWith(DataStatisticsAggregator other)
    {
      base.AggregateBaseDataWith(other);

      var otherAggregator = (DetailsDataAggregator)other;

      Counts = Counts ?? new long[otherAggregator.Counts.Length];

      Debug.Assert(Counts.Length == otherAggregator.Counts.Length);

      for (int i = 0; i < Counts.Length; i++)
        Counts[i] += otherAggregator.Counts[i];
    }
  }
}
