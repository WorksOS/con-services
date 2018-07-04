<<<<<<< HEAD:src/TagFileHarvester/TaskQueues/TagFileProcessTask.cs
﻿using System;
using System.IO;
using System.Threading;
using log4net;
using Microsoft.Practices.Unity;
using TAGProcServiceDecls;
using VSS.Productivity3D.TagFileHarvester.Interfaces;
using VSS.Productivity3D.TagFileHarvester.Models;


namespace VSS.Productivity3D.TagFileHarvester.TaskQueues
{
  public class TagFileProcessTask
  {
    private IUnityContainer unityContainer;
    private ILog log;
    private CancellationToken token;

    public TagFileProcessTask(IUnityContainer unityContainer, CancellationToken token)
    {
      this.unityContainer = unityContainer;
      log = unityContainer.Resolve<ILog>();
      this.token = token;
    }

    public TAGProcServiceDecls.TTAGProcServerProcessResult? ProcessTagfile(string tagFilename, Organization org)
    {
      if (token.IsCancellationRequested)
        return null;
      try
      {
        log.DebugFormat("Processing file {0} for org {1}", tagFilename, org.shortName);
        Stream file = unityContainer.Resolve<IFileRepository>().GetFile(org, tagFilename);
        if (file == null) return null;
        if (token.IsCancellationRequested)
          return null;
        log.DebugFormat("Submittting file {0} for org {1}", tagFilename, org.shortName);
        TTAGProcServerProcessResult result =
            unityContainer.Resolve<ITAGProcessorClient>().SubmitTAGFileToTAGFileProcessor(org.orgId, Path.GetFileName(tagFilename), file);
        if (token.IsCancellationRequested)
          return null;
        if (OrgsHandler.TCCArchiveFiles && result==TTAGProcServerProcessResult.tpsprOK)
        {
          log.DebugFormat("Archiving file {0} for org {1}", tagFilename, org.shortName);
          if (!unityContainer.Resolve<IFileRepository>()
              .MoveFile(org, tagFilename,
                  tagFilename.Remove(tagFilename.IndexOf(OrgsHandler.tccSynchMachineFolder), OrgsHandler.tccSynchMachineFolder.Length+1).Replace(OrgsHandler.TCCSynchProductionDataFolder,
                      OrgsHandler.TCCSynchProductionDataArchivedFolder)))
            return null;
        }
        return result;
      }
      catch (Exception ex)
      {
        log.ErrorFormat("Exception while processing file {0} occured {1}", tagFilename, ex.Message);
      }
      return null;
    }
  }
}
=======
﻿using System;
using System.IO;
using System.Threading;
using log4net;
using Microsoft.Practices.Unity;
using TagFileHarvester.Interfaces;
using TagFileHarvester.Models;
using TAGProcServiceDecls;


namespace TagFileHarvester.TaskQueues
{
  public class TagFileProcessTask
  {
    private IUnityContainer unityContainer;
    private ILog log;
    private CancellationToken token;

    public TagFileProcessTask(IUnityContainer unityContainer, CancellationToken token)
    {
      this.unityContainer = unityContainer;
      log = unityContainer.Resolve<ILog>();
      this.token = token;
    }

    public TAGProcServiceDecls.TTAGProcServerProcessResult? ProcessTagfile(string tagFilename, Organization org)
    {
      if (token.IsCancellationRequested)
        return null;
      try
      {
        log.DebugFormat("Processing file {0} for org {1}", tagFilename, org.shortName);
        Stream file = unityContainer.Resolve<IFileRepository>().GetFile(org, tagFilename);
        if (file == null) return null;
        if (token.IsCancellationRequested)
          return null;
        log.DebugFormat("Submittting file {0} for org {1}", tagFilename, org.shortName);
        TTAGProcServerProcessResult result =
            unityContainer.Resolve<ITAGProcessorClient>().SubmitTAGFileToTAGFileProcessor(org.orgId, Path.GetFileName(tagFilename), file);
        if (token.IsCancellationRequested)
          return null;

        var fileRepository = unityContainer.Resolve<IFileRepository>();

        if (fileRepository == null)
          return null;

        switch (result)
        {
          case TTAGProcServerProcessResult.tpsprOK:
          {
            log.DebugFormat("Archiving file {0} for org {1} to {2} folder", tagFilename, org.shortName, OrgsHandler.TCCSynchProductionDataArchivedFolder);

            if (!MoveFileTo(tagFilename, org, fileRepository, OrgsHandler.TCCSynchProductionDataArchivedFolder))
              return null;

            break;
          }
          case TTAGProcServerProcessResult.tpsprOnChooseDataModelCouldNotConvertDataModelBoundaryToGrid:
          case TTAGProcServerProcessResult.tpsprOnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary:
          case TTAGProcServerProcessResult.tpsprOnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices:
          {
            log.DebugFormat("Moving file {0} for org {1} to {2} folder", tagFilename, org.shortName, OrgsHandler.TCCSynchProjectBoundaryIssueFolder);

            if (!MoveFileTo(tagFilename, org, fileRepository, OrgsHandler.TCCSynchProjectBoundaryIssueFolder))
              return null;

            break;
          }
          case TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions:
          {
            log.DebugFormat("Moving file {0} for org {1} to {2} folder", tagFilename, org.shortName, OrgsHandler.TCCSynchSubscriptionIssueFolder);

            if (!MoveFileTo(tagFilename, org, fileRepository, OrgsHandler.TCCSynchSubscriptionIssueFolder))
              return null;

            break;
          }
          default:
          {
            log.DebugFormat("Moving file {0} for org {1} to {2} folder", tagFilename, org.shortName, OrgsHandler.TCCSynchOtherIssueFolder);

            if (!MoveFileTo(tagFilename, org, fileRepository, OrgsHandler.TCCSynchOtherIssueFolder))
              return null;

            break;
          }
        }

        return result;
      }
      catch (Exception ex)
      {
        log.ErrorFormat("Exception while processing file {0} occured {1}", tagFilename, ex.Message);
      }
      return null;
    }

    private bool MoveFileTo(string tagFilename, Organization org, IFileRepository fileRepository, string destFolder)
    {
      return fileRepository.MoveFile(org, tagFilename,
        tagFilename.Remove(tagFilename.IndexOf(OrgsHandler.tccSynchMachineFolder, StringComparison.Ordinal),
          OrgsHandler.tccSynchMachineFolder.Length + 1).Replace(OrgsHandler.TCCSynchProductionDataFolder,
          destFolder));
    }
  }
}
>>>>>>> webapi_support:TagFileHarvester/TaskQueues/TagFileProcessTask.cs
