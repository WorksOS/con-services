using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace VSS.Productivity3D.Common.Filters.Caching
{
  //Based on reference implementation
  public class CustomCachingPolicyProvider : ResponseCachingPolicyProvider
  {
    private static readonly CacheControlHeaderValue emptyCacheControl = new CacheControlHeaderValue();

    public override bool AttemptResponseCaching(ResponseCachingContext context)
    {
      var request = context.HttpContext.Request;

      // Verify the method
      return HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method);
    }

    public override bool IsResponseCacheable(ResponseCachingContext context)
    {
      var typedHeaders = context.HttpContext.Response.GetTypedHeaders();
      var responseHeaders = typedHeaders.CacheControl ?? emptyCacheControl;

      if (responseHeaders.NoStore)
      {
        return false;
      }

      if (responseHeaders.NoCache)
      {
        return false;
      }

      var response = context.HttpContext.Response;

      // Do not cache responses with Set-Cookie headers
      if (!StringValues.IsNullOrEmpty(response.Headers[HeaderNames.SetCookie]))
      {
        return false;
      }

      // Do not cache responses varying by *
      var varyHeader = response.Headers[HeaderNames.Vary];
      if (varyHeader.Count == 1 && string.Equals(varyHeader, "*", StringComparison.OrdinalIgnoreCase))
      {
        return false;
      }

      // Check private
      if (responseHeaders.Private)
      {
        return false;
      }

      // Check response code
      if (response.StatusCode != StatusCodes.Status200OK)
      {
        return false;
      }

      // Check response freshness
      if (!typedHeaders.Date.HasValue)
      {
        if (!responseHeaders.SharedMaxAge.HasValue &&
            !responseHeaders.MaxAge.HasValue &&
            context.ResponseTime.Value >= typedHeaders.Expires)
        {
          return false;
        }
      }
      else
      {
        var age = context.ResponseTime.Value - typedHeaders.Date.Value;

        // Validate shared max age
        var sharedMaxAge = responseHeaders.SharedMaxAge;
        if (age >= sharedMaxAge)
        {
          return false;
        }

        if (!sharedMaxAge.HasValue)
        {
          // Validate max age
          var maxAge = responseHeaders.MaxAge;
          if (age >= maxAge)
          {
            return false;
          }

          if (!maxAge.HasValue)
          {
            // Validate expiration
            if (context.ResponseTime.Value >= typedHeaders.Expires)
            {
              return false;
            }
          }
        }
      }

      return true;
    }
  }
}
