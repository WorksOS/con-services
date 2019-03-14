using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.ElevationStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a Elevation statistics request to be computed
  /// </summary>
  public class ElevationStatisticsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<ElevationStatisticsArgument, ElevationStatisticsComputeFunc_ClusterCompute, ElevationStatisticsResponse>
  {
  }
}
