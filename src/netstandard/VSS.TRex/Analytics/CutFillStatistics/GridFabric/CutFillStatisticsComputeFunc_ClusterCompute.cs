using VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.CutFillStatistics.GridFabric
{
  /// <summary>
  /// Cut/fill statistics specific request to make to the cluster compute context
  /// </summary>
  public class CutFillStatisticsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<CutFillStatisticsArgument, CutFillStatisticsResponse, CutFillCoordinator>
  {
  }
}
