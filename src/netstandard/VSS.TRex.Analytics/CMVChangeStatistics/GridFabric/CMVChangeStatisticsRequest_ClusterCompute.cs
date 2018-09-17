using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CMVChangeStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a CMV change statistics request to be computed.
  /// The CMV change is exposed on the client as CMV % change.
  /// </summary>
  public class CMVChangeStatisticsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<CMVChangeStatisticsArgument, CMVChangeStatisticsComputeFunc_ClusterCompute, CMVChangeStatisticsResponse>
  {
  }
}
