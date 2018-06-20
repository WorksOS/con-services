using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric.Details
{
  /// <summary>
  /// Sends a request to the grid for a CMV details request to be executed
  /// </summary>
  public class CMVDetailsRequest_ApplicationService : GenericASNodeRequest<CMVDetailsArgument, CMVDetailsComputeFunc_ApplicationService, CMVDetailsResponse>
  {
  }
}
