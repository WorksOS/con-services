using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System.Linq;
using VSS.Productivity3D.Scheduler.Common.Repository;

namespace SchedulerTests
{
  [TestClass]
  public class ImportedFileRepoNhOpTests : TestControllerBase
  {
    private ILogger _log;
    private string _projectDbConnectionString;
    private string _nhOpDbConnectionString;

    [TestInitialize]
    public void Init()
    {
      SetupDi();

      _log = LoggerFactory.CreateLogger<ImportedFileRepoNhOpTests>();
      _projectDbConnectionString = ConnectionUtils.GetConnectionStringMySql(ConfigStore, _log, "_PROJECT");
      _nhOpDbConnectionString = ConnectionUtils.GetConnectionStringMsSql(ConfigStore, Log, "NH_OP");

      Assert.IsNotNull(_log, "Log is null");
    }

    [TestMethod]
    public void ImportedFileRepoNhOp_OneFileIn()
    {
      var importedFileRepoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, LoggerFactory);

      var importedFile = new ImportedFileNhOp()
      {
        LegacyImportedFileId = new Random().Next(1, 19999999),
        LegacyProjectId = new Random().Next(100000, 19999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(1, 19999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "ImportedFileRepoNhOp_OneFileIn.TTM",
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        // DxfUnitsType N/A for SurveyedSurface
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var insertedCount = WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFile);
      Assert.AreEqual(2, insertedCount, "should have been customerProject written to NhOpDb");

      // need this for when sync occurs
      var importedFileProject = AutoMapperUtility.Automapper.Map<ImportedFileProject>(importedFile);
      insertedCount = WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFileProject);
      Assert.AreEqual(2, insertedCount, "should have written customer and project to ProjectDB");

      insertedCount = WriteNhOpDbImportedFileAndHistory(_nhOpDbConnectionString, importedFile);
      Assert.AreEqual(2, insertedCount, "should have been ImportedFile written to NhOpDb");

      var listOfNhOpFiles = importedFileRepoNhOp.Read();
      Assert.AreNotEqual(0, listOfNhOpFiles.Count, "should have been at least 1 file read from NhOpDb");
   
      ImportedFileNhOp importFileResponse = listOfNhOpFiles.FirstOrDefault(x => x.LegacyImportedFileId == importedFile.LegacyImportedFileId);
      Assert.IsNotNull(importFileResponse, "should have the one we tried to inserted");
      Assert.IsTrue((String.Compare(importFileResponse.ProjectUid, importedFile.ProjectUid, StringComparison.OrdinalIgnoreCase) == 0), 
        "unable to find the ProjectDb file we just inserted");
      Assert.AreEqual(importedFile.LegacyImportedFileId, importFileResponse.LegacyImportedFileId, "should have returned the legacy ID");
      Assert.AreEqual(importedFile.Name, importFileResponse.Name, "should have returned the name");
    }

    [TestMethod]
    public void ImportedFileRepoNhOp_OneFileIn_WrongFileType()
    {
      var ImportedFileRepoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, LoggerFactory);

      var importedFile = new ImportedFileNhOp()
      {
        LegacyImportedFileId = new Random().Next(100000, 1999999),
        LegacyProjectId = new Random().Next(100000, 19999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(1, 19999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.Alignment,
        Name = "ImportedFileRepoNhOp_OneFileIn_WrongFileType.TTM",
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var insertedCount = WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFile);
      Assert.AreEqual(2, insertedCount, "should have been customerETc written to NhOpDb");

      // need this for when sync occurs
      var importedFileProject = AutoMapperUtility.Automapper.Map<ImportedFileProject>(importedFile);
      insertedCount = WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFileProject);
      Assert.AreEqual(2, insertedCount, "should have written customer and project to ProjectDB");

      insertedCount = WriteNhOpDbImportedFileAndHistory(_nhOpDbConnectionString, importedFile);
      Assert.AreEqual(2, insertedCount, "should have been ImportedFile written to NhOpDb");

      var listOfProjectFiles = ImportedFileRepoNhOp.Read();
      ImportedFileNhOp importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => x.LegacyImportedFileId == importedFile.LegacyImportedFileId);
      Assert.IsNull(importFileResponse, "should not find the invalid one we tried to inserted");

    }

    
    [TestMethod]
    public void ImportedFileRepoNhOp_Create()
    {
      var ImportedFileRepoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, LoggerFactory);

      var importedFile = new ImportedFileNhOp()
      {
        LegacyImportedFileId = -1,
        LegacyProjectId = new Random().Next(100000, 19999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(1, 19999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "ImportedFileRepoNhOp_Create.TTM",
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var insertedCount = WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFile);
      Assert.AreEqual(2, insertedCount, "should have been customerProject written to NhOpDb");

      // need this for when sync occurs
      var importedFileProject = AutoMapperUtility.Automapper.Map<ImportedFileProject>(importedFile);
      insertedCount = WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFileProject);
      Assert.AreEqual(2, insertedCount, "should have written customer and project to ProjectDB");

      var createdLegacyImportedFileId = ImportedFileRepoNhOp.Create(importedFile);
      Assert.IsTrue(createdLegacyImportedFileId > 0, "should return the Identity field from ImportedFile written to NhOpDb");
      importedFile.LegacyImportedFileId = createdLegacyImportedFileId;

      var listOfNhOpFiles = ImportedFileRepoNhOp.Read();
      Assert.AreNotEqual(0, listOfNhOpFiles.Count, "should have been at least 1 file read from NhOpDb");
      ImportedFileNhOp importFileResponse = listOfNhOpFiles.FirstOrDefault(x => x.LegacyImportedFileId == importedFile.LegacyImportedFileId);
      Assert.IsNotNull(importFileResponse, "should have the one we tried to inserted");
      Assert.IsTrue((String.Compare(importFileResponse.ProjectUid, importedFile.ProjectUid, StringComparison.OrdinalIgnoreCase) == 0),
        "unable to find the ProjectDb file we just inserted");
      Assert.AreEqual(importedFile.LegacyImportedFileId, importFileResponse.LegacyImportedFileId, "should have returned the legacy ID");
      Assert.AreEqual(importedFile.Name, importFileResponse.Name, "should have returned the name");
    }

    [TestMethod]
    public void ImportedFileRepoNhOp_Delete()
    {
      var ImportedFileRepoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, LoggerFactory);

      var importedFile = new ImportedFileNhOp()
      {
        LegacyImportedFileId = -1,
        LegacyProjectId = new Random().Next(100000, 19999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(1, 19999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "ImportedFileRepoNhOp_Delete.TTM",
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var insertedCount = WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFile);
      Assert.AreEqual(2, insertedCount, "should have been customerProject written to NhOpDb");

      // need this for when sync occurs
      var importedFileProject = AutoMapperUtility.Automapper.Map<ImportedFileProject>(importedFile);
      insertedCount = WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFileProject);
      Assert.AreEqual(2, insertedCount, "should have written customer and project to ProjectDB");

      var createdLegacyImportedFileId = ImportedFileRepoNhOp.Create(importedFile);
      Assert.IsTrue(createdLegacyImportedFileId > 0, "should return the Identity field from ImportedFile written to NhOpDb");
      importedFile.LegacyImportedFileId = createdLegacyImportedFileId;

      var listOfNhOpFiles = ImportedFileRepoNhOp.Read();
      Assert.AreNotEqual(0, listOfNhOpFiles.Count, "should have been at least 1 file read from NhOpDb");

      ImportedFileNhOp importFileResponse = listOfNhOpFiles.FirstOrDefault(x => x.LegacyImportedFileId == importedFile.LegacyImportedFileId);
      Assert.IsNotNull(importFileResponse, "should have the one we tried to inserted");
      Assert.IsTrue((String.Compare(importFileResponse.ProjectUid, importedFile.ProjectUid, StringComparison.OrdinalIgnoreCase) == 0),
        "unable to find the ProjectDb file we just inserted");
      Assert.AreEqual(importedFile.LegacyImportedFileId, importFileResponse.LegacyImportedFileId, "should have returned the legacy ID");
      Assert.AreEqual(importedFile.Name, importFileResponse.Name, "should have returned the name");

      var deletedCount = ImportedFileRepoNhOp.Delete(importedFile);
      Assert.IsTrue(deletedCount > 1, "nhOpDb importFile and history not deleted");

      listOfNhOpFiles = ImportedFileRepoNhOp.Read();
      importFileResponse = listOfNhOpFiles.FirstOrDefault(x => x.LegacyImportedFileId == importedFile.LegacyImportedFileId);
      Assert.IsNull(importFileResponse, "should no longer find the one we created");

    }
  }
}
