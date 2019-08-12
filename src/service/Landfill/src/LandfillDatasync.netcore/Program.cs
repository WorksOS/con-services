using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using VSS.ConfigurationStore;
using VSS.Serilog.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LandfillDatasync.netcore
{
  internal class Program
  {
    private static void Main()
    {
      var provider = new ServiceCollection()
                     .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Landfill.DataSync.log")))
                     .BuildServiceProvider();

      var logger = provider.GetService<ILogger>();
      var configurationStore = new GenericConfiguration(new NullLoggerFactory());

      logger.LogDebug("Landfill Data Sync starting");
      var dataSync = new DataSynchronizer(logger, configurationStore);

      // Optionally specify a specific customer to process (this will be null if not specified)
      // If a specific customerUID is provided land fill data sync will only find projects for that customer
      var customerUid = configurationStore.GetValueString("LANDFILL_CUSTOMER_UID", string.Empty);

      if (Guid.TryParse(customerUid, out var guid))
      {
        dataSync.CustomerUid = guid;
        logger.LogDebug($"Processing CustomerUID: {guid}");
      }

      // *************  Process the volumes for the last nn days  *************** 
      var noOfDaysVolsVar = configurationStore.GetValueString("NoOfDaysBackForVolumes", string.Empty);
      var noOfDaysVols = -30;

      if (!string.IsNullOrEmpty(noOfDaysVolsVar))
      {
        noOfDaysVols = -Math.Abs(Convert.ToInt32(noOfDaysVolsVar));
      }

      dataSync.RunUpdateVolumesFromProductivity3D(noOfDaysVols);
      logger.LogDebug("***** Finished Processing volumes ***** ");

      // *************  Process the CCA for the last nn days  *************** 
      var noOfDaysCca = configurationStore.GetValueString("NoOfDaysBackForCCA", string.Empty);
      var ccaDaysBackFill = -30;

      if (!string.IsNullOrEmpty(noOfDaysCca))
      {
        ccaDaysBackFill = -Math.Abs(Convert.ToInt32(noOfDaysCca));
      }

      dataSync.RunUpdateCcaFromProductivity3D(ccaDaysBackFill);
      logger.LogDebug("***** Finished Processing CCA ******");
    }
  }
}
