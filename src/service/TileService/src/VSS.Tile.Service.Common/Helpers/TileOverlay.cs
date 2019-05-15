using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using VSS.MasterData.Models.Models;
using VSS.Tile.Service.Common.Extensions;

namespace VSS.Tile.Service.Common.Helpers
{
  public class TileOverlay
  {
    private readonly ILogger Logger;

    public TileOverlay(ILogger logger)
    {
      Logger = logger;
    }

    /// <summary>
    /// Overlay the tiles. Return an empty tile if none to overlay.
    /// </summary>
    public byte[] OverlayTiles(List<byte[]> tileList)
    {
      Logger.LogDebug($"DxfTileExecutor: Overlaying {tileList.Count} tiles");

      using (var bitmap = new Image<Rgba32>(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE))
      {
        foreach (var tileData in tileList)
        {
          if (tileData.Length < 1) continue;

          using (var tileStream = new MemoryStream(tileData))
          {
            var image = Image.Load<Rgba32>(tileStream);

            bitmap.Mutate(ctx => ctx.DrawImage(image, 1f));
          }
        }

        /*
        //Remove transparency from drawn lines
        for (var i = 0; i < WebMercatorProjection.TILE_SIZE; i++)
          for (var j = 0; j < WebMercatorProjection.TILE_SIZE; j++)
          {
            if (bitmap[i, j].A > 0)
            {
              bitmap[i, j] = new Rgba32(bitmap[i, j].R, bitmap[i, j].G, bitmap[i, j].B, byte.MaxValue);
            }
          }
          */
        return bitmap.BitmapToByteArray();
      }
    }
  }
}
