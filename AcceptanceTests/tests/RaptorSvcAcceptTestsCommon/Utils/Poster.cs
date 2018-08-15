using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestAPICoreTestFramework.Utils.Common;

namespace RaptorSvcAcceptTestsCommon.Utils
{
  /// <summary>
  /// A generic class used for POSTing Web APIs.
  /// </summary>
  /// <typeparam name="TRequest">Type of the request sent.</typeparam>
  /// <typeparam name="TResponse">Type of the service response.</typeparam>
  public class Poster<TRequest, TResponse>
  {
    public Dictionary<string, TRequest> RequestRepo { get; }
    public Dictionary<string, TResponse> ResponseRepo { get; }
    public string Uri { get; set; }
    public TRequest CurrentRequest { get; set; }
    public TResponse CurrentResponse { get; private set; }
    public ServiceResponse CurrentServiceResponse { get; private set; }

    private static readonly string TestDatapath;

    static Poster()
    {
      TestDatapath = DirectoryAgent.TraverseParentDirectories("testdata");
    }

    public Poster(string uri, TRequest request)
    {
      RequestRepo = null;
      ResponseRepo = null;

      Uri = uri;
      CurrentRequest = request;
      CurrentResponse = default(TResponse);
      CurrentServiceResponse = null;
    }

    /// <summary>
    /// Construct a service POSTer
    /// </summary>
    /// <param name="uri">URI of the service</param>
    /// <param name="requestFile">Name (with full path) of the JSON file containing request body contents.</param>
    /// <param name="responseFile">Name (with full path) of the JSON file containing expected response contents.</param>
    public Poster(string uri, string requestFile = null, string responseFile = null)
    {
      RequestRepo = null;
      ResponseRepo = null;

      Uri = uri;
      CurrentRequest = default(TRequest);
      CurrentResponse = default(TResponse);
      CurrentServiceResponse = null;

      try
      {
        if (requestFile != null)
        {
          var path = Path.Combine(TestDatapath, requestFile);

          using (var file = File.OpenText(path))
          {
            var serializer = new JsonSerializer();
            RequestRepo = (Dictionary<string, TRequest>)serializer.Deserialize(file, typeof(Dictionary<string, TRequest>));
          }
        }

        if (responseFile != null)
        {
          var path = Path.Combine(TestDatapath, responseFile);

          using (var file = File.OpenText(path))
          {
            var serializer = new JsonSerializer();
            ResponseRepo = (Dictionary<string, TResponse>)serializer.Deserialize(file, typeof(Dictionary<string, TResponse>));
          }
        }
      }
      catch (Exception e)
      {
        Logger.Error(e.Message, Logger.ContentType.Error);
        throw;
      }
    }

    /// <summary>
    /// Do an HTTP POST request - expecting success e.g. 200 OK.
    /// </summary>
    /// <param name="requestName">Request name as appears in the request JSON file. If not supplied, use CurrentRequest.</param>
    /// <param name="expectedHttpCode">Expected response HttpStatusCode - default to 200 OK.</param>
    /// <returns>Request response.</returns>
    public TResponse DoValidRequest(string requestName = null, HttpStatusCode expectedHttpCode = HttpStatusCode.OK)
    {
      return DoRequest(requestName, expectedHttpCode);
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
      return DoRequest(null, expectedHttpCode);
    }

    /// <summary>
    /// Do an HTTP POST request - expecting failure e.g. 400 BadRequest.
    /// </summary>
    /// <param name="requestName">Request name as appears in the request JSON file. If not supplied, use CurrentRequest.</param>
    /// <param name="expectedHttpCode">Expected HTTP error code - default to 400 BadRequest.</param>
    /// <returns>Request response.</returns>
    public TResponse DoInvalidRequest(string requestName = null, HttpStatusCode expectedHttpCode = HttpStatusCode.BadRequest)
    {
      return DoRequest(requestName, expectedHttpCode);
    }

    /// <summary>
    /// Do an HTTP POST request - expecting failure e.g. 400 BadRequest.
    /// </summary>
    /// <param name="request">Request object to be POST'ed.</param>
    /// <param name="expectedHttpCode">Expected HTTP error code - default to 400 BadRequest.</param>
    /// <returns>Request response.</returns>
    public TResponse DoInvalidRequest(TRequest request, HttpStatusCode expectedHttpCode = HttpStatusCode.BadRequest)
    {
      CurrentRequest = request;
      return DoRequest(null, expectedHttpCode);
    }

    /// <summary>
    /// Do an HTTP POST request - expecting failure e.g. 400 BadRequest.
    /// This method is identical to DoValidRequest(HttpStatusCode expectedHttpCode). 
    /// The name is made different to make the request purpose more explicit.
    /// </summary>
    /// <param name="expectedHttpCode"></param>
    /// <returns>Expected failure response HttpStatusCode e.g. 400 BadRequest.</returns>
    public TResponse DoInvalidRequest(HttpStatusCode expectedHttpCode)
    {
      return DoRequest(null, expectedHttpCode);
    }

    /// <summary>
    /// Implementation for public methods DoValidRequest and DoInvalidRequest.
    /// </summary>
    /// <param name="requestName">Request name as appears in the request JSON file. If null, use CurrentRequest.</param>
    /// <param name="expectedHttpCode">Expected HTTP code.</param>
    /// <returns>Request response.</returns>
    private TResponse DoRequest(string requestName, HttpStatusCode expectedHttpCode)
    {
      string requestBodyString;

      if (requestName == null)
        requestBodyString = JsonConvert.SerializeObject(CurrentRequest);
      else if (requestName.Length > 0)
        requestBodyString = JsonConvert.SerializeObject(RequestRepo[requestName]);
      else
        requestBodyString = "";

      CurrentServiceResponse = RaptorServicesClientUtil.DoHttpRequest(Uri, "POST",
          RestClientConfig.JsonMediaType, requestBodyString);

      if (CurrentServiceResponse == null)
      {
        return default(TResponse);
      }

      if (expectedHttpCode != CurrentServiceResponse.HttpCode)
      {
        Logger.Error($"Expected {expectedHttpCode}, but got {CurrentServiceResponse.HttpCode} instead.", Logger.ContentType.Error);
      }

      Assert.AreEqual(expectedHttpCode, CurrentServiceResponse.HttpCode,
        $"Expected {expectedHttpCode}, but got {CurrentServiceResponse.HttpCode} instead. Message was {CurrentServiceResponse.ResponseBody}");

      CurrentResponse = JsonConvert.DeserializeObject<TResponse>(CurrentServiceResponse.ResponseBody);

      return CurrentResponse;
    }
  }
}
