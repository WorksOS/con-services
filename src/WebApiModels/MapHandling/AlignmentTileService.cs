using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// Provides alignemnt tile functionality for reports
  /// </summary>
  public class AlignmentTileService : IAlignmentTileService
  {
    private readonly ILogger log;
    private readonly IBoundingBoxService boundingBoxService;

    public AlignmentTileService(ILoggerFactory logger, IBoundingBoxService boundingBoxService)
    {
      log = logger.CreateLogger<AlignmentTileService>();
      this.boundingBoxService = boundingBoxService;
    }

    /// <summary>
    /// Gets a map tile with alignment center lines drawn on it.
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="alignmentDescriptors">Design descriptors for the project's alignment files</param>
    /// <returns>A bitmap</returns>
    public byte[] GetAlignmentsBitmap(MapParameters parameters, long projectId, IEnumerable<DesignDescriptor> alignmentDescriptors)
    {
      log.LogInformation($"GetAlignmentsBitmap: project {projectId}");

      byte[] alignmentsImage = null;
      if (alignmentDescriptors != null && alignmentDescriptors.Any())
      {
        using (Bitmap bitmap = new Bitmap(parameters.mapWidth, parameters.mapHeight))
        using (Graphics g = Graphics.FromImage(bitmap))
        {
          foreach (var alignmentDescriptor in alignmentDescriptors)
          {
            IEnumerable<WGSPoint3D> alignmentPoints = boundingBoxService.GetAlignmentPoints(
              projectId, alignmentDescriptor);

            if (alignmentPoints != null && alignmentPoints.Any())
            {
              PointF[] pixelPoints = TileServiceUtils.LatLngToPixelOffset(alignmentPoints, parameters.pixelTopLeft, parameters.numTiles);
              Pen pen = new Pen(Color.Red, 1);
              g.DrawLines(pen, pixelPoints);
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
    byte[] GetAlignmentsBitmap(MapParameters parameters, long projectId, IEnumerable<DesignDescriptor> alignmentDescriptors);    
  }
}
