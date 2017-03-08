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
    /// A generic class used for GETTing Web APIs.
    /// </summary>
    /// <typeparam name="TResponse">Type of the service response.</typeparam>
    public class Getter<TResponse>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Getter<TResponse>));

        #region Members
        public Dictionary<string, TResponse> ResponseRepo { get; private set; }
        public string Uri { get; set; }
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

            try
            {
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
        /// Default constructor
        /// </summary>
        public Getter()
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Do an HTTP POST request - expecting success e.g. 200 OK.
        /// </summary>
        /// <param name="uri">URI of the service.</param>
        /// <param name="expectedHttpCode">Expected response HttpStatusCode - default to 200 OK.</param>
        /// <returns>Request response.</returns>
        public TResponse DoValidRequest(string uri = "", HttpStatusCode expectedHttpCode = HttpStatusCode.OK)
        {
            return DoRequest(uri, expectedHttpCode);
        }

        /// <summary>
        /// Do an HTTP POST request - expecting success e.g. 200 OK.
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
        /// Do an HTTP POST request - expecting failure e.g. 400 BadRequest.
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

            this.CurrentServiceResponse = RaptorServicesClientUtil.DoHttpRequest(this.Uri, "GET", RestClientConfig.JsonMediaType, null);

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
