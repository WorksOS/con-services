using Microsoft.Extensions.Logging;
using System.Reflection;
using VSS.TRex.Analytics.Aggregators;
using VSS.TRex.Analytics.Coordinators;
using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.SpeedStatistics
{
	/// <summary>
	/// Computes Speed statistics. Executes in the 'application service' layer and acts as the coordinator
	/// for the request onto the cluster compute layer.
	/// </summary>
  public class SpeedCoordinator : BaseAnalyticsCoordinator<SpeedStatisticsArgument, SpeedStatisticsResponse>
	{
		private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

		/// <summary>
		/// Constructs the aggregator from the supplied argument to be used for the Speed statistics analytics request
		/// Create the aggregator to collect and reduce the results.
		/// </summary>
		/// <param name="argument"></param>
		/// <returns></returns>
		public override AggregatorBase ConstructAggregator(SpeedStatisticsArgument argument) => new SpeedAggregator
		{
			RequiresSerialisation = true,
			SiteModelID = argument.DataModelID,
			//LiftBuildSettings := LiftBuildSettings;
			CellSize = SiteModel.Grid.CellSize,
			TargetMinMachineSpeed = argument.TargetMinMachineSpeed,
			TargetMaxMachineSpeed = argument.TargetMaxMachineSpeed
		};

		/// <summary>
		/// Constructs the computer from the supplied argument and aggregator for the Speed statistics analytics request
		/// </summary>
		/// <param name="argument"></param>
		/// <param name="aggregator"></param>
		/// <returns></returns>
		public override AnalyticsComputor ConstructComputor(SpeedStatisticsArgument argument, AggregatorBase aggregator) => new AnalyticsComputor()
		{
			RequestDescriptor = RequestDescriptor,
			SiteModel = SiteModel,
			Aggregator = aggregator,
			Filter = argument.Filter,
			IncludeSurveyedSurfaces = true,
			RequestedGridDataType = GridDataType.MachineSpeed
		};

		/// <summary>
		/// Pull the required counts information from the internal Speed aggregator state
		/// </summary>
		/// <param name="aggregator"></param>
		/// <param name="response"></param>
		public override void ReadOutResults(AggregatorBase aggregator, SpeedStatisticsResponse response)
		{
			response.AboveTargetCellsCount = ((SpeedAggregator)aggregator).AboveTargetCellsCount;
			response.BelowTargetCellsCount = ((SpeedAggregator)aggregator).BelowTargetCellsCount;
			response.MatchTargetCellsCount = ((SpeedAggregator)aggregator).MatchTargetCellsCount;
			response.CoverageArea = ((SpeedAggregator)aggregator).CoverageArea;
		}
	}
}
