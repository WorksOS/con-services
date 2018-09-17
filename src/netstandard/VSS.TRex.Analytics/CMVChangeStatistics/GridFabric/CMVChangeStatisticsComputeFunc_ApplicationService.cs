using VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.CMVChangeStatistics.GridFabric
{
  /// <summary>
  /// CMV change statistics specific request to make to the application service context.
  /// The CMV change is exposed on the client as CMV % change.
  /// </summary>
  public class CMVChangeStatisticsComputeFunc_ApplicationService : AnalyticsComputeFunc_ApplicationService<CMVChangeStatisticsArgument, CMVChangeStatisticsResponse, CMVChangeStatisticsRequest_ClusterCompute>
  {
  }
}
