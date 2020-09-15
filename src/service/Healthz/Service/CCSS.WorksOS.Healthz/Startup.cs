using CCSS.WorksOS.Healthz.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace CCSS.WorksOS.Healthz
{
  /// <summary>
  /// WorksOS Healthz application startup.
  /// </summary>
  public class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "Healthz API";

    /// <inheritdoc />
    public override string ServiceDescription => "A service to report on the status of other internal services";

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
      services.AddMemoryCache();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<IWebRequest, GracefulWebRequest>();
      services.AddSingleton<IHealthCheckService, HealthCheckService>();
      services.AddTransient<IWebRequest, GracefulWebRequest>();

      services.AddServiceDiscovery();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();

      services.AddOpenTracing(builder =>
        builder.ConfigureAspNetCore(options => options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping")));
    }

    /// <inheritdoc />
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
    { }
  }
}
