using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric.Details
{
  /// <summary>
  /// Sends a request to the grid for a CMV details request to be executed
  /// </summary>
  public class CMVDetailsRequest_ApplicationService : GenericASNodeRequest<CMVDetailsArgument, CMVDetailsComputeFunc_ApplicationService, DetailsAnalyticsResponse>
  {
    public CMVDetailsRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}
