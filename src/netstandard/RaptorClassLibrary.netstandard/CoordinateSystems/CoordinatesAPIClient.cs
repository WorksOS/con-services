using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using Newtonsoft.Json;

namespace VSS.TRex.CoordinateSystems
{
  /// <summary>
  /// Implements a proxy client to the Trimble Coordinates service
  /// </summary>
  public class CoordinatesApiClient //: TPaasAPIClient, ICoordinatesApiClient
  {
    private static string baseUrl = "https://api-stg.trimble.com/t/trimble.com/coordinates/1.0";
    private static string applicationAccessToken = "726bca09144f00fd1859be4c5883d650";

    private static readonly ILogger Log = Logging.Logger.CreateLogger<CoordinatesApiClient>();

    /// <summary>
    /// Converts a single LLH coordinate to a NEE corrdinate via an asynchronous service call
    /// </summary>
    /// <param name="id"></param>
    /// <param name="llh"></param>
    /// <returns></returns>
    public async Task<NEE> GetNEEAsync(string id, LLH llh)
    {
      try
      {
        using (var client = new HttpClient())
        {
          client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", applicationAccessToken);

          Uri requestUri = new Uri($"{baseUrl}/coordinates/nee/Orientated/fromLLH" +
                                   $"?Type=ReferenceGlobal&Latitude={llh.Latitude}&Longitude={llh.Longitude}&Height={llh.Height}" +
                                   $"&fromCoordinateSystemId={id}");

          var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
          var result = await client.SendAsync(request).ConfigureAwait(false);

          if (result.IsSuccessStatusCode)
          {
            var neeStr = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<NEE>(neeStr);
          }
        }
      }
      catch (Exception x)
      {
        Log.LogError($"Failed to get NEE for lat={llh.Latitude}, lon={llh.Longitude}, height={llh.Height}, Exception: {x}");
      }

      return new NEE()
      {
        East = Consts.NullDouble,
        North = Consts.NullDouble,
        Elevation = Consts.NullDouble
      };
    }

    /// <summary>
    /// Encodes the state to transform into the JSON Post body for a multi LLH->NEE coordinate conversion request
    /// </summary>
    private struct GetNEEsAsyncJSON
    {
//      [JsonProperty("neeType")]
//      public string neeType;
//      [JsonProperty("fromCoordinateSystemId")]
//      public string fromCoordinateSystemId;
      [JsonProperty("from")]
      public double[] fromLLHOrdinates;
//      [JsonProperty("fromType")]
//      public string fromType;

      public static GetNEEsAsyncJSON LLHToNEERquest(string csib, LLH[] llhs)
      {
        GetNEEsAsyncJSON result = new GetNEEsAsyncJSON
        {
//          neeType = "Orientated", 
//          fromCoordinateSystemId = csib,
          fromLLHOrdinates = new double[llhs.Length * 3]
//          fromType = "ReferenceGlobal"
        };

        // Fill in the ordinate array
        int count = 0;
        for (int i = 0; i < llhs.Length; i++)
        {
          result.fromLLHOrdinates[count++] = llhs[i].Latitude;
          result.fromLLHOrdinates[count++] = llhs[i].Longitude;
          result.fromLLHOrdinates[count++] = llhs[i].Height;
        }

        return result;
      }
    }

    /// <summary>
    /// Converts an array of LLH coordinates to a NEE corrdinates via an asynchronous service call
    /// </summary>
    /// <param name="id"></param>
    /// <param name="llh"></param>
    /// <returns></returns>
    public async Task<NEE[]> GetNEEsAsync(string id, LLH[] llhs)
    {
      try
      {
        using (var client = new HttpClient())
        {
          client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", applicationAccessToken);

          Uri requestUri = new Uri($"{baseUrl}/coordinates/nee/Orientated/fromLLH" +
                                   $"?fromCoordinateSystemId={id}&Type=ReferenceGlobal");
//          Uri requestUri = new Uri($"{baseUrl}/coordinates/nee/Orientated/fromLLH");
          GetNEEsAsyncJSON body = GetNEEsAsyncJSON.LLHToNEERquest(id, llhs);

          string s = JsonConvert.SerializeObject(body.fromLLHOrdinates);
          var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
          request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"); 
          
          var result = await client.SendAsync(request).ConfigureAwait(false);

          if (result.IsSuccessStatusCode)
          {
            var neeStr = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<NEE[]>(neeStr);
          }
        }
      }
      catch (Exception x)
      {
        Log.LogError($"Failed to get NEE for lat array, Exception: {x}");
      }

      return new NEE[]
      { new NEE { 
        East = Consts.NullDouble,
        North = Consts.NullDouble,
        Elevation = Consts.NullDouble }
      };
    }

    /// <summary>
    /// Converts a single NEE coordinate to a LLH corrdinate via an asynchronous service call
    /// </summary>
    /// <param name="id"></param>
    /// <param name="nee"></param>
    /// <returns></returns>
    public async Task<LLH> GetLLHAsync(string id, NEE nee)
    {
      try
      {
        using (var client = new HttpClient())
        {
          client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", applicationAccessToken);

          Uri requestUri = new Uri($"{baseUrl}/coordinates/llh/ReferenceGlobal/fromNEE" +
                                   $"?from.type=Orientated&from.northing={nee.North}&from.easting={nee.East}&from.elevation={nee.Elevation}" +
                                   $"&fromCoordinateSystemId={id}");

          var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
          var result = await client.SendAsync(request).ConfigureAwait(false);

          if (result.IsSuccessStatusCode)
          {
            var llhStr = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<LLH>(llhStr);
          }
        }
      }
      catch (Exception x)
      {
        Log.LogError($"Failed to get LLH for east={nee.East}, north={nee.North}, elevation={nee.Elevation}, Excpetion: {x}");
      }

      return new LLH()
      {
        Latitude = Consts.NullDouble,
        Longitude = Consts.NullDouble,
        Height = Consts.NullDouble
      };
    }

    /// <summary>
    /// Private struct to aid JSON deserialisation of coordinate system CSIB extraction from a file
    /// </summary>
    private struct CoordinateSystemWithCSIB
    {
      public string id; // the returned csib
    }

    private struct CoordinateSystem
    {
      public CoordinateSystemWithCSIB coordinateSystem;
    }

    /// <summary>
    /// Imported a DC file (presnted as a filename reference in a file system) and extracts a CSIB from it
    /// </summary>
    /// <param name="DCFilePath"></param>
    /// <returns></returns>
    public async Task<string> ImportFromDCAsync(string DCFilePath)
    {
      try
      {
        return ImportFromDCContentAsync(DCFilePath, File.ReadAllBytes(DCFilePath)).Result;
      }
      catch (Exception x)
      {
        Log.LogError($"Failed to import coordinate system from DC {DCFilePath}, Exception {x}");
      }

      return null;
    }

    /// <summary>
    /// Extracts a CSIB from a DC file presented as a byte array
    /// </summary>
    /// <param name="dCFilePath"></param>
    /// <param name="DCFileContent"></param>
    /// <returns></returns>
    public async Task<string> ImportFromDCContentAsync(string dCFilePath, byte[] DCFileContent)
    {
      string imported = null;

      try
      {
        using (var client = new HttpClient())
        {
          client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", applicationAccessToken);
          client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

          Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/imports/dc/file");

          using (var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
          {
            content.Add(new StreamContent(new MemoryStream(DCFileContent)), "DC", Path.GetFileName(dCFilePath));

            using (var result = await client.PutAsync(requestUri, content).ConfigureAwait(false))
            {
              if (!result.IsSuccessStatusCode)
                throw new Exception(result.ToString());

              var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
              var csList = JsonConvert.DeserializeObject<IEnumerable<CoordinateSystem>>(json);
              imported = csList?.FirstOrDefault().coordinateSystem.id;
            }
          }
        }
      }
      catch (Exception x)
      {
        Log.LogError($"Failed to import coordinate system from DC {dCFilePath}, Exception {x}");
      }

      return imported;
    }

  }
}

/*
  using Bellbird.Common;
using Bellbird.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Trimble.Coordinates.Model;

namespace Bellbird.CoordinatesAPI
{
    public class CoordinatesApiClient : TPaasAPIClient, ICoordinatesApiClient
    {
        private readonly string tidAccessToken;
        private readonly string baseUrl;
        private readonly ILogger log;

        private static IEnumerable<ZoneX> zonesCache;
        private static SemaphoreSlim zonesCacheMutex = new SemaphoreSlim(1);
        private static IEnumerable<DatumX> datumsCache;
        private static SemaphoreSlim datumsCacheMutex = new SemaphoreSlim(1);
        private static IEnumerable<GeoidX> geoidsCache;
        private static SemaphoreSlim geoidsCacheMutex = new SemaphoreSlim(1);

        public CoordinatesApiClient(IJsonSerializerUtility jsonSerializerUtility,
                                    ILogger logger,
                                    IApplicationState applicationState)
            : base(jsonSerializerUtility)
        {
            tidAccessToken = applicationState.TIDAuthToken;
            this.log = logger;
            baseUrl = Config.CoordinatesAPI;
        }

        public async Task<IEnumerable<MySystem>> ListMySystemsAsync()
        {
            IEnumerable<MySystem> mySystems = new MySystem[0];

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tidAccessToken);

                    Uri requestUri = new Uri($"{baseUrl}/mysystems");

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    var result = await client.SendAsync(request).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        mySystems = jsonSerializerUtility.DeserializeObject<IEnumerable<MySystem>>(json);
                    }
                }
            }
            catch (Exception x)
            {
                log.Error("Failed to list users 'My Systems'", x);
            }

            return mySystems;
        }

        public async Task<MySystem> AddMySystemAsync(string name, string id)
        {
            MySystem added = null;

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tidAccessToken);

                    // replace any existing one with the same name.
                    await client.DeleteAsync($"{baseUrl}/mysystems?name={name}");

                    Uri requestUri = new Uri($"{baseUrl}/mysystems?name={name}&id={id}");

                    var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
                    var result = await client.SendAsync(request).ConfigureAwait(false);
                    if (!result.IsSuccessStatusCode)
                        throw new Exception(result.ToString());

                    var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    added = jsonSerializerUtility.DeserializeObject<MySystem>(json);
                }
            }
            catch(Exception x)
            {
                log.Error($"Failed to save coordinate system to users 'My Systems'", x);
            }

            return added;
        }

        public async Task<CoordinateSystem> ImportFromJXLAsync(string JXLFilePath)
        {
            CoordinateSystem imported = null;

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));
                    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

                    Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/imports/jxl/file");

                    using (var content =
                                 new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                    {
                        content.Add(new StreamContent(new MemoryStream(File.ReadAllBytes(JXLFilePath))), "JXL", Path.GetFileName(JXLFilePath));

                        using (var result = await client.PutAsync(requestUri, content).ConfigureAwait(false))
                        {
                            if (!result.IsSuccessStatusCode)
                                throw new Exception(result.ToString());

                            var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                            var csList = jsonSerializerUtility.DeserializeObject<IEnumerable<CoordinateSystemWithCSIB>>(json);
                            imported = csList?.FirstOrDefault()?.CoordinateSystem;
                            var jo = JObject.Parse(json);
                            imported.DatumInfo = UpcastDatumInfo(jo.SelectToken("coordinateSystem.datumInfo"));
                        }
                    }
                }
            }
            catch(Exception x)
            {
                log.Error($"Failed to import coordinate system from Jxl {JXLFilePath}", x);
            }

            return imported;
        }

        public async Task<CoordinateSystem> ImportFromJXLDocAsync(string JXLDocument)
        {
            CoordinateSystem imported = null;

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));
                    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

                    Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/imports/jxl");

                    using (var content = new StringContent(JXLDocument, Encoding.UTF8, "text/plain"))
                    {
                        using (var result = await client.PutAsync(requestUri, content).ConfigureAwait(false))
                        {
                            if (!result.IsSuccessStatusCode)
                                throw new Exception(result.ToString());

                            var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                            imported = jsonSerializerUtility.DeserializeObject<CoordinateSystemWithCSIB>(json)?.CoordinateSystem;
                            var jo = JObject.Parse(json);
                            imported.DatumInfo = UpcastDatumInfo(jo.SelectToken("coordinateSystem.datumInfo"));
                        }
                    }
                }
            }
            catch (Exception x)
            {
                log.Error($"Failed to import coordinate system from Jxl Doc", x);
            }

            return imported;
        }

        public async Task<IEnumerable<CoordinateSystem>> SearchAsync(string searchText, double? proximityLatitude, double? proximityLongitude)
        {
            IEnumerable<CoordinateSystem> results = null;

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));

                    string epsgFilterTerm = string.Empty;
                    string latitudeFilterTerm = proximityLatitude.HasValue ? $"&proximityLatitude={proximityLatitude}" : string.Empty;
                    string longitudeFilterTerm = proximityLongitude.HasValue ? $"&proximityLongitude={proximityLongitude}" : string.Empty;
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        uint epsgCode;
                        bool isEPSGCode = uint.TryParse(searchText, out epsgCode);
                        if (isEPSGCode)
                            epsgFilterTerm = $"&epsgCodeMatch={epsgCode}";
                    }

                    Uri requestUri = new Uri($"{baseUrl}/coordinatesystems?textMatch={searchText}{epsgFilterTerm}{latitudeFilterTerm}{longitudeFilterTerm}");

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    var result = await client.SendAsync(request).ConfigureAwait(false);
                    if (!result.IsSuccessStatusCode)
                        throw new Exception(result.ToString());

                    var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    results = jsonSerializerUtility.DeserializeObject<IEnumerable<CoordinateSystem>>(json);
                }
            }
            catch(Exception x)
            {
                log.Error($"Coordinate system ({searchText},{proximityLatitude},{proximityLongitude}) search failed",x);
            }

            return results;
        }

        public async Task<IEnumerable<ZoneGroupX>> ListZoneGroupsAsync(string searchText)
        {
            IEnumerable<ZoneGroupX> zoneGroups = new ZoneGroupX[0];

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));

                    Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/zonegroups?textMatch={searchText}");

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    var result = await client.SendAsync(request).ConfigureAwait(false);
                    if (!result.IsSuccessStatusCode)
                        throw new Exception(result.ToString());

                    var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    zoneGroups = jsonSerializerUtility.DeserializeObject<IEnumerable<ZoneGroupX>>(json);
                }
            }
            catch(Exception x)
            {
                log.Error("Failed to list zone groups", x);
            }

            return zoneGroups;
        }

        public async Task<IEnumerable<ZoneX>> ListZonesInGroupAsync(ZoneGroupX zoneGroup, string searchText, double? proximityLatitude, double? proximityLongitude)
        {
            IEnumerable<ZoneX> zones = new ZoneX[0];

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));

                    string epsgFilterTerm = string.Empty;
                    string latitudeFilterTerm = proximityLatitude.HasValue ? $"&proximityLatitude={proximityLatitude}" : string.Empty;
                    string longitudeFilterTerm = proximityLongitude.HasValue ? $"&proximityLongitude={proximityLongitude}" : string.Empty;
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        uint epsgCode;
                        bool isEPSGCode = uint.TryParse(searchText, out epsgCode);
                        if (isEPSGCode)
                            epsgFilterTerm = $"&epsgCodeMatch={epsgCode}";
                    }

                    Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/zonesgroups/{zoneGroup.ZoneGroupSystemId}/zones?textMatch={searchText}{epsgFilterTerm}{latitudeFilterTerm}{longitudeFilterTerm}");

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    var result = await client.SendAsync(request).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        zones = jsonSerializerUtility.DeserializeObject<IEnumerable<ZoneX>>(json);
                    }
                }
            }
            catch(Exception x)
            {
                log.Error($"Failed to list zones in group ({zoneGroup?.ZoneGroupName},{searchText},{proximityLatitude},{proximityLongitude})",x);
            }

            return zones;
        }

        public async Task<IEnumerable<ZoneX>> ListZonesAsync(string searchText, double? proximityLatitude, double? proximityLongitude)
        {
            await zonesCacheMutex.WaitAsync().ConfigureAwait(false);

            if (null == zonesCache)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        if (IsTPaaSUrl(baseUrl))
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                                await GetApplicationAccessToken().ConfigureAwait(false));

                        Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/zones");

                        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                        var result = await client.SendAsync(request).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                            zonesCache = jsonSerializerUtility.DeserializeObject<IEnumerable<ZoneX>>(json);
                        }
                    }
                }
                catch (Exception x)
                {
                    log.Error($"Failed to list zones, ({searchText}, {proximityLatitude}, {proximityLongitude})", x);
                }
            }
            zonesCacheMutex.Release();

            IEnumerable<ZoneX> zones = null == zonesCache ? new List<ZoneX>() : new List<ZoneX>(zonesCache);

            try
            {
                if (null != proximityLatitude)
                    zones = zones.Where(z => null != z.Extents &&
                                            z.Extents.PointIsInside(new LLH { Latitude = proximityLatitude.Value, Longitude = proximityLongitude.Value }))
                                 .ToList();

                if (!string.IsNullOrEmpty(searchText))
                    zones = zones.Where(z => z.ZoneName.ToLower().Contains(searchText.ToLower()) ||
                                             z.ZoneGroupName.ToLower().Contains(searchText.ToLower()))
                                             .ToList();
            }
            catch(Exception x)
            {
                log.Error("Failed to apply filter to zones", x);
            }

            return zones;
        }

        public async Task<IEnumerable<DatumX>> ListDatumsAsync(string searchText, double? proximityLatitude, double? proximityLongitude)
        {
            await datumsCacheMutex.WaitAsync().ConfigureAwait(false);

            if ( null == datumsCache )
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        if (IsTPaaSUrl(baseUrl))
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                                await GetApplicationAccessToken().ConfigureAwait(false));

                        Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/datums");

                        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                        var result = await client.SendAsync(request).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                            datumsCache = jsonSerializerUtility.DeserializeObject<IEnumerable<DatumX>>(json);
                        }
                    }
                }
                catch (Exception x)
                {
                    log.Error($"Failed to list datums, ({searchText},{proximityLatitude},{proximityLongitude})", x);
                }
            }
            datumsCacheMutex.Release();

            List<DatumX> datums = null == datumsCache ? new List<DatumX>() : new List<DatumX>(datumsCache);

            try
            {
                if (null != proximityLatitude)
                    datums = datums.Where(dd => null != dd.Extents &&
                                                dd.Extents.PointIsInside(new LLH { Latitude = proximityLatitude.Value, Longitude = proximityLongitude.Value }))
                                   .ToList();

                if (!string.IsNullOrEmpty(searchText))
                    datums = datums.Where(z => z.DatumName.ToLower().Contains(searchText.ToLower())).ToList();
            }
            catch(Exception x)
            {
                log.Error("Failed to apply filter to datums",x);
            }

            return datums;
        }

        public async Task<DatumInfo> GetZoneDefaultDatumAsync(ZoneInfo zone)
        {
            DatumInfo defaultZoneDatum = null;

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));

                    Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/zones/{zone.ZoneSystemId}/defaultDatum");

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    var result = await client.SendAsync(request).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        defaultZoneDatum = UpcastDatumInfo(JObject.Parse(json));
                    }
                }
            }
            catch(Exception x)
            {
                log.Error($"Failed to find get default zone datum for {zone?.ZoneName}", x);
            }

            return defaultZoneDatum;
        }

        public async Task<GeoidInfo> GetZoneDefaultGeoidAsync(ZoneInfo zone)
        {
            GeoidInfo defaultZoneGeoid = null;

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));

                    Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/zones/{zone.ZoneSystemId}/defaultGeoid");

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    var result = await client.SendAsync(request).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        defaultZoneGeoid = jsonSerializerUtility.DeserializeObject<GeoidInfo>(json);
                    }
                }
            }
            catch(Exception x)
            {
                log.Error($"Failed to get default goid for zone {zone?.ZoneName}", x);
            }

            return defaultZoneGeoid;
        }

        public async Task<IEnumerable<GeoidX>> ListGeoidsAsync(string searchText, double? proximityLatitude, double? proximityLongitude)
        {
            await geoidsCacheMutex.WaitAsync().ConfigureAwait(false);

            if (null == geoidsCache)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        if (IsTPaaSUrl(baseUrl))
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                                await GetApplicationAccessToken().ConfigureAwait(false));

                        Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/geoids");

                        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                        var result = await client.SendAsync(request).ConfigureAwait(false);

                        if (result.IsSuccessStatusCode)
                        {
                            var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                            geoidsCache = jsonSerializerUtility.DeserializeObject<IEnumerable<GeoidX>>(json);
                        }
                    }
                }
                catch (Exception x)
                {
                    log.Error($"Failed to list geoids, ({searchText},{proximityLatitude},{proximityLongitude})", x);
                }
            }
            geoidsCacheMutex.Release();

            IEnumerable<GeoidX> geoids = null == geoidsCache ? new List<GeoidX>() : new List<GeoidX>(geoidsCache);

            try
            {
                if (null != proximityLatitude)
                {
                    // Spatial search of geoids is based on the extents of the zones that utilise the geoid
                    var allZones = await ListZonesAsync(null, proximityLatitude, proximityLongitude).ConfigureAwait(false);

                    var zonesWithGeoids = allZones
                        .Where(zz => zz.DefaultGeoidSystemId.HasValue)
                        .Select(zz => zz.DefaultGeoidSystemId)
                        .ToList();
                    geoids = geoids.Where(gg => zonesWithGeoids.Contains(gg.GeoidSystemId)).ToList();
                }

                if (!string.IsNullOrEmpty(searchText))
                    geoids = geoids.Where(g => g.GeoidName.ToLower().Contains(searchText.ToLower())).ToList();
            }
            catch(Exception x)
            {
                log.Error("Failed to apply filter to geoids", x);
            }

            return geoids;
        }

        public async Task<int> GetGeodataFileSizeAsync(string filename)
        {
            int size = 0;

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));

                    Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/geodataFiles/size?filename={filename}");

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    var result = await client.SendAsync(request).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var sizeResponse = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                        int.TryParse(sizeResponse, out size);
                    }
                }
            }
            catch (Exception x)
            {
                log.Error($"Failed to get geodata file size for file {filename}", x);
            }

            return size;
        }

        public async Task<ZoneInfo> GetZoneAsync(ZoneX zone)
        {
            ZoneInfo zoneInfo = null;

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));

                    Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/zones/{zone.ZoneSystemId}");

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    var result = await client.SendAsync(request).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        zoneInfo = jsonSerializerUtility.DeserializeObject<ZoneInfo>(json);

                        var jo = JObject.Parse(json);
                        zoneInfo.DefaultDatum = UpcastDatumInfo(jo.SelectToken("defaultDatum"));
                    }
                }
            }
            catch(Exception x)
            {
                log.Error($"Failed to get full zone info for zone {zone?.ZoneName}", x);
            }

            return zoneInfo;
        }

        public async Task<DatumInfo> GetDatumAsync(DatumX datum)
        {
            DatumInfo datumInfo = null;

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));

                    Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/datums/{datum.DatumSystemId}");

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    var result = await client.SendAsync(request).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        datumInfo = UpcastDatumInfo( JObject.Parse(json) );
                    }
                }
            }
            catch(Exception x)
            {
                log.Error($"Failed to get full datum info for datum {datum?.DatumName}", x);
            }

            return datumInfo;
        }

        public async Task<GeoidInfo> GetGeoidAsync(GeoidX geoid)
        {
            GeoidInfo geoidInfo = null;

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));

                    Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/geoids/{geoid.GeoidSystemId}");

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    var result = await client.SendAsync(request).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        geoidInfo = jsonSerializerUtility.DeserializeObject<GeoidInfo>(json);
                    }
                }
            }
            catch(Exception x)
            {
                log.Error($"Failed to get full geoid info for geoid {geoid?.GeoidName}", x);
            }

            return geoidInfo;
        }

        public async Task<CoordinateSystemWithCSIB> CreateAsync(CoordinateSystem customCoordinateSystem)
        {
            CoordinateSystemWithCSIB created = null;
            string csjson = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));

                    Uri requestUri = new Uri($"{baseUrl}/coordinatesystems");

                    csjson = jsonSerializerUtility.SerializeObject<CoordinateSystem>(customCoordinateSystem);

                    var result = await client.PostAsync(requestUri,
                        new StringContent(csjson, Encoding.UTF8, "application/json")).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        created = jsonSerializerUtility.DeserializeObject<CoordinateSystemWithCSIB>(json);
                    }
                }
            }
            catch(Exception x)
            {
                log.Error($"Failed to create coordinate system from custom coordinate system, {csjson}", x);
            }

            return created;
        }

        public async Task DownloadGeodataFileAsync(string fileName, string downloadPath, DateTime? ifModifiedSince)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));

                    Uri requestUri = new Uri($"{baseUrl}/coordinatesystems/geodataFiles/?filename={fileName}");

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                    if (ifModifiedSince.HasValue)
                        request.Headers.IfModifiedSince = ifModifiedSince.Value;

                    var result = await client.SendAsync(request).ConfigureAwait(false);

                    if (result.StatusCode == HttpStatusCode.OK)  // don't download if statuscode == 304-NotModified.
                    {
                        FileStream localStream = new FileStream(downloadPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                        await result.Content.CopyToAsync(localStream).ContinueWith(
                            (copyTask) =>
                            {
                                localStream.Close();
                            }).ConfigureAwait(false);
                    }
                }
            }
            catch(Exception x)
            {
                log.Error($"Failed to download geodata file, {fileName}, to '{downloadPath}'", x);
            }
        }

        public async Task<double> GetScaleFactorAsync(string id, LLH llh)
        {
            double scaleFactor = 1.0;
            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));

                    Uri requestUri = new Uri($"{baseUrl}/coordinates/combinedFactor/fromLLH/" +
                        $"?Type={llh.Type}&Latitude={llh.Latitude}&Longitude={llh.Longitude}&Height={llh.Height}" +
                        $"&fromCoordinateSystemId={id}");

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    var result = await client.SendAsync(request).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var doubleStr = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        double.TryParse(doubleStr, out scaleFactor);
                    }
                }
            }
            catch (Exception x)
            {
                log.Error($"Failed to get scale factor for lat={llh.Latitude}, lon={llh.Longitude}, height={llh.Height}", x);
            }

            return scaleFactor;
        }

        public async Task<NEE> GetNEEAsync(string id, NEEType neeType, LLH llh)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    if (IsTPaaSUrl(baseUrl))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                            await GetApplicationAccessToken().ConfigureAwait(false));

                    Uri requestUri = new Uri($"{baseUrl}/coordinates/nee/{neeType}/fromLLH/" +
                        $"?Type={llh.Type}&Latitude={llh.Latitude}&Longitude={llh.Longitude}&Height={llh.Height}" +
                        $"&fromCoordinateSystemId={id}");

                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    var result = await client.SendAsync(request).ConfigureAwait(false);

                    if (result.IsSuccessStatusCode)
                    {
                        var neeStr = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        return jsonSerializerUtility.DeserializeObject<NEE>(neeStr);
                    }
                }
            }
            catch (Exception x)
            {
                log.Error($"Failed to get NEE for lat={llh.Latitude}, lon={llh.Longitude}, height={llh.Height}", x);
            }

            return null;
        }

        private DatumInfo UpcastDatumInfo(JToken datumInfo)
        {
            if (null == datumInfo)
                return null;
            DatumType dt = (DatumType)Enum.Parse(typeof(DatumType), datumInfo["datumType"].ToString());
            switch (dt)
            {
                case DatumType.Molodensky: return datumInfo.ToObject<MolodenskyDatumInfo>();
                case DatumType.SevenParameter: return datumInfo.ToObject<SevenParameterDatumInfo>();
                case DatumType.GridDatum: return datumInfo.ToObject<GridDatumInfo>();
                case DatumType.RtcmDatum: return datumInfo.ToObject<RtcmDatumInfo>();
            }

            return datumInfo.ToObject<DatumInfo>();
        }


    }
}
*/
