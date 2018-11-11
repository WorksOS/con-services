using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using MasterDataModels = VSS.MasterData.Models.Models;
using VSS.Tile.Service.Common.Extensions;
using VSS.Tile.Service.Common.Helpers;
using VSS.Tile.Service.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.TCCFileAccess;
using Point = SixLabors.Primitives.Point;

namespace VSS.Tile.Service.Common.Services
{
  /// <summary>
  /// Provides DXF tile functionality for reports
  /// </summary>
  public class DxfTileService : IDxfTileService
  {
    private readonly IConfigurationStore config;
    private readonly IFileRepository tccFileRepo;
    private readonly ILogger log;

    private readonly string tccFilespaceId;

    public DxfTileService(IConfigurationStore configuration, IFileRepository tccRepository, ILoggerFactory logger)
    {
      config = configuration;
      tccFileRepo = tccRepository;
      log = logger.CreateLogger<DxfTileService>();
      tccFilespaceId = config.GetValueString("TCCFILESPACEID");
    }

    /// <summary>
    /// Gets a map tile with DXF linework on it. 
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="dxfFiles">The list of DXF files to overlay tiles for</param>
    /// <returns>A bitmap</returns>
    public async Task<byte[]> GetDxfBitmap(MapParameters parameters, IEnumerable<FileData> dxfFiles)
    {
      log.LogInformation("GetDxfBitmap");

      byte[] overlayData = null;

      if (dxfFiles != null && dxfFiles.Any())
      {
        List<byte[]> tileList = new List<byte[]>();
        foreach (var dxfFile in dxfFiles)
        {
          if (dxfFile.ImportedFileType == ImportedFileType.Linework)
          {
            tileList.Add(await JoinDxfTiles(parameters, dxfFile));
          }
        }

        log.LogDebug("Overlaying DXF bitmaps");
        overlayData = TileServiceUtils.OverlayTiles(parameters, tileList);
      }

      return overlayData;
    }


    /// <summary>
    /// Joins standard size DXF tiles together to form one large tile for the report
    /// </summary>
    private async Task<byte[]> JoinDxfTiles(MapParameters parameters, FileData dxfFile)
    {
      log.LogDebug($"JoinDxfTiles: {dxfFile.ImportedFileUid}, {dxfFile.Name}");

      //Find the tiles that the bounding box fits into.
      MasterDataModels.Point tileTopLeft = WebMercatorProjection.PixelToTile(parameters.pixelTopLeft);
      MasterDataModels.Point pixelBottomRight = TileServiceUtils.LatLngToPixel(
        parameters.bbox.minLat, parameters.bbox.maxLng, parameters.numTiles);
      MasterDataModels.Point tileBottomRight = WebMercatorProjection.PixelToTile(pixelBottomRight);

      int xnumTiles = (int) (tileBottomRight.x - tileTopLeft.x) + 1;
      int ynumTiles = (int) (tileBottomRight.y - tileTopLeft.y) + 1;
      int width = xnumTiles * WebMercatorProjection.TILE_SIZE;
      int height = ynumTiles * WebMercatorProjection.TILE_SIZE;

      using (Image<Rgba32> tileBitmap = new Image<Rgba32>(width, height))
      {
        //Find the offset of the bounding box top left point inside the top left tile
        var point = new MasterDataModels.Point
        {
          x = tileTopLeft.x * WebMercatorProjection.TILE_SIZE,
          y = tileTopLeft.y * WebMercatorProjection.TILE_SIZE
        };
        //Clip to the actual bounding box within the tiles. 
        int clipWidth = parameters.mapWidth;
        int clipHeight = parameters.mapHeight;
        int xClipTopLeft = (int) (parameters.pixelTopLeft.x - point.x);
        int yClipTopLeft = (int) (parameters.pixelTopLeft.y - point.y);
        //Unlike System.Drawing, which allows the clipRect to have negative x, y and which moves as well as clips when used with DrawImage
        //as the source rectangle, ImageSharp does not respect negative values. So we will have to do extra work in this situation.
        if (xClipTopLeft < 0)
        {
          clipWidth += xClipTopLeft;
          xClipTopLeft = 0;
        }
        if (yClipTopLeft < 0)
        {
          clipHeight += yClipTopLeft;
          yClipTopLeft = 0;
        }
        Rectangle clipRect = new Rectangle(xClipTopLeft, yClipTopLeft, clipWidth, clipHeight);

        //Join all the DXF tiles into one large tile
        var suffix = FileUtils.GeneratedFileSuffix(dxfFile.ImportedFileType);
        string generatedName = FileUtils.GeneratedFileName(dxfFile.Name, suffix, FileUtils.DXF_FILE_EXTENSION);
        string zoomPath =
          $"{FileUtils.ZoomPath(FileUtils.TilePath(dxfFile.Path, generatedName), parameters.zoomLevel)}";

        for (int yTile = (int) tileTopLeft.y; yTile <= (int) tileBottomRight.y; yTile++)
        {
          string targetFolder = $"{zoomPath}/{yTile}";
          //TCC only renders tiles where there is DXF data. So check if any tiles for this y.
          if (await tccFileRepo.FolderExists(tccFilespaceId, targetFolder))
          {
            for (int xTile = (int) tileTopLeft.x; xTile <= (int) tileBottomRight.x; xTile++)
            {
              string targetFile = $"{targetFolder}/{xTile}.png";
              if (await tccFileRepo.FileExists(tccFilespaceId, targetFile))
              {
                log.LogDebug($"JoinDxfTiles: getting tile {targetFile}");

                var file = await tccFileRepo.GetFile(tccFilespaceId, targetFile);
                Image<Rgba32> tile = Image.Load<Rgba32>(file);

                Point offset = new Point(
                  (xTile - (int) tileTopLeft.x) * WebMercatorProjection.TILE_SIZE,
                  (yTile - (int) tileTopLeft.y) * WebMercatorProjection.TILE_SIZE);
                tileBitmap.Mutate(ctx => ctx.DrawImage(tile, PixelBlenderMode.Normal, 1f, offset));
              }
            }
          }
        }
        //Now clip the large tile
        tileBitmap.Mutate(ctx => ctx.Crop(clipRect));
        if (clipWidth >= parameters.mapWidth && clipHeight >= parameters.mapHeight)
        {
          return tileBitmap.BitmapToByteArray();
        }

        //and resize it if required tile area was overlapping rather than within the large tile
        //(negative clip values above)
        using (Image<Rgba32> resizedBitmap = new Image<Rgba32>(parameters.mapWidth, parameters.mapHeight))
        {
          Point offset = new Point(parameters.mapWidth-clipWidth, parameters.mapHeight-clipHeight);
          resizedBitmap.Mutate(ctx => ctx.DrawImage(tileBitmap, PixelBlenderMode.Normal, 1f, offset));
          return resizedBitmap.BitmapToByteArray();
        }            
      }
    }
  }


  public interface IDxfTileService
  {
    Task<byte[]> GetDxfBitmap(MapParameters parameters, IEnumerable<FileData> dxfFiles);
  }
}
