using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.FileAccess.WebAPI
{
  /// <summary>
  /// Application startup class.
  /// </summary>
  public class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "FileAccess Service API";

    /// <inheritdoc />
    public override string ServiceDescription => "FileAccess Service API";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";

    /// <summary>
    /// Called by the runtime to configure the HTTP request pipeline.
    /// </summary>
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      services.AddSingleton<IConfigurationStore, GenericConfiguration>()
              .AddTransient<IFileRepository, FileRepository>()
              .AddOpenTracing(builder => builder.ConfigureAspNetCore(options => options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping")));
    }

    /// <inheritdoc />
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
    { }
  }
}
