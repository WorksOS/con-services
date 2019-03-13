using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.ServiceDiscovery;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Models;
using VSS.Common.Exceptions;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Common.ServiceDiscovery.UnitTests
{
  [TestClass]
  public sealed class ServiceResolutionTests
  {
    private IServiceCollection serviceCollection;

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
      serviceCollection
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>()
        .AddSingleton<IServiceResolution, InternalServiceResolver>(); // This is the class we will test
    }

    [TestMethod]
    public void TestNullResult()
    {
      // Ensure we don't throw an exception if we have no service resolvers, and no service defined
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolution>();
      Assert.IsNotNull(resolver);
      Assert.IsTrue(resolver.Resolvers.Count == 0, "No resolvers specified,");
      var result = resolver.ResolveService("my service, with no providers").Result;
      Assert.IsNull(result, "Result should be null with no resolvers");
    }

    [TestMethod]
    public void TestSingleResult()
    {
      // Test we can get an endpoint from a single resolver correctly when the service exists, and when it doesn't
      const string serviceName = "test-service-please-ignore";
      const string serviceEndpoint = "http://localhost:5000";

      const ServiceResultType serviceType = ServiceResultType.Configuration;

      var mockServiceResolver = new MockServiceResolver(serviceType, 0);
      // Register our service
      mockServiceResolver.ServiceMap[serviceName] = serviceEndpoint;

      serviceCollection.AddSingleton<IServiceResolver>(mockServiceResolver);
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolution>();

      Assert.IsNotNull(resolver);
      Assert.IsTrue(resolver.Resolvers.Count == 1);

      var positiveResult = resolver.ResolveService(serviceName).Result;
      var negativeResult = resolver.ResolveService(serviceName + serviceName).Result;
      
      Assert.IsNull(negativeResult);

      Assert.IsNotNull(positiveResult);
      Assert.AreEqual(serviceType, positiveResult.Type);
      Assert.AreEqual(serviceEndpoint, positiveResult.Endpoint);
    }

    [TestMethod]
    public void TestPriority()
    {
      // Ensure that the higher priority (lower number) service resolver is used before the lower priority
      // But if the higher priority resolver doesn't know about the service, the lower ones will be used if they do
      const string serviceOneName = "test-service-with-priority";
      const string higherPriorityServiceOneEndpoint = "http://localhost:1234";
      const string lowerPriorityServiceOneEndpoint = "http://127.0.0.1:9999";
      const int highPriority = 1;
      const int lowPriority = 2;
      const ServiceResultType higherPriorityServiceType = ServiceResultType.InternalKubernetes;
      const ServiceResultType lowerPriorityServiceType = ServiceResultType.Configuration;

      const string serviceTwoName = "test-service-low-priority-only";
      const string serviceTwoEndpoint = "http://192.168.1.1:64000";

      var highPriorityResolver = new MockServiceResolver(higherPriorityServiceType, highPriority);
      var lowPriorityResolver = new MockServiceResolver(lowerPriorityServiceType, lowPriority);

      // service one will exist in both, but service two just in the lower priority
      highPriorityResolver.ServiceMap[serviceOneName] = higherPriorityServiceOneEndpoint;
      lowPriorityResolver.ServiceMap[serviceOneName] = lowerPriorityServiceOneEndpoint;

      lowPriorityResolver.ServiceMap[serviceTwoName] = serviceTwoEndpoint;

      serviceCollection.AddSingleton<IServiceResolver>(highPriorityResolver);
      serviceCollection.AddSingleton<IServiceResolver>(lowPriorityResolver);
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolution>();

      Assert.IsNotNull(resolver);
      Assert.IsTrue(resolver.Resolvers.Count == 2);

      // Check the first service comes from the higher priority
      var serviceOne = resolver.ResolveService(serviceOneName).Result;
      Assert.IsNotNull(serviceOne);
      Assert.AreEqual(higherPriorityServiceType, serviceOne.Type);
      Assert.AreEqual(higherPriorityServiceOneEndpoint, serviceOne.Endpoint);

      var serviceTwo = resolver.ResolveService(serviceTwoName).Result;
      Assert.IsNotNull(serviceTwo);
      Assert.AreEqual(lowerPriorityServiceType, serviceTwo.Type);
      Assert.AreEqual(serviceTwoEndpoint, serviceTwo.Endpoint);
    }
  }
}
