using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.MapHandling;


namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  //The class supports overlaying bitmaps from various sources. ALK map tile provider is used as a datasource
  public class MapTileGenerator : IMapTileGenerator
  {
    private readonly ILogger log;
    private readonly ILoggerFactory logger;

    private readonly IMapTileService mapTileService;
    private readonly IProjectTileService projectTileService;
    private readonly IGeofenceTileService geofenceTileService;
    private readonly IAlignmentTileService alignmentTileService;
    private readonly IDxfTileService dxfTileService;
    private readonly IProductionDataTileService productionDataTileService;
    private readonly IBoundingBoxService boundingBoxService;

    public MapTileGenerator(ILoggerFactory logger, IBoundingBoxService bboxService,
      IMapTileService mapTileService, IProjectTileService projectTileService, IGeofenceTileService geofenceTileService,
      IAlignmentTileService alignmentTileService, IDxfTileService dxfTileService, IProductionDataTileService productionDataTileService)
    {
      log = logger.CreateLogger<MapTileGenerator>();
      this.logger = logger;
      this.mapTileService = mapTileService;
      this.projectTileService = projectTileService;
      this.geofenceTileService = geofenceTileService;
      this.alignmentTileService = alignmentTileService;
      this.dxfTileService = dxfTileService;
      this.productionDataTileService = productionDataTileService;
      boundingBoxService = bboxService;
    }

    public async Task<TileResult> GetMapData(TileGenerationRequest request)
    {
      MapBoundingBox bbox = boundingBoxService.GetBoundingBox(request.project, request.filter, request.mode, request.baseFilter, request.topFilter);

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
          request.topFilter, request.designDescriptor, null);//custom headers not used
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
  }

  public interface IMapTileGenerator
  {
    Task<TileResult> GetMapData(TileGenerationRequest request);
  }
}

