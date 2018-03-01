using System;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// Provides base map tile functionality for reports. Provider is ALK maps.
  /// </summary>
  public class MapTileService : IMapTileService
  {
    private readonly IConfigurationStore config;
    private readonly ILogger log;
    private readonly ILoggerFactory logger;

    private readonly IMemoryCache alkCache;

    private readonly string alkKey;

    public MapTileService(IConfigurationStore configuration, IMemoryCache cache, ILoggerFactory logger)
    {
      config = configuration;
      log = logger.CreateLogger<MapTileService>();
      this.logger = logger;
      alkKey = config.GetValueString("ALK_KEY");
      alkCache = cache;
      if (string.IsNullOrEmpty(alkKey))
      {
        log.LogError("Missing ALK maps key");
      }
    }

    /// <summary>
    /// Get the base map tile according to the map type.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="mapType"></param>
    /// <param name="locale"></param>
    /// <returns>ALK map tile</returns>
    public byte[] GetMapBitmap(MapParameters parameters, MapType mapType, string locale)
    {
      log.LogInformation($"GetMapBitmap: mapType={mapType}");
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

      var region = GetRegion(parameters.bbox.centerLatDegrees, parameters.bbox.centerLngDegrees);
      var dataset = "PCM_" + region; //"current";
      var mapLayers = "Cities,Labels,Roads,Commercial,Borders,Areas";
      var baseUrl = "http://pcmiler.alk.com/APIs/REST/v1.0/Service.svc/map";
      mapURL =
        $"{baseUrl}?AuthToken={alkKey}&pt1={parameters.bbox.minLngDegrees:F6},{parameters.bbox.minLatDegrees:F6}&pt2={parameters.bbox.maxLngDegrees:F6},{parameters.bbox.maxLatDegrees:F6}&width={parameters.mapWidth}&height={parameters.mapHeight}&drawergroups={mapLayers}&style={alkMapType}&srs=EPSG:900913&region={region}&dataset={dataset}&language={locale}&imgSrc=Sat1";
      if (mapType == MapType.SATELLITE)
      {
        mapURL += "&imgOption=BACKGROUND";
      }

      return alkCache.GetOrCreate(mapURL, entry =>
      {

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
        entry.AbsoluteExpiration = DateTimeOffset.MaxValue;
        return mapImage;
      });
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
      string[] REGIONS = { "EU", "AF", "AS", "EU", "NA", "OC", "SA", "ME" };

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
        var region = result["Region"] as int?;
        if (region.HasValue)
          return REGIONS[region.Value];
      }
      return "EU";
    }

  }

  public enum MapType
  {
    MAP,
    SATELLITE,
    HYBRID,
    TERRAIN
  }

  public interface IMapTileService
  {
    byte[] GetMapBitmap(MapParameters parameters, MapType mapType, string locale);
    string GetRegion(double lat, double lng);
  }
}
