using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

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
      // string projectDbConnectionString = string.Format($"Data Source=alpha-nh-raw.c31ahitxrkg7.us-west-2.rds.amazonaws.com;Initial Catalog=NH_OP;Integrated Security=False;User ID=root;Password=d3vRDS1234_;");
      string projectDbConnectionString = ConnectionUtils.GetConnectionStringMsSql(ConfigStore, Log, "NH_OP");
      var importedFileHandlerProject = new ImportedFileHandlerNhOp<NhOpImportedFile>(ConfigStore, LoggerFactory);

      var importedFile = new NhOpImportedFile()
      {
        LegacyProjectId = 123,
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = 456,
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

      //var insertedCount = WriteImportedFileToProjectDb(projectDbConnectionString, importedFile);
      //Assert.AreEqual(1, insertedCount, "should have been 1 file written to ProjectDb");

      var readCount = importedFileHandlerProject.Read();
      Assert.AreNotEqual(0, readCount, "should have been at least 1 file read from ProjectDb");

      var listOfProjectFiles = importedFileHandlerProject.List();
      Assert.IsNotNull(listOfProjectFiles, "should be valid list");
      Assert.AreNotEqual(0, listOfProjectFiles.Count, "should be at least 1 file in ProjectDb list");

      //ImportedFile importFileResponse = listOfProjectFiles.FirstOrDefault(x => x.ProjectUid == importedFile.ProjectUid);
      //Assert.IsNotNull(importFileResponse, "should have one we tried to inserted");
      //Assert.AreEqual(importedFile.ProjectUid, importFileResponse?.ProjectUid, "unable to find the ProjectDb file we just inserted");
    }

    [TestMethod]
    public void ImportedFilesHandlerNhOp_OneFileIn_WrongFileType()
    {
      //string projectDbConnectionString = ConnectionUtils.GetConnectionStringMySql(ConfigStore, _log, "_PROJECT");
      //var importedFileHandlerProject = new ImportedFileHandlerProject<ImportedFile>(ConfigStore, LoggerFactory, projectDbConnectionString);

      //var importedFile = new ImportedFile
      //{
      //  ProjectUid = Guid.NewGuid().ToString(),
      //  ImportedFileUid = Guid.NewGuid().ToString(),
      //  CustomerUid = Guid.NewGuid().ToString(),
      //  ImportedFileType = ImportedFileType.Alignment,
      //  Name = "The File Name",
      //  FileDescriptor = "wot?",
      //  FileCreatedUtc = new DateTime(2017, 1, 1),
      //  FileUpdatedUtc = new DateTime(2017, 1, 1),
      //  ImportedBy = "whoever",
      //  SurveyedUtc = new DateTime(2017, 1, 1),
      //  LastActionedUtc = new DateTime(2017, 1, 1),
      //  IsActivated = true
      //};

      //var insertedCount = WriteImportedFileToProjectDb(projectDbConnectionString, importedFile);
      //Assert.AreEqual(1, insertedCount, "should have been 1 file written to ProjectDb");

      //importedFileHandlerProject.ReadFromDb();
      //var listOfProjectFiles = importedFileHandlerProject.List();
      //ImportedFile importFileResponse = listOfProjectFiles.FirstOrDefault(x => x.ProjectUid == importedFile.ProjectUid);
      //Assert.IsNull(importFileResponse, "should not find the invalid one we tried to inserted");
    }

    [TestMethod]
    [Ignore]
    public void ImportedFilesHandlerNhOp_MergeAndWrite()
    {
     // string projectDbConnectionString = ConnectionUtils.GetConnectionStringMySql(ConfigStore, _log, "_PROJECT");
     // var importedFileHandlerProject = new ImportedFileHandlerProject<ImportedFile>(ConfigStore, LoggerFactory, projectDbConnectionString);

     // var importedFileProject = new ImportedFile
     // {
     //   ProjectUid = Guid.NewGuid().ToString(),
     //   ImportedFileUid = Guid.NewGuid().ToString(),
     //   CustomerUid = Guid.NewGuid().ToString(),
     //   ImportedFileType = ImportedFileType.SurveyedSurface,
     //   Name = "The File Name",
     //   FileDescriptor = "wot?",
     //   FileCreatedUtc = new DateTime(2017, 1, 1),
     //   FileUpdatedUtc = new DateTime(2017, 1, 1),
     //   ImportedBy = "whoever",
     //   SurveyedUtc = new DateTime(2017, 1, 1),
     //   LastActionedUtc = new DateTime(2017, 1, 1),
     //   IsActivated = true
     // };

     // var nhOpImportedFileList = new List<NhOpImportedFile>()
     // {
     //   new NhOpImportedFile()
     //   {
     //     ProjectUid = importedFileProject.ProjectUid,
     //     LegacyProjectId = new Random().Next(100000, 1999999),
     //     // ignore CG ImportedFileId 
     //     // ignore CG LegacyCustomerId
     //     CustomerUid = importedFileProject.CustomerUid,
     //     ImportedFileType = importedFileProject.ImportedFileType,
     //     Name = importedFileProject.Name,
     //     FileDescriptor = importedFileProject.FileDescriptor,
     //     SurveyedUtc = importedFileProject.SurveyedUtc,
     //     // ignore CG? LastActionedUtc = importedFileProject.LastActionedUtc
     //   }
     //};

     //var countMerged = importedFileHandlerProject.Merge(nhOpImportedFileList);
     //Assert.AreEqual(1, countMerged, "nhOpDb importFile not merged");

     //var countWritten = importedFileHandlerProject.WriteToDb();
     //Assert.AreEqual(1, countWritten, "ProjectDb importFile not written");

     // importedFileHandlerProject.EmptyList();

     // var countRead = importedFileHandlerProject.ReadFromDb();
     // Assert.AreNotEqual(0, countRead, "ProjectDb importFile not read");

     // var listOfProjectFiles = importedFileHandlerProject.List();
     // ImportedFile importFileResponse = listOfProjectFiles.FirstOrDefault(x => x.ProjectUid == importedFileProject.ProjectUid);
     // Assert.IsNotNull(importFileResponse, "should have found the ProjectDb one we just inserted");
    }

    //private int WriteImportedFileToProjectDb(string projectDbConnectionString, ImportedFile importedFile)
    //{
    //  var dbConnection = new MySqlConnection(projectDbConnectionString);
    //  dbConnection.Open();

    //  var insertCommand = string.Format(
    //    "INSERT ImportedFile " +
    //    "    (fk_ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, IsDeleted, IsActivated, LastActionedUTC) " +
    //    "  VALUES " +
    //    "    (@ProjectUid, @ImportedFileUid, @ImportedFileId, @CustomerUid, @ImportedFileType, @Name, @FileDescriptor, @FileCreatedUTC, @FileUpdatedUTC, @ImportedBy, @SurveyedUtc, 0, 1, @LastActionedUtc)");

    //  int insertedCount = dbConnection.Execute(insertCommand, importedFile);
    //  return insertedCount;
    //}
  }
}
