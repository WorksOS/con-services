using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.WebApi.Common;

namespace TCCToDataOcean
{
  /// <summary>
  /// This base class provides utility methods for performing an HTTP Request 
  /// of all HTTP Method Operations: GET, PUT, POST, DELETE
  /// This base class provides utilty methods for validation of the response status codes,
  /// response headers, and response contents
  /// </summary>
  public class RestClientUtil
  {
    private readonly ILogger Log;
    private readonly ITPaaSApplicationAuthentication Authentication;

    public RestClientUtil(ILoggerFactory loggerFactory, ITPaaSApplicationAuthentication authentication)
    {
      Log = loggerFactory.CreateLogger<RestClientUtil>();
      Authentication = authentication;
    }

    /// <summary>
    /// This method performs a valid HTTP GET, PUT, or POST request on an RESTful endpoint and stores the response in a string for later parsing
    /// This method is overloaded as it doesn't require username and password authentication
    /// </summary>
    /// <param name="resourceUri">This is the resource on the endpoint.This includes the full endpoint URI.</param>
    /// <param name="httpMethod">This is the HTTP Method: GET, PUT, POST</param>
    /// <param name="payloadData">This is the actual data to be used within an HTTP PUT or HTTP POST</param>
    /// <param name="mediaType">This is the mediaType of the HTTP request which can be json or xml </param>    
    /// <param name="customerUid">This is the customer UID for the header</param>
    /// <returns></returns>
    public string DoHttpRequest(string resourceUri, string httpMethod, string payloadData, string mediaType = "application/json", string customerUid = null)
    {
      Log.LogInformation(resourceUri);
      var msg = new Msg();
      var request = InitHttpRequest(resourceUri, httpMethod, mediaType, customerUid, Authentication.GetApplicationBearerToken());                   
      {
        request.ContentType = mediaType;
        var writeStream = request.GetRequestStreamAsync().Result;
        UTF8Encoding encoding = new UTF8Encoding();
        byte[] bytes = encoding.GetBytes(payloadData);
        writeStream.Write(bytes, 0, bytes.Length);
      }

      //Validate the HTTP Response Status Codes for Successful POST HTTP Request
      try
      {
        string responseString = null;
        Console.WriteLine("Call Web API=" + resourceUri);
        using (var response = (HttpWebResponse)request.GetResponseAsync().Result)
        {
          responseString = GetStringFromResponseStream(response);
          msg.DisplayWebApi(httpMethod, resourceUri, responseString, payloadData);
        }
        return responseString;
      }
      catch (AggregateException ex)
      {
        foreach (var e in ex.InnerExceptions)
        {
          if (!(e is WebException)) continue;
          var webException = (WebException)e;
          var response = webException.Response as HttpWebResponse;
          if (response == null) continue;
          var resp = GetStringFromResponseStream(response);
          msg.DisplayWebApi(httpMethod, resourceUri, resp, payloadData);
          return resp;
        }
        Console.WriteLine(ex.InnerException.Message);
        msg.DisplayException(ex.Message);
        return string.Empty;
      }
    }

    /// <summary>
    /// Get the HTTP Response from the response stream and store in a string variable
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    private string GetStringFromResponseStream(HttpWebResponse response)
    {
      var readStream = response.GetResponseStream();

      if (readStream != null)
      {
        var reader = new StreamReader(readStream, Encoding.UTF8);
        var responseString = reader.ReadToEnd();
        return responseString;
      }
      return string.Empty;
    }

    /// <summary>
    /// Overloaded (no auth): This method sets the Http Request Method, Header, Media Type, and Authentication
    /// </summary>
    /// <param name="resourceUri">This is the resource on the endpoint.This includes the full endpoint URI.</param>
    /// <param name="httpMethod">This is the HTTP Method: GET, PUT, POST</param>
    /// <param name="mediaType">This is the mediaType of the http request which can be json or xml </param>
    /// <param name="customerUid">This is the customer UID for the header for authentication </param>
    /// <param name="bearerToken">This is the bearer token for the header for authentication </param>
    /// <returns>This returns the HTTP request</returns>
    private HttpWebRequest InitHttpRequest(string resourceUri, string httpMethod, string mediaType, string customerUid, string bearerToken)
    {
      //Initialize the Http Request
      var request = (HttpWebRequest)WebRequest.Create(resourceUri);
      request.Method = httpMethod;
      request.Accept = mediaType;
      request.Headers["X-VisionLink-CustomerUid"] = customerUid;
      //request.Headers["X-VisionLink-ClearCache"] = "true";
      request.Headers["Authorization"] = bearerToken;
      return request;
    }

  }
}
