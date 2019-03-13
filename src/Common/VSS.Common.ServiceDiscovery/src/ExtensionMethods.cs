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
      if (useKubernetes)
      {
        services.AddSingleton<IKubernetesClientFactory, KubernetesClientFactory>();
        services.AddSingleton<IServiceResolver, KubernetesServiceResolver>();
      }

      // Add any custom service resolvers here
      // We could use reflection if adding them manually gets annoying
      services.AddSingleton<IServiceResolver, ConfigurationServiceResolver>();

      services.AddSingleton<IServiceResolution, InternalServiceResolver>();
      return services;
    }

  }
}