using System;
using System.Net;
using AutomationCore.API.Framework.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Utilization.AcceptanceTests.Utils.Common;

namespace LandfillService.AcceptanceTests.Helpers
{
    /// <summary>
    /// A generic class used for POSTing Web APIs.
    /// </summary>
    /// <typeparam name="TRequest">Type of the request sent.</typeparam>
    /// <typeparam name="TResponse">Type of the service response.</typeparam>
    public class Poster<TRequest, TResponse>
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(Poster<TRequest, TResponse>));

        #region Members
        public string Uri { get; set; }
        public TRequest Request { get; set; }
        public TResponse Response { get; private set; }
        public ServiceResponse RawResponse { get; private set; }
        #endregion

        #region Constructors

        /// <summary>
        /// Construct a service POSTer
        /// </summary>
        /// <param name="uri">URI of the service</param>
        /// <param name="request">Object to be POST'ed</param>
        public Poster(string uri, TRequest request = default(TRequest))
        {
            Uri = uri;
            Request = request;
            Response = default(TResponse);
            RawResponse = null;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Poster()
        {
            Uri = null;
            Request = default(TRequest);
            Response = default(TResponse);
            RawResponse = null;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Do an HTTP POST request - expecting success e.g. 200 OK.
        /// </summary>
        /// <param name="request">Request object to be POST'ed.</param>
        /// <param name="expectedHttpCode">Expected response HttpStatusCode - default to 200 OK.</param>
        /// <returns>Request response.</returns>
        public TResponse DoValidRequest(TRequest request, HttpStatusCode expectedHttpCode = HttpStatusCode.OK)
        {
            Request = request;
            return DoRequest(expectedHttpCode);
        }

        /// <summary>
        /// Do an HTTP POST request sending Request - expecting success e.g. 200 OK.
        /// </summary>
        /// <param name="expectedHttpCode">Expected response HttpStatusCode - default to 200 OK.</param>
        /// <returns>Request response.</returns>
        public TResponse DoValidRequest(HttpStatusCode expectedHttpCode = HttpStatusCode.OK)
        {
            return DoRequest(expectedHttpCode);
        }

        /// <summary>
        /// Do an HTTP POST request - expecting failure e.g. 400 BadRequest.
        /// </summary>
        /// <param name="request">Request object to be POST'ed.</param>
        /// <param name="expectedHttpCode">Expected HTTP error code - default to 400 BadRequest.</param>
        /// <returns>Request response.</returns>
        public TResponse DoInvalidRequest(TRequest request, HttpStatusCode expectedHttpCode = HttpStatusCode.BadRequest)
        {
            Request = request;
            return DoRequest(expectedHttpCode);
        }

        /// <summary>
        /// Do an HTTP POST request - expecting failure e.g. 400 BadRequest.
        /// </summary>
        /// <param name="expectedHttpCode">Expected HTTP error code - default to 400 BadRequest.</param>
        /// <returns>Request response.</returns>
        public TResponse DoInvalidRequest(HttpStatusCode expectedHttpCode = HttpStatusCode.BadRequest)
        {
            return DoRequest(expectedHttpCode);
        }

        /// <summary>
        /// Implementation for public methods DoValidRequest and DoInvalidRequest.
        /// </summary>
        /// <param name="expectedHttpCode">Expected HTTP code.</param>
        /// <returns>Request response.</returns>
        private TResponse DoRequest(HttpStatusCode expectedHttpCode)
        {
            // JSON has problems(tries to be clever) with serializing date and times. We have use JsonSerializerSettings to set no time zone.
            string requestBodyString = JsonConvert.SerializeObject(Request, new JsonSerializerSettings{ DateTimeZoneHandling = DateTimeZoneHandling.Unspecified});
            RawResponse = UtilizationServicesClientUtil.DoHttpRequest(Uri, "POST", RestClientConfig.JsonMediaType, requestBodyString);

            if (RawResponse == null)
            {
            //    log.Error(LogFormatter.Format("The request did not get any response at all.", LogFormatter.ContentType.Error));
                Assert.Fail("The request did not get any response at all.");
            }
            else if (expectedHttpCode != RawResponse.HttpCode)
            {
             ///   log.Error(LogFormatter.Format(String.Format("Expected http status {0}, but got {1} instead. Response = {2}", 
             //       expectedHttpCode, RawResponse.HttpCode, RawResponse.ResponseBody), LogFormatter.ContentType.Error)); 
            }

            return JsonConvert.DeserializeObject<TResponse>(RawResponse.ResponseBody,
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        #endregion
    }
}
