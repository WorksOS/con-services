using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Interfaces;
using VSS.Productivity3D.WebApiModels.MapHandling;


namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  //The class supports overlaying bitmaps from various sources. ALK map tile provider is used as a datasource
  public class MapTileGenerator : IMapTileGenerator
  {
    private readonly ILogger log;

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
      this.mapTileService = mapTileService;
      this.projectTileService = projectTileService;
      this.geofenceTileService = geofenceTileService;
      this.alignmentTileService = alignmentTileService;
      this.dxfTileService = dxfTileService;
      this.productionDataTileService = productionDataTileService;
      boundingBoxService = bboxService;
    }

    /// <summary>
    /// Gets a single tile with various types of data overlayed on it according to what is requested.
    /// </summary>
    /// <param name="request">The tile request</param>
    /// <returns>A TileResult</returns>
    public async Task<TileResult> GetMapData(TileGenerationRequest request)
    {
      log.LogInformation("Getting map tile for reports");
      log.LogDebug("TileGenerationRequest: " + JsonConvert.SerializeObject(request));

      MapBoundingBox bbox = boundingBoxService.GetBoundingBox(request.project, request.filter,
        request.overlays, request.baseFilter, request.topFilter, request.designDescriptor);

      int zoomLevel = TileServiceUtils.CalculateZoomLevel(bbox.maxLat - bbox.minLat, bbox.maxLng - bbox.minLng);
      long numTiles = TileServiceUtils.NumberOfTiles(zoomLevel);

      MapParameters parameters = new MapParameters
      {
        bbox = bbox,
        zoomLevel = zoomLevel,
        numTiles = numTiles,
        mapWidth = request.width,
        mapHeight = request.height,
        addMargin = request.overlays.Contains(TileOverlayType.ProjectBoundary)
      };

      boundingBoxService.AdjustBoundingBoxToFit(parameters);

      parameters.pixelTopLeft = TileServiceUtils.LatLngToPixel(bbox.maxLat, bbox.minLng, parameters.numTiles);
      log.LogDebug("MapParameters: " + JsonConvert.SerializeObject(parameters));

      Dictionary<TileOverlayType, byte[]> tileList = new Dictionary<TileOverlayType, byte[]>();
      object lockObject = new object();

      var overlayTasks = request.overlays.Select(async overlay =>
      {
        byte[] bitmap = null;
        switch (overlay)
        {
          case TileOverlayType.BaseMap:
            bitmap = mapTileService.GetMapBitmap(parameters, request.mapType.Value, request.language.Substring(0, 2));
            break;
          case TileOverlayType.ProductionData:
            log.LogInformation($"GetProductionDataTile: project {request.project.ProjectUid}");
            BoundingBox2DLatLon prodDataBox = BoundingBox2DLatLon.CreateBoundingBox2DLatLon(parameters.bbox.minLng, parameters.bbox.minLat, parameters.bbox.maxLng, parameters.bbox.maxLat);
            var tileResult = productionDataTileService.GetProductionDataTile(request.projectSettings, request.ProjectSettingsColors, request.filter, request.project.LegacyProjectId,
              request.mode.Value, (ushort)parameters.mapWidth, (ushort)parameters.mapHeight, prodDataBox, request.designDescriptor, request.baseFilter,
              request.topFilter, request.designDescriptor, request.volCalcType, null);//custom headers not used
            bitmap = tileResult.TileData;
            break;
          case TileOverlayType.ProjectBoundary:
            bitmap = projectTileService.GetProjectBitmap(parameters, request.project);
            break;
          case TileOverlayType.Geofences:
            bitmap = geofenceTileService.GetSitesBitmap(parameters, request.geofences);
            break;
          case TileOverlayType.FilterCustomBoundary:
            var filterCustomBoundaries = boundingBoxService.GetFilterBoundaries(request.project, request.filter, request.baseFilter, request.topFilter, FilterBoundaryType.Polygon);
            bitmap = geofenceTileService.GetFilterBoundaryBitmap(parameters, filterCustomBoundaries, FilterBoundaryType.Polygon);
            break;
          case TileOverlayType.FilterDesignBoundary:
            var filterDesignBoundaries = boundingBoxService.GetFilterBoundaries(request.project, request.filter, request.baseFilter, request.topFilter, FilterBoundaryType.Design);
            bitmap = geofenceTileService.GetFilterBoundaryBitmap(parameters, filterDesignBoundaries, FilterBoundaryType.Design);
            break;
          case TileOverlayType.FilterAlignmentBoundary:
            var filterAlignmentBoundaries = boundingBoxService.GetFilterBoundaries(request.project, request.filter, request.baseFilter, request.topFilter, FilterBoundaryType.Alignment);
            bitmap = geofenceTileService.GetFilterBoundaryBitmap(parameters, filterAlignmentBoundaries,
              FilterBoundaryType.Alignment);
            break;
          case TileOverlayType.CutFillDesignBoundary:
            var designBoundaries = boundingBoxService.GetDesignBoundaryPolygons(request.project.LegacyProjectId, request.designDescriptor);
            bitmap = geofenceTileService.GetFilterBoundaryBitmap(parameters, designBoundaries, FilterBoundaryType.Design);
            break;
          case TileOverlayType.Alignments:
            bitmap = alignmentTileService.GetAlignmentsBitmap(parameters, request.project.LegacyProjectId,
              request.alignmentDescriptors);
            break;
          case TileOverlayType.DxfLinework:
            bitmap = await dxfTileService.GetDxfBitmap(parameters, request.dxfFiles);
            break;
        }
        if (bitmap != null)
        {
          lock (lockObject)
          {
            tileList.Add(overlay, bitmap);
          }
        }
      });

      log.LogDebug("Awating tiles to be completed");
      await Task.WhenAll(overlayTasks);
      log.LogDebug("Tiles completed");

      var overlayTile = TileServiceUtils.OverlayTiles(parameters, tileList);
      log.LogDebug("Tiles overlaid");
      overlayTile = ScaleTile(request, overlayTile);
      log.LogDebug("Tiles scaled");
      return TileResult.CreateTileResult(overlayTile, TASNodeErrorStatus.asneOK);
    }

    /// <summary>
    /// Reduce the size of the tile to the requested size. This assumes the relevant calculations have been done to maintain the aspect ratio.
    /// </summary>
    /// <param name="request">Request parameters</param>
    /// <param name="overlayTile">The tile to scale</param>
    /// <returns>The scaled tile</returns>
    private byte[] ScaleTile(TileGenerationRequest request, byte[] overlayTile)
    {
      using (Bitmap dstImage = new Bitmap(request.width, request.height))
      using (Graphics g = Graphics.FromImage(dstImage))
      using (var tileStream = new MemoryStream(overlayTile))
      using (Image srcImage = Image.FromStream(tileStream))
      {
        log.LogDebug($"ScaleTile: requested size=({request.width},{request.height}), image size=({srcImage.Width},{srcImage.Height})");
        dstImage.SetResolution(srcImage.HorizontalResolution, srcImage.VerticalResolution);
        g.CompositingMode = CompositingMode.SourceCopy;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        using (var wrapMode = new ImageAttributes())
        {
          wrapMode.SetWrapMode(WrapMode.TileFlipXY);
          g.DrawImage(srcImage, new Rectangle(0, 0, request.width, request.height), 0, 0, srcImage.Width, srcImage.Height, GraphicsUnit.Pixel, wrapMode);
        }
        return dstImage.BitmapToByteArray();
      }
    }
  }



  public interface IMapTileGenerator
  {
    Task<TileResult> GetMapData(TileGenerationRequest request);
  }
}

