using System;
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
