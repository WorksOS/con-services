using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Contrib.NetCore.CoreFx;
using OpenTracing.Util;
using System;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
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

    private static readonly Uri _jaegerUri = new Uri("http://localhost:14268/api/traces");

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
      services.AddTransient<IGeofenceProxy, GeofenceProxy>();
      services.AddTransient<IRaptorProxy, RaptorProxy>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddScoped<IRequestFactory, RequestFactory>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IProjectRepository, ProjectRepository>();
      services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
      services.AddScoped<ICustomerRepository, CustomerRepository>();
      services.AddTransient<IProjectSettingsRequestHelper, ProjectSettingsRequestHelper>();
      services.AddScoped<IErrorCodesProvider, ProjectErrorCodesProvider>();

      //Add Jaegar tracing
      services.AddSingleton<ITracer>(serviceProvider =>
      {
        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        ITracer tracer;
        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_AGENT_HOST")) && 
            !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_AGENT_PORT")) && 
            !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_SAMPLER_TYPE")) &&
            !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_SERVICE_NAME"))) {

          Configuration jagerConfig = Jaeger.Configuration.FromEnv(loggerFactory);
          //ISender sender = new UdpSender(jagerConfig.GetTracerBuilder )

          //IReporter reporter = new RemoteReporter.Builder()
          //  .WithSender();

          //By default this sends the tracing results to localhost:6831
          //to test locallay run this docker run -d -p 6831:5775/udp -p 16686:16686 jaegertracing/all-in-one:latest
          tracer = jagerConfig.GetTracerBuilder()
              .Build();

        } else
        {
          //Use default tracer

          ISampler sampler = new ConstSampler(sample: true);

          //By default this sends the tracing results to localhost:6831
          //to test locallay run this docker run -d -p 6831:5775/udp -p 16686:16686 jaegertracing/all-in-one:latest
          tracer = new Tracer.Builder(SERVICE_TITLE)
              .WithLoggerFactory(loggerFactory)
              .WithSampler(sampler)
              .Build();
        }



        



        GlobalTracer.Register(tracer);

        return tracer;
      });

      // Prevent endless loops when OpenTracing is tracking HTTP requests to Jaeger.
      services.Configure<HttpHandlerDiagnosticOptions>(options =>
      {
        options.IgnorePatterns.Add(request => _jaegerUri.IsBaseOf(request.RequestUri));
      });



      //GlobalTracer.Register();

      services.AddOpenTracing();


      services.AddMemoryCache();

      var tccUrl = (new GenericConfiguration(new LoggerFactory())).GetValueString("TCCBASEURL");
      var useMock = string.IsNullOrEmpty(tccUrl) || tccUrl == "mock";
      if (useMock)
        services.AddTransient<IFileRepository, MockFileRepository>();
      else
        services.AddTransient<IFileRepository, FileRepository>();

      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


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
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.BuildServiceProvider();

      //HealthMiddleware must be before Common
      app.UseMiddleware<HealthMiddleware>();

      //Enable CORS before TID so OPTIONS works without authentication
      app.UseCommon(SERVICE_TITLE);

#if NET_4_7
      if (Configuration["newrelic"] == "true")
      {
        app.UseMiddleware<NewRelicMiddleware>();
      }
#endif



      app.UseFilterMiddleware<ProjectAuthentication>();
      app.UseMvc();
    }
  }
}