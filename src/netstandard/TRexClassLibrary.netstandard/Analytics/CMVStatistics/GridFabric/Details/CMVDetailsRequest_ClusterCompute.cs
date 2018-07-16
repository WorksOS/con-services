using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric.Details
{
  /// <summary>
  /// Sends a request to the grid for a CMV details request to be computed
  /// </summary>
  public class CMVDetailsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<CMVDetailsArgument, CMVDetailsComputeFunc_ClusterCompute, DetailsAnalyticsResponse>
  {
  }
}
