using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using VSS.Serilog.Extensions;

namespace VSS.Productivity3D.WebApiTests.Caching
{
  [TestClass]
  public class CacheBuilderTests
  {
    public IServiceProvider ServiceProvider;

    [TestInitialize]
    public void InitTest()
    {
      ServiceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log")))
        .AddTransient<IOptions<MemoryCacheOptions>, MemoryCacheOptions>()
        .BuildServiceProvider();
    }
  }
}
