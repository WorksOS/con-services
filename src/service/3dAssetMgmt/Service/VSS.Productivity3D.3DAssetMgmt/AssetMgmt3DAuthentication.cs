using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.AssetMgmt3D
{
  public class AssetMgmt3DAuthentication : TIDAuthentication
  {
    public AssetMgmt3DAuthentication(RequestDelegate next, ICwsAccountClient cwsAccountClient, IConfigurationStore store,
      ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler) : base(next, cwsAccountClient, store,
      logger, serviceExceptionHandler)
    {
    }

    public override bool RequireCustomerUid(HttpContext context)
    {
      return true;
    }
  }
}
