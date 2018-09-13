using System.Reflection;
using Microsoft.Extensions.Logging;
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
  /// </summary>
  public class CMVChangeStatisticsCoordinator : BaseAnalyticsCoordinator<CMVChangeStatisticsArgument, CMVChangeStatisticsResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    public override AggregatorBase ConstructAggregator(CMVChangeStatisticsArgument argument) => new CMVChangeStatisticsAggregator
    {
      RequiresSerialisation = true,
      SiteModelID = argument.ProjectID,
      CellSize = SiteModel.Grid.CellSize,
      CMVPercentChangeDetailsDataValues = argument.CMVChangeDatalValues,
      Counts = argument.CMVChangeDatalValues != null ? new long[argument.CMVChangeDatalValues.Length] : null
    };

    public override AnalyticsComputor ConstructComputor(CMVChangeStatisticsArgument argument, AggregatorBase aggregator) => new AnalyticsComputor()
    {
      RequestDescriptor = RequestDescriptor,
      SiteModel = SiteModel,
      Aggregator = aggregator,
      Filters = argument.Filters,
      IncludeSurveyedSurfaces = true,
      RequestedGridDataType = GridDataType.CCVPercentChange
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
