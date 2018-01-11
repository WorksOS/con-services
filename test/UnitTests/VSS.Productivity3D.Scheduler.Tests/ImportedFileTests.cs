using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Controller;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Scheduler.Tests
{
  [TestClass]
  public class ImportedFileTests : BaseTests
  {
    protected ILogger _log;

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [TestMethod]
    public void MapProjectImportedFileToNhOpImportedFile()
    {
      var source = new ImportedFileProject()
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

      ImportedFileNhOp destination = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(source);
      Assert.AreEqual(source.LegacyImportedFileId, destination.LegacyImportedFileId,
        "LegacyImportedFileId has not been mapped correctly");
      Assert.AreEqual(source.LegacyProjectId, destination.LegacyProjectId,
        "LegacyProjectId has not been mapped correctly");
      Assert.AreEqual(0, string.Compare(source.ProjectUid, destination.ProjectUid, StringComparison.OrdinalIgnoreCase), "ProjectUid has not been mapped correctly");
      Assert.AreEqual(source.LegacyCustomerId, destination.LegacyCustomerId,
        "LegacyCustomerId has not been mapped correctly");
      Assert.AreEqual(0, string.Compare(source.CustomerUid, destination.CustomerUid, StringComparison.OrdinalIgnoreCase), "CustomerUid has not been mapped correctly");
      Assert.AreEqual(source.ImportedFileType, destination.ImportedFileType,
        "ImportedFileType has not been mapped correctly");
      Assert.AreEqual(source.DxfUnitsType, destination.DxfUnitsType, "DxfUnitsType has not been mapped correctly");
      Assert.AreEqual(source.Name, destination.Name, "Name has not been mapped correctly");
      Assert.AreEqual(source.SurveyedUtc, destination.SurveyedUtc, "SurveyedUtc has not been mapped correctly");
      Assert.AreEqual(source.FileCreatedUtc, destination.FileCreatedUtc,
        "FileCreatedUtc has not been mapped correctly");
      Assert.AreEqual(source.FileUpdatedUtc, destination.FileUpdatedUtc,
        "FileUpdatedUtc has not been mapped correctly");
      Assert.AreEqual(source.ImportedBy, destination.ImportedBy, "ImportedBy has not been mapped correctly");
      Assert.AreEqual(source.LastActionedUtc, destination.LastActionedUtc,
        "LastActionedUtc has not been mapped correctly");

      // just make a copy
      ImportedFileProject copyOfSource = AutoMapperUtility.Automapper.Map<ImportedFileProject>(source);
      Assert.AreEqual(source.ProjectUid, copyOfSource.ProjectUid, "ProjectUid has not been mapped correctly");
      Assert.AreEqual(source.ImportedFileUid, copyOfSource.ImportedFileUid,
        "ImportedFileUid has not been mapped correctly");
    }

    [TestMethod]
    public void MapNhOpImportedFileToProjectImportedFile()
    {
      var source = new ImportedFileNhOp()
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
        ImportedBy = "someoneElse@gmail.com", // todo NhOp only includes fk_UserId
        LastActionedUtc = new DateTime(2017, 1, 1, 10, 23, 01, 555),
      };

      ImportedFileProject destination = AutoMapperUtility.Automapper.Map<ImportedFileProject>(source);
      Assert.AreEqual(source.LegacyImportedFileId, destination.LegacyImportedFileId,
        "LegacyImportedFileId has not been mapped correctly");
      Assert.AreEqual(source.LegacyProjectId, destination.LegacyProjectId,
        "LegacyProjectId has not been mapped correctly");
      Assert.AreEqual(source.ProjectUid, destination.ProjectUid, "ProjectUid has not been mapped correctly");
      Assert.AreEqual(source.LegacyCustomerId, destination.LegacyCustomerId,
        "LegacyCustomerId has not been mapped correctly");
      Assert.AreEqual(source.CustomerUid, destination.CustomerUid, "CustomerUid has not been mapped correctly");
      Assert.AreEqual(source.ImportedFileType, destination.ImportedFileType,
        "ImportedFileType has not been mapped correctly");
      Assert.AreEqual(source.DxfUnitsType, destination.DxfUnitsType, "DxfUnitsType has not been mapped correctly");
      Assert.AreEqual(source.Name, destination.Name, "Name has not been mapped correctly");
      Assert.AreEqual(source.SurveyedUtc, destination.SurveyedUtc, "SurveyedUtc has not been mapped correctly");
      Assert.AreEqual(source.FileCreatedUtc, destination.FileCreatedUtc,
        "FileCreatedUtc has not been mapped correctly");
      Assert.AreEqual(source.FileUpdatedUtc, destination.FileUpdatedUtc,
        "FileUpdatedUtc has not been mapped correctly");
      Assert.AreEqual(source.ImportedBy, destination.ImportedBy, "ImportedBy has not been mapped correctly");
      Assert.AreEqual(source.LastActionedUtc, destination.LastActionedUtc,
        "LastActionedUtc has not been mapped correctly");

      // just make a copy
      ImportedFileNhOp copyOfSource = AutoMapperUtility.Automapper.Map<ImportedFileNhOp>(source);
      Assert.AreEqual(source.ProjectUid, copyOfSource.ProjectUid, "ProjectUid has not been mapped correctly");
      Assert.AreEqual(source.LegacyImportedFileId, copyOfSource.LegacyImportedFileId,
        "LegacyImportedFileId has not been mapped correctly");
    }

    [TestMethod]
    public void MapNhOpImportedFile_RemoveSurveyedUtcFromName()
    {
      // JB topo southern motorway_2010-11-29T153300Z.TTM   SS=2010-11-29 15:33:00.0000000

      var nhOpName = "JB topo southern motorway_2010-11-29T153300Z.TTM";
      var expectedProjectName = "JB topo southern motorway.TTM";
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.AreEqual(expectedProjectName, projectName, "File name has not been converted correctly");
    }

    [TestMethod]
    public void MapNhOpImportedFile_RemoveSurveyedUtcFromName_DoubleUtc()
    {
      // Aerial Survey 120819_2012-08-19T035400Z_2016-08-16T003724Z.TTM ssUtc=2016-08-16 00:37:24.0000000

      var nhOpName = "Aerial Survey 120819_2012-08-19T035400Z_2016-08-16T003724Z.TTM";
      var expectedProjectName = "Aerial Survey 120819_2012-08-19T035400Z.TTM";
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.AreEqual(expectedProjectName, projectName, "File name has not been converted correctly");
    }

    [TestMethod]
    public void MapNhOpImportedFile_IncludeSurveyedUtcInName()
    {
      // JB topo southern motorway_2010-11-29T153300Z.TTM   SS=2010-11-29 15:33:00.0000000

      var projectName = "JB topo southern motorway.TTM";
      var surveyUtc = new DateTime(2010, 11, 29, 15, 33, 00);
      var expectedNhOpName = "JB topo southern motorway_2010-11-29T153300Z.TTM";
      var nhOpName = ImportedFileUtils.IncludeSurveyedUtcInName(projectName, surveyUtc);

      Assert.AreEqual(expectedNhOpName, nhOpName, "File name has not been converted correctly");
    }

    [TestMethod]
    public void MapNhOpImportedFile_IncludeSurveyedUtcInName_Double()
    {
      // Aerial Survey 120819_2012-08-19T035400Z_2016-08-16T003724Z.TTM ssUtc=2016-08-16 00:37:24.0000000

      var projectName = "Aerial Survey 120819_2012-08-19T035400Z.TTM";
      var surveyUtc = new DateTime(2016, 8, 16, 0, 37, 24);
      var expectedNhOpName = "Aerial Survey 120819_2012-08-19T035400Z_2016-08-16T003724Z.TTM";
      var nhOpName = ImportedFileUtils.IncludeSurveyedUtcInName(projectName, surveyUtc);

      Assert.AreEqual(expectedNhOpName, nhOpName, "File name has not been converted correctly");
    }

    [TestMethod]
    public void MapNhOpImportedFile_RemoveSurveyedUtcFromNameWithUnderscores()
    {
      var nhOpName = "Surveyed_Surface_2010-11-29T153300Z.TTM";
      var expectedProjectName = "Surveyed_Surface.TTM";
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.AreEqual(expectedProjectName, projectName, "File name has not been converted correctly");
    }

    [TestMethod]
    public void MapNhOpImportedFile_RemoveSurveyedUtcFromNameWithNoSurveyedUtc()
    {
      var nhOpName = "Design_Surface.TTM";
      var expectedProjectName = nhOpName;
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.AreEqual(expectedProjectName, projectName, "File name has not been converted correctly");
    }
    public async Task CanGet3DpmBearerTokenMoq()
    {
      var raptorProxy = new Mock<IRaptorProxy>();
      var tPaasProxy = new Mock<ITPaasProxy>();
      var impFileProxy = new Mock<IImportedFileProxy>();
      var fileRepo = new Mock<IFileRepository>();
      var accessToken = "blah";
      raptorProxy.Setup(ps => ps.NotifyImportedFileChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());
      tPaasProxy.Setup(ps => ps.Get3DPmSchedulerBearerToken(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(new TPaasOauthResult() { tPaasOauthRawResult = new TPaasOauthRawResult() { access_token = accessToken } });
      var importFileSync = new ImportedFileSynchronizerBase(_configStore, _logger, raptorProxy.Object, tPaasProxy.Object, impFileProxy.Object, fileRepo.Object);
      var bearer = await importFileSync.Get3DPmSchedulerBearerToken().ConfigureAwait(false);

      Assert.AreEqual(accessToken, bearer, "should have returned a bearer token");
    }

    //[TestMethod]
    //public async Task CanGet3DpmBearerToken()
    //{
    // done as part of acceptance tests  
    //  var raptorProxy = serviceProvider.GetRequiredService<IRaptorProxy>();
    //  var tPaasProxy = serviceProvider.GetRequiredService<ITPaasProxy>();
    //  var importFileSync = new ImportedFileSynchronizerBase(_configStore, _logger, raptorProxy, tPaasProxy);
    //  var bearer = await importFileSync.Get3DPmSchedulerBearerToken().ConfigureAwait(false);

    //  Assert.AreNotEqual(string.Empty, bearer, "should have returned a bearer token");
    //}

  }
}
