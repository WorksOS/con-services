using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Filter.WebApi
{
  public class PreferencesAuthentication : TIDAuthentication
  {
    public PreferencesAuthentication(RequestDelegate next, ICwsAccountClient accountClient, IConfigurationStore store, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler) : base(next, accountClient, store, logger, serviceExceptionHandler)
    {
    }

    public override bool RequireCustomerUid(HttpContext context)
    {
      return false;
    }
  }
}
