using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric.Summary;
using VSS.TRex.Analytics.PassCountStatistics.Summary;

namespace VSS.TRex.Analytics.PassCountStatistics
{
  /// <summary>
  /// Provides a client onsumable operation for performing Pass Count analytics that returns a client model space Pass Count result.
  /// </summary>
  public class PassCountSummaryOperation : AnalyticsOperation<PassCountSummaryRequest_ApplicationService, PassCountSummaryArgument, PassCountSummaryResponse, PassCountSummaryCountResult>
  {
  }
}
