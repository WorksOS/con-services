using System.IO;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.FIlters;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.WebApi.Common;

#if NET_4_7
using VSS.Productivity3D.TagFileAuth.WebAPI.Filters;
#endif

namespace VSS.Productivity3D.TagFileAuth.WebAPI
{
  /// <summary>
  /// Configures services and request pipelines.
  /// </summary>
  public class Startup
  {
    /// <summary>
    /// The name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "3dpm Service API";

    /// <summary>
    /// Log4net repository logger name.
    /// </summary>
    public const string LOGGER_REPO_NAME = "WebApi";

    /// <summary>
    /// Gets the root configuration object.
    /// </summary>
    public IConfigurationRoot Configuration { get; }

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
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCommon<Startup>(SERVICE_TITLE, "API for 3D File Access");
      services.AddLogging();

      //Configure CORS
      services.AddCors(options =>
      {
        options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
          .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization",
            "X-VisionLink-CustomerUid", "X-VisionLink-UserUid")
          .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
      });

      // Add framework services.
      services
        .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
        .AddTransient<IRepository<IAssetEvent>, AssetRepository>()
        .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
        .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()
        .AddSingleton<IKafka, RdKafkaDriver>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddMvc(
        config =>
        {
          // for jsonProperty validation
          config.Filters.Add(new ValidationFilterAttribute());
        });

      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new Info { Title = "Tagfile authorization service API", Description = "API for Tagfile authorization service", Version = "v1" });
      });

      services.ConfigureSwaggerGen(options =>
      {
        string pathToXml;

        var moduleName = typeof(Startup).GetTypeInfo().Assembly.ManifestModule.Name;
        var assemblyName = moduleName.Substring(0, moduleName.LastIndexOf('.'));

        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), assemblyName + ".xml")))
          pathToXml = Directory.GetCurrentDirectory();
        else if (File.Exists(Path.Combine(System.AppContext.BaseDirectory, assemblyName + ".xml")))
          pathToXml = System.AppContext.BaseDirectory;
        else
        {
          var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
          pathToXml = Path.GetDirectoryName(pathToExe);
        }
        options.IncludeXmlComments(Path.Combine(pathToXml, assemblyName + ".xml"));
        options.IgnoreObsoleteProperties();
        options.DescribeAllEnumsAsStrings();
      });
      serviceCollection = services;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
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

      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(LOGGER_REPO_NAME);

      //Enable CORS before TID so OPTIONS works without authentication
      app.UseCommon("VSS");

#if NET_4_7
      if (Configuration["newrelic"] == "true")
      {
        app.UseMiddleware<NewRelicMiddleware>();
      }
#endif

      app.UseMvc();
    }
  }
}
