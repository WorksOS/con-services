using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.ServiceDiscovery;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Clients;

namespace VSS.Productivity3D.Push.WebAPI
{
  public static class ExtensionMethods
  {
    /// <summary>
    /// Add a Push Service Client as a singleton, and a hosted service to allow solution wide receiving of events
    /// </summary>
    /// <typeparam name="TInterface">The Hub Client interface type</typeparam>
    /// <typeparam name="TImplementation">The Hub Client implementation type</typeparam>
    public static IServiceCollection AddPushServiceClient<TInterface, TImplementation>(this IServiceCollection services) 
      where TInterface : class, IHubClient 
      where TImplementation : class, IHubClient, TInterface
    {
      services.AddHostedService<HostedClientService<TInterface>>();
      services.AddSingleton<IServiceResolution, InternalServiceResolver>();
      services.AddTransient<IWebRequest, GracefulWebRequest>();

      var t = ActivatorUtilities.CreateInstance<TImplementation>(services.BuildServiceProvider());
      t.Connect();
      services.AddSingleton<TInterface>(t);
      return services;
    }
  }
}
