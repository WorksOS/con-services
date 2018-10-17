using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using TINSurfaceExportResult = VSS.TRex.Gateway.Common.ResultHandling.TINSurfaceExportResult;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class TINSurfaceExportExecutor : BaseExecutor
  {
    /// <summary>
    /// TagFileExecutor
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
    public TINSurfaceExportExecutor(IConfigurationStore configStore,
      ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public TINSurfaceExportExecutor()
    {
    }

    private Guid[] GetSurveyedSurfaceExclusionList(ISiteModel siteModel, bool includeSurveyedSurfaces)
    {
      return siteModel.SurveyedSurfaces == null || includeSurveyedSurfaces ? new Guid[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as CompactionExportRequest;

      if (request == null)
        ThrowRequestTypeCastException<CompactionExportRequest>();

      var siteModel = GetSiteModel(request?.ProjectUid);

      var filter = ConvertFilter(request?.Filter, siteModel);

      var tinRequest = new TINSurfaceRequest();
      var response = tinRequest.Execute(new TINSurfaceRequestArgument
      {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          Tolerance = request?.Tolerance ?? 0.0
      });

      return TINSurfaceExportResult.CreateTINResult(response.data);
    }

    /// <summary>
    /// Processes the surface request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
