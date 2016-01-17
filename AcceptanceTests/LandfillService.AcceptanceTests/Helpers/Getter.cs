using System;
using System.Net;
using AutomationCore.API.Framework.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace LandfillService.AcceptanceTests.Helpers
{

    /// <summary>
    /// A generic class used for GETTing Web APIs.
    /// </summary>
    /// <typeparam name="TResponse">Type of the service response.</typeparam>
    public class Getter<TResponse>
    {
       // private static readonly ILog log = LogManager.GetLogger(typeof(Getter<TResponse>));

        #region Members
        public string Uri { get; set; }
        public TResponse Response { get; private set; }
        public ServiceResponse RawResponse { get; private set; }
        #endregion

        #region Constructors

        /// <summary>
        /// Construct a service Getter
        /// </summary>
        /// <param name="uri">URI of the service</param>
        public Getter(string uri)
        {
            Uri = uri;
            Response = default(TResponse);
            RawResponse = null;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Getter()
        {
            Uri = null;
            Response = default(TResponse);
            RawResponse = null;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Do an HTTP GET request - expecting success e.g. 200 OK.
        /// </summary>
        /// <param name="uri">URI of the service.</param>
        /// <param name="expectedHttpCode">Expected response HttpStatusCode - default to 200 OK.</param>
        /// <returns>Request response.</returns>
        public TResponse DoValidRequest(string uri, HttpStatusCode expectedHttpCode = HttpStatusCode.OK)
        {
            Uri = uri;
            return DoRequest(expectedHttpCode);
        }

        /// <summary>
        /// Do an HTTP GET request - expecting success e.g. 200 OK.
        /// </summary>
        /// <param name="expectedHttpCode">Expected response HttpStatusCode - default to 200 OK.</param>
        /// <returns>Request response.</returns>
        public TResponse DoValidRequest(HttpStatusCode expectedHttpCode = HttpStatusCode.OK)
        {
            return DoRequest(expectedHttpCode);
        }

        /// <summary>
        /// Do an invalid GET request - expecting failure e.g. 400 BadRequest.
        /// </summary>
        /// <param name="uri">URI of the service.</param>
        /// <param name="expectedHttpCode">Expected response HttpStatusCode - default to 400 BadRequest.</param>
        /// <returns>Request response.</returns>
        public TResponse DoInvalidRequest(string uri, HttpStatusCode expectedHttpCode = HttpStatusCode.BadRequest)
        {
            Uri = uri;
            return DoRequest(expectedHttpCode);
        }

        /// <summary>
        /// Do an HTTP POST request - expecting failure e.g. 400 BadRequest.
        /// </summary>
        /// <param name="expectedHttpCode">Expected response HttpStatusCode - default to 400 BadRequest.</param>
        /// <returns>Request response.</returns>
        public TResponse DoInvalidRequest(HttpStatusCode expectedHttpCode = HttpStatusCode.BadRequest)
        {
            return DoRequest(expectedHttpCode);
        }

        /// <summary>
        /// Implementation for public methods DoValidRequest and DoInvalidRequest.
        /// </summary>
        /// <param name="expectedHttpCode">Expected response HttpStatusCode.</param>
        /// <returns>Request response.</returns>
        private TResponse DoRequest(HttpStatusCode expectedHttpCode)
        {
            RawResponse = ServicesClientUtil.DoHttpRequest(Uri, "GET", RestClientConfig.JsonMediaType, null);

            if (RawResponse == null)
            {
                try // Doing it this way for the log
                {
                    throw new Exception("The request did not get any response at all.");
                }
                catch (Exception)
                {
                    Assert.Fail("Test Failed - the request did not get any response at all.");
                }      
            }
            else if (expectedHttpCode != RawResponse.HttpCode)
            {
                try
                {
                    throw new Exception("The HttpCode returned is incorrect.");
                }
                catch (Exception)
                {
                    Assert.Fail("Test Failed - expected {0}, but got {1} instead. Response = {2}", expectedHttpCode, RawResponse.HttpCode, RawResponse.ResponseBody);
                }
            }

            return JsonConvert.DeserializeObject<TResponse>(RawResponse.ResponseBody, 
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        #endregion
    }
}
