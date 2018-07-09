using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Designs.Storage;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.Rendering.Servers.Client;
using VSS.TRex.Servers;
using VSS.TRex.Servers.Client;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class TileExecutor : RequestExecutorContainer
  {
    public TileExecutor(IConfigurationStore configStore,
      ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public TileExecutor()
    {
    }

    private Guid[] GetSurveyedSurfaceExclusionList(ISiteModel siteModel, bool includeSurveyedSurfaces)
    {
      return siteModel.SurveyedSurfaces == null || includeSurveyedSurfaces ? new Guid[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();
    }

    private void ConvertFilter(FilterResult filter, ISiteModel siteModel, out CombinedFilter combinedFilter)
    {
      var returnEarliestFilteredCellPass = filter.ReturnEarliest.HasValue && filter.ReturnEarliest.Value;

      CellPassAttributeFilter AttributeFilter = new CellPassAttributeFilter
      {
        ReturnEarliestFilteredCellPass = returnEarliestFilteredCellPass,
        HasElevationTypeFilter = true,
        ElevationType = returnEarliestFilteredCellPass ? ElevationType.First : ElevationType.Last,
        // TODO Map the excluded surveyed surfaces from the filter.SurveyedSurfaceExclusionList to the ones that are in the TRex database
        SurveyedSurfaceExclusionList = GetSurveyedSurfaceExclusionList(siteModel, filter.SurveyedSurfaceExclusionList.Count == 0)
      };

      var fence = new Fence();

      if (filter.PolygonGrid != null)
      {
        for (int i = 0; i < filter.PolygonGrid.Count; i++)
          fence.Points.Add(new FencePoint()
          {
            X = filter.PolygonGrid[i].x,
            Y = filter.PolygonGrid[i].y,
            Z = 0
          });
      }
      else
      {
        for (int i = 0; i < filter.PolygonLL.Count; i++)
          fence.Points.Add(new FencePoint()
          {
            X = filter.PolygonLL[i].Lon,
            Y = filter.PolygonLL[i].Lat,
            Z = 0
          });
      }

      CellSpatialFilter SpatialFilter = new CellSpatialFilter
      {
        CoordsAreGrid = filter.PolygonGrid != null,
        IsSpatial = true,
        Fence = fence
      };

      combinedFilter = new CombinedFilter(AttributeFilter, SpatialFilter);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;

      var request = item as TileRequest;

      ISiteModel siteModel = SiteModels.SiteModels.Instance().GetSiteModel(request.ProjectUid.Value);

      if (siteModel == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Site model {request.ProjectUid} is unavailable"));
      }

      try
      {
        // Filter 1...
        CombinedFilter filter1 = null;

        if (request.Filter1 != null)
          ConvertFilter(request.Filter1, siteModel, out filter1);

        // Filter 2...
        CombinedFilter filter2 = null;

        if (request.Filter2 != null)
          ConvertFilter(request.Filter2, siteModel, out filter2);

        TileRenderingServer tileRenderServer = TileRenderingServer.NewInstance(new[] { ApplicationServiceServer.DEFAULT_ROLE_CLIENT, ServerRoles.TILE_RENDERING_NODE });

        //TODO: TRex expects a Guid for the cut-fill design. Raptor has a DesignDescriptor with long (id) and file name etc.
        //Raymond: how are designs implemented in TRex?
        //We could create a derived class of DesignDescriptor containing the Guid and 3dpm can create a new TileRequest
        //with all the same data but a derived DesignDescriptor with the Guid set, assuming serialization/deserialization
        //gives us the derived class here

        BoundingWorldExtent3D extents = null;
        bool hasGridCoords = false;
        if (request.BoundBoxLatLon != null)
        {
          extents = new BoundingWorldExtent3D(request.BoundBoxLatLon.bottomLeftLon, request.BoundBoxLatLon.bottomLeftLat, request.BoundBoxLatLon.topRightLon, request.BoundBoxLatLon.topRightLat);
        }
        if (request.BoundBoxGrid != null)
        {
          hasGridCoords = true;
          extents = new BoundingWorldExtent3D(request.BoundBoxGrid.bottomLeftX, request.BoundBoxGrid.bottomleftY, request.BoundBoxGrid.topRightX, request.BoundBoxGrid.topRightY);
        }

        TileRenderResponse_Core2 response = tileRenderServer.RenderTile(
          new TileRenderRequestArgument
          (siteModel.ID,
            (DisplayMode)request.Mode,
            extents,
            hasGridCoords,
            request.Width, // PixelsX
            request.Height, // PixelsY
            filter1,
            filter2,
            Guid.Empty//TODO: request.DesignDescriptor
          )) as TileRenderResponse_Core2;
        
        return TileResult.CreateTileResult(response?.TileBitmap);
      }
      catch (Exception E)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Exception: {E.Message}"));
      }
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
