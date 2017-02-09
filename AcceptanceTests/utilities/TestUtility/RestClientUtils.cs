using System;
using System.IO;
using System.Net;
using System.Text;
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
    /// <param name="mediaType">This is the mediaType of the HTTP request which can be json or xml </param>
    /// <param name="payloadData">This is the actual data to be used within an HTTP PUT or HTTP POST</param>
    /// <param name="httpResponseCode">This is the HTTP STATUS CODE: 200, 201, etc.</param>
    /// <param name="customerUid">This is the customer UID for the header</param>
    /// <returns></returns>
    public string DoHttpRequest(string resourceUri, string httpMethod,string mediaType, string payloadData, HttpStatusCode httpResponseCode, string customerUid=null)
        {
           // Stream writeStream = null;
            string responseString = null;

            Log.Info(resourceUri, Log.ContentType.ApiSend);
            //Initialize the Http Request
            HttpWebRequest request = InitHttpRequest(resourceUri, httpMethod, mediaType, customerUid);
            //Perform the PUT or POST request with the payload

            if (payloadData != null)
            {
                request.ContentType = mediaType;
                var writeStream = request.GetRequestStreamAsync().Result;
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = encoding.GetBytes(payloadData);
                writeStream.Write(bytes, 0, bytes.Length);
            }

                
            try
            {
                //Validate the HTTP Response Status Codes for Successful POST HTTP Request
                using (var response = (HttpWebResponse) request.GetResponseAsync().Result)
                {
                    Assert.AreEqual(httpResponseCode, response.StatusCode,
                        "Expected this response code, " + httpResponseCode +
                        ", but the actual response code was this instead, " + response.StatusCode);

                    responseString = GetStringFromResponseStream(response);
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    var exception = (e as AggregateException).InnerExceptions[0] as WebException;
                    using (var response = exception.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse) response;
                      if (httpResponse != null)
                      {
                        Assert.AreEqual(httpResponseCode, httpResponse.StatusCode,
                          "Expected this response code, " + httpResponseCode +
                          ", but the actual response code was this instead, " + httpResponse.StatusCode);
                        responseString = GetStringFromResponseStream(httpResponse);
                      }
                      else
                      {
                        responseString = e.Message;
                      }
                    }
                }
            }
            Log.Info("Web Api Response: " + responseString, Log.ContentType.ApiResponse);
            var msg = new Msg();
            msg.DisplayWebApi(httpMethod, resourceUri, responseString, payloadData);
            return responseString;
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
    /// <returns>This returns the HTTP request</returns>
    private HttpWebRequest InitHttpRequest(string resourceUri, string httpMethod,string mediaType,string customerUid)
        {
            //Initialize the Http Request
            var request = (HttpWebRequest)WebRequest.Create(resourceUri);
            request.Method = httpMethod;
            request.Accept = mediaType;
            //Hardcode authentication for now
            request.Headers["X-JWT-Assertion"] =
            "eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6IjE0NTU1Nzc4MjM5MzAiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoxMDc5LCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IlV0aWxpemF0aW9uIERldmVsb3AgQ0kiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9udGllciI6IiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBpY29udGV4dCI6Ii90L3RyaW1ibGUuY29tL3V0aWxpemF0aW9uYWxwaGFlbmRwb2ludCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdmVyc2lvbiI6IjEuMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMva2V5dHlwZSI6IlBST0RVQ1RJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbmR1c2VyVGVuYW50SWQiOiIxIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbWFpbGFkZHJlc3MiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJDbGF5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9sYXN0bmFtZSI6IkFuZGVyc29uIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9vbmVUaW1lUGFzc3dvcmQiOm51bGwsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6IlN1YnNjcmliZXIscHVibGlzaGVyIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91dWlkIjoiMjM4ODY5YWYtY2E1Yy00NWUyLWI0ZjgtNzUwNjE1YzhhOGFiIn0=.kTaMf1IY83fPHqUHTtVHn6m6aQ9wFch6c0FsNDQ7x1k=";
            if (!string.IsNullOrEmpty(customerUid))
            {
              request.Headers["X-VisionLink-CustomerUid"] = customerUid;
            }
            return request;
        }

      //public const string CUSTOMER_UID = "d7cafa93-634e-e311-b5a0-90e2ba076184";
    }
}