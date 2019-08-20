using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Serilog.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LandfillDatasync.netcore
{
  internal class Program
  {
    protected static IServiceProvider ServiceProvider;
    protected static IConfigurationStore ConfigStore;
    protected static ILoggerFactory Logger;
    protected static ILogger Log;
    private static void Main()
    {
      var provider = new ServiceCollection()
          .AddLogging()
          .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure()))
          .AddSingleton<IConfigurationStore, GenericConfiguration>();

      provider.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
      provider.AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>(); 
      ServiceProvider = provider.BuildServiceProvider();
      Logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      Log = Logger.CreateLogger<Program>();
      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();

      Log.LogDebug("Landfill Data Sync starting");
      var dataSync = new DataSynchronizer(Log, ConfigStore);

      // Optionally specify a specific customer to process (this will be null if not specified)
      // If a specific customerUID is provided land fill data sync will only find projects for that customer
      var customerUid = ConfigStore.GetValueString("LANDFILL_CUSTOMER_UID", string.Empty);

      if (Guid.TryParse(customerUid, out var guid))
      {
        dataSync.CustomerUid = guid;
        Log.LogDebug($"Processing CustomerUID: {guid}");
      }

      // *************  Process the volumes for the last nn days  *************** 
      var noOfDaysVolsVar = ConfigStore.GetValueString("NoOfDaysBackForVolumes", string.Empty);
      var noOfDaysVols = -30;

      if (!string.IsNullOrEmpty(noOfDaysVolsVar))
      {
        noOfDaysVols = -Math.Abs(Convert.ToInt32(noOfDaysVolsVar));
      }

      dataSync.RunUpdateVolumesFromProductivity3D(noOfDaysVols);
      Log.LogDebug("***** Finished Processing volumes ***** ");

      // *************  Process the CCA for the last nn days  *************** 
      var noOfDaysCca = ConfigStore.GetValueString("NoOfDaysBackForCCA", string.Empty);
      var ccaDaysBackFill = -30;

      if (!string.IsNullOrEmpty(noOfDaysCca))
      {
        ccaDaysBackFill = -Math.Abs(Convert.ToInt32(noOfDaysCca));
      }

      dataSync.RunUpdateCcaFromProductivity3D(ccaDaysBackFill);
      Log.LogDebug("***** Finished Processing CCA ******");
    }
  }
}
