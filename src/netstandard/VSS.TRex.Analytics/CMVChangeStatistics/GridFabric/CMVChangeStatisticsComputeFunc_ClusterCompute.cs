using VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.CMVChangeStatistics.GridFabric
{
  /// <summary>
  /// CMV change statistics specific request to make to the cluster compute context
  /// The CMV change is exposed on the client as CMV % change.
  /// </summary>
  public class CMVChangeStatisticsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<CMVChangeStatisticsArgument, CMVChangeStatisticsResponse, CMVChangeStatisticsCoordinator>
  {
  }
}
