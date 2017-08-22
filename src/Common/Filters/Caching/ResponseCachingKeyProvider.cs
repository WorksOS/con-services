using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace VSS.Productivity3D.Common.Filters
{

  //Based on a reference implementation
  public class CustomResponseCachingKeyProvider : IResponseCachingKeyProvider
  {
    // Use the record separator for delimiting components of the cache key to avoid possible collisions
    private static readonly char KeyDelimiter = '\x1e';
    internal static readonly char ProjectDelimiter = '\x1f';

    private readonly ObjectPool<StringBuilder> _builderPool;
    private readonly ResponseCachingOptions _options;

    public CustomResponseCachingKeyProvider(ObjectPoolProvider poolProvider, IOptions<ResponseCachingOptions> options)
    {
      if (poolProvider == null)
      {
        throw new ArgumentNullException(nameof(poolProvider));
      }
      if (options == null)
      {
        throw new ArgumentNullException(nameof(options));
      }

      _builderPool = poolProvider.CreateStringBuilderPool();
      _options = options.Value;
    }

    public IEnumerable<string> CreateLookupVaryByKeys(ResponseCachingContext context)
    {
      return new string[] { CreateStorageVaryByKey(context) };
    }

    // GET<delimiter>/PATH
    //Not testable!
    public string CreateBaseKey(ResponseCachingContext context)
    {
      if (context == null)
      {
        throw new ArgumentNullException(nameof(context));
      }

      var request = context.HttpContext.Request;
      return GenerateBaseKeyFromRequest(request);
    }

    public string GenerateBaseKeyFromRequest(HttpRequest request)
    {
      var builder = _builderPool.Get();

      try
      {
        builder
          .Append(request.Method.ToUpperInvariant())
          .Append(KeyDelimiter);

        if (_options.UseCaseSensitivePaths)
        {
          builder.Append(request.Path.Value);
        }
        else
        {
          builder.Append(request.Path.Value.ToUpperInvariant());
        }

        if (request.Query.ContainsKey("projectUid"))
          builder.Append(ProjectDelimiter).Append(request.Query["projectUid"]);

        return builder.ToString();
      }
      finally
      {
        _builderPool.Return(builder);
      }
    }

    // BaseKey<delimiter>H<delimiter>HeaderName=HeaderValue<delimiter>Q<delimiter>QueryName=QueryValue
    public string CreateStorageVaryByKey(ResponseCachingContext context)
    {
      if (context == null)
      {
        throw new ArgumentNullException(nameof(context));
      }

      var varyByRules = context.CachedVaryByRules;
      if (varyByRules == null)
      {
        throw new InvalidOperationException($"{nameof(CachedVaryByRules)} must not be null on the {nameof(ResponseCachingContext)}");
      }

      if ((StringValues.IsNullOrEmpty(varyByRules.Headers) && StringValues.IsNullOrEmpty(varyByRules.QueryKeys)))
      {
        return varyByRules.VaryByKeyPrefix;
      }

      var request = context.HttpContext.Request;
      var builder = _builderPool.Get();

      try
      {
        // Prepend with the Guid of the CachedVaryByRules
        builder.Append(varyByRules.VaryByKeyPrefix);

        // Vary by headers
        if (varyByRules?.Headers.Count > 0)
        {
          // Append a group separator for the header segment of the cache key
          builder.Append(KeyDelimiter)
            .Append('H');

          foreach (var header in varyByRules.Headers)
          {
            builder.Append(KeyDelimiter)
              .Append(header)
              .Append("=")
              // TODO: Perf - iterate the string values instead?
              .Append(context.HttpContext.Request.Headers[header]);
          }
        }

        // Vary by query keys
        if (varyByRules?.QueryKeys.Count > 0)
        {
          // Append a group separator for the query key segment of the cache key
          builder.Append(KeyDelimiter)
            .Append('Q');

          if (varyByRules.QueryKeys.Count == 1 && string.Equals(varyByRules.QueryKeys[0], "*", StringComparison.Ordinal))
          {
            // Vary by all available query keys
            foreach (var query in context.HttpContext.Request.Query.OrderBy(q => q.Key, StringComparer.OrdinalIgnoreCase))
            {
              builder.Append(KeyDelimiter)
                .Append(query.Key.ToUpperInvariant())
                .Append("=")
                .Append(query.Value);
            }
          }
          else
          {
            foreach (var queryKey in varyByRules.QueryKeys)
            {
              builder.Append(KeyDelimiter)
                .Append(queryKey)
                .Append("=")
                // TODO: Perf - iterate the string values instead?
                .Append(context.HttpContext.Request.Query[queryKey]);
            }
          }
        }

        return builder.ToString();
      }
      finally
      {
        _builderPool.Return(builder);
      }
    }
  }

  public static class CachingKeyExtensions
  {
    public static Guid ExtractProjectGuidFromKey(this IResponseCachingKeyProvider cachingKeyProvider, string key)
    {
      if (key.IndexOf(CustomResponseCachingKeyProvider.ProjectDelimiter) <= 0) return Guid.Empty;
      var indexOfDelimiter = key.LastIndexOf(CustomResponseCachingKeyProvider.ProjectDelimiter);
      return Guid.Parse(key.Substring(indexOfDelimiter + 1, 36));
    }
  }
}