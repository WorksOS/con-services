using System;
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
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
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
  }
}
