using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.FIlters;
using VSS.Productivity3D.Common.Filters;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi
{
  public partial class Startup
  {
    /// <summary>
    /// The name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "3dpm Service API";
    /// <summary>
    /// Log4net repository logger name.
    /// </summary>
    public const string LOGGER_REPO_NAME = "WebApi";

    private IServiceCollection serviceCollection;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      env.ConfigureLog4Net("log4net.xml", LOGGER_REPO_NAME);

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();

      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    /// <summary>
    /// Gets the default configuration object.
    /// </summary>
    public IConfigurationRoot Configuration { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCommon<Startup>(SERVICE_TITLE, "API for 3D compaction and volume data");
   
      services.AddResponseCompression();
      services.AddMemoryCache();
      services.AddCustomResponseCaching();     
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

      ConfigureApplicationServices(services);
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    /// </summary>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();

      serviceCollection.AddSingleton(loggerFactory);
      var serviceProvider = serviceCollection.BuildServiceProvider();

      //Enable CORS before TID so OPTIONS works without authentication
      app.UseCommon(SERVICE_TITLE);

      app.UseTIDAuthentication();

      //Add stats
      if (Configuration["newrelic"] == "true")
      {
        app.UseFilterMiddleware<NewRelicMiddleware>();
      }

      app.UseResponseCompression();

      app.UseResponseCaching();

      ConfigureRaptor(serviceProvider);    
    }

    /// <summary>
    /// Check if the configuration is correct and we are able to connect to Raptor.
    /// </summary>
    private void ConfigureRaptor(ServiceProvider serviceProvider)
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

      log.LogInformation("Testing Raptor configuration with sending config request");

      try
      {
        serviceProvider.GetRequiredService<IASNodeClient>().RequestConfig(out string config);
        log.LogTrace("Received config {0}", config);
        if (config.Contains("Error retrieving Raptor config"))
          throw new Exception(config);
      }
      catch (Exception e)
      {
        log.LogError("Exception loading config: {0} at {1}", e.Message, e.StackTrace);
        log.LogCritical("Can't talk to Raptor for some reason - check configuration");
        Environment.Exit(138);
      }
    }
  }
}
