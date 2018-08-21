using VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric.Summary
{
  /// <summary>
  /// Pass Count summary specific request to make to the application service context
  /// </summary>
  public class PassCountSummaryComputeFunc_ApplicationService : AnalyticsComputeFunc_ApplicationService<PassCountSummaryArgument, PassCountSummaryResponse, PassCountSummaryRequest_ClusterCompute>
  {
  }
}
