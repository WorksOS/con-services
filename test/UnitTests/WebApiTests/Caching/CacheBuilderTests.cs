using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        .AddTransient<IOptions<MemoryCacheOptions>, MemoryCacheOptions>();
      
      serviceProvider = serviceCollection.BuildServiceProvider();
    }
  }
}
