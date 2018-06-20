using VSS.TRex.Analytics.PassCountStatistics.GridFabric.Summary;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric.Summary
{
  /// <summary>
  /// Sends a request to the grid for a Pass Count summary request to be executed
  /// </summary>
  public class PassCountSummaryRequest_ApplicationService : GenericASNodeRequest<PassCountSummaryArgument, PassCountSummaryComputeFunc_ApplicationService, PassCountSummaryResponse>
  {
  }
}
