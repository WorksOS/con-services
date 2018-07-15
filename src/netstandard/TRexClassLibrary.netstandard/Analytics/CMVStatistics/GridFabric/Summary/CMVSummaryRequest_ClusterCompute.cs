using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric.Summary
{
  /// <summary>
  /// Sends a request to the grid for a CMV summary request to be computed
  /// </summary>
  public class CMVSummaryRequest_ClusterCompute : GenericPSNodeBroadcastRequest<CMVSummaryArgument, CMVSummaryComputeFunc_ClusterCompute, CMVSummaryResponse>
  {
  }
}
