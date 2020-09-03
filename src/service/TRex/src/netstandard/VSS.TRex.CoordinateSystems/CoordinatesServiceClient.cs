using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CoreXModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VSS.TRex.CoordinateSystems
{
  /// <summary>
  /// Implements a proxy httpClient to the Trimble Coordinates service
  /// </summary>
  public class CoordinatesServiceClient
  {
    public const string COORDINATE_SERVICE_URL_ENV_KEY = "COORDINATE_SERVICE_URL";

    private static readonly ILogger _log = Logging.Logger.CreateLogger<CoordinatesServiceClient>();
    private readonly CoordinateServiceHttpClient _serviceHttpClient;

    // Bug 81615 - We are adding a cache to Coordinate system calls until CoreX is done, as we are hitting rate limits when generating tiles
    // When CoreX is implemented, this cache will not be needed (nor will this class)
    private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly TimeSpan _cacheTimeout = new TimeSpan(0, 8, 0, 0);

    public CoordinatesServiceClient(HttpClient httpClient)
    {
      if (httpClient == null)
      {
        throw new NullReferenceException($"'{nameof(httpClient)}' cannot be null.");
      }

      _serviceHttpClient = new CoordinateServiceHttpClient(httpClient);
    }

    /// <summary>
    /// Extracts a coordinate system definition response object from a DC file presented as a byte array.
    /// </summary>
    public async Task<CoordinateSystemResponse> ImportCSDFromDCContentAsync(string filePath, byte[] fileContent)
    {
      try
      {
        var uniqueIdentifier = $"{nameof(ImportCSDFromDCContentAsync)}-{filePath}-{GenerateHash(fileContent)}";
        var cacheKey = GenerateKey(uniqueIdentifier);

        if (_cache.TryGetValue<CoordinateSystemResponse>(cacheKey, out var cachedItem))
        {
          return cachedItem;
        }

        using var content = new MultipartFormDataContent("Upload----" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
        {
          { new StreamContent(new MemoryStream(fileContent)), "file", Path.GetFileName(filePath) }
        };

        var response = await _serviceHttpClient.SendRequest("/coordinatesystems/imports/dc/file", content);

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
      catch (Exception exception)
      {
        _log.LogError(exception, $"Failed to import coordinate system definition from DC '{filePath}'");
      }

      return new CoordinateSystemResponse();
    }

    /// <summary>
    /// Extracts a coordinate system id from a CSIB string.
    /// </summary>
    public async Task<string> ImportCoordinateServiceIdFromCSIBAsync(string csib)
    {
      try
      {
        var uniqueIdentifier = $"{nameof(ImportCoordinateServiceIdFromCSIBAsync)}-{csib}";
        var cacheKey = GenerateKey(uniqueIdentifier);

        if (_cache.TryGetValue<string>(cacheKey, out var cachedItem))
        {
          return cachedItem;
        }

        var urlEncodedCsib = System.Net.WebUtility.UrlEncode(csib);

        using var content = new MultipartFormDataContent("Upload----" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
        var response = await _serviceHttpClient.SendRequest($"/coordinatesystems/imports/csib/base64?csib_64={urlEncodedCsib}", HttpMethod.Put);

        if (!response.IsSuccessStatusCode)
        {
          throw new Exception(response.ToString());
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<CoordinateSystemResponse>(json);

        _cache.Set(uniqueIdentifier, result.CoordinateSystem.Id, _cacheTimeout);

        return result.CoordinateSystem.Id;
      }
      catch (Exception exception)
      {
        _log.LogError(exception, "Failed to import coordinate system definition from CSIB");
      }

      return null;
    }

    /// <summary>
    /// Extracts a coordinate system definition response object from a CSIB string.
    /// </summary>
    public async Task<CoordinateSystemResponse> ImportCSDFromCoordinateySystemId(string csib)
    {
      try
      {
        var uniqueIdentifier = $"{nameof(ImportCSDFromCoordinateySystemId)}-{csib}";
        var cacheKey = GenerateKey(uniqueIdentifier);

        if (_cache.TryGetValue<CoordinateSystemResponse>(cacheKey, out var cachedItem))
        {
          return cachedItem;
        }

        using var content = new MultipartFormDataContent("Upload----" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
        var response = await _serviceHttpClient.SendRequest($"/coordinatesystems/byId?id={csib}", HttpMethod.Get);

        if (!response.IsSuccessStatusCode)
        {
          throw new Exception(response.ToString());
        }

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<CoordinateSystemResponse>(json);
        _cache.Set(uniqueIdentifier, result, _cacheTimeout);

        return result;
      }
      catch (Exception exception)
      {
        _log.LogError(exception, "Failed to import coordinate system definition from CSIB");
      }

      return new CoordinateSystemResponse();
    }

    private string GenerateHash(byte[] data)
    {
      using var md5 = MD5.Create();
      md5.Initialize();
      var hash = md5.ComputeHash(data);
      var sb = new StringBuilder();

      foreach (var b in hash)
      {
        sb.Append(b.ToString("X2"));
      }

      return sb.ToString();
    }

    private string GenerateKey(string data) => GenerateHash(Encoding.UTF8.GetBytes(data));
  }
}
