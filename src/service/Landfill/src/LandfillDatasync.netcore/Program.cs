using System;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Logging.Abstractions;
using VSS.ConfigurationStore;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml",
  Watch = true)]
namespace LandfillDatasync.netcore
{
  internal class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private static void Main(string[] args)
    {
      var configurationStore = new GenericConfiguration(new NullLoggerFactory());

      Log.Debug("Landfill Data Sync starting");
   //   XmlConfigurator.Configure( new FileInfo("log4net.xml"));
      var dataSync = new DataSynchronizer(Log, configurationStore);

      // Optionally specify a specific customer to process (this will be null if not specified)
      // If a specific customerUID is provided land fill data sync will only find projects for that customer
      var customerUid = configurationStore.GetValueString("LANDFILL_CUSTOMER_UID", string.Empty);
      if (Guid.TryParse(customerUid, out var guid))
      {
        dataSync.CustomerUid = guid;
        Log.Debug($"Processing CustomerUID: {guid}");
      }

      // *************  Process the volumes for the last nn days  *************** 
      var noOfDaysVolsVar = configurationStore.GetValueString("NoOfDaysBackForVolumes", string.Empty);
      var noOfDaysVols = -30;
      if (!string.IsNullOrEmpty(noOfDaysVolsVar))
      {
        noOfDaysVols = -Math.Abs(Convert.ToInt32(noOfDaysVolsVar));
      }
      dataSync.RunUpdateVolumesFromRaptor(noOfDaysVols);
      Log.Debug("***** Finished Processing volumes ***** ");

      // *************  Process the CCA for the last nn days  *************** 
      var noOfDaysCca = configurationStore.GetValueString("NoOfDaysBackForCCA", string.Empty);
      var ccaDaysBackFill = -30;
      if (!string.IsNullOrEmpty(noOfDaysCca))
      {
        ccaDaysBackFill = -Math.Abs(Convert.ToInt32(noOfDaysCca));
      }
      dataSync.RunUpdateCcaFromRaptor(ccaDaysBackFill);
      Log.Debug("***** Finished Processing CCA ******");
    }
  }
}