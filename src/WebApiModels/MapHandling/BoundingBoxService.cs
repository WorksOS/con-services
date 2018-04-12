using DesignProfilerDecls;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApiModels.Coord.Executors;
using VSS.Productivity3D.WebApiModels.Coord.Models;
using VSS.Productivity3D.WebApiModels.Coord.ResultHandling;
using WGSPoint = VSS.Productivity3D.Common.Models.WGSPoint;

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
    /// Get a list of all boundaries or polygons used by the filters. 
    /// For design boundaries there may be multiple polygons per design. 
    /// For custom boundaries and alignments there is at most one.
    /// </summary>
    /// <param name="project">The project for the report</param>
    /// <param name="filter">The filter for production data tiles</param>
    /// <param name="baseFilter">The base filter for summary volumes</param>
    /// <param name="topFilter">The top filter for summary volumes</param>
    /// <param name="boundaryType">Type of boundary to get: custom polygon or design boundaries or both</param>
    /// <returns>A list of boundaries (polygons). Points are latitude/longitude in degrees.</returns>
    public List<List<WGSPoint>> GetFilterBoundaries(ProjectData project, FilterResult filter,
                                                    FilterResult baseFilter, FilterResult topFilter, FilterBoundaryType boundaryType)
    {
      var boundaries = GetFilterBoundaries(project, filter, boundaryType);
      boundaries.AddRange(GetFilterBoundaries(project, baseFilter, boundaryType));
      boundaries.AddRange(GetFilterBoundaries(project, topFilter, boundaryType));
      return boundaries;
    }

    /// <summary>
    /// Get a list of boundaries or polygons used by the filter. 
    /// For design boundaries there may be multiple polygons per design. 
    /// For custom boundaries and alignments there is at most one.
    /// </summary>
    /// <param name="project">The project for the report</param>
    /// <param name="filter">The filter to get boundaries for</param>
    /// <param name="boundaryType">Type of boundary to get: custom polygon or design boundaries or both</param>
    /// <returns>A list of boundaries (polygons). Points are latitude/longitude in degrees.</returns>
    private List<List<WGSPoint>> GetFilterBoundaries(ProjectData project, FilterResult filter, FilterBoundaryType boundaryType)
    {
      var boundaries = new List<List<WGSPoint>>();
      if (filter != null)
      {
        if (boundaryType == FilterBoundaryType.Alignment || boundaryType == FilterBoundaryType.All)
        {
          if (filter.AlignmentFile != null)
          {
            this.log.LogDebug($"GetFilterBoundaries: adding design boundary polygons for projectId={project.LegacyProjectId}, filter name={filter.Name}");
            boundaries.Add(GetAlignmentPoints(project.LegacyProjectId, filter.AlignmentFile,
              filter.StartStation ?? 0, filter.EndStation ?? 0, filter.LeftOffset ?? 0, filter.RightOffset ?? 0).ToList());
          }
        }
        if (boundaryType == FilterBoundaryType.Design || boundaryType == FilterBoundaryType.All)
        {
          if (filter.DesignFile != null)
          {
            log.LogDebug($"GetFilterBoundaries: adding design boundary polygons for projectId={project.LegacyProjectId}, filter name={filter.Name}");
            boundaries.AddRange(GetDesignBoundaryPolygons(project.LegacyProjectId, filter.DesignFile));
          }
        }
        if (boundaryType == FilterBoundaryType.Polygon || boundaryType == FilterBoundaryType.All)
        {
          if (filter.PolygonLL != null && filter.PolygonLL.Count > 0)
          {
            log.LogDebug($"GetFilterBoundaries: adding custom polygon for projectId={project.LegacyProjectId}, filter name={filter.Name}");
            boundaries.Add(filter.PolygonLL);
          }
        }
      }
      return boundaries;
    }


    /// <summary>
    /// Gets a single list of points representing all the spatial filters in the given filters 
    /// and the design boundary for cut-fill and volumes.
    /// </summary>
    /// <param name="project">The project for the report</param>
    /// <param name="filter">The filter for production data tiles</param>
    /// <param name="baseFilter">The base filter for summary volumes</param>
    /// <param name="topFilter">The top filter for summary volumes</param>
    /// <returns>A list of latitude/longitude points in degrees</returns>
    private List<WGSPoint> GetFilterPoints(ProjectData project, FilterResult filter, FilterResult baseFilter, FilterResult topFilter)
    {
      var boundaries = GetFilterBoundaries(project, filter, baseFilter, topFilter, FilterBoundaryType.All);
      return GetPointsFromPolygons(boundaries);
    }

    /// <summary>
    /// Gets a single list containing all the points from a list of polygons
    /// </summary>
    /// <param name="polygons"></param>
    /// <returns></returns>
    private List<WGSPoint> GetPointsFromPolygons(List<List<WGSPoint>> polygons)
    {
      var points = new List<WGSPoint>();
      foreach (var polygon in polygons)
      {
        points.AddRange(polygon);
      }
      return points;
    }

    /// <summary>
    /// Gets the map bounding box to use for the report.
    /// </summary>
    /// <param name="project">The project for the report</param>
    /// <param name="filter">The filter for production data tiles</param>
    /// <param name="overlays">The overlay or layer types</param>
    /// <param name="baseFilter">The base filter for summary volumes</param>
    /// <param name="topFilter">The top filter for summary volumes</param>
    /// <param name="designDescriptor">The design for cut-fill & summary volumes</param>
    /// <returns>A bounding box in latitude/longitude (radians)</returns>
    public MapBoundingBox GetBoundingBox(ProjectData project, FilterResult filter, TileOverlayType[] overlays, FilterResult baseFilter, FilterResult topFilter, DesignDescriptor designDescriptor)
    {
      log.LogInformation($"GetBoundingBox: project {project.ProjectUid}");

      MapBoundingBox bbox = null;

      //If the filter has an area then use it as the bounding box
      List<WGSPoint> filterPoints = GetFilterPoints(project, filter, baseFilter, topFilter);
      if (filterPoints.Count > 0)
      {
        log.LogDebug("GetBoundingBox: Using area filter");

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
        //If no spatial filter then use cut-fill/volumes design
        var boundaryPoints = GetPointsFromPolygons(GetDesignBoundaryPolygons(project.LegacyProjectId, designDescriptor));
        if (boundaryPoints.Count > 0)
        {
          log.LogDebug("GetBoundingBox: Using cut-fill design boundary");
          bbox = new MapBoundingBox
          {
            minLat = boundaryPoints.Min(p => p.Lat),
            minLng = boundaryPoints.Min(p => p.Lon),
            maxLat = boundaryPoints.Max(p => p.Lat),
            maxLng = boundaryPoints.Max(p => p.Lon)
          };
        }
        else
        {
          log.LogDebug("GetBoundingBox: No spatial filter");
          //No area filter so use production data extents as the bounding box.
          //Only applies if doing production data tiles.
          //Also if doing the project boundary tile we assume the user wants to see that so production data extents not applicable.
          if (overlays.Contains(TileOverlayType.ProductionData) && !overlays.Contains(TileOverlayType.ProjectBoundary))
          {
            var productionDataExtents = GetProductionDataExtents(project.LegacyProjectId, filter);
            if (productionDataExtents != null)
            {
              log.LogDebug(
                $"GetBoundingBox: Production data extents {productionDataExtents.conversionCoordinates[0].y},{productionDataExtents.conversionCoordinates[0].x},{productionDataExtents.conversionCoordinates[1].y},{productionDataExtents.conversionCoordinates[1].x}");

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
          var projectPoints = RaptorConverters.geometryToPoints(project.ProjectGeofenceWKT).ToList();
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
            log.LogDebug(
              $"GetBoundingBox: Using project extents {projectMinLat},{projectMinLng},{projectMaxLat},{projectMaxLng}");

            bbox = new MapBoundingBox
            {
              minLat = projectMinLat,
              minLng = projectMinLng,
              maxLat = projectMaxLat,
              maxLng = projectMaxLng
            };
          }
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

      TryZoomIn(parameters, out int requiredWidth, out int requiredHeight, out var pixelMin, out var pixelMax);

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
    private CoordinateConversionResult GetProductionDataExtents(long projectId, FilterResult filter)
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
        TwoDConversionCoordinate.CreateTwoDConversionCoordinate(statsResult.extents.MinX, statsResult.extents.MinY),
        TwoDConversionCoordinate.CreateTwoDConversionCoordinate(statsResult.extents.MaxX, statsResult.extents.MaxY)
      };

      var coordRequest = CoordinateConversionRequest.CreateCoordinateConversionRequest(projectId,
        TwoDCoordinateConversionType.NorthEastToLatLon, coordList.ToArray());
      var coordResult = RequestExecutorContainerFactory.Build<CoordinateConversionExecutor>(logger, raptorClient)
        .Process(coordRequest) as CoordinateConversionResult;
      return coordResult;   
    }

 
    /// <summary>
    /// Gets a list of polygons representing the design surface boundary. 
    /// The boundary may consist of a number of polygons.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="designDescriptor">The design to get the boundary of</param>
    /// <returns>A list of latitude/longitude points in degrees</returns>
    public List<List<WGSPoint>> GetDesignBoundaryPolygons(long projectId, DesignDescriptor designDescriptor)
    {
      var polygons = new List<List<WGSPoint>>();
      var description = TileServiceUtils.DesignDescriptionForLogging(designDescriptor);
      log.LogDebug($"GetDesignBoundaryPolygons: projectId={projectId}, design={description}");
      if (designDescriptor == null) return polygons;
      var geoJson = GetDesignBoundary(projectId, designDescriptor);
      log.LogDebug($"GetDesignBoundaryPolygons: geoJson={geoJson}");
      if (string.IsNullOrEmpty(geoJson)) return polygons;
      var root = JsonConvert.DeserializeObject<RootObject>(geoJson);
      foreach (var feature in root.features)
      {
        var points = new List<WGSPoint>();
        foreach (var coordList in feature.geometry.coordinates)
        {
          foreach (var coordPair in coordList)
          {
            points.Add(WGSPoint.CreatePoint(coordPair[1].LatDegreesToRadians(),
              coordPair[0].LonDegreesToRadians())); //GeoJSON is lng/lat
          }
        }
        polygons.Add(points);
      }
      return polygons;
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
        bool success = raptorClient.GetDesignBoundary(
          DesignProfiler.ComputeDesignBoundary.RPC.__Global.Construct_CalculateDesignBoundary_Args(
            projectId,
            RaptorConverters.DesignDescriptor(designDescriptor),
            DesignProfiler.ComputeDesignBoundary.RPC.TDesignBoundaryReturnType.dbrtJson,
            DesignBoundariesRequest.BOUNDARY_POINTS_INTERVAL,
            TVLPDDistanceUnits.vduMeters,
            0),
          out memoryStream,
          out var designProfilerResult);

        if (success)
        {
          if (designProfilerResult == TDesignProfilerRequestResult.dppiOK && memoryStream != null &&
              memoryStream.Length > 0)
          {
            memoryStream.Position = 0;
            using (StreamReader sr = new StreamReader(memoryStream))
            {
              var resultPolygon = sr.ReadToEnd();
              log.LogDebug($"Design file {JsonConvert.SerializeObject(designDescriptor)} generated bounary {resultPolygon}");
              return resultPolygon;
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

    /// <summary>
    /// Gets the list of points making up the alignment boundary. 
    /// If the start & end station and left & right offsets are zero,
    /// then gets the centerline of the alignment.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="alignDescriptor">Design descriptor for the alignment file</param>
    /// <param name="startStation">Start station for the alignment file boundary</param>
    /// <param name="endStation">End station for the alignment file boundary</param>
    /// <param name="leftOffset">Left offset for the alignment file boundary</param>
    /// <param name="rightOffset">Right offset for the alignment file boundary</param>
    /// <returns>A list of latitude/longitude points in degrees</returns>
    public IEnumerable<WGSPoint> GetAlignmentPoints(long projectId, DesignDescriptor alignDescriptor,
      double startStation=0, double endStation=0, double leftOffset=0, double rightOffset=0)
    {
      var description = TileServiceUtils.DesignDescriptionForLogging(alignDescriptor);
      log.LogDebug($"GetAlignmentPoints: projectId={projectId}, alignment={description}");
      List<WGSPoint> alignmentPoints = null;
      if (alignDescriptor != null)
      {
        bool success = true;
        bool isCenterline = startStation == 0 && endStation == 0 &&
                            leftOffset == 0 && rightOffset == 0;
        if (isCenterline)
        {
          try
          {
            var stationRange = GetAlignmentStationRange(projectId, alignDescriptor);
            startStation = stationRange.StartStation;
            endStation = stationRange.EndStation;
          }
          catch
          {
            success = false;
          }
        }
        if (success)
        {
          log.LogDebug($"GetAlignmentPoints: projectId={projectId}, station range={startStation}-{endStation}");

          TVLPDDesignDescriptor alignmentDescriptor = RaptorConverters.DesignDescriptor(alignDescriptor);

          success = raptorClient.GetDesignFilterBoundaryAsPolygon(
            DesignProfiler.ComputeDesignFilterBoundary.RPC.__Global.Construct_CalculateDesignFilterBoundary_Args(
              projectId,
              alignmentDescriptor,
              startStation, endStation, leftOffset, rightOffset,
              DesignProfiler.ComputeDesignFilterBoundary.RPC.TDesignFilterBoundaryReturnType.dfbrtList), out TWGS84Point[] pdsPoints);

          if (success && pdsPoints != null && pdsPoints.Length > 0)
          {
            log.LogDebug($"GetAlignmentPoints success: projectId={projectId}, number of points={pdsPoints.Length}");

            alignmentPoints = new List<WGSPoint>();
            //For centerline, we only need half the points as normally GetDesignFilterBoundaryAsPolygon 
            //has offsets so is returning a polygon.
            //Since we have no offsets we have the centreline twice.
            int count = isCenterline ? pdsPoints.Length / 2 : pdsPoints.Length;
            for (int i = 0; i < count; i++)
            {
              alignmentPoints.Add(WGSPoint.CreatePoint(pdsPoints[i].Lat, pdsPoints[i].Lon));
            }
          }
        }
      }
      return alignmentPoints;
    }

    /// <summary>
    /// Get the station range for the alignment file
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="alignDescriptor"></param>
    /// <returns>The statio range</returns>
    public AlignmentStationResult GetAlignmentStationRange(long projectId, DesignDescriptor alignDescriptor)
    {
      if (alignDescriptor == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
          ContractExecutionStatesEnum.FailedToGetResults,
          "Invalid request - Missing alignment file"));
      }

      AlignmentStationResult result = null;
      var description = TileServiceUtils.DesignDescriptionForLogging(alignDescriptor);
      log.LogDebug($"GetAlignmentStationRange: projectId={projectId}, alignment={description}");
 
      //Get the station extents
      TVLPDDesignDescriptor alignmentDescriptor = RaptorConverters.DesignDescriptor(alignDescriptor);
      bool success = raptorClient.GetStationExtents(projectId, alignmentDescriptor,
        out double startStation, out double endStation);
      if (success)
      {
        result = AlignmentStationResult.CreateAlignmentOffsetResult(startStation, endStation);
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
          ContractExecutionStatesEnum.FailedToGetResults,
          "Failed to get station range for alignment file"));
      }

      return result;
    }

  }


  public interface IBoundingBoxService
  {
    MapBoundingBox GetBoundingBox(ProjectData project, FilterResult filter, TileOverlayType[] overlays, FilterResult baseFilter, FilterResult topFilter, DesignDescriptor designDescriptor);

    void AdjustBoundingBoxToFit(MapParameters parameters);

    List<List<WGSPoint>> GetFilterBoundaries(ProjectData project, FilterResult filter,
                                             FilterResult baseFilter, FilterResult topFilter, FilterBoundaryType boundaryType);

    IEnumerable<WGSPoint> GetAlignmentPoints(long projectId, DesignDescriptor alignDescriptor,
      double startStation=0, double endStation=0, double leftOffset=0, double rightOffset=0);

    List<List<WGSPoint>> GetDesignBoundaryPolygons(long projectId, DesignDescriptor designDescriptor);

    AlignmentStationResult GetAlignmentStationRange(long projectId, DesignDescriptor alignDescriptor);
  }

  public enum FilterBoundaryType
  {
    All,
    Polygon,
    Design,
    Alignment
  }
}
