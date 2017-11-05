using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Common.Executors;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Helpers;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Factories.ProductionData;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.Common.MapHandling
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
    private readonly string tccFilespace;

    public int MapWidth => 128;
    public int MapHeight => 128;
    public string Locale => "EN";

    public long ProjectId { get; private set; }
    public long CustomerId { get; private set; }

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
      tccFilespace = config.GetValueString("TCCFILESPACEID");
    }

    public class MapBoundingBox
    {
      public double minLat;
      public double minLng;
      public double maxLat;
      public double maxLng;

      public double centerLat => minLat + (maxLat - minLat) / 2;

      public double centerLng => minLng + (maxLng - minLng) / 2;
    }

    /// <summary>
    /// Gets the ALK region.
    /// </summary>
    /// <param name="key">The key.</param>
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

    public byte[] GetMapBitmap(MapBoundingBox bbox, MapType mapType, int zoomLevel)
    {
      const int SATELLITE = 2;
      const int HYBRID = 3;
      const int TERRAIN = 4;

      string mapURL = null;

      string alkMapType;
      switch (mapType)
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

      var region = GetRegion(bbox.centerLat, bbox.centerLng);
      var dataset = "PCM_" + region; //"current";
      var mapLayers = "Cities,Labels,Roads,Commercial,Borders,Areas";
      var baseUrl = "http://pcmiler.alk.com/APIs/REST/v1.0/Service.svc/map";
      mapURL =
        $"{baseUrl}?AuthToken={alkKey}&pt1={bbox.minLng:F6},{bbox.minLat:F6}&pt2={bbox.maxLng:F6},{bbox.maxLat:F6}&width={MapWidth}&height={MapHeight}&drawergroups={mapLayers}&style={alkMapType}&srs=EPSG:900913&region={region}&dataset={dataset}&language={Locale}&imgSrc=Sat1";
      if (mapType == MapType.SATELLITE)
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

    protected void GetMapData(MapReportData reportData, int displayType, bool gotZoomBounds)
    {
      if (!gotZoomBounds)
        reportData.zoomLevel = CalculateZoomLevel(reportData.boundingBox);
      reportData.numTiles = 1 << reportData.zoomLevel; //equivalent to 2 to the power of zoomLevel
      //if (!gotZoomBounds)
      CalculateWidthHeight(reportData.boundingBox, reportData.numTiles);

      BE.Point latLngTopLeft = new BE.Point(reportData.boundingBox.maxLat, reportData.boundingBox.minLng);
      reportData.pixelTopLeft = WebMercatorProjection.LatLngToPixel(latLngTopLeft, reportData.numTiles);

      //Get the map data
      reportData.mapImage = GetMapBitmap(reportData.boundingBox, reportData.zoomLevel);
      if (displayType != ExportReportParameterXMLData.LOAD_DETAIL && displayType != ExportReportParameterXMLData.CYCLE_DETAIL)
        reportData.wmsImage = GetRaptorBitmap(reportData.boundingBox, displayType);
      reportData.sitesImage = GetSitesBitmap(reportData.boundingBox, displayType, reportData.pixelTopLeft, reportData.numTiles);
      reportData.locationsImage = GetLocationsBitmap(reportData.pixelTopLeft, reportData.numTiles);
      reportData.alignmentsImage = GetAlignmentsBitmap(reportData.pixelTopLeft, reportData.numTiles);
      GetDxfBitmaps(reportData);
      reportData.legendImage = GetLegendBitmap(displayType);
    }



    /// <summary>
    /// Overlays the DXF tiles. This can not be resued from the execuor as it does merging of tiles into a single one
    /// </summary>
    /// <param name="dxfFileName">Name of the DXF file.</param>
    /// <param name="bbox">The bbox.</param>
    /// <param name="zoomLevel">The zoom level.</param>
    /// <returns></returns>
    private async Task<byte[]> OverlayDxfTiles(string dxfFileName, MapBoundingBox bbox, int zoomLevel)
    {
      //Find the tiles that the bounding box fits into.
      Point tileTopLeft = WebMercatorProjection.PixelToTile(reportData.pixelTopLeft);
      Point tileBottomRight = WebMercatorProjection.LatLngToTile(new Point(bbox.minLat, bbox.maxLng), 1 << zoomLevel);

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
        int xClipTopLeft = (int)(reportData.pixelTopLeft.x - point.x);
        int yClipTopLeft = (int)(reportData.pixelTopLeft.y - point.y);
        Rectangle clipRect = new Rectangle(xClipTopLeft, yClipTopLeft, MapWidth, MapHeight);
        g.SetClip(clipRect);

        string zoomFolder = FileUtils.ZoomFolder(zoomLevel);
        string zoomPath = $"{FileUtils.FilePath(CustomerId,ProjectId)}/{FileUtils.TilesFolderWithSuffix(dxfFileName)}/{zoomFolder}";
        for (int yTile = (int)tileTopLeft.y; yTile <= (int)tileBottomRight.y; yTile++)
        {
          string yFolder = yTile.ToString();
          //TCC only renders tiles where there is DXF data. So check if any tiles for this y.
          if (await tccFileRepository.FolderExists(tccFilespace, $"{zoomPath}/{yFolder}"))
          {
            string targetFolder = $"{zoomPath}/{yFolder}";
            for (int xTile = (int)tileTopLeft.x; xTile <= (int)tileBottomRight.x; xTile++)
            {
              string targetFile = $"{xTile}.png";
              if (await tccFileRepository.FileExists(tccFilespace, $"{targetFolder}/{targetFile}"))
              {
                var file = await tccFileRepository.GetFile(tccFilespace, $"{targetFolder}/{targetFile}");
                Image tile = Image.FromStream(file);

                System.Drawing.Point offset = new System.Drawing.Point(
                      (xTile - (int)tileTopLeft.x) * WebMercatorProjection.TILE_SIZE,
                      (yTile - (int)tileTopLeft.y) * WebMercatorProjection.TILE_SIZE);
                g.DrawImage(tile, offset);
              }
            }
          }
        }

        using (Bitmap clipBitmap = new Bitmap(MapWidth, MapHeight))
        using (Graphics clipGraphics = Graphics.FromImage(clipBitmap))
        {
          clipGraphics.DrawImage(tileBitmap, 0, 0, clipRect, GraphicsUnit.Pixel);
         return BitmapToByteArray(clipBitmap);
        }
      }
    }

    private byte[] BitmapToByteArray(Bitmap bitmap)
    {
      byte[] data;
      using (var bitmapStream = new MemoryStream())
      {
        bitmap.Save(bitmapStream, ImageFormat.Png);
        bitmapStream.Position = 0;
        data = new byte[bitmapStream.Length];
        bitmapStream.Read(data, 0, (int)bitmapStream.Length);
        bitmapStream.Close();
      }
      return data;
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
    public TileResult GetProductionDataTile(CompactionProjectSettings projectSettings, Common.Models.Filter filter, long projectId, DisplayMode mode, ushort width, ushort height,
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
    private ElevationStatisticsResult GetElevationExtents(CompactionProjectSettings projectSettings, Common.Models.Filter filter, long projectId, DisplayMode mode)
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

    TileResult GetProductionDataTile(CompactionProjectSettings projectSettings, Common.Models.Filter filter,
      long projectId, DisplayMode mode, ushort width, ushort height,
      BoundingBox2DLatLon bbox, DesignDescriptor cutFillDesign, IDictionary<string, string> customHeaders);
  }
}

