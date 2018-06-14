using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a Pass Count statistics request to be computed
  /// </summary>
  public class PassCountStatisticsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<PassCountStatisticsArgument, PassCountStatisticsComputeFunc_ClusterCompute, PassCountStatisticsResponse>
  {
  }
}
