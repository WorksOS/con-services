using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Constants;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Abstractions.AssetLocations;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.Hubs;
using VSS.Productivity3D.Push.Hubs.AssetLocations;
using VSS.Productivity3D.Push.WebAPI;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Push
{
  /// <summary>
  /// Application startup class.
  /// </summary>
  public class Startup : BaseStartup
  {

    /// <inheritdoc />
    public override string ServiceName => "Push Service API";

    /// <inheritdoc />
    public override string ServiceDescription => "A service to manage distribution of notifications between services";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";

    /// <inheritdoc />
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
     services.AddMvc();

      // Required for authentication
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddServiceDiscovery();
      services.AddTransient<IProjectProxy, ProjectV4Proxy>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, PushResult>();
      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();

      services.AddSingleton<IAssetStatusState, InMemoryAssetStatusState>();
      // Attempt to resolve the redis cache, and use it for SignalR
      var serviceDiscovery = services.BuildServiceProvider().GetService<IServiceResolution>();
      var redisService = serviceDiscovery.ResolveService(ServiceNameConstants.REDIS_CACHE).Result;

      if (redisService.Type == ServiceResultType.Unknown || string.IsNullOrEmpty(redisService.Endpoint))
      {
        Log.LogWarning("Failed to find REDIS SERVER, SignalR not scalable");
        services.AddSignalR(options => { options.EnableDetailedErrors = true; } );
      }
      else
      {
        Log.LogInformation($"SignalR Using `{redisService.Endpoint}` for Redis Server");
        services.AddSignalR(options => { options.EnableDetailedErrors = true; })
          .AddStackExchangeRedis(redisService.Endpoint, options => { options.Configuration.ChannelPrefix = "push-service"; });
      }
      
    }

    /// <inheritdoc />
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
    {
      app.UseFilterMiddleware<PushAuthentication>();
      
      app.UseSignalR(route =>
      {
        route.MapHub<NotificationHub>(HubRoutes.NOTIFICATIONS);
        route.MapHub<AssetStatusClientHub>(HubRoutes.ASSET_STATUS_CLIENT);
        route.MapHub<AssetStatusServerHub>(HubRoutes.ASSET_STATUS_SERVER);
        route.MapHub<ProjectEventHub>(HubRoutes.PROJECT_EVENTS);
      });
      
      app.UseMvc();
    }

    /// <inheritdoc />
    protected override IEnumerable<(string, CorsPolicy)> GetCors()
    {
      // .NET core 2.2 stopped Any Origin with credentials 
      // But we need this (as we handle do our own validation per hub)
      // https://github.com/aspnet/AspNetCore/issues/4483#issuecomment-451781103
      yield return ("PUSH-CORS", new CorsPolicyBuilder()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed(isOriginAllowed: _ => true)
        .AllowCredentials()
        .Build());

      foreach (var cors in base.GetCors())
        yield return cors;
    }
  }
}
