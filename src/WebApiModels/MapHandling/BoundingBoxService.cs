﻿using System.Collections.Generic;
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
  /// <summary>
  /// Used to calculate the map bounding box for the report.
  /// </summary>
  public class BoundingBoxService : IBoundingBoxService
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
    /// <summary>
    /// Gets the map bounding box to use for the report.
    /// </summary>
    /// <param name="project">The project for the report</param>
    /// <param name="filter">The filter for production data tiles</param>
    /// <param name="haveProdDataOverlay">True if doing production data tiles</param>
    /// <param name="baseFilter">The base filter for summary volumes</param>
    /// <param name="topFilter">The top filter for summary volumes</param>
    /// <returns>A bounding box in latitude/longitude (degrees)</returns>
    public MapBoundingBox GetBoundingBox(ProjectDescriptor project, Filter filter, bool haveProdDataOverlay, Filter baseFilter, Filter topFilter)
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
          maxLng = filterPoints.Max(p => p.Lon)
        };
      }
      else
      {
        //No area filter so use production data extents as the bounding box
        //Only applies if doing production data tiles
        if (haveProdDataOverlay)
        {
          var productionDataExtents = GetProductionDataExtents(project.projectId, filter);
          if (productionDataExtents != null)
          {
            bbox = new MapBoundingBox
            {
              minLat = productionDataExtents.conversionCoordinates[0].y,
              minLng = productionDataExtents.conversionCoordinates[0].x,
              maxLat = productionDataExtents.conversionCoordinates[1].y,
              maxLng = productionDataExtents.conversionCoordinates[1].x
            };
          }
        }

        //Sometimes tag files way outside the project boundary are imported. These mean the data extents are
        //invalid and and give problems. So need to check for this and use project extents in this case.

        //Also use project boundary extents if fail to get production data extents or not doing production data tiles
        //e.g. project thumbnails
        var projectPoints = RaptorConverters.geometryToPoints(project.projectGeofenceWKT);
        var projectMinLat = projectPoints.Min(p => p.Lat);
        var projectMinLng = projectPoints.Min(p => p.Lon);
        var projectMaxLat = projectPoints.Max(p => p.Lat);
        var projectMaxLng = projectPoints.Max(p => p.Lon);
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
      ProjectStatisticsResult statsResult = null;
      try
      {
        ProjectStatisticsRequest statsRequest =
          ProjectStatisticsRequest.CreateStatisticsParameters(projectId,
            filter?.SurveyedSurfaceExclusionList?.ToArray());
        statsResult =
          RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(logger, raptorClient)
            .Process(statsRequest) as ProjectStatisticsResult;
      }
      catch (ServiceException se)
      {
        if (se.Code == HttpStatusCode.BadRequest && se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          return null;
        }
        throw;
      }

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

    /// <summary>
    /// Get the list of points representing any area filters in the filter.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="filter">The filter</param>
    /// <returns>A list of latitude/longitude points in degrees</returns>
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

    /// <summary>
    /// Gets a list of points representing the design surface boundary
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="designDescriptor">The design ro get the boundary of</param>
    /// <returns>A list of latitude/longitude points in degrees</returns>
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
              points.Add(WGSPoint.CreatePoint(coordPair[1].LatDegreesToRadians(), coordPair[0].LonDegreesToRadians()));//GeoJSON is lng/lat
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
    /// <returns>A GeoJSON representation of the design boundary</returns>
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
    MapBoundingBox GetBoundingBox(ProjectDescriptor project, Filter filter, bool haveProdDataOverlay, Filter baseFilter, Filter topFilter);
  }
}
