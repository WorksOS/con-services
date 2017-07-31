using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using VSS.Common.Exceptions;
using VSS.Common.Filters;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Filter.WebApi.Filters;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.Filter.WebApi
{
  public class Startup
  {
    private const string loggerRepoName = "WebApi";
    private readonly bool isDevEnv;
    IServiceCollection serviceCollection;

    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();
     
      env.ConfigureLog4Net("log4net.xml", loggerRepoName);
      isDevEnv = env.IsEnvironment("Development");
      if (isDevEnv)
      {
        // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
        builder.AddApplicationInsightsSettings(developerMode: true);
      }

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
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddSingleton<IKafka, RdKafkaDriver>();
      services.AddTransient<ICustomerProxy, CustomerProxy>(); // used in TDI auth for customer/user validation
      services.AddTransient<IProjectListProxy, ProjectListProxy>(); // used for customer/project validation
      services.AddTransient<IRepository<IFilterEvent>, FilterRepository>();
      services.AddMemoryCache();
      
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
