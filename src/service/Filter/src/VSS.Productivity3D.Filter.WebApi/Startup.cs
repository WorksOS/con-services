using CCSS.CWS.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Filters.Authentication;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Productivity3D.Project.Repository;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.WebAPI;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Filter.WebApi
{
  /// <summary>
  /// VSS.Productivity3D.Filter startup
  /// </summary>
  public class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "Filter Service API";

    /// <inheritdoc />
    public override string ServiceDescription => "A service to manage Filter related CRUD requests within the 3DP service architecture.";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public new IConfigurationRoot Configuration { get; }

    /// <inheritdoc />
    public Startup(IWebHostEnvironment env)
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

      // the code in these repos is used by FilterSvc to refer to tables contained in the local Filter database (not the ProjectSvc one).
      //    e.g. when a filter with boundary is created, ths service calls projectRepo.Store(associateProjectGeofence),
      //         which stores it in the local VSS.Filter.ProejctGeofence table.
      //         then it will query the local db e.g. projectRepo.GetAssociatedGeofences refers to the VSS.Filter.ProejctGeofence table which it previouse
      services.AddTransient<IRepository<IFilterEvent>, FilterRepository>();
      services.AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>();
      services.AddTransient<IRepository<IProjectEvent>, ProjectRepository>();

      services.AddTransient<IErrorCodesProvider, FilterErrorCodesProvider>();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

      // we don't have these VSS services available to ccss yet
      //services.AddSingleton<IUnifiedProductivityProxy, UnifiedProductivityProxy>();
      //services.AddSingleton<IGeofenceProxy, GeofenceProxy>();

      services.AddTransient<IProductivity3dV2ProxyNotification, Productivity3dV2ProxyNotification>();
      services.AddTransient<IProductivity3dV2ProxyCompaction, Productivity3dV2ProxyCompaction>();
      services.AddTransient<IProjectProxy, ProjectV6Proxy>();
      services.AddTransient<IFileImportProxy, FileImportV4Proxy>();
      services.AddTransient<ICwsAccountClient, CwsAccountClient>();

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
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
    {
      app.UseFilterMiddleware<FilterAuthentication>();
      app.UseMvc();
    }
  }
}
