using VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric
{
  /// <summary>
  /// Pass Count statistics specific request to make to the application service context
  /// </summary>
  public class PassCountStatisticsComputeFunc_ApplicationService : AnalyticsComputeFunc_ApplicationService<PassCountStatisticsArgument, PassCountStatisticsResponse, PassCountStatisticsRequest_ClusterCompute>
  {
  }
}
