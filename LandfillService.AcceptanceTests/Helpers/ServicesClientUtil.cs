using System.IO;
using System.Net;
using System.Text;
using AutomationCore.API.Framework.Library;

namespace LandfillService.AcceptanceTests.Helpers
{
    public class ServicesClientUtil : RestClientUtil
    {
     //   private static readonly ILog log = LogManager.GetLogger(typeof(UtilizationServicesClientUtil));

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
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resourceUri);
            //request.Headers = RaptorClientConfig.HeaderWithAuth;
            request.KeepAlive = true;
            request.Method = httpMethod;
            request.Accept = acceptType;

            // Logging
            //log.Info(LogFormatter.Format(resourceUri, LogFormatter.ContentType.URI));
            //log.Info(LogFormatter.Format(httpMethod, LogFormatter.ContentType.HttpMethod));
            //log.Info(LogFormatter.Format(string.IsNullOrEmpty(request.Headers.ToString()) ? request.Headers.ToString() : 
            //    request.Headers.ToString().Replace(Environment.NewLine, ","), LogFormatter.ContentType.RequestHeader));
            //log.Info(LogFormatter.Format(string.IsNullOrEmpty(payloadData) ? payloadData : payloadData.Replace(Environment.NewLine, ","), 
            //    LogFormatter.ContentType.RequestBody));

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
                //else
                //    log.Error(LogFormatter.Format(e.Message, LogFormatter.ContentType.Error), e);
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
            HttpWebResponse httpResponse = null;
            httpResponse = DoHttpRequest(resourceUri, httpMethod, mediaType, mediaType, payloadData);

            if (httpResponse != null)
            {
                var responseHeader = httpResponse.Headers;
                var httpResponseCode = httpResponse.StatusCode;

                // Get the response body string for debug message
                string responseString = null;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    responseString = streamReader.ReadToEnd();

                // Logging
                //log.Info(LogFormatter.Format(string.IsNullOrEmpty(responseHeader.ToString()) ? responseHeader.ToString() :
                //    responseHeader.ToString().Replace(Environment.NewLine, ","), LogFormatter.ContentType.ResponseHeader));
                //log.Info(LogFormatter.Format(httpResponse.StatusCode.ToString(), LogFormatter.ContentType.HttpCode));
                //log.Info(LogFormatter.Format(responseString, LogFormatter.ContentType.ResponseBody));

                httpResponse.Close();

                return new ServiceResponse()
                {
                    ResponseHeader = responseHeader,
                    HttpCode = httpResponseCode,
                    ResponseBody = responseString
                };
            }
            else
                return null;
        }
    }
}
