using VSS.TRex.Analytics.CMVStatistics.Details;
using VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric.Details
{
  /// <summary>
  /// CMV details specific request to make to the cluster compute context
  /// </summary>
  public class CMVDetailsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<CMVDetailsArgument, DetailsAnalyticsResponse, CMVDetailsCoordinator>
  {
  }
}
