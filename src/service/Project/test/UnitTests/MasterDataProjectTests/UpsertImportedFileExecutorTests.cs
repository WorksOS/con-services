using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class UpsertImportedFileExecutorTests : ExecutorBaseTests
  {
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
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = "u710e3466-1d47-45e3-87b8-81d1127ed4ed",
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
        logger.CreateLogger<UpsertImportedFileExecutorTests>(), serviceExceptionHandler, fileRepo.Object).ConfigureAwait(false);
    }

    // todoJeannie

    //[TestMethod]
    //public async Task UpsertImportedFileExecutor_HappyPath_Create()
    //{
    //  var customHeaders = new Dictionary<string, string>();

    //  var project = new Repositories.DBModels.Project() { CustomerUID = _customerUid, ProjectUID = _projectUid };

    //  var importedFileUid = Guid.NewGuid().ToString();
    //  var existingImportedFileList = new List<ImportedFile>();
    //  ImportedFile existingImportedFile = new ImportedFile() {ImportedFileUid = importedFileUid, ImportedFileId = 200000};

    //  var importedFileUpsertEvent = UpdateImportedFile.CreateImportedFileUpsertEvent(
    //   project,ImportedFileType.DesignSurface, null, DxfUnitsType.Meters,
    //   DateTime.UtcNow.AddHours(-45), DateTime.UtcNow.AddHours(-44),
    //   FileDescriptor.CreateFileDescriptor("u3bdc38d-1afe-470e-8c1c-fc241d4c5e01", "/BC Data/Sites/Chch Test Site", "CTCTSITECAL.dc")
    //  );

    //  var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
    //  var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
    //  var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
    //  var producer = new Mock<IKafka>();
    //  producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
    //  producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));
      
    //  var raptorProxy = new Mock<IRaptorProxy>();
    //  var raptorAddFileResult = new AddFileResult(ContractExecutionStatesEnum.ExecutedSuccessfully, ContractExecutionResult.DefaultMessage);
    //  raptorProxy.Setup(rp => rp.AddFile(It.IsAny<Guid>(), It.IsAny<ImportedFileType>(), It.IsAny<Guid>(),
    //    It.IsAny<string>(), It.IsAny<long>(), It.IsAny<DxfUnitsType>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(raptorAddFileResult);
    //  var projectRepo = new Mock<IProjectRepository>();
    //  projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateProjectEvent>())).ReturnsAsync(1);
    //  projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateImportedFileEvent>())).ReturnsAsync(1);
    //  projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateImportedFileEvent>())).ReturnsAsync(1);
    //  projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>())).ReturnsAsync(new Repositories.DBModels.Project() { LegacyProjectID = 999 });
    //  projectRepo.Setup(pr => pr.GetImportedFiles(It.IsAny<string>())).ReturnsAsync(existingImportedFileList);
    //  projectRepo.Setup(pr => pr.GetImportedFile(It.IsAny<string>())).ReturnsAsync(existingImportedFile);
    //  var fileRepo = new Mock<IFileRepository>();
      

    //  var executor = RequestExecutorContainerFactory
    //    .Build<UpdateImportedFileExecutor>(logger, configStore, serviceExceptionHandler,
    //      _customerUid, _userId, _userEmailAddress, customHeaders,
    //      producer.Object, KafkaTopicName,
    //      raptorProxy.Object, null, null,
    //      projectRepo.Object, null, fileRepo.Object);
    //  await executor.ProcessAsync(importedFileUpsertEvent);
    //}
  }
}
