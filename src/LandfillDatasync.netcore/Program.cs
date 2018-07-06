using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using LandFillServiceDataSynchronizer;

namespace LandfillDatasync.netcore
{
  internal class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private static void Main(string[] args)
    {
      XmlConfigurator.Configure(new FileInfo("log4net.xml"));

      var dataSync = new DataSynchronizer(Log);


      dataSync.RunUpdateVolumesFromRaptor();


      var startDate = Environment.GetEnvironmentVariable("StartDateForCCA");
      var startUtc = DateTime.UtcNow.Date;
      if (!string.IsNullOrEmpty(startDate)) DateTime.TryParse(startDate, out startUtc);
      dataSync.RunUpdateCCAFromRaptor();
    }
  }
}