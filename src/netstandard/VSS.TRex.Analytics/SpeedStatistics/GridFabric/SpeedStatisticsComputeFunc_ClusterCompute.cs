using VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.SpeedStatistics.GridFabric
{
	/// <summary>
	/// Speed statistics specific request to make to the cluster compute context
	/// </summary>
  public class SpeedStatisticsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<SpeedStatisticsArgument, SpeedStatisticsResponse, SpeedCoordinator>
	{
  }
}
