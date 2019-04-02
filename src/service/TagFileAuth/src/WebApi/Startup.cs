using System;
using OpenTracing.Contrib.NetCore.CoreFx;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.FIlters;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Project.Repository;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.WebApi.Common;


namespace VSS.Productivity3D.TagFileAuth.WebAPI
{
  /// <summary>
  /// Configures services and request pipelines.
  /// </summary>
  public class Startup : BaseStartup
  {


    public Startup(IHostingEnvironment env) : base(env, LoggerRepoName)
    {
    }
    /// <summary>
    /// Log4net repository logger name.
    /// </summary>
    public const string LoggerRepoName = "TFA_WebApi";

    public override string ServiceName => "3dpm Tag File Auth API";
    public override string ServiceDescription => "The service is used for TagFile authorization";
    public override string ServiceVersion => "v1";

 
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {

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
    }

  }
}
