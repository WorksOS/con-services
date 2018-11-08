using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Threading;
using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using VSS.Productivity3D.TagFileHarvester;
using VSS.Productivity3D.TagFileHarvester.Implementation;
using VSS.Productivity3D.TagFileHarvester.Interfaces;
using VSS.Productivity3D.TagFileHarvester.Models;
using VSS.Productivity3D.TagFileHarvester.TaskQueues;


namespace VSS.Productivity3D.TagFileHarvester.Service
{
  // Provides a task scheduler that ensures a maximum concurrency level while  
  // running on top of the thread pool. 


  public static class TagFileHarvesterService
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    //This is the main list of orgs 
    private static Timer SyncTimer = null;

    public static void Start()
    {
      log.Debug("TagFileHarvester.Start: Entered Start()");
      //register dependencies here
      OrgsHandler.Initialize(new UnityContainer().RegisterType<IFileRepository, FileRepository>()
          .RegisterInstance<IHarvesterTasks>(new LimitedConcurrencyHarvesterTasks())
          .RegisterType<ITAGProcessorClient, TagFileProcessingRaptor>()
          .RegisterInstance<ILog>(log));

      //here we need to sync filespaces and tasks
      SyncTimer = new System.Threading.Timer(OrgsHandler.CheckAvailableOrgs);
      SyncTimer.Change(TimeSpan.FromMinutes(3), TimeSpan.FromMilliseconds(0));
      log.DebugFormat("TagFileHarvester.Start: Running on multiple threads");
    }


    public static void Stop()
    {
      log.Debug("TagFileHarvester.Stop: Entered Stop()");
      //Send everyone cancel message
      OrgsHandler.OrgProcessingTasks.ForEach(t=>t.Value.Item2.Cancel());
      //Wait for them to stop and give 5 minutes timeout to stop
      Task.WaitAll(OrgsHandler.OrgProcessingTasks.Select(t => t.Key).ToArray(),TimeSpan.FromMinutes(5));
      log.Debug("TagFileHarvester.Stop: Exiting Stop()");
    }
  }


  internal class TagFileResults
  {
    public int successCount = 0;
    public int refusedCount = 0;
    public int ignoredCount = 0;
    public int failureCount = 0;

    private Object thisLock = new Object();

    public void Add(TagFileResults results)
    {
      //Make this update thread safe.
      lock (thisLock)
      {
        this.ignoredCount += results.ignoredCount;
        this.refusedCount += results.refusedCount;
        this.successCount += results.successCount;
        this.failureCount += results.failureCount;
      }
    }
  }
}
