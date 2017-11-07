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

    [TestInitialize]
    public void Init()
    {
      SetupDi();

      _log = LoggerFactory.CreateLogger<ImportedFilesHandlerProjectTests>();
      Assert.IsNotNull(_log, "Log is null");
    }

    [TestMethod]
    public void ImportedFilesHandlerProject_OneFileIn()
    {
      string projectDbConnectionString = ConnectionUtils.GetConnectionStringMySql(ConfigStore, _log, "_PROJECT");
      var importedFileHandlerProject = new ImportedFileHandlerProject<ProjectImportedFile>(ConfigStore, LoggerFactory);

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

      var insertedCount = WriteImportedFileToProjectDb(projectDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been 1 file written to ProjectDb");

      var readCount = importedFileHandlerProject.Read();
      Assert.AreNotEqual(0, readCount, "should have been at least 1 file read from ProjectDb");

      var listOfProjectFiles = importedFileHandlerProject.List();
      Assert.IsNotNull(listOfProjectFiles, "should be valid list");
      Assert.AreNotEqual(0, listOfProjectFiles.Count, "should be at least 1 file in ProjectDb list");

      ImportedFile importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => x.ImportedFileUid == importedFile.ImportedFileUid);
      Assert.IsNotNull(importFileResponse, "should have one we tried to inserted");
      Assert.AreEqual(importedFile.ImportedFileUid, importFileResponse?.ImportedFileUid,
        "unable to find the ImportedFileUid file we just inserted");
    }

    [TestMethod]
    public void ImportedFilesHandlerProject_OneFileIn_WrongFileType()
    {
      string projectDbConnectionString = ConnectionUtils.GetConnectionStringMySql(ConfigStore, _log, "_PROJECT");
      var importedFileHandlerProject = new ImportedFileHandlerProject<ProjectImportedFile>(ConfigStore, LoggerFactory);

      var importedFile = new ProjectImportedFile
      {
        LegacyProjectId = new Random().Next(100000, 1999999),
        LegacyCustomerId = new Random().Next(100000, 1999999),
        ProjectUid = Guid.NewGuid().ToString(),
        ImportedFileUid = Guid.NewGuid().ToString(),
        // ImportedFileId = new Random().Next(100000, 1999999), // i.e. NOT legacyImportedFileId, just a NG Id could we use it?
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.Alignment,
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

      var insertedCount = WriteImportedFileToProjectDb(projectDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been 1 file written to ProjectDb");

      importedFileHandlerProject.Read();
      var listOfProjectFiles = importedFileHandlerProject.List();
      ImportedFile importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => x.ImportedFileUid == importedFile.ImportedFileUid);
      Assert.IsNull(importFileResponse, "should not find the invalid one we tried to inserted");
    }

    [TestMethod]
    public void ImportedFilesHandlerProject_Create()
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

      importedFileHandlerProject.EmptyList();

      var readCount = importedFileHandlerProject.Read();
      Assert.AreNotEqual(0, readCount, "ProjectDb importFile not read");

      var listOfProjectFiles = importedFileHandlerProject.List();
      ImportedFile importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => x.ImportedFileUid == importedFile.ImportedFileUid);
      Assert.IsNotNull(importFileResponse, "should have found the ImportedFileUid one we just created");
    }

    public void ImportedFilesHandlerProject_Delete()
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

      var createCount = importedFileHandlerProject.Create(projectImportedFileList);
      Assert.AreEqual(1, createCount, "nhOpDb importFile not created");

      var readCount = importedFileHandlerProject.Read();
      Assert.AreNotEqual(0, readCount, "ProjectDb importFile not read");

      var listOfProjectFiles = importedFileHandlerProject.List();
      ImportedFile importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => x.ImportedFileUid == importedFile.ImportedFileUid);
      Assert.IsNotNull(importFileResponse, "should have found the ImportedFileUid one we just created");

      var deletedCount = importedFileHandlerProject.Delete(projectImportedFileList);
      Assert.AreEqual(1, deletedCount, "nhOpDb importFile not deleted");

      importedFileHandlerProject.EmptyList();
      importedFileHandlerProject.Read();

      listOfProjectFiles = importedFileHandlerProject.List();
      importFileResponse = listOfProjectFiles.FirstOrDefault(x => x.ImportedFileUid == importedFile.ImportedFileUid);
      Assert.IsNull(importFileResponse, "should no longer find the one we created");

    }

    private int WriteImportedFileToProjectDb(string projectDbConnectionString, ImportedFile importedFile)
    {
      var dbConnection = new MySqlConnection(projectDbConnectionString);
      dbConnection.Open();

      var insertCommand = string.Format(
        "INSERT ImportedFile " +
        "    (fk_ProjectUID, ImportedFileUID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID,  IsDeleted, IsActivated, LastActionedUTC)" +
        "  VALUES " +
        "    (@ProjectUid, @ImportedFileUid, @CustomerUid, @ImportedFileType, @Name, @FileDescriptor, @FileCreatedUTC, @FileUpdatedUTC, @ImportedBy, @SurveyedUtc, @DxfUnitsType, 0, 1, @LastActionedUtc)");

      int insertedCount = dbConnection.Execute(insertCommand, importedFile);
      dbConnection.Close();
      return insertedCount;
    }
  }
}
