using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.ServiceDiscovery.Resolvers;
using VSS.Common.ServiceDiscovery.UnitTests.Mocks;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.Common.ServiceDiscovery.UnitTests
{
  public class ServiceDiscoveryExtensionMethods
  {
    private IServiceCollection serviceCollection;
    private readonly MockConfiguration mockConfiguration = new MockConfiguration();

    public ServiceDiscoveryExtensionMethods()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Common.ServiceDiscovery.UnitTests.log"));

      serviceCollection = new ServiceCollection()
                              .AddLogging()
                              .AddSingleton(loggerFactory)
                              .AddSingleton<IConfigurationStore>(mockConfiguration);
    }
    
    [Fact]
    public void TestKubernetesIsntAdded()
    {
      serviceCollection.AddServiceDiscovery(false);
      
      var resolvers = serviceCollection.BuildServiceProvider().GetServices<IServiceResolver>();
      foreach (var serviceResolver in resolvers)
      {
        Assert.IsNotType<KubernetesServiceResolver>(serviceResolver);
      }
    }

    [Fact]
    public void TestKubernetesIsAdded()
    {
      serviceCollection.AddServiceDiscovery(true);

      var found = false;
      var resolvers = serviceCollection.BuildServiceProvider().GetServices<IServiceResolver>();
      foreach (var serviceResolver in resolvers)
      {
        if (serviceResolver is KubernetesServiceResolver)
        {
          found = true;
          break;
        }
      }

      Assert.True(found, $"Count not find {nameof(KubernetesServiceResolver)} in Resolvers");
    }
  }
}
