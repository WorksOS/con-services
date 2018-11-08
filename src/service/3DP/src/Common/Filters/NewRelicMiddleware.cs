using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using VSS.Productivity3D.Common.Filters.Authentication.Models;

namespace VSS.Productivity3D.Common.Filters
{
  /// <summary>
  /// This middleware logs events into NewRelic. This must be added after TIDAuth middleware
  /// </summary>
  public class NewRelicMiddleware
  {
    private readonly RequestDelegate nextRequestDelegate;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public NewRelicMiddleware(RequestDelegate nextRequestDelegate)
    {
      this.nextRequestDelegate = nextRequestDelegate;
    }

    /// <summary>
    /// Callback executed on each request made to the server.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> object.</param>
    public async Task Invoke(HttpContext context)
    {
      var watch = System.Diagnostics.Stopwatch.StartNew();
      await nextRequestDelegate.Invoke(context);
      watch.Stop();

      bool cacheUsed = context.Response.Headers.ContainsKey(HeaderNames.Age);
      int cacheAge = 0;

      if (cacheUsed)
      {
        cacheAge = int.Parse(context.Response.Headers[HeaderNames.Age]);
      }

      string name = string.Empty;
      string customerUid = string.Empty;
      string userEmail = string.Empty;
      string customerName = string.Empty;
      string projectUid = string.Empty;
      string origin = string.Empty;
      string tpaasApplication = string.Empty;


      if (context.User is RaptorPrincipal principal)
      {
        name = principal.Identity.Name;
        customerUid = principal.CustomerUid;
        userEmail = principal.UserEmail;
        customerName = principal.CustomerName;
        tpaasApplication = principal.TpaasApplicationName;
      }

      if (context.Request.Query.ContainsKey("projectuid"))
      {
        projectUid = context.Request.Query["projectuid"];
      }

      if (context.Request.Headers.ContainsKey("Origin"))
      {
        origin = context.Request.Headers["Origin"];
      }

      var eventAttributes = new Dictionary<string, object>
      {
        {"endpoint", context.Request.Path.ToString()},
        {"tpaasApplication", tpaasApplication },
        {"cacheUsed", cacheUsed.ToString()},
        {"cacheAge", (float) Convert.ToDouble(cacheAge)},
        {"userUid", name},
        {"customerUid", customerUid},
        {"userName", userEmail},
        {"customerName", customerName},
        {"executionTime", (float) Convert.ToDouble(watch.ElapsedMilliseconds)},
        {"projectUid", projectUid},
        {"origin", origin},
        {"result", context.Response.StatusCode.ToString()}
      };

      NewRelic.Api.Agent.NewRelic.RecordCustomEvent("3DPM_Request", eventAttributes);
    }
  }
}
