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

    protected ILogger _log;

    public ImportedFileSynchronizer(IConfigurationStore configStore, ILoggerFactory logger, IRaptorProxy raptorProxy,
      ITPaasProxy tPaasProxy)
      : base(configStore, logger, raptorProxy, tPaasProxy)
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
      var startUtc = DateTime.UtcNow;

      // cannot modify collectionB from within collectionA
      // put it in here and remove later
      var fileListProjectToRemove = new List<ImportedFileProject>();


      // row in  NH_OP and in project, nothing has changed (a)
      // row in  NH_OP and in project, deleted in project. delete in NH_OP (b)
      // row in  NH_OP and in project, a date has changed (c)
      // row in  NH_OP NOT in project, 
      //                 if project and customer exist, then create it {d}
      //                 else error {e}
      foreach (var ifo in fileListNhOp.ToList())
      {
        // (a)
        startUtc = DateTime.UtcNow;
        var gotMatchingProject =
          fileListProject.FirstOrDefault(o => o.LegacyImportedFileId == ifo.LegacyImportedFileId);

        if (gotMatchingProject == null)
        {
          if (repoProject.ProjectAndCustomerExist(ifo.CustomerUid, ifo.ProjectUid))
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
              await NotifyRaptorImportedFileChange(projectEvent.CustomerUid, Guid.Parse(projectEvent.ProjectUid),
                  Guid.Parse(projectEvent.ImportedFileUid))
                .ConfigureAwait(false);

            var newRelicAttributes = new Dictionary<string, object>
            {
              {"message", "SS file created in NhOp, now created in Project."},
              {"projectUid", projectEvent.ProjectUid},
              {"importedFileUid", projectEvent.ImportedFileUid},
              {"fileDescriptor", projectEvent.FileDescriptor},
              {"legacyImportedFileId", projectEvent.LegacyImportedFileId}
            };
            NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Information", startUtc, _log, newRelicAttributes);
          }
          else
          {
            // (e)
            var newRelicAttributes = new Dictionary<string, object>
            {
              {
                "message",
                "SS file created in NhOp, cannot create in Project as the project and/or customer relationship doesn't exist."
              },
              {"projectUid", ifo.ProjectUid},
              {"customerUid", ifo.CustomerUid},
              {"fileDescriptor", ifo.Name},
              {"legacyImportedFileId", ifo.LegacyImportedFileId}
            };
            NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, _log, newRelicAttributes);
          }
          // fileListNhOp.RemoveAt(0);
        }
        else
        {
          if (gotMatchingProject.IsDeleted)
          {
            // (b)
            repoNhOp.Delete(ifo);
            _log.LogTrace(
              $"SyncTables: nhOp.IF is in nh_Op but was deleted in project. Deleted from NhOp: {JsonConvert.SerializeObject(ifo)}");

            var newRelicAttributes = new Dictionary<string, object>
            {
              {"message", "SS file deleted in Project, now deleted in NhOp."},
              {"projectUid", gotMatchingProject.ProjectUid},
              {"importedFileUid", gotMatchingProject.ImportedFileUid},
              {"fileDescriptor", gotMatchingProject.FileDescriptor},
              {"legacyImportedFileId", gotMatchingProject.LegacyImportedFileId}
            };
            NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Information", startUtc, _log, newRelicAttributes);
          }
          else
          {
            var projectCreatedUtcRounded =
              DateTime.Parse(gotMatchingProject.FileCreatedUtc.ToString("yyyy-MM-dd HH:mm:ss"));
            var projectUpdatedUtcRounded =
              DateTime.Parse(gotMatchingProject.FileUpdatedUtc.ToString("yyyy-MM-dd HH:mm:ss"));
            var nhOpCreatedUtcRounded = DateTime.Parse(ifo.FileCreatedUtc.ToString("yyyy-MM-dd HH:mm:ss"));
            var nhOpUpdatedUtcRounded = DateTime.Parse(ifo.FileUpdatedUtc.ToString("yyyy-MM-dd HH:mm:ss"));

            if (projectCreatedUtcRounded != nhOpCreatedUtcRounded
                || projectUpdatedUtcRounded != nhOpUpdatedUtcRounded)
            {
              // (c)
              if (projectCreatedUtcRounded > nhOpCreatedUtcRounded
                  || projectUpdatedUtcRounded > nhOpUpdatedUtcRounded)
              {
                // project is more recent, update nh_op
                ifo.FileCreatedUtc = gotMatchingProject.FileCreatedUtc;
                ifo.FileUpdatedUtc = gotMatchingProject.FileUpdatedUtc;
                ifo.LastActionedUtc = DateTime.UtcNow;
                repoNhOp.Update(ifo);

                var newRelicAttributes = new Dictionary<string, object>
                {
                  {"message", "SS file updated in project, now updated in NhOp."},
                  {"projectUid", gotMatchingProject.ProjectUid},
                  {"importedFileUid", gotMatchingProject.ImportedFileUid},
                  {"fileDescriptor", gotMatchingProject.FileDescriptor},
                  {"legacyImportedFileId", gotMatchingProject.LegacyImportedFileId}
                };
                NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Information", startUtc, _log,
                  newRelicAttributes);
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
                  await NotifyRaptorImportedFileChange(gotMatchingProject.CustomerUid,
                      Guid.Parse(gotMatchingProject.ProjectUid),
                      Guid.Parse(gotMatchingProject.ImportedFileUid))
                    .ConfigureAwait(false);

                var newRelicAttributes = new Dictionary<string, object>
                {
                  {"message", "SS file updated in NhOp, now updated in Project."},
                  {"projectUid", gotMatchingProject.ProjectUid},
                  {"importedFileUid", gotMatchingProject.ImportedFileUid},
                  {"fileDescriptor", gotMatchingProject.FileDescriptor},
                  {"legacyImportedFileId", gotMatchingProject.LegacyImportedFileId}
                };
                NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Information", startUtc, _log,
                  newRelicAttributes);
              }
            }
          }

          // (a) no change
          fileListProjectToRemove.Add(gotMatchingProject);
          // fileListNhOp.Remove(ifo);
        }
      }


      fileListProject.RemoveAll(
        x => fileListProjectToRemove.Exists(y => y.LegacyImportedFileId == x.LegacyImportedFileId));

      // row in Project but doesn't exist in nhOp
      //        if project has LegacyImportedFileId ,  and not already deleted, then delete in Project (m)
      //        if project has no LegacyImportedFileId, and not already deleted, 
      //                                 if project and customer exist in nhOp then create in nhOp (n)
      //                                 else error {q} 
      //        if project has no LegacyImportedFileId, not deleted, but has no valid legacy customer or projectID then can't be added to NhOp (o)
      //        deleted project in project but no link to nh_op, just ignore it. (p)
      foreach (var ifp in fileListProject)
      {
        if (ifp.IsDeleted)
        {
          // (p)
        }
        else
        {
          if (ifp.LegacyImportedFileId != null && ifp.LegacyImportedFileId > 0)
          {
            // (m)
            repoProject.Delete(ifp);

            // Notify 3dpm of SS file deleted via Legacy
            if (ifp.LegacyImportedFileId != null) // Note that LegacyImportedFileId will always be !null 
              await NotifyRaptorImportedFileChange(ifp.CustomerUid, Guid.Parse(ifp.ProjectUid),
                  Guid.Parse(ifp.ImportedFileUid))
                .ConfigureAwait(false);

            var newRelicAttributes = new Dictionary<string, object>
            {
              {"message", "SS file deleted in NhOp, now deleted from Project."},
              {"projectUid", ifp.ProjectUid},
              {"importedFileUid", ifp.ImportedFileUid},
              {"fileDescriptor", ifp.FileDescriptor},
              {"legacyImportedFileId", ifp.LegacyImportedFileId}
            };
            NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Information", startUtc, _log, newRelicAttributes);
          }
          else
          {
            if (ifp.LegacyCustomerId == 0 || ifp.LegacyProjectId >= 1000000)
            {
              // (o)
              var newRelicAttributes = new Dictionary<string, object>
              {
                {"message", "SS file in Project which has no legacyCustomerId so cannot be synced to NhOp."},
                {"projectUid", ifp.ProjectUid},
                {"importedFileUid", ifp.ImportedFileUid},
                {"fileDescriptor", ifp.FileDescriptor},
                {"legacyImportedFileId", ifp.LegacyImportedFileId}
              };
              NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Warning", startUtc, _log, newRelicAttributes);
            }
            else
            {
              if (repoNhOp.ProjectAndCustomerExist(ifp.CustomerUid, ifp.ProjectUid))
              {
                // (n)
                var nhOpEvent = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(ifp);
                nhOpEvent.Name =
                  ImportedFileUtils.IncludeSurveyedUtcInName(nhOpEvent.Name, nhOpEvent.SurveyedUtc.Value);
                var legacyImportedFileId = repoNhOp.Create(nhOpEvent);
                ifp.LegacyImportedFileId = legacyImportedFileId;
                repoProject.Update(ifp);

                var newRelicAttributes = new Dictionary<string, object>
                {
                  {"message", "SS file created in project, now created in NhOp."},
                  {"projectUid", ifp.ProjectUid},
                  {"importedFileUid", ifp.ImportedFileUid},
                  {"fileDescriptor", ifp.FileDescriptor},
                  {"legacyImportedFileId", ifp.LegacyImportedFileId}
                };
                NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Information", startUtc, _log,
                  newRelicAttributes);
              }
              else
              {
                // (q)
                var newRelicAttributes = new Dictionary<string, object>
                {
                  {
                    "message",
                    "SS file created in project, cannot create in NhOp as the project and/or customer relationship doesn't exist."
                  },
                  {"projectUid", ifp.ProjectUid},
                  {"customerUid", ifp.CustomerUid},
                  {"fileDescriptor", ifp.Name},
                  {"ImportedFileUid", ifp.ImportedFileUid}
                };
                NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, _log, newRelicAttributes);
              }
            }
          }
        }
      }
    }
  }
}
