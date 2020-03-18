using CCSS.CWS.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.TagFileAuth.WebAPI
{
  /// <summary>
  /// Application startup class.
  /// </summary>
  public class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "3dpm Tag File Auth API";

    /// <inheritdoc />
    public override string ServiceDescription => "The service is used for TagFile authorization";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";

    /// <inheritdoc />
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      Log.LogDebug("Loading application service descriptors");

      // Add framework services.
      services
        .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
        .AddTransient<IAccountClient, AccountClient>()
        .AddTransient<IProjectProxy, ProjectV6Proxy>()
        .AddTransient<IDeviceProxy, DeviceV5Proxy>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>();

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
    { }
  }
}
