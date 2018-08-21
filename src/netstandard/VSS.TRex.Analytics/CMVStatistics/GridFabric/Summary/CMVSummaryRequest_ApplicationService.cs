using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric.Summary
{
  /// <summary>
  /// Sends a request to the grid for a CMV summary request to be executed
  /// </summary>
  public class CMVSummaryRequest_ApplicationService : GenericASNodeRequest<CMVSummaryArgument, CMVSummaryComputeFunc_ApplicationService, CMVSummaryResponse>
  {
    public CMVSummaryRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}
