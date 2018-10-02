using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      const string ERROR_MESSAGE = "Failed to get requested Pass Count summary data";

      PassCountSummaryRequest request = item as PassCountSummaryRequest;

      if (request == null)
        ThrowRequestTypeCastException(typeof(PassCountSummaryRequest));

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      var targetPassCountRange = new PassCountRangeRecord();
      if (request.OverridingTargetPassCountRange != null)
        targetPassCountRange.SetMinMax(request.OverridingTargetPassCountRange.Min, request.OverridingTargetPassCountRange.Max);

      PassCountStatisticsOperation operation = new PassCountStatisticsOperation();
      PassCountStatisticsResult passCountSummaryResult = operation.Execute(
        new PassCountStatisticsArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          OverridingTargetPassCountRange = targetPassCountRange,
          OverrideTargetPassCount = request.OverridingTargetPassCountRange != null
        }
      );

      if (passCountSummaryResult != null)
      {
        if (passCountSummaryResult.ResultStatus == RequestErrorStatus.OK)
          return ConvertResult(passCountSummaryResult);

        throw CreateServiceException(ERROR_MESSAGE, passCountSummaryResult.ResultStatus);
      }

      throw CreateServiceException(ERROR_MESSAGE);
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
  }
}
