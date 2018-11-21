using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Xunit;

namespace ProductionDataSvc.AcceptanceTests.Utils
{
  /// <summary>
  /// A generic class used for GETTing Web APIs.
  /// </summary>
  public class Getter<TResponse>
  {
    public Dictionary<string, TResponse> ResponseRepo { get; }
    public string Uri { get; set; }
    public Dictionary<string, string> QueryString { get; }
    public HttpResponseMessage HttpResponseMessage { get; private set; }
    public TResponse CurrentResponse { get; private set; }
    public byte[] ByteContent { get; private set; }

    private readonly string testDatapath = DirectoryAgent.TraverseParentDirectories("testdata");

    public Getter(string uri, string responseFile = null)
    {
      ResponseRepo = null;
      Uri = uri;
      CurrentResponse = default(TResponse);
      ByteContent = null;
      HttpResponseMessage = null;
      QueryString = new Dictionary<string, string>();

      if (responseFile == null)
      {
        return;
      }

      using (var file = File.OpenText(Path.Combine(testDatapath, responseFile)))
      {
        var serializer = new JsonSerializer();
        ResponseRepo = (Dictionary<string, TResponse>)serializer.Deserialize(file, typeof(Dictionary<string, TResponse>));
      }
    }

    // TODO url cannot be optional
    public TResponse SendRequest(string url = "", int expectedHttpCode = 200, string acceptHeader = MediaTypes.JSON)
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

      Assert.Equal(expectedHttpCode, (int)HttpResponseMessage.StatusCode);

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

    /// <summary>
    /// Returns an aggregated query string 
    /// </summary>
    /// <returns></returns>
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
          queryString.Append(QueryString[parameter]);
        }
      }

      return queryString.ToString();
    }
  }
}
