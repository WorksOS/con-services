using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Pegasus.Client;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.WebAPI;
using VSS.TCCFileAccess;
using VSS.Tile.Service.Common.Authentication;
using VSS.Tile.Service.Common.Helpers;
using VSS.Tile.Service.Common.Interfaces;
using VSS.Tile.Service.Common.Services;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.WebApi
{
  public class Startup : BaseStartup
  {

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public override string ServiceName => "Tiling service";
    public override string ServiceDescription => "Provides tiling endpoints and tile generation";
    public override string ServiceVersion => "v1";

    public Startup(IHostingEnvironment env) : base(env, null, useSerilog: true)
    { }

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      // Add framework services.
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<ICustomerProxy, CustomerProxy>(); // used in TID auth for customer/user validation
      services.AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>();//Replace with custom error codes provider if required
      services.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddSingleton<IPreferenceProxy, PreferenceProxy>();
      services.AddScoped<IMapTileGenerator, MapTileGenerator>();
      services.AddScoped<IMapTileService, MapTileService>();
      services.AddScoped<IProjectTileService, ProjectTileService>();
      services.AddScoped<ILoadDumpTileService, LoadDumpTileService>();
      services.AddScoped<IGeofenceTileService, GeofenceTileService>();
      services.AddScoped<IAlignmentTileService, AlignmentTileService>();
      services.AddScoped<IDxfTileService, DxfTileService>();
      services.AddScoped<IBoundingBoxService, BoundingBoxService>();
      services.AddScoped<IBoundingBoxHelper, BoundingBoxHelper>();
      services.AddSingleton<IProductivity3dV2ProxyCompactionTile, Productivity3dV2ProxyCompactionTile>();

      services.AddSingleton<IGeofenceProxy, GeofenceProxy>();
      services.AddSingleton<ILoadDumpProxy, LoadDumpProxy>();
      services.AddTransient<IDataOceanClient, DataOceanClient>();
      services.AddTransient<IPegasusClient, PegasusClient>();
      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();

      services.AddSingleton<IProjectProxy, ProjectV4Proxy>();
      services.AddSingleton<IFileImportProxy, FileImportV4Proxy>();

      services.AddSingleton<IFileRepository, FileRepository>();

      services.AddSingleton<CacheInvalidationService>();

      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();

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
      app.UseFilterMiddleware<TileAuthentication>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
  }
}
