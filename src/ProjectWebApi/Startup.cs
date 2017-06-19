using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectWebApi.Filters;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using log4netExtensions;
using KafkaConsumer.Kafka;
using MasterDataProxies;
using MasterDataProxies.Interfaces;
using Repositories;
using VSS.GenericConfiguration;
using ProjectWebApiCommon.ResultsHandling;
using Swashbuckle.Swagger.Model;
using ProjectWebApiCommon.Utilities;
using TCCFileAccess;
using VSS.Raptor.Service.Common.Proxies;

namespace ProjectWebApi
{
  /// <summary>
  /// 
  /// </summary>
  public class Startup
  {
    private readonly string loggerRepoName = "WebApi";
    private bool isDevEnv = false;
    IServiceCollection serviceCollection;

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

    public IConfigurationRoot Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
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
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<ISubscriptionProxy, SubscriptionProxy>();
      services.AddTransient<IGeofenceProxy, GeofenceProxy>();
      services.AddTransient<IRaptorProxy, RaptorProxy>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();

      var tccUrl = (new GenericConfiguration(new LoggerFactory())).GetValueString("TCCBASEURL");
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

        string pathToXml="";
        if (File.Exists(Path.Combine(System.IO.Directory.GetCurrentDirectory(),"ProjectWebApi.xml")))
          pathToXml = System.IO.Directory.GetCurrentDirectory();
        else if (File.Exists(Path.Combine(System.AppContext.BaseDirectory,"ProjectWebApi.xml")))
          pathToXml = System.AppContext.BaseDirectory;
        else
        {
          var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
          pathToXml = Path.GetDirectoryName(pathToExe);
        }
        options.IncludeXmlComments(Path.Combine(pathToXml,"ProjectWebApi.xml"));
        options.IgnoreObsoleteProperties();
        options.DescribeAllEnumsAsStrings();
      });
      serviceCollection = services;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
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

      app.UseApplicationInsightsRequestTelemetry();
      app.UseApplicationInsightsExceptionTelemetry();

      app.UseMvc();

      app.UseSwagger();
      app.UseSwaggerUi();
    }
  }
}
