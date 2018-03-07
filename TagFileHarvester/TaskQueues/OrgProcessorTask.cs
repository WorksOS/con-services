using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using TagFileHarvester.Implementation;
using TagFileHarvester.Interfaces;
using TagFileHarvester.Models;
using TAGProcServiceDecls;

namespace TagFileHarvester.TaskQueues
{
  public class OrgProcessorTask
  {
    private IUnityContainer Container { get; set; }
    private ILog log;
    private readonly Dictionary<Task, Tuple<Organization, CancellationTokenSource>> orgsTracker;

    public OrgProcessingResult Result
    {
      get { return this.result; }
    }

    //each org should have it's own queue of threads processing files
    private readonly Organization org;
    private readonly CancellationTokenSource cancellationToken;
    private readonly OrgProcessingResult result;

    public OrgProcessorTask(IUnityContainer container, Organization org, CancellationTokenSource cancellationToken, Dictionary<Task, Tuple<Organization, CancellationTokenSource>> orgsList)
    {
      result = new OrgProcessingResult();
      Container = container;
      this.org = org;
      result.Reset();
      this.cancellationToken = cancellationToken;
      log = container.Resolve<ILog>();
      orgsTracker = orgsList;
    }


    public OrgProcessingResult ProcessOrg(bool SingleCycle = false, Action<OrgProcessorTask> onOrgProcessed = null)
    {
      var fileTasks = new List<Task>();
      var filenames = new List<FileRepository.TagFile>();
      var failuredFiles = new List<FileRepository.TagFile>();
      var processedFiles = new List<FileRepository.TagFile>();

      bool repositoryError = false;
      
      if (cancellationToken.IsCancellationRequested) return result;

      //Resolve all dependencies here
      var fileRepository = Container.Resolve<IFileRepository>();
      var harvesterTasks = Container.Resolve<IHarvesterTasks>();

      var filetasksCancel = new CancellationTokenSource();
      bool sleepFlag = false;
      int totalfiles = 0;
      
      try
      {
        //Clear previous results
        filenames.Clear();
        fileTasks.Clear();
        result.Reset();
        failuredFiles.Clear();

        //We need to get list of folder recursevly here
        try
        {
          bool fromCache = false;
          var folders = fileRepository.ListFolders(org, out fromCache).ToList();

          //this could be a long time to get files, so check if we are requested to stop
          if (cancellationToken.IsCancellationRequested) return result;

          log.DebugFormat("Found {0} folders for org {1}", folders.Count(), org.shortName);
          var files = folders.SelectMany(f => fileRepository.ListFiles(org, f)).ToList();

          log.DebugFormat("Found {0} files for org {1}", files.Count(), org.shortName);

          //this could be a long time to get files, so check if we are requested to stop
          if (cancellationToken.IsCancellationRequested) return result;

          totalfiles = files.Count;

          files.OrderBy(t => t.createdUTC).Take(OrgsHandler.NumberOfFilesInPackage).ForEach(filenames.Add);

          //this could be a long time to get files, so check if we are requested to stop
          if (cancellationToken.IsCancellationRequested) return result;            

          log.DebugFormat("Got {0} files for org {1}", filenames.Count, org.shortName);
        }
        catch (Exception ex)
        {
          repositoryError = true;
          log.WarnFormat("Repository error occured for org {0}, could not get files or folders from TCC Exception: {1}", org.shortName, ex.Message);
        }

        var filelistlock = new object();

        //If we are good with the repository proceed with files
        if (!repositoryError && filenames.Count > 0)
        {
          //this could be a long time to get files, so check if we are requested to stop
          if (cancellationToken.IsCancellationRequested) return result;

          //foreach filenames here - build chain of tasks and track execution
          filenames.ForEach(f => fileTasks.Add(harvesterTasks
            .StartNewLimitedConcurrency(
              () =>
              {
                var localresult = new WebApiTagFileProcessTask(Container,
                  filetasksCancel.Token)
                  .ProcessTagfile(f.fullName, org);
                lock (filelistlock)
                {
                  result.AggregateOrgResult(localresult);
                  if (localresult == null)
                  {
                    repositoryError = true;
                    failuredFiles.Add(f);
                  }
                  else
                    processedFiles.Add(f);
                  // raise flag that we have at least one failured file
                  log.DebugFormat(
                    "TagFile {0} processed with result {1}",
                    f.fullName, localresult);
                  return localresult;
                }
              }, filetasksCancel.Token)));


            //And schedule processing of found tagfiles
            if (!Task.WaitAll(fileTasks.ToArray(), (int)OrgsHandler.TagFileSubmitterTasksTimeout.TotalMilliseconds, cancellationToken.Token))
            {
              log.WarnFormat("Filetasks ran out of time for completion for org {0}", org.shortName);
              repositoryError = true;
            }

            //cleanup tasks
            fileTasks.Clear();
          }

          log.InfoFormat("Org {0} cycle completed. Submitted files {1} Refused files {2} Errors {3}", org.shortName,
              result.ProcessedFiles, result.RefusedFiles, result.ErroneousFiles);

          //Run callback action
          try
          {
            if (onOrgProcessed != null)
              onOrgProcessed.Invoke(this);
          }
          catch (Exception ex)
          {
            log.Error("Failed while calling back",ex);
          }

          if (SingleCycle) return result;
        }
        catch (Exception ex)
        {
          log.ErrorFormat("Exception while processing org {0} occured {1}", org.shortName, ex.Message);
          return result;
        }

      if (!cancellationToken.IsCancellationRequested)
      {
        log.InfoFormat("Rescheduling processing of org {0}", org.shortName);
        //delete current task from org tacker and add a new one
        //sleep only if there is nothing to process. Otherwise process everything we could have
        bool delayExecution = false;
        log.DebugFormat("Trying to Sleep for the org {0} {1} {2}", org.shortName, result.ProcessedFiles, result.RefusedFiles);
        if ((!fileRepository.IsAnythingInCahe(org) && (result.ProcessedFiles == 0) && (result.RefusedFiles == 0)) || sleepFlag)
        {
          log.DebugFormat("Sleeping for the org {0}", org.shortName);
          /*Task.Delay(OrgsHandler.OrgProcessingDelay, cancellationToken.Token).Wait();*/
          delayExecution = true;
        }

        lock (OrgsHandler.OrgListLocker)
        {
          orgsTracker.Remove(orgsTracker.First(t => t.Value.Item1.shortName == org.shortName).Key);

          orgsTracker.Add(harvesterTasks.StartNewLimitedConcurrency2(() =>
          {
            ProcessOrg(false,
            t => log
              .InfoFormat("Tasks status is {0} in Queue1 and {1} in Queue2 on {2} Threads",
                harvesterTasks.Status().Item1,
                harvesterTasks.Status().Item2, OrgsHandler.GetUsedThreads()));
          }, cancellationToken.Token,delayExecution), new Tuple<Organization, CancellationTokenSource>(org, cancellationToken));
        }

      }
      return result;
    }

  }

  public class OrgProcessingResult
  {
    public int RefusedFiles;
    public int ProcessedFiles;
    public int ErroneousFiles;
    private readonly object _resultLocker = new object();

    public void AggregateOrgResult(BaseDataResult result)
    {
      lock (_resultLocker)
      {
        if (result == null) { ErroneousFiles++; return; }
        if (result.Code == 0 ) { ProcessedFiles++; return; }
        RefusedFiles++;
      }
    }

    public void Reset()
    {
      RefusedFiles = 0;
      ProcessedFiles = 0;
      ErroneousFiles = 0;
    }
  }
}
