using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestAPICoreTestFramework.Utils.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace RaptorSvcAcceptTestsCommon.Utils
{
  /// <summary>
  /// Methods here come from RestAPICoreFramework with some modifications.
  /// </summary>
  public class RaptorServicesClientUtil : RestClientUtil
  {
    /// <summary>
    /// This method performs a HTTP GET, PUT, or POST request on an RESTful endpoint and returns HttpWebResponse.
    /// No validation is done here, it simply returns whatever the response is.
    /// </summary>
    /// <param name="resourceUri">This is the resource on the endpoint.This includes the full endpoint URI.</param>
    /// <param name="httpMethod">This is the HTTP Method: GET, PUT, POST</param>
    /// <param name="contentType">This is the contentType of the HTTP request</param>
    /// <param name="acceptType">This is the acceptType of the HTTP request</param>
    /// <param name="payloadData">This is the actual data to be used within an HTTP PUT or HTTP POST</param>
    /// <returns>HttpWebResponse</returns>
    public static HttpWebResponse DoHttpRequest(string resourceUri, string httpMethod,
        string contentType, string acceptType, string payloadData)
    {
      Stream writeStream = null;
      HttpWebResponse httpResponse = null;

      //Initialize the Http Request
      Console.WriteLine("resourceURL:" + resourceUri);
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resourceUri);
      Console.WriteLine("After HttpWebRequest request");
      request.Headers = Auth.HeaderWithAuth;
      request.KeepAlive = true;  // Somehow need to set this as false to avoid Server Protocol Violation excpetion
      request.Method = httpMethod;
      request.Accept = acceptType;

      // Logging
      Logger.Info(resourceUri, Logger.ContentType.URI);
      Logger.Info(httpMethod, Logger.ContentType.HttpMethod);
      Logger.Info(string.IsNullOrEmpty(request.Headers.ToString()) ? request.Headers.ToString() :
          request.Headers.ToString().Replace(Environment.NewLine, ","), Logger.ContentType.RequestHeader);
      Logger.Info(string.IsNullOrEmpty(payloadData) ? payloadData : payloadData.Replace(Environment.NewLine, ","), Logger.ContentType.Request);

      //Perform the PUT or POST request with the payload
      if (payloadData != null)
      {
        request.ContentType = contentType;

        writeStream = request.GetRequestStream();
        UTF8Encoding encoding = new UTF8Encoding();
        byte[] bytes = encoding.GetBytes(payloadData);
        writeStream.Write(bytes, 0, bytes.Length);
      }

      try
      {
        WebResponse response = request.GetResponse();
        httpResponse = (HttpWebResponse)response;
      }
      catch (WebException e)
      {
        WebResponse response = e.Response;

        if (response != null)
          httpResponse = (HttpWebResponse)response;
        else
          Logger.Error(e.Message, Logger.ContentType.Error);
      }
      finally
      {
        //Dispose, flush and close the streams
        if (writeStream != null)
        {
          writeStream.Dispose();
          writeStream.Flush();
          writeStream.Close();
        }
      }

      return httpResponse;
    }

    /// <summary>
    /// This method performs a HTTP GET, PUT, or POST request on an RESTful endpoint and returns ServiceResponse(HttpStatusCode and 
    /// response body string). No validation is done here, it simply returns whatever the response is.
    /// </summary>
    /// <param name="resourceUri">This is the resource on the endpoint.This includes the full endpoint URI.</param>
    /// <param name="httpMethod">This is the HTTP Method: GET, PUT, POST</param>
    /// <param name="mediaType">This is the mediaType of the HTTP request which can be json or xml </param>
    /// <param name="payloadData">This is the actual data to be used within an HTTP PUT or HTTP POST</param>
    /// <returns>ServiceResponse(Response http status code - 200, 400 etc. + response body string)</returns>
    public static ServiceResponse DoHttpRequest(string resourceUri, string httpMethod,
        string mediaType, string payloadData)
    {
      var httpResponse = DoHttpRequest(resourceUri, httpMethod, mediaType, mediaType, payloadData);

      if (httpResponse != null)
      {
        var responseHeader = httpResponse.Headers;
        var httpResponseCode = httpResponse.StatusCode;

        // Get the response body string for debug message
        string responseString;
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
          responseString = streamReader.ReadToEnd();

        // Logging
        Logger.Info(string.IsNullOrEmpty(responseHeader.ToString()) ? responseHeader.ToString() :
            responseHeader.ToString().Replace(Environment.NewLine, ","), Logger.ContentType.ResponseHeader);
        Logger.Info(httpResponse.StatusCode.ToString(), Logger.ContentType.HttpCode);
        Logger.Info(responseString, Logger.ContentType.Response);

        httpResponse.Close();

        return new ServiceResponse
        {
          ResponseHeader = responseHeader,
          HttpCode = httpResponseCode,
          ResponseBody = responseString
        };
      }

      return null;
    }
  }

  /// <summary>
  /// Represent an HTTP request requesponse
  /// </summary>
  public class ServiceResponse
  {
    public WebHeaderCollection ResponseHeader { get; set; }
    public HttpStatusCode HttpCode { get; set; }
    public string ResponseBody { get; set; }
  }

  /// <summary>
  /// A generic class used for POSTing Web APIs.
  /// </summary>
  /// <typeparam name="TRequest">Type of the request sent.</typeparam>
  /// <typeparam name="TResponse">Type of the service response.</typeparam>
  public class Poster<TRequest, TResponse>
  {
    #region Members
    public Dictionary<string, TRequest> RequestRepo { get; }
    public Dictionary<string, TResponse> ResponseRepo { get; }
    public string Uri { get; set; }
    public TRequest CurrentRequest { get; set; }
    public TResponse CurrentResponse { get; private set; }
    public ServiceResponse CurrentServiceResponse { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Construct a service POSTer
    /// </summary>
    /// <param name="uri">URI of the service</param>
    /// <param name="requestFile">Name (with full path) of the JSON file containing request body contents.</param>
    /// <param name="responseFile">Name (with full path) of the JSON file containing expected response contents.</param>
    public Poster(string uri, string requestFile = null, string responseFile = null)
    {
      this.RequestRepo = null;
      this.ResponseRepo = null;

      this.Uri = uri;
      this.CurrentRequest = default(TRequest);
      this.CurrentResponse = default(TResponse);
      this.CurrentServiceResponse = null;

      try
      {
        if (requestFile != null)
        {
          using (StreamReader file = File.OpenText(RaptorClientConfig.TestDataPath + requestFile))
          {
            JsonSerializer serializer = new JsonSerializer();
            RequestRepo = (Dictionary<string, TRequest>)serializer.Deserialize(file,
                typeof(Dictionary<string, TRequest>));
          }
        }

        if (responseFile != null)
        {
          using (StreamReader file = File.OpenText(RaptorClientConfig.TestDataPath + responseFile))
          {
            JsonSerializer serializer = new JsonSerializer();
            ResponseRepo = (Dictionary<string, TResponse>)serializer.Deserialize(file,
                typeof(Dictionary<string, TResponse>));
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
    /// Construct a service POSTer
    /// </summary>
    /// <param name="uri">URI of the service</param>
    /// <param name="request">Object to be POST'ed</param>
    /// <param name="expectedResponse">Expected response object</param>
    public Poster(string uri, TRequest request)
    {
      this.RequestRepo = null;
      this.ResponseRepo = null;

      this.Uri = uri;
      this.CurrentRequest = request;
      this.CurrentResponse = default(TResponse);
      this.CurrentServiceResponse = null;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public Poster()
    { }
    #endregion

    #region Methods
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
      this.CurrentRequest = request;
      return DoRequest(null, expectedHttpCode);
    }

    /// <summary>
    /// Do an HTTP POST request sending CurrentRequest - expecting success e.g. 200 OK.
    /// </summary>
    /// <param name="expectedHttpCode">Expected success response HttpStatusCode e.g. 200 OK.</param>
    /// <returns></returns>
    public TResponse DoValidRequest(HttpStatusCode expectedHttpCode)
    {
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
      this.CurrentRequest = request;
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
        requestBodyString = JsonConvert.SerializeObject(this.CurrentRequest);
      else if (requestName.Length > 0)
        requestBodyString = JsonConvert.SerializeObject(this.RequestRepo[requestName]);
      else
        requestBodyString = "";

      this.CurrentServiceResponse = RaptorServicesClientUtil.DoHttpRequest(Uri, "POST",
          RestClientConfig.JsonMediaType, requestBodyString);

      if (this.CurrentServiceResponse != null)
      {
        if (expectedHttpCode != this.CurrentServiceResponse.HttpCode)
        {
          Logger.Error($"Expected {expectedHttpCode}, but got {this.CurrentServiceResponse.HttpCode} instead.",
              Logger.ContentType.Error);
        }

        Assert.AreEqual(expectedHttpCode, this.CurrentServiceResponse.HttpCode,
          $"Expected {expectedHttpCode}, but got {this.CurrentServiceResponse.HttpCode} instead. Message was {this.CurrentServiceResponse.ResponseBody}");

        this.CurrentResponse = JsonConvert.DeserializeObject<TResponse>(this.CurrentServiceResponse.ResponseBody);

        return this.CurrentResponse;
      }

      return default(TResponse);
    }

    /// <summary>
    /// Utility method for creating a poster and doing a valid request
    /// </summary>
    public static Poster<T, U> PostIt<T, U>(T request, string url)
    {
      Poster<T, U> poster = new Poster<T, U>(url, request);
      poster.DoValidRequest();
      return poster;
    }

    /// <summary>
    /// Utility method for comparing actual response with expected response
    /// </summary>
    public void CompareIt<U>(string multilineText)
    {
      U expected = JsonConvert.DeserializeObject<U>(multilineText);
      Assert.AreEqual(expected, this.CurrentResponse);
    }
    #endregion
  }

  /// <summary>
  /// A generic class used for GETTing Web APIs.
  /// </summary>
  /// <typeparam name="TResponse">Type of the service response.</typeparam>
  public class Getter<TResponse>
  {
    #region Members
    public Dictionary<string, TResponse> ResponseRepo { get; }
    public string Uri { get; set; }
    public Dictionary<string, string> QueryString { get; set; }
    public TResponse CurrentResponse { get; private set; }
    public ServiceResponse CurrentServiceResponse { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Construct service GETTer.
    /// </summary>
    /// <param name="uri">URI of the service</param>
    /// <param name="responseFile">Name (with full path) of the JSON file containing expected response contents.</param>
    public Getter(string uri, string responseFile = null)
    {
      this.ResponseRepo = null;

      this.Uri = uri;
      this.CurrentResponse = default(TResponse);
      this.CurrentServiceResponse = null;
      this.QueryString = new Dictionary<string, string>();

      try
      {
        if (responseFile != null)
        {
          using (StreamReader file = File.OpenText(RaptorClientConfig.TestDataPath + responseFile))
          {
            JsonSerializer serializer = new JsonSerializer();
            ResponseRepo = (Dictionary<string, TResponse>)serializer.Deserialize(file,
                typeof(Dictionary<string, TResponse>));
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
    /// Default constructor
    /// </summary>
    public Getter()
    { }
    #endregion

    #region Methods
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
      return DoRequest(this.Uri, expectedHttpCode);
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
      return DoRequest(this.Uri, expectedHttpCode);
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
        this.Uri = uri;
      }

      if (QueryString != null)
      {
        this.Uri += BuildQueryString();
      }


      this.CurrentServiceResponse = RaptorServicesClientUtil.DoHttpRequest(this.Uri, "GET",
              RestClientConfig.JsonMediaType, null);

      if (this.CurrentServiceResponse != null)
      {
        if (expectedHttpCode != this.CurrentServiceResponse.HttpCode)
        {
          Logger.Error($"Expected {expectedHttpCode}, but got {this.CurrentServiceResponse.HttpCode} instead.",
              Logger.ContentType.Error);
        }

        Assert.AreEqual(expectedHttpCode, this.CurrentServiceResponse.HttpCode,
          $"Expected {expectedHttpCode}, but got {this.CurrentServiceResponse.HttpCode} instead.");

        this.CurrentResponse = JsonConvert.DeserializeObject<TResponse>(this.CurrentServiceResponse.ResponseBody);
        return this.CurrentResponse;
      }

      return default(TResponse);
    }

    private string BuildQueryString()
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

    /// <summary>
    /// Utility method for creating a getter and doing a valid request
    /// </summary>
    public static Getter<T> GetIt<T>(string url, string projectUid, string queryParameters = null)
    {
      url = $"{url}?projectUid={projectUid}";
      if (!string.IsNullOrEmpty(queryParameters))
        url += queryParameters;
      Getter<T> getter = new Getter<T>(url);
      getter.DoValidRequest();
      return getter;
    }

    /// <summary>
    /// Utility method for comparing actual response with expected response
    /// </summary>
    public void CompareIt<T>(string multilineText)
    {
      T expected = JsonConvert.DeserializeObject<T>(multilineText);
      Assert.AreEqual(expected, this.CurrentResponse);
    }
    #endregion
  }





  /// <summary>
  /// A mini logger class
  /// </summary>
  public static class Logger
  {
    public enum ContentType
    {
      URI,
      HttpMethod,
      Request,
      HttpCode,
      Response,
      Error,
      RequestHeader,
      ResponseHeader
    }

    public static void Error(string message, ContentType contentType)
    {
      WriteEntry(message, "Error", Enum.GetName(typeof(ContentType), contentType));
    }

    public static void Error(Exception ex, ContentType contentType)
    {
      WriteEntry(ex.Message, "Error", Enum.GetName(typeof(ContentType), contentType));
    }

    public static void Warning(string message, ContentType contentType)
    {
      WriteEntry(message, "Warning", Enum.GetName(typeof(ContentType), contentType));
    }

    public static void Info(string message, ContentType contentType)
    {
      WriteEntry(message, "Info", Enum.GetName(typeof(ContentType), contentType));
    }

    private static void WriteEntry(string message, string logType, string contentType)
    {
      string contents = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},[{logType}],[{contentType}],{message}";

      using (StreamWriter w = File.AppendText("Log.txt"))
      {
        w.WriteLine(contents);
      }

      // Workaround to Teamcity
      File.Copy("Log.txt", "..\\..\\Log.txt", true);
    }
  }
}