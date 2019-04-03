using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using WebApiTests.Extensions;
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
    public ServiceResponse CurrentServiceResponse { get; }

    public HttpResponseMessage HttpResponseMessage { get; private set; }
    public byte[] ByteContent { get; private set; }

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

      var path = Path.Combine(TestDatapath, responseFile);

      Console.WriteLine(path);
      using (var file = File.OpenText(path))
      {
        var serializer = new JsonSerializer();
        ResponseRepo = (Dictionary<string, TResponse>)serializer.Deserialize(file, typeof(Dictionary<string, TResponse>));
      }
    }

    /// <summary>
    /// Do an HTTP GET request - expecting success e.g. 200 OK.
    /// </summary>
    /// <param name="expectedHttpCode">Expected response HttpStatusCode.</param>
    /// <returns>Request response.</returns>
    public TResponse DoValidRequest(HttpStatusCode expectedHttpCode)
    {
      return SendRequest(Uri, expectedHttpCode);
    }

    /// <summary>
    /// Do an HTTP GET request - expecting failure e.g. 400 BadRequest.
    /// </summary>
    /// <param name="expectedHttpCode">Expected response HttpStatusCode.</param>
    /// <returns>Request response.</returns>
    public TResponse DoInvalidRequest(HttpStatusCode expectedHttpCode)
    {
      return SendRequest(Uri, expectedHttpCode);
    }

    public TResponse SendRequest(string url = "", HttpStatusCode expectedHttpCode = HttpStatusCode.OK, string acceptHeader = MediaTypes.JSON)
    {
      if (string.IsNullOrEmpty(url))
      {
        url = Uri;
      }

      HttpResponseMessage = RestClient.SendHttpClientRequest(
        url,
        BuildQueryString(),
        HttpMethod.Get,
        acceptHeader,
        MediaTypes.JSON,
        null).Result;

      Assert.Equal(expectedHttpCode, HttpResponseMessage.StatusCode);

      var receiveStream = HttpResponseMessage.Content.ReadAsStreamAsync().Result;
      var readStream = new StreamReader(receiveStream, Encoding.UTF8);
      var responseBody = readStream.ReadToEnd();

      if (!HttpResponseMessage.IsSuccessStatusCode)
      {
        CurrentResponse = JsonConvert.DeserializeObject<TResponse>(responseBody, new JsonSerializerSettings
        {
          Formatting = Formatting.Indented
        });

        return CurrentResponse;
      }

      switch (HttpResponseMessage.Content.Headers.ContentType.MediaType)
      {
        case MediaTypes.JSON:
          {
            CurrentResponse = JsonConvert.DeserializeObject<TResponse>(responseBody, new JsonSerializerSettings
            {
              Formatting = Formatting.Indented
            });

            break;
          }
        case MediaTypes.PNG:
          {
            ByteContent = HttpResponseMessage.Content.ReadAsStreamAsync().Result.ToByteArray();
            break;
          }
        case MediaTypes.ZIP:
        case MediaTypes.PROTOBUF:
          {
            ByteContent = HttpResponseMessage.Content.ReadAsByteArrayAsync().Result;
            break;
          }
        default:
          {
            Assert.True(false, $"Unsupported ContentType to request: {url}");
            break;
          }
      }

      return CurrentResponse;
    }

    public string BuildQueryString()
    {
      var queryString = new StringBuilder();
      var firstparam = true;

      if (QueryString != null)
      {
        foreach (var parameter in QueryString.Keys)
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
  }
}
