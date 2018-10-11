using System;
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
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Extensions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Interfaces;


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

      MapBoundingBox bbox = boundingBoxService.GetBoundingBox(request.Project, request.Filter,
        request.Overlays, request.BaseFilter, request.TopFilter, request.DesignDescriptor);

      int zoomLevel = TileServiceUtils.CalculateZoomLevel(bbox.maxLat - bbox.minLat, bbox.maxLng - bbox.minLng);
      long numTiles = TileServiceUtils.NumberOfTiles(zoomLevel);

      MapParameters parameters = new MapParameters
      {
        bbox = bbox,
        zoomLevel = zoomLevel,
        numTiles = numTiles,
        mapWidth = request.Width,
        mapHeight = request.Height,
        addMargin = request.Overlays.Contains(TileOverlayType.ProjectBoundary)
      };

      boundingBoxService.AdjustBoundingBoxToFit(parameters);

      parameters.pixelTopLeft = TileServiceUtils.LatLngToPixel(bbox.maxLat, bbox.minLng, parameters.numTiles);
      log.LogDebug("MapParameters: " + JsonConvert.SerializeObject(parameters));

      Dictionary<TileOverlayType, byte[]> tileList = new Dictionary<TileOverlayType, byte[]>();
      object lockObject = new object();

      var overlayTasks = request.Overlays.Select(async overlay =>
      {
        byte[] bitmap = null;
        switch (overlay)
        {
          case TileOverlayType.BaseMap:
            bitmap = mapTileService.GetMapBitmap(parameters, request.MapType.Value, request.Language.Substring(0, 2));
            break;
          case TileOverlayType.ProductionData:
            log.LogInformation($"GetProductionDataTile: project {request.Project.ProjectUid}");

            Guid.TryParse(request.Project.ProjectUid, out Guid projectUid);

            BoundingBox2DLatLon prodDataBox = new BoundingBox2DLatLon(parameters.bbox.minLng, parameters.bbox.minLat, parameters.bbox.maxLng, parameters.bbox.maxLat);
            var tileResult = productionDataTileService.GetProductionDataTile(request.ProjectSettings, request.ProjectSettingsColors, request.Filter, request.Project.LegacyProjectId, projectUid,
              request.Mode.Value, (ushort)parameters.mapWidth, (ushort)parameters.mapHeight, prodDataBox, request.DesignDescriptor, request.BaseFilter,
              request.TopFilter, request.DesignDescriptor, request.VolCalcType, null);//custom headers not used
            bitmap = tileResult.TileData;
            break;
          case TileOverlayType.ProjectBoundary:
            bitmap = projectTileService.GetProjectBitmap(parameters, request.Project);
            break;
          case TileOverlayType.Geofences:
            bitmap = geofenceTileService.GetSitesBitmap(parameters, request.Geofences);
            break;
          case TileOverlayType.FilterCustomBoundary:
            var filterCustomBoundaries = boundingBoxService.GetFilterBoundaries(request.Project, request.Filter, request.BaseFilter, request.TopFilter, FilterBoundaryType.Polygon);
            bitmap = geofenceTileService.GetFilterBoundaryBitmap(parameters, filterCustomBoundaries, FilterBoundaryType.Polygon);
            break;
          case TileOverlayType.FilterDesignBoundary:
            var filterDesignBoundaries = boundingBoxService.GetFilterBoundaries(request.Project, request.Filter, request.BaseFilter, request.TopFilter, FilterBoundaryType.Design);
            bitmap = geofenceTileService.GetFilterBoundaryBitmap(parameters, filterDesignBoundaries, FilterBoundaryType.Design);
            break;
          case TileOverlayType.FilterAlignmentBoundary:
            var filterAlignmentBoundaries = boundingBoxService.GetFilterBoundaries(request.Project, request.Filter, request.BaseFilter, request.TopFilter, FilterBoundaryType.Alignment);
            bitmap = geofenceTileService.GetFilterBoundaryBitmap(parameters, filterAlignmentBoundaries,
              FilterBoundaryType.Alignment);
            break;
          case TileOverlayType.CutFillDesignBoundary:
            var designBoundaries = boundingBoxService.GetDesignBoundaryPolygons(request.Project.LegacyProjectId, request.DesignDescriptor);
            bitmap = geofenceTileService.GetFilterBoundaryBitmap(parameters, designBoundaries, FilterBoundaryType.Design);
            break;
          case TileOverlayType.Alignments:
            bitmap = alignmentTileService.GetAlignmentsBitmap(parameters, request.Project.LegacyProjectId,
              request.AlignmentDescriptors);
            break;
          case TileOverlayType.DxfLinework:
            bitmap = await dxfTileService.GetDxfBitmap(parameters, request.DxfFiles);
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
      return new TileResult(overlayTile);
    }

    /// <summary>
    /// Reduce the size of the tile to the requested size. This assumes the relevant calculations have been done to maintain the aspect ratio.
    /// </summary>
    /// <param name="request">Request parameters</param>
    /// <param name="overlayTile">The tile to scale</param>
    /// <returns>The scaled tile</returns>
    private byte[] ScaleTile(TileGenerationRequest request, byte[] overlayTile)
    {
      using (Bitmap dstImage = new Bitmap(request.Width, request.Height))
      using (Graphics g = Graphics.FromImage(dstImage))
      using (var tileStream = new MemoryStream(overlayTile))
      using (Image srcImage = Image.FromStream(tileStream))
      {
        log.LogDebug($"ScaleTile: requested size=({request.Width},{request.Height}), image size=({srcImage.Width},{srcImage.Height})");
        dstImage.SetResolution(srcImage.HorizontalResolution, srcImage.VerticalResolution);
        g.CompositingMode = CompositingMode.SourceCopy;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        using (var wrapMode = new ImageAttributes())
        {
          wrapMode.SetWrapMode(WrapMode.TileFlipXY);
          g.DrawImage(srcImage, new Rectangle(0, 0, request.Width, request.Height), 0, 0, srcImage.Width, srcImage.Height, GraphicsUnit.Pixel, wrapMode);
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

