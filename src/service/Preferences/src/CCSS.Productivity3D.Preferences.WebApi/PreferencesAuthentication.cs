using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Entitlements.Abstractions.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Filter.WebApi
{
  public class PreferencesAuthentication : TIDAuthentication
  {
    public PreferencesAuthentication(RequestDelegate next, ICwsAccountClient accountClient, IConfigurationStore store, ILoggerFactory logger, IEntitlementProxy entitlementProxy, IServiceExceptionHandler serviceExceptionHandler) : base(next, accountClient, store, logger, entitlementProxy, serviceExceptionHandler)
    { }

    public override bool RequireCustomerUid(HttpContext context)
    {
      return false;
    }
  }
}
