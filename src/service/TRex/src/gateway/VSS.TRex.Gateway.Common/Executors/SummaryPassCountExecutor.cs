using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Common.Records;
using VSS.TRex.Filters;
using VSS.TRex.Types;
using PassCountStatisticsResult = VSS.TRex.Analytics.PassCountStatistics.PassCountStatisticsResult;
using SummaryResult = VSS.Productivity3D.Models.ResultHandling.PassCountSummaryResult;
using TargetPassCountRange = VSS.Productivity3D.Models.Models.TargetPassCountRange;


namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get Pass Count summary.
  /// </summary>
  public class SummaryPassCountExecutor : BaseExecutor
  {
    public SummaryPassCountExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryPassCountExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as PassCountSummaryRequest;

      if (request == null)
        ThrowRequestTypeCastException< PassCountSummaryRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      var targetPassCountRange = new PassCountRangeRecord();
      var overridingTargetPassCountRange = request.Overrides.OverridingTargetPassCountRange;
      if (overridingTargetPassCountRange != null)
        targetPassCountRange.SetMinMax(overridingTargetPassCountRange.Min, overridingTargetPassCountRange.Max);

      var operation = new PassCountStatisticsOperation();
      var passCountSummaryResult = await operation.ExecuteAsync(
        new PassCountStatisticsArgument
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          OverridingTargetPassCountRange = targetPassCountRange,
          OverrideTargetPassCount = overridingTargetPassCountRange != null
        }
      );

      if (passCountSummaryResult != null)
      {
        if (passCountSummaryResult.ResultStatus == RequestErrorStatus.OK)
          return ConvertResult(passCountSummaryResult);

        throw CreateServiceException<SummaryPassCountExecutor>(passCountSummaryResult.ResultStatus);
      }

      throw CreateServiceException<SummaryPassCountExecutor>();
    }

    private SummaryResult ConvertResult(PassCountStatisticsResult summary)
    {
      return new SummaryResult(
        new TargetPassCountRange(summary.ConstantTargetPassCountRange.Min, summary.ConstantTargetPassCountRange.Max),
        summary.IsTargetPassCountConstant,
        summary.WithinTargetPercent,
        summary.AboveTargetPercent,
        summary.BelowTargetPercent,
        (short)summary.ReturnCode,
        summary.TotalAreaCoveredSqMeters);
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
