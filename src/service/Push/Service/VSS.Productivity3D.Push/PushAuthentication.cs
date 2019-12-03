using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Push
{
  public class PushAuthentication : TIDAuthentication
  {
    private ILogger log;
    private readonly IProjectProxy projectProxy;


    public PushAuthentication(RequestDelegate next, ICustomerProxy customerProxy, IConfigurationStore store, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler, IProjectProxy projectProxy)
      : base(next, customerProxy, store, logger, serviceExceptionHandler)
    {
      log = logger.CreateLogger<PushAuthentication>();
      this.projectProxy = projectProxy;
    }

    public override bool RequireCustomerUid(HttpContext context)
    {
      log.LogDebug($"{nameof(RequireCustomerUid)} path: {context.Request.Path}");
      return (context.Request.Path.Value.ToLower().Contains("projectevent"));
    }

    public override bool InternalConnection(HttpContext context)
    {
      // Websockets don't support headers, so we may need to use Query Strings, if so map to a header for us
      // https://github.com/aspnet/SignalR/issues/875#issuecomment-333390304
      if (context.Request.Query.ContainsKey(HeaderConstants.AUTHORIZATION))
      {
        log.LogInformation($"Found {HeaderConstants.AUTHORIZATION} in a query param for url `{context.Request.Path}`");
        context.Request.Headers.Add(HeaderConstants.AUTHORIZATION, context.Request.Query[HeaderConstants.AUTHORIZATION]);
      }

      if (context.Request.Query.ContainsKey(HeaderConstants.X_JWT_ASSERTION))
      {
        log.LogInformation($"Found {HeaderConstants.X_JWT_ASSERTION} in a query param for url `{context.Request.Path}`");
        context.Request.Headers.Add(HeaderConstants.X_JWT_ASSERTION, context.Request.Query[HeaderConstants.X_JWT_ASSERTION]);
      }

      // This is only used locally.
      if (context.Request.Headers.ContainsKey(Clients.BaseClient.SKIP_AUTHENTICATION_HEADER))
      {
        return true;
      }
      
      return base.InternalConnection(context);
    }

    /// <summary>
    /// Create 3dpm principal
    /// </summary>
    public override TIDCustomPrincipal CreatePrincipal(string userUid, string customerUid, string customerName,
      string userEmail, bool isApplicationContext, IDictionary<string, string> contextHeaders, string tpaasApplicationName)
    {
      //Delegate customer->project association resolution to the principal object for now as it has execution context and can invalidate cache if required
      // note that userUid may actually be the ApplicationId if isApplicationContext
      return new PushPrincipal(new GenericIdentity(userUid), customerUid, customerName, userEmail, isApplicationContext, tpaasApplicationName, projectProxy, contextHeaders);
    }
  }
}
