using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Analytics.Aggregators;
using VSS.TRex.Analytics.CMVStatistics.GridFabric.Summary;
using VSS.TRex.Analytics.Coordinators;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVStatistics.Summary
{
  /// <summary>
  /// Computes CMV summary. Executes in the 'application service' layer and acts as the coordinator
  /// for the request onto the cluster compute layer.
  /// </summary>
  public class CMVSummaryCoordinator : BaseAnalyticsCoordinator<CMVSummaryArgument, CMVSummaryResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Constructs the aggregator from the supplied argument to be used for the CMV summary analytics request
    /// Create the aggregator to collect and reduce the results.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public override AggregatorBase ConstructAggregator(CMVSummaryArgument argument) => new CMVSummaryAggregator
    {
      RequiresSerialisation = true,
      SiteModelID = argument.ProjectID,
      //LiftBuildSettings := LiftBuildSettings;
      CellSize = SiteModel.Grid.CellSize,
      OverrideMachineCMV = argument.OverrideMachineCMV,
      OverridingMachineCMV = argument.OverridingMachineCMV,
      CMVPercentageRange = argument.CMVPercentageRange
    };

    /// <summary>
    /// Constructs the computer from the supplied argument and aggregator for the CMV summary analytics request
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="aggregator"></param>
    /// <returns></returns>
    public override AnalyticsComputor ConstructComputor(CMVSummaryArgument argument, AggregatorBase aggregator) => new AnalyticsComputor()
    {
      RequestDescriptor = RequestDescriptor,
      SiteModel = SiteModel,
      Aggregator = aggregator,
      Filters = argument.Filters,
      IncludeSurveyedSurfaces = true,
      RequestedGridDataType = GridDataType.CCV
    };

    /// <summary>
    /// Pull the required counts information from the internal CMV summary aggregator state
    /// </summary>
    /// <param name="aggregator"></param>
    /// <param name="response"></param>
    public override void ReadOutResults(AggregatorBase aggregator, CMVSummaryResponse response)
    {
      var tempAggregator = (DataStatisticsAggregator) aggregator;

      response.CellSize = tempAggregator.CellSize;
      response.SummaryCellsScanned = tempAggregator.SummaryCellsScanned;

      response.CellsScannedOverTarget = tempAggregator.CellsScannedOverTarget;
      response.CellsScannedUnderTarget = tempAggregator.CellsScannedUnderTarget;
      response.CellsScannedAtTarget = tempAggregator.CellsScannedAtTarget;

      response.IsTargetValueConstant = tempAggregator.IsTargetValueConstant;
      response.MissingTargetValue = tempAggregator.MissingTargetValue;

      response.LastTargetCMV = ((CMVSummaryAggregator)aggregator).LastTargetCMV;
    }
  }
}
