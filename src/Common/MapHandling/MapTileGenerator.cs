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
using VSS.ConfigurationStore;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.Common.MapHandling
{
  //The class supports overlaying bitmaps from various sources. ALK map tile provider is used as a datasource
  public class MapTileGenerator : IMapTileGenerator
  {
    private readonly IConfigurationStore config;
    private readonly IFileRepository tccFileRepository;
    private readonly string alkKey;
    private readonly string tccFilespace;

    public int MapWidth => 128;
    public int MapHeight => 128;
    public string Locale => "EN";

    public long ProjectId { get; private set; }
    public long CustomerId { get; private set; }

    public MapTileGenerator(IConfigurationStore configuration, IFileRepository tccRepository)
    {
      config = configuration;
      tccFileRepository = tccRepository;
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

    private async Task<byte[]> OverlayDxfTiles(string dxfFileName, MapBoundingBox bbox, int zoomLevel)
    {
      //Find the tiles that the bounding box fits into.
      Point tileTopLeft = WebMercatorProjection.PixelToTile(reportData.pixelTopLeft);
      Point tileBottomRight = WebMercatorProjection.LatLngToTile(new Point(bbox.minLat, bbox.maxLng), reportData.numTiles);

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
  }
}

