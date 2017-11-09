using System.Drawing;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  public class ProjectTileService
  {
    private readonly ILogger log;
    private readonly ILoggerFactory logger;


    public ProjectTileService(ILoggerFactory logger)
    {
      log = logger.CreateLogger<ProjectTileService>();
      this.logger = logger;
    }

    public byte[] GetProjectBitmap(MapParameters parameters, ProjectDescriptor project)
    {
      const int PROJECT_BOUNDARY_COLOR = 0xFF8000;
      const int STROKE_TRANSPARENCY = 0x73; //0.45 of FF
      const int PROJECT_OUTLINE_WIDTH = 4;

      byte[] projectImage = null;

      if (project != null)
      {
        using (Bitmap bitmap = new Bitmap(parameters.mapWidth, parameters.mapHeight))
        using (Graphics g = Graphics.FromImage(bitmap))
        {
          var projectPoints = TileServiceUtils.GeometryToPoints(project.projectGeofenceWKT);
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
    byte[] GetProjectBitmap(MapParameters parameters, ProjectDescriptor project);
  }
}
