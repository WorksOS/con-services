using System.Linq;
using CCSS.CWS.Client;
using CCSS.CWS.Client.MockClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Proxy;
using VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Proxy;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Configuration;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Interfaces;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.WebApi.Models.Notification.Helpers;
using VSS.Productivity3D.WebApi.Models.Services;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Notification.Models;
using VSS.TCCFileAccess;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.TRex.Gateway.Common.Proxy;

// ReSharper disable once CheckNamespace
namespace VSS.Productivity3D.WebApi
{
  /// <summary>
  /// Partial implementation of startup configuration for service descriptor contracts.
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
#if RAPTOR
      services.AddScoped<IASNodeClient, ASNodeClient>();
      services.AddScoped<ITagProcessor, TagProcessor>();
      services.AddScoped<IErrorCodesProvider, RaptorResult>();
#else
      services.AddScoped<IErrorCodesProvider, TRexResult>();
#endif
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();

      // Required for TIDAuthentication  
      // CCSSSCON-216 temporary move to real endpoints when available
      services.AddCwsClient<ICwsAccountClient, CwsAccountClient, MockCwsAccountClient>("MOCK_CWS_ACCOUNT");

      services.AddTransient<IFileRepository, FileRepository>();
      services.AddSingleton<IPreferenceProxy, PreferenceProxy>();
      services.AddTransient<ITileGenerator, TileGenerator>();
      services.AddSingleton<IElevationExtentsProxy, ElevationExtentsProxy>();
      services.AddScoped<ICompactionSettingsManager, CompactionSettingsManager>();
      services.AddScoped<IProductionDataRequestFactory, ProductionDataRequestFactory>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddTransient<ICompactionProfileResultHelper, CompactionProfileResultHelper>();
      services.AddScoped<IProductionDataTileService, ProductionDataTileService>();
      services.AddScoped<IBoundingBoxService, BoundingBoxService>();
      services.AddScoped<ITransferProxy>(sp => new TransferProxy(sp.GetRequiredService<IConfigurationStore>(), "AWS_TAGFILE_BUCKET_NAME"));
      services.AddSingleton<IHostedService, AddFileProcessingService>();
      services.AddSingleton(provider => (IEnqueueItem<ProjectFileDescriptor>) provider.GetServices<IHostedService>()
                                                                                      .First(service => service.GetType() == typeof(AddFileProcessingService)));
      services.AddSingleton<IBoundingBoxHelper, BoundingBoxHelper>();
      services.AddSingleton<IRaptorFileUploadUtility, RaptorFileUploadUtility>();

      // Action services
      services.AddSingleton<ISummaryDataHelper, SummaryDataHelper>();
      services.AddTransient<IProjectSettingsProxy, ProjectSettingsV4Proxy>();
      services.AddTransient<IProjectProxy, ProjectV6Proxy>();
      services.AddTransient<IFileImportProxy, FileImportV6Proxy>();
      services.AddTransient<IFilterServiceProxy, FilterV1Proxy>();
      services.AddTransient<ISchedulerProxy, SchedulerV1Proxy>();
      services.AddTransient<ITRexTagFileProxy, TRexTagFileV2Proxy>();
      services.AddTransient<ITRexConnectedSiteProxy, TRexConnectedSiteV1Proxy>();
      services.AddTransient<ITRexCompactionDataProxy, TRexCompactionDataV1Proxy>();
      services.AddTransient<ITagFileAuthProjectProxy, TagFileAuthProjectV4Proxy>();

      //Disable CAP for now #76666
      /*
      var serviceProvider = services.BuildServiceProvider();
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
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
        });
        x.UseDashboard(); //View dashboard at http://localhost:5000/cap
      });
      */
    }
  }
}
