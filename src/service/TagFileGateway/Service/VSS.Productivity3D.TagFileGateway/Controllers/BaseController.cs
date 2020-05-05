using System;
using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.TagFileForwarder.Controllers
{
  public abstract class BaseController : Controller
  {
    protected BaseController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler)
    {
      ServiceExceptionHandler = serviceExceptionHandler;
      Log = loggerFactory.CreateLogger(GetType().Name);
    }

    protected ILogger Log { get; private set; }
    
    protected IServiceExceptionHandler ServiceExceptionHandler { get; private set; }

    /// <summary>
    /// Gets the custom customHeaders for the request.
    /// </summary>
    /// <value>
    /// The custom customHeaders.
    /// </value>
    protected IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders(true);

    protected string CustomerUid => Request.Headers[HeaderConstants.X_VISION_LINK_CUSTOMER_UID].ToString();

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
