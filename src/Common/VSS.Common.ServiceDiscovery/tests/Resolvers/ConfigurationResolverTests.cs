using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.ServiceDiscovery.Resolvers;
using VSS.Common.ServiceDiscovery.UnitTests.Mocks;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.Common.ServiceDiscovery.UnitTests.Resolvers
{
  public class ConfigurationResolverTests
  {
    private readonly IServiceProvider serviceProvider;
    private readonly MockConfiguration mockConfiguration = new MockConfiguration();

    public ConfigurationResolverTests()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Common.ServiceDiscovery.UnitTests.log"));
      
      var serviceCollection = new ServiceCollection()
                          .AddLogging()
                          .AddSingleton(loggerFactory)
                          .AddSingleton<IConfigurationStore>(mockConfiguration)
                          .AddServiceDiscovery(false);

      serviceProvider = serviceCollection.BuildServiceProvider();
    }

    private ConfigurationServiceResolver GetResolver()
    {
      var serviceResolvers = serviceProvider.GetServices<IServiceResolver>();
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
      mockConfiguration.Values["ConfigurationServicePriority"] = expectedPriority;

      var resolver = GetResolver();
      Assert.NotNull(resolver);

      Assert.Equal(expectedPriority, resolver.Priority);
    }

    [Fact]
    public void TestValidEntry()
    {
      const string expectedServiceEndpoint = "http://localhost:9999";
      const string serviceKey = "test-service";
      mockConfiguration.Values[serviceKey] = expectedServiceEndpoint;


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
