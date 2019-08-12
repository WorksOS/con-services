using VSS.TRex.Analytics.ElevationStatistics.GridFabric;
using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Analytics.Foundation.Coordinators;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.ElevationStatistics
{
  /// <summary>
  /// Computes Elevation statistics. Executes in the 'application service' layer and acts as the coordinator
  /// for the request onto the cluster compute layer.
  /// </summary>
  public class ElevationStatisticsCoordinator : BaseAnalyticsCoordinator<ElevationStatisticsArgument, ElevationStatisticsResponse>
  {
    /// <summary>
    /// Constructs the aggregator from the supplied argument to be used for the Elevation statistics analytics request
    /// Create the aggregator to collect and reduce the results.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public override AggregatorBase ConstructAggregator(ElevationStatisticsArgument argument) => new ElevationStatisticsAggregator
    {
      RequiresSerialisation = true,
      SiteModelID = argument.ProjectID,
      CellSize = SiteModel.CellSize
    };

    /// <summary>
    /// Constructs the computor from the supplied argument and aggregator for the Elevation statistics analytics request
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="aggregator"></param>
    /// <returns></returns>
    public override AnalyticsComputor ConstructComputor(ElevationStatisticsArgument argument, AggregatorBase aggregator) => new AnalyticsComputor
    {
      RequestDescriptor = RequestDescriptor,
      SiteModel = SiteModel,
      Aggregator = aggregator,
      Filters = argument.Filters,
      IncludeSurveyedSurfaces = true,
      RequestedGridDataType = GridDataType.Height,
      LiftParams = argument.LiftParams
    };

    /// <summary>
    /// Pull the required counts information from the internal Elevation statistics aggregator state
    /// </summary>
    /// <param name="aggregator"></param>
    /// <param name="response"></param>
    public override void ReadOutResults(AggregatorBase aggregator, ElevationStatisticsResponse response)
    {
      var tempAggregator = (ElevationStatisticsAggregator)aggregator;

      response.CellSize = tempAggregator.CellSize;
      response.MinElevation = tempAggregator.MinElevation;
      response.MaxElevation = tempAggregator.MaxElevation;
      response.CellsUsed = tempAggregator.CellsUsed;
      response.CellsScanned = tempAggregator.CellsScanned;
      response.BoundingExtents = tempAggregator.BoundingExtents;
    }
  }
}
