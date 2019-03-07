﻿using System;
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
  public class DxfTileExecutor : RequestExecutorContainer
  {

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as DxfTileRequest;

      if (request == null)
        ThrowRequestTypeCastException<DxfTileRequest>();

      //Calculate zoom level
      int zoomLevel = TileServiceUtils.CalculateZoomLevel(request.bbox.TopRightLat - request.bbox.BottomLeftLat,
        request.bbox.TopRightLon - request.bbox.BottomLeftLon);
      log.LogDebug("DxfTileExecutor: BBOX differences {0} {1} {2}", request.bbox.TopRightLat - request.bbox.BottomLeftLat,
        request.bbox.TopRightLon - request.bbox.BottomLeftLon, zoomLevel);
      int numTiles = TileServiceUtils.NumberOfTiles(zoomLevel);
      Point topLeftLatLng = new Point(request.bbox.TopRightLat.LatRadiansToDegrees(),
        request.bbox.BottomLeftLon.LonRadiansToDegrees());
      Point topLeftTile = WebMercatorProjection.LatLngToTile(topLeftLatLng, numTiles);
      log.LogDebug("DxfTileExecutor: zoomLevel={0}, numTiles={1}, xtile={2}, ytile={3}", zoomLevel, numTiles,
        topLeftTile.x, topLeftTile.y);

      log.LogDebug("DxfTileExecutor: {0} files", request.files.Count());

      //Short circuit overlaying if there no files to overlay as ForAll is an expensive operation
      if (!request.files.Any())
      {
        byte[] emptyOverlayData = null;
        using (Bitmap bitmap = new Bitmap(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE))
        {
          emptyOverlayData = bitmap.BitmapToByteArray();
        }
        return new TileResult(emptyOverlayData);
      }

      log.LogDebug(string.Join(",", request.files.Select(f => f.Name).ToList()));

      List<byte[]> tileList = new List<byte[]>();
      var rootFolder = configStore.GetValueString("DATA_OCEAN_ROOT_FOLDER");

      var fileTasks = request.files.Select(async file =>
      {
        //foreach (var file in request.files)
        //Check file type to see if it has tiles
        if (file.ImportedFileType == ImportedFileType.Linework || file.ImportedFileType == ImportedFileType.Alignment)
        {
          var fullPath = DataOceanFileUtil.DataOceanPath(rootFolder, file.CustomerUid, file.ProjectUid);
          var fileName = DataOceanFileUtil.GeneratedFileName(file.Name, file.ImportedFileType);

          if (zoomLevel >= file.MinZoomLevel)
          {
            byte[] tileData = null;
            if (zoomLevel <= file.MaxZoomLevel || file.MaxZoomLevel == 0) //0 means not calculated
            {
              tileData = await GetTileAtRequestedZoom(topLeftTile, zoomLevel, fullPath, fileName);
            }
            else if (zoomLevel - file.MaxZoomLevel <= 5) //Don't try to scale if the difference is too excessive
            {
              tileData = await GetTileAtHigherZoom(topLeftTile, zoomLevel, fullPath, fileName,
                file.MaxZoomLevel, numTiles);

            }
            else
            {
              log.LogDebug(
                "DxfTileExecutor: difference between requested and maximum zooms too large; not even going to try to scale tile");
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
      log.LogDebug("DxfTileExecutor: Overlaying {0} tiles", tileList.Count);
      byte[] overlayData = null;
      System.Drawing.Point origin = new System.Drawing.Point(0, 0);
      using (Bitmap bitmap = new Bitmap(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE))
      using (Graphics g = Graphics.FromImage(bitmap))
      {
        foreach (byte[] tileData in tileList)
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
      string fullTileName = GetFullTileName(topLeftTile, zoomLevel, path, fileName);
      log.LogDebug("DxfTileExecutor: looking for requested tile {0}", fullTileName);

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
      int zoomLevelFound = maxZoomLevel;

      // Calculate the tile coords of the higher zoom level tile that covers the requested tile
      Point ptRequestedTile = new Point(topLeftTile.y, topLeftTile.x);
      Point ptRequestedPixel = WebMercatorProjection.TileToPixel(ptRequestedTile);

      int numTilesAtRequestedZoomLevel = numTiles;
      Point ptRequestedWorld =
        WebMercatorProjection.PixelToWorld(ptRequestedPixel, numTilesAtRequestedZoomLevel);

      int numTilesAtFoundZoomLevel = TileServiceUtils.NumberOfTiles(maxZoomLevel);
      Point ptHigherPixel = WebMercatorProjection.WorldToPixel(ptRequestedWorld, numTilesAtFoundZoomLevel);

      Point ptHigherTile = WebMercatorProjection.PixelToTile(ptHigherPixel);
      //Note PixelToTile uses Math.Floor so this tile coordinate will be the top left of the tile

      // With the new tile coords of the higher zoom level tile, see if it exists in DataOcean
      string fullHigherTileName = GetFullTileName(ptHigherTile, zoomLevelFound, path, fileName);
      log.LogDebug("DxfTileExecutor: looking for higher tile {0}", fullHigherTileName);

      byte[] tileData = await DownloadTile(fullHigherTileName, "higher");

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
      Point ptHigherWorldTopLeft =
        WebMercatorProjection.PixelToWorld(WebMercatorProjection.TileToPixel(ptHigherTileTopLeft),
          numTilesAtFoundZoomLevel);
      Point ptHigherWorldBotRight =
        WebMercatorProjection.PixelToWorld(WebMercatorProjection.TileToPixel(ptHigherTileBotRight),
          numTilesAtFoundZoomLevel);

      double ratioX = (ptRequestedWorld.x - ptHigherWorldTopLeft.x) /
                      (ptHigherWorldBotRight.x - ptHigherWorldTopLeft.x);
      double ratioY = (ptRequestedWorld.y - ptHigherWorldTopLeft.y) /
                      (ptHigherWorldBotRight.y - ptHigherWorldTopLeft.y);

      int startX = (int)Math.Floor(WebMercatorProjection.TILE_SIZE * ratioX);
      int startY = (int)Math.Floor(WebMercatorProjection.TILE_SIZE * ratioY);

      // Calculate how much up-scaling of higher level zoom tile we need to do
      // based on the difference between the requested and higher zoom levels
      int croppedTileSize = WebMercatorProjection.TILE_SIZE / (1 << zoomLevelDifference);

      // Set the crop rectangle and draw it into a new bitmap, scaling it up to the standard tile size
      Rectangle cropRect = new Rectangle(startX, startY, croppedTileSize, croppedTileSize);
      log.LogDebug("DxfTileExecutor: crop rectangle x = {0}, y = {1}, size = {2}", startX, startY,
        croppedTileSize);

      //source bitmap
      using (var tileStream = new MemoryStream(tileData))
      using (Bitmap higherBitmap = new Bitmap(tileStream))
      //destination bitmap
      using (Bitmap target = new Bitmap(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE))
      using (Graphics g = Graphics.FromImage(target))
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
        log.LogDebug($"DxfTileExecutor: {what} tile downloaded with size of {stream.Length} bytes");

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
          $"DxfTileExecutor: tile at {what} zoom level does not exist - design simply doesn't fall over the requested tile");
      }
      return tileData;
    }
  }
}