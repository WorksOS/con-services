using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Analytics.Aggregators;
using VSS.TRex.Analytics.CMVStatistics.GridFabric.Details;
using VSS.TRex.Analytics.Coordinators;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVStatistics.Details
{
  /// <summary>
  /// Computes CMV details. Executes in the 'application service' layer and acts as the coordinator
  /// for the request onto the cluster compute layer.
  /// </summary>
  public class CMVDetailsCoordinator : BaseAnalyticsCoordinator<CMVDetailsArgument, CMVDetailsResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Constructs the aggregator from the supplied argument to be used for the CMV details analytics request
    /// Create the aggregator to collect and reduce the results.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public override AggregatorBase ConstructAggregator(CMVDetailsArgument argument) => new CMVDetailsAggregator()
    {
      CMVDetailValues = argument.CMVDetailValues
    };

    /// <summary>
    /// Constructs the computer from the supplied argument and aggregator for the CMV details analytics request
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="aggregator"></param>
    /// <returns></returns>
    public override AnalyticsComputor ConstructComputor(CMVDetailsArgument argument, AggregatorBase aggregator) => new AnalyticsComputor()
    {
      RequestDescriptor = RequestDescriptor,
      SiteModel = SiteModel,
      Aggregator = aggregator,
      Filters = argument.Filters,
      IncludeSurveyedSurfaces = true,
      RequestedGridDataType = GridDataType.CCV
    };

    /// <summary>
    /// Pull the required counts information from the internal CMV details aggregator state
    /// </summary>
    /// <param name="aggregator"></param>
    /// <param name="response"></param>
    public override void ReadOutResults(AggregatorBase aggregator, CMVDetailsResponse response)
    {
      response.Counts = ((CMVDetailsAggregator)aggregator).Counts;
    }
  }
}
