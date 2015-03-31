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

        private IHttpActionResult ForemanRequest(Func<IHttpActionResult> body)
        {
            try
            {
                return body();
            }
            catch (ForemanApiException e)
            {
                return Content(e.Code, e.Message);
            }
        }


        [Route("login")]
        [AllowAnonymous]
        public IHttpActionResult Login([FromBody] Credentials credentials)
        {
            return ForemanRequest(() => 
            {
                var response = foremanApiClient.Login(credentials);
                var user = LandfillDb.CreateOrGetUser(credentials);
                LandfillDb.SaveSession(user, response);
                return Ok(response);
            });

            //try
            //{
            //    return Ok(foremanApiClient.Login(credentials));
            //}
            //catch (ForemanApiException e)
            //{
            //    return Content(e.code, e.Message);
            //}
        }

        [Route("logout")]
        public IHttpActionResult Logout()
        {
            return ForemanRequest(() =>
            {
                var sessionId = Request.Headers.GetValues("SessionID").First();
                System.Diagnostics.Debug.WriteLine("Logging out session " + sessionId);
                foremanApiClient.Logout(sessionId);
                LandfillDb.DeleteSession(sessionId);
                return Ok();
            });
        }
    }
}
