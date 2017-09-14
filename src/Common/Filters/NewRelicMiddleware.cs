using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Productivity3D.Common.Filters.Authentication.Models;

namespace VSS.Productivity3D.Common.Filters
{
  //This middleware logs events into NewRelic. This must be added after TIDAuth middleware
  public class NewRelicMiddleware
  {
    private readonly RequestDelegate _next;
    private Dictionary<string, object> _eventAttributes;

    public NewRelicMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task Invoke(HttpContext context)
    {

      var watch = System.Diagnostics.Stopwatch.StartNew();

      await _next.Invoke(context);

      watch.Stop();

      if (context.User is RaptorPrincipal principal)
      {
        string projectUid=String.Empty;
        string origin = String.Empty;

        if (context.Request.Query.ContainsKey("projectuid"))
          projectUid = context.Request.Query["projectuid"];

        if (context.Request.Headers.ContainsKey("Origin"))
          origin = context.Request.Headers["Origin"];


        _eventAttributes = new Dictionary<string, object>()
        {
          {"endpoint", context.Request.Path},
          {"useruid", principal.Identity.Name},
          {"customeruid", principal.CustomerUid},
          {"userName", principal.userEmail},
          {"customerName", principal.customerName},
          {"elapsedTime", (Single) watch.ElapsedMilliseconds},
          {"projectUid",projectUid },
          {"origin",origin },
          {"result", context.Response.StatusCode.ToString() }
        };

        NewRelic.Api.Agent.NewRelic.RecordCustomEvent("3DPM_Request", _eventAttributes);
      }


    }
  }
}
