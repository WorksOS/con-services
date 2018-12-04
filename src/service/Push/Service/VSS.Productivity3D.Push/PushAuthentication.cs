using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Push
{
  public class PushAuthentication : TIDAuthentication
  {
    public const string SKIP_AUTHENTICATION_HEADER = "X-VSS-NO-TPAAS";

    public PushAuthentication(RequestDelegate next, ICustomerProxy customerProxy, IConfigurationStore store, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler) : base(next, customerProxy, store, logger, serviceExceptionHandler)
    {
    }

    public override bool RequireCustomerUid(HttpContext context)
    {
      return false;
    }

    public override bool InternalConnection(HttpContext context)
    {
      if (context.Request.Headers.ContainsKey(SKIP_AUTHENTICATION_HEADER))
      {
        return true;
      }
      
      return base.InternalConnection(context);
    }
  }
}