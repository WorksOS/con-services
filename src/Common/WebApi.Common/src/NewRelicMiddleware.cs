using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VSS.WebApi.Common
{
  /// <summary>
  /// This middleware logs events into NewRelic. This must be added after TIDAuth middleware
  /// </summary>
  public class NewRelicMiddleware
  {
    public static string ServiceName = string.Empty;
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


      var projectUid = string.Empty;
      var origin = string.Empty;
      var customerUid = string.Empty;
      var userEmail = string.Empty;

      if (context.User is TIDCustomPrincipal principal)
      {
        customerUid = principal.CustomerUid;
        userEmail = principal.UserEmail;
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
          { "endpoint", context.Request.Path.ToString() },
          { "customerUid", customerUid },
          { "userName", userEmail },
          { "executionTime",(Single)watch.ElapsedMilliseconds },
          { "projectUid", projectUid },
          { "origin",origin },
          { "result", context.Response.StatusCode.ToString() }
        };

        NewRelic.Api.Agent.NewRelic.RecordCustomEvent(ServiceName, eventAttributes);
      
    }
  }
}
