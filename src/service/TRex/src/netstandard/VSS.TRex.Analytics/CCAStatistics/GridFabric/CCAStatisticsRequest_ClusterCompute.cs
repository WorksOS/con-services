using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CCAStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a CCA statistics request to be computed
  /// </summary>
  public class CCAStatisticsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<CCAStatisticsArgument, CCAStatisticsComputeFunc_ClusterCompute, CCAStatisticsResponse>
  {
  }
}
