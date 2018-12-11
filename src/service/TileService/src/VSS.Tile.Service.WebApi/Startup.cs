using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Clients;
using VSS.Productivity3D.Push.WebAPI;
using VSS.WebApi.Common;
using VSS.TCCFileAccess;
using VSS.Tile.Service.Common.Authentication;
using VSS.Tile.Service.Common.Helpers;
using VSS.Tile.Service.Common.Interfaces;
using VSS.Tile.Service.Common.Services;

namespace VSS.Tile.Service.WebApi
{
  public class Startup
  {

    /// <summary>
    /// The name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "Tile Service API";

    /// <summary>
    /// For Log4Net
    /// </summary>
    public const string LOGGER_REPO_NAME = "WebApi";

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      // Add framework services.
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<ICustomerProxy, CustomerProxy>(); // used in TID auth for customer/user validation
      services.AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>();//Replace with custom error codes provider if required
      services.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddSingleton<IFileRepository, FileRepository>();
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
      services.AddSingleton<IRaptorProxy, RaptorProxy>();
      services.AddSingleton<IFileListProxy, FileListProxy>();
      services.AddSingleton<IProjectListProxy, ProjectListProxy>();
      services.AddSingleton<IGeofenceProxy, GeofenceProxy>();
      services.AddSingleton<ILoadDumpProxy, LoadDumpProxy>();

      services.AddSingleton<CacheInvalidationService>();

      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });

      services.AddJaeger(SERVICE_TITLE);

      services.AddMemoryCache();
      services.AddCommon<Startup>(SERVICE_TITLE);

      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      //Enable CORS before TID so OPTIONS works without authentication
      app.UseCommon(SERVICE_TITLE);
      app.UseFilterMiddleware<TileAuthentication>();
      app.UseMvc();
      
    }
  }
}
