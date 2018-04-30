using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VSS.Productivity3D.WebApiTests.Caching
{
  [TestClass]
  public class CacheBuilderTests
  {
    public IServiceProvider ServiceProvider;

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

      ServiceProvider = serviceCollection.BuildServiceProvider();
    }
  }
}
