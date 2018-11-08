using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CutFillStatistics.GridFabric
{
    /// <summary>
    /// Sends a request to the grid for a cut fill statistics request to be executed
    /// </summary>
    public class CutFillStatisticsRequest_ApplicationService : GenericASNodeRequest<CutFillStatisticsArgument, CutFillStatisticsComputeFunc_ApplicationService, CutFillStatisticsResponse>
    {
      public CutFillStatisticsRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
      {
      }
  }
}

