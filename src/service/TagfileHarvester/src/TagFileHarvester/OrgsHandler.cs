<<<<<<< HEAD:src/TagFileHarvester/OrgsHandler.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using VSS.Productivity3D.TagFileHarvester.Interfaces;
using VSS.Productivity3D.TagFileHarvester.Models;
using VSS.Productivity3D.TagFileHarvester.TaskQueues;

namespace VSS.Productivity3D.TagFileHarvester
{
  public static class OrgsHandler
  {
    //Static settings initilized here
    public static int MaxThreadsToProcessTagFiles = 256;
    public static string tccSynchFilespaceShortName;
    public static string tccSynchMachineFolder;
    public static bool TCCArchiveFiles=false;
    public static string TCCSynchProductionDataFolder;
    public static string TCCSynchProductionDataArchivedFolder;
    public static TimeSpan TagFileSubmitterTasksTimeout;
    public static TimeSpan TCCRequestTimeout;
    public static int NumberOfFilesInPackage;
    public static TimeSpan OrgProcessingDelay;
    public static TimeSpan FolderSearchTimeSpan;
    public static bool UseModifyTimeInsteadOfCreateTime;
    public static string BookmarkPath;



    public static Dictionary<System.Threading.Tasks.Task,Tuple<Organization, CancellationTokenSource>> OrgProcessingTasks 
    {
      get { return orgProcessingTasks; }
    }

    private static readonly Dictionary<System.Threading.Tasks.Task, Tuple<Organization, CancellationTokenSource>> orgProcessingTasks = new Dictionary<System.Threading.Tasks.Task, Tuple<Organization, CancellationTokenSource>>();
    private static ILog log;

    public static IUnityContainer Container { get; private set; }
    public static TimeSpan BookmarkTolerance  { get; set; }
    public static bool EnableHardScanningLogic { get; set; }
    public static TimeSpan BadFilesToleranceRollback { get; set; }
    public static bool CacheEnabled { get; set; }
    public static bool FilenameDumpEnabled { get; set; }

    public static readonly object OrgListLocker = new object();

    public static void Clean()
    {
      orgProcessingTasks.Clear();
    }

    public static void Initialize(IUnityContainer container)
    {
      Container = container;
      log = Container.Resolve<ILog>();
    }

    public static List<Organization> GetOrgs()
    {
      return Container.Resolve<IFileRepository>().ListOrganizations();
    }

    public static void CheckAvailableOrgs(object sender)
    {
      try
      {
        //Make sure that data export is running
        Container.Resolve<IBookmarkManager>().StopDataExport();
        Container.Resolve<IBookmarkManager>().StartDataExport();
        log.InfoFormat("Got {0} updated bookmarks after merging", Container.Resolve<IBookmarkManager>().MergeWithUpdatedBookmarks());
        var orgs = GetOrgs();
        log.InfoFormat("Got {0} orgs from repository", orgs.Count);
        MergeAndProcessOrgs(orgs);
      }
      catch (Exception ex)
      {
        log.ErrorFormat("Exception while listing orgs occured {0}", ex.Message);
      }
    }

    private static void MergeAndProcessOrgs(List<Organization> orgs)
    {
      string result="";
      foreach (var organization in orgs)
      {
        result += organization.orgDisplayName + "," + organization.shortName + "," + organization.filespaceId + "," + organization.orgId + "\n";
      }
      //Filter out all stopped\completed tasks
      lock (OrgListLocker)
      {
        var stoppedTasks = OrgProcessingTasks.Where(o => o.Key.IsCompleted || o.Key.IsFaulted || o.Key.IsCanceled)
          .Select(d => d.Key).ToList();
        stoppedTasks.ForEach(t=>OrgProcessingTasks.Remove(t));
        log.InfoFormat("Currently processing orgs: {0} ",
          OrgProcessingTasks.Select(o => o.Value.Item1).DefaultIfEmpty(new Organization())
            .Select(t => t.shortName).Aggregate((current, next) => current + ", " + next));
        log.DebugFormat("Tasks status when trying to add new orgs is {0} in Queue1 and {1} in Queue2 on {2} Threads",
          Container.Resolve<IHarvesterTasks>().Status().Item1,
          Container.Resolve<IHarvesterTasks>().Status().Item2, GetUsedThreads());
        //do merge here - if there is no org in the list of tasks - build it. If there is no org but there in the list but there is a task - kill the task
        orgs.Where(o => !OrgProcessingTasks.Select(t => t.Value.Item1).Contains(o)).ForEach(o =>
        {
          log.InfoFormat("Adding {0} org for processing", o.shortName);
          var cancellationToken = new CancellationTokenSource();
          OrgProcessingTasks.Add(Container.Resolve<IHarvesterTasks>().StartNewLimitedConcurrency2(() =>
          {
            new OrgProcessorTask(Container, o, cancellationToken, OrgProcessingTasks).ProcessOrg(false,
              t => log
                .InfoFormat("Tasks status is {0} in Queue1 and {1} in Queue2 on {2} Threads",
                  Container.Resolve<IHarvesterTasks>().Status().Item1,
                  Container.Resolve<IHarvesterTasks>().Status().Item2, GetUsedThreads()));
          }, cancellationToken.Token), new Tuple<Organization, CancellationTokenSource>(o, cancellationToken));
        });
        //Reversed situation - org has been removed from filespaces but there is a task - cancel it
        if (OrgProcessingTasks.Any(o => !orgs.Contains(o.Value.Item1)))
        {
          OrgProcessingTasks.Where(o => !orgs.Contains(o.Value.Item1)).ForEach(o => o.Value.Item2.Cancel());
          log.InfoFormat("Removing {0} org from processing",
            OrgProcessingTasks.Select(o => o.Value.Item1)
              .Where(y => !orgs.Contains(y))
              .Select(t => t.shortName)
              .Aggregate((current, next) => current + ", " + next));
          OrgProcessingTasks.Where(o => !orgs.Contains(o.Value.Item1))
            .Select(d => d.Key)
            .ForEach(t => OrgProcessingTasks.Remove(t));
        }
      }
    }

    public static int GetUsedThreads()
    {
      int i, j, k;
      ThreadPool.GetAvailableThreads(out i, out k);
      ThreadPool.GetMaxThreads(out j, out k);
      return j-i;
    }
  }
=======
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using TagFileHarvester.Interfaces;
using TagFileHarvester.Models;
using TagFileHarvester.TaskQueues;

namespace TagFileHarvester
{
  public static class OrgsHandler
  {
    //Static settings initilized here
    public static int MaxThreadsToProcessTagFiles = 256;
    public static string tccSynchFilespaceShortName;
    public static string tccSynchMachineFolder;
    public static string TCCSynchProductionDataFolder;
    public static string TCCSynchProductionDataArchivedFolder;
    public static string TCCSynchProjectBoundaryIssueFolder;
    public static string TCCSynchSubscriptionIssueFolder;
    public static string TCCSynchOtherIssueFolder;
    public static TimeSpan TagFileSubmitterTasksTimeout;
    public static TimeSpan TCCRequestTimeout;
    public static int NumberOfFilesInPackage;
    public static TimeSpan OrgProcessingDelay;
    public static TimeSpan FolderSearchTimeSpan;
    public static bool UseModifyTimeInsteadOfCreateTime;
    public static string BookmarkPath;
    public static byte TagFilesFolderLifeSpanInDays;
    public static string ShortOrgName;



    public static Dictionary<System.Threading.Tasks.Task,Tuple<Organization, CancellationTokenSource>> OrgProcessingTasks 
    {
      get { return orgProcessingTasks; }
    }

    private static readonly Dictionary<System.Threading.Tasks.Task, Tuple<Organization, CancellationTokenSource>> orgProcessingTasks = new Dictionary<System.Threading.Tasks.Task, Tuple<Organization, CancellationTokenSource>>();
    private static ILog log;

    public static IUnityContainer Container { get; private set; }
    public static TimeSpan BookmarkTolerance  { get; set; }
    public static bool EnableHardScanningLogic { get; set; }
    public static TimeSpan BadFilesToleranceRollback { get; set; }
    public static bool CacheEnabled { get; set; }
    public static bool FilenameDumpEnabled { get; set; }
    public static string TagFileEndpoint { get; set; }

    public static readonly object OrgListLocker = new object();

    public static void Clean()
    {
      orgProcessingTasks.Clear();
    }

    public static void Initialize(IUnityContainer container)
    {
      Container = container;
      log = Container.Resolve<ILog>();
    }

    public static List<Organization> GetOrgs()
    {
      return Container.Resolve<IFileRepository>().ListOrganizations();
    }

    public static void CheckAvailableOrgs(object sender)
    {
      try
      {
        //Make sure that data export is running
        var orgs = GetOrgs();
        log.InfoFormat("Got {0} orgs from repository", orgs.Count);
        MergeAndProcessOrgs(orgs);
      }
      catch (Exception ex)
      {
        log.ErrorFormat("Exception while listing orgs occured {0}", ex.Message);
      }
    }

    private static void MergeAndProcessOrgs(List<Organization> orgs)
    {
      string result="";
      foreach (var organization in orgs)
      {
        result += organization.orgDisplayName + "," + organization.shortName + "," + organization.filespaceId + "," + organization.orgId + "\n";
      }
      //Filter out all stopped\completed tasks
      lock (OrgListLocker)
      {
        OrgProcessingTasks.Where(o => o.Key.IsCompleted || o.Key.IsFaulted || o.Key.IsCanceled)
          .Select(d => d.Key)
          .ForEach(d => OrgProcessingTasks.Remove(d));
        log.InfoFormat("Currently processing orgs: {0} ",
          OrgProcessingTasks.Select(o => o.Value.Item1).DefaultIfEmpty(new Organization())
            .Select(t => t.shortName).Aggregate((current, next) => current + ", " + next));
        log.DebugFormat("Tasks status when trying to add new orgs is {0} in Queue1 and {1} in Queue2 on {2} Threads",
          Container.Resolve<IHarvesterTasks>().Status().Item1,
          Container.Resolve<IHarvesterTasks>().Status().Item2, GetUsedThreads());
        //do merge here - if there is no org in the list of tasks - build it. If there is no org but there in the list but there is a task - kill the task
        orgs.Where(o => !OrgProcessingTasks.Select(t => t.Value.Item1).Contains(o)).Where(o => String.IsNullOrEmpty(OrgsHandler.ShortOrgName) || o.shortName == OrgsHandler.ShortOrgName).ForEach(o =>
        {
          log.InfoFormat("Adding {0} org for processing", o.shortName);
          var cancellationToken = new CancellationTokenSource();
          OrgProcessingTasks.Add(Container.Resolve<IHarvesterTasks>().StartNewLimitedConcurrency2(() =>
          {
              new OrgProcessorTask(Container, o, cancellationToken, OrgProcessingTasks).ProcessOrg(false,
              t => log
                .InfoFormat("Tasks status is {0} in Queue1 and {1} in Queue2 on {2} Threads",
                  Container.Resolve<IHarvesterTasks>().Status().Item1,
                  Container.Resolve<IHarvesterTasks>().Status().Item2, GetUsedThreads()));
          }, cancellationToken.Token,false), new Tuple<Organization, CancellationTokenSource>(o, cancellationToken));
        });
        //Reversed situation - org has been removed from filespaces but there is a task - cancel it
        if (OrgProcessingTasks.Any(o => !orgs.Contains(o.Value.Item1)))
        {
          OrgProcessingTasks.Where(o => !orgs.Contains(o.Value.Item1)).ForEach(o => o.Value.Item2.Cancel());
          log.InfoFormat("Removing {0} org from processing",
            OrgProcessingTasks.Select(o => o.Value.Item1)
              .Where(y => !orgs.Contains(y))
              .Select(t => t.shortName)
              .Aggregate((current, next) => current + ", " + next));
          OrgProcessingTasks.Where(o => !orgs.Contains(o.Value.Item1))
            .Select(d => d.Key)
            .ForEach(t => OrgProcessingTasks.Remove(t));
        }
      }
    }

    public static int GetUsedThreads()
    {
      int i, j, k;
      ThreadPool.GetAvailableThreads(out i, out k);
      ThreadPool.GetMaxThreads(out j, out k);
      return j-i;
    }
  }
>>>>>>> webapi_support:TagFileHarvester/OrgsHandler.cs
}