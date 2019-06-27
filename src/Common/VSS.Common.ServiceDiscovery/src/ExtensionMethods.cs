using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.ServiceDiscovery;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Kubernetes.Factories;
using VSS.Common.Kubernetes.Interfaces;
using VSS.Common.ServiceDiscovery.Resolvers;

namespace VSS.Common.ServiceDiscovery
{
  public static class ExtensionMethods
  {
    public static IServiceCollection AddServiceDiscovery(this IServiceCollection services, bool useKubernetes = true)
    {
      // Already added
      if (services.Any(s => s.ServiceType == typeof(IServiceResolution)))
        return services;

      if (useKubernetes)
      {
        services.AddSingleton<IKubernetesClientFactory, KubernetesClientFactory>();
        services.AddSingleton<IServiceResolver, KubernetesServiceResolver>();
      }

      // Add any custom service resolvers here
      // We could use reflection if adding them manually gets annoying
      services.AddSingleton<IServiceResolver, ConfigurationServiceResolver>();

      // Uses a development file found in %appsettings%, if it's not there it won't resolve any services
      services.AddSingleton<IServiceResolver, DevelopmentServiceResolver>(); 

      services.AddSingleton<IServiceResolution, InternalServiceResolver>();
      return services;
    }

  }
}
