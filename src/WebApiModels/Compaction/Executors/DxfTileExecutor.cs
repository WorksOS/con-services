using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using TCCFileAccess;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using WebApiModels.Compaction.Helpers;
using WebApiModels.Compaction.Models;
using WebApiModels.Notification.Helpers;
using Point = WebApiModels.Notification.Helpers.Point;

namespace WebApiModels.Compaction.Executors
{
  /// <summary>
  /// Processes the request to get a DXF tile.
  /// </summary>
  public class DxfTileExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="raptorClient"></param>
    /// <param name="fileRepository"></param>
    /// <param name="tileGenerator"></param>
    public DxfTileExecutor(ILoggerFactory logger, IConfigurationStore configStore, IFileRepository fileRepository) :
      base(logger, null, null, configStore, fileRepository, null)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DxfTileExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        DxfTileRequest request = item as DxfTileRequest;

        string filespaceId = GetFilespaceId();

        //Calculate zoom level
        int zoomLevel = CalculateZoomLevel(request.bbox);
        int numTiles = 1 << zoomLevel; //equivalent to 2 to the power of zoomLevel
        Point topLeftLatLng = new Point(WebMercatorProjection.RadiansToDegrees(request.bbox.topRightLat), WebMercatorProjection.RadiansToDegrees(request.bbox.bottomLeftLon));
        Point topLeftTile = WebMercatorProjection.LatLngToTile(topLeftLatLng, numTiles);
        log.LogDebug("DxfTileExecutor: zoomLevel={0}, numTiles={1}, topLeftTile={2},{3}", zoomLevel, numTiles, topLeftTile.x, topLeftTile.y);

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
            string fullTileName = string.Format("{0}/{1}/{2}.png", FileUtils.ZoomPath(FileUtils.TilePath(file.Path, generatedName), zoomLevel), topLeftTile.y, topLeftTile.x);
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
      finally
      {
        
      }

    }

    /// <summary>
    /// Gets the TCC filespaceId for the vldatastore filespace
    /// </summary>
    /// <returns></returns>
    private string GetFilespaceId()
    {
      string filespaceId = configStore.GetValueString("TCCFILESPACEID");
      if (string.IsNullOrEmpty(filespaceId))
      {
        var errorString = "Your application is missing an environment variable TCCFILESPACEID";
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }
      return filespaceId;
    }

    /// <summary>
    /// Calculates the zoom level from the bounding box
    /// </summary>
    /// <param name="bbox">The bounding box of the tile</param>
    /// <returns>The zoom level</returns>
    private int CalculateZoomLevel(BoundingBox2DLatLon bbox)
    {
      const int MAXZOOM = 24;

      double selectionLatSize = Math.Abs(bbox.topRightLat - bbox.bottomLeftLat);
      double selectionLongSize = Math.Abs(bbox.topRightLon - bbox.bottomLeftLon);

      //Google maps zoom level starts at 0 for whole world (-90.0 to 90.0, -180.0 to 180.0)
      //and doubles the precision both horizontally and vertically for each suceeding level.
      int zoomLevel = 0;
      double latSize = Math.PI; //180.0;
      double longSize = 2 * Math.PI; //360.0;
      while (latSize > selectionLatSize && longSize > selectionLongSize && zoomLevel < MAXZOOM)
      {
        zoomLevel++;
        latSize /= 2;
        longSize /= 2;
      }
      return zoomLevel;
    }
  }
}
