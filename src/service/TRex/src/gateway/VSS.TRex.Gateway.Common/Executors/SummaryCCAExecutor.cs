using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.CCAStatistics;
using VSS.TRex.Analytics.CCAStatistics.GridFabric;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get CMV summary.
  /// </summary>
  public class SummaryCCAExecutor : BaseExecutor
  {
    public SummaryCCAExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryCCAExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as CCASummaryRequest;

      if (request == null)
        ThrowRequestTypeCastException<CCASummaryRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      var operation = new CCAStatisticsOperation();

      var ccaSummaryResult = await operation.ExecuteAsync(
        new CCAStatisticsArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          Overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides),
          LiftParams = AutoMapperUtility.Automapper.Map<LiftParameters>(request.LiftSettings)
        }
      );

      if (ccaSummaryResult != null)
      {
        if (ccaSummaryResult.ResultStatus == RequestErrorStatus.OK)
          return ConvertResult(ccaSummaryResult);

        throw CreateServiceException<SummaryCCAExecutor>(ccaSummaryResult.ResultStatus);
      }

      throw CreateServiceException<SummaryCCAExecutor>();
    }

    private CCASummaryResult ConvertResult(CCAStatisticsResult summary)
    {
      return CCASummaryResult.Create(
        summary.WithinTargetPercent,
        summary.AboveTargetPercent,
        (short) summary.ReturnCode,
        summary.TotalAreaCoveredSqMeters,
        summary.BelowTargetPercent);
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
