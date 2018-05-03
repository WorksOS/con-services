using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Productivity3D.Scheduler.Common.Controller;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.Productivity3D.Scheduler.Common.Repository;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.MasterData.Models.Models;

namespace SchedulerTestsImportedFileSync
{
  [TestClass]
  public class ImportedFileSynchronizerTests : TestControllerBase
  {
    private ILogger _log;
    private string _projectDbConnectionString;
    private string _nhOpDbConnectionString;

    private long _fixedLegacyCustomerId = 9999999;
    private long _fixedLegacyProjectId = 1001158;
    private string _fixedCustomerUid = Guid.NewGuid().ToString();
    private string _fixedProjectUid = "ff91dd40-1569-4765-a2bc-014321f76ace";

    [TestInitialize]
    public void Init()
    {
      SetupDi();

      _log = LoggerFactory.CreateLogger<ImportedFileSynchronizerTests>();
      _projectDbConnectionString = ConnectionUtils.GetConnectionStringMySql(ConfigStore, _log, "_PROJECT");
      _nhOpDbConnectionString = ConnectionUtils.GetConnectionStringMsSql(ConfigStore, Log, "NH_OP");

      Assert.IsNotNull(_log, "Log is null");

      //For file types other than SS need fixed customer/project so can donwload from TCC
      if (!NhOpDbCustomerAndProjectExists(_nhOpDbConnectionString, _fixedLegacyCustomerId, _fixedLegacyProjectId))
      {
        var importedFileNhOp = new ImportedFileNhOp
        {
          CustomerUid = _fixedCustomerUid,
          ProjectUid = _fixedProjectUid,
          LegacyCustomerId = _fixedLegacyCustomerId,
          LegacyProjectId = _fixedLegacyProjectId,
          Name = "My test customer"
        };
        WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFileNhOp);
        var importedFileProject = AutoMapperUtility.Automapper.Map<ImportedFileProject>(importedFileNhOp);
        WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFileProject);
      }
    }

    [TestMethod]
    public async Task ImpFileSyncSS_CreatedInProject()
    {
      var importedFileRepoProject = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);

      var importedFileProject = new ImportedFileProject()
      {
        LegacyProjectId = new Random().Next(1, 900000),
        LegacyCustomerId = new Random().Next(1, 9999999),
        ProjectUid = Guid.NewGuid().ToString(),
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyImportedFileId = null,
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "ImportedFileRepoProject_OneFileInProject.TTM",
        // FileDescription: form this from FileSpaceID, CustomerUID, ProjectUID
        FileDescriptor =
          "{ \"filespaceId\":\"u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01\",\"path\":\"/87bdf851-44c5-e311-aa77-00505688274d/62a52e4f-faa2-e511-80e5-0050568821e6\",\"fileName\":\"DesignSVL13072017034205.svl\"}",
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        IsDeleted = false,
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFileProject);
      var importedFileNhOp = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(importedFileProject);
      WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFileNhOp);

      var createdCount = importedFileRepoProject.Create(importedFileProject);
      Assert.AreEqual(1, createdCount, "Project importFile not created");

      var sync = new ImportedFileSynchronizer(ConfigStore, LoggerFactory, RaptorProxy, TPaasProxy, ImpFileProxy, FileRepo, true);
      await sync.SyncTables();

      // now lets see if it synced to NhOp

      // need to read the project again, as it will now have the LegacyImportedFileId
      var importedFileRepoProjectReRead = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);
      var listOfProjectFilesReRead = importedFileRepoProjectReRead.Read(true);
      Assert.AreNotEqual(0, listOfProjectFilesReRead.Count, "project importFile not read");
      ImportedFileProject importFileProjectReReadResponse =
        listOfProjectFilesReRead.FirstOrDefault(x => x.ImportedFileUid == importedFileProject.ImportedFileUid);
      Assert.IsNotNull(importFileProjectReReadResponse, "should have found the importedFile we re-read from Project");

      // ok now lets look at copy in NhOp
      var importedFileRepoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, LoggerFactory);

      var listOfNhOpFiles = importedFileRepoNhOp.Read(true);
      Assert.AreNotEqual(0, listOfNhOpFiles.Count, "NhOpDb importFile not read");
      ImportedFileNhOp importFileResponse =
        listOfNhOpFiles.FirstOrDefault(x => x.LegacyImportedFileId ==
                                            importFileProjectReReadResponse.LegacyImportedFileId);

      Assert.IsNotNull(importFileResponse, "should have found the importedFile we created in Project, synced to NhOp");
      Assert.AreEqual(importedFileProject.LegacyProjectId, importFileResponse.LegacyProjectId,
        "LegacyProjectId not synced to NhOp.");
      Assert.IsTrue(
        (String.Compare(importedFileProject.ProjectUid, importFileResponse.ProjectUid,
           StringComparison.OrdinalIgnoreCase) == 0),
        "ProjectUid not synced to NhOp");
      Assert.AreEqual(importedFileProject.LegacyCustomerId, importFileResponse.LegacyCustomerId,
        "LegacyCustomerId not synced to NhOp.");
      Assert.IsTrue(
        (String.Compare(importedFileProject.CustomerUid, importFileResponse.CustomerUid,
           StringComparison.OrdinalIgnoreCase) == 0),
        "CustomerUid not synced to NhOp");
      Assert.AreEqual(importedFileProject.ImportedFileType, importFileResponse.ImportedFileType,
        "ImportedFileType not synced to NhOp.");
      Assert.AreEqual(importedFileProject.DxfUnitsType, importFileResponse.DxfUnitsType,
        "DxfUnitsType not synced to NhOp.");
      Assert.AreEqual(importedFileProject.SurveyedUtc, importFileResponse.SurveyedUtc,
        "SurveyedUtc not synced to NhOp.");

      Assert.AreEqual(
        ImportedFileUtils.IncludeSurveyedUtcInName(importedFileProject.Name, importedFileProject.SurveyedUtc.Value),
        importFileResponse.Name, "File Name was not sunced to NhOp.");

      Assert.AreEqual(importedFileProject.FileCreatedUtc, importFileResponse.FileCreatedUtc,
        "FileCreatedUtc not synced to NhOp.");
      Assert.AreEqual(importedFileProject.FileUpdatedUtc, importFileResponse.FileUpdatedUtc,
        "FileUpdatedUtc not synced to NhOp.");
      Assert.IsNull(importFileResponse.ImportedBy, "ImportedBy not synced to NhOp.");
    }

    [TestMethod]
    public async Task ImpFileSyncSS_CreatedInProject_DeletedFromProject()
    {
      var importedFileRepoProject = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);

      var importedFileProject = new ImportedFileProject()
      {
        LegacyProjectId = new Random().Next(1, 900000),
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
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFileProject);
      var importedFileNhOp = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(importedFileProject);
      WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFileNhOp);

      var createdCount = importedFileRepoProject.Create(importedFileProject);
      Assert.AreEqual(1, createdCount, "Project importFile not created");

      var sync = new ImportedFileSynchronizer(ConfigStore, LoggerFactory, RaptorProxy, TPaasProxy, ImpFileProxy, FileRepo, true);
      await sync.SyncTables();

      // now lets see if it synced to NhOp

      // need to read the project again, as it will now have the LegacyImportedFileId
      var importedFileRepoProjectReRead = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);
      var listOfProjectFilesReRead = importedFileRepoProjectReRead.Read(true);
      Assert.AreNotEqual(0, listOfProjectFilesReRead.Count, "project importFile not read");
      ImportedFileProject importFileProjectReReadResponse =
        listOfProjectFilesReRead.FirstOrDefault(x => x.ImportedFileUid == importedFileProject.ImportedFileUid);
      Assert.IsNotNull(importFileProjectReReadResponse, "should have found the importedFile we re-read from Project");

      // ok now lets look at copy in NhOp
      var importedFileRepoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, LoggerFactory);

      var listOfNhOpFiles = importedFileRepoNhOp.Read(true);
      Assert.AreNotEqual(0, listOfNhOpFiles.Count, "NhOpDb importFile not read");
      ImportedFileNhOp importFileNhOpResponse =
        listOfNhOpFiles.FirstOrDefault(x => x.LegacyImportedFileId ==
                                            importFileProjectReReadResponse.LegacyImportedFileId);

      Assert.IsNotNull(importFileNhOpResponse,
        "should have found the importedFile we created in Project, synced to NhOp");

      // now delete from Project
      var deletedCount = importedFileRepoProject.Delete(importFileProjectReReadResponse);
      Assert.AreEqual(1, deletedCount, "Project importFile not deleted");

      await sync.SyncTables();

      listOfNhOpFiles = importedFileRepoNhOp.Read(true);
      if (listOfNhOpFiles.Count > 0)
      {
        importFileNhOpResponse =
          listOfNhOpFiles.FirstOrDefault(x => x.LegacyImportedFileId ==
                                              importFileProjectReReadResponse.LegacyImportedFileId);

        Assert.IsNull(importFileNhOpResponse,
          "should NOT have found the importedFile we deleted in Project, synced to NhOp");
      }

      var listOfProjectFiles = importedFileRepoProject.Read(true);
      Assert.AreNotEqual(0, listOfProjectFiles.Count, "project importFile not read");
      ImportedFileProject importFileProjectResponse =
        listOfProjectFiles.FirstOrDefault(x => x.LegacyImportedFileId ==
                                               importFileProjectReReadResponse.LegacyImportedFileId);

      Assert.IsNotNull(importFileProjectResponse, "should have found the importedFile we deleted in Project");
      Assert.IsTrue(importFileProjectResponse.IsDeleted, "should have been marked as deleted in Project");
      Assert.AreEqual(importFileProjectReReadResponse.LegacyImportedFileId,
        importFileProjectResponse.LegacyImportedFileId, "LegacyImportedFileId should still be present after deletion.");
    }

    [TestMethod]
    public async Task ImpFileSyncSS_CreatedInProject_UpdatedInProject()
    {
      var importedFileRepoProject = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);

      var importedFileProject = new ImportedFileProject()
      {
        LegacyProjectId = new Random().Next(1, 900000),
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
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFileProject);
      var importedFileNhOp = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(importedFileProject);
      WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFileNhOp);

      importedFileRepoProject.Create(importedFileProject);

      var sync = new ImportedFileSynchronizer(ConfigStore, LoggerFactory, RaptorProxy, TPaasProxy, ImpFileProxy, FileRepo, true);
      await sync.SyncTables();

      // now lets see if it synced to NhOp

      // need to read the project again, as it will now have the LegacyImportedFileId
      var importedFileRepoProjectReRead = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);
      var listOfProjectFilesReRead = importedFileRepoProjectReRead.Read(true);
      ImportedFileProject importFileProjectReReadResponse =
        listOfProjectFilesReRead.FirstOrDefault(x => x.ImportedFileUid == importedFileProject.ImportedFileUid);
      Assert.IsNotNull(importFileProjectReReadResponse, "should have found the project we updated");

      // now update in Project note only FileCreatedUtc and FileInsertedUtc can be updated
      importFileProjectReReadResponse.FileCreatedUtc = importFileProjectReReadResponse.FileCreatedUtc.AddDays(1)
        .AddMinutes(4);
      importFileProjectReReadResponse.FileUpdatedUtc = importFileProjectReReadResponse.FileUpdatedUtc.AddDays(2)
        .AddMinutes(23);
      importFileProjectReReadResponse.LastActionedUtc = DateTime.UtcNow;
      var updatedCount = importedFileRepoProject.Update(importFileProjectReReadResponse);
      Assert.AreEqual(1, updatedCount, "Project importFile not updated");

      await sync.SyncTables();

      // ok now lets look at copy in NhOp to see that it has the updated file dates
      var importedFileRepoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, LoggerFactory);
      var listOfNhOpFiles = importedFileRepoNhOp.Read(true);
      ImportedFileNhOp importFileNhOpResponse =
        listOfNhOpFiles.FirstOrDefault(x => x.LegacyImportedFileId ==
                                            importFileProjectReReadResponse.LegacyImportedFileId);

      Assert.IsNotNull(importFileNhOpResponse, "should have found the importedFile we updated");
      Assert.AreEqual(importFileProjectReReadResponse.LegacyImportedFileId, importFileNhOpResponse.LegacyImportedFileId,
        "LegacyImportedFileId should still be present after deletion.");
      Assert.AreEqual(importFileProjectReReadResponse.FileCreatedUtc, importFileNhOpResponse.FileCreatedUtc,
        "should have the updated FilecreatedUtc in nh_op");
      Assert.AreEqual(importFileProjectReReadResponse.FileUpdatedUtc, importFileNhOpResponse.FileUpdatedUtc,
        "should have the updated FileUpdatedUtc in nh_op");
    }

    [TestMethod]
    public async Task ImpFileSyncSS_CreatedInNhOp()
    {
      var importedFileRepoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, LoggerFactory);

      var importedFileNhOp = new ImportedFileNhOp()
      {
        LegacyProjectId = new Random().Next(100000, 19999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(1, 19999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = $"ImportedFileRepoProject_NewFileInNhOp {Guid.NewGuid()}.TTM",
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };
      importedFileNhOp.Name =
        ImportedFileUtils.IncludeSurveyedUtcInName(importedFileNhOp.Name, importedFileNhOp.SurveyedUtc.Value);

      WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFileNhOp);
      var importedFileProject = AutoMapperUtility.Automapper.Map<ImportedFileProject>(importedFileNhOp);
      WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFileProject);


      var createdLegacyImportedFileId = importedFileRepoNhOp.Create(importedFileNhOp);
      Assert.IsTrue(createdLegacyImportedFileId > 0, "nhOpDb importFile not created");

      var sync = new ImportedFileSynchronizer(ConfigStore, LoggerFactory, RaptorProxy, TPaasProxy, ImpFileProxy, FileRepo, true);
      await sync.SyncTables();

      // now lets see if it synced to Project
      var haveRetrievedProjectImportedFile = HaveRetrievedProjectImportedFile(_projectDbConnectionString, createdLegacyImportedFileId);
      Assert.IsTrue(haveRetrievedProjectImportedFile, "Project importFile not read");

      var importedFileRepoProject = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);

      var listOfProjectFiles = importedFileRepoProject.Read(true);
      Assert.AreNotEqual(0, listOfProjectFiles.Count, "Project importFile not read");
      ImportedFileProject importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => x.LegacyImportedFileId == createdLegacyImportedFileId);

      Assert.IsNotNull(importFileResponse, "should have found the importedFile we created in NhOp, synced to Project");
      Assert.AreEqual(importedFileNhOp.LegacyProjectId, importFileResponse.LegacyProjectId,
        "LegacyProjectId not synced to Project.");
      Assert.IsTrue(
        (String.Compare(importedFileNhOp.ProjectUid, importFileResponse.ProjectUid,
           StringComparison.OrdinalIgnoreCase) == 0),
        "ProjectUid not synced to Project");
      Assert.AreEqual(importedFileNhOp.LegacyCustomerId, importFileResponse.LegacyCustomerId,
        "LegacyCustomerId not synced to Project.");
      Assert.IsTrue(
        (String.Compare(importedFileNhOp.CustomerUid, importFileResponse.CustomerUid,
           StringComparison.OrdinalIgnoreCase) == 0),
        "CustomerUid not synced to Project");
      Assert.IsNotNull(importFileResponse.ImportedFileUid, "Project should have a valid ImportedFileUid");
      Assert.AreEqual(importedFileNhOp.ImportedFileType, importFileResponse.ImportedFileType,
        "ImportedFileType not synced to Project.");
      Assert.AreEqual(importedFileNhOp.DxfUnitsType, importFileResponse.DxfUnitsType,
        "DxfUnitsType not synced to Project.");
      Assert.AreEqual(importedFileNhOp.SurveyedUtc, importFileResponse.SurveyedUtc,
        "SurveyedUtc not synced to Project.");

      Assert.AreEqual(
        ImportedFileUtils.RemoveSurveyedUtcFromName(importedFileNhOp.Name),
        importFileResponse.Name, "File Name was not sunced to Project.");

      // files imported via CG must during the Lift&Shift point to the customer/project Id locations
      //   it is not till NG supports importing files (during Lift and Shift) that they will be generated at all under UIDs.
      var fileDescriptor = JsonConvert.SerializeObject(FileDescriptor.CreateFileDescriptor(FileSpaceId,
        importFileResponse.LegacyCustomerId.ToString(), importFileResponse.LegacyProjectId.ToString(),
        importFileResponse.Name));
      Assert.AreEqual(fileDescriptor, importFileResponse.FileDescriptor,
        "FileDescriptor not created correctly in Project.");

      Assert.AreEqual(importedFileNhOp.FileCreatedUtc, importFileResponse.FileCreatedUtc,
        "FileCreatedUtc not synced to Project.");
      Assert.AreEqual(importedFileNhOp.FileUpdatedUtc, importFileResponse.FileUpdatedUtc,
        "FileUpdatedUtc not synced to Project.");

      Assert.IsNotNull(importFileResponse.ImportedFileHistory, "ImportedFileHistory was not created.");
      Assert.AreEqual(1, importFileResponse.ImportedFileHistory.ImportedFileHistoryItems.Count,"ImportedFileHistory count is incorrect.");
      Assert.AreEqual(importedFileNhOp.FileCreatedUtc, importFileResponse.ImportedFileHistory.ImportedFileHistoryItems[0].FileCreatedUtc,
        "FileCreatedUtc not synced to ImportedFileHistory.");
      Assert.AreEqual(importedFileNhOp.FileUpdatedUtc, importFileResponse.ImportedFileHistory.ImportedFileHistoryItems[0].FileUpdatedUtc,
        "FileUpdatedUtc not synced to ImportedFileHistory.");

      Assert.AreEqual("", importFileResponse.ImportedBy, "ImportedBy not synced to Project.");
      Assert.IsFalse(importFileResponse.IsDeleted, "IsDeleted not synced to Project.");
    }
    
    [TestMethod]
    public async Task ImpFileSyncSS_CreatedInNhOp_DeletedFromNhOp()
    {
      var importedFileRepoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, LoggerFactory);

      var importedFileNhOp = new ImportedFileNhOp()
      {
        LegacyProjectId = new Random().Next(100000, 19999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(1, 19999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "ImportedFileRepoProject_NewFileInNhOp.TTM",
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };
      importedFileNhOp.Name =
        ImportedFileUtils.IncludeSurveyedUtcInName(importedFileNhOp.Name, importedFileNhOp.SurveyedUtc.Value);

      WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFileNhOp);
      var importedFileProject = AutoMapperUtility.Automapper.Map<ImportedFileProject>(importedFileNhOp);
      WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFileProject);

      var createdLegacyImportedFileId = importedFileRepoNhOp.Create(importedFileNhOp);
      Assert.IsTrue(createdLegacyImportedFileId > 0, "nhOpDb importFile not created");

      var sync = new ImportedFileSynchronizer(ConfigStore, LoggerFactory, RaptorProxy, TPaasProxy, ImpFileProxy, FileRepo, true);
      await sync.SyncTables();

      // now lets see if it synced to Project
      var importedFileRepoProject = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);

      var listOfProjectFiles = importedFileRepoProject.Read(true);
      Assert.AreNotEqual(0, listOfProjectFiles.Count, "Project importFile not read");
      ImportedFileProject importFileResponse =
        listOfProjectFiles.FirstOrDefault(x => x.LegacyImportedFileId == createdLegacyImportedFileId);

      Assert.IsNotNull(importFileResponse, "should have found the importedFile we created in NhOp, synced to Project");


      // now delete from NhOp
      var deletedCount = importedFileRepoNhOp.Delete(importedFileNhOp);
      Assert.AreEqual(2, deletedCount, "NhOp importFile not deleted");

      await sync.SyncTables();

      // now lets see if its deleted in NhOp
      var listOfNhOpFiles = importedFileRepoNhOp.Read(true);
      if (listOfNhOpFiles.Count > 0)
      {
        var importFileResponseNhOp =
          listOfNhOpFiles.FirstOrDefault(x => x.LegacyImportedFileId == createdLegacyImportedFileId);

        Assert.IsNull(importFileResponseNhOp, "should NOT have found the importedFile we deleted in, NhOp");
      }

      // now lets see if its deleted in project
      listOfProjectFiles = importedFileRepoProject.Read(true);
      Assert.AreNotEqual(0, listOfProjectFiles.Count, "project importFile not read");
      ImportedFileProject importFileProjectResponse =
        listOfProjectFiles.FirstOrDefault(x => x.LegacyImportedFileId == createdLegacyImportedFileId);

      Assert.IsNotNull(importFileProjectResponse, "should have found the importedFile we deleted in NhOp");
      Assert.IsTrue(importFileProjectResponse.IsDeleted, "should have been marked as deleted in NhOp");
    }

    [TestMethod]
    public async Task ImpFileSyncSS_CreatedInNhOp_UpdatedInNhOp()
    {
      var importedFileRepoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, LoggerFactory);
      var initialFileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01);
      var initialFileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12);

      var importedFileNhOp = new ImportedFileNhOp()
      {
        LegacyProjectId = new Random().Next(100000, 19999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(1, 19999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "ImportedFileRepoProject_NewFileInNhOp.TTM",
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        FileCreatedUtc = initialFileCreatedUtc,
        FileUpdatedUtc = initialFileUpdatedUtc,
        ImportedBy = "someoneElse@gmail.com",
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };
      importedFileNhOp.Name =
        ImportedFileUtils.IncludeSurveyedUtcInName(importedFileNhOp.Name, importedFileNhOp.SurveyedUtc.Value);

      WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFileNhOp);
      var importedFileProject = AutoMapperUtility.Automapper.Map<ImportedFileProject>(importedFileNhOp);
      WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFileProject);

      var createdLegacyImportedFileId = importedFileRepoNhOp.Create(importedFileNhOp);
      Assert.IsTrue(createdLegacyImportedFileId > 0, "nhOpDb importFile not created");

      var sync = new ImportedFileSynchronizer(ConfigStore, LoggerFactory, RaptorProxy, TPaasProxy, ImpFileProxy, FileRepo, true);
      await sync.SyncTables();

      // now update in NhOp note only FileCreatedUtc and FileInsertedUtc can be updated
      importedFileNhOp.FileCreatedUtc = importedFileNhOp.FileCreatedUtc.AddDays(1).AddMinutes(4);
      importedFileNhOp.FileUpdatedUtc = importedFileNhOp.FileUpdatedUtc.AddDays(2).AddMinutes(23);
      importedFileNhOp.LastActionedUtc = DateTime.UtcNow;
      var updatedCount = importedFileRepoNhOp.Update(importedFileNhOp);
      Assert.AreEqual(1, updatedCount, "NhOp importFile not updated");

      await sync.SyncTables();

      // ok now lets look at copy in project to see that it has the updated file dates
      var importedFileRepoProject = new ImportedFileRepoProject<ImportedFileProject>(ConfigStore, LoggerFactory);
      var listOfProjectFiles = importedFileRepoProject.Read(true);
      ImportedFileProject importFileProjectResponse =
        listOfProjectFiles.FirstOrDefault(x => x.LegacyImportedFileId == importedFileNhOp.LegacyImportedFileId);

      Assert.IsNotNull(importFileProjectResponse, "should have found the importedFile we updated");
      Assert.AreEqual(importedFileNhOp.LegacyImportedFileId, importFileProjectResponse.LegacyImportedFileId,
        "LegacyImportedFileId should still be present after deletion.");
      Assert.AreEqual(importedFileNhOp.FileCreatedUtc, importFileProjectResponse.FileCreatedUtc,
        "should have the updated FilecreatedUtc in project");
      Assert.AreEqual(importedFileNhOp.FileUpdatedUtc, importFileProjectResponse.FileUpdatedUtc,
        "should have the updated FileUpdatedUtc in project");

      Assert.IsNotNull(importFileProjectResponse.ImportedFileHistory, "ImportedFileHistory was not created.");
      Assert.AreEqual(2, importFileProjectResponse.ImportedFileHistory.ImportedFileHistoryItems.Count, "ImportedFileHistory count is incorrect.");
      Assert.AreEqual(initialFileCreatedUtc, importFileProjectResponse.ImportedFileHistory.ImportedFileHistoryItems[0].FileCreatedUtc,
        "The initial FileCreatedUtc not synced to ImportedFileHistory.");
      Assert.AreEqual(initialFileUpdatedUtc, importFileProjectResponse.ImportedFileHistory.ImportedFileHistoryItems[0].FileUpdatedUtc,
        "The initial FileUpdatedUtc not synced to ImportedFileHistory.");
      Assert.AreEqual(importedFileNhOp.FileCreatedUtc, importFileProjectResponse.ImportedFileHistory.ImportedFileHistoryItems[1].FileCreatedUtc,
        "FileCreatedUtc not synced to ImportedFileHistory.");
      Assert.AreEqual(importedFileNhOp.FileUpdatedUtc, importFileProjectResponse.ImportedFileHistory.ImportedFileHistoryItems[1].FileUpdatedUtc,
        "FileUpdatedUtc not synced to ImportedFileHistory.");
    }

    [TestMethod]
    public async Task ImpFileSyncDxf_CreatedInNhOp()
    {
      var importedFileRepoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, LoggerFactory);

      //The legacy customer and project IDs and the file must exist in TCC so we can download it.
      var importedFileNhOp = new ImportedFileNhOp()
      {
        LegacyProjectId = _fixedLegacyProjectId,
        ProjectUid = _fixedProjectUid,
        LegacyCustomerId = _fixedLegacyCustomerId,
        CustomerUid = _fixedCustomerUid,
        ImportedFileType = ImportedFileType.Linework,
        Name = "CERA.bg.dxf",
        SurveyedUtc = null,
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      var importedFileProject = AutoMapperUtility.Automapper.Map<ImportedFileProject>(importedFileNhOp);

      var createdLegacyImportedFileId = importedFileRepoNhOp.Create(importedFileNhOp);
      Assert.IsTrue(createdLegacyImportedFileId > 0, "nhOpDb importFile not created");

      var sync = new ImportedFileSynchronizer(ConfigStore, LoggerFactory, RaptorProxy, TPaasProxy, ImpFileProxy, FileRepo, false);
      await sync.SyncTables();

      //Since we mock the project web api and the mock does nothing, the imported file has not been saved in the Project database.
      //Therefore we cannot check anything here. Above code just exercises the download and imported file proxy.
    }

    public async Task ImportedFileSynchronizer_CreatedInNhOp_NoCustomerProjectRelationship()
    {
      // shouldn't create one in ng unless relationship exists
      var importedFileRepoNhOp = new ImportedFileRepoNhOp<ImportedFileNhOp>(ConfigStore, LoggerFactory);

      var importedFileNhOp = new ImportedFileNhOp()
      {
        LegacyProjectId = new Random().Next(100000, 19999999),
        ProjectUid = Guid.NewGuid().ToString(),
        LegacyCustomerId = new Random().Next(1, 19999999),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "ImportedFileRepoProject_NewFileInNhOp.TTM",
        SurveyedUtc = new DateTime(2016, 12, 15, 10, 23, 01),
        FileCreatedUtc = new DateTime(2017, 1, 2, 10, 23, 01),
        FileUpdatedUtc = new DateTime(2017, 1, 2, 11, 50, 12),
        ImportedBy = "someoneElse@gmail.com",
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };
      importedFileNhOp.Name =
        ImportedFileUtils.IncludeSurveyedUtcInName(importedFileNhOp.Name, importedFileNhOp.SurveyedUtc.Value);

      WriteNhOpDbCustomerAndProject(_nhOpDbConnectionString, importedFileNhOp);
      //var importedFileProject = AutoMapperUtility.Automapper.Map<ImportedFileProject>(importedFileNhOp);
      //WriteToProjectDBCustomerProjectAndProject(_projectDbConnectionString, importedFileProject);


      var createdLegacyImportedFileId = importedFileRepoNhOp.Create(importedFileNhOp);
      Assert.IsTrue(createdLegacyImportedFileId > 0, "nhOpDb importFile not created");

      var sync = new ImportedFileSynchronizer(ConfigStore, LoggerFactory, RaptorProxy, TPaasProxy, ImpFileProxy, FileRepo, true);
      await sync.SyncTables();

      // now lets see if it synced to Project - it shouldn't have
      var haveRetrievedProjectImportedFile = HaveRetrievedProjectImportedFile(_projectDbConnectionString, createdLegacyImportedFileId);
      Assert.IsFalse(haveRetrievedProjectImportedFile, "Project importFile not read");
    }

  }

}
