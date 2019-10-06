using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Proxy;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.WebAPI;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Now3D
{
  /// <summary>
  /// VSS.Productivity3D.Now3D application startup.
  /// </summary>
  public class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "3D Now Composite API";

    /// <inheritdoc />
    public override string ServiceDescription => "A service to manage requests to multiple services for each of use for external customers";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";
    
    /// <summary>
    /// Configures services and the application request pipeline.
    /// </summary>
    public Startup()
    { }

    /// <inheritdoc />
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      services.AddMvc();

      // Required for authentication
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddTransient<IProductivity3dV2ProxyCompaction, Productivity3dV2ProxyCompaction>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<IWebRequest, GracefulWebRequest>();

      services.AddServiceDiscovery();
      services.AddTransient<IProjectProxy, ProjectV4Proxy>();
      services.AddTransient<IFilterServiceProxy, FilterV1Proxy>();
      services.AddTransient<IFileImportProxy, FileImportV4Proxy>();

      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, Now3DExecutionStates>();

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
      app.UseFilterMiddleware<Now3DAuthentication>();
      app.UseMvc();
    }
  }
}
