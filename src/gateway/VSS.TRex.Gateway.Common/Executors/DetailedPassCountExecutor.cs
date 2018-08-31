using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.Foundation.Models;
using VSS.TRex.Analytics.PassCountStatistics.Details;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric.Details;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Requests;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get Pass Count details.
  /// </summary>
  public class DetailedPassCountExecutor : BaseExecutor
  {
    public DetailedPassCountExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DetailedPassCountExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      const ushort MIN_TARGET_PASS_COUNT = 1;
      const ushort MAX_TARGET_PASS_COUNT = ushort.MaxValue;
      const double DUMMY_TOTAL_AREA = 0.0;

      PassCountDetailsRequest request = item as PassCountDetailsRequest;

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.filter, siteModel);

      PassCountDetailsOperation operation = new PassCountDetailsOperation();
      DetailsAnalyticsResult passCountDetailsResult = operation.Execute(new PassCountDetailsArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        PassCountDetailValues = request.passCounts
      });

      if (passCountDetailsResult != null)
        return new PassCountDetailedResult(new TargetPassCountRange(
          MIN_TARGET_PASS_COUNT, MAX_TARGET_PASS_COUNT),  
          false,
          passCountDetailsResult.Percents,
          DUMMY_TOTAL_AREA
        );

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Pass Count details data"));
    }

  }
}
