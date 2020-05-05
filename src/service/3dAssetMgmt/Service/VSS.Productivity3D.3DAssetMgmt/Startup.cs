using CCSS.CWS.Client;
using CCSS.CWS.Client.MockClients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.WebAPI;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.AssetMgmt3D
{
  /// <summary>
  /// Application startup class.
  /// </summary>
  public class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "3D Asset Management API";

    /// <inheritdoc />
    public override string ServiceDescription => "A service to match 3D assets with telematics and manage legacy asset identifiers in 3D";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";

    /// <inheritdoc />
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      services.AddMvc();

      // Required for TIDAuthentication  
      // CCSSSCON-216 temporary move to real endpoints when available
      services.AddCwsClient<ICwsAccountClient, CwsAccountClient, MockCwsAccountClient>(CwsClientMockExtensionMethods.MOCK_ACCOUNT_KEY);
      services.AddTransient<IProjectProxy, ProjectV6Proxy>();

      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<IWebRequest, GracefulWebRequest>();
      // CCSSSCON-85 placeholder
      //services.AddTransient<IAssetRepository, AssetRepository>();

      services.AddSingleton<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddSingleton<IErrorCodesProvider, AssetMgmt3DExecutionStates>();

      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();
      services.AddSingleton<CacheInvalidationService>();

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
    {
      app.UseFilterMiddleware<AssetMgmt3DAuthentication>();
      app.UseMvc();
    }
  }
}
