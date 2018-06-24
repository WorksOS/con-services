using VSS.TRex.Analytics.CMVStatistics.GridFabric.Details;
using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Models;

namespace VSS.TRex.Analytics.CMVStatistics.Details
{
  /// <summary>
  /// Provides a client consumable operation for performing CMV details analytics that returns a client model space CMV result.
  /// </summary>
  public class CMVDetailsOperation : AnalyticsOperation<CMVDetailsRequest_ApplicationService, CMVDetailsArgument, DetailsAnalyticsResponse, DetailsAnalyticsResult>
  {
  }
}
