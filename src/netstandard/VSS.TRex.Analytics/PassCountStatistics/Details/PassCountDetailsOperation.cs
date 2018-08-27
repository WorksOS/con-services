using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Models;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric.Details;

namespace VSS.TRex.Analytics.PassCountStatistics.Details
{
  /// <summary>
  /// Provides a client onsumable operation for performing Pass Count details analytics that returns a client model space CMV result.
  /// </summary>
  public class PassCountDetailsOperation : AnalyticsOperation<PassCountDetailsRequest_ApplicationService, PassCountDetailsArgument, DetailsAnalyticsResponse, DetailsAnalyticsResult>
  {
  }
}
