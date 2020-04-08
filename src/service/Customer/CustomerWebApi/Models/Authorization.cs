using Microsoft.Extensions.Logging;
using VSS.Authentication.JWT;

namespace CustomerWebApi.Models
{
  public class Authorization
  {

    public string jwtAssertion;

    public string customerUid;

    public string userUid;

    public string userEmail;

    public bool IsApplicationToken;

    protected readonly ILogger<Authorization> log;

    public Authorization(ILoggerFactory loggerFactory, Microsoft.AspNetCore.Http.HttpRequest request)
    {
      log = loggerFactory.CreateLogger<Authorization>();
      GetAuthorizationDetails(request);
    }

    public void GetAuthorizationDetails(Microsoft.AspNetCore.Http.HttpRequest Request)
    {
      jwtAssertion = Request.Headers["X-Jwt-Assertion"];
      if (string.IsNullOrEmpty(jwtAssertion))
      {
        log.LogError("No X-Jwt-Assertion token found. Are you calling the service via tpaas or direct. If direct add X-Jwt-Assertion to header");
      }
      var jwtToken = new TPaaSJWT(jwtAssertion);
      customerUid = Request.Headers["X-VisionLink-CustomerUID"];
      IsApplicationToken = jwtToken.IsApplicationToken;
      userEmail = IsApplicationToken ? jwtToken.ApplicationName : jwtToken.EmailAddress;
      userUid = IsApplicationToken ? jwtToken.ApplicationId : jwtToken.UserUid.ToString();
      if (IsApplicationToken)
      {
        log.LogInformation($"Authorization for application {jwtToken.ApplicationName} and id {jwtToken.ApplicationId}");
      }
      else 
      {
        log.LogInformation($"Authorization email {userEmail} and  userUid={userUid}");
      }
      log.LogInformation($"Authorization: X-VisionLink-CustomerUID={customerUid} and JWT={jwtToken.EncodedJWT}");      
    }
  }
}
