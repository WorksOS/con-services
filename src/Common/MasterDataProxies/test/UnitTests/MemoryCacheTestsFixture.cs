using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Serilog.Extensions;

namespace VSS.MasterData.Proxies.UnitTests
{
  public class MemoryCacheTestsFixture : IDisposable
  {
    public IServiceProvider serviceProvider;

    public MemoryCacheTestsFixture()
    {
      serviceProvider = new ServiceCollection()
                        .AddLogging()
                        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.MasterData.Proxies.UnitTests.log")))
                        .AddSingleton<IConfigurationStore, GenericConfiguration>()
                        .AddTransient<IMemoryCache, MemoryCache>()
                        .BuildServiceProvider();
    }

    public void Dispose()
    { }
  }
}
