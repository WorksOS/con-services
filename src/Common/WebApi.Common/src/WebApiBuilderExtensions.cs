using System;
using System.Reflection;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.AspNetCore.Health;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Prometheus;
using Jaeger;
using Jaeger.Samplers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;

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
        throw new ArgumentNullException("app");

      app.UseExceptionTrap();
      app.UseCors("VSS");
      app.UseFilterMiddleware<RequestIDMiddleware>();

      app.UseSwagger();
      //Swagger documentation can be viewed with http://localhost:5000/swagger/v1/swagger.json
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", serviceTitle);
      });

      app.UseFilterMiddleware<RequestTraceMiddleware>();
      //TIDAuthentication added by those servicesd which need it
      //MVC must be last; added by individual services after their custom services.
      return app;
    }

    private static IMetricsRoot Metrics;

    /// <summary>
    /// Uses the prometheus.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">builder</exception>
    public static IWebHostBuilder UsePrometheus(this IWebHostBuilder builder)
    {
      Metrics = AppMetrics.CreateDefaultBuilder()
        .OutputMetrics.AsPrometheusPlainText()
        .OutputMetrics.AsPrometheusProtobuf()
        .Build();


      if (builder == null)
        throw new ArgumentNullException("builder");

      builder.ConfigureMetrics(Metrics)
        .UseMetrics(
          options =>
          {
            options.EndpointOptions = endpointsOptions =>
            {
              endpointsOptions.MetricsTextEndpointOutputFormatter =
                Metrics.OutputMetricsFormatters.GetType<MetricsPrometheusTextOutputFormatter>();
              endpointsOptions.MetricsEndpointOutputFormatter =
                Metrics.OutputMetricsFormatters.GetType<MetricsPrometheusProtobufOutputFormatter>();
            };
            options.TrackingMiddlewareOptions = trackingOptions =>
            {
              trackingOptions.ApdexTrackingEnabled = true;
            };
          })
        .UseHealth();

      return builder;
    }


    public static IServiceCollection AddJaeger(this IServiceCollection collection, string service_title)
    {
      //Add Jaegar tracing
      try
      {
        collection.AddSingleton<ITracer>(serviceProvider =>
        {
          ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

          ITracer tracer;
          if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_AGENT_HOST")) &&
              !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_AGENT_PORT")) &&
              !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_SAMPLER_TYPE")) &&
              !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("JAEGER_SERVICE_NAME")))
          {

            Configuration jagerConfig = Jaeger.Configuration.FromEnv(loggerFactory);
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

          GlobalTracer.Register(tracer);

          return tracer;
        });
      }
      catch (Exception ex)
      {
       //Dump exception somewhere since logger is not available in this scope
       Console.WriteLine($"Can't start Jaeger since {ex.Message}");
      }

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
