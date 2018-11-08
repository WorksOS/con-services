using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Topshelf;
using Topshelf.Runtime;

namespace LandFillServiceDataSynchronizer
{
  class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


    static void Main(string[] args)
    {
      XmlConfigurator.Configure();

      AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

      TopshelfExitCode exitCode = HostFactory.Run(c =>
      {
        c.SetServiceName("_LandfillDataSyncService");
        c.SetDisplayName("_LandfillDataSyncService");
        c.SetDescription("Service for syncing data between landfill app and raptor");
        c.RunAsLocalSystem();
        c.StartAutomatically();
        c.EnableServiceRecovery(cfg =>
        {
          cfg.RestartService(1);
          cfg.RestartService(1);
          cfg.RestartService(1);
        });
        c.Service<ServiceController>(svc =>
        {
          svc.ConstructUsing(ServiceFactory);
          svc.WhenStarted(s => s.Start());
          svc.WhenStopped(s => s.Stop());
        });
        c.UseLog4Net();
      });

      if (exitCode == TopshelfExitCode.Ok)
      {
        Log.InfoFormat("Lanfill datasync service - {0}", exitCode);
      }
      else
      {
        Log.DebugFormat("Lanfill datasync service - {0}", exitCode);
      }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      if (e.IsTerminating)
      {
        Log.Fatal("A fatal unhandled exception has occurred", e.ExceptionObject as Exception);
      }
      else
      {
        Log.Error("A non-fatal unhandled exception has occurred", e.ExceptionObject as Exception);
      }
    }

    private static ServiceController ServiceFactory(HostSettings settings)
    {
      return new ServiceController();
    }
  }

  public class ServiceController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static Timer SyncVolumesTimer = null;
    private static Timer SyncCCATimer = null;

    public void Start()
    {
      var dataSync = new DataSynchronizer(Log);     

      Log.Debug("Starting service...");

      SyncVolumesTimer = new Timer(dataSync.RunUpdateVolumesFromRaptor);
      var sleepTime = ConfigurationManager.AppSettings["HoursToSleepForVolumes"];
      var hoursToSleep = string.IsNullOrEmpty(sleepTime) ? 2 : double.Parse(sleepTime);
      SyncVolumesTimer.Change(TimeSpan.FromMinutes(30), TimeSpan.FromHours(hoursToSleep));

      sleepTime = ConfigurationManager.AppSettings["HoursToSleepForCCA"];
      hoursToSleep = string.IsNullOrEmpty(sleepTime) ? 24 : double.Parse(sleepTime);
      string startDate = ConfigurationManager.AppSettings["StartDateForCCA"];
      DateTime startUtc = DateTime.UtcNow.Date;
      if (!string.IsNullOrEmpty(startDate))
      {
        DateTime.TryParse(startDate, out startUtc);
      }
      SyncCCATimer = new Timer(dataSync.RunUpdateCCAFromRaptor, DateTime.UtcNow.Date.AddMonths(-1), TimeSpan.FromSeconds(5), TimeSpan.FromHours(hoursToSleep));
    }

    public void Stop()
    {
    }
  }
}
