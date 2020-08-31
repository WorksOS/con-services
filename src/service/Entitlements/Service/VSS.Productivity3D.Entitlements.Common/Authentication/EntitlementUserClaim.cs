using System.Security.Claims;
using System.Security.Principal;

namespace VSS.Productivity3D.Entitlements.Common.Authentication
{
  public class EntitlementUserClaim : ClaimsPrincipal
  {
    public string UserEmail { get; }

    public string ApplicationName { get; }

    public bool IsApplicationContext { get; }

    public EntitlementUserClaim(GenericIdentity identity, 
      string userEmail, 
      string applicationName, 
      in bool isApplicationContext)
    {
      UserEmail = userEmail;
      ApplicationName = applicationName;
      IsApplicationContext = isApplicationContext;
    }
  }
}
