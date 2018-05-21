using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.SpeedStatistics.GridFabric
{
	/// <summary>
	/// Sends a request to the grid for a Speed statistics request to be executed
	/// </summary>
  public class SpeedStatisticsRequest_ApplicationService : GenericASNodeRequest<SpeedStatisticsArgument, SpeedStatisticsComputeFunc_ApplicationService, SpeedStatisticsResponse>
	{
  }
}
