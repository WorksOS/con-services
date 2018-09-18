using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Xunit;

namespace WebApiTests.Utilities
{
  /// <summary>
  /// A generic class used for GETTing Web APIs.
  /// </summary>
  /// <typeparam name="TResponse">Type of the service response.</typeparam>
  public class Getter<TResponse>
  {
    public Dictionary<string, TResponse> ResponseRepo { get; }
    public string Uri { get; set; }
    public Dictionary<string, string> QueryString { get; set; }
    public TResponse CurrentResponse { get; private set; }
    public ServiceResponse CurrentServiceResponse { get; private set; }

    public HttpResponseMessage HttpResponseMessage { get; private set; }

    private static readonly string TestDatapath;

    static Getter()
    {
      TestDatapath = DirectoryAgent.TraverseParentDirectories("TestData");
    }


    /// <summary>
    /// Construct service Getter.
    /// </summary>
    /// <param name="uri">URI of the service</param>
    /// <param name="responseFile">Name (with full path) of the JSON file containing expected response contents.</param>
    public Getter(string uri, string responseFile = null)
    {
      ResponseRepo = null;

      Uri = uri;
      CurrentResponse = default(TResponse);
      CurrentServiceResponse = null;
      QueryString = new Dictionary<string, string>();

      if (responseFile == null)
      {
        return;
      }

      try
      {
        var path = Path.Combine(TestDatapath, responseFile);

        Console.WriteLine(path);
        using (var file = File.OpenText(path))
        {
          var serializer = new JsonSerializer();
          ResponseRepo = (Dictionary<string, TResponse>)serializer.Deserialize(file, typeof(Dictionary<string, TResponse>));
        }
      }
      catch (Exception e)
      {
        Logger.Error(e.Message, Logger.ContentType.Error);
        throw;
      }
    }

    /// <summary>
    /// Do an HTTP GET request - expecting success e.g. 200 OK.
    /// </summary>
    /// <param name="uri">URI of the service.</param>
    /// <param name="expectedHttpCode">Expected response HttpStatusCode - default to 200 OK.</param>
    /// <returns>Request response.</returns>
    public TResponse DoValidRequest(string uri = "", HttpStatusCode expectedHttpCode = HttpStatusCode.OK)
    {
      return DoRequest(uri, expectedHttpCode);
    }

    /// <summary>
    /// Do an HTTP GET request - expecting success e.g. 200 OK.
    /// </summary>
    /// <param name="expectedHttpCode">Expected response HttpStatusCode.</param>
    /// <returns>Request response.</returns>
    public TResponse DoValidRequest(HttpStatusCode expectedHttpCode)
    {
      return DoRequest(Uri, expectedHttpCode);
    }

    /// <summary>
    /// Do an invalid GET request - expecting failure e.g. 400 BadRequest.
    /// </summary>
    /// <param name="uri">URI of the service.</param>
    /// <param name="expectedHttpCode">Expected response HttpStatusCode - default to 400 BadRequest.</param>
    /// <returns>Request response.</returns>
    public TResponse DoInvalidRequest(string uri = "", HttpStatusCode expectedHttpCode = HttpStatusCode.BadRequest)
    {
      return DoRequest(uri, expectedHttpCode);
    }

    /// <summary>
    /// Do an HTTP GET request - expecting failure e.g. 400 BadRequest.
    /// </summary>
    /// <param name="expectedHttpCode">Expected response HttpStatusCode.</param>
    /// <returns>Request response.</returns>
    public TResponse DoInvalidRequest(HttpStatusCode expectedHttpCode)
    {
      return DoRequest(Uri, expectedHttpCode);
    }

    /// <summary>
    /// Implementation for public methods DoValidRequest and DoInvalidRequest.
    /// </summary>
    /// <param name="uri">URI of the service.</param>
    /// <param name="expectedHttpCode">Expected response HttpStatusCode.</param>
    /// <returns>Request response.</returns>
    private TResponse DoRequest(string uri, HttpStatusCode expectedHttpCode)
    {
      if (uri != "")
      {
        Uri = uri;
      }

      if (QueryString != null)
      {
        Uri += BuildQueryString();
      }

      CurrentServiceResponse = TileClientUtil.DoHttpRequest(Uri, HttpMethod.Get.ToString(), TileClientUtil.JsonMediaType, null);

      if (CurrentServiceResponse == null)
      {
        Console.WriteLine("No HTTP response received");
        return default(TResponse);
      }

      if (expectedHttpCode != CurrentServiceResponse.HttpCode)
      {
        Logger.Error($"Expected {expectedHttpCode}, but got {CurrentServiceResponse.HttpCode} instead.",
          Logger.ContentType.Error);
      }

      Assert.Equal(expectedHttpCode, CurrentServiceResponse.HttpCode);

      CurrentResponse = JsonConvert.DeserializeObject<TResponse>(CurrentServiceResponse.ResponseBody, new JsonSerializerSettings
      {
        Formatting = Formatting.Indented
      });

      return CurrentResponse;
    }

   
    public byte[] DoRequestWithStreamResponse(string uri)
    {
      if (uri != "")
      {
        Uri = uri;
      }

      if (QueryString != null)
      {
        Uri += BuildQueryString();
      }

      var httpResponse = TileClientUtil.DoHttpRequest(Uri, HttpMethod.Get.ToString(), TileClientUtil.JsonMediaType, "image/png", null);

      return TileClientUtil.GetStreamContentsFromResponse(httpResponse);
    }

    public string BuildQueryString()
    {
      StringBuilder queryString = new StringBuilder();
      bool firstparam = true;
      if (QueryString != null)
      {
        foreach (string parameter in QueryString.Keys)
        {
          queryString.Append(firstparam ? "?" : "&");
          firstparam = false;
          queryString.Append(WebUtility.UrlEncode(parameter));
          queryString.Append("=");
          queryString.Append(WebUtility.UrlEncode(QueryString[parameter]));
        }
      }

      return queryString.ToString();
    }

    // TODO (Aaron) Make QueryString private and refactor step classes removing unnecesary string.isnullorempty() checks. Use AddQueryParam() instead.
    public bool AddQueryParam(string key, string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return false;
      }

      QueryString.Add(key, value);

      return true;
    }

    public void Send(string acceptHeader, string contentType, string content = null)
    {
      var response = TileClientUtil.SendHttpClientRequest(Uri, BuildQueryString(), HttpMethod.Get, acceptHeader, contentType, content).Result;
      HttpResponseMessage = response;
    }
  }
}
