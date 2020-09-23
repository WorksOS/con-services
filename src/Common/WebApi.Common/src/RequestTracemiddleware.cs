using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace VSS.WebApi.Common
{
  public class RequestTraceMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTraceMiddleware> _log;

    public RequestTraceMiddleware(RequestDelegate next, ILoggerFactory logger)
    {
      _log = logger.CreateLogger<RequestTraceMiddleware>();
      _next = next;
    }

    /// <summary>
    /// Invokes the specified context.
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
      switch (context.Request.Path.Value)
      {
        case string path when path.Contains("/ping"):
          {
            await _next.Invoke(context);
            break;
          }
        default:
          {
            var watch = Stopwatch.StartNew();
            _log.LogInformation($"Request {context.Request.Method} {context.Request.Path} {context.Request.QueryString.Value}");

            await _next.Invoke(context);

            _log.LogInformation($"Response {context.Response.StatusCode} {watch.ElapsedMilliseconds}ms");
            break;
          }
      }
    }
  }
}
