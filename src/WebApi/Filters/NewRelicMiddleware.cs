#if NET_4_7
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VSS.Productivity3D.FileAccess.Service.WebAPI.Filters
{
  //This middleware logs events into NewRelic. This must be added after TIDAuth middleware
  public class NewRelicMiddleware
  {
    private readonly RequestDelegate _next;

    public NewRelicMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
      Dictionary<string, object> _eventAttributes;

      var watch = System.Diagnostics.Stopwatch.StartNew();

      await _next.Invoke(context);

      watch.Stop();

 
        _eventAttributes = new Dictionary<string, object>()
        {
          {"endpoint", context.Request.Path},
          {"elapsedTime", (float) watch.ElapsedMilliseconds},
          {"result", context.Response.StatusCode.ToString() }
        };

        NewRelic.Api.Agent.NewRelic.RecordCustomEvent("FileAccess_Request", _eventAttributes);
    }
  }
}
#endif
