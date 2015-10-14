using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using TagFileHarvester.Implementation;
using TagFileHarvester.Interfaces;
using Topshelf;
using Topshelf.Runtime;
using Timer = System.Threading.Timer;

namespace TagFileHarvester
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


    private static void Main(string[] args)
    {
      XmlConfigurator.Configure();

      AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

      TopshelfExitCode exitCode = HostFactory.Run(c =>
      {
        c.SetServiceName("_TagFileHarvester");
        c.SetDisplayName("_TagFileHarvester");
        c.SetDescription("Service for finding TAG files and importing to Raptor.");
        c.RunAsLocalSystem();
        c.StartAutomatically();
/*        c.EnableServiceRecovery(cfg =>
        {
          cfg.RestartService(1);
          cfg.RestartService(1);
          cfg.RestartService(1);
        });*/
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
        Log.InfoFormat("Raptor Tag File Harvester Service - {0}", exitCode);
      }
      else
      {
        Log.DebugFormat("Raptor Tag File Harvester Service - {0}", exitCode);
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

    private static Timer SyncTimer = null;

   

    public void Start()
    {

      XmlConfigurator.Configure();

      OrgsHandler.MaxThreadsToProcessTagFiles = TagFileHarvesterServiceSettings.Default.MaxThreadsToProcessTagFiles;
      OrgsHandler.tccSynchFilespaceShortName = TagFileHarvesterServiceSettings.Default.TCCSynchFilespaceShortName;
      OrgsHandler.tccSynchMachineFolder = TagFileHarvesterServiceSettings.Default.TCCSynchMachineControlFolder;
      OrgsHandler.TCCArchiveFiles = TagFileHarvesterServiceSettings.Default.TCCArchiveFiles;
      OrgsHandler.TCCSynchProductionDataFolder = TagFileHarvesterServiceSettings.Default.TCCSynchProductionDataFolder;
      OrgsHandler.TCCSynchProductionDataArchivedFolder = TagFileHarvesterServiceSettings.Default.TCCSynchProductionDataArchivedFolder;
      OrgsHandler.TagFileSubmitterTasksTimeout = TagFileHarvesterServiceSettings.Default.TagFileSubmitterTasksTimeout;
      OrgsHandler.TCCRequestTimeout = TagFileHarvesterServiceSettings.Default.TCCRequestTimeout;
      OrgsHandler.NumberOfFilesInPackage = TagFileHarvesterServiceSettings.Default.NumberOfFilesInPackage;
      OrgsHandler.OrgProcessingDelay = TagFileHarvesterServiceSettings.Default.OrgProcessingDelay;
      OrgsHandler.BookmarkTolerance = TagFileHarvesterServiceSettings.Default.BookmarkTolerance;
      OrgsHandler.EnableHardScanningLogic = TagFileHarvesterServiceSettings.Default.EnableHardScanningLogic;
      OrgsHandler.BadFilesToleranceRollback = TagFileHarvesterServiceSettings.Default.BadFilesToleranceRollback;
      OrgsHandler.CacheEnabled = TagFileHarvesterServiceSettings.Default.CacheEnabled;
      OrgsHandler.FilenameDumpEnabled = TagFileHarvesterServiceSettings.Default.FilenameDumpEnabled;

      Log.Debug("TagFileHarvester.Start: Entered Start()");
      //register dependencies here
      OrgsHandler.Initialize(new UnityContainer().RegisterType<IFileRepository, FileRepository>()
          .RegisterInstance<IBookmarkManager>(XMLBookMarkManager.Instance)
          .RegisterInstance<IHarvesterTasks>(new LimitedConcurrencyHarvesterTasks())
          .RegisterType<ITAGProcessorClient, TagFileProcessingRaptor>()
          .RegisterInstance<ILog>(Log));
      FileRepository.Log = Log;
      TagFileProcessingRaptor.Log = Log;
      XMLBookMarkManager.Log = Log;
      //here we need to sync filespaces and tasks
      SyncTimer = new System.Threading.Timer(OrgsHandler.CheckAvailableOrgs);
      SyncTimer.Change(TimeSpan.FromSeconds(5), TagFileHarvesterServiceSettings.Default.RefreshOrgsDelay);
      Log.Info("TagFileHarvester.Started.");
    }

    public void Stop()
    {
      Log.Info("TagFileHarvester.Stopping.");
      SyncTimer.Change(Timeout.Infinite, Timeout.Infinite);
      OrgsHandler.Container.Resolve<IBookmarkManager>().StopDataExport();
      OrgsHandler.OrgProcessingTasks.ForEach(t=>t.Value.Item2.Cancel());
      Log.InfoFormat("TagFileHarvester.Waiting for {0} tasks....",(OrgsHandler.OrgProcessingTasks.Count));
      Task.WaitAll(OrgsHandler.OrgProcessingTasks.Select(t => t.Key).ToArray(),TimeSpan.FromMinutes(5));
      Log.Info("TagFileHarvester.Stopped.");

    }

  }
}
