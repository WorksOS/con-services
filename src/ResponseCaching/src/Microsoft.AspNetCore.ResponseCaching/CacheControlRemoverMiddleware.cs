using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.ResponseCaching
{
  public class CacheControlRemoverMiddleware
  {
    private readonly RequestDelegate _next;

    public CacheControlRemoverMiddleware(
      RequestDelegate next)
    {
      _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
      //This is a hacky implementation for removing cache-control headers in the response but is enough to do performance testing of hte UI
      //Normally we would want this to be a parameter for caching framework
      //The middleware should be registered before all caching logic occurs
      await _next(httpContext);
      if (httpContext.Response.Headers.ContainsKey(HeaderNames.CacheControl))
        httpContext.Response.Headers.Remove(HeaderNames.CacheControl);

    }

  }
}
