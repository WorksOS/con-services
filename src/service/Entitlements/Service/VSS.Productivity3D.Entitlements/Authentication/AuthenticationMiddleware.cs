using System;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Authentication.JWT;
using VSS.Common.Abstractions.Http;

namespace VSS.Productivity3D.Entitlements.Authentication
{
  public class AuthenticationMiddleware
  {
    private readonly RequestDelegate _next;
    private ILogger log;
    

    public AuthenticationMiddleware(RequestDelegate next, ILoggerFactory logger)
    {
      _next = next;
      log = logger.CreateLogger<AuthenticationMiddleware>();
    }

    public async Task Invoke(HttpContext context)
    {
      string authorization = context.Request.Headers[HeaderConstants.X_JWT_ASSERTION];
      try
      {
        var jwtToken = new TPaaSJWT(authorization);
        var isApplicationContext = jwtToken.IsApplicationToken;
        var applicationName = jwtToken.ApplicationName;
        var userEmail = isApplicationContext ? applicationName : jwtToken.EmailAddress;
        var userUid = isApplicationContext ? jwtToken.ApplicationId : jwtToken.UserUid.ToString();

        context.User = new EntitlementUserClaim(new GenericIdentity(userUid), userEmail, applicationName, isApplicationContext);
      }
      catch (Exception e)
      {
        log.LogWarning(e, "Invalid authentication with exception");
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        await context.Response.WriteAsync("Failed Authentication");
      }

      await _next.Invoke(context);
    }
  }
}
