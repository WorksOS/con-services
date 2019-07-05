using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Pegasus.Client;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;
using VSS.TCCFileAccess;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.ProjectTests.Executors
{
  [TestClass]
  public class ImportFileV4ExecutorTests : ExecutorBaseTests
  {
    private static string _customerUid;
    private static string _projectUid;
    private static string _userId;
    private static string _userEmailAddress;
    private static long _legacyProjectId;
    private static string _fileSpaceId;

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      _customerUid = Guid.NewGuid().ToString();
      _projectUid = Guid.NewGuid().ToString();
      _userId = "sdf870789sdf0";
      _userEmailAddress = "someone@whatever.com";
      _legacyProjectId = 111;
      _fileSpaceId = "u710e3466-1d47-45e3-87b8-81d1127ed4ed";
    }


    [TestMethod]
    public async Task CopyTCCFile()
    {
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = _fileSpaceId,
        Name = "MoundRoadlinework.dxf",
        Path = "/BC Data/Sites/Chch Test Site/Designs/Mound Road",
        ImportedFileTypeId = ImportedFileType.Linework,
        CreatedUtc = DateTime.UtcNow
      };

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(fr => fr.FolderExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
      fileRepo.Setup(fr => fr.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

      await TccHelper.CopyFileWithinTccRepository(importedFileTbc,
        _customerUid, Guid.NewGuid().ToString(), "f9sdg0sf9",
        logger.CreateLogger<ImportFileV4ExecutorTests>(), serviceExceptionHandler, fileRepo.Object).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task CreateImportedFile_RaptorHappyPath()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new Dictionary<string, string>();
      var importedFileUid = Guid.NewGuid();
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);
      var fileCreatedUtc = DateTime.UtcNow.AddHours(-45);
      var fileUpdatedUtc = fileCreatedUtc;

      var newImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        ImportedFileId = 999,
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor)
      };

      _ = new CreateImportedFileEvent
      {
        CustomerUID = Guid.Parse(_customerUid),
        ProjectUID = Guid.Parse(_projectUid),
        ImportedFileUID = importedFileUid,
        ImportedFileType = ImportedFileType.DesignSurface,
        DxfUnitsType = DxfUnitsType.Meters,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = string.Empty,
        SurveyedUTC = null,
        ParentUID = null,
        Offset = 0,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      var createImportedFile = new CreateImportedFile(
        Guid.Parse(_projectUid), fileDescriptor.FileName, fileDescriptor, ImportedFileType.DesignSurface, null, DxfUnitsType.Meters,
        DateTime.UtcNow.AddHours(-45), DateTime.UtcNow.AddHours(-44), "some folder", null, 0);

      var project = new ProjectDatabaseModel() { CustomerUID = _customerUid, ProjectUID = _projectUid, LegacyProjectID = (int)_legacyProjectId };
      var projectList = new List<ProjectDatabaseModel> { project };
      var importedFilesList = new List<ImportedFile> { newImportedFile };
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns("false");
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns("true");

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var raptorProxy = new Mock<IRaptorProxy>();
      var raptorAddFileResult = new AddFileResult(ContractExecutionStatesEnum.ExecutedSuccessfully, ContractExecutionResult.DefaultMessage);
      raptorProxy.Setup(rp => rp.AddFile(It.IsAny<Guid>(), It.IsAny<ImportedFileType>(), It.IsAny<Guid>(),
        It.IsAny<string>(), It.IsAny<long>(), It.IsAny<DxfUnitsType>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(raptorAddFileResult);
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateImportedFileEvent>())).ReturnsAsync(1); // for updating zoom levels
      projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(newImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ProjectSettingsType>())).ReturnsAsync((ProjectSettings)null);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      var fileRepo = new Mock<IFileRepository>();
      var dataOceanClient = new Mock<IDataOceanClient>();
      var authn = new Mock<ITPaaSApplicationAuthentication>();

      var executor = RequestExecutorContainerFactory
        .Build<CreateImportedFileExecutor>(
          logger, mockConfigStore.Object, serviceExceptionHandler, _customerUid, _userId, _userEmailAddress,
          customHeaders, producer.Object, KafkaTopicName, raptorProxy.Object, null, null, null, null,
          projectRepo.Object, null, fileRepo.Object, null, null, dataOceanClient.Object, authn.Object);
      var result = await executor.ProcessAsync(createImportedFile).ConfigureAwait(false) as ImportedFileDescriptorSingleResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Code, "Raptor Create should have been successful");
      Assert.IsNotNull(result.ImportedFileDescriptor, "Raptor Create should have returned single item");
      Assert.AreEqual(_projectUid, result.ImportedFileDescriptor.ProjectUid, "Raptor Create has invalid projectUid");
      Assert.AreEqual(fileDescriptor.FileName, result.ImportedFileDescriptor.Name, "Raptor Create has invalid name");
    }

    [TestMethod]
    public async Task UpdateImportedFile_RaptorHappyPath()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new Dictionary<string, string>();
      var importedFileUid = Guid.NewGuid();
      var importedFileId = 9999;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);

      var existingImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor)
      };
      var importedFilesList = new List<ImportedFile> { existingImportedFile };
      var updateImportedFile = new UpdateImportedFile(
       Guid.Parse(_projectUid), _legacyProjectId, ImportedFileType.DesignSurface,
       null, DxfUnitsType.Meters,
       DateTime.UtcNow.AddHours(-45), DateTime.UtcNow.AddHours(-44),
       fileDescriptor, importedFileUid, importedFileId, "some folder", 0
      );

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns("false");
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns("true");

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var raptorProxy = new Mock<IRaptorProxy>();
      var raptorAddFileResult = new AddFileResult(ContractExecutionStatesEnum.ExecutedSuccessfully, ContractExecutionResult.DefaultMessage);
      raptorProxy.Setup(rp => rp.AddFile(It.IsAny<Guid>(), It.IsAny<ImportedFileType>(), It.IsAny<Guid>(),
        It.IsAny<string>(), It.IsAny<long>(), It.IsAny<DxfUnitsType>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(raptorAddFileResult);
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(existingImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);
      var fileRepo = new Mock<IFileRepository>();
      var dataOceanClient = new Mock<IDataOceanClient>();
      var authn = new Mock<ITPaaSApplicationAuthentication>();

      var executor = RequestExecutorContainerFactory
        .Build<UpdateImportedFileExecutor>(
          logger, mockConfigStore.Object, serviceExceptionHandler, _customerUid, _userId, _userEmailAddress,
          customHeaders, producer.Object, KafkaTopicName, raptorProxy.Object, null, null, null, null,
          projectRepo.Object, null, fileRepo.Object, null, null, dataOceanClient.Object, authn.Object);
      var result = await executor.ProcessAsync(updateImportedFile).ConfigureAwait(false) as ImportedFileDescriptorSingleResult;
      Assert.AreEqual(0, result.Code, "Raptor Update should have been successful");
      Assert.IsNotNull(result.ImportedFileDescriptor, "Raptor Update should have returned single item");
      Assert.AreEqual(_projectUid, result.ImportedFileDescriptor.ProjectUid, "Raptor Update has invalid projectUid");
      Assert.AreEqual(fileDescriptor.FileName, result.ImportedFileDescriptor.Name, "Raptor Update has invalid name");
    }

    [TestMethod]
    public async Task DeleteImportedFile_RaptorHappyPath()
    {
      var customHeaders = new Dictionary<string, string>();
      var importedFileUid = Guid.NewGuid();
      var importedFileId = 9999;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);

      var existingImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        ParentUid = null,
        Offset = 0
      };

      var deleteImportedFile = new DeleteImportedFile(
       Guid.Parse(_projectUid), ImportedFileType.DesignSurface, fileDescriptor,
       importedFileUid, importedFileId, existingImportedFile.LegacyImportedFileId, "some folder"
      );

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns("false");
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns("true");

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var raptorProxy = new Mock<IRaptorProxy>();
      var raptorDeleteFileResult = new BaseDataResult { Code = 0 };
      raptorProxy.Setup(rp => rp.DeleteFile(It.IsAny<Guid>(), It.IsAny<ImportedFileType>(),
        It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<IDictionary<string, string>>()))
       .ReturnsAsync(raptorDeleteFileResult);

      var filterServiceProxy = new Mock<IFilterServiceProxy>();
      filterServiceProxy.Setup(fs => fs.GetFilters(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
       .ReturnsAsync(new List<FilterDescriptor>());

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<DeleteImportedFileEvent>())).ReturnsAsync(1);

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(fr => fr.FileExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
      fileRepo.Setup(fr => fr.DeleteFile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FileExists(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.DeleteFile(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var pegasusClient = new Mock<IPegasusClient>();

      var executor = RequestExecutorContainerFactory
        .Build<DeleteImportedFileExecutor>(
          logger, mockConfigStore.Object, serviceExceptionHandler, _customerUid, _userId, _userEmailAddress,
          customHeaders, producer.Object, KafkaTopicName, raptorProxy.Object, null, null, filterServiceProxy.Object,
          null, projectRepo.Object, null, fileRepo.Object, null, null, dataOceanClient.Object, authn.Object, null, pegasusClient.Object);
      await executor.ProcessAsync(deleteImportedFile);
    }

    [TestMethod]
    public async Task CreateImportedFile_HappyPath_GeoTiff()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new Dictionary<string, string>();
      var importedFileUid = Guid.NewGuid();
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.tif";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);
      var fileCreatedUtc = DateTime.UtcNow.AddHours(-45);
      var fileUpdatedUtc = fileCreatedUtc;

      var newImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        ImportedFileId = 999,
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.GeoTiff,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor)
      };

      _ = new CreateImportedFileEvent
      {
        CustomerUID = Guid.Parse(_customerUid),
        ProjectUID = Guid.Parse(_projectUid),
        ImportedFileUID = importedFileUid,
        ImportedFileType = ImportedFileType.GeoTiff,
        DxfUnitsType = DxfUnitsType.Meters,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = string.Empty,
        SurveyedUTC = null,
        ParentUID = null,
        Offset = 0,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      var createImportedFile = new CreateImportedFile(
        Guid.Parse(_projectUid), fileDescriptor.FileName, fileDescriptor, ImportedFileType.GeoTiff, null, DxfUnitsType.Meters,
        DateTime.UtcNow.AddHours(-45), DateTime.UtcNow.AddHours(-44), "some folder", null, 0);

      var importedFilesList = new List<ImportedFile> { newImportedFile };
      var mockConfigStore = new Mock<IConfigurationStore>();

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var project = new ProjectDatabaseModel() { CustomerUID = _customerUid, ProjectUID = _projectUid, LegacyProjectID = (int)_legacyProjectId };
      var projectList = new List<ProjectDatabaseModel> { project };

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(newImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      var scheduler = new Mock<ISchedulerProxy>();
      scheduler.Setup(s => s.ScheduleVSSJob(It.IsAny<JobRequest>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new ScheduleJobResult());

      var executor = RequestExecutorContainerFactory
        .Build<CreateImportedFileExecutor>(logger, mockConfigStore.Object, serviceExceptionHandler,
          _customerUid, _userId, _userEmailAddress, customHeaders,
          producer.Object, KafkaTopicName,
          null, null, null, null, null,
          projectRepo.Object, schedulerProxy: scheduler.Object);
      var result = await executor.ProcessAsync(createImportedFile).ConfigureAwait(false) as ImportedFileDescriptorSingleResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Code, "Geotiff Create should have been successful");
      Assert.IsNotNull(result.ImportedFileDescriptor, "Geotiff Create should have returned single item");
      Assert.AreEqual(_projectUid, result.ImportedFileDescriptor.ProjectUid, "Geotiff Create has invalid projectUid");
      Assert.AreEqual(fileDescriptor.FileName, result.ImportedFileDescriptor.Name, "Geotiff Create has invalid name");
    }

    [TestMethod]
    public async Task CreateImportedFile_TRexHappyPath_DesignSurface()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new Dictionary<string, string>();
      var importedFileUid = Guid.NewGuid();
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);
      var fileCreatedUtc = DateTime.UtcNow.AddHours(-45);
      var fileUpdatedUtc = fileCreatedUtc;

      var newImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        ImportedFileId = 999,
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor)
      };

      _ = new CreateImportedFileEvent
      {
        CustomerUID = Guid.Parse(_customerUid),
        ProjectUID = Guid.Parse(_projectUid),
        ImportedFileUID = importedFileUid,
        ImportedFileType = ImportedFileType.DesignSurface,
        DxfUnitsType = DxfUnitsType.Meters,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = string.Empty,
        SurveyedUTC = null,
        ParentUID = null,
        Offset = 0,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      var createImportedFile = new CreateImportedFile(
        Guid.Parse(_projectUid), fileDescriptor.FileName, fileDescriptor, ImportedFileType.DesignSurface, null, DxfUnitsType.Meters,
        DateTime.UtcNow.AddHours(-45), DateTime.UtcNow.AddHours(-44), "some folder", null, 0);

      var importedFilesList = new List<ImportedFile> { newImportedFile };
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns("true");
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns("false");

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.AddFile(It.IsAny<DesignRequest>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new ContractExecutionResult());
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(newImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);

      var executor = RequestExecutorContainerFactory
        .Build<CreateImportedFileExecutor>(logger, mockConfigStore.Object, serviceExceptionHandler,
          _customerUid, _userId, _userEmailAddress, customHeaders,
          producer.Object, KafkaTopicName,
          null, null, null, null, tRexImportFileProxy.Object,
          projectRepo.Object);
      var result = await executor.ProcessAsync(createImportedFile).ConfigureAwait(false) as ImportedFileDescriptorSingleResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Code, "Trex Create should have been successful");
      Assert.IsNotNull(result.ImportedFileDescriptor, "Trex Create should have returned single item");
      Assert.AreEqual(_projectUid, result.ImportedFileDescriptor.ProjectUid, "Trex Create has invalid projectUid");
      Assert.AreEqual(fileDescriptor.FileName, result.ImportedFileDescriptor.Name, "Trex Create has invalid name");
    }

    [TestMethod]
    public async Task UpdateImportedFile_TRexHappyPath_DesignSurface()
    {
      var customHeaders = new Dictionary<string, string>();
      var importedFileUid = Guid.NewGuid();
      var importedFileId = 9999;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);

      var existingImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor)
      };
      var importedFilesList = new List<ImportedFile> { existingImportedFile };
      var updateImportedFile = new UpdateImportedFile(
       Guid.Parse(_projectUid), _legacyProjectId, ImportedFileType.DesignSurface, null, DxfUnitsType.Meters, DateTime.UtcNow.AddHours(-45),
       DateTime.UtcNow.AddHours(-44), fileDescriptor, importedFileUid, importedFileId, "some folder", 0
      );

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns("true");
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns("false");

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.UpdateFile(It.IsAny<DesignRequest>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new ContractExecutionResult());
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(existingImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);

      var executor = RequestExecutorContainerFactory
        .Build<UpdateImportedFileExecutor>(logger, mockConfigStore.Object, serviceExceptionHandler,
          _customerUid, _userId, _userEmailAddress, customHeaders,
          producer.Object, KafkaTopicName,
          null, null, null, null, tRexImportFileProxy.Object,
          projectRepo.Object);
      var result = await executor.ProcessAsync(updateImportedFile).ConfigureAwait(false) as ImportedFileDescriptorSingleResult;
      Assert.AreEqual(0, result.Code, "Trex Update should have been successful");
      Assert.IsNotNull(result.ImportedFileDescriptor, "Trex Update should have returned single item");
      Assert.AreEqual(_projectUid, result.ImportedFileDescriptor.ProjectUid, "Trex Update has invalid projectUid");
      Assert.AreEqual(fileDescriptor.FileName, result.ImportedFileDescriptor.Name, "Trex Update has invalid name");
    }

    [TestMethod]
    public async Task DeleteImportedFile_TRexHappyPath_DesignSurface()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new Dictionary<string, string>();
      var importedFileUid = Guid.NewGuid();
      var importedFileId = 9999;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);

      var existingImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        ParentUid = null,
        Offset = 0
      };

      var deleteImportedFile = new DeleteImportedFile(
        Guid.Parse(_projectUid), ImportedFileType.DesignSurface, fileDescriptor,
        importedFileUid, importedFileId, existingImportedFile.LegacyImportedFileId, "some folder"
      );


      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns("true");
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns("false");

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.DeleteFile(It.IsAny<DesignRequest>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new ContractExecutionResult());

      var filterServiceProxy = new Mock<IFilterServiceProxy>();
      filterServiceProxy.Setup(fs => fs.GetFilters(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(new List<FilterDescriptor>());

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<DeleteImportedFileEvent>())).ReturnsAsync(1);

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(fr => fr.FileExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
      fileRepo.Setup(fr => fr.DeleteFile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FileExists(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.DeleteFile(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var pegasusClient = new Mock<IPegasusClient>();

      var executor = RequestExecutorContainerFactory
        .Build<DeleteImportedFileExecutor>(
          logger, mockConfigStore.Object, serviceExceptionHandler, _customerUid, _userId, _userEmailAddress,
          customHeaders, producer.Object, KafkaTopicName, null, null, null, filterServiceProxy.Object,
          tRexImportFileProxy.Object, projectRepo.Object, null, fileRepo.Object, null, null, dataOceanClient.Object, authn.Object, null, pegasusClient.Object);
      await executor.ProcessAsync(deleteImportedFile);
    }

    [TestMethod]
    public async Task CreateImportedFile_TRexHappyPath_ReferenceSurface()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new Dictionary<string, string>();
      var importedFileUid = Guid.NewGuid();
      var parentUid = Guid.NewGuid();
      var offset = 1.5;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);
      var fileCreatedUtc = DateTime.UtcNow.AddHours(-45);
      var fileUpdatedUtc = fileCreatedUtc;

      var newImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        ImportedFileId = 999,
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.ReferenceSurface,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        Offset = offset,
        ParentUid = parentUid.ToString()
      };

      _ = new CreateImportedFileEvent
      {
        CustomerUID = Guid.Parse(_customerUid),
        ProjectUID = Guid.Parse(_projectUid),
        ImportedFileUID = importedFileUid,
        ImportedFileType = ImportedFileType.ReferenceSurface,
        DxfUnitsType = DxfUnitsType.Meters,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = string.Empty,
        SurveyedUTC = null,
        ParentUID = parentUid,
        Offset = offset,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      var createImportedFile = new CreateImportedFile(
        Guid.Parse(_projectUid), fileDescriptor.FileName, fileDescriptor, ImportedFileType.ReferenceSurface, null, DxfUnitsType.Meters,
        DateTime.UtcNow.AddHours(-45), DateTime.UtcNow.AddHours(-44), "some folder", parentUid, offset);

      var importedFilesList = new List<ImportedFile> { newImportedFile };
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns("true");
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns("false");

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.AddFile(It.IsAny<DesignRequest>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new ContractExecutionResult());
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(newImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);

      var executor = RequestExecutorContainerFactory
        .Build<CreateImportedFileExecutor>(logger, mockConfigStore.Object, serviceExceptionHandler,
          _customerUid, _userId, _userEmailAddress, customHeaders,
          producer.Object, KafkaTopicName,
          null, null, null, null, tRexImportFileProxy.Object,
          projectRepo.Object);
      var result = await executor.ProcessAsync(createImportedFile).ConfigureAwait(false) as ImportedFileDescriptorSingleResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Code, "Trex Create should have been successful");
      Assert.IsNotNull(result.ImportedFileDescriptor, "Trex Create should have returned single item");
      Assert.AreEqual(_projectUid, result.ImportedFileDescriptor.ProjectUid, "Trex Create has invalid projectUid");
      Assert.AreEqual(fileDescriptor.FileName, result.ImportedFileDescriptor.Name, "Trex Create has invalid name");
    }

    [TestMethod]
    public async Task CreateImportedFile_TRex_ReferenceSurface_NoParentDesign()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new Dictionary<string, string>();
      var importedFileUid = Guid.NewGuid();
      var parentUid = Guid.NewGuid();
      var offset = 1.5;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);
      var fileCreatedUtc = DateTime.UtcNow.AddHours(-45);
      var fileUpdatedUtc = fileCreatedUtc;

      var newImportedFile = new ImportedFile()
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        ImportedFileId = 999,
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.ReferenceSurface,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        Offset = offset,
        ParentUid = parentUid.ToString()
      };

      _ = new CreateImportedFileEvent
      {
        CustomerUID = Guid.Parse(_customerUid),
        ProjectUID = Guid.Parse(_projectUid),
        ImportedFileUID = importedFileUid,
        ImportedFileType = ImportedFileType.ReferenceSurface,
        DxfUnitsType = DxfUnitsType.Meters,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = string.Empty,
        SurveyedUTC = null,
        ParentUID = parentUid,
        Offset = offset,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      var createImportedFile = new CreateImportedFile(
        Guid.Parse(_projectUid), fileDescriptor.FileName, fileDescriptor, ImportedFileType.ReferenceSurface, null, DxfUnitsType.Meters,
        DateTime.UtcNow.AddHours(-45), DateTime.UtcNow.AddHours(-44), "some folder", parentUid, offset);

      var importedFilesList = new List<ImportedFile> { newImportedFile };
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns("true");
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns("false");

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.AddFile(It.IsAny<DesignRequest>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new ContractExecutionResult());
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetImportedFile(newImportedFile.ImportedFileUid)).ReturnsAsync(newImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFile(parentUid.ToString())).ReturnsAsync((ImportedFile)null);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);

      var executor = RequestExecutorContainerFactory
        .Build<CreateImportedFileExecutor>(logger, mockConfigStore.Object, serviceExceptionHandler,
          _customerUid, _userId, _userEmailAddress, customHeaders,
          producer.Object, KafkaTopicName,
          null, null, null, null, tRexImportFileProxy.Object,
          projectRepo.Object);
      await Assert.ThrowsExceptionAsync<ServiceException>(async () =>
       await executor.ProcessAsync(createImportedFile).ConfigureAwait(false));
    }

    [TestMethod]
    public async Task UpdateImportedFile_TRexHappyPath_ReferenceSurface()
    {
      var customHeaders = new Dictionary<string, string>();
      var importedFileUid = Guid.NewGuid();
      var parentUid = Guid.NewGuid();
      var oldOffset = 1.5;
      var newOffset = 1.5;
      var importedFileId = 9999;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoadlinework.dxf";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);

      var existingImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.ReferenceSurface,
        Name = fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        Offset = oldOffset,
        ParentUid = parentUid.ToString()
      };
      var importedFilesList = new List<ImportedFile> { existingImportedFile };
      var updateImportedFile = new UpdateImportedFile(
       Guid.Parse(_projectUid), _legacyProjectId, ImportedFileType.ReferenceSurface, null, DxfUnitsType.Meters, DateTime.UtcNow.AddHours(-45),
       DateTime.UtcNow.AddHours(-44), fileDescriptor, importedFileUid, importedFileId, "some folder", newOffset
      );

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns("true");
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns("false");

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.UpdateFile(It.IsAny<DesignRequest>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new ContractExecutionResult());
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(existingImportedFile);
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(importedFilesList);

      var executor = RequestExecutorContainerFactory
        .Build<UpdateImportedFileExecutor>(logger, mockConfigStore.Object, serviceExceptionHandler,
          _customerUid, _userId, _userEmailAddress, customHeaders,
          producer.Object, KafkaTopicName,
          null, null, null, null, tRexImportFileProxy.Object,
          projectRepo.Object);
      var result = await executor.ProcessAsync(updateImportedFile).ConfigureAwait(false) as ImportedFileDescriptorSingleResult;
      Assert.AreEqual(0, result.Code, "Trex Update should have been successful");
      Assert.IsNotNull(result.ImportedFileDescriptor, "Trex Update should have returned single item");
      Assert.AreEqual(_projectUid, result.ImportedFileDescriptor.ProjectUid, "Trex Update has invalid projectUid");
      Assert.AreEqual(fileDescriptor.FileName, result.ImportedFileDescriptor.Name, "Trex Update has invalid name");
      Assert.AreEqual(newOffset, result.ImportedFileDescriptor.Offset, "Trex Update has invalid offset");
    }

    [TestMethod]
    public async Task DeleteImportedFile_TRexHappyPath_ReferenceSurface()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new Dictionary<string, string>();
      var importedFileUid = Guid.NewGuid();
      var parentUid = Guid.NewGuid();
      var offset = 1.5;
      var importedFileId = 9999;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);

      var existingImportedFile = new ImportedFile
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.ReferenceSurface,
        Name = fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        ParentUid = parentUid.ToString(),
        Offset = offset
      };

      var deleteImportedFile = new DeleteImportedFile(
        Guid.Parse(_projectUid), ImportedFileType.ReferenceSurface, fileDescriptor,
        importedFileUid, importedFileId, existingImportedFile.LegacyImportedFileId, "some folder"
      );


      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns("true");
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns("false");

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.DeleteFile(It.IsAny<DesignRequest>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new ContractExecutionResult());

      var filterServiceProxy = new Mock<IFilterServiceProxy>();
      filterServiceProxy.Setup(fs => fs.GetFilters(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(new List<FilterDescriptor>());

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<DeleteImportedFileEvent>())).ReturnsAsync(1);

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(fr => fr.FileExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
      fileRepo.Setup(fr => fr.DeleteFile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FileExists(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.DeleteFile(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var pegasusClient = new Mock<IPegasusClient>();

      var executor = RequestExecutorContainerFactory
        .Build<DeleteImportedFileExecutor>(
          logger, mockConfigStore.Object, serviceExceptionHandler, _customerUid, _userId, _userEmailAddress,
          customHeaders, producer.Object, KafkaTopicName, null, null, null, filterServiceProxy.Object,
          tRexImportFileProxy.Object, projectRepo.Object, null, fileRepo.Object, null, null, dataOceanClient.Object, authn.Object, null, pegasusClient.Object);
      await executor.ProcessAsync(deleteImportedFile);
    }

    [TestMethod]
    public async Task DeleteImportedFile_TRex_DesignSurface_WithReferenceSurface()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new Dictionary<string, string>();
      var importedFileUid = Guid.NewGuid();
      var parentUid = Guid.NewGuid();
      var offset = 1.5;
      var importedFileId = 9999;
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoad.ttm";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);

      var referenceImportedFile = new ImportedFile()
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
      };

      var parentImportedFile = new ImportedFile()
      {
        ProjectUid = _projectUid,
        ImportedFileUid = parentUid.ToString(),
        ImportedFileId = 998,
        LegacyImportedFileId = 200001,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileDescriptor.FileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        ParentUid = parentUid.ToString(),
        Offset = offset
      };

      var deleteImportedFile = new DeleteImportedFile(
        Guid.Parse(_projectUid), ImportedFileType.DesignSurface, fileDescriptor,
        importedFileUid, importedFileId, parentImportedFile.LegacyImportedFileId, "some folder"
      );

      var referenceList = new List<ImportedFile> { referenceImportedFile };

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns("true");
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns("false");

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var tRexImportFileProxy = new Mock<ITRexImportFileProxy>();
      tRexImportFileProxy.Setup(tr => tr.DeleteFile(It.IsAny<DesignRequest>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new ContractExecutionResult());

      var filterServiceProxy = new Mock<IFilterServiceProxy>();
      filterServiceProxy.Setup(fs => fs.GetFilters(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(new List<FilterDescriptor>());

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<DeleteImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetReferencedImportedFiles(It.IsAny<string>())).ReturnsAsync(referenceList);

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(fr => fr.FileExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
      fileRepo.Setup(fr => fr.DeleteFile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FileExists(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.DeleteFile(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var pegasusClient = new Mock<IPegasusClient>();

      var executor = RequestExecutorContainerFactory
        .Build<DeleteImportedFileExecutor>(
          logger, mockConfigStore.Object, serviceExceptionHandler, _customerUid, _userId, _userEmailAddress,
          customHeaders, producer.Object, KafkaTopicName, null, null, null, filterServiceProxy.Object,
          tRexImportFileProxy.Object, projectRepo.Object, null, fileRepo.Object, null, null, dataOceanClient.Object, authn.Object, null, pegasusClient.Object);
      await Assert.ThrowsExceptionAsync<ServiceException>(async () =>
        await executor.ProcessAsync(deleteImportedFile).ConfigureAwait(false));
    }
  }
}
