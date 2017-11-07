using ASNodeDecls;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Point = VSS.Productivity3D.WebApi.Models.MapHandling.Point;

namespace VSS.Productivity3D.WebApiModels.Compaction.Executors
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
      DxfTileRequest request = item as DxfTileRequest;

      string filespaceId = FileDescriptor.GetFileSpaceId(configStore, log);

      //Calculate zoom level
      int zoomLevel = TileServiceUtils.CalculateZoomLevel(request.bbox.topRightLat - request.bbox.bottomLeftLat, request.bbox.topRightLon - request.bbox.bottomLeftLon);
      int numTiles = TileServiceUtils.NumberOfTiles(zoomLevel);
      Point topLeftLatLng = new Point(request.bbox.topRightLat.latRadiansToDegrees(), request.bbox.bottomLeftLon.lonRadiansToDegrees());
      Point topLeftTile = WebMercatorProjection.LatLngToTile(topLeftLatLng, numTiles);
      log.LogDebug("DxfTileExecutor: zoomLevel={0}, numTiles={1}, xtile={2}, ytile={3}", zoomLevel, numTiles, topLeftTile.x, topLeftTile.y);

      log.LogDebug("DxfTileExecutor: {0} files", request.files.Count());
      log.LogDebug(string.Join(",", request.files.Select(f => f.Name).ToList()));

      List<byte[]> tileList = new List<byte[]>();
      foreach (var file in request.files)
      {
        //Check file type to see if it has tiles
        if (file.ImportedFileType == ImportedFileType.Alignment ||
            file.ImportedFileType == ImportedFileType.DesignSurface ||
            file.ImportedFileType == ImportedFileType.Linework)
        {
          //Work out tile location
          var suffix = FileUtils.GeneratedFileSuffix(file.ImportedFileType);
          string generatedName = FileUtils.GeneratedFileName(file.Name, suffix, FileUtils.DXF_FILE_EXTENSION);
          string fullTileName =
            $"{FileUtils.ZoomPath(FileUtils.TilePath(file.Path, generatedName), zoomLevel)}/{topLeftTile.y}/{topLeftTile.x}.png";
          log.LogDebug("DxfTileExecutor: looking for requested tile {0}", fullTileName);

          //Download the tile
          if (await fileRepo.FileExists(filespaceId, fullTileName))
          {
            using (Stream stream = await fileRepo.GetFile(filespaceId, fullTileName))
            {
              log.LogDebug("DxfTileExecutor: tile downloaded with size of {0} bytes", stream.Length);

              if (stream.Length > 0)
              {
                stream.Position = 0;
                byte[] tileData = new byte[stream.Length];
                stream.Read(tileData, 0, (int) stream.Length);
                tileList.Add(tileData);
              }
            }
          }
          else
          {
            log.LogDebug("DxfTileExecutor: tile at requested zoom level does not exist - design simply doesn't fall over the requested tile");
          }           
        }
      }

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

      return TileResult.CreateTileResult(overlayData, TASNodeErrorStatus.asneOK);
    }
  }
}
