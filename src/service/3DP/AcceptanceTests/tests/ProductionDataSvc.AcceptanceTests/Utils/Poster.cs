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
  /// A generic class used for POSTing Web APIs.
  /// </summary>
  public class Poster<TRequest, TResponse> where TRequest : new()
  {
    public Dictionary<string, TRequest> RequestRepo { get; }
    public Dictionary<string, TResponse> ResponseRepo { get; }
    public string Uri { get; set; }
    public TRequest CurrentRequest { get; set; }
    public TResponse CurrentResponse { get; private set; }
    public HttpResponseMessage HttpResponseMessage { get; set; }
    public byte[] ByteContent { get; private set; }

    private readonly string testDatapath = DirectoryAgent.TraverseParentDirectories("testdata");

    public Poster(string uri, TRequest request)
    {
      Uri = uri;
      CurrentRequest = request;
      CurrentResponse = default(TResponse);
    }

    /// <summary>
    /// POST service constructor.
    /// </summary>
    /// <param name="uri">URI of the service</param>
    /// <param name="requestFile">Name (with full path) of the JSON file containing request body contents.</param>
    /// <param name="responseFile">Name (with full path) of the JSON file containing expected response contents.</param>
    public Poster(string uri, string requestFile = null, string responseFile = null)
    {
      Uri = uri;
      CurrentRequest = new TRequest();
      CurrentResponse = default(TResponse);
      HttpResponseMessage = null;

      if (requestFile != null)
      {
        var path = Path.Combine(testDatapath, requestFile);

        using (var file = File.OpenText(path))
        {
          var serializer = new JsonSerializer();
          RequestRepo = (Dictionary<string, TRequest>)serializer.Deserialize(file, typeof(Dictionary<string, TRequest>));
        }
      }

      if (responseFile != null)
      {
        var path = Path.Combine(testDatapath, responseFile);

        using (var file = File.OpenText(path))
        {
          var serializer = new JsonSerializer();
          ResponseRepo = (Dictionary<string, TResponse>)serializer.Deserialize(file, typeof(Dictionary<string, TResponse>));
        }
      }
    }

    /// <summary>
    /// Do an HTTP POST request
    /// </summary>
    /// <param name="requestName">Request name as appears in the request JSON file. If not supplied, use CurrentRequest.</param>
    /// <param name="expectedHttpCode">Expected response HttpStatusCode - default to 200 OK.</param>
    public TResponse DoRequest(string requestName = null, int expectedHttpCode = (int)HttpStatusCode.OK)
    {
      return SendRequest(requestName, (HttpStatusCode)expectedHttpCode);
    }

    /// <summary>
    /// Do an HTTP POST request - expecting success e.g. 200 OK.
    /// </summary>
    /// <param name="request">Request object to be POST'ed.</param>
    /// <param name="expectedHttpCode">Expected response HttpStatusCode - default to 200 OK.</param>
    /// <returns>Request response.</returns>
    public TResponse DoValidRequest(TRequest request, HttpStatusCode expectedHttpCode = HttpStatusCode.OK)
    {
      CurrentRequest = request;
      return SendRequest(null, expectedHttpCode);
    }

    /// <summary>
    /// Implementation for public methods SendRequest and DoRequest2.
    /// </summary>
    /// <param name="requestName">Request name as appears in the request JSON file. If null, use CurrentRequest.</param>
    /// <param name="expectedHttpCode">Expected HTTP code.</param>
    private TResponse SendRequest(string requestName, HttpStatusCode expectedHttpCode)
    {
      string requestBodyString;

      if (requestName == null)
      {
        requestBodyString = JsonConvert.SerializeObject(CurrentRequest);
      }
      else if (requestName.Length > 0)
      {
        requestBodyString = JsonConvert.SerializeObject(RequestRepo[requestName]);
      }
      else
      {
        requestBodyString = string.Empty;
      }

      HttpResponseMessage = RestClient.SendHttpClientRequest(
        Uri, 
        null, 
        HttpMethod.Post, 
        MediaTypes.JSON,
        MediaTypes.JSON,
        requestBodyString).Result;
      
      var receiveStream = HttpResponseMessage.Content.ReadAsStreamAsync().Result;
      var readStream = new StreamReader (receiveStream, Encoding.UTF8);
      var responseBody = readStream.ReadToEnd();

      Assert.True((int)expectedHttpCode == (int)HttpResponseMessage.StatusCode, responseBody);

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
          ByteContent = HttpResponseMessage.Content.ReadAsByteArrayAsync().Result;
          break;
        }
        default:
        {
          Assert.True(false, $"Unsupported ContentType to request: {Uri}");
          break;
        }
      }

      return CurrentResponse;
    }
  }
}
