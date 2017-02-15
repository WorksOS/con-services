using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.TagFileAuth.Service.WebApi.Models;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.TagFileAuth.Service.WebApi.Interfaces;
using VSS.TagFileAuth.Service.WebApi.Executors;
using VSS.TagFileAuth.Service.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

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
    public void CanCallAssetIDExecutorWithRadioSerialWithRadioSerial()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest( -1, 3, "3k45LK" );

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
    public void CanCallAssetIDExecutorWithRadioSerialWithProjectId()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(345345345345, -1, null);

      GetAssetIdResult assetIdResult = new GetAssetIdResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect AssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }
    
  }
}
