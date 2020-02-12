using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Extensions.Logging;
using TagFileHarvester.Implementation;
using TagFileHarvester.Interfaces;
using VSS.ConfigurationStore;
using VSS.Nighthawk.ThreeDCommon.ThreeDAPIs.ProjectDataServer;
using VSS.Serilog.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TagFileHarvester.netcore
{
  public class Program
  {
    public class TimedHostedService : IHostedService, IDisposable
    {
      private static readonly GenericConfiguration config = new GenericConfiguration(new NullLoggerFactory());
      private static readonly ILogger Log = new SerilogLoggerProvider(SerilogExtensions.Configure("VSS.TagFileHarvesterService.log")).CreateLogger(nameof(TimedHostedService));
      private static Timer SyncTimer;

      private Timer _timer;

      public TimedHostedService()
      { }

      public Task StartAsync(CancellationToken cancellationToken)
      {
        OrgsHandler.MaxThreadsToProcessTagFiles = config.GetValueInt("MaxThreadsToProcessTagFiles");
        OrgsHandler.tccSynchFilespaceShortName = config.GetValueString("TCCSynchFilespaceShortName");
        OrgsHandler.tccSynchMachineFolder = config.GetValueString("TCCSynchMachineControlFolder");
        OrgsHandler.TCCSynchProductionDataFolder = config.GetValueString("TCCSynchProductionDataFolder");
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
        OrgsHandler.VssServiceName = config.GetValueString("PRODUCTIVITY3D_VSS_SERVICE_NAME", "productivity3dvss-service");


        ServicePointManager.DefaultConnectionLimit = 8;

        Log.LogDebug("TagFileHarvester.Start: Entered Start()");
        
        var container = new UnityContainer().RegisterType<IFileRepository, FileRepository>()
                            .RegisterInstance<IHarvesterTasks>(new LimitedConcurrencyHarvesterTasks())
                            .RegisterInstance(Log);

        //register dependencies here
        OrgsHandler.Initialize(container);
        FileRepository.Log = Log;

        //here we need to sync filespaces and tasks
        SyncTimer = new Timer(OrgsHandler.CheckAvailableOrgs);
        SyncTimer.Change(TimeSpan.FromSeconds(5), (TimeSpan)config.GetValueTimeSpan("RefreshOrgsDelay"));

        Log.LogInformation("TagFileHarvester.Started.");

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

    public static async Task Main()
    {
      var hostBuilder = new HostBuilder()
        // Add configuration, logging, ...
        .ConfigureServices((hostContext, services) => { services.AddSingleton<IHostedService, TimedHostedService>(); });

      await hostBuilder.RunConsoleAsync();
    }
  }
}

