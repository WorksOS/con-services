using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Analytics.CMVStatistics.GridFabric.Summary;
using VSS.TRex.Analytics.CMVStatistics.Summary;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Types;
using CMVSummaryResult = VSS.TRex.Analytics.CMVStatistics.Summary.CMVSummaryResult;
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
      : base(configStore, logger, exceptionHandler, null, null)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryCMVExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      CMVSummaryRequest request = item as CMVSummaryRequest;

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.filter, siteModel);

      CMVSummaryOperation operation = new CMVSummaryOperation();

      CMVSummaryResult cmvSummaryResult = operation.Execute(
        new CMVSummaryArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet { Filters = new[] { filter } },
          CMVPercentageRange = new CMVRangePercentageRecord(request.minCMVPercent, request.maxCMVPercent),
          OverrideMachineCMV = request.overrideTargetCMV,
          OverridingMachineCMV = request.cmvTarget
        }
      );

      if (cmvSummaryResult != null)
        return ConvertResult(cmvSummaryResult);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested CMV summary data"));
    }

    private SummaryResult ConvertResult(CMVSummaryResult summary)
    {
      return SummaryResult.Create(
        summary.WithinTargetPercent,
        summary.ConstantTargetCMV,
        summary.IsTargetCMVConstant,
        summary.AboveTargetPercent,
        (short) summary.ReturnCode,
        summary.TotalAreaCoveredSqMeters,
        summary.BelowTargetPercent);
    }
  }
}
