using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Now3D
{
  public class Now3DAuthentication : TIDAuthentication
  {
    public Now3DAuthentication(RequestDelegate next, ICustomerProxy customerProxy, IConfigurationStore store, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler) : base(next, customerProxy, store, logger, serviceExceptionHandler)
    { }

    public override bool RequireCustomerUid(HttpContext context)
    {
      return false;
    }
  }
}
