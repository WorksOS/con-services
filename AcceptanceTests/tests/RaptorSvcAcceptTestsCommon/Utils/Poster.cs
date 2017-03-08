using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using RestAPICoreTestFramework.Utils.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using log4net;

namespace RaptorSvcAcceptTestsCommon.Utils
{
    /// <summary>
    /// A generic class used for POSTing Web APIs.
    /// </summary>
    /// <typeparam name="TRequest">Type of the request sent.</typeparam>
    /// <typeparam name="TResponse">Type of the service response.</typeparam>
    public class Poster<TRequest, TResponse>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Poster<TRequest, TResponse>));

        #region Members
        public Dictionary<string, TRequest> RequestRepo { get; private set; }
        public Dictionary<string, TResponse> ResponseRepo { get; private set; }
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
                        RequestRepo = (Dictionary<string, TRequest>)serializer.Deserialize(file, typeof(Dictionary<string, TRequest>));
                    };
                }

                if (responseFile != null)
                {
                    using (StreamReader file = File.OpenText(RaptorClientConfig.TestDataPath + responseFile))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        ResponseRepo = (Dictionary<string, TResponse>)serializer.Deserialize(file, typeof(Dictionary<string, TResponse>));
                    };
                }
            }
            catch (Exception e)
            {
                log.Error(LogFormatter.Format(e.Message, LogFormatter.ContentType.Error));
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
            {
                requestBodyString = JsonConvert.SerializeObject(this.CurrentRequest);
            }
            else if (requestName.Length > 0)
            {
                requestBodyString = JsonConvert.SerializeObject(this.RequestRepo[requestName]);
            }
            else
            {
                requestBodyString = "";
            }

            this.CurrentServiceResponse = RaptorServicesClientUtil.DoHttpRequest(Uri, "POST", RestClientConfig.JsonMediaType, requestBodyString);

            if (this.CurrentServiceResponse != null)
            {
                if (expectedHttpCode != this.CurrentServiceResponse.HttpCode)
                {
                    log.Error(LogFormatter.Format(String.Format("Expected {0}, but got {1} instead.", expectedHttpCode, this.CurrentServiceResponse.HttpCode),
                        LogFormatter.ContentType.Error));
                }

                Assert.AreEqual(expectedHttpCode, this.CurrentServiceResponse.HttpCode,
                    String.Format("Expected {0}, but got {1} instead.", expectedHttpCode, this.CurrentServiceResponse.HttpCode));

                this.CurrentResponse = JsonConvert.DeserializeObject<TResponse>(this.CurrentServiceResponse.ResponseBody);
                return this.CurrentResponse;
            }
            else
            {
                return default(TResponse);
            }
        }

        #endregion
    }
}
