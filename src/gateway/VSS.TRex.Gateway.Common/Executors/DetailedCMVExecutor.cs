using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.CMVStatistics;
using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get CMV details.
  /// </summary>
  public class DetailedCMVExecutor : BaseExecutor
  {
    public DetailedCMVExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
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
      const string ERROR_MESSAGE = "Failed to get requested CMV details data";

      CMVDetailsRequest request = item as CMVDetailsRequest;

      if (request == null)
        ThrowRequestTypeCastException(typeof(CMVDetailsRequest));

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      CMVStatisticsOperation operation = new CMVStatisticsOperation();
      CMVStatisticsResult cmvDetailsResult = operation.Execute(new CMVStatisticsArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        CMVDetailValues = request.CustomCMVDetailTargets
      });

      if (cmvDetailsResult != null)
      {
        if (cmvDetailsResult.ResultStatus == RequestErrorStatus.OK)
          return new CMVDetailedResult(cmvDetailsResult.Percents, cmvDetailsResult.ConstantTargetCMV, cmvDetailsResult.IsTargetCMVConstant);

        throw CreateServiceException(ERROR_MESSAGE, cmvDetailsResult.ResultStatus);
      }

      throw CreateServiceException(ERROR_MESSAGE);
    }

  }
}
