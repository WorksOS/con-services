using Microsoft.AspNetCore.ResponseCaching.Internal;
using System;

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

      cache.Set(key, null, TimeSpan.FromTicks(1));

      // Response caching behaves as an HTTP cache.
      // To invalidate the removed item now rather than wait for the natural cleanup we need to try to Get the item, forcing it to be invalidated.
      cache.Get(key);
    }
  }
}