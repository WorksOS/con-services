using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Push
{
  public class PushAuthentication : TIDAuthentication
  {
    private ILogger log;
    

    public PushAuthentication(RequestDelegate next, ICustomerProxy customerProxy, IConfigurationStore store, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler) : base(next, customerProxy, store, logger, serviceExceptionHandler)
    {
      log = logger.CreateLogger<PushAuthentication>();
    }

    public override bool RequireCustomerUid(HttpContext context)
    {
      return false;
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
  }
}
