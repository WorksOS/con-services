using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
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
      await SyncNewTableToOldTable(fileListProject, repoNhOp, repoProject);
    }

    /// <summary>
    /// Synchronize nhOp to Project
    /// </summary>
    private async Task SyncOldTableToNewTable(List<ImportedFileNhOp> fileListNhOp,
      List<ImportedFileProject> fileListProject,
      ImportedFileRepoNhOp<ImportedFileNhOp> repoNhOp, ImportedFileRepoProject<ImportedFileProject> repoProject)
    {
      // cannot modify collectionB from within collectionA
      // put it in here and remove later
      var fileListProjectToRemove = new List<ImportedFileProject>();

      // row in  NH_OP and in project, nothing has changed (a)
      // row in  NH_OP and in project, logically deleted in project. physical delete in NH_OP (b)
      // row in  NH_OP and in project, a date has changed (c)
      // row in  NH_OP NOT in project, 
      //                 if project and customer exist, then create it (d)
      //                 else error (e)
      foreach (var ifo in fileListNhOp.ToList())
      {
        // (a)
        var startUtc = DateTime.UtcNow;
        var gotMatchingProject =
          fileListProject.FirstOrDefault(o => o.LegacyImportedFileId == ifo.LegacyImportedFileId);

        if (gotMatchingProject == null)
        {
          if (repoProject.ProjectAndCustomerExist(ifo.CustomerUid, ifo.ProjectUid))
          {
            // (d)
            await CreateFileInNewTable(repoProject, startUtc, ifo);
          }
          else
          {
            // (e)
            NotifyNewRelic(ifo, startUtc, $"{ifo.ImportedFileType} file created in NhOp, cannot create in Project as the project and/or customer relationship doesn't exist.", "Error");
          }
        }
        else
        {
          if (gotMatchingProject.IsDeleted)
          {
            // (b)
            if (gotMatchingProject.ImportedFileType == ImportedFileType.SurveyedSurface)
            {
              DeleteFileInOldTable(repoNhOp, gotMatchingProject, ifo, startUtc);
            }
          }
          else
          {
            var projectCreatedUtcRounded = RoundDateTimeToSeconds(gotMatchingProject.FileCreatedUtc);
            var projectUpdatedUtcRounded = RoundDateTimeToSeconds(gotMatchingProject.FileUpdatedUtc);
            var nhOpCreatedUtcRounded = RoundDateTimeToSeconds(ifo.FileCreatedUtc);
            var nhOpUpdatedUtcRounded = RoundDateTimeToSeconds(ifo.FileUpdatedUtc);

            if (projectCreatedUtcRounded != nhOpCreatedUtcRounded
                || projectUpdatedUtcRounded != nhOpUpdatedUtcRounded)
            {
              // (c)
              if (projectCreatedUtcRounded > nhOpCreatedUtcRounded
                  || projectUpdatedUtcRounded > nhOpUpdatedUtcRounded)
              {
                // project is more recent, update nh_op
                if (gotMatchingProject.ImportedFileType == ImportedFileType.SurveyedSurface)
                {
                  UpdateFileInOldTable(repoNhOp, gotMatchingProject, ifo, startUtc);
                }
              }
              else
              {
                // nh_op is more recent, update project
                await UpdateFileInNewTable(repoProject, gotMatchingProject, ifo, startUtc);
              }
            }
          }

          // (a) no change
          fileListProjectToRemove.Add(gotMatchingProject);
        }
      }
      fileListProject.RemoveAll(
        x => fileListProjectToRemove.Exists(y => y.LegacyImportedFileId == x.LegacyImportedFileId));
    }

    /// <summary>
    /// Round date time to nearest second
    /// </summary>
    private DateTime RoundDateTimeToSeconds(DateTime dateTime)
    {
      return DateTime.Parse(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    /// <summary>
    /// Synchronize Project to nhOp
    /// </summary>
    private async Task SyncNewTableToOldTable(List<ImportedFileProject> fileListProject,
      ImportedFileRepoNhOp<ImportedFileNhOp> repoNhOp, ImportedFileRepoProject<ImportedFileProject> repoProject)
    {
      // row in Project but doesn't exist in nhOp. LegacyImportedFileId determines if previously from nhOp.
      //        if project has LegacyImportedFileId ,  and not already deleted, then delete in Project (m)
      //        if project has no LegacyImportedFileId, and not already deleted, 
      //                                 if project and customer exist in nhOp then create in nhOp (n)
      //                                 else error (q) 
      //        if project has no LegacyImportedFileId, not deleted, but has no valid legacy customer or projectID then can't be added to NhOp (o)
      //        deleted project in project but no link to nhOp since in the list here so user must have created & deleted in project only, just ignore it. (p)
      foreach (var ifp in fileListProject)
      {
        var startUtc = DateTime.UtcNow;

        if (ifp.IsDeleted)
        {
          // (p) ignore
        }
        else
        {
          if (ifp.LegacyImportedFileId != null && ifp.LegacyImportedFileId > 0)
          {
            // (m)
            await DeleteFileInNewTable(repoProject, startUtc, ifp);
          }
          else
          {
            if (ifp.LegacyCustomerId == 0 || ifp.LegacyProjectId >= 1000000)
            {
              // (o)
              NotifyNewRelic(ifp, startUtc, $"{ifp.ImportedFileType} file in Project which has no legacyCustomerId so cannot be synced to NhOp.", "Warning");
            }
            else
            {
              if (repoNhOp.ProjectAndCustomerExist(ifp.CustomerUid, ifp.ProjectUid))
              {
                // (n)
                if (ifp.ImportedFileType == ImportedFileType.SurveyedSurface)
                {
                  CreateFileInOldTable(repoProject, repoNhOp, startUtc, ifp);
                }
              }
              else
              {
                // (q)
                NotifyNewRelic(ifp, startUtc, $"{ifp.ImportedFileType} file created in project, cannot create in NhOp as the project and/or customer relationship doesn't exist.", "Error");
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Create the file in nhOp and update Project with the new legacy id.
    /// </summary>
    private void CreateFileInOldTable(ImportedFileRepoProject<ImportedFileProject> repoProject,
      ImportedFileRepoNhOp<ImportedFileNhOp> repoNhOp, DateTime startUtc, ImportedFileProject ifp)
    {
      var nhOpEvent = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(ifp);
      nhOpEvent.Name =
        ImportedFileUtils.IncludeSurveyedUtcInName(nhOpEvent.Name, nhOpEvent.SurveyedUtc.Value);
      var legacyImportedFileId = repoNhOp.Create(nhOpEvent);
      ifp.LegacyImportedFileId = legacyImportedFileId;
      repoProject.Update(ifp);

      NotifyNewRelic(ifp, startUtc, $"{ifp.ImportedFileType} file created in project, now created in NhOp.");
    }

    /// <summary>
    /// Create the file in Project
    /// </summary>
    private async Task CreateFileInNewTable(ImportedFileRepoProject<ImportedFileProject> repoProject, DateTime startUtc,
      ImportedFileNhOp ifo)
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

        // Notify 3dpm of SS file created via Legacy
        if (projectEvent.LegacyImportedFileId != null) // Note that LegacyImportedFileId will always be !null 
          await NotifyRaptorImportedFileChange(projectEvent.CustomerUid, Guid.Parse(projectEvent.ProjectUid),
              Guid.Parse(projectEvent.ImportedFileUid))
            .ConfigureAwait(false);
      }
      else if (projectEvent.ImportedFileType == ImportedFileType.Linework || 
               projectEvent.ImportedFileType == ImportedFileType.DesignSurface || 
               projectEvent.ImportedFileType == ImportedFileType.Alignment)
      {
        await DownloadFileAndCallProjectWebApi(projectEvent, WebApiAction.Creating).ConfigureAwait(false);
      }

      NotifyNewRelic(projectEvent, startUtc, $"{ifo.ImportedFileType} file created in NhOp, now created in Project.");
    }

    /// <summary>
    /// Delete the file in nhOp
    /// </summary>
    private void DeleteFileInOldTable(ImportedFileRepoNhOp<ImportedFileNhOp> repoNhOp,
      ImportedFileProject gotMatchingProject, ImportedFileNhOp ifo, DateTime startUtc)
    {
      repoNhOp.Delete(ifo);
      _log.LogTrace(
        $"SyncTables: nhOp.IF is in nh_Op but was deleted in project. Deleted from NhOp: {JsonConvert.SerializeObject(ifo)}");

      NotifyNewRelic(gotMatchingProject, startUtc, $"{gotMatchingProject.ImportedFileType} file deleted in Project, now deleted in NhOp.");
    }

    /// <summary>
    /// Delete the file in Project
    /// </summary>
    private async Task DeleteFileInNewTable(ImportedFileRepoProject<ImportedFileProject> repoProject, DateTime startUtc,
      ImportedFileProject ifp)
    {
      if (ifp.ImportedFileType == ImportedFileType.SurveyedSurface)
      {
        repoProject.Delete(ifp);

        // Notify 3dpm of file deleted via Legacy
        // 3dpm will attempt to delete both by the LegacyImportedFileId and nextGens ImportedFileId
        if (ifp.LegacyImportedFileId != null) // Note that LegacyImportedFileId will always be !null 
          await NotifyRaptorFileDeletedInCGenAsync(ifp.CustomerUid, Guid.Parse(ifp.ProjectUid),
              Guid.Parse(ifp.ImportedFileUid), ifp.FileDescriptor, ifp.ImportedFileId, ifp.LegacyImportedFileId.Value)
            .ConfigureAwait(false);
      }
      else if (ifp.ImportedFileType == ImportedFileType.Linework ||
               ifp.ImportedFileType == ImportedFileType.DesignSurface ||
               ifp.ImportedFileType == ImportedFileType.Alignment)
      {
        var fileDescriptor = JsonConvert.DeserializeObject<FileDescriptor>(ifp.FileDescriptor);
        await CallProjectWebApi(ifp, WebApiAction.Deleting, fileDescriptor).ConfigureAwait(false);
      }

      NotifyNewRelic(ifp, startUtc, $"{ifp.ImportedFileType} file deleted in NhOp, now deleted from Project.");
    }

    /// <summary>
    /// Update the file in nhOp
    /// </summary>
    private void UpdateFileInOldTable(ImportedFileRepoNhOp<ImportedFileNhOp> repoNhOp,
      ImportedFileProject gotMatchingProject, ImportedFileNhOp ifo, DateTime startUtc)
    {
      ifo.FileCreatedUtc = gotMatchingProject.FileCreatedUtc;
      ifo.FileUpdatedUtc = gotMatchingProject.FileUpdatedUtc;
      ifo.LastActionedUtc = DateTime.UtcNow;
      repoNhOp.Update(ifo);

      NotifyNewRelic(gotMatchingProject, startUtc, $"{gotMatchingProject.ImportedFileType}fFile updated in project, now updated in NhOp.");
    }

    /// <summary>
    /// Update the file in Project
    /// </summary>
    private async Task UpdateFileInNewTable(ImportedFileRepoProject<ImportedFileProject> repoProject,
      ImportedFileProject gotMatchingProject, ImportedFileNhOp ifo, DateTime startUtc)
    {
      gotMatchingProject.FileCreatedUtc = ifo.FileCreatedUtc;
      gotMatchingProject.FileUpdatedUtc = ifo.FileUpdatedUtc;
      gotMatchingProject.LastActionedUtc = DateTime.UtcNow;
      repoProject.Update(gotMatchingProject);

      if (gotMatchingProject.ImportedFileType == ImportedFileType.SurveyedSurface)
      {
        // Notify 3dpm of SS file updated via Legacy
        if (gotMatchingProject.LegacyImportedFileId != null
        ) // Note that LegacyImportedFileId will always be !null 
          await NotifyRaptorImportedFileChange(gotMatchingProject.CustomerUid,
              Guid.Parse(gotMatchingProject.ProjectUid),
              Guid.Parse(gotMatchingProject.ImportedFileUid))
            .ConfigureAwait(false);
      }
      else if (gotMatchingProject.ImportedFileType == ImportedFileType.Linework ||
               gotMatchingProject.ImportedFileType == ImportedFileType.DesignSurface ||
               gotMatchingProject.ImportedFileType == ImportedFileType.Alignment)
      {
        await DownloadFileAndCallProjectWebApi(gotMatchingProject, WebApiAction.Updating).ConfigureAwait(false);
      }

      NotifyNewRelic(gotMatchingProject, startUtc, $"{gotMatchingProject.ImportedFileType} file updated in NhOp, now updated in Project.");
    }

    /// <summary>
    /// Information or warning for NewRelic
    /// </summary>
    private void NotifyNewRelic(ImportedFileProject ifp, DateTime startUtc, string message, string errorLevel = "Information")
    {
      var newRelicAttributes = new Dictionary<string, object>
      {
        { "message", message },
        { "customerUid", ifp.CustomerUid },
        { "projectUid", ifp.ProjectUid },
        { "importedFileUid", ifp.ImportedFileUid },
        { "fileDescriptor", ifp.FileDescriptor },
        { "legacyImportedFileId", ifp.LegacyImportedFileId }
      };
      NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", errorLevel, startUtc, _log, newRelicAttributes);
    }

    /// <summary>
    /// Information or warning for NewRelic
    /// </summary>
    private void NotifyNewRelic(ImportedFileNhOp ifo, DateTime startUtc, string message, string errorLevel = "Information")
    {
      var newRelicAttributes = new Dictionary<string, object>
      {
        { "message", message },
        { "customerUid", ifo.CustomerUid },
        { "projectUid", ifo.ProjectUid },
        { "fileDescriptor", ifo.Name },
        { "legacyImportedFileId", ifo.LegacyImportedFileId }
      };
      NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", errorLevel, startUtc, _log, newRelicAttributes);
    }


  }
}
