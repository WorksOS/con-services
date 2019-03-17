using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.AssetMgmt3D
{
  public class AssetMgmt3DAuthentication : TIDAuthentication
  {
    public AssetMgmt3DAuthentication(RequestDelegate next, ICustomerProxy customerProxy, IConfigurationStore store,
      ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler) : base(next, customerProxy, store,
      logger, serviceExceptionHandler)
    {
    }

    public override bool RequireCustomerUid(HttpContext context)
    {
      return true;
    }
  }
}