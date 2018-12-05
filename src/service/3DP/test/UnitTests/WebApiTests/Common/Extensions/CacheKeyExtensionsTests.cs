using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Common.Extensions;

namespace VSS.Productivity3D.WebApiTests.Common.Extensions
{
  [TestClass]
  public class CacheKeyExtensionsTests
  {
    private readonly IResponseCacheEntry cacheObj = new CachedResponse
    {
      Created = DateTime.Now,
      StatusCode = 204,
      Headers = new HeaderDictionary(),
      Body = GenerateStreamFromString("body string")
    };

    [TestMethod]
    public void InvalidateReponseCacheForProject_should_remove_object_immediately_When_invoked()
    {
      IResponseCache responseCache = new MemoryResponseCache(
        new MemoryCache(
          new MemoryCacheOptions
          {
            ExpirationScanFrequency = TimeSpan.FromMinutes(1)
          }));

      var cachekey = Guid.NewGuid().ToString();

      responseCache.Set(null, cachekey.GetProjectCacheKey(), cacheObj, TimeSpan.FromMinutes(10));
      Assert.IsNotNull(responseCache.Get(cachekey.GetProjectCacheKey()));

      responseCache.InvalidateReponseCacheForProject(cachekey);

      Assert.IsNull(responseCache.Get(cachekey));
    }

    /// <summary>
    /// Cache invalidation is by project. This test ensures only those items for the targetd project are removed from cache.
    /// </summary>
    [TestMethod]
    public void InvalidateReponseCacheForProject_should_remove_all_cached_items_for_only_the_specified_project()
    {
      const int cacheEntriesCount = 10000;

      IResponseCache responseCache = new MemoryResponseCache(
        new MemoryCache(
          new MemoryCacheOptions
          {
            ExpirationScanFrequency = TimeSpan.FromMinutes(1)
          }));

      var cacheKeysSet1 = new List<string>();
      for (var i = 0; i < cacheEntriesCount; i++)
      {
        cacheKeysSet1.Add(Guid.NewGuid().ToString());
      }

      // Establish the root cache key for the test project.
      var projectUid1 = Guid.NewGuid();
      var projectCacheKey1 = projectUid1.ToString().GetProjectCacheKey();
      responseCache.Set(null, projectCacheKey1, cacheObj, TimeSpan.FromMinutes(10));

      var projectUid2 = Guid.NewGuid();
      var projectCacheKey2 = projectUid2.ToString().GetProjectCacheKey();
      responseCache.Set(null, projectCacheKey2, cacheObj, TimeSpan.FromMinutes(10));

      // Populate the internal cache keys collection for the test projects.
      PopulateResponseCache(responseCache, projectCacheKey1, cacheKeysSet1);
      PopulateResponseCache(responseCache, projectCacheKey2, cacheKeysSet1);

      // After cache hydration has completed test it's structured correctly.
      var cacheKeysField = ((MemoryResponseCache)responseCache).GetType().GetField("_cacheKeys", BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic);
      var cacheKeysValue = (ConcurrentDictionary<string, ConcurrentBag<string>>)cacheKeysField?.GetValue(responseCache);

      Assert.AreEqual(2, cacheKeysValue?.Count); // There are 2 projects in use...
      Assert.AreEqual(cacheKeysSet1.Count, cacheKeysValue?.Values.First().Count); // ...with N number of cached responses

      responseCache.InvalidateReponseCacheForProject(projectUid1);

      Assert.AreEqual(1, cacheKeysValue?.Count); // The root key should have been removed so we reduce from 2 projects to 1.
      Assert.AreEqual(projectCacheKey2, cacheKeysValue?.First().Key); // ...and the reaminder should be the project we didn't invalidate.
    }

    /// <summary>
    /// This tests that responses for given project, cached while old response caches are being invalidate, are retained and left in cache.
    /// </summary>
    [TestMethod]
    public void InvalidateReponseCacheForProject_should_not_remove_items_added_during_execution_of_the_cache_clean()
    {
      const int cacheEntriesCount = 30000;

      IResponseCache responseCache = new MemoryResponseCache(
        new MemoryCache(
          new MemoryCacheOptions
          {
            ExpirationScanFrequency = TimeSpan.FromMinutes(1)
          }));

      var cacheKeysSet1 = new List<string>();
      for (var i = 0; i < cacheEntriesCount; i++)
      {
        cacheKeysSet1.Add(Guid.NewGuid().ToString());
      }

      var cacheKeysSet2 = new List<string>();
      for (var i = 0; i < cacheEntriesCount / 2; i++)
      {
        cacheKeysSet2.Add(Guid.NewGuid().ToString());
      }

      // Establish the root cache key for the test project.
      var projectUid1 = Guid.NewGuid();
      var projectCacheKey1 = projectUid1.ToString().GetProjectCacheKey();
      responseCache.Set(null, projectCacheKey1, cacheObj, TimeSpan.FromMinutes(10));

      // Populate the internal cache keys collection for the test projects.
      PopulateResponseCache(responseCache, projectCacheKey1, cacheKeysSet1);

      responseCache.InvalidateReponseCacheForProject(projectUid1);
      // Add a second set of test response objects to the cache, while the first set are being invalidated.
      PopulateResponseCache(responseCache, projectCacheKey1, cacheKeysSet2);

      // After cache hydration has completed test it's structured correctly.
      var cacheKeysField = ((MemoryResponseCache)responseCache).GetType().GetField("_cacheKeys", BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic);
      var cacheKeysValue = (ConcurrentDictionary<string, ConcurrentBag<string>>)cacheKeysField?.GetValue(responseCache);

      Assert.AreEqual(1, cacheKeysValue?.Count); // There should be only one root key in this test.
      Assert.AreEqual(cacheKeysSet2.Count, cacheKeysValue?.Values.First().Count); // ...and after invalidating the cache we should have the 500 items that were added during that process.
    }

    [Ignore("Ignored while Aaron determines why this is failing on CI machine.")]
    [TestMethod]
    public void InvalidateReponseCacheForProject_should_handle_multithreaded_interaction()
    {
      IResponseCache responseCache = new MemoryResponseCache(
        new MemoryCache(
          new MemoryCacheOptions
          {
            ExpirationScanFrequency = TimeSpan.FromMinutes(1)
          }));

      var cacheKeysSet1 = new List<string>();
      for (var i = 0; i < 200000; i++)
      {
        cacheKeysSet1.Add(Guid.NewGuid().ToString());
      }
      cacheKeysSet1.Sort((x, y) => x.CompareTo(y));


      var cacheKeysSet2 = new List<string>();
      for (var i = 0; i < 50000; i++)
      {
        cacheKeysSet2.Add(Guid.NewGuid().ToString());
      }
      cacheKeysSet2.Sort((x, y) => x.CompareTo(y));


      // Establish the root cache keys for the test projects.
      var projectUid1 = Guid.NewGuid();
      var projectCacheKey1 = projectUid1.ToString().GetProjectCacheKey();
      responseCache.Set(null, projectCacheKey1, cacheObj, TimeSpan.FromMinutes(10));

      var projectUid2 = Guid.NewGuid();
      var projectCacheKey2 = projectUid2.ToString().GetProjectCacheKey();
      responseCache.Set(null, projectCacheKey2, cacheObj, TimeSpan.FromMinutes(10));

      var cachePopulated1 = false;
      var cachePopulated2 = false;

      // Populate the internal cache keys collection for the test projects.
#pragma warning disable 4014
      Task.Run(() => PopulateResponseCache(responseCache, projectCacheKey1, cacheKeysSet1))
          .ContinueWith(task =>
          {
            // After cache hydration has completed test it's structured correctly.
            var cacheKeysFieldAfter1 = ((MemoryResponseCache)responseCache).GetType().GetField("_cacheKeys", BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic);
            var cacheKeysValueAfter1 = (ConcurrentDictionary<string, ConcurrentBag<string>>)cacheKeysFieldAfter1?.GetValue(responseCache);
            cacheKeysValueAfter1.Should().NotBeNull();
            cacheKeysValueAfter1.Count.Should().BeGreaterOrEqualTo(1);
            cacheKeysValueAfter1.TryGetValue(projectCacheKey1, out var project1CachedResponses).Should().BeTrue();
            var sortedValues1 = project1CachedResponses.ToList();
            sortedValues1.Sort((x, y) => x.CompareTo(y));
            sortedValues1.SequenceEqual(cacheKeysSet1).Should().BeTrue();
   
            // Set the flag only when the response cache has finished populating.
            cachePopulated1 = true;
          });

      Task.Run(() => PopulateResponseCache(responseCache, projectCacheKey2, cacheKeysSet2))
        .ContinueWith(task =>
      {
        //Verify the cache has the values we expect in it
        var cacheKeysFieldAfter2 = ((MemoryResponseCache)responseCache).GetType().GetField("_cacheKeys", BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic);
        var cacheKeysValueAfter2 = (ConcurrentDictionary<string, ConcurrentBag<string>>)cacheKeysFieldAfter2?.GetValue(responseCache);
        cacheKeysValueAfter2.Should().NotBeNull();
        cacheKeysValueAfter2.Should().ContainKey(projectCacheKey2);
        cacheKeysValueAfter2.TryGetValue(projectCacheKey2, out var project2CachedResponses).Should().BeTrue();
        var sortedValues2 = project2CachedResponses.ToList();
        sortedValues2.Sort((x, y) => x.CompareTo(y));
        sortedValues2.SequenceEqual(cacheKeysSet2).Should().BeTrue();
        // Set the flag only when the response cache has finished populating.
        cachePopulated2 = true;
      });
#pragma warning restore 4014

      while (!(cachePopulated1 && cachePopulated2)) { }

      // After cache hydration has completed test it's structured correctly.
      var cacheKeysField = ((MemoryResponseCache)responseCache).GetType().GetField("_cacheKeys", BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic);
      var cacheKeysValue = (ConcurrentDictionary<string, ConcurrentBag<string>>)cacheKeysField?.GetValue(responseCache);

      cacheKeysValue.Should().NotBeNull();
      cacheKeysValue.Should().ContainKeys(projectCacheKey1, projectCacheKey2);

      // Invalidation check
      responseCache.InvalidateReponseCacheForProject(projectUid1);
      cacheKeysValue = (ConcurrentDictionary<string, ConcurrentBag<string>>)cacheKeysField?.GetValue(responseCache);
      cacheKeysValue.Should().ContainKey(projectCacheKey2);
      cacheKeysValue.Should().NotContainKey(projectCacheKey1);
      cacheKeysValue.TryGetValue(projectCacheKey2, out var project2CachedResponsesAfterInvalidate).Should().BeTrue();

      var sortedproject2CachedResponsesAfterInvalidate = project2CachedResponsesAfterInvalidate.ToList();
      sortedproject2CachedResponsesAfterInvalidate.Sort((x, y) => x.CompareTo(y));
      sortedproject2CachedResponsesAfterInvalidate.SequenceEqual(cacheKeysSet2).Should().BeTrue();
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

    private void PopulateResponseCache(IResponseCache responseCache, string projectUidCacheKey, IEnumerable<string> cacheEntries)
    {
      Parallel.ForEach(cacheEntries, currentKey =>
      {
        responseCache.SetAsync(projectUidCacheKey, currentKey, cacheObj, TimeSpan.FromMinutes(10));
      });
    }
  }
}
