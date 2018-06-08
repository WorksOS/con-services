using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Analytics.Aggregators;
using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Analytics.Coordinators;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Analytics.TemperatureStatistics;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVStatistics
{
  /// <summary>
  /// Computes CMV statistics. Executes in the 'application service' layer and acts as the coordinator
  /// for the request onto the cluster compute layer.
  /// </summary>
  public class CMVCoordinator : BaseAnalyticsCoordinator<CMVStatisticsArgument, CMVStatisticsResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Constructs the aggregator from the supplied argument to be used for the CMV statistics analytics request
    /// Create the aggregator to collect and reduce the results.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public override AggregatorBase ConstructAggregator(CMVStatisticsArgument argument) => new CMVAggregator
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
    /// Constructs the computer from the supplied argument and aggregator for the CMV statistics analytics request
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="aggregator"></param>
    /// <returns></returns>
    public override AnalyticsComputor ConstructComputor(CMVStatisticsArgument argument, AggregatorBase aggregator) => new AnalyticsComputor()
    {
      RequestDescriptor = RequestDescriptor,
      SiteModel = SiteModel,
      Aggregator = aggregator,
      Filters = argument.Filters,
      IncludeSurveyedSurfaces = true,
      RequestedGridDataType = GridDataType.CCV
    };

    /// <summary>
    /// Pull the required counts information from the internal CMV aggregator state
    /// </summary>
    /// <param name="aggregator"></param>
    /// <param name="response"></param>
    public override void ReadOutResults(AggregatorBase aggregator, CMVStatisticsResponse response)
    {
      response.CellSize = ((DataStatisticsAggregator)aggregator).CellSize;
      response.SummaryCellsScanned = ((DataStatisticsAggregator)aggregator).SummaryCellsScanned;

      response.CellsScannedOverTarget = ((DataStatisticsAggregator)aggregator).CellsScannedOverTarget;
      response.CellsScannedUnderTarget = ((DataStatisticsAggregator)aggregator).CellsScannedUnderTarget;
      response.CellsScannedAtTarget = ((DataStatisticsAggregator)aggregator).CellsScannedAtTarget;

      response.IsTargetValueConstant = ((DataStatisticsAggregator)aggregator).IsTargetValueConstant;
      response.MissingTargetValue = ((DataStatisticsAggregator)aggregator).MissingTargetValue;

      response.LastTargetCMV = ((CMVAggregator)aggregator).LastTargetCMV;
    }
  }
}
