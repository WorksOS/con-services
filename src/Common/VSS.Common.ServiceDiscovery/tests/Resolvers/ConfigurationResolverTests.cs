using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.ServiceDiscovery.Resolvers;
using Xunit;

namespace VSS.Common.ServiceDiscovery.UnitTests.Resolvers
{
  public class ConfigurationResolverTests : IClassFixture<ServiceDiscoveryTestFixture>
  {
    private readonly ServiceDiscoveryTestFixture _fixture;
    public ConfigurationResolverTests(ServiceDiscoveryTestFixture fixture)
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
    public void TestServiceResolverExists()
    {
      var resolver = GetResolver();
      Assert.NotNull(resolver);
      Assert.Equal(ServiceResultType.Configuration, resolver.ServiceType);
    }

    [Fact]
    public void TestServiceResolverIsEnabled()
    {
      var resolver = GetResolver();
      Assert.NotNull(resolver);
      
      Assert.True(resolver.IsEnabled);
    }

    [Fact]
    public void TestPriority()
    {
      const int expectedPriority = 12345;
      _fixture.mockConfiguration.Values["ConfigurationServicePriority"] = expectedPriority;

      var resolver = GetResolver();
      Assert.NotNull(resolver);

      Assert.Equal(expectedPriority, resolver.Priority);
    }

    [Fact]
    public void TestValidEntry()
    {
      const string expectedServiceEndpoint = "http://localhost:9999";
      const string serviceKey = "test-service";
      _fixture.mockConfiguration.Values[serviceKey] = expectedServiceEndpoint;


      var resolver = GetResolver();
      Assert.NotNull(resolver);

      Assert.Equal(expectedServiceEndpoint, resolver.ResolveService(serviceKey).Result);
    }

    [Fact]
    public void TestInvalidEntry()
    {
      var resolver = GetResolver();
      Assert.NotNull(resolver);

      var result = resolver.ResolveService("my-service-that-is-not-real").Result;
      Assert.Null(result);
    }
  }
}
