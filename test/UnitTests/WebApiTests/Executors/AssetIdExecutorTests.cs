using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.TagFileAuth.Service.WebApi.Models;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.TagFileAuth.Service.WebApi.Interfaces;
using VSS.TagFileAuth.Service.WebApi.Executors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VSS.TagFileAuth.Service.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System;
using VSS.TagFileAuth.Service.Repositories.Interfaces;

namespace VSS.TagFileAuth.Service.WebApiTests.Executors
{
  [TestClass]
  public class AssetIdExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    public void GetFactory()
    {
      IRepositoryFactory factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      Assert.IsNotNull(factory, "Unable to retrieve factory from DI");
    }

    [TestMethod]
    public void GetLogger()
    {
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
      Assert.IsNotNull(loggerFactory, "Unable to retrieve loggerFactory from DI");
    }

    [TestMethod]
    public void CanCallAssetIDExecutorNoValidInput()
    {
      GetAssetIdRequest assetIdRequest = new GetAssetIdRequest();
      GetAssetIdResult assetIdResult = new GetAssetIdResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect AssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void CanCallAssetIDExecutorWithRadioSerialNoAssetExists()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 3, "3k45LK");

      GetAssetIdResult assetIdResult = new GetAssetIdResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect AssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void CanCallAssetIDExecutorWithRadioSerialWithManualDeviceType()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "3k45LK");

      GetAssetIdResult assetIdResult = new GetAssetIdResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect AssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void CanCallAssetIDExecutorWithRadioSerial()
    {
      long legacyAssetID = 898989;
      long legacyProjectID = -1;
      int deviceType = 3;
      string radioSerial = "3k45LK";
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectID, deviceType, radioSerial);
      var asset = new CreateAssetEvent
      {
        AssetUID = Guid.NewGuid(),
        LegacyAssetId = legacyAssetID        
      };
      var ttt = serviceProvider.GetRequiredService<IRepositoryFactory>().GetAssetRepository();
      var storeResult = ttt.StoreAsset(asset);
      Assert.IsNotNull(storeResult, "store mock Asset failed");
      Assert.AreEqual(1, storeResult.Result, "unable to store Asset");

      GetAssetIdResult assetIdResult = new GetAssetIdResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(legacyAssetID, result.assetId, "executor returned incorrect AssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }
    
  }
}
