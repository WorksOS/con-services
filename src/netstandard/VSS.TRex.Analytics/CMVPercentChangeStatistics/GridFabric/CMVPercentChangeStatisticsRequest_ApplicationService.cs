using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CMVPercentChangeStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a CMV % change statistics request to be executed
  /// </summary>
  public class CMVPercentChangeStatisticsRequest_ApplicationService : GenericASNodeRequest<CMVPercentChangeStatisticsArgument, CMVPercentChangeStatisticsComputeFunc_ApplicationService, CMVPercentChangeStatisticsResponse>
  {
    public CMVPercentChangeStatisticsRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}
