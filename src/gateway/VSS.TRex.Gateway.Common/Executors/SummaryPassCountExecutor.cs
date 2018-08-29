using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric.Summary;
using VSS.TRex.Analytics.PassCountStatistics.Summary;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Types;
using PassCountSummaryResult = VSS.TRex.Analytics.PassCountStatistics.Summary.PassCountSummaryResult;
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
      PassCountSummaryRequest request = item as PassCountSummaryRequest;

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.filter, siteModel);

      var targetPassCountRange = new PassCountRangeRecord();
      if (request.overridingTargetPassCountRange != null)
        targetPassCountRange.SetMinMax(request.overridingTargetPassCountRange.min, request.overridingTargetPassCountRange.max);

      PassCountSummaryOperation operation = new PassCountSummaryOperation();
      PassCountSummaryResult passCountSummaryResult = operation.Execute(
        new PassCountSummaryArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          OverridingTargetPassCountRange = targetPassCountRange,
          OverrideTargetPassCount = request.overridingTargetPassCountRange != null
        }
      );

      if (passCountSummaryResult != null)
        return ConvertResult(passCountSummaryResult);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Pass Count summary data"));
    }

    private SummaryResult ConvertResult(PassCountSummaryResult summary)
    {
      return SummaryResult.Create(
        TargetPassCountRange.CreateTargetPassCountRange(summary.ConstantTargetPassCountRange.Min, summary.ConstantTargetPassCountRange.Max),
        summary.IsTargetPassCountConstant,
        summary.WithinTargetPercent,
        summary.AboveTargetPercent,
        summary.BelowTargetPercent,
        (short)summary.ReturnCode,
        summary.TotalAreaCoveredSqMeters);
    }
  }
}
