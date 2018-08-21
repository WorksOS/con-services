using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric.Summary
{
  /// <summary>
  /// Sends a request to the grid for a Pass Count summary request to be executed
  /// </summary>
  public class PassCountSummaryRequest_ApplicationService : GenericASNodeRequest<PassCountSummaryArgument, PassCountSummaryComputeFunc_ApplicationService, PassCountSummaryResponse>
  {
    public PassCountSummaryRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}
