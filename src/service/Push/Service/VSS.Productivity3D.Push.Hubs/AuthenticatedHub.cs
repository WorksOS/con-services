using System;
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

    protected string CustomerUid => GetHeaderFromQueryString(HeaderConstants.X_VISION_LINK_CUSTOMER_UID);

    protected string AuthorizationToken
    {
      get
      {
        var token = GetHeaderFromQueryString(HeaderConstants.AUTHORIZATION);
        return !string.IsNullOrEmpty(token) 
          ? token 
          : GetHeaderFromQueryString(SIGNALR_AUTH_HEADER); 
      }
    }

    protected string JwtAssertion => GetHeaderFromQueryString(HeaderConstants.X_JWT_ASSERTION);

    private string GetHeaderFromQueryString(string header)
    {
      // For SignalR we need to use query parameters instead of header values
      // This is due to the different connection types (websockets don't support Request Headers)
      var httpContext = Context.GetHttpContext();
      if (httpContext?.Request == null)
        return null;

      if (httpContext.Request.Query.TryGetValue(header, out var c))
        return c;

      return null;
    }
  }
}
