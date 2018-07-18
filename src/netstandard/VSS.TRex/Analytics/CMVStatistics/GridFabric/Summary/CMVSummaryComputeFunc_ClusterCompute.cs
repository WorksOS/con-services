using VSS.TRex.Analytics.CMVStatistics.Summary;
using VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric.Summary
{
  /// <summary>
  /// CMV summary specific request to make to the cluster compute context
  /// </summary>
  public class CMVSummaryComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<CMVSummaryArgument, CMVSummaryResponse, CMVSummaryCoordinator>
  {
  }
}
