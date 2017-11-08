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
  public class ImportedFilesHandlerTests : TestControllerBase
  {
    private ILogger _log;
    private string _projectDbConnectionString;

    [TestInitialize]
    public void Init()
    {
      SetupDi();

      _log = LoggerFactory.CreateLogger<ImportedFilesHandlerTests>();
      _projectDbConnectionString = ConnectionUtils.GetConnectionStringMySql(ConfigStore, _log, "_PROJECT");

      Assert.IsNotNull(_log, "Log is null");
    }

    [TestMethod]
    public void ImportedFilesHandler_OneFileInProject()
    {
      var importedFileHandlerProject =
        new ImportedFileHandlerProject<ProjectImportedFile>(ConfigStore, LoggerFactory);

      var importedFile = new ProjectImportedFile()
      {
        LegacyProjectId = new Random().Next(100000, 1999999),
        LegacyCustomerId = new Random().Next(100000, 1999999),
        ProjectUid = Guid.NewGuid().ToString(),
        ImportedFileUid = Guid.NewGuid().ToString(),
        ImportedFileId = new Random().Next(100000, 1999999),
        LegacyImportedFileId = new Random().Next(100000, 1999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "JB topo southern motorway.TTM",
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

      var insertedCount = WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have written customer and project");

      var createdCount = importedFileHandlerProject.Create(importedFile);
      Assert.AreEqual(1, createdCount, "nhOpDb importFile not created");

      var sync = new ImportedFileHandler(ConfigStore, LoggerFactory);
      sync.SyncTables();

      // now lets see if it synced to NhOp
      var importedFileHandlerNhOp =
        new ImportedFileHandlerNhOp<NhOpImportedFile>(ConfigStore, LoggerFactory);

      var listOfNhOpFiles = importedFileHandlerNhOp.Read();
      Assert.AreNotEqual(0, listOfNhOpFiles.Count, "NhOpDb importFile not read");
      NhOpImportedFile importFileResponse =
        listOfNhOpFiles.FirstOrDefault(x => (String.Compare(x.ProjectUid, importedFile.ProjectUid,
                                               StringComparison.OrdinalIgnoreCase) == 0)
                                            && x.Name == importedFile.Name);
      Assert.IsNotNull(importFileResponse, "should have found the importedFile we created in project, synced to NhOp");
    }
  }
}
