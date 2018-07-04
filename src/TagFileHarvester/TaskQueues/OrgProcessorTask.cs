<<<<<<< HEAD:src/TagFileHarvester/TaskQueues/OrgProcessorTask.cs
﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using TAGProcServiceDecls;
using VSS.Productivity3D.TagFileHarvester.Implementation;
using VSS.Productivity3D.TagFileHarvester.Interfaces;
using VSS.Productivity3D.TagFileHarvester.Models;

namespace VSS.Productivity3D.TagFileHarvester.TaskQueues
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
      DateTime bookmark=DateTime.MinValue;

      if (cancellationToken.IsCancellationRequested) return result;

      //Resolve all dependencies here
      var bookmarkManager = Container.Resolve<IBookmarkManager>();
      var fileRepository = Container.Resolve<IFileRepository>();
      var harvesterTasks = Container.Resolve<IHarvesterTasks>();

      var filetasksCancel = new CancellationTokenSource();
      
      try
        {
          //Clear previous results
          filenames.Clear();
          fileTasks.Clear();
          result.Reset();
          failuredFiles.Clear();

          if (bookmarkManager.GetBookmark(org).OrgIsDisabled)
          {
            log.InfoFormat("Org {0} is disabled.", org.shortName);
            return result;
          }

          bookmarkManager
              .SetBookmarkInProgress(org, true)
              .SetBookmarkLastCycleStartDateTime(org, DateTime.UtcNow)
              .WriteBookmarksAsync();

          bookmark =bookmarkManager
                      .GetBookmark(org).BookmarkUTC;
          log.DebugFormat("Got bookmark {0} for org {1}", bookmark, org.shortName);

          var scanTime = bookmarkManager.GetBookmark(org).LastTCCScanDateTime;

          //Make sure that all files are processed
         if (bookmark != DateTime.MinValue)
            bookmark = bookmark.Subtract(OrgsHandler.BookmarkTolerance);

          //if scanning occured outside period of tolerance - use scanning time as a bookmark
          if (scanTime < bookmark && OrgsHandler.EnableHardScanningLogic)
          {
            log.Warn("Hard scanning logic enabled.");
            bookmark = scanTime;
          }

          //We need to get list of folder recursevly here
          try
          {
          bool fromCache = false;
          var folders = fileRepository
              .ListFolders(org, bookmark, out fromCache).ToList();


          //this could be a long time to get files, so check if we are requested to stop
            if (cancellationToken.IsCancellationRequested) return result;

              log.DebugFormat("Found {0} folders for org {1}", folders.Count(), org.shortName);
              var files = folders.SelectMany(f =>
                  fileRepository
                      .ListFiles(org, f, bookmark)).ToList();

              log.DebugFormat("Found {0} files for org {1}", files.Count(), org.shortName);

              if (!fromCache)
                bookmarkManager.SetBookmarkLastTCCScanTimeUTC(org, DateTime.UtcNow).WriteBookmarksAsync();

              //this could be a long time to get files, so check if we are requested to stop
              if (cancellationToken.IsCancellationRequested) return result;       

              files.OrderBy(t => t.createdUTC).Take(OrgsHandler.NumberOfFilesInPackage)
                  .ForEach(filenames.Add);

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
                  var localresult = new TagFileProcessTask(Container,
                    filetasksCancel.Token)
                    .ProcessTagfile(f.fullName, org);
                  lock (filelistlock)
                  {
                    result.AggregateOrgResult(localresult);
                    if (localresult == null ||
                        localresult ==
                        TTAGProcServerProcessResult
                          .tpsprOnSubmissionBaseConnectionFailure ||
                        localresult ==
                        TTAGProcServerProcessResult
                          .tpsprOnSubmissionResultConnectionFailure)
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
    
            //Now we need to update bookmark
            if (repositoryError) //Don't update bookmark
            {
              filetasksCancel.Cancel();
              if (failuredFiles.Count > 0)
              {
                log.DebugFormat(
                    "Found failured files. First failured file has timestamp {0} and last file in chunk timestamp was {1}",
                    failuredFiles.Min(f => f.createdUTC), filenames.Max(f => f.createdUTC));
                log.WarnFormat("Submit file error occured for org {0}, rolling back bookmark to {1}",
                    org.shortName, failuredFiles.Min(f=>f.createdUTC));
                bookmarkManager
                    .SetBookmarkUTC(org, failuredFiles.Min(f => f.createdUTC).Subtract(OrgsHandler.BadFilesToleranceRollback))
                    .WriteBookmarksAsync();
              }
              else
              {
                //Something very bad happened here, ignoring everything and trying next time from the scratch
                log.WarnFormat("Get file list error occured for org {0}, rolling back bookmark to initial {1}",
                    org.shortName,
                    bookmark);
                bookmarkManager
                    .SetBookmarkUTC(org, bookmark)
                    .WriteBookmarksAsync();
              }
              //if we have failured files in the list - tell repository that cache is dirty and carry on with getting the list of files once again. 
              //This will force all failed files to be listed from TCC and reprocessed. Don't shift bookmark again
              fileRepository.CleanCache(org);
            }
            else
            {
              //Update bookmark only if we are sure that we have retreived list of files
              //Set bookmark 10 min before the last file only if we have succeseeded with retreiving the files
              log.DebugFormat("Setting bookmark to {0} for org {1}", processedFiles.Max(f => f.createdUTC),
                  org.shortName);
              bookmarkManager
                  .SetBookmarkUTC(org, processedFiles.Max(f => f.createdUTC))
                  .WriteBookmarksAsync();
              fileRepository.RemoveObsoleteFilesFromCache(org, processedFiles);
            }
          }

          bookmarkManager
                .SetBookmarkLastCycleStopDateTime(org, DateTime.UtcNow)
                .SetBookmarkInProgress(org, false)
                .SetBookmarkLastFilesError(org, result.ErroneousFiles)
                .SetBookmarkLastFilesRefused(org, result.RefusedFiles)
                .SetBookmarkLastFilesProcessed(org, result.ProcessedFiles)
                .IncBookmarkCyclesCompleted(org)
                .WriteBookmarksAsync();

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
          //Rollback bookmark
          log.ErrorFormat("Exception while processing org {0} occured {1}, rolling back bookmark to {2}", org.shortName, ex.Message, bookmark);
          log.Error("Exception is", ex);
          bookmarkManager
                .SetBookmarkInProgress(org, false)
                .SetBookmarkUTC(org, bookmark)
                .WriteBookmarksAsync();
          return result;
        }

      //reschedule here my execution if cancellation is not requested
      bookmarkManager
          .SetBookmarkInProgress(org, false)
          .WriteBookmarksAsync();

      if (!cancellationToken.IsCancellationRequested)
      {
        log.InfoFormat("Rescheduling processing of org {0}", org.shortName);
        //delete current task from org tacker and add a new one

        lock (OrgsHandler.OrgListLocker)
        {
          orgsTracker.Remove(orgsTracker.First(t => t.Value.Item1.shortName == org.shortName).Key);

          orgsTracker.Add(harvesterTasks.StartNewLimitedConcurrency2(() =>
          {
            //sleep only if there is nothing to process. Otherwise process everything we could have
            if (!fileRepository.IsAnythingInCahe(org))
            {
              log.DebugFormat("Sleeping for the org {0}", org.shortName);
              Task.Delay(OrgsHandler.OrgProcessingDelay, cancellationToken.Token).Wait(cancellationToken.Token);
            }

            ProcessOrg(false,
              t => log
                .InfoFormat("Tasks status is {0} in Queue1 and {1} in Queue2 on {2} Threads",
                  harvesterTasks.Status().Item1,
                  harvesterTasks.Status().Item2, OrgsHandler.GetUsedThreads()));
          }, cancellationToken.Token), new Tuple<Organization, CancellationTokenSource>(org, cancellationToken));
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

    public void AggregateOrgResult(TTAGProcServerProcessResult? result)
    {
      lock (_resultLocker)
      {
        if (result == null) { ErroneousFiles++; return; }
        if (result == TTAGProcServerProcessResult.tpsprOK) { ProcessedFiles++; return; }
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
=======
﻿using System;
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
>>>>>>> webapi_support:TagFileHarvester/TaskQueues/OrgProcessorTask.cs
