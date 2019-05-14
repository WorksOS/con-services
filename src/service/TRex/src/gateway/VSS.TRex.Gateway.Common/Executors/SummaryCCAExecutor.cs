using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.CCAStatistics;
using VSS.TRex.Analytics.CCAStatistics.GridFabric;
using VSS.TRex.Filters;
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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      CCASummaryRequest request = item as CCASummaryRequest;

      if (request == null)
        ThrowRequestTypeCastException<CCASummaryRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      CCAStatisticsOperation operation = new CCAStatisticsOperation();

      CCAStatisticsResult ccaSummaryResult = operation.Execute(
        new CCAStatisticsArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter)
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
  }
}
