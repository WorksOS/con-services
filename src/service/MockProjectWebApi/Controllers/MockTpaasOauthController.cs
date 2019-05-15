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
    public TPaasOauthResult DummyGetBearerTokenPost(
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
      var message = $"DummyGetBearerTokenPost: res {JsonConvert.SerializeObject(res)}. grantType {grantType}";
      Console.WriteLine(message);
      return res;
    }

    /// <summary>
    /// Dummies revoking a bearer token from TPaaS Oauth
    /// </summary>
    [Route("api/oauth2/revoke")]
    [HttpPost]
    public BaseDataResult DummyREvokeBearerTokenPost(
      [FromForm] string token)
    {
      var res = new BaseDataResult()
      {
        Code = 0
      };
      var message = $"DummyRevokeBearerTokenPost: res {JsonConvert.SerializeObject(res)}. token {token}";
      Console.WriteLine(message);
      return res;
    }
  }
}
