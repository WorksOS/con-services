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

        private IHttpActionResult ForemanRequest(Func<IHttpActionResult> body)
        {
            try
            {
                return body();
            }
            catch (ForemanApiException e)
            {
                return Content(e.code, e.Message);
            }
        }


        [Route("login")]
        [AllowAnonymous]
        public IHttpActionResult Login([FromBody] Credentials credentials)
        {
            return ForemanRequest(() => Ok(foremanApiClient.Login(credentials)));

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
                System.Diagnostics.Debug.WriteLine("Logging out session " + Request.Headers.GetValues("SessionID").First());
                foremanApiClient.Logout(Request.Headers.GetValues("SessionId").First());
                // TODO: invalidate the session in the DB
                return Ok();
            });
        }
    }
}
