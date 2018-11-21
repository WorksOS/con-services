using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Tile.Service.Common.Extensions;
using VSS.Tile.Service.Common.Helpers;
using VSS.Tile.Service.Common.Interfaces;
using VSS.Tile.Service.Common.Models;

namespace VSS.Tile.Service.Common.Services
{
  //The class supports overlaying bitmaps from various sources. ALK map tile provider is used as a datasource
  public class MapTileGenerator : IMapTileGenerator
  {
    private readonly ILogger log;

    private readonly IMapTileService mapTileService;
    private readonly IProjectTileService projectTileService;
    private readonly IGeofenceTileService geofenceTileService;
    private readonly ILoadDumpTileService loadDumpTileService;
    private readonly IAlignmentTileService alignmentTileService;
    private readonly IDxfTileService dxfTileService;
    private readonly IBoundingBoxHelper boundingBoxHelper;
    private readonly IBoundingBoxService boundingBoxService;
    private readonly IRaptorProxy raptorProxy;
    private readonly ILoadDumpProxy loadDumpProxy;

    public MapTileGenerator(ILoggerFactory logger, IBoundingBoxService bboxService, IRaptorProxy raptorProxy,
      IMapTileService mapTileService, IProjectTileService projectTileService, IGeofenceTileService geofenceTileService,
      IAlignmentTileService alignmentTileService, IDxfTileService dxfTileService, IBoundingBoxHelper boundingBoxHelper,
      ILoadDumpTileService loadDumpTileService, ILoadDumpProxy loadDumpProxy)
    {
      log = logger.CreateLogger<MapTileGenerator>();
      this.mapTileService = mapTileService;
      this.projectTileService = projectTileService;
      this.geofenceTileService = geofenceTileService;
      this.alignmentTileService = alignmentTileService;
      this.dxfTileService = dxfTileService;
      this.boundingBoxHelper = boundingBoxHelper;
      boundingBoxService = bboxService;
      this.raptorProxy = raptorProxy;
      this.loadDumpTileService = loadDumpTileService;
      this.loadDumpProxy = loadDumpProxy;
    }

    /// <summary>
    /// Get the map parameters for the report tile
    /// </summary>
    public MapParameters GetMapParameters(string bbox, int width, int height, bool addMargin, bool adjustBoundingBox)
    {
      log.LogDebug($"GetMapParameters: bbox={bbox}, width={width}, height={height}, addMargin={addMargin}, adjustBoundingBox={adjustBoundingBox}");

      var bboxRadians = boundingBoxHelper.GetBoundingBox(bbox);
      MapBoundingBox mapBox = new MapBoundingBox
      {
        minLat = bboxRadians.BottomLeftLat,
        minLng = bboxRadians.BottomLeftLon,
        maxLat = bboxRadians.TopRightLat,
        maxLng = bboxRadians.TopRightLon
      };

      int zoomLevel = TileServiceUtils.CalculateZoomLevel(mapBox.maxLat - mapBox.minLat, mapBox.maxLng - mapBox.minLng);
      long numTiles = TileServiceUtils.NumberOfTiles(zoomLevel);

      MapParameters parameters = new MapParameters
      {
        bbox = mapBox,
        zoomLevel = zoomLevel,
        numTiles = numTiles,
        mapWidth = width,
        mapHeight = height,
        addMargin = addMargin
      };

      if (adjustBoundingBox)
      {
        boundingBoxService.AdjustBoundingBoxToFit(parameters);
      }

      parameters.pixelTopLeft = TileServiceUtils.LatLngToPixel(mapBox.maxLat, mapBox.minLng, parameters.numTiles);
      log.LogDebug("MapParameters: " + JsonConvert.SerializeObject(parameters));

      return parameters;
    }

    /// <summary>
    /// Gets a single tile with various types of data overlayed on it according to what is requested.
    /// </summary>
    /// <param name="request">The tile request</param>
    /// <returns>A TileResult</returns>
    public async Task<byte[]> GetMapData(TileGenerationRequest request)
    {
      log.LogInformation("Getting map tile");
      log.LogDebug("TileGenerationRequest: " + JsonConvert.SerializeObject(request));

      log.LogDebug("Awaiting tiles to be completed");

      var overlays = await Task.WhenAll(request.overlays.Select(overlay => RequestTile(request, overlay)));

      log.LogDebug($"Tiles completed: {overlays.Count()} overlays");

      var overlayTile =
        TileServiceUtils.OverlayTiles(request.mapParameters, overlays.ToDictionary(k => k.Item1, v => v.Item2));
      log.LogDebug("Tiles overlaid");
      overlayTile = ScaleTile(request, overlayTile);
      log.LogDebug("Tiles scaled");
      return overlayTile;
    }

    private async Task<(TileOverlayType,byte[])> RequestTile(TileGenerationRequest request, TileOverlayType overlay)
    {
      byte[] bitmap=null;
      log.LogDebug($"Processing tile of type {overlay}");
      switch (overlay)
      {
        case TileOverlayType.BaseMap:
          bitmap = mapTileService.GetMapBitmap(request.mapParameters, request.mapType.Value,
            request.language.Substring(0, 2));
          break;
        case TileOverlayType.ProductionData:
          var bbox =
            $"{request.mapParameters.bbox.minLatDegrees},{request.mapParameters.bbox.minLngDegrees},{request.mapParameters.bbox.maxLatDegrees},{request.mapParameters.bbox.maxLngDegrees}";
          bitmap = await raptorProxy.GetProductionDataTile(Guid.Parse(request.project.ProjectUid), request.filterUid,
            request.cutFillDesignUid, (ushort) request.mapParameters.mapWidth, (ushort) request.mapParameters.mapHeight,
            bbox, request.mode.Value, request.baseUid, request.topUid, request.volCalcType, request.customHeaders);
          break;
        case TileOverlayType.ProjectBoundary:
          bitmap = projectTileService.GetProjectBitmap(request.mapParameters, request.project);
          break;
        case TileOverlayType.GeofenceBoundary:
        case TileOverlayType.Geofences:
          bitmap = geofenceTileService.GetSitesBitmap(request.mapParameters, request.geofences);
          break;
        case TileOverlayType.FilterCustomBoundary:
          bitmap = geofenceTileService.GetFilterBoundaryBitmap(request.mapParameters, request.customFilterBoundary,
            FilterBoundaryType.Polygon);
          break;
        case TileOverlayType.FilterDesignBoundary:
          bitmap = geofenceTileService.GetFilterBoundaryBitmap(request.mapParameters, request.designFilterBoundary,
            FilterBoundaryType.Design);
          break;
        case TileOverlayType.FilterAlignmentBoundary:
          bitmap = geofenceTileService.GetFilterBoundaryBitmap(request.mapParameters, request.alignmentFilterBoundary,
            FilterBoundaryType.Alignment);
          break;
        case TileOverlayType.CutFillDesignBoundary:
          bitmap = geofenceTileService.GetFilterBoundaryBitmap(request.mapParameters, request.designBoundaryPoints,
            FilterBoundaryType.Design);
          break;
        case TileOverlayType.Alignments:
          bitmap = alignmentTileService.GetAlignmentsBitmap(request.mapParameters, request.project.LegacyProjectId,
            request.alignmentPointsList);
          break;
        case TileOverlayType.DxfLinework:
          bitmap = await dxfTileService.GetDxfBitmap(request.mapParameters, request.dxfFiles);
          break;
        case TileOverlayType.LoadDumpData:
          var loadDumpLocations =
            await loadDumpProxy.GetLoadDumpLocations(request.project.ProjectUid, request.customHeaders);
          bitmap = loadDumpTileService.GetLoadDumpBitmap(request.mapParameters, loadDumpLocations);
          break;
      }

      return (overlay,bitmap);
    }

    /// <summary>
    /// Reduce the size of the tile to the requested size. This assumes the relevant calculations have been done to maintain the aspect ratio.
    /// </summary>
    /// <param name="request">Request parameters</param>
    /// <param name="overlayTile">The tile to scale</param>
    /// <returns>The scaled tile</returns>
    private byte[] ScaleTile(TileGenerationRequest request, byte[] overlayTile)
    {
      using (var tileStream = new MemoryStream(overlayTile))
      using (Image<Rgba32> bitmap = Image.Load<Rgba32>(tileStream))
      {
        log.LogDebug($"ScaleTile: requested size=({request.width},{request.height}), image size=({bitmap.Width},{bitmap.Height})");
        bitmap.Mutate(ctx => ctx.Resize(request.width, request.height));
        return bitmap.BitmapToByteArray();
      }
    }
  }

  public interface IMapTileGenerator
  {
    Task<byte[]> GetMapData(TileGenerationRequest request);

    MapParameters GetMapParameters(string bbox, int width, int height, bool addMargin, bool adjustBoundingBox);
  }
}

