using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.TemperatureStatistics.GridFabric
{
	/// <summary>
	/// Sends a request to the grid for a Temperature statistics request to be executed
	/// </summary>
	public class TemperatureStatisticsRequest_ApplicationService : GenericASNodeRequest<TemperatureStatisticsArgument, TemperatureStatisticsComputeFunc_ApplicationService, TemperatureStatisticsResponse>
	{
	  public TemperatureStatisticsRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
	  {
	  }
  }
}
