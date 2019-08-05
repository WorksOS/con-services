using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Analytics.CMVStatistics;
using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Types;
using CMVStatisticsResult = VSS.TRex.Analytics.CMVStatistics.CMVStatisticsResult;
using SummaryResult = VSS.Productivity3D.Models.ResultHandling.CMVSummaryResult;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get CMV summary.
  /// </summary>
  public class SummaryCMVExecutor : BaseExecutor
  {
    public SummaryCMVExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryCMVExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as CMVSummaryRequest;

      if (request == null)
        ThrowRequestTypeCastException<CMVSummaryRequest>();
      
      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      var operation = new CMVStatisticsOperation();
      var cmvSummaryResult = await operation.ExecuteAsync(
        new CMVStatisticsArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          Overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides),
          LiftParams = AutoMapperUtility.Automapper.Map<LiftParameters>(request.LiftSettings)
        }
      );

      if (cmvSummaryResult != null)
      {
        if (cmvSummaryResult.ResultStatus == RequestErrorStatus.OK)
          return ConvertResult(cmvSummaryResult);

        throw CreateServiceException<SummaryCMVExecutor>(cmvSummaryResult.ResultStatus);
      }

      throw CreateServiceException<SummaryCMVExecutor>();
    }

    private SummaryResult ConvertResult(CMVStatisticsResult summary)
    {
      return new SummaryResult(
        summary.WithinTargetPercent,
        summary.ConstantTargetCMV,
        summary.IsTargetCMVConstant,
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
