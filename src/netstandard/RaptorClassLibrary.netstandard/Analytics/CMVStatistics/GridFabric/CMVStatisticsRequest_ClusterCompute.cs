using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a CMV statistics request to be executed
  /// </summary>
  public class CMVStatisticsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<CMVStatisticsArgument, CMVStatisticsComputeFunc_ClusterCompute, CMVStatisticsResponse>
  {
  }
}
