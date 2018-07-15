using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric.Details
{
  /// <summary>
  /// Pass Count details specific request to make to the application service context
  /// </summary>
  public class PassCountDetailsComputeFunc_ApplicationService : AnalyticsComputeFunc_ApplicationService<PassCountDetailsArgument, DetailsAnalyticsResponse, PassCountDetailsRequest_ClusterCompute>
  {
  }
}
