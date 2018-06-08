using VSS.TRex.Analytics.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric
{
  /// <summary>
  /// CMV statistics specific request to make to the cluster compute context
  /// </summary>
  public class CMVStatisticsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<CMVStatisticsArgument, CMVStatisticsResponse, CMVCoordinator>
  {
  }
}
