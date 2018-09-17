using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CMVChangeStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a CMV change statistics request to be executed
  /// The CMV change is exposed on the client as CMV % change.
  /// </summary>
  public class CMVChangeStatisticsRequest_ApplicationService : GenericASNodeRequest<CMVChangeStatisticsArgument, CMVChangeStatisticsComputeFunc_ApplicationService, CMVChangeStatisticsResponse>
  {
    public CMVChangeStatisticsRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}
