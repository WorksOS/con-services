using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using TagFileHarvester.Implementation;
using TagFileHarvester.Interfaces;
using VSS.ConfigurationStore;
using VSS.Nighthawk.ThreeDCommon.ThreeDAPIs.ProjectDataServer;

namespace TagFileHarvester.netcore
{
  public class Program
  {

    public class TimedHostedService : IHostedService, IDisposable
    {
      private Timer _timer;

      public TimedHostedService()
      {
      }

      public Task StartAsync(CancellationToken cancellationToken)
      {

        XmlConfigurator.Configure(new FileInfo("log4net.xml"));


        OrgsHandler.MaxThreadsToProcessTagFiles = config.GetValueInt("MaxThreadsToProcessTagFiles");
        OrgsHandler.tccSynchFilespaceShortName = config.GetValueString("TCCSynchFilespaceShortName");
        OrgsHandler.tccSynchMachineFolder = config.GetValueString("TCCSynchMachineControlFolder");
        OrgsHandler.TCCSynchProductionDataFolder = config.GetValueString("Default.TCCSynchProductionDataFolder");
        OrgsHandler.TCCSynchProductionDataArchivedFolder =
          config.GetValueString("TCCSynchProductionDataArchivedFolder");
        OrgsHandler.TCCSynchProjectBoundaryIssueFolder =
          config.GetValueString("TCCSynchProjectBoundaryIssueFolder");
        OrgsHandler.TCCSynchSubscriptionIssueFolder =
          config.GetValueString("TCCSynchSubscriptionIssueFolder");
        OrgsHandler.TCCSynchOtherIssueFolder = config.GetValueString("TCCSynchOtherIssueFolder");
        OrgsHandler.TagFilesFolderLifeSpanInDays = (byte)config.GetValueInt("TagFilesFolderLifeSpanInDays");
        OrgsHandler.TagFileSubmitterTasksTimeout = (TimeSpan)config.GetValueTimeSpan("TagFileSubmitterTasksTimeout");
        OrgsHandler.TCCRequestTimeout = (TimeSpan)config.GetValueTimeSpan("TCCRequestTimeout");
        OrgsHandler.NumberOfFilesInPackage = config.GetValueInt("NumberOfFilesInPackage");
        OrgsHandler.OrgProcessingDelay = (TimeSpan)config.GetValueTimeSpan("OrgProcessingDelay");
        OrgsHandler.CacheEnabled = (bool)config.GetValueBool("CacheEnabled");
        OrgsHandler.FilenameDumpEnabled = (bool)config.GetValueBool("FilenameDumpEnabled");
        OrgsHandler.ShortOrgName = config.GetValueString("ShortOrgName");
        OrgsHandler.TagFileEndpoint = config.GetValueString("TagFileEndpoint");
        OrgsHandler.newrelic = config.GetValueString("newrelic");


        ServicePointManager.DefaultConnectionLimit = 8;

        Log.Debug("TagFileHarvester.Start: Entered Start()");
        //register dependencies here
        OrgsHandler.Initialize(new UnityContainer().RegisterType<IFileRepository, FileRepository>()
          .RegisterInstance<IHarvesterTasks>(new LimitedConcurrencyHarvesterTasks())
          .RegisterInstance(Log));
        FileRepository.Log = Log;
        //here we need to sync filespaces and tasks
        SyncTimer = new Timer(OrgsHandler.CheckAvailableOrgs);
        SyncTimer.Change(TimeSpan.FromSeconds(5), (TimeSpan)config.GetValueTimeSpan("RefreshOrgsDelay"));

        Log.Info("TagFileHarvester.Started.");

        return Task.CompletedTask;
      }

      public Task StopAsync(CancellationToken cancellationToken)
      {

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
      }

      public void Dispose()
      {
        _timer?.Dispose();
      }
    }

    private static readonly GenericConfiguration config = new GenericConfiguration(new NullLoggerFactory());
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static Timer SyncTimer;


    public static async Task Main(string[] args)
    {
      var hostBuilder = new HostBuilder()
        // Add configuration, logging, ...
        .ConfigureServices((hostContext, services) => { services.AddSingleton<IHostedService, TimedHostedService>(); });

      await hostBuilder.RunConsoleAsync();
    }
  }
}

