using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing.Contrib.NetCore.CoreFx;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Landfill.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Landfill.WebAPI.Middleware;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace LandfillService.WebApi.netcore
{
  /// <summary>
  /// </summary>
  public class Startup
  {
    /// <summary>
    ///   The name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "Landfill Service API";

    /// <summary>
    ///   The logger repository name
    /// </summary>
    public const string LoggerRepoName = "WebApi";

    private static readonly Uri _jaegerUri = new Uri("http://localhost:14268/api/traces");
    private IServiceCollection serviceCollection;

    /// <summary>
    ///   Initializes a new instance of the <see cref="Startup" /> class.
    /// </summary>
    /// <param name="env">The env.</param>
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

      env.ConfigureLog4Net("log4net.xml", LoggerRepoName);

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    /// <summary>
    ///   Gets the configuration.
    /// </summary>
    /// <value>
    ///   The configuration.
    /// </value>
    private IConfigurationRoot Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    /// <summary>
    ///   Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
      // Add framework services.
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, ProjectErrorCodesProvider>();

      services.Configure<HttpHandlerDiagnosticOptions>(options =>
      {
        options.IgnorePatterns.Add(request => request.RequestUri.AbsolutePath.ToString() == "/ping");
      });
      services.AddJaeger(SERVICE_TITLE);

      // Prevent endless loops when OpenTracing is tracking HTTP requests to Jaeger.
      /*services.Configure<HttpHandlerDiagnosticOptions>(options =>
      {
        options.IgnorePatterns.Add(request => _jaegerUri.IsBaseOf(request.RequestUri));
      });*/

      //GlobalTracer.Register();

      //services.AddOpenTracing();
      services.AddMemoryCache();
      services.AddCommon<Startup>(SERVICE_TITLE);

      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddTransient<IRaptorProxy, RaptorProxy>();
      services.AddTransient<IFileListProxy, FileListProxy>();

      serviceCollection = services;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    /// <summary>
    ///   Configures the specified application.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="env">The env.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.BuildServiceProvider();

      //Enable CORS before TID so OPTIONS works without authentication
      app.UseCommon(SERVICE_TITLE);

      if (Configuration["newrelic"] == "true") app.UseMiddleware<NewRelicMiddleware>();
      app.UseFilterMiddleware<TIDAuthentication>();
      app.UseMvc();
    }
  }
}