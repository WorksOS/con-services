using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling;
using VSS.Productivity3D.Common.Filters.Caching;

namespace VSS.Productivity3D.WebApiTests.Caching
{
  [TestClass]
  public class CacheBuilderTests
  {
    public IServiceProvider serviceProvider;

    [TestInitialize]
    public void InitTest()
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      var serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton<IMemoryCacheBuilder<Guid>, MemoryCacheBuilder<Guid>>()
        .AddTransient<IOptions<MemoryCacheOptions>, MemoryCacheOptions>();
      
      serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [TestMethod]
    public void CanResolveMemoryCacheBuilder()
    {
      Assert.IsNotNull(serviceProvider.GetRequiredService<IMemoryCacheBuilder<Guid>>());
    }

    [TestMethod]
    public void CanGetMemoryCacheObject()
    {
      var builder = serviceProvider.GetRequiredService<IMemoryCacheBuilder<Guid>>();
      var cache1 = builder.GetMemoryCache(Guid.NewGuid());
      var cache2 = builder.GetMemoryCache(Guid.NewGuid());
      Assert.IsNotNull(cache1);
      Assert.IsNotNull(cache2);
      Assert.AreNotSame(cache1,cache2);
    }

    [TestMethod]
    public void CanDisposeMemoryCacheObject()
    {
      var builder = serviceProvider.GetRequiredService<IMemoryCacheBuilder<Guid>>();
      var cacheGuid = Guid.NewGuid();
      var cache1 = builder.GetMemoryCache(cacheGuid);
      builder.ClearMemoryCache(cacheGuid);

      Assert.ThrowsException<ObjectDisposedException>(()=>cache1.TryGetValue("123",out var value));
    }


    [TestMethod]
    public void CanSwallowInvalidClearRequest()
    {
      var builder = serviceProvider.GetRequiredService<IMemoryCacheBuilder<Guid>>();
      var cacheGuid = Guid.NewGuid();
      var cache1 = builder.GetMemoryCache(cacheGuid);
      builder.ClearMemoryCache(Guid.NewGuid());
      Assert.IsNotNull(cache1);
    }

  }
}
