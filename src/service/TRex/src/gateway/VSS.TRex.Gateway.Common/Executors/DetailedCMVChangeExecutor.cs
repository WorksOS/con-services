using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.CMVChangeStatistics;
using VSS.TRex.Analytics.CMVChangeStatistics.GridFabric;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Types;

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

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as CMVChangeDetailsRequest;

      if (request == null)
        ThrowRequestTypeCastException<CMVChangeDetailsRequest>();

      var siteModel = GetSiteModel(request?.ProjectUid);

      var filter = ConvertFilter(request?.Filter, siteModel);

      var operation = new CMVChangeStatisticsOperation();
      var cmvChangeDetailsResult = await operation.ExecuteAsync(new CMVChangeStatisticsArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        CMVChangeDetailsDataValues = request?.CMVChangeDetailsValues,
        Overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides),
        LiftParams = AutoMapperUtility.Automapper.Map<LiftParameters>(request.LiftSettings)
      });

      if (cmvChangeDetailsResult != null)
      {
        if (cmvChangeDetailsResult.ResultStatus == RequestErrorStatus.OK)
          return new CMVChangeSummaryResult(cmvChangeDetailsResult.Percents, cmvChangeDetailsResult.TotalAreaCoveredSqMeters);

        throw CreateServiceException<DetailedCMVChangeExecutor>(cmvChangeDetailsResult.ResultStatus);
      }

      throw CreateServiceException<DetailedCMVChangeExecutor>();
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
