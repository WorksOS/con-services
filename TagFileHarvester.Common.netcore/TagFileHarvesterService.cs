using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using TagFileHarvester;
using TagFileHarvester.Implementation;
using TagFileHarvester.Interfaces;

namespace VSS.Nighthawk.ThreeDCommon.ThreeDAPIs.ProjectDataServer
{
  // Provides a task scheduler that ensures a maximum concurrency level while  
  // running on top of the thread pool. 


  public static class TagFileHarvesterService
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    //This is the main list of orgs 
    private static Timer SyncTimer;

    public static void Start()
    {
      log.Debug("TagFileHarvester.Start: Entered Start()");
      //register dependencies here
      OrgsHandler.Initialize(new UnityContainer().RegisterType<IFileRepository, FileRepository>()
        .RegisterInstance<IHarvesterTasks>(new LimitedConcurrencyHarvesterTasks())
        .RegisterInstance(log));

      //here we need to sync filespaces and tasks
      SyncTimer = new Timer(OrgsHandler.CheckAvailableOrgs);
      SyncTimer.Change(TimeSpan.FromMinutes(3), TimeSpan.FromMilliseconds(0));
      log.DebugFormat("TagFileHarvester.Start: Running on multiple threads");
    }


    public static void Stop()
    {
      log.Debug("TagFileHarvester.Stop: Entered Stop()");
      //Send everyone cancel message
      OrgsHandler.OrgProcessingTasks.ForEach(t => t.Value.Item2.Cancel());
      //Wait for them to stop and give 5 minutes timeout to stop
      Task.WaitAll(OrgsHandler.OrgProcessingTasks.Select(t => t.Key).ToArray(), TimeSpan.FromMinutes(5));
      log.Debug("TagFileHarvester.Stop: Exiting Stop()");
    }
  }


  internal class TagFileResults
  {
    public int failureCount;
    public int ignoredCount;
    public int refusedCount;
    public int successCount;

    private readonly object thisLock = new object();

    public void Add(TagFileResults results)
    {
      //Make this update thread safe.
      lock (thisLock)
      {
        ignoredCount += results.ignoredCount;
        refusedCount += results.refusedCount;
        successCount += results.successCount;
        failureCount += results.failureCount;
      }
    }
  }
}