using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.Productivity3D.Scheduler.Common.Repository;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace SchedulerTestsImportedFileSync
{
  [TestClass]
  public class ImportedFileRepoProjectTests : TestControllerBase
  {
    private ILogger _log;
    private string _projectDbConnectionString;
    private string _nhOpDbConnectionString;

    [TestInitialize]
    public void Init()
    {
      SetupDi();

      _log = LoggerFactory.CreateLogger<ImportedFileRepoProjectTests>();
      _projectDbConnectionString = ConnectionUtils.GetConnectionStringMySql(ConfigStore, _log, "_PROJECT");
      _nhOpDbConnectionString = ConnectionUtils.GetConnectionStringMsSql(ConfigStore, Log, "NH_OP");

      Assert.IsNotNull(_log, "Log is null");
    }

    [TestMethod]
    public void ImportedFileRepoProject_OneFileIn()
    {
      var importedFileRepoProject = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);

      var importedFile = new ImportedFileProject()
      {
        LegacyProjectId = new Random().Next(1, 900000),
        LegacyCustomerId = new Random().Next(1, 9999999),
        ProjectUid = Guid.NewGuid().ToString(),
        ImportedFileUid = Guid.NewGuid().ToString(),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "ImportedFileRepoProject_OneFileIn.TTM",
        FileDescriptor =
          "{ \"filespaceId\":\"u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01\",\"path\":\"/87bdf851-44c5-e311-aa77-00505688274d/62a52e4f-faa2-e511-80e5-0050568821e6\",\"fileName\":\"DesignSVL13072017034205.svl\"}",
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        IsDeleted = false,
        IsActivated = true,
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        // DxfUnitsType N/A for SurveyedSurface
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var insertedCount = WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFile);
      Assert.AreEqual(2, insertedCount, "should have written customer and project");

      // need this for when sync occurs
      var importedFileNhOp = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(importedFile);
      insertedCount = WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFileNhOp);
      Assert.AreEqual(2, insertedCount, "should have written customer and project to NhOpDb");

      insertedCount = WriteToProjectDBImportedFile(_projectDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been 1 file written to ProjectDb");

      var listOfProjectFiles = importedFileRepoProject.Read();
      Assert.AreNotEqual(0, listOfProjectFiles.Count, "should have been at least 1 file read from ProjectDb");
      Assert.AreNotEqual(0, listOfProjectFiles.Count, "should be at least 1 file in ProjectDb list");

      ImportedFile importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => (String.Compare(x.ImportedFileUid, importedFile.ImportedFileUid,
                                                  StringComparison.OrdinalIgnoreCase) == 0));
      Assert.IsNotNull(importFileResponse, "should have one we tried to inserted");
      Assert.AreEqual(importedFile.ImportedFileUid, importFileResponse?.ImportedFileUid,
        "unable to find the ImportedFileUid file we just inserted");
    }

    [TestMethod]
    public void ImportedFileRepoProject_OneFileIn_WrongFileType()
    {
      var importedFileRepoProject = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);

      var importedFile = new ImportedFileProject()
      {
        LegacyProjectId = new Random().Next(1, 900000),
        LegacyCustomerId = new Random().Next(1, 9999999),
        ProjectUid = Guid.NewGuid().ToString(),
        ImportedFileUid = Guid.NewGuid().ToString(),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.Alignment,
        Name = "ImportedFileRepoProject_OneFileIn_WrongFileType.TTM",
        FileDescriptor =
          "{ \"filespaceId\":\"u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01\",\"path\":\"/87bdf851-44c5-e311-aa77-00505688274d/62a52e4f-faa2-e511-80e5-0050568821e6\",\"fileName\":\"DesignSVL13072017034205.svl\"}",
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        IsDeleted = false,
        IsActivated = true,
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var insertedCount = WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFile);
      Assert.AreEqual(2, insertedCount, "should have written customer and project");

      // need this for when sync occurs
      var importedFileNhOp = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(importedFile);
      insertedCount = WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFileNhOp);
      Assert.AreEqual(2, insertedCount, "should have written customer and project to NhOpDb");

      insertedCount = WriteToProjectDBImportedFile(_projectDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been 1 ImportedFile written to ProjectDb");

      var listOfProjectFiles = importedFileRepoProject.Read();
      ImportedFile importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => (String.Compare(x.ImportedFileUid, importedFile.ImportedFileUid,
                                                  StringComparison.OrdinalIgnoreCase) == 0));
      Assert.IsNull(importFileResponse, "should not find the invalid one we tried to inserted");
    }

    [TestMethod]
    public void ImportedFileRepoProject_Create()
    {
      var importedFileRepoProject = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);

      var importedFile = new ImportedFileProject()
      {
        LegacyProjectId = new Random().Next(1, 900000),
        LegacyCustomerId = new Random().Next(1, 9999999),
        ProjectUid = Guid.NewGuid().ToString(),
        ImportedFileUid = Guid.NewGuid().ToString(),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "ImportedFileRepoProject_Create.TTM",
        FileDescriptor =
          "{ \"filespaceId\":\"u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01\",\"path\":\"/87bdf851-44c5-e311-aa77-00505688274d/62a52e4f-faa2-e511-80e5-0050568821e6\",\"fileName\":\"DesignSVL13072017034205.svl\"}",
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        IsDeleted = false,
        IsActivated = true,
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var insertedCount = WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFile);
      Assert.AreEqual(2, insertedCount, "should have written customer and project");

      // need this for when sync occurs
      var importedFileNhOp = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(importedFile);
      insertedCount = WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFileNhOp);
      Assert.AreEqual(2, insertedCount, "should have written customer and project to NhOpDb");

      var createdCount = importedFileRepoProject.Create(importedFile);
      Assert.AreEqual(1, createdCount, "nhOpDb importFile not created");

      var listOfProjectFiles = importedFileRepoProject.Read();
      Assert.AreNotEqual(0, listOfProjectFiles.Count, "ProjectDb importFile not read");

      ImportedFile importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => (String.Compare(x.ImportedFileUid, importedFile.ImportedFileUid,
                                                  StringComparison.OrdinalIgnoreCase) == 0));
      Assert.IsNotNull(importFileResponse, "should have found the ImportedFileUid one we just created");
    }

    public void ImportedFileRepoProject_Delete()
    {
      var importedFileRepoProject = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);

      var importedFile = new ImportedFileProject()
      {
        LegacyProjectId = new Random().Next(1, 900000),
        LegacyCustomerId = new Random().Next(1, 9999999),
        ProjectUid = Guid.NewGuid().ToString(),
        ImportedFileUid = Guid.NewGuid().ToString(),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "ImportedFileRepoProject_Delete.TTM",
        FileDescriptor =
          "{ \"filespaceId\":\"u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01\",\"path\":\"/87bdf851-44c5-e311-aa77-00505688274d/62a52e4f-faa2-e511-80e5-0050568821e6\",\"fileName\":\"DesignSVL13072017034205.svl\"}",
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        IsDeleted = false,
        IsActivated = true,
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var createCount = importedFileRepoProject.Create(importedFile);
      Assert.AreEqual(1, createCount, "nhOpDb importFile not created");

      var listOfProjectFiles = importedFileRepoProject.Read();
      Assert.AreNotEqual(0, listOfProjectFiles.Count, "ProjectDb importFile not read");

      ImportedFile importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => (String.Compare(x.ImportedFileUid, importedFile.ImportedFileUid,
                                                  StringComparison.OrdinalIgnoreCase) == 0));
      Assert.IsNotNull(importFileResponse, "should have found the ImportedFileUid one we just created");

      var deletedCount = importedFileRepoProject.Delete(importedFile);
      Assert.AreEqual(1, deletedCount, "nhOpDb importFile not deleted");

      listOfProjectFiles = importedFileRepoProject.Read();
      importFileResponse = listOfProjectFiles.FirstOrDefault(x => (String.Compare(x.ImportedFileUid, importedFile.ImportedFileUid,
                                                                     StringComparison.OrdinalIgnoreCase) == 0));
      Assert.IsNull(importFileResponse, "should no longer find the one we created");
    }
  }
}
