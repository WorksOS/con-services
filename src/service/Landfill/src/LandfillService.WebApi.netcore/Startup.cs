﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Landfill.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Productivity3D.Productivity3D.Proxy;
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

    public override string ServiceName => SERVICE_TITLE;
    public override string ServiceDescription => "A service for landfill request";
    public override string ServiceVersion => "v1";

    /// <summary>
    ///   Initializes a new instance of the <see cref="Startup" /> class.
    /// </summary>
    public Startup(IHostingEnvironment env) : base(env, null, useSerilog: true)
    { }

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
      services.AddTransient<IFileImportProxy, FileImportV4ServiceDiscoveryProxy>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddTransient<IProductivity3dProxy, Productivity3dProxy>();
      services.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>();
      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      app.UseFilterMiddleware<TIDAuthentication>();
      app.UseMvc();
    }
  }
}
