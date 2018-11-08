using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using VSS.Tile.Service.Common.Extensions;
using VSS.Tile.Service.Common.Helpers;
using VSS.Tile.Service.Common.Models;
using VSS.MasterData.Models.Models;
using PointF = SixLabors.Primitives.PointF;


namespace VSS.Tile.Service.Common.Services
{
  /// <summary>
  /// Provides alignemnt tile functionality for reports
  /// </summary>
  public class AlignmentTileService : IAlignmentTileService
  {
    private readonly ILogger log;

    public AlignmentTileService(ILoggerFactory logger)
    {
      log = logger.CreateLogger<AlignmentTileService>();
    }

    /// <summary>
    /// Gets a map tile with alignment center lines drawn on it.
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="alignmentPointsList">Points for the project's alignment files</param>
    /// <returns>A bitmap</returns>
    public byte[] GetAlignmentsBitmap(MapParameters parameters, long projectId, List<List<WGSPoint>> alignmentPointsList)
    {
      log.LogInformation($"GetAlignmentsBitmap: project {projectId}");

      byte[] alignmentsImage = null;
      if (alignmentPointsList != null && alignmentPointsList.Any())
      {
        using (Image<Rgba32> bitmap = new Image<Rgba32>(parameters.mapWidth, parameters.mapHeight))
        {
          foreach (var alignmentPoints in alignmentPointsList)
          {
            if (alignmentPoints != null && alignmentPoints.Any())
            {
              PointF[] pixelPoints = TileServiceUtils.LatLngToPixelOffset(alignmentPoints, parameters.pixelTopLeft, parameters.numTiles);
              bitmap.Mutate(ctx => ctx.DrawLines(Rgba32.Red, 1, pixelPoints));
            }
          }
          alignmentsImage = bitmap.BitmapToByteArray();
        }
      }
      return alignmentsImage;
    }

  }

  public interface IAlignmentTileService
  {
    byte[] GetAlignmentsBitmap(MapParameters parameters, long projectId, List<List<WGSPoint>> alignmentPointsList);    
  }
}
