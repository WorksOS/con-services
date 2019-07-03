using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.ServiceDiscovery.Resolvers;
using Xunit;

namespace VSS.Common.ServiceDiscovery.UnitTests
{
  public class ServiceDiscoveryExtensionMethods : IClassFixture<ServiceDiscoveryTestFixture>
  {
    private readonly ServiceDiscoveryTestFixture _fixture;
    public ServiceDiscoveryExtensionMethods(ServiceDiscoveryTestFixture fixture)
    {
      _fixture = fixture;
    }

    private ConfigurationServiceResolver GetResolver()
    {
      var serviceResolvers = _fixture.serviceProvider.GetServices<IServiceResolver>();
      return serviceResolvers.OfType<ConfigurationServiceResolver>()
                             .Select(serviceResolver => serviceResolver)
                             .FirstOrDefault();
    }
    
    [Fact]
    public void TestKubernetesIsntAdded()
    {
      _fixture.serviceCollection.AddServiceDiscovery(false);

      var resolvers = _fixture.serviceCollection.BuildServiceProvider().GetServices<IServiceResolver>();
      foreach (var serviceResolver in resolvers)
      {
        Assert.IsNotType<KubernetesServiceResolver>(serviceResolver);
      }
    }

    [Fact]
    public void TestKubernetesIsAdded()
    {
      _fixture.serviceCollection.AddServiceDiscovery(true);

      var found = false;
      var resolvers = _fixture.serviceCollection.BuildServiceProvider().GetServices<IServiceResolver>();
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
