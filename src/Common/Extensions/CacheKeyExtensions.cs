using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        cache.Set(projectUid.GetProjectCacheKey(),null,TimeSpan.FromTicks(1));
      }

      public static void InvalidateReponseCacheForProject(this IResponseCache cache, Guid projectUid)
      {
        cache.Set(projectUid.ToString().GetProjectCacheKey(), null, TimeSpan.FromTicks(1));
      }

  }
}
