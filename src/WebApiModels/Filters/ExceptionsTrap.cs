using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VSS.Raptor.Service.Common.Utilities;
using VSS.Raptor.Service.WebApiModels.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Filters
{
  public class ExceptionsTrap
  {
    private static readonly ILogger log = DependencyInjectionProvider.ServiceProvider.GetService<ILoggerFactory>().CreateLogger<ExceptionsTrap>();
    private readonly RequestDelegate _next;

    public ExceptionsTrap(RequestDelegate next)
    {
      _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
      try
      {
        await _next.Invoke(context);
      }
      catch (ServiceException ex)
      {
        context.Response.StatusCode = (int)ex.Response.StatusCode;
        await context.Response.WriteAsync(ex.GetContent);
      }
      catch (Exception ex)
      {
        try
        {
          context.Response.StatusCode = 500;
          await context.Response.WriteAsync(ex.Message);
        }
        finally
        {
          log.LogCritical("EXCEPTION: {0}, {1}, {2}", ex.Message, ex.Source, ex.StackTrace);
          if (ex is AggregateException)
          {
            var exception = ex as AggregateException;
            log.LogCritical("EXCEPTION AGGREGATED: {0}, {1}, {2}",
                exception.InnerExceptions.Select(i => i.Message).Aggregate((i, j) => i + j),
                exception.InnerExceptions.Select(i => i.Source).Aggregate((i, j) => i + j),
                exception.InnerExceptions.Select(i => i.StackTrace).Aggregate((i, j) => i + j));

          }
        }
      }
    }
  }

  public static class ExceptionsTrapExtensions
  {
    public static IApplicationBuilder UseExceptionTrap(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<ExceptionsTrap>();
    }
  }
}
