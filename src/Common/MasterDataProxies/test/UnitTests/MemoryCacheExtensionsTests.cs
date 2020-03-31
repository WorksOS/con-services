using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using Xunit;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace VSS.MasterData.Proxies.UnitTests
{
  public class MemoryCacheExtensionsTests : IClassFixture<MemoryCacheTestsFixture>
  {
    private readonly MemoryCacheTestsFixture _testFixture;
    private readonly IConfigurationStore _configStore;
    private readonly ILogger _logger;

    public MemoryCacheExtensionsTests(MemoryCacheTestsFixture TestFixture)
    {
      _testFixture = TestFixture;

      _configStore = _testFixture.serviceProvider.GetRequiredService<IConfigurationStore>();

      var logger = _testFixture.serviceProvider.GetRequiredService<ILoggerFactory>();
      _logger = logger.CreateLogger<MemoryCacheExtensionsTests>();

    }
    
    [Fact]
    public void CanGetCacheOptions()
    {
      var opts = (new MemoryCacheEntryOptions()).GetCacheOptions("PROJECT_SETTINGS_CACHE_LIFE", _configStore, _logger);
      Assert.NotNull(opts);
      Assert.Equal(new TimeSpan(0,10,0), opts.SlidingExpiration);
    }

    [Fact]
    public void CanGetDefaultCacheOptions()
    {
      var opts = (new MemoryCacheEntryOptions()).GetCacheOptions(string.Empty, _configStore, _logger);
      Assert.NotNull(opts);
      Assert.Equal(new TimeSpan(0, 15, 0), opts.SlidingExpiration);
    }

    [Fact]
    public void CacheDoesNotReturnInvalidEntries()
    {
      new MemoryCacheEntryOptions().GetCacheOptions(string.Empty, _configStore, _logger);
      var cache = _testFixture.serviceProvider.GetService<IMemoryCache>();

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
        catch
        {
          // The test will throw an exception, which represents what happens when a failed cache item is added (such as a service exception)
        }

        // Each execution should increase the counter, as we aren't caching the failed result
        Assert.True(taskExecutionCounter == cnt);
      }
    }

    [Fact]
    public void CacheDoesCacheValidEntries()
    {
      new MemoryCacheEntryOptions().GetCacheOptions(string.Empty, _configStore, _logger);
      var cache = _testFixture.serviceProvider.GetService<IMemoryCache>();

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
        Assert.True(string.Compare(cacheEntry, "Passed", StringComparison.Ordinal) == 0);
        Assert.True(taskExecutionCounter == 1);
      }
    }
  }
}
