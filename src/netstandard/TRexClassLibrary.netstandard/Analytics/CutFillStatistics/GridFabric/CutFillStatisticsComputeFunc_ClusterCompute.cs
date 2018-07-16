using VSS.TRex.Analytics.Coordinators;
using VSS.TRex.Analytics.GridFabric.Arguments;
using VSS.TRex.Analytics.GridFabric.Responses;

namespace VSS.TRex.Analytics.GridFabric.ComputeFuncs
{
  /// <summary>
  /// Cut/fill statistics specific request to make to the cluster compute context
  /// </summary>
  public class CutFillStatisticsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<CutFillStatisticsArgument, CutFillStatisticsResponse, CutFillCoordinator>
  {
  }
}
