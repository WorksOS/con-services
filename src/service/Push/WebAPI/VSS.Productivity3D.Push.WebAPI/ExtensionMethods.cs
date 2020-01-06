using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
      services.AddSingleton<TInterface, TImplementation>();
      return services;
    }

    /// <summary>
    /// Start the connection logic for each hub
    /// Note: The connection logic runs in the background, this will return even if the hub isn't connected straight away
    /// </summary>
    public static async Task StartPushClients(this IServiceProvider services)
    {
      //todoJeannie has this ever worked as GetServices by base interface seems to always return emptyList?

      var clients = services.GetServices<IHubClient>().ToList();
      var tasks = new List<Task>(clients.Count);
      foreach (var hubClient in clients)
      {
        if(hubClient.IsConnecting || hubClient.Connected)
          continue;
        tasks.Add(hubClient.Connect());
      }

      await Task.WhenAll(tasks);
    }
  }
}
