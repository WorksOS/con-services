using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestAPICoreTestFramework.Utils.Common;

namespace RaptorSvcAcceptTestsCommon.Utils
{
  /// <summary>
  /// Methods here come from RestAPICoreFramework with some modifications.
  /// </summary>
  public class RaptorServicesClientUtil : RestClientUtil
  {
    public static async Task<HttpResponseMessage> SendHttpClientRequest(string uri, string route, HttpMethod method, string acceptHeader, string contentType, string payloadData)
    {
      Console.WriteLine("resourceURL:" + uri + route);

      var client = new HttpClient { BaseAddress = new Uri(uri) };
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));
      client.DefaultRequestHeaders.Add("X-VisionLink-CustomerUid", "87bdf851-44c5-e311-aa77-00505688274d");
      client.DefaultRequestHeaders.Add("X-JWT-Assertion", "eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6IjE0NTU1Nzc4MjM5MzAiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoxMDc5LCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IlV0aWxpemF0aW9uIERldmVsb3AgQ0kiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9udGllciI6IiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBpY29udGV4dCI6Ii90L3RyaW1ibGUuY29tL3V0aWxpemF0aW9uYWxwaGFlbmRwb2ludCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdmVyc2lvbiI6IjEuMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMva2V5dHlwZSI6IlBST0RVQ1RJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbmR1c2VyVGVuYW50SWQiOiIxIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbWFpbGFkZHJlc3MiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJDbGF5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9sYXN0bmFtZSI6IkFuZGVyc29uIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9vbmVUaW1lUGFzc3dvcmQiOm51bGwsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6IlN1YnNjcmliZXIscHVibGlzaGVyIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91dWlkIjoiMjM4ODY5YWYtY2E1Yy00NWUyLWI0ZjgtNzUwNjE1YzhhOGFiIn0=.kTaMf1IY83fPHqUHTtVHn6m6aQ9wFch6c0FsNDQ7x1k=");
      client.DefaultRequestHeaders.Add("X-VisionLink-ClearCache", "true");
      client.DefaultRequestHeaders.Add("pragma", "no-cache");

      Console.WriteLine("After HttpWebRequest request");

      Logger.Info(uri, Logger.ContentType.URI);
      Logger.Info(method.ToString(), Logger.ContentType.HttpMethod);
      Logger.Info(client.DefaultRequestHeaders.ToString().Replace(Environment.NewLine, ","), Logger.ContentType.RequestHeader);
      Logger.Info(string.IsNullOrEmpty(payloadData) ? payloadData : payloadData.Replace(Environment.NewLine, ","), Logger.ContentType.Request);
      
      var requestMessage = new HttpRequestMessage(method, route);

      if (payloadData != null)
      {
        requestMessage.Content = new StringContent(payloadData, Encoding.UTF8, contentType);
      }

      try
      {
        return await client.SendAsync(requestMessage);
      }
      catch (HttpRequestException e)
      {
        Logger.Error(e.Message, Logger.ContentType.Error);

        return null;
      }
    }
    

    /// <summary>
    /// This method performs a HTTP GET, PUT, or POST request on an RESTful endpoint and returns HttpWebResponse.
    /// No validation is done here, it simply returns whatever the response is.
    /// </summary>
    /// <param name="resourceUri">This is the resource on the endpoint.This includes the full endpoint URI.</param>
    /// <param name="httpMethod">This is the HTTP Method: GET, PUT, POST</param>
    /// <param name="contentType">This is the contentType of the HTTP request</param>
    /// <param name="acceptType">This is the acceptType of the HTTP request</param>
    /// <param name="payloadData">This is the actual data to be used within an HTTP PUT or HTTP POST</param>
    /// <param name="headers"></param>
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
      request.Headers.Add("pragma", "no-cache");
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
