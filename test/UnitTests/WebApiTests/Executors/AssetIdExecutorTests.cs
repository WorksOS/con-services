using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System;
using VSS.TagFileAuth.Service.ResultHandling;
using VSS.TagFileAuth.Service.WebApi.Models;
using VSS.TagFileAuth.Service.WebApi.Interfaces;
using VSS.TagFileAuth.Service.WebApi.Executors;
using VSS.Masterdata;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.TagFileAuth.Service.WebApiTests.Executors
{
  [TestClass]
  public class AssetIdExecutorTests : ExecutorBaseTests
  {
    /****** todo
     *  // needed for TFAS
    public async Task<AssetDevice> GetAssociatedAsset(string radioSerial, string deviceType)
    {
      try
      {
        await PerhapsOpenConnection();
        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          return (await Connection.QueryAsync<AssetDevice>
                  (@"SELECT 
                        AssetUID, LegacyAssetID, OwningCustomerUID, DeviceUid, DeviceType, DeviceSerialNumber AS RadioSerial
                      FROM Device d
                        INNER JOIN AssetDevice ad ON ad.fk_DeviceUID = d.DeviceUID
                        INNER JOIN Asset a ON a.AssetUID = ad.fk_AssetUID
                      WHERE d.DeviceSerialNumber = @radioSerial
                        AND a.IsDeleted = 0
                        AND d.DeviceType LIKE @deviceType"
                      , new { radioSerial, deviceType }
                  )).FirstOrDefault();
        });
      }
      finally
      {
        PerhapsCloseConnection();
      }
    }
    *****/

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
    [Ignore]
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
      var ttt = serviceProvider.GetRequiredService<IRepositoryFactory>().GetRepository<IAssetEvent>();
      var storeResult = ttt.StoreEvent(asset);
      Assert.IsNotNull(storeResult, "store mock Asset failed");
      Assert.AreEqual(1, storeResult.Result, "unable to store Asset");

      GetAssetIdResult assetIdResult = new GetAssetIdResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(legacyAssetID, result.assetId, "executor returned incorrect AssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    [Ignore]
    public void CanCallAssetIDExecutorWithProjectId()
    {
      long legacyAssetID = -1;
      long legacyProjectID = 4564546456;
      int deviceType = 0;
      string radioSerial = null;
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectID, deviceType, radioSerial);
      var asset = new CreateAssetEvent
      {
        AssetUID = Guid.NewGuid(),
        LegacyAssetId = legacyAssetID
      };
      var ttt = serviceProvider.GetRequiredService<IRepositoryFactory>().GetRepository<IAssetEvent>();
      var storeResult = ttt.StoreEvent(asset);
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
