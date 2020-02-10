using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Tpaas.Client.Constants;
using VSS.TRex.Common;
using VSS.TRex.CoordinateSystems.Models;
using VSS.TRex.Types;

namespace VSS.TRex.CoordinateSystems
{
  /// <summary>
  /// Implements a proxy httpClient to the Trimble Coordinates service
  /// </summary>
  public class CoordinatesServiceClient
  {
    public const string COORDINATE_SERVICE_URL_ENV_KEY = "COORDINATE_SERVICE_URL";

    private static readonly ILogger log = Logging.Logger.CreateLogger<CoordinatesServiceClient>();
    private readonly CoordinateServiceHttpClient serviceHttpClient;

    // Bug 81615 - We are adding a cahce to Coordinate system calls until CoreX is done, as we are hitting rate limits when generating tiles
    // When CoreX is implemented, this cache will not be needed (nor will this class)
    private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly TimeSpan _cacheTimeout = new TimeSpan(0, 8, 0, 0);

    public CoordinatesServiceClient(HttpClient httpClient)
    {
      if (httpClient == null)
      {
        throw new NullReferenceException($"'{nameof(httpClient)}' cannot be null.");
      }

      serviceHttpClient = new CoordinateServiceHttpClient(httpClient);
    }

    /// <summary>
    /// Converts a single LLH coordinate to a NEE coordinate via an asynchronous service call.
    /// </summary>
    public async Task<NEE> GetNEEFromLLHAsync(string id, LLH coordinates)
    {
      try
      {
        var uniqueIdentifier = $"{nameof(GetNEEFromLLHAsync)}-{id}-{coordinates.Latitude}-{coordinates.Longitude}-{coordinates.Height}";
        var cacheKey = GenerateKey(uniqueIdentifier);
        if (_cache.TryGetValue<NEE>(cacheKey, out var cachedItem))
          return cachedItem;

        var route = "/coordinates/nee/Orientated/fromLLH" +
                    $"?Type=ReferenceGlobal&Latitude={coordinates.Latitude}&Longitude={coordinates.Longitude}&Height={coordinates.Height}" +
                    $"&fromCoordinateSystemId={id}";

        var response = await serviceHttpClient.SendRequest(route, HttpMethod.Get);

        if (response.IsSuccessStatusCode)
        {
          var neeStr = await response.Content.ReadAsStringAsync();

          var result = JsonConvert.DeserializeObject<NEE>(neeStr);
          _cache.Set(cacheKey, result, _cacheTimeout);
          return result;
        }
      }
      catch (Exception exception)
      {
        log.LogError(exception, $"Failed to get NEE for lat={coordinates.Latitude}, lon={coordinates.Longitude}, height={coordinates.Height}");
      }

      return new NEE {East = Consts.NullDouble, North = Consts.NullDouble, Elevation = Consts.NullDouble};
    }

    /// <summary>
    /// Converts a multi dim array of doubles to <see cref="NEE"/> coordinates via an asynchronous service call.
    /// </summary>
    public async Task<(RequestErrorStatus ErrorCode, NEE[] NEECoordinates)> GetNEEFromLLHAsync(string id, double[,] coordinates)
    {
      try
      {
        var route = $"/coordinates/nee/Orientated/fromLLH?fromCoordinateSystemId={id}&fromType=ReferenceGlobal";

        var requestObj = JsonConvert.SerializeObject(coordinates);

        var uniqueId = $"{nameof(GetNEEFromLLHAsync)}-{id}-{requestObj}";
        var cacheKey = GenerateKey(uniqueId);
        if (_cache.TryGetValue<(RequestErrorStatus, NEE[])>(cacheKey, out var cachedItem))
          return cachedItem;

        var response = await serviceHttpClient.SendRequest(route, HttpMethod.Post, MediaTypes.JSON, requestObj);

        if (response.IsSuccessStatusCode)
        {
          var neeStr = await response.Content.ReadAsStringAsync();
          var resultArray = JsonConvert.DeserializeObject<double[,]>(neeStr);
          var result = (RequestErrorStatus.OK, resultArray.ToNEEArray());

          _cache.Set(cacheKey, result, _cacheTimeout);

          return result;
        }
      }
      catch (Exception exception)
      {
        log.LogError(exception, "Failed to get NEE for lat array");
      }

      return (RequestErrorStatus.Exception, null);
    }

    /// <summary>
    /// Converts a single NEE coordinate to a LLH coordinate via an asynchronous service call.
    /// </summary>
    public async Task<LLH> GetLLHFromNEEAsync(string id, NEE coordinates)
    {
      try
      {
        var route = "/coordinates/llh/ReferenceGlobal/fromNEE" +
                    $"?from.type=Orientated&from.northing={coordinates.North}&from.easting={coordinates.East}&from.elevation={coordinates.Elevation}" +
                    $"&fromCoordinateSystemId={id}";

        var uniqueIdentifier = $"{nameof(GetLLHFromNEEAsync)}-{id}-{coordinates.North}-{coordinates.East}-{coordinates.Elevation}";
        var cacheKey = GenerateKey(uniqueIdentifier);
        if (_cache.TryGetValue<LLH>(cacheKey, out var cachedItem))
          return cachedItem;

        var response = await serviceHttpClient.SendRequest(route, HttpMethod.Get);

        if (response.IsSuccessStatusCode)
        {
          var llhStr = await response.Content.ReadAsStringAsync();

          var result = JsonConvert.DeserializeObject<LLH>(llhStr);
          _cache.Set(cacheKey, result, _cacheTimeout);
          return result;
        }
      }
      catch (Exception exception)
      {
        log.LogError(exception, $"Failed to get LLH for east={coordinates.East}, north={coordinates.North}, elevation={coordinates.Elevation}");
      }

      return new LLH {Latitude = Consts.NullDouble, Longitude = Consts.NullDouble, Height = Consts.NullDouble};
    }

    /// <summary>
    /// Converts an array of NEE coordinates to a list of LLH coordinates via an asynchronous service call.
    /// </summary>
    public async Task<(RequestErrorStatus ErrorCode, LLH[] LLHCoordinates)> GetLLHFromNEEAsync(string id, double[,] coordinates)
    {
      try
      {
        var route = $"/coordinates/llh/ReferenceGlobal/fromNEE?fromCoordinateSystemId={id}&fromType=Orientated";

        var requestObj = JsonConvert.SerializeObject(coordinates);

        var uniqueIdentifier = $"{nameof(GetLLHFromNEEAsync)}-{id}-{requestObj}";
        var cacheKey = GenerateKey(uniqueIdentifier);
        if (_cache.TryGetValue<(RequestErrorStatus ErrorCode, LLH[] LLHCoordinates)>(cacheKey, out var cachedItem))
          return cachedItem;

        var response = await serviceHttpClient.SendRequest(route, HttpMethod.Post, MediaTypes.JSON, requestObj);

        if (response.IsSuccessStatusCode)
        {
          var llhStr = await response.Content.ReadAsStringAsync();
          var resultArray = JsonConvert.DeserializeObject<double[,]>(llhStr);

          var result = (RequestErrorStatus.OK, resultArray.ToLLHArray());
          _cache.Set(cacheKey, result, _cacheTimeout);
          return result;
        }
      }
      catch (Exception exception)
      {
        log.LogError(exception, "Failed to get coordinates");
      }

      return (RequestErrorStatus.Exception, null);
    }

    /// <summary>
    /// Imported a DC file (presented as a filePath reference in a file system) and extracts a CSIB from it.
    /// </summary>
    public async Task<string> ImportFromDCAsync(string filePath)
    {
      try
      {
        return await ImportFromDCContentAsync(filePath, File.ReadAllBytes(filePath));
      }
      catch (Exception exception)
      {
        log.LogError(exception, $"Failed to import coordinate system from DC {filePath}");
      }

      return null;
    }

    /// <summary>
    /// Extracts a coordinate system defintion response object from a DC file presented as a byte array.
    /// </summary>
    public async Task<CoordinateSystemResponse> ImportCSDFromDCContentAsync(string filePath, byte[] fileContent)
    {
      try
      {
        var uniqueIdentifier = $"{nameof(ImportCSDFromDCContentAsync)}-{filePath}-{GenerateHash(fileContent)}";
        var cacheKey = GenerateKey(uniqueIdentifier);
        if (_cache.TryGetValue<CoordinateSystemResponse>(cacheKey, out var cachedItem))
          return cachedItem;
        
        using (var content = new MultipartFormDataContent("Upload----" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)))
        {
          content.Add(new StreamContent(new MemoryStream(fileContent)), "DC", Path.GetFileName(filePath));

          var response = await serviceHttpClient.SendRequest("/coordinatesystems/imports/dc/file", content);
          if (!response.IsSuccessStatusCode)
          {
            throw new Exception(response.ToString());
          }

          var json = await response.Content.ReadAsStringAsync();
          var csList = JsonConvert.DeserializeObject<IEnumerable<CoordinateSystemResponse>>(json);

          var result = csList.FirstOrDefault();
          _cache.Set(uniqueIdentifier, result, _cacheTimeout);
          return result;
        }
      }
      catch (Exception exception)
      {
        log.LogError(exception, $"Failed to import coordinate system definition from DC '{filePath}'");
      }

      return new CoordinateSystemResponse();
    }

    /// <summary>
    /// Extracts a CSIB from a DC file presented as a byte array.
    /// </summary>
    public async Task<string> ImportFromDCContentAsync(string filePath, byte[] fileContent)
    {
      var csd = await ImportCSDFromDCContentAsync(filePath, fileContent);

      return csd.CoordinateSystem.Id;
    }

    /// <summary>
    /// Extracts a coordinate system defintion response object from a CSIB string.
    /// </summary>
    public async Task<CoordinateSystemResponse> ImportCSDFromCSIBAsync(string csib)
    {
      try
      {
        var uniqueIdentifier = $"{nameof(ImportCSDFromCSIBAsync)}-{csib}";
        var cacheKey = GenerateKey(uniqueIdentifier);
        if (_cache.TryGetValue<CoordinateSystemResponse>(cacheKey, out var cachedItem))
          return cachedItem;

        using (var content = new MultipartFormDataContent("Upload----" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)))
        {
          var response = await serviceHttpClient.SendRequest($"/coordinatesystems/byId?id={csib}", HttpMethod.Get);
          if (!response.IsSuccessStatusCode)
          {
            throw new Exception(response.ToString());
          }

          var json = await response.Content.ReadAsStringAsync();
          var result = JsonConvert.DeserializeObject<CoordinateSystemResponse>(json);
          _cache.Set(uniqueIdentifier, result, _cacheTimeout);
          return result;
        }
      }
      catch (Exception exception)
      {
        log.LogError(exception, "Failed to import coordinate system definition from CSIB");
      }

      return new CoordinateSystemResponse();
    }

    private string GenerateHash(byte[] data)
    {
      using (var md5 = MD5.Create())
      {
        md5.Initialize();
        var hash = md5.ComputeHash(data);
        var sb = new StringBuilder();
        foreach (var b in hash)
        {
          sb.Append(b.ToString("X2"));
        }

        return sb.ToString();
      }
    }

    private string GenerateKey(string data)
    {
      return GenerateHash(Encoding.UTF8.GetBytes(data));
    }

  }
}
