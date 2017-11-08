using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Dapper;
using MySql.Data.MySqlClient;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace SchedulerTests
{
  [TestClass]
  public class ImportedFilesHandlerProjectTests : TestControllerBase
  {
    private ILogger _log;
    private string _projectDbConnectionString; 

    [TestInitialize]
    public void Init()
    {
      SetupDi();

      _log = LoggerFactory.CreateLogger<ImportedFilesHandlerProjectTests>();
      _projectDbConnectionString = ConnectionUtils.GetConnectionStringMySql(ConfigStore, _log, "_PROJECT");
      Assert.IsNotNull(_log, "Log is null");
    }

    [TestMethod]
    public void ImportedFilesHandlerProject_OneFileIn()
    {
      var importedFileHandlerProject = new ImportedFileHandlerProject<ProjectImportedFile>(ConfigStore, LoggerFactory);

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

      insertedCount = WriteToProjectDBImportedFile(_projectDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been 1 file written to ProjectDb");

      var listOfProjectFiles = importedFileHandlerProject.Read();
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
    public void ImportedFilesHandlerProject_OneFileIn_WrongFileType()
    {
      var importedFileHandlerProject = new ImportedFileHandlerProject<ProjectImportedFile>(ConfigStore, LoggerFactory);

      var importedFile = new ProjectImportedFile()
      {
        LegacyProjectId = new Random().Next(100000, 1999999),
        LegacyCustomerId = new Random().Next(100000, 1999999),
        ProjectUid = Guid.NewGuid().ToString(),
        ImportedFileUid = Guid.NewGuid().ToString(),
        ImportedFileId = new Random().Next(100000, 1999999),
        LegacyImportedFileId = new Random().Next(100000, 1999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.Alignment,
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

      insertedCount = WriteToProjectDBImportedFile(_projectDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been 1 ImportedFile written to ProjectDb");

      var listOfProjectFiles = importedFileHandlerProject.Read();
      ImportedFile importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => (String.Compare(x.ImportedFileUid, importedFile.ImportedFileUid,
                                                  StringComparison.OrdinalIgnoreCase) == 0));
      Assert.IsNull(importFileResponse, "should not find the invalid one we tried to inserted");
    }

    [TestMethod]
    public void ImportedFilesHandlerProject_Create()
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

      var listOfProjectFiles = importedFileHandlerProject.Read();
      Assert.AreNotEqual(0, listOfProjectFiles.Count, "ProjectDb importFile not read");

      ImportedFile importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => (String.Compare(x.ImportedFileUid, importedFile.ImportedFileUid,
                                                  StringComparison.OrdinalIgnoreCase) == 0));
      Assert.IsNotNull(importFileResponse, "should have found the ImportedFileUid one we just created");
    }

    public void ImportedFilesHandlerProject_Delete()
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

      var createCount = importedFileHandlerProject.Create(importedFile);
      Assert.AreEqual(1, createCount, "nhOpDb importFile not created");

      var listOfProjectFiles = importedFileHandlerProject.Read();
      Assert.AreNotEqual(0, listOfProjectFiles.Count, "ProjectDb importFile not read");

      ImportedFile importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => (String.Compare(x.ImportedFileUid, importedFile.ImportedFileUid,
                                                  StringComparison.OrdinalIgnoreCase) == 0));
      Assert.IsNotNull(importFileResponse, "should have found the ImportedFileUid one we just created");

      var deletedCount = importedFileHandlerProject.Delete(importedFile);
      Assert.AreEqual(1, deletedCount, "nhOpDb importFile not deleted");

      listOfProjectFiles = importedFileHandlerProject.Read();
      importFileResponse = listOfProjectFiles.FirstOrDefault(x => (String.Compare(x.ImportedFileUid, importedFile.ImportedFileUid,
                                                                     StringComparison.OrdinalIgnoreCase) == 0));
      Assert.IsNull(importFileResponse, "should no longer find the one we created");
    }
  }
}
