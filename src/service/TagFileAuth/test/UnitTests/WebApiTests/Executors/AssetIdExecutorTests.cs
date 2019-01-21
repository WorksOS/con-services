using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using Moq;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Repositories.ExtendedModels;

namespace WebApiTests.Executors
{
  [TestClass]
  public class AssetIdExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    public void GetAssetRepository()
    {
      var repo = serviceProvider.GetRequiredService<IRepository<IAssetEvent>>();
      Assert.IsNotNull(repo, "Unable to retrieve asset repo from DI");
    }

    [TestMethod]
    public void GetLogger()
    {
      var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
      Assert.IsNotNull(loggerFactory, "Unable to retrieve loggerFactory from DI");
    }

    [TestMethod]
    public async Task CanCallAssetIDExecutorNoValidInput()
    {
      var assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "");
      var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<AssetIdExecutor>(loggerFactory.CreateLogger<AssetIdExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionRepository);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect AssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public async Task CanCallAssetIDExecutorWithRadioSerialWithManualDeviceType()
    {
      var assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "3k45LK");

      var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<AssetIdExecutor>(loggerFactory.CreateLogger<AssetIdExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionRepository);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect AssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");      
    }

    [TestMethod]
    [DataRow("SNM940", "snm940Serial", "SNM940", "snm940Serial", "SNM940", "snm940Serial")]
    [DataRow("SNM941", "snm941Serial", "SNM941", "snm941Serial", "SNM941", "snm941Serial")]
    [DataRow("SNM940", "snm940Serial", "SNM941", "snm941Serial", "SNM941", "snm941Serial")]
    [DataRow("EC520", "ec520Serial", "EC520", "ec520Serial", "EC520", "ec520Serial")]
    public async Task AssetUidExecutor_GetAssetDevice_HappyPath(
      string deviceTypeRequested, string radioSerialRequested,
      string deviceTypeExisting, string radioSerialExisting,
      string deviceTypeExpected, string radioSerialExpected)
    {
      var assetDeviceIdsToBeReturned = new AssetDeviceIds()
      {
        AssetUID = Guid.NewGuid().ToString(),
        DeviceType = deviceTypeExpected,
        DeviceUID = Guid.NewGuid().ToString(),
        RadioSerial = radioSerialExpected
      };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<AssetIdExecutorTests>();
      Mock<IDeviceRepository> deviceRepo = new Mock<IDeviceRepository>();
      deviceRepo.Setup(d => d.GetAssociatedAsset(radioSerialRequested, deviceTypeRequested)).ReturnsAsync(assetDeviceIdsToBeReturned);

      DataRepository dataRepository = new DataRepository(logger, configStore, null, deviceRepo.Object,
        null, null, null, null, null);
     
      var assetDevice = await dataRepository.LoadAssetDevice(radioSerialRequested, deviceTypeRequested);
      Assert.AreEqual(assetDevice.DeviceType, deviceTypeExpected);
      Assert.AreEqual(assetDevice.RadioSerial, radioSerialExpected);
    }

    [TestMethod]
    [DataRow("SNM940", "snm940Serial")]
    public async Task AssetUidExecutor_GetAssetDevice_UnHappyPath(
      string deviceTypeRequested, string radioSerialRequested)
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<AssetIdExecutorTests>();
      Mock<IDeviceRepository> deviceRepo = new Mock<IDeviceRepository>();
      deviceRepo.Setup(d => d.GetAssociatedAsset(radioSerialRequested, deviceTypeRequested)).ReturnsAsync((AssetDeviceIds)null);

      DataRepository dataRepository = new DataRepository(logger, configStore, null, deviceRepo.Object,
        null, null, null, null, null);

      var assetDevice = await dataRepository.LoadAssetDevice(radioSerialRequested, deviceTypeRequested);
      Assert.IsNull(assetDevice);
    }

  }
}
