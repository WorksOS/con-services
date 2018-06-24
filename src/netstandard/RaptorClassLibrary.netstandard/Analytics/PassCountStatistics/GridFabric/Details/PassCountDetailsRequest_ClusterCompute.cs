using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric.Details
{
  /// <summary>
  /// Sends a request to the grid for a Pass Count details request to be computed
  /// </summary>
  public class PassCountDetailsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<PassCountDetailsArgument, PassCountDetailsComputeFunc_ClusterCompute, DetailsAnalyticsResponse>
  {
  }
}
