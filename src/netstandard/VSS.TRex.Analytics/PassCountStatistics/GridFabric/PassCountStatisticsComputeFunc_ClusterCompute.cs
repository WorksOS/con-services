using VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric
{
  /// <summary>
  /// Pass Count statistics specific request to make to the cluster compute context
  /// </summary>
  public class PassCountStatisticsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<PassCountStatisticsArgument, PassCountStatisticsResponse, PassCountStatisticsCoordinator>
  {
  }
}
