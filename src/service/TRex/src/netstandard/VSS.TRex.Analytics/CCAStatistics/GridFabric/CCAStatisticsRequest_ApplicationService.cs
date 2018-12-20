using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CCAStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a CCA statistics request to be executed
  /// </summary>
  public class CCAStatisticsRequest_ApplicationService : GenericASNodeRequest<CCAStatisticsArgument, CCAStatisticsComputeFunc_ApplicationService, CCAStatisticsResponse>
  {
    public CCAStatisticsRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}
