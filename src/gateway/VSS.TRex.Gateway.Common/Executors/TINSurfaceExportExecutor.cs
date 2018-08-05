using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.Servers.Client;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class TINSurfaceExportExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// TagFileExecutor
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="tinSurfaceExportServer"></param>
    public TINSurfaceExportExecutor(IConfigurationStore configStore,
      ILoggerFactory logger, IServiceExceptionHandler exceptionHandler, ITINSurfaceExportRequestServer tinSurfaceExportServer) : base(configStore, logger, exceptionHandler, null, null)
    {
      this.tINSurfaceExportRequestServer = tinSurfaceExportServer;
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

    private CombinedFilter ConvertFilter(FilterResult filter, ISiteModel siteModel)
    {
      if (filter == null)
        return new CombinedFilter();//TRex doesn't like null filter

      var combinedFilter = AutoMapperUtility.Automapper.Map<FilterResult, CombinedFilter>(filter);

      // TODO Map the excluded surveyed surfaces from the filter.SurveyedSurfaceExclusionList to the ones that are in the TRex database
      bool includeSurveyedSurfaces = filter.SurveyedSurfaceExclusionList == null || filter.SurveyedSurfaceExclusionList.Count == 0;
      var excludedIds = siteModel.SurveyedSurfaces == null || includeSurveyedSurfaces ? new Guid[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();
      combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList = excludedIds;
      return combinedFilter;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as TINSurfaceExportRequest;

      ISiteModel siteModel = SiteModels.SiteModels.Instance().GetSiteModel(request.ProjectUid.Value);

      if (siteModel == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Site model {request.ProjectUid} is unavailable"));
      }

      var response = tINSurfaceExportRequestServer.Execute(new TINSurfaceRequestArgument
        {
          Tolerance = request.Tolerance ?? 0.0,
          ProjectID = request.ProjectUid.Value,
          Filters = new FilterSet(ConvertFilter(request.Filter, siteModel))
        }
      );

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
