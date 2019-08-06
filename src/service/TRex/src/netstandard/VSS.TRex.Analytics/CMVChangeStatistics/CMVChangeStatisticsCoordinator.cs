using VSS.TRex.Analytics.CMVChangeStatistics.GridFabric;
using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Analytics.Foundation.Coordinators;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVChangeStatistics
{
  /// <summary>
  /// Computes CMV change statistics. Executes in the 'application service' layer and acts as the coordinator
  /// for the request onto the cluster compute layer.
  /// The CMV change is exposed on the client as CMV % change.
  /// </summary>
  public class CMVChangeStatisticsCoordinator : BaseAnalyticsCoordinator<CMVChangeStatisticsArgument, CMVChangeStatisticsResponse>
  {
    public override AggregatorBase ConstructAggregator(CMVChangeStatisticsArgument argument) => new CMVChangeStatisticsAggregator
    {
      RequiresSerialisation = true,
      SiteModelID = argument.ProjectID,
      CellSize = SiteModel.CellSize,
      CMVChangeDetailsDataValues = argument.CMVChangeDetailsDataValues,
      Counts = argument.CMVChangeDetailsDataValues != null ? new long[argument.CMVChangeDetailsDataValues.Length] : null
    };

    public override AnalyticsComputor ConstructComputor(CMVChangeStatisticsArgument argument, AggregatorBase aggregator) => new AnalyticsComputor
    {
      RequestDescriptor = RequestDescriptor,
      SiteModel = SiteModel,
      Aggregator = aggregator,
      Filters = argument.Filters,
      IncludeSurveyedSurfaces = true,
      RequestedGridDataType = GridDataType.CCVPercentChange,
      LiftParams = argument.LiftParams
    };

    public override void ReadOutResults(AggregatorBase aggregator, CMVChangeStatisticsResponse response)
    {
      var tempAggregator = (DataStatisticsAggregator)aggregator;

      response.CellSize = tempAggregator.CellSize;
      response.SummaryCellsScanned = tempAggregator.SummaryCellsScanned;

      response.Counts = tempAggregator.Counts;
    }
  }
}
