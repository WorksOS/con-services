using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.FlowJSHandler;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.Productivity3D.Scheduler.Common.Repository;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Scheduler.Common.Controller
{
  public class ImportedFileSynchronizer : ImportedFileSynchronizerBase
  {

    protected ILogger _log;

    public ImportedFileSynchronizer(IConfigurationStore configStore, ILoggerFactory logger, IRaptorProxy raptorProxy, 
      ITPaasProxy tPaasProxy, IImportedFileProxy impFileProxy, IFileRepository fileRepo) 
      : base(configStore, logger, raptorProxy, tPaasProxy, impFileProxy, fileRepo)
    {
      _log = logger.CreateLogger<ImportedFileSynchronizer>();
    }

    /// <summary>
    /// Read from NGen Project.ImportedFile table
    ///   May initially be limited to SurveyedSurface type
    /// </summary>
    public async Task SyncTables()
    {
      ImportedFileRepoNhOp<ImportedFileNhOp> repoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, Logger);
      ImportedFileRepoProject<ImportedFileProject> repoProject =
        new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, Logger);

      var fileListProject = repoProject.Read();
      var fileListNhOp = repoNhOp.Read();

      await SyncOldTableToNewTable(fileListNhOp, fileListProject, repoNhOp, repoProject);
      //Only Files need to be sync'd in reverse.
      var ssListProject = fileListProject.Where(x => x.ImportedFileType == ImportedFileType.SurveyedSurface).ToList();
      await SyncNewTableToOldTable(ssListProject, repoNhOp, repoProject);
    }

    /// <summary>
    /// Synchronize nhOp to Project
    /// </summary>
    private async Task SyncOldTableToNewTable(List<ImportedFileNhOp> fileListNhOp, List<ImportedFileProject> fileListProject, 
      ImportedFileRepoNhOp<ImportedFileNhOp> repoNhOp, ImportedFileRepoProject<ImportedFileProject> repoProject)
    {
      var startUtc = DateTime.UtcNow;

      // cannot modify collectionB from within collectionA
      // put it in here and remove later
      var fileListProjectToRemove = new List<ImportedFileProject>();

      // row in  NH_OP and in project, nothing has changed (a)
      // row in  NH_OP and in project, logically deleted in project. physically delete in NH_OP (b)
      // row in  NH_OP and in project, a date has changed, update whichever is older (c)
      // row in  NH_OP NOT in project, create it in project (d)
      for (int i = fileListNhOp.Count - 1; i >= 0; i--)
      {
        startUtc = DateTime.UtcNow;
        var gotMatchingProject =
          fileListProject.FirstOrDefault(o => o.LegacyImportedFileId == fileListNhOp[i].LegacyImportedFileId);

        if (gotMatchingProject == null)
        {
          // (d)
          await CreateFileInNewTable(repoProject, startUtc, fileListNhOp[i]);
        }
        else
        {
          if (gotMatchingProject.IsDeleted)
          {
            // (b)
            DeleteFileInOldTable(repoNhOp, gotMatchingProject, fileListNhOp[i], startUtc);
          }
          else
          {
            const string DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
            var projectCreatedUtcRounded = DateTime.Parse(gotMatchingProject.FileCreatedUtc.ToString(DATE_TIME_FORMAT));
            var projectUpdatedUtcRounded = DateTime.Parse(gotMatchingProject.FileUpdatedUtc.ToString(DATE_TIME_FORMAT));
            var nhOpCreatedUtcRounded = DateTime.Parse(fileListNhOp[i].FileCreatedUtc.ToString(DATE_TIME_FORMAT));
            var nhOpUpdatedUtcRounded = DateTime.Parse(fileListNhOp[i].FileUpdatedUtc.ToString(DATE_TIME_FORMAT));

            if (projectCreatedUtcRounded != nhOpCreatedUtcRounded 
             || projectUpdatedUtcRounded != nhOpUpdatedUtcRounded)
            {
              // (c)
              if (gotMatchingProject.FileCreatedUtc > fileListNhOp[i].FileCreatedUtc
               || gotMatchingProject.FileUpdatedUtc > fileListNhOp[i].FileUpdatedUtc)
              {
                // project is more recent, update nh_op
                UpdateFileInOldTable(repoNhOp, gotMatchingProject, fileListNhOp[i], startUtc);
              }
              else
              {
                // nh_op is more recent, update project
                await UpdateFileInNewTable(repoProject, gotMatchingProject, fileListNhOp[i], startUtc);
              }
            }
          }
          // else (a) no change
          fileListProjectToRemove.Add(gotMatchingProject);
        }
      }


      fileListProject.RemoveAll(
        x => fileListProjectToRemove.Exists(y => y.LegacyImportedFileId == x.LegacyImportedFileId));
    }

    /// <summary>
    /// Synchronize Project to nhOp
    /// </summary>
    private async Task SyncNewTableToOldTable(List<ImportedFileProject> fileListProject,
      ImportedFileRepoNhOp<ImportedFileNhOp> repoNhOp, ImportedFileRepoProject<ImportedFileProject> repoProject)
    {
      var startUtc = DateTime.UtcNow;

      // row in Project but doesn't exist in nhOp. LegacyImportedFileId determines if previously from nhOp.
      //        if project has LegacyImportedFileId, and not already deleted, then delete in Project (m)
      //        if project has no LegacyImportedFileId, and not already deleted, then create in nhOp (n)
      //        if project has no LegacyImportedFileId, not deleted, but has no valid legacy customer or projectID then can't be added to nhOp (o)
      //        deleted project in project but no link to nhOp since in the list here so user must have created & deleted in project only , just ignore it. (p)
      for (int i=fileListProject.Count-1; i>=0; i--)
      {
        startUtc = DateTime.UtcNow;

        if (fileListProject[i].IsDeleted)
        {
          // (p) ignore
        }
        else if (fileListProject[i].LegacyImportedFileId != null && fileListProject[i].LegacyImportedFileId > 0)
        {
          // (m)
          await DeleteFileInNewTable(repoProject, startUtc, fileListProject[i]);
        }
        else if (fileListProject[i].LegacyCustomerId == 0 || fileListProject[i].LegacyProjectId >= 1000000)
        {
          // (o)
          NotifyNewRelic(fileListProject[i], startUtc, "File in Project which has no legacyCustomerId so cannot be synced to NhOp.", "Warning");
        }
        else
        {
          // (n)
          CreateFileInOldTable(repoProject, repoNhOp, startUtc, fileListProject[i]);
        }
      }

    }
    /// <summary>
    /// Create the file in nhOp and update Project with the new legacy id.
    /// </summary>
    private void CreateFileInOldTable(ImportedFileRepoProject<ImportedFileProject> repoProject, ImportedFileRepoNhOp<ImportedFileNhOp> repoNhOp, DateTime startUtc, ImportedFileProject ifp)
    {
      var nhOpEvent = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(ifp);
      nhOpEvent.Name = ImportedFileUtils.IncludeSurveyedUtcInName(nhOpEvent.Name, nhOpEvent.SurveyedUtc.Value);
      var legacyImportedFileId = repoNhOp.Create(nhOpEvent);
      ifp.LegacyImportedFileId = legacyImportedFileId;
      repoProject.Update(ifp);
  
      NotifyNewRelic(ifp, startUtc, "File created in project, now created in NhOp.");
    }
    

    /// <summary>
    /// Create the file in Project
    /// </summary>
    private async Task CreateFileInNewTable(ImportedFileRepoProject<ImportedFileProject> repoProject, DateTime startUtc, ImportedFileNhOp ifo)
    {
      var projectEvent = AutoMapperUtility.Automapper.Map<ImportedFileProject>(ifo);
      if (projectEvent.ImportedFileType == ImportedFileType.SurveyedSurface)
      {
        projectEvent.Name = ImportedFileUtils.RemoveSurveyedUtcFromName(projectEvent.Name);
      }
      projectEvent.ImportedFileUid = Guid.NewGuid().ToString();
      // for L&S if it has come from CG then use legacyIds
      projectEvent.FileDescriptor = JsonConvert.SerializeObject(FileDescriptor.CreateFileDescriptor(FileSpaceId,
        projectEvent.LegacyCustomerId.ToString(), projectEvent.LegacyProjectId.ToString(), projectEvent.Name));
      if (projectEvent.ImportedBy == null) projectEvent.ImportedBy = string.Empty;

      if (projectEvent.ImportedFileType == ImportedFileType.SurveyedSurface)
      {
        repoProject.Create(projectEvent);

        // Notify 3dpm of File created via Legacy
        if (projectEvent.LegacyImportedFileId != null) // Note that LegacyImportedFileId will always be !null 
          await NotifyRaptorImportedFileChange(projectEvent.CustomerUid, Guid.Parse(projectEvent.ProjectUid),
              Guid.Parse(projectEvent.ImportedFileUid))
            .ConfigureAwait(false);
      }
      else
      {
        await ImpFileProxy.CreateImportedFile(new FlowFile{flowFilename = projectEvent.Name, path = "TODO"}, new Guid(projectEvent.ProjectUid), projectEvent.ImportedFileType,
          projectEvent.FileCreatedUtc, projectEvent.FileUpdatedUtc, projectEvent.DxfUnitsType, projectEvent.SurveyedUtc,
          await GetCustomHeaders(projectEvent.CustomerUid)).ConfigureAwait(false);
      }

      NotifyNewRelic(projectEvent, startUtc, "File created in NhOp, now created in Project.");
    }

    /// <summary>
    /// Delete the file in nhOp
    /// </summary>
    private void DeleteFileInOldTable(ImportedFileRepoNhOp<ImportedFileNhOp> repoNhOp, ImportedFileProject gotMatchingProject, ImportedFileNhOp ifo, DateTime startUtc)
    {
      repoNhOp.Delete(ifo);
      Log.LogTrace(
        $"SyncTables: nhOp.IF is in nh_Op but was deleted in project. Deleted from NhOp: {JsonConvert.SerializeObject(ifo)}");
      NotifyNewRelic(gotMatchingProject, startUtc, "File deleted in Project, now deleted in NhOp.");
    }

    /// <summary>
    /// Delete the file in Project
    /// </summary>
    private async Task DeleteFileInNewTable(ImportedFileRepoProject<ImportedFileProject> repoProject, DateTime startUtc, ImportedFileProject ifp)
    {
      repoProject.Delete(ifp);

      // Notify 3dpm of File deleted via Legacy
      if (ifp.LegacyImportedFileId != null) // Note that LegacyImportedFileId will always be !null 
        await NotifyRaptorImportedFileChange(ifp.CustomerUid, Guid.Parse(ifp.ProjectUid), Guid.Parse(ifp.ImportedFileUid))
          .ConfigureAwait(false);

      NotifyNewRelic(ifp, startUtc, "File deleted in NhOp, now deleted from Project.");
    }

    /// <summary>
    /// Update the file in nhOp
    /// </summary>
    private void UpdateFileInOldTable(ImportedFileRepoNhOp<ImportedFileNhOp> repoNhOp, ImportedFileProject gotMatchingProject, ImportedFileNhOp ifo, DateTime startUtc)
    {
      ifo.FileCreatedUtc = gotMatchingProject.FileCreatedUtc;
      ifo.FileUpdatedUtc = gotMatchingProject.FileUpdatedUtc;
      ifo.LastActionedUtc = DateTime.UtcNow;
      repoNhOp.Update(ifo);
      NotifyNewRelic(gotMatchingProject, startUtc, "File updated in project, now updated in NhOp.");
    }

    /// <summary>
    /// Update the file in Project
    /// </summary>
    private async Task UpdateFileInNewTable(ImportedFileRepoProject<ImportedFileProject> repoProject, ImportedFileProject gotMatchingProject, ImportedFileNhOp ifo, DateTime startUtc)
    {
      gotMatchingProject.FileCreatedUtc = ifo.FileCreatedUtc;
      gotMatchingProject.FileUpdatedUtc = ifo.FileUpdatedUtc;
      gotMatchingProject.LastActionedUtc = DateTime.UtcNow;
      repoProject.Update(gotMatchingProject);

      // Notify 3dpm of File updated via Legacy
      if (gotMatchingProject.LegacyImportedFileId != null
      ) // Note that LegacyImportedFileId will always be !null 
        await NotifyRaptorImportedFileChange(gotMatchingProject.CustomerUid, Guid.Parse(gotMatchingProject.ProjectUid),
            Guid.Parse(gotMatchingProject.ImportedFileUid))
          .ConfigureAwait(false);

      NotifyNewRelic(gotMatchingProject, startUtc, "File updated in NhOp, now updated in Project.");
    }

    /// <summary>
    /// Information or warning for NewRelic
    /// </summary>
    private void NotifyNewRelic(ImportedFileProject ifp, DateTime startUtc, string message, string errorLevel= "Information")
    {
      var newRelicAttributes = new Dictionary<string, object>
      {
        { "message", message },
        { "projectUid", ifp.ProjectUid },
        { "importedFileUid", ifp.ImportedFileUid },
        { "fileDescriptor", ifp.FileDescriptor },
        { "legacyImportedFileId", ifp.LegacyImportedFileId }
      };
      NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", errorLevel, startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, _log, newRelicAttributes);
    }
  }
}
