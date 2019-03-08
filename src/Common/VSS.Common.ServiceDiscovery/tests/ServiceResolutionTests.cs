using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery;
using VSS.Common.Abstractions.ServiceDiscovery.Constants;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Cache.MemoryCache;
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
        .AddMemoryCache()
        .AddSingleton<IDataCache, InMemoryDataCache>()
        .AddSingleton<IServiceResolution, InternalServiceResolver>(); // This is the class we will test
    }

    /// <summary>
    /// When using the Service Resolver, a value not defined (or no resolvers to match it) should NOT throw an exception
    /// </summary>
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

    /// <summary>
    /// Test that a service can be resolved
    /// </summary>
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

    /// <summary>
    /// Service Resolvers have priority, this test ensures that the higher priority resolver is used when the service exists in both
    /// But if the higher priority doesn't contain the service, it will use the lower priority
    /// </summary>
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

    /// <summary>
    /// Ensure that each of the API Services, and API Types are defined - as they're used to generate URLs (with the exception of None)
    /// </summary>
    [TestMethod]
    public void TestEnumsNameDefined()
    {
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolution>() as InternalServiceResolver;
      Assert.IsNotNull(resolver);

      foreach (ApiService apiService in Enum.GetValues(typeof(ApiService)))
      {
        if (apiService == ApiService.None)
        {
          Assert.ThrowsException<ArgumentOutOfRangeException>(() => resolver.GetServiceName(apiService));
        }
        else
        {
          var name = resolver.GetServiceName(apiService);
          Assert.IsNotNull(name, $"Could not find a valid service name for the enum value {apiService}");
        }
      }

      foreach (ApiType apiType in Enum.GetValues(typeof(ApiType)))
      {
        var apiComponent = resolver.GetApiRoute(apiType);
        Assert.IsNotNull(apiComponent, $"Could not find a API Component for API Type {apiType}");
      }
    }

    /// <summary>
    /// Test that the resolver can convert a Service, with a version and Type resolves to a single configuration string
    /// These configuration strings will be used to define the endpoints for the resolvers to match with
    /// </summary>
    [TestMethod]
    public void TestServiceNameConfigurationValue()
    {
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolution>() as InternalServiceResolver;
      Assert.IsNotNull(resolver);
      Assert.AreEqual("servicename-public-v1", resolver.GetServiceConfigurationName("ServiceName", ApiType.Public, ApiVersion.V1));
      Assert.AreEqual("servicename-public-v2", resolver.GetServiceConfigurationName("SERVICENAME", ApiType.Public, ApiVersion.V2));
      Assert.AreEqual("servicename-private-v1", resolver.GetServiceConfigurationName("ServiceName", ApiType.Private, ApiVersion.V1));
      Assert.AreEqual("servicename-private-v4", resolver.GetServiceConfigurationName("ServiceName", ApiType.Private, ApiVersion.V4));
    }

    /// <summary>
    /// Test that the API Types map to the URL Components we use in our services
    /// </summary>
    [TestMethod]
    public void TestApiComponent()
    {
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolution>() as InternalServiceResolver;
      Assert.IsNotNull(resolver);

      Assert.AreEqual("api", resolver.GetApiRoute(ApiType.Public));
      Assert.AreEqual("internal", resolver.GetApiRoute(ApiType.Private));
    }

    /// <summary>
    /// Test that a URL for a service is resolved correctly when we build the URL from components, including encoding url parameters
    /// </summary>
    [TestMethod]
    public void TestUrlResolution()
    {
      // Test we can get an endpoint from a single resolver correctly when the service exists, and when it doesn't
      const string serviceName = "test-service-for-url";
      const string serviceEndpoint = "http://localhost:5000/"; // note the final slash, this should be removed
      const string route = "/my-route";
      const ApiVersion apiVersion = ApiVersion.V2;
      const ApiType apiType = ApiType.Private;

      var parameters =  new Dictionary<string, string>
      {
        { "test1", "%#$%^%" },
        { "parameter2", "just-a-normal-value" }
      };

      const string expectedUrl = "http://localhost:5000/internal/v2/my-route?test1=%25%23%24%25%5E%25&parameter2=just-a-normal-value";
      
      var mockServiceResolver = new MockServiceResolver(ServiceResultType.Configuration, 0);
      // Register our service
      mockServiceResolver.ServiceMap[serviceName] = serviceEndpoint;

      serviceCollection.AddSingleton<IServiceResolver>(mockServiceResolver);
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolution>();
      Assert.IsNotNull(resolver);


      var url = resolver.ResolveRemoteServiceEndpoint(serviceName, apiType, apiVersion, route, parameters).Result;

      Assert.AreEqual(expectedUrl, url);
    }

    /// <summary>
    /// Test that when we explicitly define a endpoint value for a service, version and type that it's used and no built (i.e we don't api/v4 to the end)
    /// </summary>
    [TestMethod]
    public void TestExplicitUrlResolution()
    {
      // Test we can get an endpoint from a single resolver correctly when the service exists, and when it doesn't
      const string serviceName = "test-service-for-url";
      const string
        serviceConfigurationKey = "test-service-for-url-public-v2"; // Modified to include the api type and version
      const string
        serviceEndpoint =
          "http://localhost:5023/test-endpoint/rewritten/for/simplicity/v1.99/"; // Something like a TPaaS URL which doesn't match our naming convention
      const string route = "/explicit-route";
      const ApiVersion apiVersion = ApiVersion.V2;
      const ApiType apiType = ApiType.Public;

      const string expectedUrl = "http://localhost:5023/test-endpoint/rewritten/for/simplicity/v1.99/explicit-route";

      var mockServiceResolver = new MockServiceResolver(ServiceResultType.Configuration, 0);
      // Register our service, using the new config key which clear describes the exact endpoint (so no generation occurs in our logic)
      mockServiceResolver.ServiceMap[serviceConfigurationKey] = serviceEndpoint;

      serviceCollection.AddSingleton<IServiceResolver>(mockServiceResolver);
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolution>();
      Assert.IsNotNull(resolver);


      var url = resolver.ResolveRemoteServiceEndpoint(serviceName, apiType, apiVersion, route).Result;

      Assert.AreEqual(expectedUrl, url);
    }

    /// <summary>
    /// Test that we will prioritize an explicit service, version type over the generic built one.
    /// e.g if we had Scheduler base defined, and a specific url for v4 then v4 would return the specific endpoint, but any other version would be built.
    /// </summary>
    [TestMethod]
    public void TestExplicitUrlResolutionPriority()
    {
      // Explicit URLs should be given higher priority than URLS being assembled from scratch

      // Test we can get an endpoint from a single resolver correctly when the service exists, and when it doesn't
      const string serviceName = "test-service";
      const string explictConfigKey = "test-service-public-v2"; // Modified to include the api type and version
      const string serviceRealEndpoint = "http://my-real-host/with/my/real/route/";
      const string serviceBaseUrl = "http://localhost:9000/";
      const string route = "/i/dont/know/where/i/am";

      const ApiType apiType = ApiType.Public;
      const ApiVersion explictApiVersion = ApiVersion.V2;
      const ApiVersion otherApiVersion = ApiVersion.V3; // different to whats defined in the configuration key, this should be build from the base url

      const string expectedV2Url = "http://my-real-host/with/my/real/route/i/dont/know/where/i/am";
      const string expectedOtherUrl = "http://localhost:9000/api/v3/i/dont/know/where/i/am";


      var mockServiceResolver = new MockServiceResolver(ServiceResultType.Configuration, 0);

      mockServiceResolver.ServiceMap[explictConfigKey] = serviceRealEndpoint; // should be ued if looking for v2
      mockServiceResolver.ServiceMap[serviceName] = serviceBaseUrl;

      serviceCollection.AddSingleton<IServiceResolver>(mockServiceResolver);
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolution>();
      Assert.IsNotNull(resolver);
        
      
      var v2Url = resolver.ResolveRemoteServiceEndpoint(serviceName, apiType, explictApiVersion, route).Result;
      var otherUrl = resolver.ResolveRemoteServiceEndpoint(serviceName, apiType, otherApiVersion, route).Result;

      Assert.AreEqual(expectedV2Url, v2Url);
      Assert.AreEqual(expectedOtherUrl, otherUrl);

    }

    [TestMethod]
    public void TestLocalServiceResolution()
    {
      const string serviceName = ServiceNameConstants.PROJECT_SERVICE;
      const string serviceEndpoint = "http://localhost:5000";
      const string serviceRoute = "/route";

      const string expectedUrl = "http://localhost:5000/api/v1/route";

      const ServiceResultType serviceType = ServiceResultType.Configuration;

      var mockServiceResolver = new MockServiceResolver(serviceType, 0);
      // Register our service
      mockServiceResolver.ServiceMap[serviceName] = serviceEndpoint;

      serviceCollection.AddSingleton<IServiceResolver>(mockServiceResolver);
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolution>();

      Assert.IsNotNull(resolver);

      var url = resolver.ResolveLocalServiceEndpoint(ApiService.Project, ApiType.Public, ApiVersion.V1, serviceRoute).Result;

      Assert.AreEqual(expectedUrl, url);
    }

    [TestMethod]
    public void TestVersion1Dot1Resolution()
    {
      // Versions can be v1.1, which we need to support
      const string serviceName = ServiceNameConstants.PROJECT_SERVICE;
      const string serviceEndpoint = "http://localhost:5020";
      const string serviceRoute = "/my-special-version";

      const string expectedUrl = "http://localhost:5020/api/v1.1/my-special-version";

      const ServiceResultType serviceType = ServiceResultType.Configuration;

      var mockServiceResolver = new MockServiceResolver(serviceType, 0);
      // Register our service
      mockServiceResolver.ServiceMap[serviceName] = serviceEndpoint;

      serviceCollection.AddSingleton<IServiceResolver>(mockServiceResolver);
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolution>();

      Assert.IsNotNull(resolver);

      var url = resolver.ResolveLocalServiceEndpoint(ApiService.Project, ApiType.Public, ApiVersion.V1_1, serviceRoute).Result;

      Assert.AreEqual(expectedUrl, url);
    }
  }
}

