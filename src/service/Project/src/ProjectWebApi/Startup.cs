using System;
using CCSS.CWS.Client;
using CCSS.CWS.Client.MockClients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
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
using VSS.Pegasus.Client;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Proxy;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Project.Repository;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.WebAPI;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Proxy;
using VSS.TCCFileAccess;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.TRex.Gateway.Common.Proxy;
using VSS.WebApi.Common;

namespace VSS.MasterData.Project.WebAPI
{
  /// <summary>
  /// Application startup class.
  /// </summary>
  public class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "Project Service API";

    /// <inheritdoc />
    public override string ServiceDescription => " Project masterdata service";

    /// <inheritdoc />
    public override string ServiceVersion => "v6";

    private static IServiceProvider _serviceProvider;

    /// <inheritdoc />
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      //TODO: Check if SetPreflightMaxAge(TimeSpan.FromSeconds(2520) in WebApi pkg matters

      // Add framework services.
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddScoped<IRequestFactory, RequestFactory>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IProjectRepository, ProjectRepository>();
      services.AddScoped<IDeviceRepository, DeviceRepository>();
      services.AddTransient<IProjectSettingsRequestHelper, ProjectSettingsRequestHelper>();
      services.AddScoped<IErrorCodesProvider, ProjectErrorCodesProvider>();
      services.AddTransient<IFileRepository, FileRepository>();
      services.AddSingleton<IDataOceanClient, DataOceanClient>();
      services.AddTransient<IPegasusClient, PegasusClient>();
      services.AddSingleton<Func<TransferProxyType, ITransferProxy>>(_ => TransferProxyMethod);
      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddSingleton<IPreferenceProxy, PreferenceProxy>();

      services.AddScoped<IFilterServiceProxy, FilterV1Proxy>();
      services.AddTransient<ISchedulerProxy, SchedulerV1Proxy>();
      services.AddTransient<ITRexImportFileProxy, TRexImportFileV1Proxy>();
      services.AddTransient<IProductivity3dV1ProxyCoord, Productivity3dV1ProxyCoord>();
      services.AddTransient<IProductivity3dV2ProxyNotification, Productivity3dV2ProxyNotification>();
      services.AddTransient<IProductivity3dV2ProxyCompaction, Productivity3dV2ProxyCompaction>();

      // CCSSSCON-216 temporary move to real endpoints when available
      services.AddCwsClient<ICwsAccountClient, CwsAccountClient, MockCwsAccountClient>(CwsClientMockExtensionMethods.MOCK_ACCOUNT_KEY);
      services.AddCwsClient<ICwsProjectClient, CwsProjectClient, MockCwsProjectClient>(CwsClientMockExtensionMethods.MOCK_PROJECT_KEY);
      services.AddCwsClient<ICwsDeviceClient, CwsDeviceClient, MockCwsDeviceClient>(CwsClientMockExtensionMethods.MOCK_DEVICE_KEY);
      services.AddCwsClient<ICwsDesignClient, CwsDesignClient, MockCwsDesignClient>(CwsClientMockExtensionMethods.MOCK_DESIGN_KEY);
      services.AddCwsClient<ICwsProfileSettingsClient, CwsProfileSettingsClient, MockCwsProfileSettingsClient>(CwsClientMockExtensionMethods.MOCK_PROFILE_KEY);

      services.AddOpenTracing(builder => builder.ConfigureAspNetCore(options => options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping")));

      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();
      services.AddSingleton<CacheInvalidationService>();
      services.AddTransient<ImportedFileUpdateService>();
    }

    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
    {
      app.UseFilterMiddleware<ProjectAuthentication>();
      app.UseStaticFiles();
      // Because we use Flow Files, and Background tasks we sometimes need to reread the body of the request
      // Without this, the Request Body Stream cannot set it's read position to 0.
      // See https://stackoverflow.com/questions/31389781/read-request-body-twice
      app.Use(next => context =>
      {
        context.Request.EnableBuffering();
        return next(context);
      });
      _serviceProvider = ServiceProvider;
    }

    private static ITransferProxy TransferProxyMethod(TransferProxyType type)
    {
      return type switch
      {
        TransferProxyType.DesignImport => new TransferProxy(_serviceProvider.GetRequiredService<IConfigurationStore>(), "AWS_DESIGNIMPORT_BUCKET_NAME"),
        _ => new TransferProxy(_serviceProvider.GetRequiredService<IConfigurationStore>(), "AWS_BUCKET_NAME")
      };
    }
  }
}
