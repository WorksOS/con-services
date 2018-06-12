using VSS.TRex.Analytics.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.MDPStatistics.GridFabric
{
  /// <summary>
  /// MDP statistics specific request to make to the cluster compute context
  /// </summary>
  public class MDPStatisticsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<MDPStatisticsArgument, MDPStatisticsResponse, MDPCoordinator>
  {
  }
}
