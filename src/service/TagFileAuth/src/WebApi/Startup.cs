﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.TRex.Gateway.Common.Proxy;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.TagFileAuth.WebAPI
{
  /// <summary>
  /// Application startup class.
  /// </summary>
  public class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "Tag File Auth API";

    /// <inheritdoc />
    public override string ServiceDescription => "The service is used for TagFile authorization";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";

    /// <inheritdoc />
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      Log.LogDebug("Loading application service descriptors");

      // Add framework services.
      services
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
        .AddSingleton<IWebRequest, GracefulWebRequest>()

        .AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>()
        .AddTransient<ITPaasProxy, TPaasProxy>()

        .AddTransient<IProjectInternalProxy, ProjectInternalV6Proxy>()
        .AddTransient<IDeviceInternalProxy, DeviceInternalV1Proxy>()
        .AddTransient<ITRexCompactionDataProxy, TRexCompactionDataV1Proxy>();
      
      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });
    }

    /// <inheritdoc />
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
    {  }
  }
}
