using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Extensions;

namespace VSS.Productivity3D.Common.Filters.Caching
{
  //Based on a reference implementation
  public class CustomResponseCachingKeyProvider : IResponseCachingKeyProvider
  {
    // Use the record separator for delimiting components of the cache key to avoid possible collisions
    private const char KEY_DELIMITER = '\x1e';

    internal static readonly char ProjectDelimiter = '\x1f';
    internal static readonly char FilterDelimiter = '\x1d';

    private readonly ObjectPool<StringBuilder> builderPool;
    private readonly ResponseCachingOptions options;
    private readonly IFilterServiceProxy filterServiceProxy;
    private ILogger<CustomResponseCachingKeyProvider> logger;

    public CustomResponseCachingKeyProvider(ObjectPoolProvider poolProvider, IFilterServiceProxy filterProxy, ILogger<CustomResponseCachingKeyProvider> logger, IOptions<ResponseCachingOptions> options)
    {
      if (poolProvider == null)
      {
        throw new ArgumentNullException(nameof(poolProvider));
      }
      if (options == null)
      {
        throw new ArgumentNullException(nameof(options));
      }

      this.builderPool = poolProvider.CreateStringBuilderPool();
      this.options = options.Value;
      this.filterServiceProxy = filterProxy;
      this.logger = logger;
    }

    public IEnumerable<string> CreateLookupVaryByKeys(ResponseCachingContext context)
    {
      return new[] { CreateStorageVaryByKey(context) };
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
      var builder = this.builderPool.Get();
      builder.Clear();

      try
      {
        //In case of multitier cache the top level will be project UID. So ALL cache attributes MUST have VaryTules to invalidate underlying tier properly
        //FOr the requests with ProjectUID (v1 requests) standard rules apply
        if (request.Query.ContainsKey("projectUid"))
        {
          builder.Append(request.Query["projectUid"][0].GetProjectCacheKey());
        }
        else
        {
          builder
            .Append(request.Method.ToUpperInvariant())
            .Append(KEY_DELIMITER);

          builder.Append(this.options.UseCaseSensitivePaths
            ? request.Path.Value
            : request.Path.Value.ToUpperInvariant());
        }
        var baseKey = builder.ToString();
        logger?.LogDebug($"Base key {baseKey}");
        return baseKey;
      }
      finally
      {
        this.builderPool.Return(builder);
      }
    }

    //Here fun begins. VaryRule MUST include proper filter uid hashing
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

      var builder = this.builderPool.Get();
      builder.Clear();
      var request = context.HttpContext.Request;

      try
      {

        builder
          .Append(request.Method.ToUpperInvariant())
          .Append("QQ")
          .Append(request.Scheme.ToUpperInvariant())
          .Append("QQ")
          .Append(request.Host.Value.ToUpperInvariant());

          builder
            .Append(request.PathBase.Value.ToUpperInvariant())
            .Append(request.Path.Value.ToUpperInvariant());

        // Vary by headers
        // This is the default implementation
        if (varyByRules.Headers.Count > 0)
        {
          // Append a group separator for the header segment of the cache key
          builder.Append(KEY_DELIMITER)
            .Append('H');

          foreach (var header in varyByRules.Headers)
          {
            builder.Append(KEY_DELIMITER)
              .Append(header)
              .Append("=")
              // TODO: Perf - iterate the string values instead?
              .Append(context.HttpContext.Request.Headers[header]);
          }
        }

        // Vary by query keys
        var projectUid = string.Empty;

        if (varyByRules.QueryKeys.Count > 0)
        {
          // Append a group separator for the query key segment of the cache key
          builder.Append(KEY_DELIMITER)
            .Append('Q');

          var projectUids = context.HttpContext.Request.Query
            ?.Where(s => string.Equals(s.Key, "projectUid", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault().Value;

          if (projectUids.HasValue && projectUids.Value.Count > 0)
          {
            projectUid = projectUids.Value[0];
          }

          if (varyByRules.QueryKeys.Count == 1 && string.Equals(varyByRules.QueryKeys[0], "*", StringComparison.Ordinal))
          {
            // Vary by all available query keys
            foreach (var query in context.HttpContext.Request.Query.OrderBy(q => q.Key,
              StringComparer.OrdinalIgnoreCase))
            {
              if (query.Key.ToUpperInvariant() != "TIMESTAMP")
              {
                var value = query.Value;

                if (!string.IsNullOrEmpty(projectUid) &&  query.Key.ToUpperInvariant() == "FILTERUID")
                {
                  value = GenerateFilterHash(projectUid, query.Value[0],
                    context.HttpContext.Request.Headers.GetCustomHeaders(true)).ToString();
                }

                builder.Append(KEY_DELIMITER)
                  .Append(query.Key.ToUpperInvariant())
                  .Append("=")
                  .Append(value);
              }
            }
          }
          else
          {
            foreach (var queryKey in varyByRules.QueryKeys)
            {
              if (queryKey.ToUpperInvariant() != "TIMESTAMP")
              {
                var value = context.HttpContext.Request.Query[queryKey];

                if (!string.IsNullOrEmpty(projectUid) && queryKey.ToUpperInvariant() == "FILTERUID")
                {
                  value = GenerateFilterHash(projectUid, queryKey,
                    context.HttpContext.Request.Headers.GetCustomHeaders(true)).ToString();
                }

                builder.Append(KEY_DELIMITER)
                  .Append(queryKey)
                  .Append("=")
                  // TODO: Perf - iterate the string values instead?
                  .Append(value);
              }
            }
          }
        }
        var key = builder.ToString();
        logger?.LogDebug($"Cache key: {key}");
        return key;
      }
      finally
      {
        this.builderPool.Return(builder);
      }
    }

    /// <remarks>
    /// Method must be run synchronously to avoid any filter caching read anomolies.
    /// </remarks>>
    private int GenerateFilterHash(string projectUid, string filterUid, IDictionary<string, string> headers)
    {
      var filter = this.filterServiceProxy.GetFilter(projectUid, filterUid, headers).Result;
      if (filter == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Filter not found, id: { filterUid }"));
      }

      return JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filter.FilterJson).GetHashCode();
    }
  }

}
