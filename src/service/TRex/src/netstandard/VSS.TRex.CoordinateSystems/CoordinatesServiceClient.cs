using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.TRex.HttpClients.Constants;
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
        var route = "/coordinates/nee/Orientated/fromLLH" +
                    $"?Type=ReferenceGlobal&Latitude={coordinates.Latitude}&Longitude={coordinates.Longitude}&Height={coordinates.Height}" +
                    $"&fromCoordinateSystemId={id}";

        var response = serviceHttpClient.SendRequest(route, HttpMethod.Get).Result;

        if (response.IsSuccessStatusCode)
        {
          var neeStr = await response.Content.ReadAsStringAsync();

          return JsonConvert.DeserializeObject<NEE>(neeStr);
        }
      }
      catch (Exception exception)
      {
        log.LogError($"Failed to get NEE for lat={coordinates.Latitude}, lon={coordinates.Longitude}, height={coordinates.Height}, Exception: {exception}");
      }

      return new NEE
      {
        East = Consts.NullDouble,
        North = Consts.NullDouble,
        Elevation = Consts.NullDouble
      };
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
        var response = serviceHttpClient.SendRequest(route, HttpMethod.Post, MediaTypes.JSON, requestObj).Result;

        if (response.IsSuccessStatusCode)
        {
          var neeStr = await response.Content.ReadAsStringAsync();
          var resultArray = JsonConvert.DeserializeObject<double[,]>(neeStr);

          return (RequestErrorStatus.OK, resultArray.ToNEEArray());
        }
      }
      catch (Exception exception)
      {
        log.LogError($"Failed to get NEE for lat array, Exception: {exception}");
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

        var response = serviceHttpClient.SendRequest(route, HttpMethod.Get).Result;

        if (response.IsSuccessStatusCode)
        {
          var llhStr = await response.Content.ReadAsStringAsync();

          return JsonConvert.DeserializeObject<LLH>(llhStr);
        }
      }
      catch (Exception exception)
      {
        log.LogError($"Failed to get LLH for east={coordinates.East}, north={coordinates.North}, elevation={coordinates.Elevation}, Exception: {exception}");
      }

      return new LLH
      {
        Latitude = Consts.NullDouble,
        Longitude = Consts.NullDouble,
        Height = Consts.NullDouble
      };
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
        var response = serviceHttpClient.SendRequest(route, HttpMethod.Post, MediaTypes.JSON, requestObj).Result;

        if (response.IsSuccessStatusCode)
        {
          var llhStr = await response.Content.ReadAsStringAsync();
          var resultArray = JsonConvert.DeserializeObject<double[,]>(llhStr);

          return (RequestErrorStatus.OK, resultArray.ToLLHArray());
        }
      }
      catch (Exception exception)
      {
        log.LogError($"Failed to get coordinates, Exception: {exception}");
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
        log.LogError($"Failed to import coordinate system from DC {filePath}, Exception {exception}");
      }

      return null;
    }

    /// <summary>
    /// Extracts a CSIB from a DC file presented as a byte array.
    /// </summary>
    public async Task<string> ImportFromDCContentAsync(string filePath, byte[] fileContent)
    {
      string imported = null;

      try
      {
        using (var content = new MultipartFormDataContent("Upload----" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)))
        {
          content.Add(new StreamContent(new MemoryStream(fileContent)), "DC", Path.GetFileName(filePath));

          var response = serviceHttpClient.SendRequest("/coordinatesystems/imports/dc/file", content).Result;
          if (!response.IsSuccessStatusCode)
          {
            throw new Exception(response.ToString());
          }

          var json = await response.Content.ReadAsStringAsync();
          var csList = JsonConvert.DeserializeObject<IEnumerable<CoordinateSystemResponse>>(json);

          imported = csList?.FirstOrDefault().CoordinateSystem.Id;
        }
      }
      catch (Exception exception)
      {
        log.LogError($"Failed to import coordinate system from DC '{filePath}', Exception {exception}");
      }

      return imported;
    }
  }
}
