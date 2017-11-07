using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace SchedulerTests
{
  [TestClass]
  public class ImportedFilesHandlerTests : TestControllerBase
  {
    private ILogger _log;

    [TestInitialize]
    public void Init()
    {
      SetupDi();

      _log = LoggerFactory.CreateLogger<ImportedFilesHandlerTests>();
      Assert.IsNotNull(_log, "Log is null");
    }

    [TestMethod]
    public void ImportedFilesHandler_OneFileInProject()
    {
      var importedFileHandlerProject =
        new ImportedFileHandlerProject<ProjectImportedFile>(ConfigStore, LoggerFactory);

      var importedFile = new ProjectImportedFile
      {
        LegacyProjectId = new Random().Next(100000, 1999999),
        LegacyCustomerId = new Random().Next(100000, 1999999),
        ProjectUid = Guid.NewGuid().ToString(),
        ImportedFileUid = Guid.NewGuid().ToString(),
        // ImportedFileId = new Random().Next(100000, 1999999), // i.e. NOT legacyImportedFileId, just a NG Id could we use it?
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "The File Name.ttm",
        FileDescriptor = "wot?",
        FileCreatedUtc = new DateTime(2017, 1, 1),
        FileUpdatedUtc = new DateTime(2017, 1, 2),
        ImportedBy = "whoever@gmail.com",
        IsDeleted = false,
        IsActivated = true,
        SurveyedUtc = new DateTime(2017, 1, 1),
        DxfUnitsType = 0,
        LastActionedUtc = new DateTime(2017, 1, 1)
      };

      var projectImportedFileList = new List<ProjectImportedFile>()
      {
        importedFile
      };

      var createdCount = importedFileHandlerProject.Create(projectImportedFileList);
      Assert.AreEqual(1, createdCount, "nhOpDb importFile not created");

      var sync = new ImportedFileHandler(ConfigStore, LoggerFactory);
      sync.SyncTables();

      // now lets see if it synced to NhOp
      var importedFileHandlerNhOp =
        new ImportedFileHandlerNhOp<NhOpImportedFile>(ConfigStore, LoggerFactory);

      var readCount = importedFileHandlerNhOp.Read();
      Assert.AreNotEqual(0, readCount, "NhOpDb importFile not read");

      var listOfNhOpFiles = importedFileHandlerNhOp.List();
      NhOpImportedFile importFileResponse =
        listOfNhOpFiles.FirstOrDefault(x => x.ProjectUid == importedFile.ProjectUid
                                            && x.Name == importedFile.Name);
      Assert.IsNotNull(importFileResponse, "should have found the importedFile we created in project, synced to NhOp");
    }
  }
}
