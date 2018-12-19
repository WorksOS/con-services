using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace VSS.WebApi.Common
{
  public class RequestTraceMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTraceMiddleware> log;



    public RequestTraceMiddleware(RequestDelegate next, ILoggerFactory logger)
    {
      log = logger.CreateLogger<RequestTraceMiddleware>();
      this._next = next;
    }

    /// <summary>
    /// Invokes the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
      log.LogInformation($"Request {context.Request.Method} {context.Request.Path}");
      var watch = Stopwatch.StartNew();
      await this._next.Invoke(context);
      watch.Stop();
      log.LogInformation($"Response {context.Response.StatusCode} {watch.ElapsedMilliseconds}ms");
    }
  }

}
