using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DesignProfilerDecls;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Coord.Executors;
using VSS.Productivity3D.WebApiModels.Coord.Models;
using VSS.Productivity3D.WebApiModels.Coord.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Executors;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  public class BoundingBoxService
  {
    private readonly ILogger log;
    private readonly ILoggerFactory logger;
    private readonly IASNodeClient raptorClient;

    public BoundingBoxService(ILoggerFactory logger, IASNodeClient raptor)
    {
      log = logger.CreateLogger<ProjectTileService>();
      this.logger = logger;
      raptorClient = raptor;
    }

    public MapBoundingBox GetBoundingBox(ProjectDescriptor project, Filter filter, DisplayMode? mode, Filter baseFilter, Filter topFilter)
    {
      MapBoundingBox bbox = null;

      //If the filter has an area then use it as the bounding box
      List<WGSPoint> filterPoints = new List<WGSPoint>();
      //Summary volumes potentially has 2 filters
      if (baseFilter != null || topFilter != null)
      {
        filterPoints.AddRange(GetFilterPoints(project.projectId, baseFilter));
        filterPoints.AddRange(GetFilterPoints(project.projectId, topFilter));
      }
      else
      {
        filterPoints.AddRange(GetFilterPoints(project.projectId, filter));
      }

      if (filterPoints.Count > 0)
      {
        bbox = new MapBoundingBox
        {
          minLat = filterPoints.Min(p => p.Lat),
          minLng = filterPoints.Min(p => p.Lon),
          maxLat = filterPoints.Max(p => p.Lat),
          maxLng = filterPoints.Min(p => p.Lon)
        };
      }
      else
      {
        //No area filter so use production data extents as the bounding box
        //Only applies if doing production data tiles
        if (mode.HasValue)
        {
          var productionDataExtents = GetProductionDataExtents(project.projectId, filter);
          if (productionDataExtents != null)
          {
            bbox = new MapBoundingBox
            {
              minLat = productionDataExtents.conversionCoordinates[0].y.latRadiansToDegrees(),
              minLng = productionDataExtents.conversionCoordinates[0].x.lonRadiansToDegrees(),
              maxLat = productionDataExtents.conversionCoordinates[1].y.latRadiansToDegrees(),
              maxLng = productionDataExtents.conversionCoordinates[1].x.lonRadiansToDegrees()
            };
          }
        }

        //Sometimes tag files way outside the project boundary are imported. These mean the data extents are
        //invalid and and give problems. So need to check for this and use project extents in this case.

        //Also use project boundary extents if fail to get production data extents or not doing production data tiles
        //e.g. project thumbnails
        var projectPoints = TileServiceUtils.GeometryToPoints(project.projectGeofenceWKT);
        var projectMinLat = projectPoints.Min(p => p.Latitude);
        var projectMinLng = projectPoints.Min(p => p.Longitude);
        var projectMaxLat = projectPoints.Max(p => p.Latitude);
        var projectMaxLng = projectPoints.Max(p => p.Longitude);
        bool assign = bbox == null
          ? true
          : bbox.minLat < projectMinLat || bbox.minLat > projectMaxLat ||
            bbox.maxLat < projectMinLat || bbox.maxLat > projectMaxLat ||
            bbox.minLng < projectMinLng || bbox.minLng > projectMaxLng ||
            bbox.maxLng < projectMinLng || bbox.minLng > projectMaxLng;

        if (assign)
        {
          bbox = new MapBoundingBox
          {
            minLat = projectMinLat,
            minLng = projectMinLng,
            maxLat = projectMaxLat,
            maxLng = projectMaxLng
          };
        }
      }

      return bbox;
    }

    /// <summary>
    /// Get the production data extents for the project.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    private CoordinateConversionResult GetProductionDataExtents(long projectId, Filter filter)
    {
      ProjectStatisticsRequest statsRequest = ProjectStatisticsRequest.CreateStatisticsParameters(projectId, filter?.SurveyedSurfaceExclusionList?.ToArray());
      var statsResult =
        RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(logger, raptorClient)
          .Process(statsRequest) as ProjectStatisticsResult;

      if (statsResult.Code == ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        var coordList = new List<TwoDConversionCoordinate>
        {
          TwoDConversionCoordinate.CreateTwoDConversionCoordinate(statsResult.extents.minX, statsResult.extents.minY),
          TwoDConversionCoordinate.CreateTwoDConversionCoordinate(statsResult.extents.maxX, statsResult.extents.maxY)
        };

        var coordRequest = CoordinateConversionRequest.CreateCoordinateConversionRequest(projectId,
          TwoDCoordinateConversionType.NorthEastToLatLon, coordList.ToArray());
        var coordResult = RequestExecutorContainerFactory.Build<CoordinateConversionExecutor>(logger, raptorClient)
          .Process(coordRequest) as CoordinateConversionResult;
        return coordResult;
      }
      return null;
    }

    /// <summary>
    /// Get the list of points representing any area filters in the filter.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="filter">The filter</param>
    /// <returns>A list of points</returns>
    private List<WGSPoint> GetFilterPoints(long projectId, Filter filter)
    {
      List<WGSPoint> points = new List<WGSPoint>();
      if (filter != null)
      {
        if (filter.PolygonLL != null && filter.PolygonLL.Count > 0)
        {
          points.AddRange(filter.PolygonLL);
        }
        if (filter.DesignOrAlignmentFile != null)
        {
          points.AddRange(GetDesignBoundaryPoints(projectId, filter.DesignOrAlignmentFile));
        }
      }
      return points;
    }

    private List<WGSPoint> GetDesignBoundaryPoints(long projectId, DesignDescriptor designDescriptor)
    {
      List<WGSPoint> points = new List<WGSPoint>();
      var geoJson = GetDesignBoundary(projectId, designDescriptor);
      if (!string.IsNullOrEmpty(geoJson))
      {
        var root = JsonConvert.DeserializeObject<RootObject>(geoJson);
        foreach (var feature in root.features)
        {
          foreach (var coordList in feature.geometry.coordinates)
          {
            foreach (var coordPair in coordList)
            {
              points.Add(WGSPoint.CreatePoint(coordPair[1], coordPair[0]));//GeoJSON is lng/lat
            }
          }
        }
      }
      return points;
    }

    /// <summary>
    /// Gets the boundary of the design surface as GeoJson
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="designDescriptor">The design to get the boundary for</param>
    /// <returns></returns>
    private string GetDesignBoundary(long projectId, DesignDescriptor designDescriptor)
    {
      MemoryStream memoryStream;
      TDesignProfilerRequestResult designProfilerResult;


      bool success = raptorClient.GetDesignBoundary(
        DesignProfiler.ComputeDesignBoundary.RPC.__Global.Construct_CalculateDesignBoundary_Args(
          projectId,
          RaptorConverters.DesignDescriptor(designDescriptor),
          DesignProfiler.ComputeDesignBoundary.RPC.TDesignBoundaryReturnType.dbrtJson,
          DesignBoundariesRequest.BOUNDARY_POINTS_INTERVAL,
          TVLPDDistanceUnits.vduMeters,
          0),
        out memoryStream,
        out designProfilerResult);

      if (success)
      {
        if (designProfilerResult == TDesignProfilerRequestResult.dppiOK && memoryStream != null && memoryStream.Length > 0)
        {
          memoryStream.Position = 0;
          var sr = new StreamReader(memoryStream);
          string geoJSONStr = sr.ReadToEnd();
          return geoJSONStr;
        }
      }
      else
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Failed to get design boundary for file: {designDescriptor.file.fileName}"));
      }
      return null;
    }

  }


  public interface IBoundingBoxService
  {
    MapBoundingBox GetBoundingBox(ProjectDescriptor project, Filter filter, DisplayMode? mode, Filter baseFilter, Filter topFilter);
  }
}
