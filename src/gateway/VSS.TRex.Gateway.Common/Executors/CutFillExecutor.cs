using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.CutFillStatistics;
using VSS.TRex.Analytics.CutFillStatistics.GridFabric;
using VSS.TRex.Filters;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get cut-fill details.
  /// </summary>
  public class CutFillExecutor : BaseExecutor
  {
    public CutFillExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler, null, null)
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

      var siteModel = GetSiteModel(request.ProjectUid);
      
      // TODO...
      //var filter = RaptorConverters.ConvertFilter(null, request.filter, request.ProjectId);
      //var designDescriptor = RaptorConverters.DesignDescriptor(request.designDescriptor);
      //var liftBuildSettings =
      //  RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmNone);

      var filter = ConvertFilter(request.filter, siteModel);

      CutFillOperation operation = new CutFillOperation();
      CutFillResult cutFillResult = operation.Execute(new CutFillStatisticsArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        DesignID = request.designDescriptor.uid ?? Guid.Empty,
        Offsets = request.CutFillTolerances
      });

      if (cutFillResult != null)
        return CompactionCutFillDetailedResult.CreateCutFillDetailedResult(cutFillResult.Percents);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested cut-fill details data"));
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
