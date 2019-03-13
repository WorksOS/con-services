using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Extensions;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Tile.Service.Common.Helpers;
using VSS.Tile.Service.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Point = VSS.MasterData.Models.Models.Point;

namespace VSS.Tile.Service.Common.Executors
{
  /// <summary>
  /// Processes the request to get a DXF tile.
  /// </summary>
  public class DxfTile3dExecutor : RequestExecutorContainer
  {

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as DxfTile3dRequest;

      if (request == null)
        ThrowRequestTypeCastException<DxfTileRequest>();

      var numTiles = TileServiceUtils.NumberOfTiles(request.zoomLevel);
      var topLeftTile = new Point { x = request.xTile, y = request.yTile };

      log.LogDebug("DxfTile3dExecutor: {0} files", request.files.Count());

      //Short circuit overlaying if there no files to overlay as ForAll is an expensive operation
      if (!request.files.Any())
      {
        byte[] emptyOverlayData = null;
        using (var bitmap = new Bitmap(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE))
        {
          emptyOverlayData = bitmap.BitmapToByteArray();
        }
        return new TileResult(emptyOverlayData);
      }

      log.LogDebug(string.Join(",", request.files.Select(f => f.Name).ToList()));

      var tileList = new List<byte[]>();
      var rootFolder = configStore.GetValueString("DATA_OCEAN_ROOT_FOLDER");

      var fileTasks = request.files.Select(async file =>
      {
        //foreach (var file in request.files)
        //Check file type to see if it has tiles
        if (file.ImportedFileType == ImportedFileType.Linework || file.ImportedFileType == ImportedFileType.Alignment)
        {
          var fullPath = DataOceanFileUtil.DataOceanPath(rootFolder, file.CustomerUid, file.ProjectUid);
          var fileName = DataOceanFileUtil.GeneratedFileName(file.Name, file.ImportedFileType);

          if (request.zoomLevel >= file.MinZoomLevel)
          {
            byte[] tileData = null;
            if (request.zoomLevel <= file.MaxZoomLevel || file.MaxZoomLevel == 0) //0 means not calculated
            {
              tileData = await GetTileAtRequestedZoom(topLeftTile, request.zoomLevel, fullPath, fileName);
            }
            else if (request.zoomLevel - file.MaxZoomLevel <= 5) //Don't try to scale if the difference is too excessive
            {
              tileData = await GetTileAtHigherZoom(topLeftTile, request.zoomLevel, fullPath, fileName,
                file.MaxZoomLevel, numTiles);

            }
            else
            {
              log.LogDebug(
                "DxfTile3dExecutor: difference between requested and maximum zooms too large; not even going to try to scale tile");
            }
            if (tileData != null)
            {
              tileList.Add(tileData);
            }
          }
        }
      });

      await Task.WhenAll(fileTasks);

      //Overlay the tiles. Return an empty tile if none to overlay.
      log.LogDebug("DxfTile3dExecutor: Overlaying {0} tiles", tileList.Count);
      byte[] overlayData = null;
     var origin = new System.Drawing.Point(0, 0);
      using (var bitmap = new Bitmap(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE))
      using (var g = Graphics.FromImage(bitmap))
      {
        foreach (var tileData in tileList)
        {
          using (var tileStream = new MemoryStream(tileData))
          {
            Image image = Image.FromStream(tileStream);
            g.DrawImage(image, origin);
          }
        }
        overlayData = bitmap.BitmapToByteArray();
      }

      return new TileResult(overlayData);
    }

    /// <summary>
    /// Gets a tile at the requested zoom level.
    /// </summary>
    /// <param name="topLeftTile">The top left tile coordinates</param>
    /// <param name="zoomLevel">The requested zoom level</param>
    /// <param name="path">The file path</param>
    /// <param name="fileName">The name of the DXF file</param>
    /// <returns>A generated tile</returns>
    private async Task<byte[]> GetTileAtRequestedZoom(Point topLeftTile, int zoomLevel, string path,
      string fileName)
    {
      //Work out tile location
      var fullTileName = GetFullTileName(topLeftTile, zoomLevel, path, fileName);
      log.LogDebug("DxfTile3dExecutor: looking for requested tile {0}", fullTileName);

      //Download the tile
      return await DownloadTile(fullTileName, "requested");
    }

    /// <summary>
    /// Gets a tile at a higher zoom level and scales part of it for the requested tile
    /// </summary>
    /// <param name="topLeftTile">The top left tile coordinates</param>
    /// <param name="zoomLevel">The requested zoom level</param>
    /// <param name="path">The file path</param>
    /// <param name="fileName">The name of the DXF file</param>
    /// <param name="maxZoomLevel">The maximum zoom level for which tiles have been generated</param>
    /// <param name="numTiles">The number of tiles for the requested zoom level</param>
    /// <returns>A scaled tile</returns>
    private async Task<byte[]> GetTileAtHigherZoom(Point topLeftTile, int zoomLevel, string path,
      string fileName, int maxZoomLevel, int numTiles)
    {
      var zoomLevelFound = maxZoomLevel;

      // Calculate the tile coords of the higher zoom level tile that covers the requested tile
      var ptRequestedTile = new Point(topLeftTile.y, topLeftTile.x);
      var ptRequestedPixel = WebMercatorProjection.TileToPixel(ptRequestedTile);

      var numTilesAtRequestedZoomLevel = numTiles;
      var ptRequestedWorld =
        WebMercatorProjection.PixelToWorld(ptRequestedPixel, numTilesAtRequestedZoomLevel);

      var numTilesAtFoundZoomLevel = TileServiceUtils.NumberOfTiles(maxZoomLevel);
      var ptHigherPixel = WebMercatorProjection.WorldToPixel(ptRequestedWorld, numTilesAtFoundZoomLevel);

      var ptHigherTile = WebMercatorProjection.PixelToTile(ptHigherPixel);
      //Note PixelToTile uses Math.Floor so this tile coordinate will be the top left of the tile

      // With the new tile coords of the higher zoom level tile, see if it exists in DataOcean
      var fullHigherTileName = GetFullTileName(ptHigherTile, zoomLevelFound, path, fileName);
      log.LogDebug("DxfTile3dExecutor: looking for higher tile {0}", fullHigherTileName);

     var tileData = await DownloadTile(fullHigherTileName, "higher");

      if (tileData != null)
      {
        tileData = ScaleTile(tileData, zoomLevel - zoomLevelFound, ptHigherTile, ptRequestedWorld,
          numTilesAtFoundZoomLevel);
      }
      return tileData;
    }

    /// <summary>
    /// Scales a tile
    /// </summary>
    /// <param name="tileData">The tile to scale</param>
    /// <param name="zoomLevelDifference">The difference between the downloaded tile and the rqeuested tile zoom levels</param>
    /// <param name="ptHigherTile"></param>
    /// <param name="ptRequestedWorld"></param>
    /// <param name="numTilesAtFoundZoomLevel">The number of tiles for the higher zoom level</param>
    /// <returns>A scaled tile</returns>
    private byte[] ScaleTile(byte[] tileData, int zoomLevelDifference, Point ptHigherTile, Point ptRequestedWorld,
      int numTilesAtFoundZoomLevel)
    {
      // Calculate the tile coords of the BR corner of the higher zoom level tile
      // so that we can identify which sub-part of it to crop and scale
      Point ptHigherTileTopLeft = ptHigherTile;
      Point ptHigherTileBotRight = new Point(ptHigherTile.y + 1, ptHigherTile.x + 1);

      // Calculate the sub-tile rectangle that we need to crop out of the  higher tile
      // using a simple proportion calculation based on which part of the higher tile
      // covers the original requested tile in world coordinates
      var ptHigherWorldTopLeft =
        WebMercatorProjection.PixelToWorld(WebMercatorProjection.TileToPixel(ptHigherTileTopLeft),
          numTilesAtFoundZoomLevel);
      var ptHigherWorldBotRight =
        WebMercatorProjection.PixelToWorld(WebMercatorProjection.TileToPixel(ptHigherTileBotRight),
          numTilesAtFoundZoomLevel);

      var ratioX = (ptRequestedWorld.x - ptHigherWorldTopLeft.x) /
                      (ptHigherWorldBotRight.x - ptHigherWorldTopLeft.x);
      var ratioY = (ptRequestedWorld.y - ptHigherWorldTopLeft.y) /
                      (ptHigherWorldBotRight.y - ptHigherWorldTopLeft.y);

      var startX = (int)Math.Floor(WebMercatorProjection.TILE_SIZE * ratioX);
      var startY = (int)Math.Floor(WebMercatorProjection.TILE_SIZE * ratioY);

      // Calculate how much up-scaling of higher level zoom tile we need to do
      // based on the difference between the requested and higher zoom levels
      var croppedTileSize = WebMercatorProjection.TILE_SIZE / (1 << zoomLevelDifference);

      // Set the crop rectangle and draw it into a new bitmap, scaling it up to the standard tile size
      var cropRect = new Rectangle(startX, startY, croppedTileSize, croppedTileSize);
      log.LogDebug("DxfTile3dExecutor: crop rectangle x = {0}, y = {1}, size = {2}", startX, startY,
        croppedTileSize);

      //source bitmap
      using (var tileStream = new MemoryStream(tileData))
      using (var higherBitmap = new Bitmap(tileStream))
      //destination bitmap
      using (var target = new Bitmap(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE))
      using (var g = Graphics.FromImage(target))
      {
        g.CompositingMode = CompositingMode.SourceCopy;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.DrawImage(higherBitmap, new Rectangle(0, 0, target.Width, target.Height), cropRect,
          GraphicsUnit.Pixel);
        g.Flush();
        return target.BitmapToByteArray();
      }
    }

    /// <summary>
    /// Gets the full file name for a tile
    /// </summary>
    /// <param name="topLeftTile">The top left tile coordinates</param>
    /// <param name="zoomLevel">The zoom level of the tile</param>
    /// <param name="path">The file path</param>
    /// <param name="fileName">The name of the DXF file</param>
    /// <returns></returns>
    private string GetFullTileName(Point topLeftTile, int zoomLevel, string path, string fileName)
    {
      var dataOceanFileUtil = new DataOceanFileUtil(fileName, path);
      return dataOceanFileUtil.GetTileFileName(zoomLevel, (int)topLeftTile.y, (int)topLeftTile.x);
    }

    /// <summary>
    /// Tries to download a tile from DataOcean
    /// </summary>
    /// <param name="fullTileName">The full filename of the tile</param>
    /// <param name="what">Either 'requested' or 'higher' used for logging</param>
    /// <returns>The downloaded tile if it exists</returns>
    private async Task<byte[]> DownloadTile(string fullTileName, string what)
    {
      byte[] tileData = null;
      var stream = await dataOceanClient.GetFile(fullTileName, authn.CustomHeaders());
      if (stream != null)
      {
        log.LogDebug($"DxfTile3dExecutor: {what} tile downloaded with size of {stream.Length} bytes");

        if (stream.Length > 0)
        {
          stream.Position = 0;
          tileData = new byte[stream.Length];
          stream.Read(tileData, 0, (int)stream.Length);
        }
        stream.Dispose();
      }
      else
      {
        log.LogDebug(
          $"DxfTile3dExecutor: tile at {what} zoom level does not exist - design simply doesn't fall over the requested tile");
      }
      return tileData;
    }
  }
}
