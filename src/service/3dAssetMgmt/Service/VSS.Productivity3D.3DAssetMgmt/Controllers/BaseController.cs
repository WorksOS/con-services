using System;
using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.AssetMgmt3D.Controllers
{
  public abstract class BaseController : Controller
  {
    private ILogger _log;
    private IServiceExceptionHandler _serviceExceptionHandler;

    protected ILogger Log => _log ??= HttpContext?.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().Name);
    protected IServiceExceptionHandler ServiceExceptionHandler => _serviceExceptionHandler ??= HttpContext.RequestServices.GetRequiredService<IServiceExceptionHandler>();
    protected IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders(true);
    protected string CustomerUid => Request.Headers[HeaderConstants.X_VISION_LINK_CUSTOMER_UID].ToString();
    protected string UserId => GetUserId();

    private string GetUserId()
    {
      if (User is TIDCustomPrincipal principal && principal.Identity is GenericIdentity identity)
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }
  }
}
