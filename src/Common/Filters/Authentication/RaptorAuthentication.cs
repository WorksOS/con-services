using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Common.Filters.Authentication
{
  /// <summary>
  /// 3dpm Authentication middleware
  /// </summary>
  public class RaptorAuthentication : TIDAuthentication
  {
    private readonly IProjectListProxy projectListProxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="RaptorAuthentication"/> class.
    /// </summary>
    public RaptorAuthentication(RequestDelegate next,
      ICustomerProxy customerProxy,
      IConfigurationStore store,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy) : base(next, customerProxy, store, logger, serviceExceptionHandler)
    {
      this.projectListProxy = projectListProxy;
    }

    /// <summary>
    /// 3dpm specific logic for skipping authentication
    /// </summary>
    public override bool InternalConnection(HttpContext context)
    {
      //HACK allow internal connections without authn for tagfile submission by the harvester

      return
        (context.Request.Path.Value.Contains("api/v1/tagfiles") ||
         context.Request.Path.Value.Contains("api/v2/tagfiles")) && context.Request.Method == "POST" &&
        context.Request.HttpContext.Connection.RemoteIpAddress.ToString().StartsWith("10.") &&
        !context.Request.Headers.ContainsKey("X-Jwt-Assertion") &&
        !context.Request.Headers.ContainsKey("Authorization");
    }

    /// <summary>
    /// 3dpm specific logic for requiring customerUid
    /// </summary>
    public override bool RequireCustomerUid(HttpContext context)
    {
      // The v1 TAG file submission end point does not require a customer UID to be provided
      // However there is some schizophrenia here as we need to support UI manual tag file submission WITH proper authn\z as well
      return !(
        (context.Request.Path.Value.Contains("api/v1/tagfiles") ||
         context.Request.Path.Value.Contains("api/v2/tagfiles")) && context.Request.Method == "POST"
         && !context.Request.Headers.ContainsKey("X-VisionLink-CustomerUid")
              );
    }

    /// <summary>
    /// Create 3dpm principal
    /// </summary>
    public override TIDCustomPrincipal CreatePrincipal(string userUid, string customerUid, string customerName, 
      string userEmail, bool isApplicationContext, IDictionary<string, string> contextHeaders, string tpaasApplicationName)
    {
      //Delegate customer->project association resolution to the principal object for now as it has execution context and can invalidate cache if required
      // note that userUid may actually be the ApplicationId if isApplicationContext
      return new RaptorPrincipal(new GenericIdentity(userUid), customerUid, customerName, userEmail, isApplicationContext, tpaasApplicationName, projectListProxy, contextHeaders);
    }

  }
}
