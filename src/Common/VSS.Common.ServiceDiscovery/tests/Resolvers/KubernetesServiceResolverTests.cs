using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Kubernetes.Interfaces;
using VSS.Common.ServiceDiscovery.Resolvers;
using VSS.Common.ServiceDiscovery.UnitTests.Mocks;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.Common.ServiceDiscovery.UnitTests.Resolvers
{
  public class KubernetesServiceResolverTests
  {
    private readonly IServiceCollection serviceCollection;
    private readonly MockConfiguration mockConfiguration = new MockConfiguration();

    public KubernetesServiceResolverTests()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Common.ServiceDiscovery.UnitTests.log"));
      
      serviceCollection = new ServiceCollection()
                          .AddLogging()
                          .AddSingleton(loggerFactory)
                          .AddSingleton<IConfigurationStore>(mockConfiguration)
                          .AddSingleton<IServiceResolver, KubernetesServiceResolver>();
    }

    private void CreateMockFactory(Mock<IKubernetes> kubernetesClientMock)
    {
      var mock = new Mock<IKubernetesClientFactory>();
      mock
        .Setup(m => m.CreateClient(It.IsAny<string>()))
        .Returns((kubernetesClientMock.Object, "default"));

      serviceCollection.AddSingleton(mock.Object);
    }

    private static void AddServiceResult(Mock<IKubernetes> mock, V1ServiceList results, string serviceName = null)
    {
      // If we have a specific service name, this will restrict the method to that
      var expectedFilter = $"service-name={serviceName}";
      var result = new HttpOperationResponse<V1ServiceList>()
      {
        Body = results
      };

      // Kubernetes client uses extension methods to find services, this is the acutal method that will finally be called
      mock.Setup(m =>
          m.ListNamespacedServiceWithHttpMessagesAsync(It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<string>(s => string.IsNullOrEmpty(serviceName) || s == expectedFilter),
            It.IsAny<int?>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<bool?>(),
            It.IsAny<bool?>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, List<string>>>(),
            It.IsAny<CancellationToken>()))
        .Returns(Task.FromResult(result));
    }

    private static void VerifyKubernetesServiceSearch(Mock<IKubernetes> client, string serviceName)
    {
      var expectedFilter = $"service-name={serviceName}";

      client.Verify(m => m.ListNamespacedServiceWithHttpMessagesAsync(It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.Is<string>(s => s == expectedFilter),
        It.IsAny<int?>(),
        It.IsAny<string>(),
        It.IsAny<int?>(),
        It.IsAny<bool?>(),
        It.IsAny<bool?>(),
        It.IsAny<string>(),
        It.IsAny<Dictionary<string, List<string>>>(),
        It.IsAny<CancellationToken>()));
    }

    [Fact]
    public void TestEnabled()
    {
      CreateMockFactory(new Mock<IKubernetes>());
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolver>() as KubernetesServiceResolver;

      Assert.NotNull(resolver);
      Assert.True(resolver.IsEnabled);
    }

    [Fact]
    public void TestPriority()
    {
      const int expectedPriority = 99942;
      mockConfiguration.Values["KubernetesServicePriority"] = expectedPriority;

      CreateMockFactory(new Mock<IKubernetes>());
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolver>() as KubernetesServiceResolver;

      Assert.NotNull(resolver);
      Assert.Equal(expectedPriority, resolver.Priority);

      Assert.Equal(ServiceResultType.InternalKubernetes, resolver.ServiceType);
    }

    [Fact]
    public void TestKubernetesConfigValues()
    {
      const string expectedContext = "test-context";

      mockConfiguration.Values["KubernetesContext"] = expectedContext;

      var mockFactory = new Mock<IKubernetesClientFactory>();

      serviceCollection.AddSingleton(mockFactory.Object);

      var resolver =
        serviceCollection.BuildServiceProvider().GetService<IServiceResolver>() as KubernetesServiceResolver;

      // Verify that the factory was called with the config values
      Assert.NotNull(resolver);
      mockFactory.Verify(m =>
        m.CreateClient(It.Is<string>(c => c == expectedContext)));
    }

    [Fact]
    public void TestNoKubernetesResult()
    {
      var mockFactory = new Mock<IKubernetesClientFactory>();
      mockFactory.Setup(m => m.CreateClient( It.IsAny<string>()))
        .Returns((null, null));

      serviceCollection.AddSingleton(mockFactory.Object);

      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolver>() as KubernetesServiceResolver;
      Assert.NotNull(resolver);

      var result = resolver.ResolveService("no-services-casue-we-are-offline").Result;
      Assert.Null(result);
    }

    [Fact]
    public void TestKubernetesNoServicesMatch()
    {
      const string serviceName = "my-service";

      var clusterResult = new V1ServiceList();
      var mockClient = new Mock<IKubernetes>();
      AddServiceResult(mockClient, clusterResult);

      CreateMockFactory(mockClient);
      
      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolver>() as KubernetesServiceResolver;
      Assert.NotNull(resolver);

      var result = resolver.ResolveService(serviceName).Result;
      Assert.Null(result);

      // Verify that we searched for a service with the name expected
      VerifyKubernetesServiceSearch(mockClient, serviceName);
    }

    [Fact]
    public void TestKubernetesReturnsService()
    {
      const string expectedUrl = "http://127.0.0.1:80";
      const string serviceName = "my-service-working";
      const string missingServiceName = "my-service-not-found";

      var clusterResult = new V1ServiceList()
      {
        Items = new List<V1Service>()
        {
          new V1Service()
          {
            Spec = new V1ServiceSpec()
            {
              ClusterIP = "127.0.0.1",
              Ports = new List<V1ServicePort>()
              {
                new V1ServicePort(80, "http")
              }
            }
          }
        }
      };
      var mockClient = new Mock<IKubernetes>();
      // and a service result for other service names
      AddServiceResult(mockClient, new V1ServiceList());
      // Add explicit service result
      AddServiceResult(mockClient, clusterResult, serviceName);
      

      CreateMockFactory(mockClient);

      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolver>() as KubernetesServiceResolver;
      Assert.NotNull(resolver);

      var result = resolver.ResolveService(serviceName).Result;
      Assert.Equal(expectedUrl, result);
      
      // Ensure that a service doesn't exist has no results
      var noResult = resolver.ResolveService(missingServiceName).Result;
      Assert.Null(noResult);

      VerifyKubernetesServiceSearch(mockClient, serviceName);
      VerifyKubernetesServiceSearch(mockClient, missingServiceName);
    }

    [Fact]
    public void TestKubernetesReturnsServiceWithPort80()
    {
      const string expectedUrl = "http://my-host:80";
      const string serviceName = "my-service-non-standard-port";
      // Define 3 services
      // one has the wrong port defined, needs 80
      // one has the right port, but no ClusterIP (we only support them at the moment)
      // the last one is correct
      var clusterResult = new V1ServiceList()
      {
        Items = new List<V1Service>()
        {
          // Bad Service
          new V1Service()
          {
            Metadata = new V1ObjectMeta()
            {
              Name = "Wrong Service",
            },
            Spec = new V1ServiceSpec()
            {
              ClusterIP = "127.0.0.1",
              Ports = new List<V1ServicePort>()
              {
                new V1ServicePort(242, "ssh")
              }
            }
          },
          new V1Service()
          {
            Metadata = new V1ObjectMeta()
            {
              Name = "Right Service but no cluster IP",
            },
            Spec = new V1ServiceSpec()
            {
              LoadBalancerIP = "my-host",
              Ports = new List<V1ServicePort>()
              {
                new V1ServicePort(80, "tcp"),
              }
            }
          },
          new V1Service()
          {
            Metadata = new V1ObjectMeta()
            {
              Name = "Right Service",
            },
            Spec = new V1ServiceSpec()
            {
              ClusterIP = "my-host",
              Ports = new List<V1ServicePort>()
              {
                
                new V1ServicePort(22, "ssh"), // we shouldn't use this port
                new V1ServicePort(80, "tcp"),
              }
            }
          }
        }
      };
      var mockClient = new Mock<IKubernetes>();
      AddServiceResult(mockClient, clusterResult, serviceName);

      CreateMockFactory(mockClient);

      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolver>() as KubernetesServiceResolver;
      Assert.NotNull(resolver);

      var result = resolver.ResolveService(serviceName).Result;
      Assert.Equal(expectedUrl, result);
      VerifyKubernetesServiceSearch(mockClient, serviceName);
    }

    [Fact]
    public void TestKubernetesReturnsNoServiceWithoutPort80()
    {
      const string serviceName = "my-service-but-no-http";

      var clusterResult = new V1ServiceList()
      {
        Items = new List<V1Service>()
        {
          new V1Service()
          {
            Metadata = new V1ObjectMeta()
            {
              Name = "Service without http"
            },
            Spec = new V1ServiceSpec()
            {
              ClusterIP = "127.0.0.1",
              Ports = new List<V1ServicePort>()
              {
                new V1ServicePort(81, "tcp") // this isn't an http port, so it wont be returned
              }
            }
          }
        }
      };
      var mockClient = new Mock<IKubernetes>();
      AddServiceResult(mockClient, clusterResult, serviceName);
      
      CreateMockFactory(mockClient);

      var resolver = serviceCollection.BuildServiceProvider().GetService<IServiceResolver>() as KubernetesServiceResolver;
      Assert.NotNull(resolver);

      var result = resolver.ResolveService(serviceName).Result;
      Assert.Null(result);
    }
  }
}
