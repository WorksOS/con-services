using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System.Drawing;
using VSS.Productivity3D.Common.Extensions;
using Point = VSS.MasterData.Models.Models.Point;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// Provides DXF tile functionality for reports
  /// </summary>
  public class DxfTileService : IDxfTileService
  {
    private readonly IConfigurationStore config;
    private readonly IFileRepository tccFileRepository;
    private readonly ILogger log;

    private readonly string tccFilespaceId;

    public DxfTileService(IConfigurationStore configuration, IFileRepository tccRepository, ILoggerFactory logger)
    {
      config = configuration;
      tccFileRepository = tccRepository;
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
      Point tileTopLeft = WebMercatorProjection.PixelToTile(parameters.pixelTopLeft);
      Point pixelBottomRight = TileServiceUtils.LatLngToPixel(
        parameters.bbox.minLat, parameters.bbox.maxLng, parameters.numTiles);
      Point tileBottomRight = WebMercatorProjection.PixelToTile(pixelBottomRight);

      int xnumTiles = (int)(tileBottomRight.x - tileTopLeft.x) + 1;
      int ynumTiles = (int)(tileBottomRight.y - tileTopLeft.y) + 1;
      int width = xnumTiles * WebMercatorProjection.TILE_SIZE;
      int height = ynumTiles * WebMercatorProjection.TILE_SIZE;

      using (Bitmap tileBitmap = new Bitmap(width, height))
      using (Graphics g = Graphics.FromImage(tileBitmap))
      {
        //Find the offset of the bounding box top left point inside the top left tile
        var point = new Point
        {
          x = tileTopLeft.x * WebMercatorProjection.TILE_SIZE,
          y = tileTopLeft.y * WebMercatorProjection.TILE_SIZE
        };
        //Clip to the actual bounding box within the tiles
        int xClipTopLeft = (int)(parameters.pixelTopLeft.x - point.x);
        int yClipTopLeft = (int)(parameters.pixelTopLeft.y - point.y);
        Rectangle clipRect = new Rectangle(xClipTopLeft, yClipTopLeft, parameters.mapWidth, parameters.mapHeight);
        g.SetClip(clipRect);

        var suffix = FileUtils.GeneratedFileSuffix(dxfFile.ImportedFileType);
        string generatedName = FileUtils.GeneratedFileName(dxfFile.Name, suffix, FileUtils.DXF_FILE_EXTENSION);
        string zoomPath =
          $"{FileUtils.ZoomPath(FileUtils.TilePath(dxfFile.Path, generatedName), parameters.zoomLevel)}";

        for (int yTile = (int)tileTopLeft.y; yTile <= (int)tileBottomRight.y; yTile++)
        {
          string targetFolder = $"{zoomPath}/{yTile}";
          //TCC only renders tiles where there is DXF data. So check if any tiles for this y.
          if (await tccFileRepository.FolderExists(tccFilespaceId, targetFolder))
          {
            for (int xTile = (int)tileTopLeft.x; xTile <= (int)tileBottomRight.x; xTile++)
            {
              string targetFile = $"{targetFolder}/{xTile}.png";
              if (await tccFileRepository.FileExists(tccFilespaceId, targetFile))
              {
                log.LogDebug($"JoinDxfTiles: getting tile {targetFile}");

                var file = await tccFileRepository.GetFile(tccFilespaceId, targetFile);
                Image tile = Image.FromStream(file);

                System.Drawing.Point offset = new System.Drawing.Point(
                  (xTile - (int)tileTopLeft.x) * WebMercatorProjection.TILE_SIZE,
                  (yTile - (int)tileTopLeft.y) * WebMercatorProjection.TILE_SIZE);
                g.DrawImage(tile, offset);
              }
            }
          }
        }

        using (Bitmap clipBitmap = new Bitmap(parameters.mapWidth, parameters.mapHeight))
        using (Graphics clipGraphics = Graphics.FromImage(clipBitmap))
        {
          clipGraphics.DrawImage(tileBitmap, 0, 0, clipRect, GraphicsUnit.Pixel);
          return clipBitmap.BitmapToByteArray();
        }
      }
    }

  }

  public interface IDxfTileService
  {
    Task<byte[]> GetDxfBitmap(MapParameters parameters, IEnumerable<FileData> dxfFiles);
  }
}
