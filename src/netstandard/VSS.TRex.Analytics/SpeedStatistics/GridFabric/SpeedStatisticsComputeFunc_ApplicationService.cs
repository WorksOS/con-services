using VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.SpeedStatistics.GridFabric
{
	/// <summary>
	/// Speed statistics specific request to make to the application service context
	/// </summary>
  public class SpeedStatisticsComputeFunc_ApplicationService : AnalyticsComputeFunc_ApplicationService<SpeedStatisticsArgument, SpeedStatisticsResponse, SpeedStatisticsRequest_ClusterCompute>
	{
  }
}
