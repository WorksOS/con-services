using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.WebApi.Common;

namespace VSS.MasterData.Landfill.WebAPI.Middleware
{
  /// <summary>
  ///   This middleware logs events into NewRelic. This must be added after TIDAuth middleware
  /// </summary>
  public class NewRelicMiddleware
  {
    private readonly RequestDelegate NextRequestDelegate;

    /// <summary>
    ///   Default constructor.
    /// </summary>
    /// <param name="nextRequestDelegate"></param>
    public NewRelicMiddleware(RequestDelegate nextRequestDelegate)
    {
      NextRequestDelegate = nextRequestDelegate;
    }

    /// <summary>
    ///   Callback executed on each request made to the server.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext" /> object.</param>
    public async Task Invoke(HttpContext context)
    {

      var watch = Stopwatch.StartNew();
      await NextRequestDelegate.Invoke(context);

      watch.Stop();

      if (context.User is TIDCustomPrincipal principal)
      {
        var projectUid = string.Empty;
        var origin = string.Empty;

        if (context.Request.Query.ContainsKey("projectuid")) projectUid = context.Request.Query["projectuid"];

        if (context.Request.Headers.ContainsKey("Origin")) origin = context.Request.Headers["Origin"];

        var eventAttributes = new Dictionary<string, object>
        {
          {"endpoint", context.Request.Path.ToString()},
          {"customerUid", principal.CustomerUid},
          {"userName", principal.UserEmail},
          {"executionTime", (float) watch.ElapsedMilliseconds},
          {"projectUid", projectUid},
          {"origin", origin},
          {"result", context.Response.StatusCode.ToString()}
        };

        NewRelic.Api.Agent.NewRelic.RecordCustomEvent("LandfillService_Request", eventAttributes);
      }
    }
  }
}