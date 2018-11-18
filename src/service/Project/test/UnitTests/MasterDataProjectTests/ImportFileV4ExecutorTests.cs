using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ImportFileV4ExecutorTests : ExecutorBaseTests
  {
    private static string _customerUid;
    private static string _projectUid;
    private static string _userId;
    private static string _userEmailAddress;
    private static long   _legacyProjectId;
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
        ImportedFileTypeId = ImportedFileType.DesignSurface,
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
    public async Task CreateImportedFile_HappyPath()
    {
      // FlowFile uploads the file from client (possibly as a background task via scheduler)
      // Controller uploads file to TCC and/or S3
      //    V2 Note: BCC file has already put the file on TCC.
      //          the controller a) copies within TCC to client project (raptor)
      //                         b) copies locally and hence to S3. (TRex)
      var customHeaders = new Dictionary<string, string>();
      var importedFileUid = Guid.NewGuid();
      var TCCFilePath = "/BC Data/Sites/Chch Test Site";
      var fileName = "MoundRoadlinework.dxf";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);
      var fileCreatedUtc = DateTime.UtcNow.AddHours(-45);
      var fileUpdatedUtc = fileCreatedUtc;

      ImportedFile newImportedFile = new ImportedFile()
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        ImportedFileId = 999,
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileDescriptor.fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor)
      };

      var createImportedFileEvent = new CreateImportedFileEvent
      {
        CustomerUID = Guid.Parse(_customerUid),
        ProjectUID = Guid.Parse(_projectUid),
        ImportedFileUID = importedFileUid,
        ImportedFileType = ImportedFileType.DesignSurface,
        DxfUnitsType = DxfUnitsType.Meters,
        Name = fileDescriptor.fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor),
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = string.Empty,
        SurveyedUTC = null,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      var createImportedFile = CreateImportedFile.CreateACreateImportedFile(Guid.Parse(_projectUid), 
        fileDescriptor.fileName, fileDescriptor,  ImportedFileType.DesignSurface, 
        null, DxfUnitsType.Meters, DateTime.UtcNow.AddHours(-45), DateTime.UtcNow.AddHours(-44));

      var project = new Repositories.DBModels.Project() { CustomerUID = _customerUid, ProjectUID = _projectUid, LegacyProjectID = (int) _legacyProjectId };
      var projectList = new List<Repositories.DBModels.Project>(); projectList.Add(project);

      var importedFilesList = new List<Repositories.DBModels.ImportedFile>(); importedFilesList.Add(newImportedFile);

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
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), It.IsAny<string>(), It.IsAny< ProjectSettingsType>())).ReturnsAsync((ProjectSettings)null);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      var fileRepo = new Mock<IFileRepository>();


      var executor = RequestExecutorContainerFactory
        .Build<CreateImportedFileExecutor>(logger, mockConfigStore.Object, serviceExceptionHandler,
          _customerUid, _userId, _userEmailAddress, customHeaders,
          producer.Object, KafkaTopicName,
          raptorProxy.Object, null, null,
          projectRepo.Object, null, fileRepo.Object);
      await executor.ProcessAsync(createImportedFile);
    }

    [TestMethod]
    public async Task UpdateImportedFile_HappyPath()
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
      var fileName = "MoundRoadlinework.dxf";
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(_fileSpaceId, TCCFilePath, fileName);
      var existingImportedFileList = new List<ImportedFile>();
      ImportedFile existingImportedFile = new ImportedFile()
      {
        ProjectUid = _projectUid,
        ImportedFileUid = importedFileUid.ToString(),
        LegacyImportedFileId = 200000,
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = fileName,
        FileDescriptor = JsonConvert.SerializeObject(fileDescriptor)
      };

      var updateImportedFile = UpdateImportedFile.CreateUpdateImportedFile(
       Guid.Parse(_projectUid), _legacyProjectId, ImportedFileType.DesignSurface,
       null, DxfUnitsType.Meters,
       DateTime.UtcNow.AddHours(-45), DateTime.UtcNow.AddHours(-44),
       fileDescriptor, importedFileUid, importedFileId
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
      var fileRepo = new Mock<IFileRepository>();


      var executor = RequestExecutorContainerFactory
        .Build<UpdateImportedFileExecutor>(logger, mockConfigStore.Object, serviceExceptionHandler,
          _customerUid, _userId, _userEmailAddress, customHeaders,
          producer.Object, KafkaTopicName,
          raptorProxy.Object, null, null,
          projectRepo.Object, null, fileRepo.Object);
      await executor.ProcessAsync(updateImportedFile);
    }
   
  }
}
