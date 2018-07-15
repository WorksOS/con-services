using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric.Details
{
  /// <summary>
  /// CMV details specific request to make to the application service context
  /// </summary>
  public class CMVDetailsComputeFunc_ApplicationService : AnalyticsComputeFunc_ApplicationService<CMVDetailsArgument, DetailsAnalyticsResponse, CMVDetailsRequest_ClusterCompute>
  {
  }
}
