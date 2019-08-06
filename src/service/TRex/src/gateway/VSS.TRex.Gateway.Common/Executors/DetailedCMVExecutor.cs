using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.CMVStatistics;
using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
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

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as CMVDetailsRequest;

      if (request == null)
        ThrowRequestTypeCastException<CMVDetailsRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      var operation = new CMVStatisticsOperation();
      var cmvDetailsResult = await operation.ExecuteAsync(new CMVStatisticsArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        CMVDetailValues = request.CustomCMVDetailTargets,
        Overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides),
        LiftParams = ConvertLift(request.LiftSettings, request.Filter?.LayerType)
      });

      if (cmvDetailsResult != null)
      {
        if (cmvDetailsResult.ResultStatus == RequestErrorStatus.OK)
          return new CMVDetailedResult(cmvDetailsResult.Percents, cmvDetailsResult.ConstantTargetCMV, cmvDetailsResult.IsTargetCMVConstant);

        throw CreateServiceException<DetailedCMVExecutor>(cmvDetailsResult.ResultStatus);
      }

      throw CreateServiceException<DetailedCMVExecutor>();
    }

    /// <summary>
    /// Processes the tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
