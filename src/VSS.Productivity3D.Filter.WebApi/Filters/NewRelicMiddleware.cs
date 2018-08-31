using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Filter.WebAPI.Filters
{
  /// <summary>
  /// This middleware logs events into NewRelic. This must be added after TIDAuth middleware
  /// </summary>
  public class NewRelicMiddleware
  {
    private readonly RequestDelegate NextRequestDelegate;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="nextRequestDelegate"></param>
    public NewRelicMiddleware(RequestDelegate nextRequestDelegate)
    {
      this.NextRequestDelegate = nextRequestDelegate;
    }

    /// <summary>
    /// Callback executed on each request made to the server.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> object.</param>
    public async Task Invoke(HttpContext context)
    {
      var watch = System.Diagnostics.Stopwatch.StartNew();

      await this.NextRequestDelegate.Invoke(context);

      watch.Stop();

      if (context.User is TIDCustomPrincipal principal)
      {
        string projectUid = String.Empty;
        string origin = String.Empty;

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
          {"userUid", principal.Identity.Name.ToString()},
          {"customerUid", principal.CustomerUid.ToString()},
          {"userName", principal.UserEmail.ToString()},
          {"customerName", principal.CustomerName.ToString()},
          {"elapsedTime", (Single) watch.ElapsedMilliseconds},
          {"projectUid",projectUid.ToString() },
          {"origin",origin.ToString() },
          {"result", context.Response.StatusCode.ToString() }
        };

        NewRelic.Api.Agent.NewRelic.RecordCustomEvent("3DPM_Filter_Request", eventAttributes);
      }
    }
  }
}
