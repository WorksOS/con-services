using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Analytics.Aggregators;
using VSS.TRex.Analytics.Coordinators;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.TemperatureStatistics
{
	/// <summary>
	/// Computes Temperature statistics. Executes in the 'application service' layer and acts as the coordinator
	/// for the request onto the cluster compute layer.
	/// </summary>
	public class TemperatureCoordinator : BaseAnalyticsCoordinator<TemperatureStatisticsArgument, TemperatureStatisticsResponse>
	{
		private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

		/// <summary>
		/// Constructs the aggregator from the supplied argument to be used for the Temperature statistics analytics request
		/// Create the aggregator to collect and reduce the results.
		/// </summary>
		/// <param name="argument"></param>
		/// <returns></returns>
		public override AggregatorBase ConstructAggregator(TemperatureStatisticsArgument argument) => new TemperatureAggregator
		{
			RequiresSerialisation = true,
			SiteModelID = argument.ProjectID,
			//LiftBuildSettings := LiftBuildSettings;
			CellSize = SiteModel.Grid.CellSize,
			OverrideTemperatureWarningLevels = argument.OverrideTemperatureWarningLevels,
			OverridingTemperatureWarningLevels = argument.OverridingTemperatureWarningLevels
		};

		/// <summary>
		/// Constructs the computer from the supplied argument and aggregator for the Temperature statistics analytics request
		/// </summary>
		/// <param name="argument"></param>
		/// <param name="aggregator"></param>
		/// <returns></returns>
		public override AnalyticsComputor ConstructComputor(TemperatureStatisticsArgument argument, AggregatorBase aggregator) => new AnalyticsComputor()
		{
			RequestDescriptor = RequestDescriptor,
			SiteModel = SiteModel,
			Aggregator = aggregator,
			Filters = argument.Filters,
			IncludeSurveyedSurfaces = true,
			RequestedGridDataType = GridDataType.Temperature
		};

		/// <summary>
		/// Pull the required counts information from the internal Temperature aggregator state
		/// </summary>
		/// <param name="aggregator"></param>
		/// <param name="response"></param>
		public override void ReadOutResults(AggregatorBase aggregator, TemperatureStatisticsResponse response)
		{
		  response.CellSize = ((SummaryAggregator)aggregator).CellSize;
      response.SummaryCellsScanned = ((SummaryAggregator)aggregator).SummaryCellsScanned;

      response.CellsScannedOverTarget = ((SummaryAggregator)aggregator).CellsScannedOverTarget;
			response.CellsScannedUnderTarget = ((SummaryAggregator)aggregator).CellsScannedUnderTarget;
			response.CellsScannedAtTarget = ((SummaryAggregator)aggregator).CellsScannedAtTarget;

      response.IsTargetValueConstant = ((SummaryAggregator)aggregator).IsTargetValueConstant;
      response.MissingTargetValue = ((SummaryAggregator)aggregator).MissingTargetValue;

			response.LastTempRangeMin = ((TemperatureAggregator) aggregator).LastTempRangeMin;
			response.LastTempRangeMax = ((TemperatureAggregator)aggregator).LastTempRangeMax;
		}
	}
}
