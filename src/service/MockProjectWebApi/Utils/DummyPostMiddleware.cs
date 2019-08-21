using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MockProjectWebApi.Utils
{
  public class ExceptionDummyPostMiddleware
  {
    private readonly RequestDelegate _next;

    public ExceptionDummyPostMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
      if (context.Request.Path.Value.Contains("dummy"))
      {
        context.Response.StatusCode = 200;
      }
      else
      {
        await _next.Invoke(context);
      }
    }
  }

  public static class ExceptionDummyPostMiddlewareExtensions
  {
    public static IApplicationBuilder UseExceptionDummyPostMiddleware(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<ExceptionDummyPostMiddleware>();
    }
  }
}
