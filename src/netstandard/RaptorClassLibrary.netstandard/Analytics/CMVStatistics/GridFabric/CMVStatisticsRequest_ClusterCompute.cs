using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric
{
  public class CMVStatisticsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<CMVStatisticsArgument, CMVStatisticsComputeFunc_ClusterCompute, CMVStatisticsResponse>
  {
  }
}
