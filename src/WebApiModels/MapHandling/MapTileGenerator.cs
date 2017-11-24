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
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
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

    /// <summary>
    /// Gets a single tile with various types of data overlayed on it according to what is requested.
    /// </summary>
    /// <param name="request">The tile request</param>
    /// <returns>A TileResult</returns>
    public async Task<TileResult> GetMapData(TileGenerationRequest request)
    {
      log.LogInformation("Getting map tile for reports");
      log.LogDebug("TileGenerationRequest: " + JsonConvert.SerializeObject(request));

      MapParameters parameters = boundingBoxService.GetMapParameters(request);

      List<byte[]> tileList = new List<byte[]>();
      if (request.overlays.Contains(TileOverlayType.BaseMap))
        tileList.Add(mapTileService.GetMapBitmap(parameters, request.mapType.Value, request.language.Substring(0, 2)));
      if (request.overlays.Contains(TileOverlayType.ProductionData))
      {
        log.LogInformation("GetProductionDataTile");
        BoundingBox2DLatLon prodDataBox = BoundingBox2DLatLon.CreateBoundingBox2DLatLon(parameters.bbox.minLng, parameters.bbox.minLat, parameters.bbox.maxLng, parameters.bbox.maxLat);
        var tileResult = productionDataTileService.GetProductionDataTile(request.projectSettings, request.filter, request.project.projectId,
          request.mode.Value, (ushort)parameters.mapWidth, (ushort)parameters.mapHeight, prodDataBox, request.designDescriptor, request.baseFilter, 
          request.topFilter, request.designDescriptor, null);//custom headers not used
        tileList.Add(tileResult.TileData);
      }
      if (request.overlays.Contains(TileOverlayType.ProjectBoundary))
      {
        var projectBitmap = projectTileService.GetProjectBitmap(parameters, request.project);
        if (projectBitmap != null)
          tileList.Add(projectBitmap);
      }
      if (request.overlays.Contains(TileOverlayType.Geofences))
      {
        var geofencesBitmap = geofenceTileService.GetSitesBitmap(parameters, request.geofences);
        if (geofencesBitmap != null)
          tileList.Add(geofencesBitmap);
      }
      if (request.overlays.Contains(TileOverlayType.Alignments))
      {
        var alignmentsBitmap = alignmentTileService.GetAlignmentsBitmap(parameters, request.project.projectId,
          request.alignmentDescriptors);
        if (alignmentsBitmap != null)
          tileList.Add(alignmentsBitmap);
      }
      if (request.overlays.Contains(TileOverlayType.DxfLinework))
      {
        var dxfBitmap = await dxfTileService.GetDxfBitmap(parameters, request.dxfFiles);
        if (dxfBitmap != null)
          tileList.Add(dxfBitmap);
      }
      var overlayTile = TileServiceUtils.OverlayTiles(parameters, tileList);
      if (parameters.scaleDown)
      {
        overlayTile = ScaleTileDown(request, overlayTile);
      }
      return TileResult.CreateTileResult(overlayTile, TASNodeErrorStatus.asneOK);
    }

    /// <summary>
    /// Reduce the size of the tile to the requested size. This assumes the relevant calculations have been done to maintain the aspect ratio.
    /// </summary>
    /// <param name="request">Request parameters</param>
    /// <param name="overlayTile">The tile to scale</param>
    /// <returns>The scaled tile</returns>
    private byte[] ScaleTileDown(TileGenerationRequest request, byte[] overlayTile)
    { 
      using (Bitmap dstImage = new Bitmap(request.width, request.height))
      using (Graphics g = Graphics.FromImage(dstImage))
      using (var tileStream = new MemoryStream(overlayTile))
      using (Image srcImage = Image.FromStream(tileStream))
      {
        /*
        //Need to maintain aspect ratio. Figure out the ratio.
        double ratioX = (double)request.width / (double)srcImage.Width;
        double ratioY = (double)request.height / (double)srcImage.Height;
        // use whichever multiplier is smaller
        double ratio = ratioX < ratioY ? ratioX : ratioY;

        // now we can get the new height and width
        int newHeight = Convert.ToInt32(srcImage.Height * ratio);
        int newWidth = Convert.ToInt32(srcImage.Width * ratio);

        // Now calculate the X,Y position of the upper-left corner 
        // (one of these will always be zero)
        int posX = Convert.ToInt32((request.width - (srcImage.Width * ratio)) / 2);
        int posY = Convert.ToInt32((request.height - (srcImage.Height * ratio)) / 2);

        //g.Clear(Color.Red); // white padding
        //g.DrawImage(image, posX, posY, newWidth, newHeight);
        */
        

        dstImage.SetResolution(srcImage.HorizontalResolution, srcImage.VerticalResolution);
        g.CompositingMode = CompositingMode.SourceCopy;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        using (var wrapMode = new ImageAttributes())
        {
          wrapMode.SetWrapMode(WrapMode.TileFlipXY);
          //g.Clear(Color.Red); // white padding
          //g.DrawImage(srcImage, posX, posY, newWidth, newHeight);
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

