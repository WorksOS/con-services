using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using Moq;
using VSS.Common.Exceptions;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace WebApiTests.Executors
{
  [TestClass]
  public class TagFileProcessingErrorV2ExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    public async Task TagFileProcessingErrorV2Executor_tccOrgIdFound()
    {
      string tccOrgId = Guid.NewGuid().ToString();
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = 1;
      string tagFileName = "Machine Name--whatever --161230235959";
      string deviceSerialNumber = null;

      string customerUid = Guid.NewGuid().ToString();

      var tagFileProcessingErrorRequest = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber, 0);
      tagFileProcessingErrorRequest.Validate();
      var customerRepo = new Mock<ICustomerRepository>();
      var customerTccOrg = new CustomerTccOrg() { Name = "theName", CustomerType = VSS.VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer, CustomerUID = customerUid, TCCOrgID = tccOrgId};
      customerRepo.Setup(c => c.GetCustomerWithTccOrg(It.IsAny<string>())).ReturnsAsync(customerTccOrg);

      var producer = new Mock<IKafka>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor =
        RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(
          loggerFactory.CreateLogger<TagFileProcessingErrorV2ExecutorTests>(), configStore, 
          assetRepository, deviceRepository,
          customerRepo.Object, projectRepository, subscriptionRepository, 
          producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "Result should be true");
      Assert.AreEqual(0, result.Code, "Code should be success");
      Assert.AreEqual("success", result.Message, "Message should be: 'success'");
    }

    [TestMethod]
    public async Task TagFileProcessingErrorV2Executor_tccOrgIdNotFound_NoCustomerUid()
    {
      string tccOrgId = Guid.NewGuid().ToString();
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = 1;
      string tagFileName = "Machine Name--whatever --161230235959";
      string deviceSerialNumber = null;

      var tagFileProcessingErrorRequest = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      tagFileProcessingErrorRequest.Validate();
      var customerRepo = new Mock<ICustomerRepository>();

      var producer = new Mock<IKafka>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor =
        RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(
          loggerFactory.CreateLogger<TagFileProcessingErrorV2ExecutorTests>(), configStore, 
          assetRepository, deviceRepository,
          customerRepo.Object, projectRepository, subscriptionRepository, 
          producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "Result should be true");
      Assert.AreEqual(0, result.Code, "Code should be success");
      Assert.AreEqual("success", result.Message, "Message should be: success");
    }

    [TestMethod]
    public async Task TagFileProcessingErrorV2Executor_tccOrgId_CustomerRepoUnavailable()
    {
      string tccOrgId = Guid.NewGuid().ToString();
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = 1;
      string tagFileName = "Machine Name--whatever --161230235959";
      string deviceSerialNumber = null;

      var tagFileProcessingErrorRequest = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      tagFileProcessingErrorRequest.Validate();

      var producer = new Mock<IKafka>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor =
        RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(
          loggerFactory.CreateLogger<TagFileProcessingErrorV2ExecutorTests>(), configStore, 
          assetRepository, deviceRepository,
          customerRepository, projectRepository, subscriptionRepository,
          producer.Object, kafkaTopicName);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() => executor.ProcessAsync(tagFileProcessingErrorRequest));
      Assert.AreEqual(HttpStatusCode.InternalServerError, ex.Code);
      Assert.AreEqual(-3, ex.GetResult.Code);
      Assert.AreEqual("A problem occurred accessing database. Exception: Unable to connect to any of the specified MySQL hosts.", ex.GetResult.Message);
    }

    [TestMethod]
    public async Task TagFileProcessingErrorV2Executor_ProjectUidFound_UsedAsCustomerUid()
    {
      string tccOrgId =null;
      long? legacyAssetId = null;
      int? legacyProjectId = 1234;
      long legacyCustomerId = 567; 
      int error = 1;
      string tagFileName = "Machine Name--whatever --161230235959";
      string deviceSerialNumber = null;

      string customerUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();

      var tagFileProcessingErrorRequest = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      tagFileProcessingErrorRequest.Validate();
  
      var projectRepo = new Mock<IProjectRepository>();
      var project = new Project() { LegacyProjectID = legacyProjectId.Value, ProjectUID = projectUid, Name = "theProjectName", ProjectType = ProjectType.LandFill, CustomerUID = customerUid, LegacyCustomerID = legacyCustomerId};
      projectRepo.Setup(c => c.GetProject(It.IsAny<long>())).ReturnsAsync(project);

      var producer = new Mock<IKafka>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor =
        RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(
          loggerFactory.CreateLogger<TagFileProcessingErrorV2ExecutorTests>(), configStore,
          assetRepository, deviceRepository,
          customerRepository, projectRepo.Object, subscriptionRepository,
          producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "Result should be true");
      Assert.AreEqual(0, result.Code, "Code should be success");
      Assert.AreEqual("success", result.Message, "Message should be: 'success'");
    }

    [TestMethod]
    public async Task TagFileProcessingErrorV2Executor_AssetUidFound_UsedAsCustomerUid()
    {
      string tccOrgId = null;
      long? legacyAssetId = 667;
      int? legacyProjectId = null;
      int error = 1;
      string tagFileName = "Machine Name--whatever --161230235959";
      string deviceSerialNumber = null;

      string customerUid = Guid.NewGuid().ToString();
      string assetUid = Guid.NewGuid().ToString();
      
      var tagFileProcessingErrorRequest = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      tagFileProcessingErrorRequest.Validate();
     
      var assetRepo = new Mock<IAssetRepository>();
      var asset = new Asset() { LegacyAssetID = legacyAssetId.Value, AssetUID = assetUid, Name = "theAssetName", SerialNumber = "assetSerialNumber", AssetType = "SNM940", OwningCustomerUID = customerUid };
      assetRepo.Setup(c => c.GetAsset(It.IsAny<long>())).ReturnsAsync(asset);

      var producer = new Mock<IKafka>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor =
        RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(
          loggerFactory.CreateLogger<TagFileProcessingErrorV2ExecutorTests>(), configStore,
          assetRepo.Object, deviceRepository,
          customerRepository, projectRepository, subscriptionRepository,
          producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "Result should be true");
      Assert.AreEqual(0, result.Code, "Code should be success");
      Assert.AreEqual("success", result.Message, "Message should be: 'success'");
    }

    [TestMethod]
    public async Task TagFileProcessingErrorV2Executor_DeviceSerialNumber_resolved()
    {
      // todo do we need to resolve this at all? maybe if no assetID?
      string tccOrgId = Guid.NewGuid().ToString();
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = 1;
      string tagFileName = "Machine Name--whatever --161230235959";
      string deviceSerialNumber = "IsASNM940_5555";

      string customerUid = Guid.NewGuid().ToString();
      string assetUid = Guid.NewGuid().ToString();
      string deviceUid = Guid.NewGuid().ToString();
      int deviceType = 6; // "SNM940";
      string deviceTypeString = "SNM940";
      string radioSerial = "theRadioSerial9999";

      var tagFileProcessingErrorRequest = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber, 
        deviceType);
      tagFileProcessingErrorRequest.Validate();
      var customerRepo = new Mock<ICustomerRepository>();
      var customerTccOrg = new CustomerTccOrg() { Name = "theName", CustomerType = VSS.VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer, CustomerUID = customerUid, TCCOrgID = tccOrgId };
      customerRepo.Setup(c => c.GetCustomerWithTccOrg(It.IsAny<string>())).ReturnsAsync(customerTccOrg);

      var deviceRepo = new Mock<IDeviceRepository>();
      var assetDeviceIds = new AssetDeviceIds() { DeviceUID = deviceUid, AssetUID = assetUid, OwningCustomerUID = customerUid, DeviceType = deviceTypeString, RadioSerial = radioSerial};
      deviceRepo.Setup(c => c.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDeviceIds);

      var producer = new Mock<IKafka>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor =
        RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(
          loggerFactory.CreateLogger<TagFileProcessingErrorV2ExecutorTests>(), configStore,
          assetRepository, deviceRepo.Object,
          customerRepo.Object, projectRepository, subscriptionRepository,
          producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "Result should be true");
      Assert.AreEqual(0, result.Code, "Code should be success");
      Assert.AreEqual("success", result.Message, "Message should be: 'success'");
    }

    [TestMethod]
    public async Task TagFileProcessingErrorV2Executor_ErrorNumber_resolved()
    {
      string tccOrgId = Guid.NewGuid().ToString();
      long? legacyAssetId = null;
      int? legacyProjectId = null;
      int error = -1;
      string tagFileName = "Machine Name--whatever --161230235959";
      string deviceSerialNumber = null;

      string customerUid = Guid.NewGuid().ToString();

      var tagFileProcessingErrorRequest = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, legacyAssetId, legacyProjectId,
        error, tagFileName,
        deviceSerialNumber);
      tagFileProcessingErrorRequest.Validate();
      var customerRepo = new Mock<ICustomerRepository>();
      var customerTccOrg = new CustomerTccOrg() { Name = "theName", CustomerType = VSS.VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer, CustomerUID = customerUid, TCCOrgID = tccOrgId };
      customerRepo.Setup(c => c.GetCustomerWithTccOrg(It.IsAny<string>())).ReturnsAsync(customerTccOrg);

      var producer = new Mock<IKafka>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor =
        RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(
          loggerFactory.CreateLogger<TagFileProcessingErrorV2ExecutorTests>(), configStore,
          assetRepository, deviceRepository,
          customerRepo.Object, projectRepository, subscriptionRepository,
          producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "Result should be true");
      Assert.AreEqual(0, result.Code, "Code should be success");
      Assert.AreEqual("success", result.Message, "Message should be: 'success'");
    }


  }
}
