using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestAPICoreTestFramework.Utils.Common;

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
    public static HttpWebResponse DoHttpRequest(string resourceUri, string httpMethod, string contentType, string acceptType, string payloadData)
    {
      Stream writeStream = null;
      HttpWebResponse httpResponse = null;

      //Initialize the Http Request
      Console.WriteLine("resourceURL:" + resourceUri);
      var request = (HttpWebRequest)WebRequest.Create(resourceUri);
      Console.WriteLine("After HttpWebRequest request");
      request.Headers = Auth.HeaderWithAuth;
      request.KeepAlive = true; // Somehow need to set this as false to avoid Server Protocol Violation excpetion
      request.Method = httpMethod;
      request.Accept = acceptType;

      // Logging
      Logger.Info(resourceUri, Logger.ContentType.URI);
      Logger.Info(httpMethod, Logger.ContentType.HttpMethod);
      Logger.Info(
        string.IsNullOrEmpty(request.Headers.ToString())
          ? request.Headers.ToString()
          : request.Headers.ToString().Replace(Environment.NewLine, ","), Logger.ContentType.RequestHeader);
      Logger.Info(string.IsNullOrEmpty(payloadData) ? payloadData : payloadData.Replace(Environment.NewLine, ","),
        Logger.ContentType.Request);

      //Perform the PUT or POST request with the payload
      if (payloadData != null)
      {
        request.ContentType = contentType;

        writeStream = request.GetRequestStream();
        var encoding = new UTF8Encoding();
        var bytes = encoding.GetBytes(payloadData);
        writeStream.Write(bytes, 0, bytes.Length);
      }

      try
      {
        var response = request.GetResponse();
        httpResponse = (HttpWebResponse)response;
      }
      catch (WebException e)
      {
        var response = e.Response;

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
    public static ServiceResponse DoHttpRequest(string resourceUri, string httpMethod, string mediaType, string payloadData)
    {
      var httpResponse = DoHttpRequest(resourceUri, httpMethod, mediaType, mediaType, payloadData);

      if (httpResponse == null)
      {
        return null;
      }

      var responseHeader = httpResponse.Headers;
      var httpResponseCode = httpResponse.StatusCode;

      // Get the response body string for debug message
      string responseString;

      using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
      {
        responseString = streamReader.ReadToEnd();
      }

      Logger.Info(string.IsNullOrEmpty(responseHeader.ToString())
        ? responseHeader.ToString()
        : responseHeader.ToString().Replace(Environment.NewLine, ","), Logger.ContentType.ResponseHeader);

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

    public static byte[] GetStreamContentsFromResponse(HttpWebResponse httpResponse)
    {
      byte[] fileContents = null;
      if (httpResponse != null)
      {
        Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode,
          $"Expected {HttpStatusCode.OK}, but got {httpResponse.StatusCode} instead.");

        using (var responseStream = httpResponse.GetResponseStream())
        using (var memoryStream = new MemoryStream())
        {
          int count;
          do
          {

            var buffer = new byte[1024];
            count = responseStream.Read(buffer, 0, buffer.Length);
            memoryStream.Write(buffer, 0, count);

          } while (count != 0);

          fileContents = memoryStream.ToArray();
        }

        httpResponse.Close();
      }
      return fileContents;
    }
  }
}
