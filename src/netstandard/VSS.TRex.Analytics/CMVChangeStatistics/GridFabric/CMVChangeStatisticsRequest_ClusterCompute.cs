using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CMVChangeStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a CMV change statistics request to be computed
  /// </summary>
  public class CMVChangeStatisticsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<CMVChangeStatisticsArgument, CMVChangeStatisticsComputeFunc_ClusterCompute, CMVChangeStatisticsResponse>
  {
  }
}
