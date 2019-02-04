using System;
using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Proxies;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Now3D.Controllers
{
  public abstract class BaseController : Controller
  {
    protected BaseController(ILoggerFactory loggerFactory)
    {
      Log = loggerFactory.CreateLogger(GetType().Name);
    }

    protected ILogger Log { get; private set; }

    /// <summary>
    /// Gets the custom customHeaders for the request.
    /// </summary>
    /// <value>
    /// The custom customHeaders.
    /// </value>
    protected IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders();

    protected string UserId => GetUserId();

    private string GetUserId()
    {
      if (User is TIDCustomPrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }
  }
}