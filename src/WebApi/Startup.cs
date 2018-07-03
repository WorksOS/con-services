using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using OpenTracing;
using OpenTracing.Contrib.NetCore.CoreFx;
using OpenTracing.Util;
using Jaeger;
using Jaeger.Samplers;
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
    private const string SERVICE_TITLE = "3dpm Tag File Auth API";

    /// <summary>
    /// Log4net repository logger name.
    /// </summary>
    public const string LOGGER_REPO_NAME = "WebApi";

    private IServiceCollection serviceCollection;

    private static readonly Uri _jaegerUri = new Uri("http://localhost:14268/api/traces");

    /// <summary>
    /// Gets the root configuration object.
    /// </summary>
    public IConfigurationRoot Configuration { get; }



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
      services.AddCommon<Startup>(SERVICE_TITLE, "API for 3D Tag File Auth");
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

      //Add Jaegar tracing
      services.AddSingleton<ITracer>(serviceProvider =>
      {
        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        ITracer tracer;
        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_AGENT_HOST")) &&
            !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_AGENT_PORT")) &&
            !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_SAMPLER_TYPE")) &&
            !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_SERVICE_NAME")))
        {

          Configuration jagerConfig = Jaeger.Configuration.FromEnv(loggerFactory);
          //ISender sender = new UdpSender(jagerConfig.GetTracerBuilder )

          //IReporter reporter = new RemoteReporter.Builder()
          //  .WithSender();

          //By default this sends the tracing results to localhost:6831
          //to test locallay run this docker run -d -p 6831:5775/udp -p 16686:16686 jaegertracing/all-in-one:latest
          tracer = jagerConfig.GetTracerBuilder()
            .Build();

        }
        else
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
      services.AddOpenTracing();


      serviceCollection = services;
    }


    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    /// <summary>
    /// Configures the specified application.
    /// </summary>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.BuildServiceProvider();

      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(LOGGER_REPO_NAME);

      app.UseCommon(SERVICE_TITLE);

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
