using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Analytics.Foundation.Coordinators;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.PassCountStatistics
{
  /// <summary>
  /// Computes Pass Clount summary. Executes in the 'application service' layer and acts as the coordinator
  /// for the request onto the cluster compute layer.
  /// </summary>
  public class PassCountStatisticsCoordinator : BaseAnalyticsCoordinator<PassCountStatisticsArgument, PassCountStatisticsResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Constructs the aggregator from the supplied argument to be used for the Pass Count summary analytics request
    /// Create the aggregator to collect and reduce the results.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public override AggregatorBase ConstructAggregator(PassCountStatisticsArgument argument) => new PassCountStatisticsAggregator()
    {
      RequiresSerialisation = true,
      SiteModelID = argument.ProjectID,
      CellSize = SiteModel.Grid.CellSize,
      OverrideTargetPassCount = argument.OverrideTargetPassCount,
      OverridingTargetPassCountRange = argument.OverridingTargetPassCountRange,
      DetailsDataValues = argument.PassCountDetailValues,
      Counts = new long[argument.PassCountDetailValues.Length]
    };

    /// <summary>
    /// Constructs the computor from the supplied argument and aggregator for the Pass Count statistics analytics request
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="aggregator"></param>
    /// <returns></returns>
    public override AnalyticsComputor ConstructComputor(PassCountStatisticsArgument argument, AggregatorBase aggregator) => new AnalyticsComputor()
    {
      RequestDescriptor = RequestDescriptor,
      SiteModel = SiteModel,
      Aggregator = aggregator,
      Filters = argument.Filters,
      IncludeSurveyedSurfaces = true,
      RequestedGridDataType = GridDataType.PassCount
    };

    /// <summary>
    /// Pull the required counts information from the internal Pass Count aggregator state
    /// </summary>
    /// <param name="aggregator"></param>
    /// <param name="response"></param>
    public override void ReadOutResults(AggregatorBase aggregator, PassCountStatisticsResponse response)
    {
      var tempAggregator = (DataStatisticsAggregator)aggregator;

      response.CellSize = tempAggregator.CellSize;
      response.SummaryCellsScanned = tempAggregator.SummaryCellsScanned;

      response.CellsScannedOverTarget = tempAggregator.CellsScannedOverTarget;
      response.CellsScannedUnderTarget = tempAggregator.CellsScannedUnderTarget;
      response.CellsScannedAtTarget = tempAggregator.CellsScannedAtTarget;

      response.IsTargetValueConstant = tempAggregator.IsTargetValueConstant;
      response.MissingTargetValue = tempAggregator.MissingTargetValue;

      response.LastPassCountTargetRange = ((PassCountStatisticsAggregator)aggregator).LastPassCountTargetRange;

      response.Counts = tempAggregator.Counts;
    }
  }
}
