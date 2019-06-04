using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Services;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.MasterData.Project.WebAPI.Middleware;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Pegasus.Client;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Project.Repository;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.WebAPI;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Proxy;
using VSS.TCCFileAccess;
using VSS.TRex.Mutable.Gateway.Abstractions;
using VSS.TRex.Mutable.Gateway.Proxy;
using VSS.WebApi.Common;

namespace VSS.MasterData.Project.WebAPI
{
  /// <summary>
  /// 
  /// </summary>
  public class Startup : BaseStartup
  {
    public const string LoggerRepoName = "projectservice";
    public override string ServiceName => "Project Service API";
    public override string ServiceDescription => " Project masterdata service";
    public override string ServiceVersion => "v4";

    private static IServiceProvider serviceProvider;

    public Startup(IHostingEnvironment env) : base(env, LoggerRepoName)
    { }

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
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
      services.AddTransient<IFileRepository, FileRepository>();
      services.AddTransient<IDataOceanClient, DataOceanClient>();
      services.AddTransient<IPegasusClient, PegasusClient>();
      services.AddSingleton<Func<TransferProxyType, ITransferProxy>>(transfer => TransferProxyMethod);
      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddSingleton<IPreferenceProxy, PreferenceProxy>();

      services.AddScoped<IFilterServiceProxy, FilterV1ServiceDiscoveryProxy>();
      services.AddTransient<ISchedulerProxy, SchedulerV1ServiceDiscoveryProxy>();
      services.AddTransient<ITRexImportFileProxy, TRexImportFileV1ServiceDiscoveryProxy>();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });

      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();
      services.AddSingleton<CacheInvalidationService>();
      services.AddTransient<ImportedFileUpdateService>();

      //Note: The injection of CAP subscriber service needed before 'services.AddCap()'
      //services.AddTransient<ISubscriberService, SubscriberService>();
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
    }

    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
    {
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
      serviceProvider = ServiceProvider;
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
