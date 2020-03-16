using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;

namespace WebApiTests.Executors
{
  [TestClass]
  public class AssetIdExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    public void GetLogger()
    {
      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
      Assert.IsNotNull(loggerFactory, "Unable to retrieve loggerFactory from DI");
    }

    [TestMethod]
    public async Task CanCallAssetIDExecutorNoValidInput()
    {
      var assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "");
      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<AssetIdExecutor>(loggerFactory.CreateLogger<AssetIdExecutorTests>(), ConfigStore,
        projectProxy.Object, accountProxy.Object, deviceProxy.Object);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect AssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public async Task CanCallAssetIDExecutorWithRadioSerialWithManualDeviceType()
    {
      var assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "3k45LK");

      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<AssetIdExecutor>(loggerFactory.CreateLogger<AssetIdExecutorTests>(), ConfigStore,
        projectProxy.Object, accountProxy.Object, deviceProxy.Object);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect AssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");      
    }

    [TestMethod]
    [DataRow("snm940Serial", "snm940Serial")]
    [DataRow("snm941Serial", "snm941Serial")]
    [DataRow("snm940Serial", "snm941Serial")]
    [DataRow("ec520Serial",  "ec520Serial")]
    public async Task AssetUidExecutor_GetAssetDevice_HappyPath(string serialNumberRequested, string serialNumberExpected)
    {
      var deviceDataToBeReturned = new DeviceData
      {
        AccountUid = Guid.NewGuid().ToString(),
        DeviceUid = Guid.NewGuid().ToString(),
        SerialNumber = serialNumberExpected
      };

      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<AssetIdExecutorTests>();
      deviceProxy.Setup(d => d.GetDevice(serialNumberRequested)).ReturnsAsync(deviceDataToBeReturned);

      var dataRepository = new DataRepository(logger, ConfigStore, projectProxy.Object, accountProxy.Object, deviceProxy.Object);
     
      var device = await dataRepository.GetDevice(serialNumberRequested);
      Assert.AreEqual(serialNumberExpected, device.SerialNumber);
    }

    [TestMethod]
    [DataRow("snm940Serial")]
    public async Task AssetUidExecutor_GetAssetDevice_UnHappyPath(string serialNumberRequested)
    {
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<AssetIdExecutorTests>();
      deviceProxy.Setup(d => d.GetDevice(serialNumberRequested)).ReturnsAsync((DeviceData)null);

      var dataRepository = new DataRepository(logger, ConfigStore, projectProxy.Object, accountProxy.Object, deviceProxy.Object);

      var device = await dataRepository.GetDevice(serialNumberRequested);
      Assert.IsNull(device);
    }

  }
}
