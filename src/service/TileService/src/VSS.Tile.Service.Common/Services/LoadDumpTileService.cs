using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using VSS.MasterData.Models.Models;
using VSS.Tile.Service.Common.Extensions;
using VSS.Tile.Service.Common.Helpers;
using VSS.Tile.Service.Common.Models;

namespace VSS.Tile.Service.Common.Services
{
  /// <summary>
  /// Provides load/dump tile functionality for reports
  /// </summary>
  public class LoadDumpTileService : ILoadDumpTileService
  {
    private readonly ILogger log;

    public LoadDumpTileService(ILoggerFactory logger)
    {
      log = logger.CreateLogger<LoadDumpTileService>();
    }
    /// <summary>
    /// Gets a map tile with load/dump locations drawn on it.
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="loadDumpLocations">List of Load/Dump locations</param>
    /// <returns>A bitmap</returns>
    public byte[] GetLoadDumpBitmap(MapParameters parameters, List<LoadDumpLocation> loadDumpLocations)
    {
      log.LogInformation($"GetLoadDumpBitmap");

      //Note: packed is abgr order
      const uint LOAD_COLOR = 0xFF008F01; //Green 0x018F00
      const uint DUMP_COLOR = 0xFFFF3304; //Blue 0x0433FF
      Rgba32 LOAD_RGBA = new Rgba32(LOAD_COLOR);
      Rgba32 DUMP_RGBA = new Rgba32(DUMP_COLOR);
      var loadPen = new Pen<Rgba32>(LOAD_RGBA, 1);
      var dumpPen = new Pen<Rgba32>(DUMP_RGBA, 1);

      byte[] loadDumpImage = null;
      if (loadDumpLocations != null && loadDumpLocations.Any())
      {
        using (Image<Rgba32> bitmap = new Image<Rgba32>(parameters.mapWidth, parameters.mapHeight))
        {
          IEnumerable<WGSPoint> loads = loadDumpLocations
            .Select(x => new WGSPoint(x.loadLatitude.LatDegreesToRadians(), x.loadLongitude.LonDegreesToRadians())).ToList();          
          PointF[] pixelPoints = TileServiceUtils.LatLngToPixelOffset(loads, parameters.pixelTopLeft, parameters.numTiles);
          foreach (var p in pixelPoints)
          {
            //Coloring one pixel doesn't show up well therefore do rectangle of 4 pixels
            var x = (int) p.X;
            var y = (int) p.Y;
            var rect = new RectangleF(x, y, 2, 2);
            bitmap.Mutate(ctx => ctx.Draw(loadPen, rect));
            //bitmap[(int) p.X, (int) p.Y] = LOAD_RGBA;
          }

          IEnumerable<WGSPoint> dumps = loadDumpLocations
            .Select(x => new WGSPoint(x.dumpLatitude.LatDegreesToRadians(), x.dumpLongitude.LonDegreesToRadians())).ToList();
          pixelPoints = TileServiceUtils.LatLngToPixelOffset(dumps, parameters.pixelTopLeft, parameters.numTiles);
          foreach (var p in pixelPoints)
          {
            var x = (int)p.X;
            var y = (int)p.Y;
            var rect = new RectangleF(x, y, 2, 2);
            bitmap.Mutate(ctx => ctx.Draw(dumpPen, rect));
            //bitmap[(int) p.X, (int) p.Y] = DUMP_RGBA;
          }

          loadDumpImage = bitmap.BitmapToByteArray();
        }
      }

      return loadDumpImage;

    }
  }

  public interface ILoadDumpTileService
  {
    byte[] GetLoadDumpBitmap(MapParameters parameters, List<LoadDumpLocation> loadDumpLocations);
  }
}
