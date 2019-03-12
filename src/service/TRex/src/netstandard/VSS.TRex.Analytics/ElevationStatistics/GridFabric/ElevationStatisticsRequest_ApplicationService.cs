using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.ElevationStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a Elevation statistics request to be executed
  /// </summary>
  public class ElevationStatisticsRequest_ApplicationService : GenericASNodeRequest<ElevationStatisticsArgument, ElevationStatisticsComputeFunc_ApplicationService, ElevationStatisticsResponse>
  {
    public ElevationStatisticsRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}
