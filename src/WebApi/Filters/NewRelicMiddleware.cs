#if NET_4_7
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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

 
        _eventAttributes = new Dictionary<string, object>()
        {
          {"endpoint", context.Request.Path},
          {"elapsedTime", (Single) watch.ElapsedMilliseconds},
          {"result", context.Response.StatusCode.ToString() }
        };

        NewRelic.Api.Agent.NewRelic.RecordCustomEvent("TagFileAuth_Request", _eventAttributes);
    }
  }
}
#endif