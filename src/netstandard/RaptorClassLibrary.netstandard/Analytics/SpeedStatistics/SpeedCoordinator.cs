using VSS.TRex.Analytics.Aggregators;
using VSS.TRex.Analytics.Coordinators;
using VSS.TRex.Analytics.SpeedStatistics.GridFabric;

namespace VSS.TRex.Analytics.SpeedStatistics
{
	/// <summary>
	/// Computes Speed statistics. Executes in the 'application service' layer and acts as the coordinator
	/// for the request onto the cluster compute layer.
	/// </summary>
  public class SpeedCoordinator : BaseAnalyticsCoordinator<SpeedStatisticsArgument, SpeedStatisticsResponse>
	{
		public override AggregatorBase ConstructAggregator(SpeedStatisticsArgument argument)
		{
			throw new System.NotImplementedException();
		}

		public override AnalyticsComputor ConstructComputor(SpeedStatisticsArgument argument, AggregatorBase aggregator)
		{
			throw new System.NotImplementedException();
		}

		public override void ReadOutResults(AggregatorBase aggregator, SpeedStatisticsResponse response)
		{
			throw new System.NotImplementedException();
		}
	}
}
