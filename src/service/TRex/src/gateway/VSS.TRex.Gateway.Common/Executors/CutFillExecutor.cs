using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.CutFillStatistics;
using VSS.TRex.Analytics.CutFillStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get cut-fill details.
  /// </summary>
  public class CutFillExecutor : BaseExecutor
  {
    public CutFillExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CutFillExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      CutFillDetailsRequest request = item as CutFillDetailsRequest;

      if (request == null)
        ThrowRequestTypeCastException<CutFillDetailsRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      // TODO: Configure design settings
      //var designDescriptor = RaptorConverters.DesignDescriptor(request.designDescriptor);

      // TODO: Configure lift build settings
      //var liftBuildSettings = RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmNone);

      var filter = ConvertFilter(request.Filter, siteModel);

      CutFillStatisticsOperation operation = new CutFillStatisticsOperation();
      CutFillStatisticsResult cutFillResult = operation.Execute(new CutFillStatisticsArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        DesignID = request.DesignDescriptor.Uid ?? Guid.Empty,
        Offsets = request.CutFillTolerances
      });

      if (cutFillResult != null)
      {
        if (cutFillResult.ResultStatus == RequestErrorStatus.OK)
          return new CompactionCutFillDetailedResult(cutFillResult.Percents);

        throw CreateServiceException<CutFillExecutor>(cutFillResult.ResultStatus);
      }

      throw CreateServiceException<CutFillExecutor>();
    }

    /// <summary>
    /// Processes the cut/fill details request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
