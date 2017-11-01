using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using VSS.ConfigurationStore;

namespace VSS.Productivity3D.Common.MapHandling
{
  //The class supports overlaying bitmaps from various sources. ALK map tile provider is used as a datasource
  public class MapTileGenerator : IMapTileGenerator
  {
    private IConfigurationStore config;
    private string alkKey;

    public int MapWidth => 128;
    public int MapHeight => 128;
    public string Locale => "EN";

    public MapTileGenerator(IConfigurationStore configuration)
    {
      config = configuration;
      alkKey = config.GetValueString("ALK_KEY");
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

