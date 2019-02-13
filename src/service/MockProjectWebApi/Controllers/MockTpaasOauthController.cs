using Microsoft.AspNetCore.Mvc;
using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockTpaasOauthController : Controller
  {
    /// <summary>
    /// Dummies getting a new bearer token from TPaaS Oauth
    /// </summary>
    [Route("api/oauth2/token")]
    [HttpPost]
    public TPaasOauthResult DummyBearerTokenPost(
      [FromForm] string grantType)
    {
      var res = new TPaasOauthResult()
      {
        Code = 0,
        tPaasOauthRawResult = new TPaasOauthRawResult()
        {
          access_token = "sdfs98du9sdfdnfkj",
          expires_in = 50400,
          token_type = "Bearer"
        }
      };
      var message = $"DummyBearerTokenPost: res {JsonConvert.SerializeObject(res)}. grantType {grantType}";
      Console.WriteLine(message);
      return res;
    }
  }
}