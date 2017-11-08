using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System.Linq;

namespace SchedulerTests
{
  [TestClass]
  public class ImportedFilesHandlerNhOpTests : TestControllerBase
  {
    private ILogger _log;
    private string _nhOpDbConnectionString;

    [TestInitialize]
    public void Init()
    {
      SetupDi();

      _log = LoggerFactory.CreateLogger<ImportedFilesHandlerNhOpTests>();
      _nhOpDbConnectionString = ConnectionUtils.GetConnectionStringMsSql(ConfigStore, Log, "NH_OP");

      Assert.IsNotNull(_log, "Log is null");
    }

    [TestMethod]
    public void ImportedFilesHandlerNhOp_OneFileIn()
    {
      var importedFileHandlerNhOp = new ImportedFileHandlerNhOp<NhOpImportedFile>(ConfigStore, LoggerFactory);

      var importedFile = new NhOpImportedFile()
      {
        LegacyImportedFileId = new Random().Next(100000, 1999999),
        LegacyProjectId = new Random().Next(100000, 1999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(100000, 1999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        DxfUnitsType = DxfUnitsType.UsSurveyFeet,
        Name = "JB topo southern motorway_2010-11-29T153300Z.TTM",
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var insertedCount = WriteNhOpDbCustomerProject(_nhOpDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been customerProject written to NhOpDb");

      insertedCount = WriteNhOpDbImportedFileAndHistory(_nhOpDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been ImportedFile written to NhOpDb");

      var listOfNhOpFiles = importedFileHandlerNhOp.Read();
      Assert.AreNotEqual(0, listOfNhOpFiles.Count, "should have been at least 1 file read from NhOpDb");
   
      NhOpImportedFile importFileResponse = listOfNhOpFiles.FirstOrDefault(x => x.LegacyImportedFileId == importedFile.LegacyImportedFileId);
      Assert.IsNotNull(importFileResponse, "should have the one we tried to inserted");
      Assert.IsTrue((String.Compare(importFileResponse.ProjectUid, importedFile.ProjectUid, StringComparison.OrdinalIgnoreCase) == 0), 
        "unable to find the ProjectDb file we just inserted");
      Assert.AreEqual(importedFile.LegacyImportedFileId, importFileResponse.LegacyImportedFileId, "should have returned the legacy ID");
      Assert.AreEqual(importedFile.Name, importFileResponse.Name, "should have returned the name");
    }

    [TestMethod]
    public void ImportedFilesHandlerNhOp_OneFileIn_WrongFileType()
    {
      var importedFileHandlerNhOp = new ImportedFileHandlerNhOp<NhOpImportedFile>(ConfigStore, LoggerFactory);

      var importedFile = new NhOpImportedFile()
      {
        LegacyImportedFileId = new Random().Next(100000, 1999999),
        LegacyProjectId = new Random().Next(100000, 1999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(100000, 1999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.Alignment,
        DxfUnitsType = DxfUnitsType.UsSurveyFeet,
        Name = "JB topo southern motorway_2010-11-29T153300Z.TTM",
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var insertedCount = WriteNhOpDbCustomerProject(_nhOpDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been customerETc written to NhOpDb");

      insertedCount = WriteNhOpDbImportedFileAndHistory(_nhOpDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been ImportedFile written to NhOpDb");

      var listOfProjectFiles = importedFileHandlerNhOp.Read();
      NhOpImportedFile importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => x.LegacyImportedFileId == importedFile.LegacyImportedFileId);
      Assert.IsNull(importFileResponse, "should not find the invalid one we tried to inserted");

    }

    
    [TestMethod]
    public void ImportedFilesHandlerNhOp_Create()
    {
      var importedFileHandlerNhOp = new ImportedFileHandlerNhOp<NhOpImportedFile>(ConfigStore, LoggerFactory);

      var importedFile = new NhOpImportedFile()
      {
        LegacyImportedFileId = -1,
        LegacyProjectId = new Random().Next(100000, 1999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(100000, 1999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        DxfUnitsType = DxfUnitsType.UsSurveyFeet,
        Name = "JB topo southern motorway_2010-11-29T153300Z.TTM",
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var insertedCount = WriteNhOpDbCustomerProject(_nhOpDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been customerProject written to NhOpDb");

      var createdLegacyImportedFileId = importedFileHandlerNhOp.Create(importedFile);
      Assert.IsTrue(createdLegacyImportedFileId > 0, "should return the Identity field from ImportedFile written to NhOpDb");
      importedFile.LegacyImportedFileId = createdLegacyImportedFileId;

      var listOfNhOpFiles = importedFileHandlerNhOp.Read();
      Assert.AreNotEqual(0, listOfNhOpFiles.Count, "should have been at least 1 file read from NhOpDb");
      NhOpImportedFile importFileResponse = listOfNhOpFiles.FirstOrDefault(x => x.LegacyImportedFileId == importedFile.LegacyImportedFileId);
      Assert.IsNotNull(importFileResponse, "should have the one we tried to inserted");
      Assert.IsTrue((String.Compare(importFileResponse.ProjectUid, importedFile.ProjectUid, StringComparison.OrdinalIgnoreCase) == 0),
        "unable to find the ProjectDb file we just inserted");
      Assert.AreEqual(importedFile.LegacyImportedFileId, importFileResponse.LegacyImportedFileId, "should have returned the legacy ID");
      Assert.AreEqual(importedFile.Name, importFileResponse.Name, "should have returned the name");
    }

    [TestMethod]
    public void ImportedFilesHandlerNhOp_Delete()
    {
      var importedFileHandlerNhOp = new ImportedFileHandlerNhOp<NhOpImportedFile>(ConfigStore, LoggerFactory);

      var importedFile = new NhOpImportedFile()
      {
        LegacyImportedFileId = -1,
        LegacyProjectId = new Random().Next(100000, 1999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(100000, 1999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        DxfUnitsType = DxfUnitsType.UsSurveyFeet,
        Name = "JB topo southern motorway_2010-11-29T153300Z.TTM",
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var insertedCount = WriteNhOpDbCustomerProject(_nhOpDbConnectionString, importedFile);
      Assert.AreEqual(1, insertedCount, "should have been customerProject written to NhOpDb");

      var createdLegacyImportedFileId = importedFileHandlerNhOp.Create(importedFile);
      Assert.IsTrue(createdLegacyImportedFileId > 0, "should return the Identity field from ImportedFile written to NhOpDb");
      importedFile.LegacyImportedFileId = createdLegacyImportedFileId;

      var listOfNhOpFiles = importedFileHandlerNhOp.Read();
      Assert.AreNotEqual(0, listOfNhOpFiles.Count, "should have been at least 1 file read from NhOpDb");

      NhOpImportedFile importFileResponse = listOfNhOpFiles.FirstOrDefault(x => x.LegacyImportedFileId == importedFile.LegacyImportedFileId);
      Assert.IsNotNull(importFileResponse, "should have the one we tried to inserted");
      Assert.IsTrue((String.Compare(importFileResponse.ProjectUid, importedFile.ProjectUid, StringComparison.OrdinalIgnoreCase) == 0),
        "unable to find the ProjectDb file we just inserted");
      Assert.AreEqual(importedFile.LegacyImportedFileId, importFileResponse.LegacyImportedFileId, "should have returned the legacy ID");
      Assert.AreEqual(importedFile.Name, importFileResponse.Name, "should have returned the name");

      var deletedCount = importedFileHandlerNhOp.Delete(importedFile);
      Assert.IsTrue(deletedCount > 1, "nhOpDb importFile and history not deleted");

      listOfNhOpFiles = importedFileHandlerNhOp.Read();
      importFileResponse = listOfNhOpFiles.FirstOrDefault(x => x.LegacyImportedFileId == importedFile.LegacyImportedFileId);
      Assert.IsNull(importFileResponse, "should no longer find the one we created");

    }
  }
}
