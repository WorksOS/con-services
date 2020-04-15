using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Scheduler.WebAPI.Middleware
{
  /// <summary>
  /// Scheduler authentication middleware
  /// </summary>
  public class SchedulerAuthentication : TIDAuthentication
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public SchedulerAuthentication(RequestDelegate next,
      ICwsAccountClient cwsAccountClient,
      IConfigurationStore store,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler) : base(next, cwsAccountClient, store, logger, serviceExceptionHandler)
    {
    }

    /// <summary>
    /// Scheduler specific logic for skipping authentication
    /// </summary>
    public override bool InternalConnection(HttpContext context)
    {
      //HACK allow internal connections without authn for hangfire dashboard
      return
        context.Request.Path.Value.StartsWith("/hangfire") &&
        !context.Request.Headers.ContainsKey("X-Jwt-Assertion") &&
        !context.Request.Headers.ContainsKey("Authorization");
    }

  }
}
