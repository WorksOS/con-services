using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.ResponseCaching.Internal
{
    public class CustomCachingPolicyProvider : ResponseCachingPolicyProvider
    {

        private static readonly CacheControlHeaderValue EmptyCacheControl = new CacheControlHeaderValue();

        public override bool IsRequestCacheable(ResponseCachingContext context)
        {
            // Verify the method
            var request = context.HttpContext.Request;
            if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
            {
                return false;
            }


            var typedHeaders = context.HttpContext.Response.GetTypedHeaders();
            var requestCacheControl = typedHeaders.CacheControl ?? EmptyCacheControl;


            // Verify request cache-control parameters
            if (!StringValues.IsNullOrEmpty(request.Headers[HeaderNames.CacheControl]))
            {
                if (requestCacheControl.NoCache)
                {
                    return false;
                }
            }
            else
            {
                // Support for legacy HTTP 1.0 cache directive
                var pragmaHeaderValues = request.Headers[HeaderNames.Pragma];
                foreach (var directive in pragmaHeaderValues)
                {
                    if (string.Equals("no-cache", directive, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
