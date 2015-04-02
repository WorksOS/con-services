using LandfillService.WebApi.ApiClients;
using LandfillService.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LandfillService.WebApi.Controllers
{
    [RoutePrefix("api/v1/users")]
    public class UsersController : ApiController
    {
        private ForemanApiClient foremanApiClient = new ForemanApiClient();
        private LandfillDb db = new LandfillDb();

        private IHttpActionResult ForemanRequest(string sessionId, Func<IHttpActionResult> body)
        {
            try
            {
                return body();
            }
            catch (ForemanApiException e)
            {
                if (e.Code == HttpStatusCode.Unauthorized)
                    LandfillDb.DeleteSession(sessionId);
                return Content(e.Code, e.Message);
            }
        }


        [Route("login")]
        [AllowAnonymous]
        public IHttpActionResult Login([FromBody] Credentials credentials)
        {
            var sessionId = Request.Headers.GetValues("SessionID").First();
            return ForemanRequest(sessionId, () => 
            {
                var response = foremanApiClient.Login(credentials);
                var user = LandfillDb.CreateOrGetUser(credentials);
                LandfillDb.SaveSession(user, response);
                return Ok(response);
            });
        }

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
