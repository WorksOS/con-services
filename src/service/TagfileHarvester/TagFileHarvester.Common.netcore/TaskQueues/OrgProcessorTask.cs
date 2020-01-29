using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CCSS.TagFileSplitter.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TagFileHarvester.Common.netcore.TaskQueues;
using TagFileHarvester.Implementation;
using TagFileHarvester.Interfaces;
using TagFileHarvester.Models;

namespace TagFileHarvester.TaskQueues
{
  public class OrgProcessorTask
  {
    private readonly CancellationTokenSource cancellationToken;

    //each org should have it's own queue of threads processing files
    private readonly Organization org;
    private readonly Dictionary<Task, Tuple<Organization, CancellationTokenSource>> orgsTracker;
    private readonly ILogger log;

    public OrgProcessorTask(IUnityContainer container, Organization org, CancellationTokenSource cancellationToken,
      Dictionary<Task, Tuple<Organization, CancellationTokenSource>> orgsList)
    {
      Result = new OrgProcessingResult();
      Container = container;
      this.org = org;
      Result.Reset();
      this.cancellationToken = cancellationToken;
      log = container.Resolve<ILogger>();
      orgsTracker = orgsList;
    }

    private IUnityContainer Container { get; }

    public OrgProcessingResult Result { get; }


    public OrgProcessingResult ProcessOrg(bool SingleCycle = false, Action<OrgProcessorTask> onOrgProcessed = null)
    {
      var fileTasks = new List<Task>();
      var filenames = new List<FileRepository.TagFile>();
      var failuredFiles = new List<FileRepository.TagFile>();
      var processedFiles = new List<FileRepository.TagFile>();

      var repositoryError = false;

      if (cancellationToken.IsCancellationRequested) return Result;

      if (OrgsHandler.newrelic == "true")
      {
        var eventAttributes = new Dictionary<string, object>
        {
          { "Org", org.shortName }
        };

        NewRelic.Api.Agent.NewRelic.RecordCustomEvent("TagFileHarvester_Process", eventAttributes);
      }


      //Resolve all dependencies here
      var fileRepository = Container.Resolve<IFileRepository>();
      var harvesterTasks = Container.Resolve<IHarvesterTasks>();

      var filetasksCancel = new CancellationTokenSource();
      var sleepFlag = false;

      try
      {
        //Clear previous results
        filenames.Clear();
        fileTasks.Clear();
        Result.Reset();
        failuredFiles.Clear();

        //We need to get list of folder recursively here
        try
        {
          var folders = fileRepository.ListFolders(org, out _).ToList();

          //this could be a long time to get files, so check if we are requested to stop
          if (cancellationToken.IsCancellationRequested) return Result;

          log.LogDebug("Found {0} folders for org {1}", folders.Count, org.shortName);
          var files = folders.SelectMany(f => fileRepository.ListFiles(org, f)).ToList();

          log.LogDebug("Found {0} files for org {1}", files.Count, org.shortName);

          //this could be a long time to get files, so check if we are requested to stop
          if (cancellationToken.IsCancellationRequested) return Result;

          files.OrderBy(t => t.createdUTC).Take(OrgsHandler.NumberOfFilesInPackage).ForEach(filenames.Add);

          //this could be a long time to get files, so check if we are requested to stop
          if (cancellationToken.IsCancellationRequested) return Result;

          log.LogDebug("Got {0} files for org {1}", filenames.Count, org.shortName);
        }
        catch (Exception ex)
        {
          repositoryError = true;
          log.LogWarning("Repository error occured for org {0}, could not get files or folders from TCC Exception: {1}",
            org.shortName, ex.Message);
        }

        var fileListLock = new object();

        //If we are good with the repository proceed with files
        if (!repositoryError && filenames.Count > 0)
        {
          //this could be a long time to get files, so check if we are requested to stop
          if (cancellationToken.IsCancellationRequested) return Result;

          //foreach filenames here - build chain of tasks and track execution
          filenames.ForEach(f => fileTasks.Add(harvesterTasks
            .StartNewLimitedConcurrency(
              () =>
              {
                var localresult = new WebApiTagFileProcessTask(Container,
                    filetasksCancel.Token)
                  .ProcessTagfile(f.fullName, org);
                lock (fileListLock)
                {
                  Result.AggregateOrgResult(localresult);
                  if (localresult == null)
                  {
                    repositoryError = true;
                    failuredFiles.Add(f);
                  }
                  else
                  {
                    processedFiles.Add(f);
                  }

                  // raise flag that we have at least one failed file
                  log.LogDebug(
                    "TagFile {0} processed with result {1}",
                    f.fullName, JsonConvert.SerializeObject(localresult));
                  return localresult;
                }
              }, filetasksCancel.Token)));


          //And schedule processing of found tagfiles
          if (!Task.WaitAll(fileTasks.ToArray(), (int) OrgsHandler.TagFileSubmitterTasksTimeout.TotalMilliseconds,
            cancellationToken.Token))
          {
            log.LogWarning("Filetasks ran out of time for completion for org {0}", org.shortName);
            repositoryError = true;
          }

          //cleanup tasks
          fileTasks.Clear();
        }

        log.LogInformation("Org {0} cycle completed. Submitted files {1} Refused files {2} Errors {3}", org.shortName,
          Result.ProcessedFiles, Result.RefusedFiles, Result.ErroneousFiles);

        //Run callback action
        try
        {
          if (onOrgProcessed != null)
            onOrgProcessed.Invoke(this);
        }
        catch (Exception ex)
        {
          log.LogError("Failed while calling back", ex);
        }

        if (SingleCycle) return Result;
      }
      catch (Exception ex)
      {
        log.LogError("Exception while processing org {0} occured {1}", org.shortName, ex.Message);
        return Result;
      }

      if (!cancellationToken.IsCancellationRequested)
      {
        log.LogInformation("Rescheduling processing of org {0}", org.shortName);
        //delete current task from org tacker and add a new one
        //sleep only if there is nothing to process. Otherwise process everything we could have
        var delayExecution = false;
        log.LogDebug("Trying to Sleep for the org {0} {1} {2}", org.shortName, Result.ProcessedFiles,
          Result.RefusedFiles);
        if (!fileRepository.IsAnythingInCahe(org) && Result.ProcessedFiles == 0 && Result.RefusedFiles == 0 ||
            sleepFlag)
        {
          log.LogDebug("Sleeping for the org {0}", org.shortName);
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
                  .LogInformation("Tasks status is {0} in Queue1 and {1} in Queue2 on {2} Threads",
                    harvesterTasks.Status().Item1,
                    harvesterTasks.Status().Item2, OrgsHandler.GetUsedThreads()));
            }, cancellationToken.Token, delayExecution),
            new Tuple<Organization, CancellationTokenSource>(org, cancellationToken));
        }
      }

      return Result;
    }
  }

  public class OrgProcessingResult
  {
    private readonly object _resultLocker = new object();
    public int ErroneousFiles;
    public int ProcessedFiles;
    public int RefusedFiles;

    public void AggregateOrgResult(TagFileSplitterAutoResponse result)
    {
      lock (_resultLocker)
      {
        if (result == null)
        {
          ErroneousFiles++;
          return;
        }

        if (result.Code == 0)
        {
          ProcessedFiles++;
          return;
        }

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
