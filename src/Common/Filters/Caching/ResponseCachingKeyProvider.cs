using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Common.Filters
{

  //Based on a reference implementation
  public class CustomResponseCachingKeyProvider : IResponseCachingKeyProvider
  {
    // Use the record separator for delimiting components of the cache key to avoid possible collisions
    private static readonly char KeyDelimiter = '\x1e';
    internal static readonly char ProjectDelimiter = '\x1f';
    internal static readonly char FilterDelimiter = '\x1d';

    private readonly ObjectPool<StringBuilder> _builderPool;
    private readonly ResponseCachingOptions _options;
    private readonly IFilterServiceProxy filterServiceProxy;

    public CustomResponseCachingKeyProvider(ObjectPoolProvider poolProvider, IFilterServiceProxy filterProxy, IOptions<ResponseCachingOptions> options)
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
      filterServiceProxy = filterProxy;
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
        {
          builder.Append(ProjectDelimiter).Append(request.Query["projectUid"]);

          if (request.Query.ContainsKey("filterUid"))
            builder.Append(FilterDelimiter).Append(GenerateFilterHash(request.Query["projectUid"],
              request.Query["filterUid"], request.Headers.GetCustomHeaders()));
        }


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
            foreach (var query in context.HttpContext.Request.Query.OrderBy(q => q.Key,
              StringComparer.OrdinalIgnoreCase))
            {
              if (query.Key.ToUpperInvariant() != "FILTERUID")
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
              if (queryKey.ToUpperInvariant() != "FILTERUID")
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


    private int GenerateFilterHash(string projectUid, string filterUid, IDictionary<string,string> headers)
    {
      var filterJson = filterServiceProxy.GetFilter(projectUid, filterUid, headers).Result.FilterJson;

      var filter = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filterJson);
      return filter.GetHashCode();
 
      /*
      return JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filterServiceProxy
        .GetFilter(projectUid, filterUid, headers).Result.FilterJson).GetHashCode();
        */
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

    public static int ExtractFilterHashFromKey(this IResponseCachingKeyProvider cachingKeyProvider, string key)
    {
      if (key.IndexOf(CustomResponseCachingKeyProvider.FilterDelimiter) <= 0) return -1;
      var indexOfDelimiter = key.LastIndexOf(CustomResponseCachingKeyProvider.FilterDelimiter);
      return int.Parse(Regex.Match(key.Substring(indexOfDelimiter + 1), @"\d+").Value);
    }
  }
}