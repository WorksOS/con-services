using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.ServiceDiscovery;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.ServiceDiscovery.Resolvers;

namespace VSS.Common.ServiceDiscovery
{
  public static class ExtensionMethods
  {
    public static IServiceCollection AddServiceDiscovery(this IServiceCollection services)
    {
      // Add any custom service resolvers here
      // We could use reflection if adding them manually gets annoying
      services.AddSingleton<IServiceResolver, KubernetesServiceResolver>();
      services.AddSingleton<IServiceResolver, ConfigurationServiceResolver>();

      services.AddSingleton<IServiceResolution, InternalServiceResolver>();

      return services;
    }

  }
}