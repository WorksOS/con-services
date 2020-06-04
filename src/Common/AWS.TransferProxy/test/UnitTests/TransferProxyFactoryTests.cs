using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.AWS.TransferProxy.UnitTests
{
  public class TransferProxyFactoryTests
  {
    public IServiceProvider serviceProvider;

    public TransferProxyFactoryTests()
    {
      serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.AWS.TransferProxy.UnitTests.log")))
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .BuildServiceProvider();
    }

    [Fact]
    public void Creation()
    {
      var factory = new TransferProxyFactory(serviceProvider.GetRequiredService<IConfigurationStore>(), serviceProvider.GetRequiredService<ILoggerFactory>());
      Assert.False(factory == null);
    }

    [Fact]
    public void Ensure_all_proxy_types_supported_in_factory()
    {
      var factory = new TransferProxyFactory(serviceProvider.GetRequiredService<IConfigurationStore>(), serviceProvider.GetRequiredService<ILoggerFactory>());

      foreach (TransferProxyType type in Enum.GetValues(typeof(TransferProxyType)))
      {
        Assert.False(factory.NewProxy(type) == null);
      }
    }
  }
}
