using System;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

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
          });

      return builder;
    }
  }
}
