using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Analytics.Foundation.Coordinators;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric.Details;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.PassCountStatistics.Details
{
  /// <summary>
  /// Computes Pass Count details. Executes in the 'application service' layer and acts as the coordinator
  /// for the request onto the cluster compute layer.
  /// </summary>
  public class PassCountDetailsCoordinator : BaseAnalyticsCoordinator<PassCountDetailsArgument, DetailsAnalyticsResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Constructs the aggregator from the supplied argument to be used for the Pass Count details analytics request
    /// Create the aggregator to collect and reduce the results.
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public override AggregatorBase ConstructAggregator(PassCountDetailsArgument argument) => new PassCountDetailsAggregator()
    {
      RequiresSerialisation = true,
      SiteModelID = argument.ProjectID,
      //LiftBuildSettings := LiftBuildSettings;
      CellSize = SiteModel.Grid.CellSize,
      DetailsDataValues = argument.PassCountDetailValues,
      Counts = new long[argument.PassCountDetailValues.Length]
    };

    /// <summary>
    /// Constructs the computer from the supplied argument and aggregator for the Pass Count details analytics request
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="aggregator"></param>
    /// <returns></returns>
    public override AnalyticsComputor ConstructComputor(PassCountDetailsArgument argument, AggregatorBase aggregator) => new AnalyticsComputor()
    {
      RequestDescriptor = RequestDescriptor,
      SiteModel = SiteModel,
      Aggregator = aggregator,
      Filters = argument.Filters,
      IncludeSurveyedSurfaces = true,
      RequestedGridDataType = GridDataType.PassCount
    };

    /// <summary>
    /// Pull the required counts information from the internal Pass Count details aggregator state
    /// </summary>
    /// <param name="aggregator"></param>
    /// <param name="response"></param>
    public override void ReadOutResults(AggregatorBase aggregator, DetailsAnalyticsResponse response)
    {
      response.Counts = ((PassCountDetailsAggregator)aggregator).Counts;
    }
  }
}
