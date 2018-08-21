using VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.PassCountStatistics.Details;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric.Details
{
  /// <summary>
  /// Pass Count details specific request to make to the cluster compute context
  /// </summary>
  public class PassCountDetailsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<PassCountDetailsArgument, DetailsAnalyticsResponse, PassCountDetailsCoordinator>
  {
  }
}
