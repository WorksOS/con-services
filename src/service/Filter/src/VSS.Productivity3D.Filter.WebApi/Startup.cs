using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.Productivity3D.AssetMgmt3D.Proxy;
using VSS.Productivity3D.Filter.Common.Filters.Authentication;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Productivity3D.Project.Repository;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.WebAPI;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Filter.WebApi
{
  /// <summary>
  /// VSS.Productivity3D.Filter startup
  /// </summary>
  public class Startup : BaseStartup
  {
    internal const string LoggerRepoName = "WebApi";

    public override string ServiceName => "Filter Service API";

    public override string ServiceDescription => "A service to manage Filter related CRUD requests within the 3DP service architecture.";

    public override string ServiceVersion => "v1";

    
    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public IConfigurationRoot Configuration { get; }

    /// <inheritdoc />
    public Startup(IHostingEnvironment env) : base(env, LoggerRepoName)
    {
      var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    /// <inheritdoc />
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddSingleton<IKafka, RdKafkaDriver>();
      services.AddTransient<ICustomerProxy, CustomerProxy>(); // used in TDI auth for customer/user validation
      services.AddTransient<IRaptorProxy, RaptorProxy>();
      services.AddTransient<IRepository<IFilterEvent>, FilterRepository>();
      services.AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>();
      services.AddTransient<IRepository<IProjectEvent>, ProjectRepository>();
      services.AddTransient<IErrorCodesProvider, FilterErrorCodesProvider>();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddSingleton<IGeofenceProxy, GeofenceProxy>();

      services.AddServiceDiscovery();
      services.AddScoped<IAssetResolverProxy, AssetResolverProxy>();
      services.AddTransient<IProjectProxy, ProjectV4ServiceDiscoveryProxy>();
      services.AddTransient<IFileImportProxy, FileImportV4ServiceDiscoveryProxy>();

      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();
      services.AddSingleton<CacheInvalidationService>();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });
    }

    /// <inheritdoc />
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
    {
      app.UseFilterMiddleware<FilterAuthentication>();
      app.UseMvc();
    }
  }
}
