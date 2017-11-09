using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Scheduler.Common.Utilities;

namespace VSS.Productivity3D.Scheduler.Common.Models
{
  public class ImportedFileSynchronizer
  {
    private IConfigurationStore _configStore;
    private ILogger _log;
    private ImportedFileRepoNhOp<ImportedFileNhOp> _repoNhOp;
    private ImportedFileRepoProject<ImportedFileProject> _repoProject;

    /// <summary>
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    public ImportedFileSynchronizer(IConfigurationStore configStore, ILoggerFactory logger)
    {
      _configStore = configStore;
      _log = logger.CreateLogger<ImportedFileSynchronizer>();

      _repoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(configStore, logger);
      _repoProject = new ImportedFileRepoProject<ImportedFileProject>(configStore, logger);
    }

    /// <summary>
    /// Read from NGen Project.ImportedFile table
    ///   May initially be limited to SurveyedSurface type
    /// </summary>
    public void SyncTables()
    {
      var fileListProject = _repoProject.Read();
      var fileListNhOp = _repoNhOp.Read();

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
        var gotMatchingProject =
          fileListProject.FirstOrDefault(o => o.LegacyImportedFileId == ifo.LegacyImportedFileId);

        if (gotMatchingProject == null)
        {
          // (d)
          var projectEvent = AutoMapperUtility.Automapper.Map<ImportedFileProject>(ifo);
          projectEvent.ImportedFileUid = Guid.NewGuid().ToString();
          // todo create FileDescriptor and ImportedBy
          projectEvent.FileDescriptor = "todo";
          projectEvent.ImportedBy = "todo";
          _repoProject.Create(projectEvent);
          fileListNhOp.RemoveAt(0);
          _log.LogTrace(
            $"SyncTables: nhOp.IF is not in Project. Add it to Project : {JsonConvert.SerializeObject(gotMatchingProject)}");
        }
        else
        {
          if (gotMatchingProject.IsDeleted)
          {
            // (b)
            _repoNhOp.Delete(ifo);
            fileListNhOp.RemoveAt(0);
            _log.LogTrace(
              $"SyncTables: nhOp.IF is in nh_Op but was deleted in project. Deleted from NhOp: {JsonConvert.SerializeObject(ifo)}");
          }
          else
          {
            // todo can surveyName change?
            // todo determine which is more recent and update the other to match
            if (gotMatchingProject.SurveyedUtc != ifo.SurveyedUtc
                || gotMatchingProject.FileCreatedUtc != ifo.FileCreatedUtc
                || gotMatchingProject.FileUpdatedUtc != ifo.FileUpdatedUtc)
            {
              // (c)
              gotMatchingProject.SurveyedUtc = ifo.SurveyedUtc;
              gotMatchingProject.FileCreatedUtc = ifo.FileCreatedUtc;
              gotMatchingProject.FileUpdatedUtc = ifo.FileUpdatedUtc;
              _repoProject.Update(gotMatchingProject);
              fileListNhOp.RemoveAt(0);
              _log.LogTrace(
                $"SyncTables: nhOp.IF is in Project and nhOp but some aspect has changed. Update in both Project and NhOp: {JsonConvert.SerializeObject(gotMatchingProject)}");
            }
            else
            {
              // (a)
              fileListProjectToRemove.Add(gotMatchingProject);
              fileListNhOp.Remove(ifo);
            }
          }
        }
      }

      if (fileListNhOp.Count > 0)
        throw new InvalidOperationException(
          "ImportedFileSynchroniser internal error as fileListNhOp list should be empty");

      fileListProject.RemoveAll(
        x => fileListProjectToRemove.Exists(y => y.LegacyImportedFileId == x.LegacyImportedFileId));

      // row in Project but doesn't exist in nhOp
      //        if project has LegacyImportedFileId ,  and not already deleted, then delete in Project (m)
      //        if project has no LegacyImportedFileId, and not already deleted, then create in nhOp (n)
      //        if project has no LegacyImportedFileId, not deleted, but has no valid legacy customer or projectID then can't be added to NhOp (o)
      //        deleted project in project but no link to nh_op, just delete it. (p)
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
            _repoProject.Delete(ifp);
            fileListProject.RemoveAt(0);
            _log.LogTrace(
              $"SyncTables: Project.IF is not in NH_OP but was. Deleted from Project: {JsonConvert.SerializeObject(ifp)}");
          }
          else
          {
            if (ifp.LegacyCustomerId == 0 || ifp.LegacyProjectId >= 1000000)
            {
              // (o)
              fileListProject.RemoveAt(0);
              _log.LogWarning(
                $"SyncTables: Project.IF is not in NH_OP. However it has no legacy customer/projectID so cannot be added to nhOp: {JsonConvert.SerializeObject(ifp)}");
            }
            else
            {
              // (n)
              var nhOpEvent = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(ifp);
              var legacyImportedFileId = _repoNhOp.Create(nhOpEvent);
              ifp.LegacyImportedFileId = legacyImportedFileId;
              _repoProject.Update(ifp);
              fileListProject
                .RemoveAt(0); // Remove doesn't work as item changed, even if you make a copy using Automapper
              _log.LogTrace(
                $"SyncTables: Project.IF is not in NH_OP. Added to nhOp: {JsonConvert.SerializeObject(nhOpEvent)}");
            }
          }
        }
      }

      if (fileListProject.Count > 0)
        throw new InvalidOperationException(
          "ImportedFileSynchroniser internal error as fileListProject list should be empty");

      // todo TCC and RaptorNotiications
    }
  }
}
