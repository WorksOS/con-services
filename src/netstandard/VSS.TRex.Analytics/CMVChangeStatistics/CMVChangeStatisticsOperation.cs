using VSS.TRex.Analytics.CMVChangeStatistics.GridFabric;
using VSS.TRex.Analytics.Foundation;

namespace VSS.TRex.Analytics.CMVChangeStatistics
{
  /// <summary>
  /// Provides a client consumable operation for performing CMV change statistics analytics that returns a client model space CMV result.
  /// The CMV change is exposed on the client as CMV % change.
  /// </summary>
  public class CMVChangeStatisticsOperation : AnalyticsOperation<CMVChangeStatisticsRequest_ApplicationService, CMVChangeStatisticsArgument, CMVChangeStatisticsResponse, CMVChangeStatisticsResult>
  {
  }
}
