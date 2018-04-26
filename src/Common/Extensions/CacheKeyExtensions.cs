using System;
using Microsoft.AspNetCore.ResponseCaching.Internal;

namespace VSS.Productivity3D.Common.Extensions
{
  public static class CacheKeyExtensions
  {
    public static string GetProjectCacheKey(this string projectUid)
    {
      return $"PRJUID={projectUid.ToUpperInvariant()}";
    }

    public static void InvalidateReponseCacheForProject(this IResponseCache cache, string projectUid)
    {
      InvalidateResponseCacheItem(cache, projectUid);
    }

    public static void InvalidateReponseCacheForProject(this IResponseCache cache, Guid projectUid)
    {
      InvalidateResponseCacheItem(cache, projectUid.ToString());
    }

    private static void InvalidateResponseCacheItem(IResponseCache cache, string projectUid)
    {
      var key = projectUid.GetProjectCacheKey();

      cache.Set(null, key, null, TimeSpan.FromTicks(1));

      // Response caching behaves as an HTTP cache with the framework default for _expirationScanFrequency set to one minute.
      // To invalidate the expired item now rather than wait for the next scan we need to try to read or get the object. This forces the framework to scan for expired items.
      cache.Get(key);

      // Custom VSS solution to remove specific project related response objects from the cache.
      ((IExtendedResponseCache)cache).ClearAsync(key);
    }
  }
}
