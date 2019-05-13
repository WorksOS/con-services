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
using VSS.Log4Net.Extensions;
using VSS.MasterData.Landfill.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace LandfillService.WebApi.netcore
{
  /// <summary>
  /// </summary>
  public class Startup : BaseStartup
  {
    /// <summary>
    ///   The name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "Landfill Service API";

    /// <summary>
    ///   The logger repository name
    /// </summary>
    public const string LoggerRepoName = "WebApi";

    /// <summary>
    ///   Initializes a new instance of the <see cref="Startup" /> class.
    /// </summary>
    /// <param name="env">The env.</param>
    public Startup(IHostingEnvironment env) : base(env, LoggerRepoName)
    {
    }
    
    public override string ServiceName => SERVICE_TITLE;

    public override string ServiceDescription => "A service for landfill request";

    public override string ServiceVersion => "v1";

    // This method gets called by the runtime. Use this method to add services to the container
    /// <summary>
    ///   Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      services.AddMvc();
      // Add framework services.
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, ProjectErrorCodesProvider>();


      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddTransient<IRaptorProxy, RaptorProxy>();
      services.AddTransient<IFileListProxy, FileListProxy>();
      
      services.AddServiceDiscovery();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    /// <summary>
    ///   Configures the specified application.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="env">The env.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      app.UseFilterMiddleware<TIDAuthentication>();
      app.UseMvc();
    }
  }
}
