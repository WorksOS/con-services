using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Cache.MemoryCache;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Project.Proxy;
using VSS.Serilog.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LandfillDatasync.netcore
{
  internal class Program
  {
    private static void Main()
    {
      var provider = new ServiceCollection()
          .AddLogging()
          .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure()))
          .AddSingleton<IConfigurationStore, GenericConfiguration>();

      provider.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
      provider.AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>();
      
      // for serviceDiscovery
      provider.AddServiceDiscovery()
        .AddTransient<IWebRequest, GracefulWebRequest>()
        .AddMemoryCache()
        .AddSingleton<IDataCache, InMemoryDataCache>()
        .AddTransient<IProductivity3dV1ProxyCoord, Productivity3dV1ProxyCoord>()
        .AddTransient<IFileImportProxy, FileImportV4Proxy>();

      var serviceProvider = provider.BuildServiceProvider();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var log = logger.CreateLogger<Program>();
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();

      log.LogDebug("Landfill Data Sync starting");
      var dataSync = new DataSynchronizer(log, logger, configStore, 
        serviceProvider.GetRequiredService<IProductivity3dV1ProxyCoord>(),
        serviceProvider.GetRequiredService<IFileImportProxy>()
        );

      // Optionally specify a specific customer to process (this will be null if not specified)
      // If a specific customerUID is provided land fill data sync will only find projects for that customer
      var customerUid = configStore.GetValueString("LANDFILL_CUSTOMER_UID", string.Empty);

      if (Guid.TryParse(customerUid, out var guid))
      {
        dataSync.CustomerUid = guid;
        log.LogDebug($"Processing CustomerUID: {guid}");
      }

      // *************  Process the volumes for the last nn days  *************** 
      var noOfDaysVolsVar = configStore.GetValueString("NoOfDaysBackForVolumes", string.Empty);
      var noOfDaysVols = -30;

      if (!string.IsNullOrEmpty(noOfDaysVolsVar))
      {
        noOfDaysVols = -Math.Abs(Convert.ToInt32(noOfDaysVolsVar));
      }

      dataSync.RunUpdateVolumesFromProductivity3D(noOfDaysVols);
      log.LogDebug("***** Finished Processing volumes ***** ");

      // *************  Process the CCA for the last nn days  *************** 
      var noOfDaysCca = configStore.GetValueString("NoOfDaysBackForCCA", string.Empty);
      var ccaDaysBackFill = -30;

      if (!string.IsNullOrEmpty(noOfDaysCca))
      {
        ccaDaysBackFill = -Math.Abs(Convert.ToInt32(noOfDaysCca));
      }

      dataSync.RunUpdateCcaFromProductivity3D(ccaDaysBackFill);
      log.LogDebug("***** Finished Processing CCA ******");
    }
  }
}
