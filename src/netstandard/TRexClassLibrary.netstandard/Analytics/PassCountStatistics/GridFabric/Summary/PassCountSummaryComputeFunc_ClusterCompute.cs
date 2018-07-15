using VSS.TRex.Analytics.GridFabric.ComputeFuncs;
using VSS.TRex.Analytics.PassCountStatistics.Summary;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric.Summary
{
  /// <summary>
  /// Pass Count summary specific request to make to the cluster compute context
  /// </summary>
  public class PassCountSummaryComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<PassCountSummaryArgument, PassCountSummaryResponse, PassCountSummaryCoordinator>
  {
  }
}
