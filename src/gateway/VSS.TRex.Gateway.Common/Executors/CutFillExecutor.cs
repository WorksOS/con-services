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
      const string ERROR_MESSAGE = "Failed to get requested cut-fill details data";

      CutFillDetailsRequest request = item as CutFillDetailsRequest;

      if (request == null)
        ThrowRequestTypeCastException(typeof(CutFillDetailsRequest));

      var siteModel = GetSiteModel(request.ProjectUid);
      
      // TODO: Configure design and lift build settings
      //var designDescriptor = RaptorConverters.DesignDescriptor(request.designDescriptor);
      //var liftBuildSettings =
      //  RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmNone);

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

        throw CreateServiceException(ERROR_MESSAGE, cutFillResult.ResultStatus);
      }

      throw CreateServiceException(ERROR_MESSAGE);
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
