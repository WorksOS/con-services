using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching;

namespace Common.Filters
{
    public class ReponseCacheQueryFeature
    {

        private readonly RequestDelegate _next;

        public ReponseCacheQueryFeature(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Features.Get<IResponseCachingFeature>() != null)
                context.Features.Get<IResponseCachingFeature>().VaryByQueryKeys = new[] {"*"};
            await _next.Invoke(context);
        }
    }

    public static class ReponseCacheQueryFeatureExtensions
    {
        public static IApplicationBuilder UseReponseCacheQueryFeature(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ReponseCacheQueryFeature>();
        }
    }
}
