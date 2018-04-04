using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShineOn.Rtl;
using System;
using System.IO;
using VSS.Productivity3D.Common.Extensions;

namespace VSS.Productivity3D.WebApiTests.Common.Extensions
{
  [TestClass]
  public class CacheKeyExtensionsTests
  {
    [TestMethod]
    public void InvalidateReponseCacheForProject_should_remove_object_immediately_When_invoked()
    {
      IResponseCache responseCache = new MemoryResponseCache(
        new MemoryCache(
          new MemoryCacheOptions
          {
            ExpirationScanFrequency = TimeSpan.FromMinutes(1)
          }));

      IResponseCacheEntry cacheObj = new CachedResponse
      {
        Created = DateTime.Now,
        StatusCode = 204,
        Headers = new HeaderDictionary(),
        Body = GenerateStreamFromString(this.ClassName())
      };

      var cachekey = Guid.NewGuid().ToString();

      responseCache.Set(cachekey.GetProjectCacheKey(), cacheObj, TimeSpan.FromMinutes(10));
      Assert.IsNotNull(responseCache.Get(cachekey.GetProjectCacheKey()));

      responseCache.InvalidateReponseCacheForProject(cachekey);

      Assert.IsNull(responseCache.Get(cachekey));
    }

    private static Stream GenerateStreamFromString(string seedString)
    {
      var stream = new MemoryStream();
      var writer = new StreamWriter(stream);

      writer.Write(seedString);
      writer.Flush();
      stream.Position = 0;

      return stream;
    }
  }
}