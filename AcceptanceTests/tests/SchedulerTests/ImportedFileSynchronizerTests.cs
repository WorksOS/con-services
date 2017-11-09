using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace SchedulerTests
{
  [TestClass]
  public class ImportedFileSynchronizerTests : TestControllerBase
  {
    private ILogger _log;
    private string _projectDbConnectionString;
    private string _nhOpDbConnectionString;

    [TestInitialize]
    public void Init()
    {
      SetupDi();

      _log = LoggerFactory.CreateLogger<ImportedFileSynchronizerTests>();
      _projectDbConnectionString = ConnectionUtils.GetConnectionStringMySql(ConfigStore, _log, "_PROJECT");
      _nhOpDbConnectionString = ConnectionUtils.GetConnectionStringMsSql(ConfigStore, Log, "NH_OP");

      Assert.IsNotNull(_log, "Log is null");
    }

    [TestMethod]
    public void ImportedFileRepoProject_OneFileInProject()
    {
      var importedFileRepoProject = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);

      var importedFileProject = new ImportedFileProject()
      {
        LegacyProjectId = new Random().Next(1, 1000000),
        LegacyCustomerId = new Random().Next(1, 9999999),
        ProjectUid = Guid.NewGuid().ToString(),
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyImportedFileId = null,
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "ImportedFileRepoProject_OneFileInProject.TTM",
        FileDescriptor =
          "{ \"filespaceId\":\"u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01\",\"path\":\"/87bdf851-44c5-e311-aa77-00505688274d/62a52e4f-faa2-e511-80e5-0050568821e6\",\"fileName\":\"DesignSVL13072017034205.svl\"}",
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        IsDeleted = false,
        IsActivated = true,
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        DxfUnitsType = DxfUnitsType.UsSurveyFeet,
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var insertedCount = WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFileProject);
      Assert.AreEqual(2, insertedCount, "should have written customer and project to ProjectDB");

      // need this for when sync occurs
      var importedFileNhOp = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(importedFileProject);
      insertedCount = WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFileNhOp);
      Assert.AreEqual(2, insertedCount, "should have written customer and project to NhOpDb");

      var createdCount = importedFileRepoProject.Create(importedFileProject);
      Assert.AreEqual(1, createdCount, "nhOpDb importFile not created");

      var sync = new ImportedFileSynchronizer(ConfigStore, LoggerFactory);
      sync.SyncTables();

      // now lets see if it synced to NhOp
      var importedFileRepoNhOp =
        new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, LoggerFactory);

      var listOfNhOpFiles = importedFileRepoNhOp.Read();
      Assert.AreNotEqual(0, listOfNhOpFiles.Count, "NhOpDb importFile not read");
      ImportedFileNhOp importFileResponse =
        listOfNhOpFiles.FirstOrDefault(x => (String.Compare(x.ProjectUid, importedFileProject.ProjectUid,
                                               StringComparison.OrdinalIgnoreCase) == 0)
                                            && x.Name == importedFileProject.Name);
      Assert.IsNotNull(importFileResponse, "should have found the importedFile we created in project, synced to NhOp");
    }
  }
}
