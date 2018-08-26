using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.MDPStatistics;
using VSS.TRex.Analytics.MDPStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Types;
using MDPSummaryResult = VSS.TRex.Analytics.MDPStatistics.MDPResult;
using SummaryResult = VSS.Productivity3D.Models.ResultHandling.MDPSummaryResult;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get MDP summary.
  /// </summary>
  public class SummaryMDPExecutor : BaseExecutor
  {
    public SummaryMDPExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryMDPExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      MDPSummaryRequest request = item as MDPSummaryRequest;

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.filter, siteModel);

      MDPOperation operation = new MDPOperation();

      MDPSummaryResult mdpSummaryResult = operation.Execute(
        new MDPStatisticsArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          MDPPercentageRange = new MDPRangePercentageRecord(request.minMDPPercent, request.maxMDPPercent),
          OverrideMachineMDP = request.overrideTargetMDP,
          OverridingMachineMDP = request.mdpTarget
        }
      );

      if (mdpSummaryResult != null)
        return ConvertResult(mdpSummaryResult);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested MDP summary data"));
    }

    private SummaryResult ConvertResult(MDPResult summary)
    {
      return SummaryResult.Create(
        summary.WithinTargetPercent,
        summary.ConstantTargetMDP,
        summary.IsTargetMDPConstant,
        summary.AboveTargetPercent,
        (short)summary.ReturnCode,
        summary.TotalAreaCoveredSqMeters,
        summary.BelowTargetPercent);
    }

  }
}
