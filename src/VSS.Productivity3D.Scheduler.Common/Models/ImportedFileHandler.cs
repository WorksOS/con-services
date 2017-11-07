using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Scheduler.Common.Utilities;

namespace VSS.Productivity3D.Scheduler.Common.Models
{
  public class ImportedFileHandler
  {
    private IConfigurationStore _configStore;
    private ILogger _log;
    private ImportedFileHandlerNhOp<NhOpImportedFile> _nhOpRepo;
    private ImportedFileHandlerProject<ProjectImportedFile> _projectRepo;

    /// <summary>
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    public ImportedFileHandler(IConfigurationStore configStore, ILoggerFactory logger)
    {
      _configStore = configStore;
      _log = logger.CreateLogger<ImportedFileHandlerProject<ImportedFileHandler>>();

      _nhOpRepo = new ImportedFileHandlerNhOp<NhOpImportedFile>(configStore, logger);
      _projectRepo = new ImportedFileHandlerProject<ProjectImportedFile>(configStore, logger);
    }

    /// <summary>
    /// Read from NGen Project.ImportedFile table
    ///   May initially be limited to SurveyedSurface type
    /// </summary>
    public void SyncTables()
    {
      _projectRepo.Read();
      var ifsProject = _projectRepo.List();
      _nhOpRepo.Read();
      var ifsNhOp = _nhOpRepo.List();

      // store these for reversal or just exec as we go?
      var createProject = new List<ProjectImportedFile>();
      var updateProject = new List<ProjectImportedFile>();
      var deleteProject = new List<ProjectImportedFile>();
      var createNhOp = new List<NhOpImportedFile>();
      var createHistoryNhOp = new List<NhOpImportedFile>();
      var deleteNhOp = new List<NhOpImportedFile>();

      // row in project and NH_OP
      // a) if deleteFlag has been set in project, then delete from NHOp
      // b) remove all which are already syncd
      foreach (var ifp in ifsProject)
      {
        var gotMatchingNhOp = ifsNhOp.First(o =>
          o.ProjectUid == ifp.ProjectUid && o.Name == ifp.Name && o.SurveyedUtc == ifp.SurveyedUtc &&
          o.FileCreatedUtc == ifp.FileCreatedUtc && o.FileUpdatedUtc == ifp.FileUpdatedUtc);

        if (gotMatchingNhOp != null)
        {
          // deleted in project, need to delete in NH_OP
          if (ifp.IsDeleted)
          {
            var nhOpImportedFile = AutoMapperUtility.Automapper.Map<NhOpImportedFile>(ifp);
            deleteNhOp.Add(nhOpImportedFile);
          }

          ifsProject.Remove(ifp);
          ifsNhOp.Remove(gotMatchingNhOp);
        }
      }

      // row in  NH_OP but not in project
      // a) insert into project
      foreach (var ifo in ifsNhOp)
      {
        var gotMatchingProject = ifsProject.First(o => o.ProjectUid == ifo.ProjectUid && o.Name == ifo.Name &&
                                                       o.SurveyedUtc == ifo.SurveyedUtc);
        if (gotMatchingProject == null)
        {
          var projectImportedFile = AutoMapperUtility.Automapper.Map<ProjectImportedFile>(ifo);
          createProject.Add(projectImportedFile);
          ifsNhOp.Remove(ifo);
        }
      }

      // row in project but not in NH_OP
      // a) if it has since been deleted from NH_OP, then delete from project
      // b) else insert into NH_OP
      foreach (var ifp in ifsProject)
      {
        var gotMatchingNhOp = ifsNhOp.FindAll(o => o.ProjectUid == ifp.ProjectUid && o.Name == ifp.Name &&
                                                   o.SurveyedUtc == ifp.SurveyedUtc);

        // is nh_op missing because it needs to be created, or has it been deleted
        if (gotMatchingNhOp.Count == 0)
        {
          var nhOpImportedFile = AutoMapperUtility.Automapper.Map<NhOpImportedFile>(ifp);
          if (_nhOpRepo.IsImportedFileDeleted(nhOpImportedFile))
          {
            ifp.IsDeleted = true;
            deleteProject.Add(ifp);
          }
          else
            createNhOp.Add(nhOpImportedFile);

          ifsProject.Remove(ifp);
        }
      }

      // survey file name exists in both,
      //     but have been updated (FileCreatedUTC) in one and/or other
      //     i.e. file has been re-imported so new updateUtc
      foreach (var ifp in ifsProject)
      {
        var gotMatchingNhOps = ifsNhOp.FindAll(o => o.ProjectUid == ifp.ProjectUid && o.Name == ifp.Name &&
                                                    o.SurveyedUtc == ifp.SurveyedUtc
                                                    && (o.FileCreatedUtc != ifp.FileCreatedUtc ||
                                                        o.FileUpdatedUtc != ifp.FileUpdatedUtc))
          .OrderBy(o => o.FileUpdatedUtc);

        // NH_OP stores each update fileCreated/Updated in ImportedFileHistory, however we will only have the latest one in project
        // to write to nh_Op must have a prior NH+OP one to get legacyProjectID and CustomerID
        NhOpImportedFile importedFileHistory = gotMatchingNhOps.First();
        importedFileHistory.FileCreatedUtc = ifp.FileCreatedUtc;
        importedFileHistory.FileUpdatedUtc = ifp.FileUpdatedUtc;
        createHistoryNhOp.Add(importedFileHistory);

        // Project stores only the most recent fileCreated/Updated
        if (ifp.FileUpdatedUtc < gotMatchingNhOps.Last().FileUpdatedUtc)
        {
          ifp.FileUpdatedUtc = gotMatchingNhOps.Last().FileUpdatedUtc;
          ifp.FileCreatedUtc = gotMatchingNhOps.Last().FileCreatedUtc;
          ifp.LastActionedUtc = gotMatchingNhOps.Last().LastActionedUtc;
          updateProject.Add(ifp);
        }
        ifsProject.Remove(ifp);
        foreach (var gotMatchingNhOp in gotMatchingNhOps)
        {
          ifsNhOp.Remove(gotMatchingNhOp);
        }
      }

      // should be none left in either list now

      // store these for reversal or just exec as we go?
      _projectRepo.Create(createProject);
      _projectRepo.Update(updateProject);
      _nhOpRepo.Create(createNhOp);
      _nhOpRepo.Update(createHistoryNhOp);
      _nhOpRepo.Delete(deleteNhOp);

      // todo TCC and RaptorNotiications
      // todo rollback
    }

  }
}
