using LandfillService.WebApi.ApiClients;
using LandfillService.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Hosting;
using System.Web.Http;

namespace LandfillService.WebApi.Controllers
{
    /// <summary>
    /// Handles user related requests
    /// </summary>
    [RoutePrefix("api/v1/users")]
    public class UsersController : ApiController
    {
        private ForemanApiClient foremanApiClient = new ForemanApiClient();

        /// <summary>
        /// Wraps a request to the Foreman API & deletes the session if invalid
        /// </summary>
        /// <param name="sessionId">Session ID provided by the Foreman API</param>
        /// <param name="body">Code to execute</param>
        /// <returns>The result of executing body() or error details</returns>
        private IHttpActionResult ForemanRequest(string sessionId, Func<IHttpActionResult> body)
        {
            try
            {
                return body();
            }
            catch (ForemanApiException e)
            {
                if (e.code == HttpStatusCode.Unauthorized)
                    LandfillDb.DeleteSession(sessionId);
                return Content(e.code, e.Message);
            }
        }


        /// <summary>
        /// Deletes stale sessions via a background task
        /// </summary>
        /// <returns></returns>
        private void DeleteStaleSessionsInBackground()
        {
            HostingEnvironment.QueueBackgroundWorkItem((CancellationToken cancel) =>
            {
                LandfillDb.DeleteStaleSessions();
            });
        }

        /// <summary>
        /// Gets a session ID for the supplied credentials from the Foreman API, creates the user in the DB if needed and saves the session to the DB;
        /// possibly deletes stale sessions
        /// </summary>
        /// <param name="credentials">User name and password</param>
        /// <returns>Session ID to be used in subsequent requests</returns>
        [Route("login")]
        [AllowAnonymous]
        public IHttpActionResult Login([FromBody] Credentials credentials)
        {
            var sessionId = Request.Headers.GetValues("SessionID").First();

            if (new Random().Next(100) % 100 < 5)  // occasionally, delete stale sessions
                DeleteStaleSessionsInBackground();

            return ForemanRequest(sessionId, () => 
            {
                var response = foremanApiClient.Login(credentials);
                var prefs = foremanApiClient.GetUserUnits(sessionId);
                var user = LandfillDb.CreateOrGetUser(credentials.userName,(int)prefs);
                LandfillDb.SaveSession(user, response);
                return Ok(String.Format("{0}${1}",response,prefs));
            });
        }

        /// <summary>
        /// Gets a session ID for the supplied one-time key from the Foreman API, creates the user in the DB if needed and saves the session to the DB;
        /// possibly deletes stale sessions
        /// </summary>
        /// <param name="credentials">User name and one-time key</param>
        /// <returns>Session ID to be used in subsequent requests</returns>
        [Route("login/vl")]
        [AllowAnonymous]
        public IHttpActionResult LoginVl([FromBody] VlCredentials credentials)
        {
            var sessionId = Request.Headers.GetValues("SessionID").First();

            if (new Random().Next(100) % 100 < 5)  // occasionally, delete stale sessions
                DeleteStaleSessionsInBackground();

            return ForemanRequest(sessionId, () =>
            {
                var response = foremanApiClient.LoginWithKey(credentials.key);
                var prefs = foremanApiClient.GetUserUnits(sessionId);
                var user = LandfillDb.CreateOrGetUser(credentials.userName, (int)prefs);
                LandfillDb.SaveSession(user, response);
                return Ok(String.Format("{0}${1}", response, prefs));
            });
        }

        /// <summary>
        /// Logs the user out via the Foreman API and deletes the session from the DB
        /// </summary>
        /// <returns></returns>
        [Route("logout")]
        public IHttpActionResult Logout()
        {
            var sessionId = Request.Headers.GetValues("SessionID").First();
            return ForemanRequest(sessionId, () =>
            {
                System.Diagnostics.Debug.WriteLine("Logging out session " + sessionId);
                LandfillDb.DeleteSession(sessionId);
                foremanApiClient.Logout(sessionId);
                return Ok();
            });
        }
    }
}
