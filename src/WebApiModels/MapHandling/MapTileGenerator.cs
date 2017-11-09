using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Coord.Executors;
using VSS.Productivity3D.WebApiModels.Coord.Models;
using VSS.Productivity3D.WebApiModels.Coord.ResultHandling;
using VSS.Productivity3D.WebApiModels.MapHandling;
using VSS.Productivity3D.WebApiModels.Report.Executors;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  //The class supports overlaying bitmaps from various sources. ALK map tile provider is used as a datasource
  public class MapTileGenerator : IMapTileGenerator
  {
    private readonly ILogger log;
    private readonly ILoggerFactory logger;
    private readonly IASNodeClient raptorClient;

    private readonly IMapTileService mapTileService;
    private readonly IProjectTileService projectTileService;
    private readonly IGeofenceTileService geofenceTileService;
    private readonly IAlignmentTileService alignmentTileService;
    private readonly IDxfTileService dxfTileService;
    private readonly IProductionDataTileService productionDataTileService;

    public MapTileGenerator(ILoggerFactory logger, IASNodeClient raptor,
      IMapTileService mapTileService, IProjectTileService projectTileService, IGeofenceTileService geofenceTileService,
      IAlignmentTileService alignmentTileService, IDxfTileService dxfTileService, IProductionDataTileService productionDataTileService)
    {
      log = logger.CreateLogger<MapTileGenerator>();
      this.logger = logger;
      raptorClient = raptor;
      this.mapTileService = mapTileService;
      this.projectTileService = projectTileService;
      this.geofenceTileService = geofenceTileService;
      this.alignmentTileService = alignmentTileService;
      this.dxfTileService = dxfTileService;
      this.productionDataTileService = productionDataTileService;
    }

    public async Task<TileResult> GetMapData(TileGenerationRequest request)
    {
      MapBoundingBox bbox = await GetBoundingBox(request.project, request.filter, request.mode, request.baseFilter, request.topFilter, request.volumeDesign);

      int zoomLevel = TileServiceUtils.CalculateZoomLevel(bbox.maxLat - bbox.minLat, bbox.maxLng - bbox.minLng);
      int numTiles = TileServiceUtils.NumberOfTiles(zoomLevel);
      var pixelTopLeft = WebMercatorProjection.LatLngToPixel(new Point(bbox.maxLat, bbox.minLng), numTiles);

      MapParameters parameters = new MapParameters
      {
        bbox = bbox,
        zoomLevel = zoomLevel,
        numTiles = numTiles,
        mapWidth = request.width,
        mapHeight = request.height,
        pixelTopLeft = pixelTopLeft,
      };

      List<byte[]> tileList = new List<byte[]>();
      if (request.overlays.Contains(TileOverlayType.BaseMap))
        tileList.Add(mapTileService.GetMapBitmap(parameters, request.mapType.Value, request.language.Substring(0, 2)));
      if (request.overlays.Contains(TileOverlayType.ProductionData))
      {
        BoundingBox2DLatLon prodDataBox = BoundingBox2DLatLon.CreateBoundingBox2DLatLon(bbox.minLng, bbox.minLat, bbox.maxLng, bbox.maxLat);
        var tileResult = productionDataTileService.GetProductionDataTile(request.projectSettings, request.filter, request.project.projectId,
          request.mode.Value, (ushort)request.width, (ushort)request.height, prodDataBox, request.designDescriptor, request.baseFilter, 
          request.topFilter, request.volumeDesign, null);//custom headers not used
        tileList.Add(tileResult.TileData);
      }
      if (request.overlays.Contains(TileOverlayType.ProjectBoundary))
        tileList.Add(projectTileService.GetProjectBitmap(parameters, request.project));
      if (request.overlays.Contains(TileOverlayType.Geofences))
        tileList.Add(geofenceTileService.GetSitesBitmap(parameters, request.geofences));
      if (request.overlays.Contains(TileOverlayType.Alignments))
        tileList.Add(alignmentTileService.GetAlignmentsBitmap(parameters, request.project.projectId, request.alignmentDescriptors));
      if (request.overlays.Contains(TileOverlayType.DxfLinework))
        tileList.Add(await dxfTileService.GetDxfBitmap(parameters, request.dxfFiles));

      return TileResult.CreateTileResult(TileServiceUtils.OverlayTiles(parameters, tileList), TASNodeErrorStatus.asneOK);
    }

    private async Task<MapBoundingBox> GetBoundingBox(ProjectDescriptor project, Productivity3D.Common.Models.Filter filter, DisplayMode? mode,
      Productivity3D.Common.Models.Filter baseFilter, Productivity3D.Common.Models.Filter topFilter, DesignDescriptor volumeDesign)
    {
      MapBoundingBox bbox = null;

      //If the filter has an area then use it as the bounding box
      List<Productivity3D.Common.Models.WGSPoint> filterPoints = new List<Productivity3D.Common.Models.WGSPoint>();
      //Summary volumes potentially has 2 filters
      if (mode == DisplayMode.CutFill && (baseFilter != null || topFilter != null))
      {
        if (baseFilter != null && baseFilter.PolygonLL != null && baseFilter.PolygonLL.Count > 0)
        {
          filterPoints.AddRange(baseFilter.PolygonLL);
        }
        if (topFilter != null && topFilter.PolygonLL != null && topFilter.PolygonLL.Count > 0)
        {
          filterPoints.AddRange(topFilter.PolygonLL);
        }
      }
      else
      {
        if (filter != null && filter.PolygonLL != null && filter.PolygonLL.Count > 0)
        {
          filterPoints.AddRange(filter.PolygonLL);
        }
      }
     
      if (filterPoints.Count > 0)
      {
        bbox = new MapBoundingBox
        {
          minLat = filterPoints.Min(p => p.Lat),
          minLng = filterPoints.Min(p => p.Lon),
          maxLat = filterPoints.Max(p => p.Lat),
          maxLng = filterPoints.Min(p => p.Lon)
        };
      }
      else 
      {
        //No area filter so use production data extents as the bounding box
        //Only applies if doing production data tiles
        if (mode.HasValue)
        {
          var productionDataExtents = await GetProductionDataExtents(project.projectId, filter);
          if (productionDataExtents != null)
          {
            bbox = new MapBoundingBox
            {
              minLat = productionDataExtents.conversionCoordinates[0].y.latRadiansToDegrees(),
              minLng = productionDataExtents.conversionCoordinates[0].x.lonRadiansToDegrees(),
              maxLat = productionDataExtents.conversionCoordinates[1].y.latRadiansToDegrees(),
              maxLng = productionDataExtents.conversionCoordinates[1].x.lonRadiansToDegrees()
            };
          }
        }

        //Sometimes tag files way outside the project boundary are imported. These mean the data extents are
        //invalid and and give problems. So need to check for this and use project extents in this case.

        //Also use project boundary extents if fail to get production data extents or not doing production data tiles
        //e.g. project thumbnails
        var projectPoints = TileServiceUtils.GeometryToPoints(project.projectGeofenceWKT);
        var projectMinLat = projectPoints.Min(p => p.Latitude);
        var projectMinLng = projectPoints.Min(p => p.Longitude);
        var projectMaxLat = projectPoints.Max(p => p.Latitude);
        var projectMaxLng = projectPoints.Max(p => p.Longitude);
        bool assign = bbox == null
          ? true
          : bbox.minLat < projectMinLat || bbox.minLat > projectMaxLat ||
            bbox.maxLat < projectMinLat || bbox.maxLat > projectMaxLat ||
            bbox.minLng < projectMinLng || bbox.minLng > projectMaxLng ||
            bbox.maxLng < projectMinLng || bbox.minLng > projectMaxLng;

        if (assign)
        {
          bbox = new MapBoundingBox
          {
            minLat = projectMinLat,
            minLng = projectMinLng,
            maxLat = projectMaxLat,
            maxLng = projectMaxLng
          };
        }
      }
  
      return bbox;
    }

    /// <summary>
    /// Get the production data extents for the project.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    private async Task<CoordinateConversionResult> GetProductionDataExtents(long projectId, Productivity3D.Common.Models.Filter filter)
    {
      ProjectStatisticsRequest statsRequest = ProjectStatisticsRequest.CreateStatisticsParameters(projectId, filter?.SurveyedSurfaceExclusionList?.ToArray());
      var statsResult =
        RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(logger, raptorClient)
          .Process(statsRequest) as ProjectStatisticsResult;

      if (statsResult.Code == ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        var coordList = new List<TwoDConversionCoordinate>
        {
          TwoDConversionCoordinate.CreateTwoDConversionCoordinate(statsResult.extents.minX, statsResult.extents.minY),
          TwoDConversionCoordinate.CreateTwoDConversionCoordinate(statsResult.extents.maxX, statsResult.extents.maxY)
        };

        var coordRequest = CoordinateConversionRequest.CreateCoordinateConversionRequest(projectId,
          TwoDCoordinateConversionType.NorthEastToLatLon, coordList.ToArray());
        var coordResult = RequestExecutorContainerFactory.Build<CoordinateConversionExecutor>(logger, raptorClient)
          .Process(coordRequest) as CoordinateConversionResult;
        return coordResult;
      }
      return null;
    }

  }


  public interface IMapTileGenerator
  {
    Task<TileResult> GetMapData(TileGenerationRequest request);
  }
}

