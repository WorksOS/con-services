using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Utilities;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.FileAccess.WebAPI
{
  /// <summary>
  /// Application startup class.
  /// </summary>
  public class Startup : BaseStartup
  {
    /// <summary>
    /// Log4net repository logger name.
    /// </summary>
    public const string LoggerRepoName = "FileAccessWebApi";

    public Startup(IHostingEnvironment env) : base(env, LoggerRepoName)
    {
    }

    public override string ServiceName => "FileAccess Service API";
    public override string ServiceDescription => "FileAccess Service API";
    public override string ServiceVersion => "v1";

 
    /// <summary>
    /// Called by the runtime to configure the HTTP request pipeline.
  

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
 
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();

      var tccUrl = Configuration.GetValueString("TCCBASEURL");
      var useMock = string.IsNullOrEmpty(tccUrl) || tccUrl == "mock";

      if (useMock)
        services.AddTransient<IFileRepository, MockFileRepository>();
      else
        services.AddTransient<IFileRepository, FileRepository>();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });
    }

    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
    {

    }


  }
}
