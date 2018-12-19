using System;
using System.Security.Principal;
using Microsoft.AspNetCore.SignalR;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Push.Hubs
{
  public class AuthenticatedHub<T> : Hub<T> where T : class
  {
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
  }
}