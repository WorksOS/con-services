using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.Masterdata;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.TagFileAuth.Service.WebApiModels.Executors;
using Microsoft.Extensions.Logging;

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
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory, loggerFactory.CreateLogger<AssetIdExecutorTests>()).Process(assetIdRequest) as GetAssetIdResult;
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
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory, loggerFactory.CreateLogger<AssetIdExecutorTests>()).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect AssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }
      
  }
}
