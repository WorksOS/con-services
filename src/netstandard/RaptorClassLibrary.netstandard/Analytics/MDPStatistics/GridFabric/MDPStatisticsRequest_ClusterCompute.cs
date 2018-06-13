using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.MDPStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a MDP statistics request to be computed
  /// </summary>
  public class MDPStatisticsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<MDPStatisticsArgument, MDPStatisticsComputeFunc_ClusterCompute, MDPStatisticsResponse>
  {
  }
}
