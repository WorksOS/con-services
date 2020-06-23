using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Entitlements.Abstractions.Interfaces;
using VSS.WebApi.Common;

namespace VSS.MasterData.Project.WebAPI.Middleware
{
  /// <summary>
  /// Project authentication middleware
  /// </summary>
  public class ProjectAuthentication : TIDAuthentication
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectAuthentication"/> class.
    /// </summary>
    public ProjectAuthentication(RequestDelegate next,
      ICwsAccountClient cwsAccountClient,
      IConfigurationStore store,
      ILoggerFactory logger,
      IEntitlementProxy entitlementProxy,
      IServiceExceptionHandler serviceExceptionHandler) : base(next, cwsAccountClient, store, logger, entitlementProxy, serviceExceptionHandler)
    {
    }

    public override bool RequireCustomerUid(HttpContext context)
    {
      var path = context.Request.Path.Value.ToLower();
      var isCustomersEndpoint = path.Contains("/accounthierarchy") || path.Contains("/me");
      if (isCustomersEndpoint)
        return false;
      return true;
    }

    public override bool RequireEntitlementValidation(HttpContext context)
    {
      // These are called from Works Manager when creating a project
      // Works Manager doesn't use the same licensing as WorksOS Currently 
      var acceptedPaths = new List<string> 
      {
        "project/notifyassociation", 
        "project/notifychange", 
        "project/validate"

      };
      var path = context.Request.Path.Value.ToLower();
      foreach (var acceptedPath in acceptedPaths)
      {
        if (path.Contains(acceptedPath))
          return false;
      }

      return base.RequireEntitlementValidation(context);
    }

    /// <summary>
    /// calls coming from e.g. TFA which don't have a user/customer context
    ///    but instead use a TPaaS application context
    /// </summary>
    public override bool InternalConnection(HttpContext context)
    {
      var isDeviceInternalControllerContext =  context.Request.Path.Value.ToLower().Contains("internal/v1/device");
      var isProjectInternalControllerContext = context.Request.Path.Value.ToLower().Contains("internal/v6/project");
      
      if ((isDeviceInternalControllerContext || isProjectInternalControllerContext)
          && context.Request.Method == "GET")
      {
        log.LogDebug($"{nameof(InternalConnection)} Internal connection path: {context.Request.Path}");
        return true;
      }

      return false;
    }
  }
}
