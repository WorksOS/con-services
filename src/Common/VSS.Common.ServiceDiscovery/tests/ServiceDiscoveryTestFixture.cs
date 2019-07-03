using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.ServiceDiscovery.Resolvers;
using VSS.Common.ServiceDiscovery.UnitTests.Mocks;
using VSS.Serilog.Extensions;

namespace VSS.Common.ServiceDiscovery.UnitTests
{
  public class ServiceDiscoveryTestFixture : IDisposable
  {
    public IServiceProvider serviceProvider;
    public IServiceCollection serviceCollection;
    public MockConfiguration mockConfiguration = new MockConfiguration();

    public ServiceDiscoveryTestFixture()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Common.ServiceDiscovery.UnitTests.log"));
      
      serviceCollection = new ServiceCollection()
                          .AddLogging()
                          .AddSingleton(loggerFactory)
                          .AddSingleton<IConfigurationStore>(mockConfiguration)
                          .AddSingleton<IServiceResolver, KubernetesServiceResolver>();

      serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public void Dispose()
    { }
  }
}
