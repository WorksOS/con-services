using System;
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
using VSS.Productivity3D.WebApiModels.MapHandling;
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
    /// <param name="overlays">The overlay or layer types</param>
    /// <param name="baseFilter">The base filter for summary volumes</param>
    /// <param name="topFilter">The top filter for summary volumes</param>
    /// <returns>A bounding box in latitude/longitude (radians)</returns>
    public MapBoundingBox GetBoundingBox(ProjectDescriptor project, Filter filter, TileOverlayType[] overlays, Filter baseFilter, Filter topFilter)
    {
      log.LogInformation($"GetBoundingBox: project {project.projectUid}");

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
        log.LogDebug("GetBoundingBox: No area filter");
        //No area filter so use production data extents as the bounding box.
        //Only applies if doing production data tiles.
        //Also if doing the project boundary tile we assume the user wants to see that so production data extents not applicable.
        if (overlays.Contains(TileOverlayType.ProductionData) && !overlays.Contains(TileOverlayType.ProjectBoundary))
        {
          var productionDataExtents = GetProductionDataExtents(project.projectId, filter);
          if (productionDataExtents != null)
          {
            log.LogDebug($"GetBoundingBox: Production data extents {productionDataExtents.conversionCoordinates[0].y},{productionDataExtents.conversionCoordinates[0].x},{productionDataExtents.conversionCoordinates[1].y},{productionDataExtents.conversionCoordinates[1].x}");

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
        //e.g. project thumbnails or user has requested project boundary overlay
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
          log.LogDebug($"GetBoundingBox: Using project extents {projectMinLat},{projectMinLng},{projectMaxLat},{projectMaxLng}");

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
    /// Adjust the bounding box to fit the requested tile size.
    /// </summary>
    /// <param name="parameters">The map parameters icluding the bounding box.</param>
    public void AdjustBoundingBoxToFit(MapParameters parameters)
    {
      log.LogInformation($"AdjustBoundingBoxToFit: requestedWidth={parameters.mapWidth}, requestedHeight={parameters.mapHeight}, bbox={parameters.bbox}");

      Point pixelMin, pixelMax;
      int requiredWidth, requiredHeight;
      TryZoomIn(parameters, out requiredWidth, out requiredHeight, out pixelMin, out pixelMax);

      bool adjust = false;

      const double MARGIN_FACTOR = 1.1; //10% margin
      var adjustedRequiredWidth = parameters.addMargin ? (int)Math.Round(requiredWidth * MARGIN_FACTOR) : requiredWidth;
      var adjustedRequiredHeight = parameters.addMargin ? (int)Math.Round(requiredHeight * MARGIN_FACTOR) : requiredHeight;

      //Is it bigger or smaller than the requested size?
      if (adjustedRequiredWidth > parameters.mapWidth || adjustedRequiredHeight > parameters.mapHeight)
      {
        log.LogDebug("AdjustBoundingBoxToFit: scaling down");

        //We'll make the bounding box bigger in the same shape as the requested tile and scale down once the tile has been drawn. 
        adjust = true;
        //Need to maintain aspect ratio. Figure out the ratio.
        double ratioX = (double)requiredWidth / (double)parameters.mapWidth;
        double ratioY = (double)requiredHeight / (double)parameters.mapHeight;
        // use whichever multiplier is bigger
        double ratio = ratioX > ratioY ? ratioX : ratioY;

        // now we can get the new height and width
        var factor = parameters.addMargin ? MARGIN_FACTOR : 1.0; 
        int newHeight = Convert.ToInt32(parameters.mapHeight * ratio * factor);
        int newWidth = Convert.ToInt32(parameters.mapWidth * ratio * factor);

        var xDiff = Math.Abs(newWidth - requiredWidth) / 2;
        var yDiff = Math.Abs(newHeight - requiredHeight) / 2;

        //Pixel origin is top left
        pixelMin.x -= xDiff;
        pixelMax.x += xDiff;
        pixelMin.y += yDiff;
        pixelMax.y -= yDiff;

        //Adjust the tile width & height
        parameters.mapWidth = (int)Math.Abs(pixelMax.x - pixelMin.x);
        parameters.mapHeight = (int)Math.Abs(pixelMax.y - pixelMin.y);
      }
      else
      {
        log.LogDebug("AdjustBoundingBoxToFit: expanding to fill tile ");
  
        //Expand the bounding box to fill the requested tile size
        if (adjustedRequiredWidth < parameters.mapWidth)
        {
          double scaleWidth = (double)parameters.mapWidth / adjustedRequiredWidth;
          adjustedRequiredWidth = (int)(scaleWidth * adjustedRequiredWidth);
          double pixelCenterX = pixelMin.x + (pixelMax.x - pixelMin.x) / 2.0;
          //Pixel origin is top left
          pixelMin.x = pixelCenterX - adjustedRequiredWidth / 2.0;
          pixelMax.x = pixelCenterX + adjustedRequiredWidth / 2.0;
          adjust = true;
        }

        if (adjustedRequiredHeight < parameters.mapHeight)
        {
          double scaleHeight = (double)parameters.mapHeight / adjustedRequiredHeight;
          adjustedRequiredHeight = (int)(scaleHeight * adjustedRequiredHeight);
          double pixelCenterY = pixelMin.y + (pixelMax.y - pixelMin.y) / 2.0;
          //Pixel origin is top left
          pixelMin.y = pixelCenterY + adjustedRequiredHeight / 2.0;
          pixelMax.y = pixelCenterY - adjustedRequiredHeight / 2.0;
          adjust = true;
        }
      }

      if (adjust)
      {
        //Convert the adjusted bbox to lat/lng
        var minLatLngDegrees = WebMercatorProjection.PixelToLatLng(pixelMin, parameters.numTiles);
        var maxLatLngDegrees = WebMercatorProjection.PixelToLatLng(pixelMax, parameters.numTiles);
        parameters.bbox.minLat = minLatLngDegrees.Latitude.LatDegreesToRadians();
        parameters.bbox.maxLat = maxLatLngDegrees.Latitude.LatDegreesToRadians();
        parameters.bbox.minLng = minLatLngDegrees.Longitude.LonDegreesToRadians();
        parameters.bbox.maxLng = maxLatLngDegrees.Longitude.LonDegreesToRadians();
      }
      log.LogInformation($"AdjustBoundingBoxToFit: returning mapWidth={parameters.mapWidth}, mapHeight={parameters.mapHeight}, bbox={parameters.bbox}");
    }

    private void TryZoomIn(MapParameters parameters, out int requiredWidth, out int requiredHeight, out Point pixelMin, out Point pixelMax)
    {
      pixelMin = TileServiceUtils.LatLngToPixel(parameters.bbox.minLat, parameters.bbox.minLng, parameters.numTiles);
      pixelMax = TileServiceUtils.LatLngToPixel(parameters.bbox.maxLat, parameters.bbox.maxLng, parameters.numTiles);

      requiredWidth = (int)Math.Abs(pixelMax.x - pixelMin.x);
      requiredHeight = (int)Math.Abs(pixelMax.y - pixelMin.y);

      //See if we can zoom in - occurs when the requested tile size is much larger than the bbox
      var zoomedWidth = requiredWidth;
      var zoomedHeight = requiredHeight;
      int zoomLevel = parameters.zoomLevel;
      Point zoomedPixelMin = pixelMin;
      Point zoomedPixelMax = pixelMax;
      long numTiles = parameters.numTiles;

      while (zoomedWidth < parameters.mapWidth && zoomedHeight < parameters.mapHeight)
      {
        parameters.zoomLevel = zoomLevel;
        parameters.numTiles = numTiles;
        requiredWidth = zoomedWidth;
        requiredHeight = zoomedHeight;
        pixelMin = zoomedPixelMin;
        pixelMax = zoomedPixelMax;

        zoomLevel++;
        numTiles = TileServiceUtils.NumberOfTiles(zoomLevel);

        zoomedPixelMin = TileServiceUtils.LatLngToPixel(parameters.bbox.minLat, parameters.bbox.minLng, numTiles);
        zoomedPixelMax = TileServiceUtils.LatLngToPixel(parameters.bbox.maxLat, parameters.bbox.maxLng, numTiles);

        zoomedWidth = (int)Math.Abs(zoomedPixelMax.x - zoomedPixelMin.x);
        zoomedHeight = (int)Math.Abs(zoomedPixelMax.y - zoomedPixelMin.y);
      }
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
          log.LogDebug($"GetFilterPoints: adding polygon points for projectId={projectId}, filter name={filter.Name}");
          points.AddRange(filter.PolygonLL);
        }
        if (filter.DesignOrAlignmentFile != null)
        {
          log.LogDebug($"GetFilterPoints: adding design boundary points for projectId={projectId}, filter name={filter.Name}");
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
      var description = TileServiceUtils.DesignDescriptionForLogging(designDescriptor);
      log.LogDebug($"GetDesignBoundaryPoints: projectId={projectId}, design={description}");
      List<WGSPoint> points = new List<WGSPoint>();
      var geoJson = GetDesignBoundary(projectId, designDescriptor);
      log.LogDebug($"GetDesignBoundaryPoints: geoJson={geoJson}");
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
      MemoryStream memoryStream = null;
      try
      {        
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
          if (designProfilerResult == TDesignProfilerRequestResult.dppiOK && memoryStream != null &&
              memoryStream.Length > 0)
          {
            memoryStream.Position = 0;
            using (StreamReader sr = new StreamReader(memoryStream))
            {
              return sr.ReadToEnd();
            }
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
      finally
      {
        memoryStream?.Close();
      }
    }

  }


  public interface IBoundingBoxService
  {
    MapBoundingBox GetBoundingBox(ProjectDescriptor project, Filter filter, TileOverlayType[] overlays, Filter baseFilter, Filter topFilter);

    void AdjustBoundingBoxToFit(MapParameters parameters);
  }
}
