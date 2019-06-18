using System;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;

namespace VSS.WebApi.Common
{
  public class ExceptionsTrap
  {
    private readonly ILogger log;
    private readonly RequestDelegate _next;

    public ExceptionsTrap(RequestDelegate next, ILogger<ExceptionsTrap> logger)
    {
      _next = next;
      log = logger;
    }

    public async Task Invoke(HttpContext context)
    {
      try
      {
        await _next.Invoke(context);
      }
      catch (AuthenticationException)
      {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
      }
      catch (ServiceException ex)
      {
        await HandleServiceException(context, ex);
      }
      catch (Exception ex)
      {
        //Exceptions may get wrapped depending on order of middleware
        if (ex.InnerException is ServiceException innerException)
        {
          await HandleServiceException(context, innerException);
        }
        else
        {
          try
          {
            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(ex.Message);
          }
          finally
          {
            log.LogCritical(ex, $"EXCEPTION: {ex.Message} {ex.Source}");

            if (ex is AggregateException exception)
            {
              log.LogCritical("EXCEPTION AGGREGATED: {0}, {1}, {2}",
                exception.InnerExceptions.Select(i => i.Message).Aggregate((i, j) => i + j),
                exception.InnerExceptions.Select(i => i.Source).Aggregate((i, j) => i + j),
                exception.InnerExceptions.Select(i => i.StackTrace).Aggregate((i, j) => i + j));
            }
          }
        }
      }
    }

    private Task HandleServiceException(HttpContext context, ServiceException ex)
    {
      log.LogWarning(ex, $"Service exception: {ex.Source} {ex.GetFullContent} statusCode: {ex.Code}");
      context.Response.StatusCode = (int)ex.Code;
      context.Response.ContentType = ContentTypeConstants.ApplicationJson;
      
      context.Response.WriteAsync(ex.GetContent);
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
