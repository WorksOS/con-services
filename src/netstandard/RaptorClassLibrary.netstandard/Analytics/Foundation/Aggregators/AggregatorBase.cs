using System;
using VSS.TRex.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Analytics.Aggregators
{
  /// <summary>
  /// Base class used by all analytics aggregators supporting funcitons such as pass count summary/details, cut/fill summary, speed summary/details etc
  /// where the analytics are calculated at the cluster compute layer and reduced at the application service layer.
  /// </summary>
  public class AggregatorBase : ISubGridRequestsAggregator
  {
    /// <summary>
    /// The project the aggregation is operating on
    /// </summary>
    public Guid SiteModelID { get; set; }

    /// <summary>
    /// The cell size of the site model the aggregation is being performed over
    /// </summary>
    public double CellSize { get; set; }

    /// <summary>
    /// Provides any state initialization logic for the aggregator
    /// </summary>
    /// <param name="state"></param>
    public virtual void Initialise(AggregatorBase state)
    {
      // Initialise the aggregator ... no base implementation yet
    }

    /// <summary>
    /// Performs base aggregator handling of subgrid results
    /// </summary>
    /// <param name="subGrids"></param>
    public virtual void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids)
    {
      // Processes the given set of subgrids into this aggregator
      // ... no base implementation yet
    }

    /// <summary>
    /// Performs base aggregator finalisation activities
    /// </summary>
    public virtual void Finalise()
    {
      // Finalise the aggregator ... no base implementation yet
    }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public AggregatorBase()
    {
    }
  }
}
