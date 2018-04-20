using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.TCCFileAccess;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class UpsertImportedFileExecutorTests : ExecutorBaseTests
  {
    protected ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();
    private static string _customerUid;
    private static string _projectUid;
    private static string _userId;
    private static string _userEmailAddress;

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      _customerUid = Guid.NewGuid().ToString();
      _projectUid = Guid.NewGuid().ToString();
      _userId = "sdf870789sdf0";
      _userEmailAddress = "someone@whatever.com";
    }


    [TestMethod]
    public async Task UpsertImportedFileExecutorTests_CopyTCCFile()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    public async Task UpsertImportedFileExecutor_HappyPath_Update()
    {
      throw new NotImplementedException();
    }


    [TestMethod]
    public async Task UpsertImportedFileExecutor_HappyPath_Create()
    {
      throw new NotImplementedException();

      var customHeaders = new Dictionary<string, string>();

      var project = new Repositories.DBModels.Project() { CustomerUID = _customerUid, ProjectUID = _projectUid };

      var importedFileUid = Guid.NewGuid().ToString();
      var existingImportedFileList = new List<ImportedFile>();
      ImportedFile existingImportedFile = new ImportedFile() {ImportedFileUid = importedFileUid};

      var importedFileUpsertEvent = new ImportedFileUpsertEvent()
      {
        Project = project,
        ImportedFileTypeId = ImportedFileType.DesignSurface,
        SurveyedUtc = null,
        DxfUnitsTypeId = DxfUnitsType.Meters,
        FileCreatedUtc = DateTime.UtcNow.AddHours(-45),
        FileUpdatedUtc = DateTime.UtcNow.AddHours(-44),
        ImportedFileInTcc = FileDescriptor.CreateFileDescriptor("u3bdc38d-1afe-470e-8c1c-fc241d4c5e01", "/BC Data/Sites/Chch Test Site", "CTCTSITECAL.dc")
      };

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();

      var raptorProxy = new Mock<IRaptorProxy>();
     
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateProjectEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateImportedFileEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>())).ReturnsAsync(new Repositories.DBModels.Project() { LegacyProjectID = 999 });
      projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(existingImportedFileList);
      projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(existingImportedFile);
      var fileRepo = new Mock<IFileRepository>();
      

      var executor = RequestExecutorContainerFactory
        .Build<UpsertImportedFileExecutor>(logger, configStore, serviceExceptionHandler,
          _customerUid, _userId, _userEmailAddress, customHeaders,
          producer.Object, kafkaTopicName,
          null, raptorProxy.Object, null,
          projectRepo.Object, null, fileRepo.Object);
      await executor.ProcessAsync(importedFileUpsertEvent);
    }
  }
}
