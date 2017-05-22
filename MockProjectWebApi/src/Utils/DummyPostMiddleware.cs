using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Utils;

namespace src.Utils
{
    public class ExceptionDummyPostMiddleware
    {
        private readonly ILogger log;
        private readonly RequestDelegate _next;

        public ExceptionDummyPostMiddleware(RequestDelegate next, ILoggerFactory logger)
        {
            log = logger.CreateLogger<ExceptionsTrap>();
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.GetUri().AbsolutePath.Contains("dummy"))
            {
                context.Response.StatusCode = 200;
            }
            await _next.Invoke(context);
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
