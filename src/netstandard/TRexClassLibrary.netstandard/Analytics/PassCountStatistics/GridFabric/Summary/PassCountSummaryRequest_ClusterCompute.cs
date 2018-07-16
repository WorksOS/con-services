using VSS.TRex.Analytics.PassCountStatistics.GridFabric.Summary;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric.Summary
{
  /// <summary>
  /// Sends a request to the grid for a Pass Count summary request to be computed
  /// </summary>
  public class PassCountSummaryRequest_ClusterCompute : GenericPSNodeBroadcastRequest<PassCountSummaryArgument, PassCountSummaryComputeFunc_ClusterCompute, PassCountSummaryResponse>
  {
  }
}
