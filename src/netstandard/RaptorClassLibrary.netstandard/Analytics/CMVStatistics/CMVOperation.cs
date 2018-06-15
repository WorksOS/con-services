using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Analytics.Foundation;

namespace VSS.TRex.Analytics.CMVStatistics
{
  /// <summary>
  /// Provides a client onsumable operation for performing CMV analytics that returns a client model space CMV result.
  /// </summary>
  public class CMVOperation : AnalyticsOperation<CMVStatisticsRequest_ApplicationService, CMVStatisticsArgument, CMVStatisticsResponse, CMVResult>
  {
  }
}
