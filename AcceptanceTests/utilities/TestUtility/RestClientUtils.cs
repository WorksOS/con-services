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
        /// <returns></returns>
        public string DoHttpRequest(string resourceUri, string httpMethod,string mediaType, string payloadData, HttpStatusCode httpResponseCode)
        {
           // Stream writeStream = null;
            string responseString = null;

            Log.Info(resourceUri, Log.ContentType.ApiSend);
            //Initialize the Http Request
            HttpWebRequest request = InitHttpRequest(resourceUri, httpMethod, mediaType);
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
                        Assert.AreEqual(httpResponseCode, httpResponse.StatusCode,
                            "Expected this response code, " + httpResponseCode +
                            ", but the actual response code was this instead, " + httpResponse.StatusCode);


                        responseString = GetStringFromResponseStream(httpResponse);
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
        /// <returns>This returns the HTTP request</returns>
        private HttpWebRequest InitHttpRequest(string resourceUri, string httpMethod,string mediaType)
        {
            //Initialize the Http Request
            var request = (HttpWebRequest)WebRequest.Create(resourceUri);
            request.Method = httpMethod;
            request.Accept = mediaType;
            return request;
        }
    }
}