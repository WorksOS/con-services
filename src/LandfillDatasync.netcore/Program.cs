using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;

namespace LandfillDatasync.netcore
{
  internal class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private static void Main(string[] args)
    {
      XmlConfigurator.Configure(new FileInfo("log4net.xml"));
      var dataSync = new DataSynchronizer(Log);

      // *************  Process the volumes for the last nn days  *************** 
      var noOfDaysVolsVar = Environment.GetEnvironmentVariable("NoOfDaysBackForVolumes");
      var noOfDaysVols = 30;
      if (!string.IsNullOrEmpty(noOfDaysVolsVar))
      {
        noOfDaysVols = -Math.Abs(Convert.ToInt32(noOfDaysVolsVar));
      }
      dataSync.RunUpdateVolumesFromRaptor(noOfDaysVols);
      Log.Debug("***** Finished Processing volumes ***** ");

      // *************  Process the CCA for the last nn days  *************** 
      var noOfDaysCca = Environment.GetEnvironmentVariable("NoOfDaysBackForCCA");
      var ccaDaysBackFill = 30;
      if (!string.IsNullOrEmpty(noOfDaysCca))
      {
        ccaDaysBackFill = -Math.Abs(Convert.ToInt32(noOfDaysCca));
      }
      dataSync.RunUpdateCcaFromRaptor(ccaDaysBackFill);
      Log.Debug("***** Finished Processing CCA ******");
    }
  }
}