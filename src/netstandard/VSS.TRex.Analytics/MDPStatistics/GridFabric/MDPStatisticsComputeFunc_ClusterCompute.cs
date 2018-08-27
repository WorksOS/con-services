using VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs;
using VSS.TRex.Analytics.MDPStatistics.GridFabric;

namespace VSS.TRex.Analytics.MDPStatistics.GridFabricc
{
  /// <summary>
  /// MDP statistics specific request to make to the cluster compute context
  /// </summary>
  public class MDPStatisticsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<MDPStatisticsArgument, MDPStatisticsResponse, MDPCoordinator>
  {
  }
}
