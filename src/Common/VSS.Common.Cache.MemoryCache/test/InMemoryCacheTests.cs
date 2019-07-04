using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Cache.Models;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.Common.Cache.MemoryCache.UnitTests
{
  public class InMemoryCacheTests
  {
    protected IServiceProvider ServiceProvider;

    private static readonly Random random = new Random();

    public InMemoryCacheTests()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Common.Cache.MemoryCache.UnitTests.log"));

      var serviceCollection = new ServiceCollection()
                          .AddLogging()
                          .AddSingleton(loggerFactory)
                          .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
                          .AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>()
                          .AddMemoryCache()
                          .AddSingleton<IDataCache, InMemoryDataCache>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    public static string RandomString(int length)
    {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    [Fact]
    public void CanCreateCache()
    {
      var memoryCache = ServiceProvider.GetService(typeof(IMemoryCache));
      Assert.NotNull(memoryCache);

      var dataCache = ServiceProvider.GetService(typeof(IDataCache));
      Assert.NotNull(memoryCache);

      Assert.IsType<InMemoryDataCache>(dataCache);
    }

    [Fact]
    public void Test_AddValuesDirectly()
    {
      var key = $"Test_AddValuesDirectly-{Guid.NewGuid()}";
      var data = RandomString(64);

      var cache = ServiceProvider.GetService<IDataCache>();

      var obj = cache.Get<string>(key);
      Assert.Null(obj);

      cache.Set(key, data, null);

      obj = cache.Get<string>(key);
      Assert.NotNull(obj);
      Assert.Equal(data, obj);
    }

    [Fact]
    public void Test_RemoveValuesDirectly()
    {
      var key = $"Test_RemoveValuesDirectly-{Guid.NewGuid()}";
      var data = RandomString(64);

      var cache = ServiceProvider.GetService<IDataCache>();

      cache.Set(key, data, null);

      var result = cache.Get<string>(key);
      Assert.Equal(data, result);

      cache.RemoveByKey(key);

      result = cache.Get<string>(key);
      Assert.Null(result);
    }

    [Fact]
    public void Test_RemoveSingleByTag()
    {
      var key = $"Test_RemoveValuesDirectly-{Guid.NewGuid()}";
      var data = RandomString(64);
      var tag = $"Tag-Test_RemoveValuesDirectly-{Guid.NewGuid()}";

      var cache = ServiceProvider.GetService<IDataCache>();


      cache.Set(key, data, new List<string> { tag });

      var result = cache.Get<string>(key);
      Assert.Equal(data, result);

      cache.RemoveByTag(tag);

      result = cache.Get<string>(key);
      Assert.Null(result);
    }

    [Fact]
    public void Test_RemoveSomeByTag()
    {
      var cache = ServiceProvider.GetService<IDataCache>();

      // Init data, so we have 2 values, sharing a single tag, and having a unique tag
      // We will remove one unique tag

      var key1 = $"Test_RemoveSomeByTag-1-{Guid.NewGuid()}";
      var key2 = $"Test_RemoveSomeByTag-2-{Guid.NewGuid()}";

      var tagUnique1 = Guid.NewGuid().ToString();
      var tagUnique2 = Guid.NewGuid().ToString();
      var tagShared = Guid.NewGuid().ToString();

      // Shouldn't happen, but never know
      Assert.NotEqual(tagUnique1, tagUnique2);
      Assert.NotEqual(tagUnique1, tagShared);
      Assert.Null(cache.Get<string>(key1));
      Assert.Null(cache.Get<string>(key2));

      // Add the data
      cache.Set(key1, RandomString(64), new List<string> { tagUnique1, tagShared });
      cache.Set(key2, RandomString(64), new List<string> { tagUnique2, tagShared });

      cache.RemoveByTag(tagUnique1);

      var result1 = cache.Get<string>(key1);
      var result2 = cache.Get<string>(key2);

      Assert.Null(result1);
      Assert.NotNull(result2);

    }

    [Fact]
    public void Test_GetOrCreate()
    {
      var key = $"Test_GetOrCreate-{Guid.NewGuid()}";
      var tag = $"Tag-Test_GetOrCreate-{Guid.NewGuid()}";
      var data1 = RandomString(64);
      var data2 = RandomString(32);
      var data3 = RandomString(48);

      var dataCalledCount = 0;

      var cache = ServiceProvider.GetService<IDataCache>();

      Assert.Null(cache.Get<string>(key));

      Task<CacheItem<string>> CreateCacheItemFactory(ICacheEntry e, string d)
      {
        dataCalledCount++;
        return Task.FromResult(new CacheItem<string>(d, new List<string> { tag }));
      }

      var result1 = cache.GetOrCreate(key, (e) => CreateCacheItemFactory(e, data1));
      var result2 = cache.GetOrCreate(key, (e) => CreateCacheItemFactory(e, data2));

      // The data is cached, so both calls should return the first data
      Assert.Equal(result1.Result, data1);
      Assert.Equal(result2.Result, data1);
      Assert.Equal(result1.Result, result2.Result);

      Assert.True(dataCalledCount == 1);

      cache.RemoveByKey(key);

      var result3 = cache.GetOrCreate(key, (e) => CreateCacheItemFactory(e, data3));

      Assert.Equal(data3, result3.Result);
      Assert.True(dataCalledCount == 2);
    }


    [Fact]
    public void Test_TagKeySingleCleanup()
    {
      var key = $"Test_TagKeyCleanup-{Guid.NewGuid()}";
      var tag = $"Tag-Test_TagKeyCleanup-{Guid.NewGuid()}";
      var data = RandomString(64);

      var cache = ServiceProvider.GetService<IDataCache>() as InMemoryDataCache;

      Assert.NotNull(cache);

      Assert.True(cache.CacheKeys.Count == 0);
      Assert.True(cache.CacheTags.Count == 0);


      cache.Set(key, data, new List<string> { tag });

      Assert.True(cache.CacheKeys.Count == 1);
      Assert.True(string.Compare(cache.CacheKeys[0], key, StringComparison.OrdinalIgnoreCase) == 0);

      Assert.True(cache.CacheTags.Count == 1);
      Assert.True(string.Compare(cache.CacheTags[0], tag, StringComparison.OrdinalIgnoreCase) == 0);
    }

    [Fact]
    public void Test_TagKeyMultipleCleanup()
    {
      var key1 = $"Test_TagKeyMultipleCleanup-1-{Guid.NewGuid()}";
      var key2 = $"Test_TagKeyMultipleCleanup-2-{Guid.NewGuid()}";

      var tagUnique1 = $"UnqiueTag-1-{Guid.NewGuid()}";
      var tagUnique2 = $"UnqiueTag-1-{Guid.NewGuid()}";
      var tagShared = $"UnqiueTag-shared-{Guid.NewGuid()}";

      var cache = ServiceProvider.GetService<IDataCache>() as InMemoryDataCache;
      Assert.NotNull(cache);
      Assert.True(cache.CacheKeys.Count == 0);
      Assert.True(cache.CacheTags.Count == 0);

      cache.Set(key1, RandomString(64), new List<string> { tagUnique1, tagShared });
      cache.Set(key2, RandomString(64), new List<string> { tagUnique2, tagShared });

      // NOTE: Cache keys can change case to ensure key AAAA == AAaa (useful for guids)
      Assert.True(cache.CacheKeys.Count == 2);
      Assert.True(cache.CacheKeys.Contains(key1, StringComparer.OrdinalIgnoreCase));
      Assert.True(cache.CacheKeys.Contains(key2, StringComparer.OrdinalIgnoreCase));

      Assert.True(cache.CacheTags.Count == 3);
      Assert.True(cache.CacheTags.Contains(tagUnique1, StringComparer.OrdinalIgnoreCase));
      Assert.True(cache.CacheTags.Contains(tagUnique2, StringComparer.OrdinalIgnoreCase));
      Assert.True(cache.CacheTags.Contains(tagShared, StringComparer.OrdinalIgnoreCase));

      cache.RemoveByTag(tagUnique1);

      Assert.True(cache.CacheKeys.Count == 1);
      Assert.False(cache.CacheKeys.Contains(key1, StringComparer.OrdinalIgnoreCase));
      Assert.True(cache.CacheTags.Count == 2);
      Assert.False(cache.CacheTags.Contains(tagUnique1, StringComparer.OrdinalIgnoreCase));

      // Add it back, and remove the shared tag
      cache.Set(key1, RandomString(64), new List<string> { tagUnique1, tagShared });
      Assert.True(cache.CacheKeys.Contains(key1, StringComparer.OrdinalIgnoreCase));
      Assert.True(cache.CacheTags.Contains(tagUnique1, StringComparer.OrdinalIgnoreCase));

      // Remove it all
      cache.RemoveByTag(tagShared);

      Assert.True(cache.CacheKeys.Count == 0);
      Assert.True(cache.CacheTags.Count == 0);
    }

    [Fact]
    public void Test_SetAsyncGetOrCreate()
    {
      var key = $"Test_SetAsyncGetOrCreate-1-{Guid.NewGuid()}";
      var data = RandomString(64);

      var cache = ServiceProvider.GetService<IDataCache>() as InMemoryDataCache;
      Assert.NotNull(cache);

      cache.Set(key, data, null);

      var result = cache.Get<string>(key);

      Assert.Equal(data, result);

      var result2 = cache.GetOrCreate(key, e => Task.FromResult(new CacheItem<string>(data, null)));

      Assert.Equal(data, result2.Result);

    }


    [Fact]
    public void Test_TestTimeout()
    {
      var key = $"Test_SetAsyncGetOrCreate-1-{Guid.NewGuid()}";
      var data = RandomString(64);

      var dataCalledCount = 0;

      var cache = ServiceProvider.GetService<IDataCache>();

      Assert.Null(cache.Get<string>(key));

      Task<CacheItem<string>> CreateCacheItemFactory(ICacheEntry e, string d)
      {
        dataCalledCount++;
        e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(500);
        return Task.FromResult(new CacheItem<string>(d, null));
      }

      cache.GetOrCreate(key, (e) => CreateCacheItemFactory(e, data));
      var result1 = cache.Get<string>(key);
      Assert.Equal(data, result1);

      Thread.Sleep(500);

      var result2 = cache.Get<string>(key);
      Assert.Null(result2);

      cache.GetOrCreate(key, (e) => CreateCacheItemFactory(e, data));

      var result3 = cache.Get<string>(key);
      Assert.Equal(data, result3);


      Assert.Equal(dataCalledCount, 2);
    }

    [Fact]
    public void Test_MultipleThreads()
    {
      var cache = ServiceProvider.GetService<IDataCache>() as InMemoryDataCache;
      Assert.NotNull(cache);

      var dataCount = 2000;

      var threadCount = 10;

      var keys = new List<string>(dataCount);
      var tags = new List<string>(dataCount);
      var signals = new WaitHandle[threadCount];

      for (var i = 0; i < threadCount; i++)
        signals[i] = new ManualResetEvent(false);

      for (var i = 0; i < dataCount; i++)
      {
        keys.Add($"Test_MultipleThreads-{i}-{Guid.NewGuid()}");
        tags.Add($"Test_MultipleThreads-tag-{i}-{Guid.NewGuid()}");
      }

      var sw = Stopwatch.StartNew();
      var failed = false;
      for (var i = 0; i < threadCount; i++)
      {
        new Thread((o) =>
        {

          Thread.Sleep(random.Next(1, 4));
          var idx = (int)o;
          try
          {
            for (var j = idx; j < dataCount + idx; j++)
            {
              var tagCount = random.Next(0, dataCount);
              var tagsProvided = new List<string>();
              for (var t = 0; t < tagCount; t++)
              {
                tagsProvided.Add(tags[t]);
              }

              cache.Set(keys[j % dataCount], RandomString(10), tags);
            }

            for (var j = idx; j < dataCount + idx; j++)
            {
              //cache.RemoveByTag(tags[j % dataCount]);
              cache.RemoveByKey(keys[j % dataCount]);
            }
          }
          catch (Exception e)
          {
            failed = true;
            Console.WriteLine(e);
          }

          ((ManualResetEvent)signals[idx]).Set();

        }).Start(i);
      }

      WaitHandle.WaitAll(signals);
      sw.Stop();

      Console.WriteLine($"Run Time for {threadCount} threads, with {dataCount} items each. {sw.ElapsedMilliseconds}ms");
      Assert.True(cache.CacheKeys.Count == 0, $"Keys Left: {cache.CacheKeys.Count}");
      Assert.True(cache.CacheTags.Count == 0, $"Tags Left: {cache.CacheTags.Count}");

      Assert.False(failed, "One or more threads failed due to an exception");
    }
  }
}
