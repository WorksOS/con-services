using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.AspNetCore.Health;
using Jaeger;
using Jaeger.Samplers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using Serilog.Extensions.Logging;
using VSS.Serilog.Extensions;

namespace VSS.WebApi.Common
{
  /// <summary>
  /// Extensions methods for adding common Web API middleware to the request execution pipeline
  /// </summary>
  public static class WebApiBuilderExtensions
  {
    /// <summary>
    /// Adds exceptions trap, CORS, Swagger, MVC, ... to the application builder
    /// </summary>
    public static IApplicationBuilder UseCommon(this IApplicationBuilder app, string serviceTitle)
    {
      if (app == null)
      {
        throw new ArgumentNullException(nameof(app));
      }

      app.UseExceptionTrap();
      app.UseFilterMiddleware<RequestIDMiddleware>();

      app.UseSwagger();
      ////Swagger documentation can be viewed with http://localhost:5000/swagger/v1/swagger.json
      app.UseSwaggerUI(c =>
      {
        // This version must match the version in BaseStartup
        c.SwaggerEndpoint("/swagger/v1/swagger.json", serviceTitle);
      });

      app.UseFilterMiddleware<RequestTraceMiddleware>();
      //TIDAuthentication added by those servicesd which need it
      //MVC must be last; added by individual services after their custom services.
      return app;
    }

    public static IWebHostBuilder BuildKestrelWebHost(this IWebHostBuilder builder)
    {
      var configurationRoot = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddJsonFile("kestrelsettings.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
        .Build();

      builder
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseConfiguration(configurationRoot)
        .ConfigureLogging((hostContext, loggingBuilder) =>
        {
          loggingBuilder.AddProvider(
            p => new SerilogLoggerProvider(
              SerilogExtensions.Configure(config: configurationRoot, httpContextAccessor: p.GetService<IHttpContextAccessor>())));
        })
        .ConfigureServices(services =>
        {
          // Setup the ConfigurationRoot so it's available in BaseStartup.
          services.AddSingleton<IConfigurationRoot>(configurationRoot);
        });

      ThreadPool.SetMaxThreads(1024, 2048);
      ThreadPool.SetMinThreads(1024, 2048);

      //Check how many requests we can execute
      ServicePointManager.DefaultConnectionLimit = 128;
      return builder;
    }

    /// <summary>
    /// Uses the prometheus.
    /// </summary>
    /// <param name="builder">The builder.</param>
    [Obsolete("Please do not use this method anymore")]
    public static IWebHostBuilder UsePrometheus(this IWebHostBuilder builder)
    {
      var Metrics = new MetricsBuilder()
        .OutputMetrics.AsPrometheusProtobuf()
        .Build();

      if (builder == null)
        throw new ArgumentNullException(nameof(builder));

      builder.ConfigureMetrics(Metrics)
        .UseMetrics()
        .UseHealth();

      return builder;
    }

    private static object _lock = new object();

    public static IServiceCollection AddJaeger(this IServiceCollection collection, string service_title)
    {
      //Add Jaegar tracing
      collection.AddSingleton<ITracer>(serviceProvider =>
      {
        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        ITracer tracer;
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_AGENT_HOST")) &&
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_AGENT_PORT")) &&
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_SAMPLER_TYPE")) &&
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_SERVICE_NAME")))
        {
          Configuration jagerConfig = Configuration.FromEnv(loggerFactory);
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
          tracer = new Tracer.Builder(service_title)
          .WithLoggerFactory(loggerFactory)
          .WithSampler(sampler)
          .Build();
        }

        lock (_lock)
        {
          if (!GlobalTracer.IsRegistered())
          {
            GlobalTracer.Register(tracer);
          }
        }

        return tracer;
      });

      return collection;
    }

    public static IWebHost BuildHostWithReflectionException(this IWebHostBuilder builder, Func<IWebHostBuilder, IWebHost> build)
    {
      IWebHost result = null;
      try
      {
        result = build.Invoke(builder);
      }
      catch (ReflectionTypeLoadException ex)
      {
        foreach (var item in ex.LoaderExceptions)
        {
          Console.WriteLine(item.Message);
        }

        Environment.Exit(1);
      }

      return result;
    }
  }
}
