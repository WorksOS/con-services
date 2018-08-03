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
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
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

      var combinedFilter = Mapper.Map<FilterResult, CombinedFilter>(filter);
      // TODO Map the excluded surveyed surfaces from the filter.SurveyedSurfaceExclusionList to the ones that are in the TRex database
      bool includeSurveyedSurfaces = filter.SurveyedSurfaceExclusionList.Count == 0;
      var excludedIds = siteModel.SurveyedSurfaces == null || includeSurveyedSurfaces ? new Guid[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();
      combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList = excludedIds;
      return combinedFilter;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as TileRequest;

      BoundingWorldExtent3D extents = null;
      bool hasGridCoords = false;
      if (request.BoundBoxLatLon != null)
      {
        extents = AutoMapperUtility.Automapper.Map<BoundingBox2DLatLon, BoundingWorldExtent3D>(request.BoundBoxLatLon);
      }
      else if (request.BoundBoxGrid != null)
      {
        hasGridCoords = true;
        extents = AutoMapperUtility.Automapper.Map<BoundingBox2DGrid, BoundingWorldExtent3D>(request.BoundBoxGrid);
      }

      ISiteModel siteModel = SiteModels.SiteModels.Instance().GetSiteModel(request.ProjectUid.Value);

      if (siteModel == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Site model {request.ProjectUid} is unavailable"));
      }
      CombinedFilter filter1 = ConvertFilter(request.Filter1, siteModel);
      CombinedFilter filter2 = ConvertFilter(request.Filter2, siteModel);

      var response = tINSurfaceExportRequestServer.Execute(new TINSurfaceRequestArgument()
        {}
      );

      /*      var response = tileRenderServer.RenderTile(
              new TileRenderRequestArgument
              (siteModel.ID,
                (Types.DisplayMode)request.Mode,
                extents,
                hasGridCoords,
                request.Width, // PixelsX
                request.Height, // PixelsY
                filter1,
                filter2,
                Guid.Empty //TODO: request.DesignDescriptor
              )) as TileRenderResponse_Core2;
      */

      return TINSurfaceExportResult.CreateTINResult(response.data)
    }

    /// <summary>
    /// Processes the tile request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
