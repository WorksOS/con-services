using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CMVPercentChangeStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a CMV % change statistics request to be computed
  /// </summary>
  public class CMVPercentChangeStatisticsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<CMVPercentChangeStatisticsArgument, CMVPercentChangeStatisticsComputeFunc_ClusterCompute, CMVPercentChangeStatisticsResponse>
  {
  }
}
