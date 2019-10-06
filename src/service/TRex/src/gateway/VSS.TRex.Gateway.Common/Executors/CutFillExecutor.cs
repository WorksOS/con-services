using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using VSS.TRex.Analytics.CutFillStatistics;
using VSS.TRex.Analytics.CutFillStatistics.GridFabric;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
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

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as TRexCutFillDetailsRequest;

      if (request == null)
        ThrowRequestTypeCastException<TRexCutFillDetailsRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);
      var filter = ConvertFilter(request.Filter, siteModel);

      var operation = new CutFillStatisticsOperation();
      var cutFillResult = await operation.ExecuteAsync(new CutFillStatisticsArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        ReferenceDesign = new DesignOffset(request.DesignDescriptor.FileUid ?? Guid.Empty, request.DesignDescriptor.Offset),
        Offsets = request.CutFillTolerances,
        Overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides),
        LiftParams = ConvertLift(request.LiftSettings, request.Filter?.LayerType)
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
    /// Processes the tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
