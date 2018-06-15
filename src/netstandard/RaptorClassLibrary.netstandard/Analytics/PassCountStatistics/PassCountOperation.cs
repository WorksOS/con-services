using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;

namespace VSS.TRex.Analytics.PassCountStatistics
{
  /// <summary>
  /// Provides a client onsumable operation for performing Pass Count analytics that returns a client model space Pass Count result.
  /// </summary>
  public class PassCountOperation : AnalyticsOperation<PassCountStatisticsRequest_ApplicationService, PassCountStatisticsArgument, PassCountStatisticsResponse, PassCountResult>
  {
  }
}
