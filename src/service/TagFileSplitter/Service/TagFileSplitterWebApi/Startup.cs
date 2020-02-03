using System;
using CCSS.TagFileSplitter.WebAPI.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.WebApi.Common;

namespace CCSS.TagFileSplitter.WebAPI
{
  /// <summary>
  /// Application startup class.
  /// </summary>
  public class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "TagFileSplitter Service API";

    /// <inheritdoc />
    public override string ServiceDescription => " TagFileSplitter service";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";

    private static IServiceProvider serviceProvider;

    /// <inheritdoc />
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      // Add framework services.
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddScoped<ICustomerRepository, CustomerRepository>();
      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddTransient<IGenericHttpProxy, GenericHttpProxy>();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });

    }

    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
    {
      app.UseFilterMiddleware<TagFileSplitterAuthentication>();
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
      serviceProvider = ServiceProvider;
    }
  }
}
