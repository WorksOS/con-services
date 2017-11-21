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

namespace VSS.Productivity3D.Scheduler.Common.Controller
{
  public class ImportedFileSynchronizer : ImportedFileSynchronizerBase
  {

    public ImportedFileSynchronizer(IConfigurationStore configStore, ILoggerFactory logger, IRaptorProxy raptorProxy) : base(configStore, logger, raptorProxy)
    {
    }

    /// <summary>
    /// Read from NGen Project.ImportedFile table
    ///   May initially be limited to SurveyedSurface type
    /// </summary>
    public async Task SyncTables()
    {
      ImportedFileRepoNhOp<ImportedFileNhOp> repoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, Logger);
      ImportedFileRepoProject<ImportedFileProject> repoProject = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, Logger);

      var fileListProject = repoProject.Read();
      var fileListNhOp = repoNhOp.Read();
      var startUtc = DateTime.UtcNow;

      // cannot modify collectionB from within collectionA
      // put it in here and remove later
      var fileListProjectToRemove = new List<ImportedFileProject>();


      // row in  NH_OP and in project, nothing has changed (a)
      // row in  NH_OP and in project, deleted in project. delete in NH_OP (b)
      // row in  NH_OP and in project, a date has changed (c)
      // row in  NH_OP NOT in project, create it {d}
      foreach (var ifo in fileListNhOp.ToList())
      {
        // (a)
        startUtc = DateTime.UtcNow;
        var gotMatchingProject = fileListProject.FirstOrDefault(o => o.LegacyImportedFileId == ifo.LegacyImportedFileId);

        if (gotMatchingProject == null)
        {
          // (d)
          var projectEvent = AutoMapperUtility.Automapper.Map<ImportedFileProject>(ifo);
          projectEvent.Name = ImportedFileUtils.RemoveSurveyedUtcFromName(projectEvent.Name);
          projectEvent.ImportedFileUid = Guid.NewGuid().ToString();
          // for L&S if its come from CG then use legacyIds
          projectEvent.FileDescriptor = JsonConvert.SerializeObject(FileDescriptor.CreateFileDescriptor(FileSpaceId,
            projectEvent.LegacyCustomerId.ToString(), projectEvent.LegacyProjectId.ToString(), projectEvent.Name));
          if (projectEvent.ImportedBy == null) projectEvent.ImportedBy = "";
          repoProject.Create(projectEvent);

          // Notify 3dpm of SS file created via Legacy
          if (projectEvent.LegacyImportedFileId != null) // Note that LegacyImportedFileId will always be !null 
            await NotifyRaptorFileCreatedInCGenAsync(Guid.Parse(projectEvent.ProjectUid), projectEvent.ImportedFileType,
                Guid.Parse(projectEvent.ImportedFileUid), projectEvent.FileDescriptor,
                projectEvent.LegacyImportedFileId.Value, projectEvent.DxfUnitsType)
              .ConfigureAwait(false);

          var newRelicAttributes = new Dictionary<string, object>
          {
            {"message", "SS file created in NhOp, now created in Project."},
            {"project", projectEvent}
          };
          NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Information", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
          fileListNhOp.RemoveAt(0);
        }
        else
        {
          if (gotMatchingProject.IsDeleted)
          {
            // (b)
            repoNhOp.Delete(ifo);
            Log.LogTrace(
              $"SyncTables: nhOp.IF is in nh_Op but was deleted in project. Deleted from NhOp: {JsonConvert.SerializeObject(ifo)}");

            var newRelicAttributes = new Dictionary<string, object>
            {
              {"message", "SS file deleted in Project, now deleted in NhOp."},
              {"project", gotMatchingProject}
            };
            NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Information", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
          }
          else
          {
            if (gotMatchingProject.FileCreatedUtc != ifo.FileCreatedUtc
                || gotMatchingProject.FileUpdatedUtc != ifo.FileUpdatedUtc)
            {
              // (c)
              if (gotMatchingProject.FileCreatedUtc > ifo.FileCreatedUtc
                  || gotMatchingProject.FileUpdatedUtc > ifo.FileUpdatedUtc)
              {
                // project is more recent, update nh_op
                ifo.FileCreatedUtc = gotMatchingProject.FileCreatedUtc;
                ifo.FileUpdatedUtc = gotMatchingProject.FileUpdatedUtc;
                ifo.LastActionedUtc = DateTime.UtcNow;
                repoNhOp.Update(ifo);

                var newRelicAttributes = new Dictionary<string, object>
                {
                  {"message", "SS file updated in project, now updated in NhOp."},
                  {"project", gotMatchingProject}
                };
                NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Information", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
              }
              else
              {
                // nh_op is more recent, update project
                gotMatchingProject.FileCreatedUtc = ifo.FileCreatedUtc;
                gotMatchingProject.FileUpdatedUtc = ifo.FileUpdatedUtc;
                gotMatchingProject.LastActionedUtc = DateTime.UtcNow;
                repoProject.Update(gotMatchingProject);

                // Notify 3dpm of SS file updated via Legacy
                if (gotMatchingProject.LegacyImportedFileId != null
                ) // Note that LegacyImportedFileId will always be !null 
                  await NotifyRaptorFileUpdatedInCGen(Guid.Parse(gotMatchingProject.ProjectUid),
                      Guid.Parse(gotMatchingProject.ImportedFileUid))
                    .ConfigureAwait(false);

                var newRelicAttributes = new Dictionary<string, object>
                {
                  {"message", "SS file updated in NhOp, now updated in Project."},
                  {"project", gotMatchingProject}
                };
                NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Information", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
              }
            }
          }

          // (a) no change
          fileListProjectToRemove.Add(gotMatchingProject);
          fileListNhOp.Remove(ifo);
        }
      }

      // internal error, have we forgotten some scenario?
      if (fileListNhOp.Count > 0)
      {
        // proceed with sync, but send alert to NewRelic
        var newRelicAttributes = new Dictionary<string, object>
        {
          {"message", string.Format($"ImportedFileSynchroniser internal error as fileListNhOp list should be empty")},
          {"fileListNhOp", fileListNhOp}
        };
        NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Error", startUtc,
          (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
      }

      fileListProject.RemoveAll(
        x => fileListProjectToRemove.Exists(y => y.LegacyImportedFileId == x.LegacyImportedFileId));

      // row in Project but doesn't exist in nhOp
      //        if project has LegacyImportedFileId ,  and not already deleted, then delete in Project (m)
      //        if project has no LegacyImportedFileId, and not already deleted, then create in nhOp (n)
      //        if project has no LegacyImportedFileId, not deleted, but has no valid legacy customer or projectID then can't be added to NhOp (o)
      //        deleted project in project but no link to nh_op, just ignore it. (p)
      foreach (var ifp in fileListProject.ToList())
      {
        if (ifp.IsDeleted)
        {
          // (p)
          fileListProject.RemoveAt(0);
        }
        else
        {
          if (ifp.LegacyImportedFileId != null && ifp.LegacyImportedFileId > 0)
          {
            // (m)
            repoProject.Delete(ifp);

            // Notify 3dpm of SS file deleted via Legacy
            if (ifp.LegacyImportedFileId != null) // Note that LegacyImportedFileId will always be !null 
              await NotifyRaptorFileDeletedInCGenAsync(Guid.Parse(ifp.ProjectUid), ifp.ImportedFileType,
                  Guid.Parse(ifp.ImportedFileUid), ifp.FileDescriptor, (long) ifp.LegacyImportedFileId)
                .ConfigureAwait(false);
            
            var newRelicAttributes = new Dictionary<string, object>
            {
              {"message", "SS file deleted in NhOp, now deleted from Project."},
              {"project", ifp}
            };
            NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Information", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
            fileListProject.RemoveAt(0);
          }
          else
          {
            if (ifp.LegacyCustomerId == 0 || ifp.LegacyProjectId >= 1000000)
            {
              // (o)
              var newRelicAttributes = new Dictionary<string, object>
              {
                {"message", "SS file in Project which has no legacyCustomerId so cannot be synced to NhOp."},
                {"project", ifp}
              };
              NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
              fileListProject.RemoveAt(0);
            }
            else
            {
              // (n)
              var nhOpEvent = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(ifp);
              nhOpEvent.Name = ImportedFileUtils.IncludeSurveyedUtcInName(nhOpEvent.Name, nhOpEvent.SurveyedUtc.Value);
              var legacyImportedFileId = repoNhOp.Create(nhOpEvent);
              ifp.LegacyImportedFileId = legacyImportedFileId;
              repoProject.Update(ifp);

              var newRelicAttributes = new Dictionary<string, object>
              {
                {"message", "SS file created in project, now created in NhOp."},
                {"project", ifp}
              };
              NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Information", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
              fileListProject.RemoveAt(0);
            }
          }
        }
      }

      // internal error, have we forgotten some scenario?
      if (fileListProject.Count > 0)
      {        // proceed with sync, but send alert to NewRelic
        var newRelicAttributes = new Dictionary<string, object>
        {
          {"message", string.Format($"ImportedFileSynchroniser internal error as fileListProject list should be empty")},
          {"fileListProject", fileListProject}
        };
        NewRelicUtils.NotifyNewRelic("DatabaseSyncTask", "Error", startUtc,
          (DateTime.UtcNow - startUtc).TotalMilliseconds, newRelicAttributes);
      }
    }
  }
}
