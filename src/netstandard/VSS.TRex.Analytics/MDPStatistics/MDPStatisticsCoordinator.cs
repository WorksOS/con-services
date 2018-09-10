using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Analytics.Foundation.Coordinators;
using VSS.TRex.Analytics.MDPStatistics.GridFabric;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.MDPStatistics
{
  /// <summary>
  /// Computes MDP statistics. Executes in the 'application service' layer and acts as the coordinator
  /// for the request onto the cluster compute layer.
  /// </summary>
  public class MDPStatisticsCoordinator : BaseAnalyticsCoordinator<MDPStatisticsArgument, MDPStatisticsResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Constructs the aggregator from the supplied argument to be used for the MDP statistics analytics request
    /// Create the aggregator to collect and reduce the results.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public override AggregatorBase ConstructAggregator(MDPStatisticsArgument argument) => new MDPStatisticsAggregator
    {
      RequiresSerialisation = true,
      SiteModelID = argument.ProjectID,
      //LiftBuildSettings := LiftBuildSettings;
      CellSize = SiteModel.Grid.CellSize,
      OverrideMachineMDP = argument.OverrideMachineMDP,
      OverridingMachineMDP = argument.OverridingMachineMDP,
      MDPPercentageRange = argument.MDPPercentageRange,
      DetailsDataValues = argument.MDPDetailValues,
      Counts = new long[argument.MDPDetailValues.Length]
    };

    /// <summary>
    /// Constructs the computor from the supplied argument and aggregator for the MDP statistics analytics request
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="aggregator"></param>
    /// <returns></returns>
    public override AnalyticsComputor ConstructComputor(MDPStatisticsArgument argument, AggregatorBase aggregator) => new AnalyticsComputor()
    {
      RequestDescriptor = RequestDescriptor,
      SiteModel = SiteModel,
      Aggregator = aggregator,
      Filters = argument.Filters,
      IncludeSurveyedSurfaces = true,
      RequestedGridDataType = GridDataType.MDP
    };

    /// <summary>
    /// Pull the required counts information from the internal MDP aggregator state
    /// </summary>
    /// <param name="aggregator"></param>
    /// <param name="response"></param>
    public override void ReadOutResults(AggregatorBase aggregator, MDPStatisticsResponse response)
    {
      var tempAggregator = (SummaryDataAggregator)aggregator;

      response.CellSize = tempAggregator.CellSize;
      response.SummaryCellsScanned = tempAggregator.SummaryCellsScanned;

      response.CellsScannedOverTarget = tempAggregator.CellsScannedOverTarget;
      response.CellsScannedUnderTarget = tempAggregator.CellsScannedUnderTarget;
      response.CellsScannedAtTarget = tempAggregator.CellsScannedAtTarget;

      response.IsTargetValueConstant = tempAggregator.IsTargetValueConstant;
      response.MissingTargetValue = tempAggregator.MissingTargetValue;

      response.LastTargetMDP = ((MDPStatisticsAggregator)aggregator).LastTargetMDP;

      response.Counts = tempAggregator.Counts;
    }
  }
}
