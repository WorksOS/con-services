using System.Drawing;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// Provides project boundary tile functionality for reports
  /// </summary>
  public class ProjectTileService : IProjectTileService
  {
    private readonly ILogger log;
    private readonly ILoggerFactory logger;


    public ProjectTileService(ILoggerFactory logger)
    {
      log = logger.CreateLogger<ProjectTileService>();
      this.logger = logger;
    }

    /// <summary>
    /// Gets a map tile with the project boundary drawn on it.
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="project">The project to draw the boundary for</param>
    /// <returns>A bitmap</returns>
    public byte[] GetProjectBitmap(MapParameters parameters, ProjectData project)
    {
      log.LogInformation($"GetProjectBitmap: project {project.ProjectUid}");

      const int PROJECT_BOUNDARY_COLOR = 0xFF8000;
      const int STROKE_TRANSPARENCY = 0x73; //0.45 of FF
      const int PROJECT_OUTLINE_WIDTH = 4;

      byte[] projectImage = null;

      if (project != null)
      {
        using (Bitmap bitmap = new Bitmap(parameters.mapWidth, parameters.mapHeight))
        using (Graphics g = Graphics.FromImage(bitmap))
        {
          var projectPoints = RaptorConverters.geometryToPoints(project.ProjectGeofenceWKT);
          PointF[] pixelPoints = TileServiceUtils.LatLngToPixelOffset(projectPoints, parameters.pixelTopLeft, parameters.numTiles);

          Pen pen = new Pen(Color.FromArgb(STROKE_TRANSPARENCY, Color.FromArgb(PROJECT_BOUNDARY_COLOR)),
            PROJECT_OUTLINE_WIDTH);
          g.DrawPolygon(pen, pixelPoints);

          projectImage = bitmap.BitmapToByteArray();
        }
      }

      return projectImage;
    }
  }

  public interface IProjectTileService
  {
    byte[] GetProjectBitmap(MapParameters parameters, ProjectData project);
  }
}
