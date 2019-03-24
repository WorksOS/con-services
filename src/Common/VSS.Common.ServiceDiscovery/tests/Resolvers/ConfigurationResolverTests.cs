using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.ServiceDiscovery.Resolvers;
using VSS.Common.ServiceDiscovery.UnitTests.Mocks;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;

namespace VSS.Common.ServiceDiscovery.UnitTests.Resolvers
{
  [TestClass]
  public class ConfigurationResolverTests
  {
    private IServiceCollection serviceCollection;

    private MockConfiguration mockConfiguration = new MockConfiguration();

    [TestInitialize]
    public void InitTest()
    {
      serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug().AddConsole();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore>(mockConfiguration); // need to configure the values
      serviceCollection.AddServiceDiscovery(false); // this addes the class we tests
    }

    private ConfigurationServiceResolver GetResolver()
    {
      var serviceResolvers = serviceCollection.BuildServiceProvider().GetServices<IServiceResolver>();
      return serviceResolvers.OfType<ConfigurationServiceResolver>()
        .Select(serviceResolver => serviceResolver)
        .FirstOrDefault();
    }

    [TestMethod]
    public void TestServiceResolverExists()
    {
      var resolver = GetResolver();
      Assert.IsNotNull(resolver);
      Assert.AreEqual(ServiceResultType.Configuration, resolver.ServiceType);
    }

    [TestMethod]
    public void TestServiceResolverIsEnabled()
    {
      var resolver = GetResolver();
      Assert.IsNotNull(resolver);
      
      Assert.IsTrue(resolver.IsEnabled);
    }

    [TestMethod]
    public void TestPriority()
    {
      const int expectedPriority = 12345;
      mockConfiguration.Values["ConfigurationServicePriority"] = expectedPriority;

      var resolver = GetResolver();
      Assert.IsNotNull(resolver);

      Assert.AreEqual(expectedPriority, resolver.Priority);
    }

    [TestMethod]
    public void TestValidEntry()
    {
      const string expectedServiceEndpoint = "http://localhost:9999";
      const string serviceKey = "test-service";
      mockConfiguration.Values[serviceKey] = expectedServiceEndpoint;


      var resolver = GetResolver();
      Assert.IsNotNull(resolver);

      Assert.AreEqual(expectedServiceEndpoint, resolver.ResolveService(serviceKey).Result);
    }

    [TestMethod]
    public void TestInvalidEntry()
    {
      var resolver = GetResolver();
      Assert.IsNotNull(resolver);

      var result = resolver.ResolveService("my-service-that-is-not-real").Result;
      Assert.IsNull(result);
    }
  }
}