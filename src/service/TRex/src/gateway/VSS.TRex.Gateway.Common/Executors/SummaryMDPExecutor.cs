using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Analytics.MDPStatistics;
using VSS.TRex.Analytics.MDPStatistics.GridFabric;
using VSS.TRex.Common.Records;
using VSS.TRex.Filters;
using VSS.TRex.Types;
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

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as MDPSummaryRequest;

      if (request == null)
        ThrowRequestTypeCastException<MDPSummaryRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      var operation = new MDPStatisticsOperation();
      var overrides = request.Overrides;
      var mdpSummaryResult = await operation.ExecuteAsync(
        new MDPStatisticsArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          MDPPercentageRange = new MDPRangePercentageRecord(overrides.MinMDPPercent, overrides.MaxMDPPercent),
          OverrideMachineMDP = overrides.OverrideTargetMDP,
          OverridingMachineMDP = overrides.MdpTarget
        }
      );

      if (mdpSummaryResult != null)
      {
        if (mdpSummaryResult.ResultStatus == RequestErrorStatus.OK)
          return ConvertResult(mdpSummaryResult);

        throw CreateServiceException<SummaryMDPExecutor>(mdpSummaryResult.ResultStatus);
      }

      throw CreateServiceException<SummaryMDPExecutor>();
    }

    private SummaryResult ConvertResult(MDPStatisticsResult summary)
    {
      return new SummaryResult(
        summary.WithinTargetPercent,
        summary.ConstantTargetMDP,
        summary.IsTargetMDPConstant,
        summary.AboveTargetPercent,
        (short)summary.ReturnCode,
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
