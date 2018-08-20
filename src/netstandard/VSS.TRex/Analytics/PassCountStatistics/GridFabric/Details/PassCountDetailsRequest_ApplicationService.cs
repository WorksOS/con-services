using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric.Details
{
  /// <summary>
  /// Sends a request to the grid for a Pass Count details request to be executed
  /// </summary>
  public class PassCountDetailsRequest_ApplicationService : GenericASNodeRequest<PassCountDetailsArgument, PassCountDetailsComputeFunc_ApplicationService, DetailsAnalyticsResponse>
  {
    public PassCountDetailsRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}
