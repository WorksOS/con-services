using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Tile.Service.Common.Extensions;
using VSS.Tile.Service.Common.Helpers;
using VSS.Tile.Service.Common.Models;

namespace VSS.Tile.Service.Common.Services
{
  /// <summary>
  /// Provides project boundary tile functionality for reports
  /// </summary>
  public class ProjectTileService : IProjectTileService
  {
    private readonly ILogger log;

    public ProjectTileService(ILoggerFactory logger)
    {
      log = logger.CreateLogger<ProjectTileService>();
    }

    /// <summary>
    /// Gets a map tile with the project boundary drawn on it.
    /// </summary>
    /// <param name="parameters">Map parameters such as bounding box, tile size, zoom level etc.</param>
    /// <param name="project">The project to draw the boundary for</param>
    /// <returns>A bitmap</returns>
    public byte[] GetProjectBitmap(MapParameters parameters, ProjectData project)
    {
      log.LogInformation($"GetProjectBitmap: project {project.ProjectUID}");

      const int PROJECT_BOUNDARY_COLOR = 0x0080FF;//Note: packed is abgr order
      const int STROKE_TRANSPARENCY = 0x73; //0.45 of FF
      Rgba32 PROJECT_BOUNDARY_RGBA = new Rgba32((uint)((STROKE_TRANSPARENCY << 24) | PROJECT_BOUNDARY_COLOR));
      const int PROJECT_OUTLINE_WIDTH = 4;

      byte[] projectImage = null;

      if (project != null)
      {
        using (Image<Rgba32> bitmap = new Image<Rgba32>(parameters.mapWidth, parameters.mapHeight))
        {
          var projectPoints = project.GeometryWKT.GeometryToPoints();
          PointF[] pixelPoints = TileServiceUtils.LatLngToPixelOffset(projectPoints, parameters.pixelTopLeft, parameters.numTiles);

          bitmap.Mutate(ctx => ctx.DrawPolygon(PROJECT_BOUNDARY_RGBA, PROJECT_OUTLINE_WIDTH, pixelPoints));

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
