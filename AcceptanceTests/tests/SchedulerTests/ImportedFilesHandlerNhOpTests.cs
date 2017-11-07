using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace SchedulerTests
{
  [TestClass]
  public class ImportedFilesHandlerNhOpTests : TestControllerBase
  {
    private ILogger _log;

    [TestInitialize]
    public void Init()
    {
      SetupDi();

      _log = LoggerFactory.CreateLogger<ImportedFilesHandlerNhOpTests>();
      Assert.IsNotNull(_log, "Log is null");
    }

    [TestMethod]
    public void ImportedFilesHandlerNhOp_OneFileIn()
    {
      string projectDbConnectionString = ConnectionUtils.GetConnectionStringMsSql(ConfigStore, Log, "NH_OP");
      var importedFileHandlerNhOp = new ImportedFileHandlerNhOp<NhOpImportedFile>(ConfigStore, LoggerFactory);

      var importedFile = new NhOpImportedFile()
      {
        LegacyImportedFileId = new Random().Next(100000, 1999999),
        LegacyProjectId = new Random().Next(100000, 1999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(100000, 1999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        DxfUnitsType = 0,
        Name = "The File Name_2014-05-21T210701Z.TTM",
        SurveyedUtc = new DateTime(2017, 1, 1),
        FileCreatedUtc = new DateTime(2017, 1, 1),
        FileUpdatedUtc = new DateTime(2017, 1, 1),
        ImportedBy = "blah@blahdeblah.com",
        LastActionedUtc = new DateTime(2017, 1, 1)
      };

      var insertedCount = WriteImportedFileToNnOpDb(projectDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been 1 file written to NhOpDb");

      var readCount = importedFileHandlerNhOp.Read();
      Assert.AreNotEqual(0, readCount, "should have been at least 1 file read from NhOpDb");

      var listOfNhOpFiles = importedFileHandlerNhOp.List();
      Assert.IsNotNull(listOfNhOpFiles, "should be valid list");
      Assert.AreNotEqual(0, listOfNhOpFiles.Count, "should be at least 1 file in NhOpDb list");

      NhOpImportedFile importFileResponse = listOfNhOpFiles.FirstOrDefault(x => (String.Compare(x.ProjectUid, importedFile.ProjectUid, StringComparison.OrdinalIgnoreCase ) == 0)
                                                                                && x.Name == importedFile.Name);
      Assert.IsNotNull(importFileResponse, "should have one we tried to inserted");
      Assert.IsTrue((String.Compare(importFileResponse.ProjectUid, importedFile.ProjectUid, StringComparison.OrdinalIgnoreCase) == 0), 
        "unable to find the ProjectDb file we just inserted");

      Assert.IsNotNull(importFileResponse.LegacyImportedFileId, "should have returned a generated ID");
      Assert.IsTrue(importFileResponse.LegacyImportedFileId > 0, "should have returned the generated ID");
    }

    [TestMethod]
    public void ImportedFilesHandlerNhOp_OneFileIn_WrongFileType()
    {
      string projectDbConnectionString = ConnectionUtils.GetConnectionStringMsSql(ConfigStore, Log, "NH_OP");
      var importedFileHandlerNhOp = new ImportedFileHandlerNhOp<NhOpImportedFile>(ConfigStore, LoggerFactory);

      var importedFile = new NhOpImportedFile()
      {
        LegacyProjectId = new Random().Next(100000, 1999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(100000, 1999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.Alignment,
        DxfUnitsType = 0,
        Name = "The File Name_2014-05-21T210701Z.TTM",
        SurveyedUtc = new DateTime(2017, 1, 1),
        FileCreatedUtc = new DateTime(2017, 1, 1),
        FileUpdatedUtc = new DateTime(2017, 1, 1),
        ImportedBy = "blah@blahdeblah.com",
        LastActionedUtc = new DateTime(2017, 1, 1)
      };

      var insertedCount = WriteImportedFileToNnOpDb(projectDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been 1 file written to NhOpDb");

      importedFileHandlerNhOp.Read();
      var listOfProjectFiles = importedFileHandlerNhOp.List();
      NhOpImportedFile importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => x.ProjectUid == importedFile.ProjectUid
                                               && x.Name == importedFile.Name);
      Assert.IsNull(importFileResponse, "should not find the invalid one we tried to inserted");

    }

    [TestMethod]
    public void ImportedFilesHandlerNhOp_Create()
    {
      var importedFileHandlerNhOp = new ImportedFileHandlerNhOp<NhOpImportedFile>(ConfigStore, LoggerFactory);

      var importedFile = new NhOpImportedFile()
      {
        LegacyProjectId = new Random().Next(100000, 1999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(100000, 1999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        DxfUnitsType = 0,
        Name = "The File Name_2014-05-21T210701Z.TTM",
        SurveyedUtc = new DateTime(2017, 1, 1),
        FileCreatedUtc = new DateTime(2017, 1, 1),
        FileUpdatedUtc = new DateTime(2017, 1, 1),
        ImportedBy = "blah@blahdeblah.com",
        LastActionedUtc = new DateTime(2017, 1, 1)
      };

      var nhOpImportedFileList = new List<NhOpImportedFile>()
      {
        importedFile
      };

      var countCreated = importedFileHandlerNhOp.Create(nhOpImportedFileList);
      Assert.AreEqual(1, countCreated, "nhOpDb importFile not created");

      importedFileHandlerNhOp.EmptyList();

      var readCount = importedFileHandlerNhOp.Read();
      Assert.AreNotEqual(0, readCount, "should have been at least 1 file read from NhOpDb");

      var listOfNhOpFiles = importedFileHandlerNhOp.List();
      Assert.IsNotNull(listOfNhOpFiles, "should be valid list");
      Assert.AreNotEqual(0, listOfNhOpFiles.Count, "should be at least 1 file in NhOpDb list");

      NhOpImportedFile importFileResponse = listOfNhOpFiles.FirstOrDefault(x => x.ProjectUid == importedFile.ProjectUid
                                                                                && x.Name == importedFile.Name);
      Assert.IsNotNull(importFileResponse, "should have one we tried to inserted");
      Assert.AreEqual(importedFile.ProjectUid, importFileResponse?.ProjectUid,
        "unable to find the ProjectDb file we just inserted");

      Assert.IsNotNull(importFileResponse.LegacyImportedFileId, "should have returned a generated ID");

      var deletedCount = importedFileHandlerNhOp.Delete(nhOpImportedFileList);
      Assert.AreEqual(1, deletedCount, "nhOpDb importFile not deleted");

      importedFileHandlerNhOp.EmptyList();
      importedFileHandlerNhOp.Read();

      listOfNhOpFiles = importedFileHandlerNhOp.List();
      importFileResponse = listOfNhOpFiles.FirstOrDefault(x => x.ProjectUid == importedFile.ProjectUid
                                                               && x.Name == importedFile.Name);
      Assert.IsNull(importFileResponse, "should no longer find the one we created");

    }

    [TestMethod]
    public void ImportedFilesHandlerNhOp_Delete()
    {
      var importedFileHandlerNhOp = new ImportedFileHandlerNhOp<NhOpImportedFile>(ConfigStore, LoggerFactory);

      var importedFile = new NhOpImportedFile()
      {
        LegacyProjectId = new Random().Next(100000, 1999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(100000, 1999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        DxfUnitsType = 0,
        Name = "The File Name_2014-05-21T210701Z.TTM",
        SurveyedUtc = new DateTime(2017, 1, 1),
        FileCreatedUtc = new DateTime(2017, 1, 1),
        FileUpdatedUtc = new DateTime(2017, 1, 1),
        ImportedBy = "blah@blahdeblah.com",
        LastActionedUtc = new DateTime(2017, 1, 1)
      };

      var nhOpImportedFileList = new List<NhOpImportedFile>()
      {
        importedFile
      };

      var countCreated = importedFileHandlerNhOp.Create(nhOpImportedFileList);
      Assert.AreEqual(1, countCreated, "nhOpDb importFile not created");

      var readCount = importedFileHandlerNhOp.Read();
      Assert.AreNotEqual(0, readCount, "should have been at least 1 file read from NhOpDb");

      var listOfNhOpFiles = importedFileHandlerNhOp.List();
      NhOpImportedFile importFileResponse = listOfNhOpFiles.FirstOrDefault(x => x.ProjectUid == importedFile.ProjectUid
                                                                                && x.Name == importedFile.Name);
      Assert.IsNotNull(importFileResponse, "should have one we tried to inserted");
      Assert.AreEqual(importedFile.ProjectUid, importFileResponse?.ProjectUid,
        "unable to find the ProjectDb file we just inserted");
    }

    private int WriteImportedFileToNnOpDb(string projectDbConnectionString, NhOpImportedFile importedFile)
    {
      var dbConnection = new SqlConnection(projectDbConnectionString);
      dbConnection.Open();

      int insertedCount = 0;
       var insertCommand = string.Format(
        $"INSERT Customer " +
        "   (ID, CustomerUID, Name, fk_CustomerTypeID, BSSID, fk_DealerNetworkID) " +
        "  VALUES (@LegacyCustomerId, @CustomerUid, @Name, 0, 'bssId', 0)");

      dbConnection.Execute("SET IDENTITY_INSERT Customer ON");
      insertedCount = dbConnection.Execute(insertCommand, importedFile);
      dbConnection.Execute("SET IDENTITY_INSERT Customer OFF");

      insertCommand = string.Format(
        $"INSERT Project " +
        "    (ID, ProjectUID, Name, fk_CustomerID, fk_ProjectTypeID, fk_SiteID, TimezoneName) " +
        "  VALUES " +
        "    (@LegacyProjectId, @ProjectUid, 'the project name', @LegacyCustomerId, 0, 0, 'whateverTZ')");

      dbConnection.Execute("SET IDENTITY_INSERT Project ON");
      insertedCount = dbConnection.Execute(insertCommand, importedFile);
      dbConnection.Execute("SET IDENTITY_INSERT Project OFF");

      insertCommand = string.Format(
        "INSERT ImportedFileHistory " +
        "    (fk_ImportedFileID, CreateUTC, InsertUTC) " +
        "  VALUES " +
        "    (@LegacyImportedFileId, @FileCreatedUtc, @FileUpdatedUtc)");

      insertedCount = dbConnection.Execute(insertCommand, importedFile);

      insertCommand = string.Format(
        "INSERT ImportedFile " +
        "    (ID, fk_CustomerID, fk_ProjectID, Name, fk_ImportedFileTypeID, SurveyedUTC, fk_DXFUnitsTypeID) " +
        "  VALUES " +
        "    (@LegacyImportedFileId, @LegacyCustomerId, @LegacyProjectId, @Name, @ImportedFileType, @SurveyedUtc, @DxfUnitsType)");

      dbConnection.Execute("SET IDENTITY_INSERT ImportedFile ON");
      insertedCount = dbConnection.Execute(insertCommand, importedFile);
      dbConnection.Execute("SET IDENTITY_INSERT ImportedFile OFF");

      dbConnection.Close();
      return insertedCount;
    }
  }
}
