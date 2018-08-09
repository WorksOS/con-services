using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.CMVStatistics.Details;
using VSS.TRex.Analytics.CMVStatistics.GridFabric.Details;
using VSS.TRex.Analytics.Foundation.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Requests;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get CMV details.
  /// </summary>
  public class DetailedCMVExecutor : BaseExecutor
  {
    public DetailedCMVExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler, null, null)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DetailedCMVExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      CMVDetailsRequest request = item as CMVDetailsRequest;

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.filter, siteModel);

      CMVDetailsOperation operation = new CMVDetailsOperation();
      DetailsAnalyticsResult cmvDetailsResult = operation.Execute(new CMVDetailsArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        CMVDetailValues = request.customCMVDetailTargets
      });

      if (cmvDetailsResult != null)
        return CMVDetailedResult.Create(cmvDetailsResult.Percents);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested CMV  details data"));
    }

  }
}
