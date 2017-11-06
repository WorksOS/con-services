using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Executors;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Helpers;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Point = VSS.Productivity3D.WebApi.Models.MapHandling.Point;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  //The class supports overlaying bitmaps from various sources. ALK map tile provider is used as a datasource
  public class MapTileGenerator : IMapTileGenerator
  {
    private readonly IConfigurationStore config;
    private readonly IFileRepository tccFileRepository;
    private readonly IProductionDataRequestFactory requestFactory;
    private readonly ILogger log;
    private readonly ILoggerFactory logger;
    private readonly IASNodeClient raptorClient;
    private readonly IElevationExtentsProxy elevProxy;

    private readonly string alkKey;
    private readonly string tccFilespaceId;

    //public int MapWidth => 128;
    //public int MapHeight => 128;
    //public string Locale => "EN";

    //public long ProjectId { get; private set; }
    //public long CustomerId { get; private set; }

    public MapTileGenerator(IConfigurationStore configuration, IFileRepository tccRepository,
      IProductionDataRequestFactory prodDataFactory, ILoggerFactory logger, IASNodeClient raptor, IElevationExtentsProxy extentsProxy)
    {
      config = configuration;
      tccFileRepository = tccRepository;
      requestFactory = prodDataFactory;
      log = logger.CreateLogger<MapTileGenerator>();
      this.logger = logger;
      raptorClient = raptor;
      elevProxy = extentsProxy;
      alkKey = config.GetValueString("ALK_KEY");
      tccFilespaceId = config.GetValueString("TCCFILESPACEID");
    }

    public class MapParameters
    {
      public BoundingBox2DLatLon bbox;
      public int zoomLevel;
      public int numTiles;
      public MapType mapType;
      public Point pixelTopLeft;
      public int mapWidth;
      public int mapHeight;
      public DisplayMode mode;
      public string locale;
    }

    public byte[] GetMapBitmap(MapParameters parameters)
    {
      const int SATELLITE = 2;
      const int HYBRID = 3;
      const int TERRAIN = 4;

      string mapURL = null;

      string alkMapType;
      switch (parameters.mapType)
      {
        case MapType.SATELLITE:
          alkMapType = "satellite";
          break;
        case MapType.HYBRID:
          alkMapType = "satellite";
          break;
        case MapType.TERRAIN:
          alkMapType = "terrain";
          break;
        default:
          alkMapType = "default";
          break;
      }

      //see http://pcmiler.alk.com/APIs/REST/v1.0/Service.svc/help/operations/DrawMap

      var region = GetRegion(parameters.bbox.centerLatInDecimalDegrees, parameters.bbox.centerLngInDecimalDegrees);
      var dataset = "PCM_" + region; //"current";
      var mapLayers = "Cities,Labels,Roads,Commercial,Borders,Areas";
      var baseUrl = "http://pcmiler.alk.com/APIs/REST/v1.0/Service.svc/map";
      mapURL =
        $"{baseUrl}?AuthToken={alkKey}&pt1={parameters.bbox.minLngInDecimalDegrees:F6},{parameters.bbox.minLatInDecimalDegrees:F6}&pt2={parameters.bbox.maxLngInDecimalDegrees:F6},{parameters.bbox.maxLatInDecimalDegrees:F6}&width={parameters.mapWidth}&height={parameters.mapHeight}&drawergroups={mapLayers}&style={alkMapType}&srs=EPSG:900913&region={region}&dataset={dataset}&language={parameters.locale}&imgSrc=Sat1";
      if (parameters.mapType == MapType.SATELLITE)
      {
        mapURL += "&imgOption=BACKGROUND";
      }
      byte[] mapImage = null;
      using (WebClient wc = new WebClient())
      using (Stream stream = wc.OpenRead(mapURL))
      using (var ms = new MemoryStream())
      {
        stream.CopyTo(ms);
        mapImage = ms.ToArray();
        ms.Close();
        stream.Close();
      }
      return mapImage;
    }

    /// <summary>
    /// Gets the ALK region.
    /// </summary>
    /// <param name="lat">The lat.</param>
    /// <param name="lng">The Lng.</param>
    /// <returns></returns>
    public string GetRegion(double lat, double lng)
    {
      //Note: the first "EU" is used for WW map
      string[] REGIONS = {"EU", "AF", "AS", "EU", "NA", "OC", "SA", "ME"};

      string geocodeUrl =
        $"http://pcmiler.alk.com/APIs/REST/v1.0/Service.svc/locations?coords={lng},{lat}&AuthToken={alkKey}";
      using (var client = new WebClient())
      {
        string jsonresult = client.DownloadString(geocodeUrl);
        var serializer = new JavaScriptSerializer();
        var jsonObject = serializer.Deserialize<dynamic>(jsonresult);
        /* 
         Example result
         [{"Address":{"StreetAddress":"Christchurch Southern Motorway","City":"Christchurch","State":"NZ","Zip":"8024","County":"Christchurch City","Country":"New Zealand","SPLC":null,"CountryPostalFilter":0,"AbbreviationFormat":0},"Coords":{"Lat":"-43.545639","Lon":"172.583091"},"Region":5,"Label":"","PlaceName":"","TimeZone":"+13:0","Errors":[]}]
         */
        var result = jsonObject[0];
        if (result["Region"] is int region)
          return REGIONS[region];
      }
      return "EU";
    }

    private byte[] GetProjectBitmap(MapParameters parameters, ProjectData project)
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
          var projectPoints = GeometryToPoints(project.ProjectGeofenceWKT);
          PointF[] pixelPoints = LatLngToPixelOffset(projectPoints, parameters.pixelTopLeft, parameters.numTiles);
 
          Pen pen = new Pen(Color.FromArgb(STROKE_TRANSPARENCY, Color.FromArgb(PROJECT_BOUNDARY_COLOR)), PROJECT_OUTLINE_WIDTH);
          g.DrawPolygon(pen, pixelPoints);
          
          projectImage = bitmap.BitmapToByteArray();
        }
      }

      return projectImage;
    }

    private byte[] GetSitesBitmap(MapParameters parameters, IEnumerable<GeofenceData> sites)
    {
      const int DEFAULT_SITE_COLOR = 0x0055FF;
      const int FILL_TRANSPARENCY = 0x40; //0.25 of FF
      const int STROKE_TRANSPARENCY = 0x73; //0.45 of FF
      const int SITE_OUTLINE_WIDTH = 2;

      // Exclude sites that are too small to be displayed in the current viewport. 
      double viewPortArea = Math.Abs(parameters.bbox.minLatInDecimalDegrees - parameters.bbox.maxLatInDecimalDegrees) * Math.Abs(parameters.bbox.minLngInDecimalDegrees - parameters.bbox.maxLngInDecimalDegrees);
      double minArea = viewPortArea / 10000;
      //TODO: AreaSqMeters should be in GeoFence model but not our MasterDataModel GeofenceData.
      //Need to update that and then select from 'sites' where site.area > minArea

      byte[] sitesImage = null;

      if (sites.Any())
      {
        using (Bitmap bitmap = new Bitmap(parameters.mapWidth, parameters.mapHeight))
        using (Graphics g = Graphics.FromImage(bitmap))
        {
          foreach (var site in sites)
          {
            var sitePoints = GeometryToPoints(site.GeometryWKT);

            //Exclude site if outside bbox
            bool outside = false;
            foreach (var sitePoint in sitePoints)
            {
              if (sitePoint.Latitude < parameters.bbox.minLatInDecimalDegrees || sitePoint.Latitude > parameters.bbox.maxLatInDecimalDegrees ||
                  sitePoint.Longitude < parameters.bbox.minLngInDecimalDegrees || sitePoint.Longitude > parameters.bbox.maxLngInDecimalDegrees)
                outside = true;
              break;
            }

            if (!outside)
            {
              PointF[] pixelPoints = LatLngToPixelOffset(sitePoints, parameters.pixelTopLeft, parameters.numTiles);
              int siteColor = site.FillColor > 0 ? site.FillColor : DEFAULT_SITE_COLOR;
              if (!site.IsTransparent)
              {
                Brush brush = new SolidBrush(Color.FromArgb(FILL_TRANSPARENCY, Color.FromArgb(siteColor)));
                g.FillPolygon(brush, pixelPoints, FillMode.Alternate);
              }
              Pen pen = new Pen(Color.FromArgb(STROKE_TRANSPARENCY, Color.FromArgb(siteColor)), SITE_OUTLINE_WIDTH);
              g.DrawPolygon(pen, pixelPoints);
            }
          }

          sitesImage = bitmap.BitmapToByteArray();
        }
      }
      
      return sitesImage;
    }

    private byte[] GetAlignmentsBitmap(MapParameters parameters, IEnumerable<DesignDescriptor> alignmentDescriptors)
    {
      byte[] alignmentsImage = null;
      if (alignmentDescriptors.Any())
      {
        using (Bitmap bitmap = new Bitmap(parameters.mapWidth, parameters.mapHeight))
        using (Graphics g = Graphics.FromImage(bitmap))
        {
          foreach (var alignmentDescriptor in alignmentDescriptors)
          {
            IEnumerable<Point> alignmentPoints = GetAlignmentPoints(alignmentDescriptor);

            if (alignmentPoints.Any())
            {              
              PointF[] pixelPoints = LatLngToPixelOffset(alignmentPoints, parameters.pixelTopLeft, parameters.numTiles);
              Pen pen = new Pen(Color.Red, 1);
              g.DrawLines(pen, pixelPoints);
            }
          }
          alignmentsImage = bitmap.BitmapToByteArray();
        }
      }
      return alignmentsImage;
    }

    private async Task<byte[]> GetDxfBitmap(MapParameters parameters, IEnumerable<FileData> dxfFiles)
    {
      byte[] overlayData = null;

      if (dxfFiles.Any())
      {
        List<byte[]> tileList = new List<byte[]>();
        foreach (var dxfFile in dxfFiles)
        {
          if (dxfFile.ImportedFileType == ImportedFileType.Linework)
          {
            tileList.Add(await JoinDxfTiles(parameters, dxfFile.Name));
          }
        }
        overlayData = OverlayTiles(parameters, tileList);
      }
      return overlayData;
    }

    /// <summary>
    /// Joins standard size DXF tiles together to form one large tile for the report
    /// </summary>
    private async Task<byte[]> JoinDxfTiles(MapParameters parameters, string dxfFileName)
    {
      //Find the tiles that the bounding box fits into.
      Point tileTopLeft = WebMercatorProjection.PixelToTile(parameters.pixelTopLeft);
      Point tileBottomRight = WebMercatorProjection.LatLngToTile(new Point(parameters.bbox.minLatInDecimalDegrees, parameters.bbox.maxLngInDecimalDegrees), parameters.numTiles);

      int xnumTiles = (int)(tileBottomRight.x - tileTopLeft.x) + 1;
      int ynumTiles = (int)(tileBottomRight.y - tileTopLeft.y) + 1;
      int width = xnumTiles * WebMercatorProjection.TILE_SIZE;
      int height = ynumTiles * WebMercatorProjection.TILE_SIZE;

      using (Bitmap tileBitmap = new Bitmap(width, height))
      using (Graphics g = Graphics.FromImage(tileBitmap))
      {
        //Find the offset of the bounding box top left point inside the top left tile
        var point = new Point
        {
          x = tileTopLeft.x * WebMercatorProjection.TILE_SIZE,
          y = tileTopLeft.y * WebMercatorProjection.TILE_SIZE
        };
        //Clip to the actual bounding box within the tiles
        int xClipTopLeft = (int)(parameters.pixelTopLeft.x - point.x);
        int yClipTopLeft = (int)(parameters.pixelTopLeft.y - point.y);
        Rectangle clipRect = new Rectangle(xClipTopLeft, yClipTopLeft, parameters.mapWidth, parameters.mapHeight);
        g.SetClip(clipRect);

        string zoomFolder = FileUtils.ZoomFolder(parameters.zoomLevel);
        string zoomPath = $"{FileUtils.FilePath(CustomerId, ProjectId)}/{FileUtils.TilesFolderWithSuffix(dxfFileName)}/{zoomFolder}";
        for (int yTile = (int)tileTopLeft.y; yTile <= (int)tileBottomRight.y; yTile++)
        {
          string yFolder = yTile.ToString();
          //TCC only renders tiles where there is DXF data. So check if any tiles for this y.
          if (await tccFileRepository.FolderExists(tccFilespaceId, $"{zoomPath}/{yFolder}"))
          {
            string targetFolder = $"{zoomPath}/{yFolder}";
            for (int xTile = (int)tileTopLeft.x; xTile <= (int)tileBottomRight.x; xTile++)
            {
              string targetFile = $"{xTile}.png";
              if (await tccFileRepository.FileExists(tccFilespaceId, $"{targetFolder}/{targetFile}"))
              {
                var file = await tccFileRepository.GetFile(tccFilespaceId, $"{targetFolder}/{targetFile}");
                Image tile = Image.FromStream(file);

                System.Drawing.Point offset = new System.Drawing.Point(
                  (xTile - (int)tileTopLeft.x) * WebMercatorProjection.TILE_SIZE,
                  (yTile - (int)tileTopLeft.y) * WebMercatorProjection.TILE_SIZE);
                g.DrawImage(tile, offset);
              }
            }
          }
        }

        using (Bitmap clipBitmap = new Bitmap(parameters.mapWidth, parameters.mapHeight))
        using (Graphics clipGraphics = Graphics.FromImage(clipBitmap))
        {
          clipGraphics.DrawImage(tileBitmap, 0, 0, clipRect, GraphicsUnit.Pixel);
          return clipBitmap.BitmapToByteArray();
        }
      }
    }

    private byte[] OverlayTiles(MapParameters parameters, IEnumerable<byte[]> tileList)
    {
      byte[] overlayData = null;

      //Overlay the tiles. Return an empty tile if none to overlay.
      System.Drawing.Point origin = new System.Drawing.Point(0, 0);
      using (Bitmap bitmap = new Bitmap(parameters.mapWidth, parameters.mapHeight))
      using (Graphics g = Graphics.FromImage(bitmap))
      {
        foreach (byte[] tileData in tileList)
        {
          if (tileData != null)
          {
            using (var tileStream = new MemoryStream(tileData))
            {
              Image image = Image.FromStream(tileStream);
              g.DrawImage(image, origin);
            }
          }
        }
        overlayData = bitmap.BitmapToByteArray();
      }

      return overlayData;
    }

    private IEnumerable<Point> GeometryToPoints(string geometry)
    {
      List<Point> latlngs = new List<Point>();
      //Trim off the "POLYGON((" and "))"
      geometry = geometry.Substring(9, geometry.Length - 11);
      var points = geometry.Split(',');
      foreach (var point in points)
      {
        var parts = point.Trim().Split(' ');
        var lng = double.Parse(parts[0]);
        var lat = double.Parse(parts[1]);
        latlngs.Add(new Point{ y = lat * ConversionConstants.DEGREES_TO_RADIANS, x = lng * ConversionConstants.DEGREES_TO_RADIANS });
      }
      return latlngs;
    }

    private PointF[] LatLngToPixelOffset(IEnumerable<Point> latLngs, Point pixelTopLeft, int numTiles)
    {
      List<PointF> pixelPoints = new List<PointF>();
      foreach (Point ll in latLngs)
      {
        Point pixelPt = WebMercatorProjection.LatLngToPixel(ll, numTiles);
        pixelPoints.Add(new PointF((float)(pixelPt.x - pixelTopLeft.x), (float)(pixelPt.y - pixelTopLeft.y)));
      }
      return pixelPoints.ToArray();
    }

    private void CalculateWidthHeight(BoundingBox2DLatLon bbox, int numTiles, out int mapWidth, out int mapHeight)
    {
      Point minLatLng = new Point(bbox.minLatInDecimalDegrees, bbox.minLngInDecimalDegrees);
      Point maxLatLng = new Point(bbox.maxLatInDecimalDegrees, bbox.maxLngInDecimalDegrees);
      Point pixelMin = WebMercatorProjection.LatLngToPixel(minLatLng, numTiles);
      Point pixelMax = WebMercatorProjection.LatLngToPixel(maxLatLng, numTiles);

      mapWidth = (int) Math.Abs(pixelMax.x - pixelMin.x);
      mapHeight = (int) Math.Abs(pixelMax.y - pixelMin.y);
    }


    private IEnumerable<Point> GetAlignmentPoints(DesignDescriptor alignDescriptor)
    {
      List<Point> alignmentPoints = null;
      if (alignDescriptor != null)
      {
        //Get the station extents
        TVLPDDesignDescriptor alignmentDescriptor = RaptorConverters.DesignDescriptor(alignDescriptor);
        double startStation = 0;
        double endStation = 0;
        bool success = raptorClient.GetStationExtents(ProjectId, alignmentDescriptor,
          out startStation, out endStation);
        if (success)
        {
          //Get the alignment points
          TWGS84Point[] pdsPoints = null;

          success = raptorClient.GetDesignFilterBoundaryAsPolygon(
            DesignProfiler.ComputeDesignFilterBoundary.RPC.__Global.Construct_CalculateDesignFilterBoundary_Args(
              ProjectId,
              alignmentDescriptor,
              startStation, endStation, 0, 0,
              DesignProfiler.ComputeDesignFilterBoundary.RPC.TDesignFilterBoundaryReturnType.dfbrtList), out pdsPoints);

          if (success && pdsPoints != null && pdsPoints.Length > 0)
          {
            alignmentPoints = new List<Point>();
            //We only need half the points as normally GetDesignFilterBoundaryAsPolygon has offsets so is returning a polygon.
            //Since we have no offsets we have the centreline twice.
            int count = pdsPoints.Length / 2;
            for (int i = 0; i < count; i++)
            {
              alignmentPoints.Add(new Point(pdsPoints[i].Lat.latRadiansToDegrees(), pdsPoints[i].Lon.lonRadiansToDegrees()));
            }
          }  
        }       
      }
      return alignmentPoints;
    }

    protected async Task<byte[]> GetMapData(MapType mapType, ProjectData project, IEnumerable<GeofenceData> geofences, IEnumerable<DesignDescriptor> alignmentDescriptors,
      IEnumerable<FileData> dxfFiles, string language,
      CompactionProjectSettings projectSettings, Productivity3D.Common.Models.Filter filter, long projectId, DisplayMode mode, ushort width, ushort height,
      BoundingBox2DLatLon bbox, DesignDescriptor cutFillDesign, IDictionary<string, string> customHeaders)
    {
      List<byte[]> tileList = new List<byte[]>();

      int zoomLevel = bbox.CalculateZoomLevel();
      int numTiles = 1 << zoomLevel; //equivalent to 2 to the power of zoomLevel
      int mapWidth, mapHeight;
      CalculateWidthHeight(bbox, numTiles, out mapWidth, out mapHeight);
      //TODO: Calculate width/height or use passed in parameters????
      var pixelTopLeft = WebMercatorProjection.LatLngToPixel(new Point(bbox.maxLatInDecimalDegrees, bbox.minLngInDecimalDegrees), numTiles);

      MapParameters parameters = new MapParameters
      {
        mode = mode,
        bbox = bbox,
        zoomLevel = zoomLevel,
        numTiles = numTiles,
        mapWidth = mapWidth,
        mapHeight = mapHeight,
        mapType = mapType,
        pixelTopLeft = pixelTopLeft,
        locale = language.Substring(0,2)
      };

      tileList.Add(GetMapBitmap(parameters));
      var tileResult = GetProductionDataTile(projectSettings, filter, projectId, mode, width, height, bbox,
        cutFillDesign, customHeaders);
      tileList.Add(tileResult.TileData);
      tileList.Add(GetProjectBitmap(parameters, project));
      tileList.Add(GetSitesBitmap(parameters, geofences));
      tileList.Add(GetAlignmentsBitmap(parameters, alignmentDescriptors));
      tileList.Add(await GetDxfBitmap(parameters, dxfFiles));

      return OverlayTiles(parameters, tileList);
    }

    /// <summary>
    /// Gets the requested tile from Raptor
    /// </summary>
    /// <param name="projectSettings">Project settings to use for Raptor</param>
    /// <param name="filter">Filter to use for Raptor</param>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="mode">Display mode; type of data requested</param>
    /// <param name="width">Width of the tile</param>
    /// <param name="height">Height of the tile in pixels</param>
    /// <param name="bbox">Bounding box in radians</param>
    /// <param name="cutFillDesign">Design descriptor for cut-fill design</param>
    /// <returns>Tile result</returns>
    public TileResult GetProductionDataTile(CompactionProjectSettings projectSettings, Productivity3D.Common.Models.Filter filter, long projectId, DisplayMode mode, ushort width, ushort height,
      BoundingBox2DLatLon bbox, DesignDescriptor cutFillDesign, IDictionary<string,string> customHeaders)
    {
      var tileRequest = requestFactory.Create<TileRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(customHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter)
          .DesignDescriptor(cutFillDesign))
        .CreateTileRequest(mode, width, height, bbox,
          GetElevationExtents(projectSettings, filter, projectId, mode));

      //TileRequest is both v1 and v2 model so cannot change its validation directly.
      //However for v2 we want to return a transparent empty tile for cut-fill if no design specified.
      //So catch the validation exception for this case.
      bool getTile = true;
      try
      {
        tileRequest.Validate();
      }
      catch (ServiceException se)
      {
        if (tileRequest.mode == DisplayMode.CutFill && tileRequest.designDescriptor == null)
        {
          if (se.Code == HttpStatusCode.BadRequest &&
              se.GetResult.Code == ContractExecutionStatesEnum.ValidationError &&
              se.GetResult.Message ==
              "Design descriptor required for cut/fill and design to filter or filter to design volumes display")
          {
            getTile = false;
          }
        }
        //Rethrow any other exception
        if (getTile)
          throw se;
      }

      TileResult tileResult = null;
      if (getTile)
      {
        tileResult = RequestExecutorContainerFactory
          .Build<TilesExecutor>(logger, raptorClient)
          .Process(tileRequest) as TileResult;
      }

      if (tileResult == null)
      {
        //Return en empty tile
        using (Bitmap bitmap = new Bitmap(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE))
        {
          tileResult = TileResult.CreateTileResult(bitmap.BitmapToByteArray(), TASNodeErrorStatus.asneOK);
        }
      }
      return tileResult;
    }

    /// <summary>
    /// Get the elevation extents for the palette for elevation tile requests
    /// </summary>
    /// <param name="projectSettings">Project settings to use for Raptor</param>
    /// <param name="filter">Filter to use for Raptor</param>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="mode">Display mode; type of data requested</param>
    /// <returns>Elevation extents to use</returns>
    private ElevationStatisticsResult GetElevationExtents(CompactionProjectSettings projectSettings, Productivity3D.Common.Models.Filter filter, long projectId, DisplayMode mode)
    {
      var elevExtents = mode == DisplayMode.Height ? elevProxy.GetElevationRange(projectId, filter, projectSettings) : null;
      //Fix bug in Raptor - swap elevations if required
      elevExtents?.SwapElevationsIfRequired();
      return elevExtents;
    }

  }


  public enum MapType
  {
    SATELLITE,
    HYBRID,
    TERRAIN
  }

  public interface IMapTileGenerator
  {
    string GetRegion(double lat, double lng);

    TileResult GetProductionDataTile(CompactionProjectSettings projectSettings, Productivity3D.Common.Models.Filter filter,
      long projectId, DisplayMode mode, ushort width, ushort height,
      BoundingBox2DLatLon bbox, DesignDescriptor cutFillDesign, IDictionary<string, string> customHeaders);
  }
}

