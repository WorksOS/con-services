using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Filters.Authentication;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Filter.WebAPI.Filters;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Filter.WebApi
{
  /// <summary>
  /// VSS.Productivity3D.Filter startup
  /// </summary>
  public class Startup
  {
    /// <summary>
    /// The name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "Filter Service API";
    /// <summary>
    /// 
    /// </summary>
    public const string LoggerRepoName = "WebApi";
    private IServiceCollection serviceCollection;

    /// <summary>
    /// VSS.Productivity3D.Filter startup
    /// </summary>
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      env.ConfigureLog4Net("log4net.xml", LoggerRepoName);

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    /// <value>
    /// The configuration.
    /// </value>
    public IConfigurationRoot Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCommon<Startup>(SERVICE_TITLE);

      // Add framework services.
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddSingleton<IKafka, RdKafkaDriver>();
      services.AddTransient<ICustomerProxy, CustomerProxy>(); // used in TDI auth for customer/user validation
      services.AddTransient<IProjectListProxy, ProjectListProxy>(); // used for customer/project validation
      services.AddTransient<IFileListProxy, FileListProxy>();
      services.AddTransient<IRaptorProxy, RaptorProxy>();
      services.AddTransient<IRepository<IFilterEvent>, FilterRepository>();
      services.AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>();
      services.AddTransient<IRepository<IProjectEvent>, ProjectRepository>();
      services.AddTransient<IErrorCodesProvider, FilterErrorCodesProvider>();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddMemoryCache();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });

      services.AddJaeger(SERVICE_TITLE);
      services.AddOpenTracing();

      serviceCollection = services;
    }


    // This method gets called by the runtime. Use this method to configure the HTTP Request pipeline
    /// <summary>
    /// Configures the specified application.
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
      app.UseFilterMiddleware<FilterAuthentication>();

      if (Configuration["newrelic"] == "true")
      {
        app.UseMiddleware<NewRelicMiddleware>();
      }

      app.UseMvc();

    }
  }
}
