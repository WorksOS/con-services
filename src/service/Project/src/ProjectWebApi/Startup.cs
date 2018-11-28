using System;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.MasterData.Project.WebAPI.Middleware;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;

namespace VSS.MasterData.Project.WebAPI
{
  /// <summary>
  /// 
  /// </summary>
  public class Startup
  {
    /// <summary>
    /// The name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "Project Service API";

    /// <summary>
    /// The logger repository name
    /// </summary>
    public const string LoggerRepoName = "WebApi";

    private IServiceCollection serviceCollection;
    public static IServiceProvider serviceProvider;


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
    private IConfigurationRoot Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCommon<Startup>(SERVICE_TITLE);
      //TODO: Check if SetPreflightMaxAge(TimeSpan.FromSeconds(2520) in WebApi pkg matters

      // Add framework services.
      services.AddSingleton<IKafka, RdKafkaDriver>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<ISubscriptionProxy, SubscriptionProxy>();
      services.AddTransient<IRaptorProxy, RaptorProxy>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddScoped<IRequestFactory, RequestFactory>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IProjectRepository, ProjectRepository>();
      services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
      services.AddScoped<ICustomerRepository, CustomerRepository>();
      services.AddTransient<IProjectSettingsRequestHelper, ProjectSettingsRequestHelper>();
      services.AddScoped<IErrorCodesProvider, ProjectErrorCodesProvider>();
      services.AddTransient<ISchedulerProxy, SchedulerProxy>();
      services.AddTransient<IFileRepository, FileRepository>();
      services.AddSingleton<Func<TransferProxyType, ITransferProxy>>(transfer => TransferProxyMethod);
      services.AddTransient<IFilterServiceProxy, FilterServiceProxy>();
      services.AddTransient<ITRexImportFileProxy, TRexImportFileProxy>();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
          options.Hosting.IgnorePatterns.Add(request => request.Request.GetUri().ToString().Contains("newrelic.com"));
        });
      });

      services.AddJaeger(SERVICE_TITLE);

      services.AddOpenTracing();
      services.AddMemoryCache();

      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      serviceProvider = services.BuildServiceProvider();
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      //Note: The injection of CAP subscriber service needed before 'services.AddCap()'
      services.AddTransient<ISubscriberService, SubscriberService>();
      //Disable CAP for now #76666
      /*
      services.AddCap(x =>
      {
        x.UseMySql(y =>
        {
          y.ConnectionString = configStore.GetConnectionString("VSPDB", "MYSQL_CAP_DATABASE_NAME");
          y.TableNamePrefix = configStore.GetValueString("MYSQL_CAP_TABLE_PREFIX");
        });
        x.UseKafka(z =>
        {
          z.Servers = $"{configStore.GetValueString("KAFKA_URI")}:{configStore.GetValueString("KAFKA_PORT")}";
          z.MainConfig.TryAdd("group.id", configStore.GetValueString("KAFKA_CAP_GROUP_NAME"));
          //z.MainConfig.TryAdd("auto.offset.reset", "earliest");//Uncomment for debugging locally
        });
        x.UseDashboard(); //View dashboard at http://localhost:5000/cap
      });
      */

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

      //Enable CORS before TID so OPTIONS works without authentication
      app.UseCommon(SERVICE_TITLE);

      if (Configuration["newrelic"] == "true")
      {
        app.UseMiddleware<NewRelicMiddleware>();
      }

      app.UseFilterMiddleware<ProjectAuthentication>();
      app.UseStaticFiles();
      // Because we use Flow Files, and Background tasks we sometimes need to reread the body of the request
      // Without this, the Request Body Stream cannot set it's read position to 0.
      // See https://stackoverflow.com/questions/31389781/read-request-body-twice
      app.Use(next => context =>
      {
        context.Request.EnableRewind();
        return next(context);
      });
      app.UseMvc();
    }

    private static ITransferProxy TransferProxyMethod(TransferProxyType type)
    {
      switch (type)
      {
        case TransferProxyType.DesignImport:
          return new TransferProxy(serviceProvider.GetRequiredService<IConfigurationStore>(),
            "AWS_DESIGNIMPORT_BUCKET_NAME");
        default:
          return new TransferProxy(serviceProvider.GetRequiredService<IConfigurationStore>(), 
            "AWS_BUCKET_NAME");
      }
    }
  }
}
