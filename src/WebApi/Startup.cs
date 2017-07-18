using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.Swagger.Model;
using System;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterDataProxies;
using VSS.MasterDataProxies.Interfaces;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Validation;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Notification.Helpers;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.WebApi
{
  /// <summary>
  /// 
  /// </summary>
  public class Startup
  {
    private readonly string loggerRepoName = "WebApi";
    private readonly bool isDevEnv;
    private IServiceCollection serviceCollection;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="env"></param>
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      env.ConfigureLog4Net("log4net.xml", loggerRepoName);

      isDevEnv = env.IsEnvironment("Development");
      if (isDevEnv)
      {
        // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
        builder.AddApplicationInsightsSettings(developerMode: true);
      }

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    /// <summary>
    /// 
    /// </summary>
    public IConfigurationRoot Configuration { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
      //Configure CORS
      services.AddCors(options =>
      {
        options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
                  .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization",
                      "X-VisionLink-CustomerUid", "X-VisionLink-UserUid", "Cache-Control", "X-VisionLink-ClearCache")
                  .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
      });
      // Add framework services.
      services.AddApplicationInsightsTelemetry(Configuration);
      services.AddMemoryCache();
      services.AddCustomResponseCaching();
      services.AddMvc(
          config =>
          {
            config.Filters.Add(new ValidationFilterAttribute());
          });

      //Configure swagger
      services.AddSwaggerGen();

      services.ConfigureSwaggerGen(options =>
      {
        options.SingleApiVersion(new Info
        {
          Version = "v1",
          Title = "Raptor API",
          Description = "API for 3D compaction and volume data",
          TermsOfService = "None"
        });
        string path = isDevEnv ? "bin/Debug/net47/" : string.Empty;
        options.IncludeXmlComments(path + "VSS.Productivity3D.WebApi.xml");
        options.IgnoreObsoleteProperties();
        options.DescribeAllEnumsAsStrings();
      });
      //Swagger documentation can be viewed with http://localhost:5000/swagger/ui/index.html   

      //Configure application services
      services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();
      services.AddScoped<IASNodeClient, ASNodeClient>();
      services.AddScoped<ITagProcessor, TagProcessor>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddSingleton<IProjectListProxy, ProjectListProxy>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddSingleton<IFileListProxy, FileListProxy>();
      services.AddTransient<IFileRepository, FileRepository>();
      services.AddSingleton<IPreferenceProxy, PreferenceProxy>();
      services.AddTransient<ITileGenerator, TileGenerator>();
      services.AddSingleton<IElevationExtentsProxy, ElevationExtentsProxy>();
      services.AddSingleton<IProjectSettingsProxy, ProjectSettingsProxy>();


      serviceCollection = services;
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    /// <param name="loggerFactory"></param>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddSingleton(loggerFactory);
      var serviceProvider = serviceCollection.BuildServiceProvider();
      app.UseFilterMiddleware<ExceptionsTrap>();
      //Enable CORS before TID so OPTIONS works without authentication
      app.UseCors("VSS");
      //Enable TID here
      app.UseFilterMiddleware<TIDAuthentication>();

      //For now don't use application insights as it clogs the log with lots of stuff.
      //app.UseApplicationInsightsRequestTelemetry();
      //app.UseApplicationInsightsExceptionTelemetry();

      app.UseResponseCaching();
      app.UseMvc();

      app.UseSwagger();
      app.UseSwaggerUi();


      //Check if the configuration is correct and we are able to connect to Raptor
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
