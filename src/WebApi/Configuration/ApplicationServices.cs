using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.WebApi.Models.Notification.Helpers;
using VSS.Productivity3D.WebApi.Models.Services;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Notification.Models;
using VSS.TCCFileAccess;

// ReSharper disable once CheckNamespace
namespace VSS.Productivity3D.WebApi
{
  /// <summary>
  /// Partial implemtnation of startup configuration for service descriptor contracts.
  /// </summary>
  public partial class Startup
  {
    /// <summary>
    /// Add required service descriptors to support the DI contract.
    /// </summary>
    /// <param name="services">Collection of service descriptors provided by ASP.NET on configuration startup</param>
    /// <returns>IServiceCollection collection of services for controller DI.</returns>
    public void ConfigureApplicationServices(IServiceCollection services)
    {
      //TODO We may switch over to IOptions as it is safer - proactive config validation vs lazy and strongly typed config values
      services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();
      services.AddScoped<IASNodeClient, ASNodeClient>();
      services.AddScoped<ITagProcessor, TagProcessor>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddSingleton<IProjectSettingsProxy, ProjectSettingsProxy>();
      services.AddSingleton<IProjectListProxy, ProjectListProxy>();
      services.AddSingleton<IFileListProxy, FileListProxy>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddTransient<IFileRepository, FileRepository>();
      services.AddSingleton<IPreferenceProxy, PreferenceProxy>();
      services.AddTransient<ITileGenerator, TileGenerator>();
      services.AddSingleton<IElevationExtentsProxy, ElevationExtentsProxy>();
      services.AddScoped<ICompactionSettingsManager, CompactionSettingsManager>();
      services.AddScoped<IProductionDataRequestFactory, ProductionDataRequestFactory>();
      services.AddScoped<IFilterServiceProxy, FilterServiceProxy>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, RaptorResult>();
      services.AddTransient<ICompactionProfileResultHelper, CompactionProfileResultHelper>();
      services.AddSingleton<IGeofenceProxy, GeofenceProxy>();
      services.AddSingleton<IBoundaryProxy, BoundaryProxy>();
      services.AddScoped<IMapTileGenerator, MapTileGenerator>();
      services.AddScoped<IMapTileService, MapTileService>();
      services.AddScoped<IProjectTileService, ProjectTileService>();
      services.AddScoped<IGeofenceTileService, GeofenceTileService>();
      services.AddScoped<IAlignmentTileService, AlignmentTileService>();
      services.AddScoped<IDxfTileService, DxfTileService>();
      services.AddScoped<IProductionDataTileService, ProductionDataTileService>();
      services.AddScoped<IBoundingBoxService, BoundingBoxService>();
      services.AddScoped<ISchedulerProxy, SchedulerProxy>();
      services.AddScoped<ITransferProxy, TransferProxy>();
      services.AddSingleton<IHostedService, AddFileProcessingService>();
      services.AddSingleton(provider => (IEnqueueItem<ProjectFileDescriptor>)provider.GetService<IHostedService>());

      // Action services
      services.AddSingleton<ISummaryDataHelper, SummaryDataHelper>();

      serviceCollection = services;
    }
  }
}