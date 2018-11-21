using System;
using System.IO;
using System.Net;
using System.Text;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtility
{
  /// <summary>
  /// This base class provides utility methods for performing an HTTP Request 
  /// of all HTTP Method Operations: GET, PUT, POST, DELETE
  /// This base class provides utilty methods for validation of the response status codes,
  /// response headers, and response contents
  /// </summary>
  public class RestClientUtil
  {
    /// <summary>
    /// Overloaded (no auth): This method performs a valid HTTP GET, PUT, or POST request on an RESTful endpoint and stores the response in a string for later parsing
    /// This method is overloaded as it doesn't require username and password authentication
    /// </summary>
    /// <param name="resourceUri">This is the resource on the endpoint.This includes the full endpoint URI.</param>
    /// <param name="httpMethod">This is the HTTP Method: GET, PUT, POST</param>
    /// <param name="payloadData">This is the actual data to be used within an HTTP PUT or HTTP POST</param>
    /// <param name="httpResponseCode">This is the HTTP STATUS CODE: 200, 201, etc.</param>
    /// <param name="mediaType">This is the mediaType of the HTTP request which can be json or xml </param>    
    /// <param name="customerUid">This is the customer UID for the header</param>
    /// <returns></returns>
    public string DoHttpRequest(string resourceUri, string httpMethod, string payloadData, HttpStatusCode httpResponseCode = HttpStatusCode.OK, string mediaType = "application/json", string customerUid = null)
    {
      Log.Info(resourceUri, Log.ContentType.ApiSend);
      var msg = new Msg();
      msg.DisplayWebApi(httpMethod, resourceUri, string.Empty, payloadData);
      var request = InitHttpRequest(resourceUri, httpMethod, mediaType, customerUid);                   
      if (payloadData != null)
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
        string responseString;
        Console.WriteLine($"Sending the request");
        using (var response = (HttpWebResponse)request.GetResponseAsync().Result)
        {
          Console.WriteLine($"Recieved the response to the request");
          if (response.ContentType == "application/zip")
          {
            using (MemoryStream ms = GetMemoryStreamFromResponseStream(response))
            {
              var bytes = Common.Decompress(ms);
              responseString = Encoding.Default.GetString(bytes);
            }
          }
          else
          {
            responseString = GetStringFromResponseStream(response);
          }
          msg.DisplayWebApi(httpMethod, resourceUri, responseString, payloadData);
          Assert.AreEqual(httpResponseCode, response.StatusCode, "Expected this response code, " + httpResponseCode + ", but the actual response code was this instead, " + response.StatusCode);
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
        msg.DisplayException(ex.Message);
        return string.Empty;
      }
    }

    /// <summary>
    /// Get the HTTP Response from the response stream and store in a string variable
    /// </summary>
    private static string GetStringFromResponseStream(HttpWebResponse response)
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
    /// Get the HTTP Response from the response stream and store in a memory stream
    /// </summary>
    private static MemoryStream GetMemoryStreamFromResponseStream(HttpWebResponse response)
    {
      var readStream = response.GetResponseStream();

      if (readStream != null)
      {
        var buffer = new byte[1024];
        MemoryStream ms = new MemoryStream();
        
        Array.Clear(buffer, 0, buffer.Length);
        var read = readStream.Read(buffer, 0, buffer.Length);
        ms.Write(buffer, 0, read);
        while (read > 0)
        {
          Array.Clear(buffer, 0, buffer.Length);
          read = readStream.Read(buffer, 0, buffer.Length);
          ms.Write(buffer, 0, read);
        }
        return ms;
      }
      return null;
    }

    /// <summary>
    /// Overloaded (no auth): This method sets the Http Request Method, Header, Media Type, and Authentication
    /// </summary>
    /// <param name="resourceUri">This is the resource on the endpoint.This includes the full endpoint URI.</param>
    /// <param name="httpMethod">This is the HTTP Method: GET, PUT, POST</param>
    /// <param name="mediaType">This is the mediaType of the http request which can be json or xml </param>
    /// <param name="customerUid">This is the customer UID for the header for authentication </param>
    /// <returns>This returns the HTTP request</returns>
    private static HttpWebRequest InitHttpRequest(string resourceUri, string httpMethod, string mediaType, string customerUid)
    {
      //Initialize the Http Request
      var request = (HttpWebRequest)WebRequest.Create(resourceUri);
      request.Method = httpMethod;
      request.Accept = mediaType;
      //Hardcode authentication for now
      request.Headers["X-JWT-Assertion"] = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IkNvbXBhY3Rpb24tRGV2ZWxvcC1DSSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3lEZXRhaWxzIjoiZXlKMWNHUmhkR1ZrVkdsdFpTSTZNVFE1TVRFM01ERTROamszTWl3aWFHbHpkRzl5ZVNJNld5STJOVE5pWmpJeU9EZzJOamM1TldVd05ERTVNakEyTnpFMFkyVXpNRFpsTURNeVltUXlNalppWkRVMFpqUXpOamcxTkRJME5UZGxaVEl4TURnMU5UQXdJaXdpTWpFMk56ZG1OemxpTlRWbVpqY3pOamxsTVdWbU9EQmhOV0V3WVRGaVpXSTRNamcwWkdJME16WTVNekEzT1RreFpUbGpaRFUzTkRnMk16VmpZVGRsTWlJc0ltTTVOVEF3TURaak5USXpaV0kxT0RkaFpHRXpNRFUxTWpJMFlXUmxabUUzTjJJeE1EYzJZV1JsT1RnMk1qRTBaakpqT0RJek1qWTRNR1l5TnprMk1EVWlYWDA9IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9rZXl0eXBlIjoiUFJPRFVDVElPTiIsInNjb3BlcyI6Im9wZW5pZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxWZXJpZmllZCI6InRydWUiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJkZXYtdnNzYWRtaW5AdHJpbWJsZS5jb20iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT05fVVNFUiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6InB1Ymxpc2hlciIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdFVwZGF0ZVRpbWVTdGFtcCI6IjE0OTcyNzgyMDQ5MjIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FjY291bnR1c2VybmFtZSI6IkRhdmlkX0dsYXNzZW5idXJ5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS91bmxvY2tUaW1lIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYWNjb3VudG5hbWUiOiJ0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmlyc3RuYW1lIjoiVGVzdCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3kiOiJISUdIIiwiaXNzIjoid3NvMi5vcmcvcHJvZHVjdHMvYW0iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RuYW1lIjoiUHJvamVjdE1ETSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb25pZCI6IjM3NDMiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3ZlcnNpb24iOiIxLjQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJ0ZXN0UHJvamVjdE1ETUB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdXVpZCI6Ijk4Y2RiNjE5LWIwNmItNDA4NC1iN2M1LTVkY2NjYzgyYWYzYiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlclRlbmFudElkIjoiMSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZ2l2ZW5uYW1lIjoiRGF2ZSIsImV4cCI6MTQ5ODE4MTI0NCwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS9mYWlsZWRMb2dpbkF0dGVtcHRzIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvaWRlbnRpdHkvYWNjb3VudExvY2tlZCI6ImZhbHNlIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNzLWRldi1wcm9qZWN0cyIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdExvZ2luVGltZVN0YW1wIjoiMTQ5ODE2NTAxOTM3MCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvc3RhdHVzIjoiZXlKQ1RFOURTMFZFSWpvaVptRnNjMlVpTENKWFFVbFVTVTVIWDBaUFVsOUZUVUZKVEY5V1JWSkpSa2xEUVZSSlQwNGlPaUptWVd4elpTSXNJa0pTVlZSRlgwWlBVa05GWDB4UFEwdEZSQ0k2SW1aaGJITmxJaXdpUVVOVVNWWkZJam9pZEhKMVpTSjkiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RQd2RTZXRUaW1lU3RhbXAiOiIxNDkxMTcwMTg3Mjk3IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbnRpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InRlc3RQcm9qZWN0TURNQHRyaW1ibGUuY29tIiwianRpIjoiYTU3ZTYwYWQtY2YzNC00YzY4LTk0YmQtOTQxY2E1NWFkMTVhIiwiaWF0IjoxNDk4MTc3NDc5fQ.cTQq_4hmspQ9ojOXeau1q4ZywCwwC2fIOkY_tESA5FU";

      if (!string.IsNullOrEmpty(customerUid))
      {
        request.Headers["X-VisionLink-CustomerUid"] = customerUid;
      }

      request.Headers["X-VisionLink-ClearCache"] = "true";
      return request;
    }
  }
}