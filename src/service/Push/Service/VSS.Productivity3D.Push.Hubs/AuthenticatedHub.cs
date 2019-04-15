using System;
using System.Globalization;
using System.Security.Principal;
using Microsoft.AspNetCore.SignalR;
using VSS.Common.Abstractions.Http;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Push.Hubs
{
  public class AuthenticatedHub<T> : Hub<T> where T : class
  {
    /// <summary>
    /// If the signalr client uses the built in auth, it will be set in this parameter
    /// </summary>
    private const string SIGNALR_AUTH_HEADER = "access_token";

    private const string BEARER_AUTH_TOKEN = "Bearer";

    public bool IsAuthenticated =>
      Context.User is TIDCustomPrincipal principal && (principal.Identity is GenericIdentity);

    public string UserEmail
    {
      get
      {
        if (Context.User is TIDCustomPrincipal principal && (principal.Identity is GenericIdentity identity))
        {
          return principal.UserEmail;
        }
        throw new NotImplementedException();
      }
    }

    protected string CustomerUid => GetHeaderFromRequest(HeaderConstants.X_VISION_LINK_CUSTOMER_UID);

    protected string AuthorizationToken
    {
      get
      {
        // For some reason, SignalR adds Bearer for long polling, but not for WebSockets...
        // First get the auth header, if it exists or the signalr specific header
        var requestToken = GetHeaderFromRequest(HeaderConstants.AUTHORIZATION);
        var authorization = !string.IsNullOrEmpty(requestToken) 
          ? requestToken 
          : GetHeaderFromRequest(SIGNALR_AUTH_HEADER);

        if (string.IsNullOrEmpty(authorization))
          return null;

        // Check for Bearer, which may or may not exist
        if (authorization.StartsWith(BEARER_AUTH_TOKEN, true, CultureInfo.InvariantCulture))
          return authorization;

        return $"{BEARER_AUTH_TOKEN} {authorization}";
      }
    }

    protected string JwtAssertion => GetHeaderFromRequest(HeaderConstants.X_JWT_ASSERTION);

    private string GetHeaderFromRequest(string header)
    {
      // For SignalR we need to use query parameters instead of header values in some cashes
      // This is due to the different connection types (websockets don't support Request Headers)
      // Long polling can use headers though
      var httpContext = Context.GetHttpContext();
      if (httpContext?.Request == null)
        return null;

      if (httpContext.Request.Headers.TryGetValue(header, out var value))
        return value;

      if (httpContext.Request.Query.TryGetValue(header, out value))
        return value;

      return null;
    }
  }
}
