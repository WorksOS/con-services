using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
      Assert.IsNotNull(loggerFactory, "Unable to retrieve loggerFactory from DI");
    }

    [TestMethod]
    public async Task CanCallAssetIDExecutorNoValidInput()
    {
      var assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "");

      var executor = RequestExecutorContainer.Build<AssetIdExecutor>(loggerFactory.CreateLogger<AssetIdExecutorTests>(), ConfigStore,
        authorization.Object, cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.shortRaptorAssetId, "executor returned incorrect shortRaptorAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public async Task CanCallAssetIDExecutorWithRadioSerialWithManualDeviceType()
    {
      var assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "3k45LK");

      var deviceData = new DeviceData();
      deviceProxy.Setup(d => d.GetDevice(It.IsAny<string>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(deviceData);

      var executor = RequestExecutorContainer.Build<AssetIdExecutor>(loggerFactory.CreateLogger<AssetIdExecutorTests>(), ConfigStore,
        authorization.Object, cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.shortRaptorAssetId, "executor returned incorrect shortRaptorAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    [DataRow("snm940Serial", "snm940Serial")]
    [DataRow("snm941Serial", "snm941Serial")]
    [DataRow("snm940Serial", "snm941Serial")]
    [DataRow("ec520Serial", "ec520Serial")]
    public async Task AssetUidExecutor_GetAssetDevice_HappyPath(string serialNumberRequested, string serialNumberExpected)
    {
      var deviceDataToBeReturned = new DeviceData()
      {
        CustomerUID = Guid.NewGuid().ToString(),
        DeviceUID = Guid.NewGuid().ToString(),
        SerialNumber = serialNumberExpected
      };

      var logger = loggerFactory.CreateLogger<AssetIdExecutorTests>();
      deviceProxy.Setup(d => d.GetDevice(serialNumberRequested, It.IsAny<HeaderDictionary>())).ReturnsAsync(deviceDataToBeReturned);

      var dataRepository = new DataRepository(authorization.Object, cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);

      var device = await dataRepository.GetDevice(serialNumberRequested);
      Assert.AreEqual(serialNumberExpected, device.SerialNumber);
    }

    [TestMethod]
    [DataRow("snm940Serial")]
    public async Task AssetUidExecutor_GetAssetDevice_UnHappyPath(string serialNumberRequested)
    {
      var logger = loggerFactory.CreateLogger<AssetIdExecutorTests>();
      deviceProxy.Setup(d => d.GetDevice(serialNumberRequested, It.IsAny<HeaderDictionary>())).ReturnsAsync((DeviceData)null);

      var dataRepository = new DataRepository(authorization.Object, cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);

      var device = await dataRepository.GetDevice(serialNumberRequested);
      Assert.IsNull(device);
    }

    [TestMethod]
    [DataRow("snm940Serial", 100, "Unable to locate device by serialNumber in cws")]
    public async Task AssetUidExecutor_GetAssetDevice_DeviceNotValid_UnHappyPath(string serialNumberRequested, int expectedDeviceErrorCode, string expectedMessage)
    {
      var assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "3k45LK");
      var deviceData = new DeviceData { Code = expectedDeviceErrorCode, Message = expectedMessage };
      deviceProxy.Setup(d => d.GetDevice(It.IsAny<string>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(deviceData);

      var executor = RequestExecutorContainer.Build<AssetIdExecutor>(loggerFactory.CreateLogger<AssetIdExecutorTests>(), ConfigStore, authorization.Object,
        cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(0, result.Code);
      Assert.AreEqual("success", result.Message);
    }
  }
}
