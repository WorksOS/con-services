using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Common.Filters.Authentication
{
  /// <summary>
  /// 3dpm Authentication middleware
  /// </summary>
  public class RaptorAuthentication : TIDAuthentication
  {
    private readonly IProjectProxy projectProxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="RaptorAuthentication"/> class.
    /// </summary>
    public RaptorAuthentication(RequestDelegate next,
      ICwsAccountClient cwsAccountClient,
      IConfigurationStore store,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy) : base(next, cwsAccountClient, store, logger, serviceExceptionHandler)
    {
      this.projectProxy = projectProxy;
    }

    /// <summary>
    /// 3dpm specific logic for skipping authentication
    /// </summary>
    public override bool InternalConnection(HttpContext context)
    {
      //HACK allow internal connections without authn for tagfile submission by the harvester
      // TAG Files come via TPAAS endpoints
      return
        context.Request.Path.Value.Contains("tagfiles") && 
        context.Request.Method == "POST" &&
        !context.Request.Headers.ContainsKey("X-Jwt-Assertion") &&
        !context.Request.Headers.ContainsKey("Authorization");
    }

    /// <summary>
    /// 3dpm specific logic for requiring customerUid
    ///    The v1 TAG file submission end point does not require a customer UID to be provided
    ///        However there is some schizophrenia here as we need to support UI manual tag file submission 
    ///            WITH proper authn\z as well
    ///    The v2 patches for EarthWorks cutfill end point does not require customerUID
    /// </summary>
    public override bool RequireCustomerUid(HttpContext context)
    {
      var isTagFile = context.Request.Path.Value.ToLower().Contains("tagfiles");
      var isPatch = context.Request.Path.Value.ToLower().Contains("/device/patches");

      var containsCustomerUid = context.Request.Headers.ContainsKey("X-VisionLink-CustomerUid");
      if (isTagFile && context.Request.Method == "POST" && !containsCustomerUid)
      {
        log.LogDebug($"{nameof(RequireCustomerUid)} Tagfiles request doesn't require customerUid. path: {context.Request.Path}");
        return false;
      }
      if (isPatch && context.Request.Method == "GET" && !containsCustomerUid)
      {
        log.LogDebug($"{nameof(RequireCustomerUid)} Patch request doesn't require customerUid. path: {context.Request.Path}");
        return false;
      }

      return true;
    }

    /// <summary>
    /// Create 3dpm principal
    /// </summary>
    public override TIDCustomPrincipal CreatePrincipal(string userUid, string customerUid, string customerName, 
      string userEmail, bool isApplicationContext, IDictionary<string, string> contextHeaders, string tpaasApplicationName)
    {
      //Delegate customer->project association resolution to the principal object for now as it has execution context and can invalidate cache if required
      // note that userUid may actually be the ApplicationId if isApplicationContext
      return new RaptorPrincipal(new GenericIdentity(userUid), customerUid, customerName, userEmail, isApplicationContext, tpaasApplicationName, projectProxy, contextHeaders);
    }

  }
}
