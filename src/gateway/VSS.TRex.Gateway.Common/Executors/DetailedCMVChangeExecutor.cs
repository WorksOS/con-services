using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.CMVChangeStatistics;
using VSS.TRex.Analytics.CMVChangeStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Requests;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get CMV % details.
  /// </summary>
  public class DetailedCMVChangeExecutor : BaseExecutor
  {
    public DetailedCMVChangeExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DetailedCMVChangeExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      CMVChangeDetailsRequest request = item as CMVChangeDetailsRequest;

      if (request == null)
        ThrowRequestTypeCastException(typeof(CMVChangeDetailsRequest));

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      CMVChangeStatisticsOperation operation = new CMVChangeStatisticsOperation();
      CMVChangeStatisticsResult cmvChangeDetailsResult = operation.Execute(new CMVChangeStatisticsArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        CMVChangeDetailsDatalValues = request.CMVChangeDetailsValues
      });

      if (cmvChangeDetailsResult != null)
        return new CMVChangeSummaryResult(cmvChangeDetailsResult.Percents, cmvChangeDetailsResult.TotalAreaCoveredSqMeters);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested CMV Change details data"));
    }
  }
}
