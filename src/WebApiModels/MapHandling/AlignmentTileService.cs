using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// Provides alignemnt tile functionality for reports
  /// </summary>
  public class AlignmentTileService : IAlignmentTileService
  {
    private readonly ILogger log;
    private readonly ILoggerFactory logger;
    private readonly IASNodeClient raptorClient;
    public AlignmentTileService(ILoggerFactory logger, IASNodeClient raptor)
    {
      log = logger.CreateLogger<AlignmentTileService>();
      this.logger = logger;
      raptorClient = raptor;
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
      if (alignmentDescriptors.Any())
      {
        using (Bitmap bitmap = new Bitmap(parameters.mapWidth, parameters.mapHeight))
        using (Graphics g = Graphics.FromImage(bitmap))
        {
          foreach (var alignmentDescriptor in alignmentDescriptors)
          {
            IEnumerable<WGSPoint> alignmentPoints = GetAlignmentPoints(projectId, alignmentDescriptor);

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

    /// <summary>
    /// Gets the list of points making up the alignment center line. 
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="alignDescriptor">Design descriptor for the alignment file</param>
    /// <returns></returns>
    private IEnumerable<WGSPoint> GetAlignmentPoints(long projectId, DesignDescriptor alignDescriptor)
    {
      var description = TileServiceUtils.DesignDescriptionForLogging(alignDescriptor);
      log.LogDebug($"GetAlignmentPoints: projectId={projectId}, alignment={description}");
      List<WGSPoint> alignmentPoints = null;
      if (alignDescriptor != null)
      {
        //Get the station extents
        TVLPDDesignDescriptor alignmentDescriptor = RaptorConverters.DesignDescriptor(alignDescriptor);
        double startStation = 0;
        double endStation = 0;
        bool success = raptorClient.GetStationExtents(projectId, alignmentDescriptor,
          out startStation, out endStation);
        if (success)
        {
          log.LogDebug($"GetAlignmentPoints: projectId={projectId}, station range={startStation}-{endStation}");

          //Get the alignment points
          TWGS84Point[] pdsPoints = null;

          success = raptorClient.GetDesignFilterBoundaryAsPolygon(
            DesignProfiler.ComputeDesignFilterBoundary.RPC.__Global.Construct_CalculateDesignFilterBoundary_Args(
              projectId,
              alignmentDescriptor,
              startStation, endStation, 0, 0,
              DesignProfiler.ComputeDesignFilterBoundary.RPC.TDesignFilterBoundaryReturnType.dfbrtList), out pdsPoints);

          if (success && pdsPoints != null && pdsPoints.Length > 0)
          {
            log.LogDebug($"GetAlignmentPoints success: projectId={projectId}, number of points={pdsPoints.Length}");

            alignmentPoints = new List<WGSPoint>();
            //We only need half the points as normally GetDesignFilterBoundaryAsPolygon has offsets so is returning a polygon.
            //Since we have no offsets we have the centreline twice.
            int count = pdsPoints.Length / 2;
            for (int i = 0; i < count; i++)
            {
              alignmentPoints.Add(WGSPoint.CreatePoint(pdsPoints[i].Lat, pdsPoints[i].Lon));
            }
          }
        }
      }
      return alignmentPoints;
    }

  }
  public interface IAlignmentTileService
  {
    byte[] GetAlignmentsBitmap(MapParameters parameters, long projectId, IEnumerable<DesignDescriptor> alignmentDescriptors);
  }
}
