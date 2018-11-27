using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;

namespace VSS.MasterData.Proxies.UnitTests
{
  [TestClass]

  public class MemoryCacheExtensionsTests
  {
    private IServiceProvider serviceProvider;

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      serviceCollection.AddTransient<IMemoryCache, MemoryCache>();
      serviceProvider = serviceCollection.BuildServiceProvider();
    }


    [TestMethod]
    public void CanGetCacheOptions()
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var log = logger.CreateLogger<MemoryCacheExtensionsTests>();
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var opts = (new MemoryCacheEntryOptions()).GetCacheOptions("PROJECT_SETTINGS_CACHE_LIFE", configStore, log);
      Assert.IsNotNull(opts);
      Assert.AreEqual(new TimeSpan(0,10,0), opts.SlidingExpiration);
    }

    [TestMethod]
    public void CanGetDefaultCacheOptions()
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var log = logger.CreateLogger<MemoryCacheExtensionsTests>();
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var opts = (new MemoryCacheEntryOptions()).GetCacheOptions(string.Empty, configStore, log);
      Assert.IsNotNull(opts);
      Assert.AreEqual(new TimeSpan(0, 15, 0), opts.SlidingExpiration);
    }

    [TestMethod]
    public void CacheDoesNotReturnInvalidEntries()
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var log = logger.CreateLogger<MemoryCacheExtensionsTests>();
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var opts = (new MemoryCacheEntryOptions()).GetCacheOptions(string.Empty, configStore, log);
      var cache = serviceProvider.GetService<IMemoryCache>();

      var cacheKey = $"test-CacheDoesNotReturnInvalidEntries-{Guid.NewGuid()}";
      var taskExecutionCounter = 0;

      var mockedCacheFactory = new Func<Task<object>>(() =>
      {
        taskExecutionCounter++;
        throw new Exception("Test Exception");
      });


      for (var cnt = 1; cnt <= 10; cnt++)
      {
        try
        {
          _ = cache.GetOrCreate(cacheKey, entry => mockedCacheFactory.Invoke().Result);
        }
        catch (Exception)
        {
          // The test will throw an exception, which represents what happens when a failed cache item is added (such as a service exception)
        }

        // Each execution should increase the counter, as we aren't caching the failed result
        Assert.IsTrue(taskExecutionCounter == cnt);
      }
    }

    [TestMethod]
    public void CacheDoesCacheValidEntries()
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var log = logger.CreateLogger<MemoryCacheExtensionsTests>();
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var opts = (new MemoryCacheEntryOptions()).GetCacheOptions(string.Empty, configStore, log);
      var cache = serviceProvider.GetService<IMemoryCache>();

      var cacheKey = $"test-CacheDoesCacheValidEntries-{Guid.NewGuid()}";
      var taskExecutionCounter = 0;

      var mockedCacheFactory = new Func<Task<string>>(() =>
      {
        taskExecutionCounter++;
        return Task.FromResult("Passed");
      });

      for (var cnt = 1; cnt <= 10; cnt++)
      {
        var cacheEntry = cache.GetOrCreate(cacheKey, entry => mockedCacheFactory.Invoke().Result);
        
        // We should get the same result each time, but we should never execute the method more than once
        Assert.IsTrue(string.Compare(cacheEntry, "Passed", StringComparison.Ordinal) == 0);
        Assert.IsTrue(taskExecutionCounter == 1);
      }
    }
  }
}
