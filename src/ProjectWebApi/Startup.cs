using KafkaConsumer.Kafka;
using log4netExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using VSS.GenericConfiguration;
using VSS.Productivity3D.MasterDataProxies;
using VSS.Productivity3D.MasterDataProxies.Interfaces;
using VSS.Productivity3D.ProjectWebApi.Filters;
using VSS.Productivity3D.ProjectWebApi.Internal;
using VSS.Productivity3D.ProjectWebApiCommon.ResultsHandling;
using VSS.Productivity3D.ProjectWebApiCommon.Utilities;
using VSS.Productivity3D.Repo;
using VSS.Productivity3D.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.ProjectWebApi
{
  /// <summary>
  /// 
  /// </summary>
  public class Startup
  {
    private const string loggerRepoName = "WebApi";
    private readonly bool isDevEnv;
    IServiceCollection serviceCollection;

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="env">The env.</param>
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

      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    /// <value>
    /// The configuration.
    /// </value>
    private IConfigurationRoot Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddLogging();

      //Configure CORS
      services.AddCors(options =>
      {
        options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
          .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization",
            "X-VisionLink-CustomerUID", "X-VisionLink-UserUid", "X-Jwt-Assertion", "X-VisionLink-ClearCache")
          .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
      });

      // Add framework services.
      services.AddApplicationInsightsTelemetry(Configuration);
      services.AddTransient<IRepository<IProjectEvent>, ProjectRepository>();
      services.AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>();
      services.AddSingleton<IKafka, RdKafkaDriver>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration.GenericConfiguration>();
      services.AddTransient<ISubscriptionProxy, SubscriptionProxy>();
      services.AddTransient<IGeofenceProxy, GeofenceProxy>();
      services.AddTransient<IRaptorProxy, RaptorProxy>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();

      var tccUrl = (new GenericConfiguration.GenericConfiguration(new LoggerFactory())).GetValueString("TCCBASEURL");
      var useMock = string.IsNullOrEmpty(tccUrl) || tccUrl == "mock";
      if (useMock)
        services.AddTransient<IFileRepository, MockFileRepository>();
      else
        services.AddTransient<IFileRepository, FileRepository>();

      services.AddMvc(
        config =>
        {
          config.Filters.Add(new ValidationFilterAttribute());
        }
        );
      //Configure swagger
      services.AddSwaggerGen();

      services.ConfigureSwaggerGen(options =>
      {
        options.SingleApiVersion(new Info
        {
          Version = "v1",
          Title = "Project Master Data API",
          Description = "API for project data",
          TermsOfService = "None"
        });

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
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      //new DependencyInjectionProvider(serviceCollection.BuildServiceProvider());
      serviceCollection.BuildServiceProvider();

      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      app.UseExceptionTrap();
      //Enable CORS before TID so OPTIONS works without authentication
      app.UseCors("VSS");
      //Enable TID here
      app.UseTIDAuthentication();

      /*app.UseApplicationInsightsRequestTelemetry();
      app.UseApplicationInsightsExceptionTelemetry();*/

      app.UseMvc();

      app.UseSwagger();
      app.UseSwaggerUi();
    }
  }
}