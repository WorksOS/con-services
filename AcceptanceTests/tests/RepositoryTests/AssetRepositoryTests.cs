using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using log4netExtensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.GenericConfiguration;
using VSS.Masterdata;
using MasterDataConsumer;
using VSS.Device.Data;
using VSS.Asset.Data;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace RepositoryTests
{
  [TestClass]
  public class AssetRepositoryTests
  {
    IServiceProvider serviceProvider = null;
    DeviceRepository deviceContext = null;
    AssetRepository assetContext = null;

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      serviceCollection.AddSingleton<IRepositoryFactory, RepositoryFactory>();
      serviceProvider = serviceCollection.BuildServiceProvider();

      deviceContext = new DeviceRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      assetContext = new AssetRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
    }

    /// <summary>
    /// This is used in GetAssetId, taking a radioSerial + deviceType and returning an AssetDeviceId class    ///   
    /// </summary>
    [TestMethod]
    public void CanAssociateAnAssetWithDevice()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = 34457644576;
      Guid owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      string deviceType = "woteva";
      var result = CreateAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType);
      var g = deviceContext.GetAssociatedAsset(deviceSerialNumber, deviceType); g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve AssetDevicePlus from CustomerRepo");
      Assert.AreEqual(assetUID.ToString(), g.Result.AssetUID, "AssetUID is incorrect from DeviceRepo");
      Assert.AreEqual(deviceType, g.Result.DeviceType, "DeviceType is incorrect from DeviceRepo");
      Assert.AreEqual(deviceUID.ToString(), g.Result.DeviceUID, "DeviceUID is incorrect from DeviceRepo");
      Assert.AreEqual(legacyAssetId, g.Result.LegacyAssetID, "LegacyAssetID is incorrect from DeviceRepo");
      Assert.AreEqual(owningCustomerUID.ToString(), g.Result.OwningCustomerUID, "OwningCustomerUID is incorrect from DeviceRepo");
      Assert.AreEqual(deviceSerialNumber, g.Result.RadioSerial, "DeviceSerialNumber is incorrect from DeviceRepo");
    }



    //[TestMethod]
    //public void CanCallAssetIDExecutorWithRadioSerial()
    //{
    //  long legacyAssetID = -1;
    //  long legacyProjectID = 4564546456;
    //  int deviceType = 0;
    //  string radioSerial = null;
    //  GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectID, deviceType, radioSerial);
    //  var asset = new CreateAssetEvent
    //  {
    //    AssetUID = Guid.NewGuid(),
    //    LegacyAssetId = legacyAssetID
    //  };
    //  // var ttt = serviceProvider.GetRequiredService<IRepositoryFactory>().GetRepository(IAssetEvent);
    //  var storeResult = assetContext.StoreAsset(asset);
    //  Assert.IsNotNull(storeResult, "store mock Asset failed");
    //  Assert.AreEqual(1, storeResult.Result, "unable to store Asset");

    //  GetAssetIdResult assetIdResult = new GetAssetIdResult();
    //  var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

    //  var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory).Process(assetIdRequest) as GetAssetIdResult;
    //  Assert.IsNotNull(result, "executor returned nothing");
    //  Assert.AreEqual(legacyAssetID, result.assetId, "executor returned incorrect AssetId");
    //  Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    //}

    private bool CreateAssociation(Guid assetUID, long legacyAssetId, Guid owningCustomerUID, Guid deviceUID, string deviceSerialNumber, string deviceType)
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createAssetEvent = new CreateAssetEvent()
      {
        AssetUID = assetUID,
        AssetName = "The asset Name",
        AssetType = "unknown",
        SerialNumber = "3453gg",
        LegacyAssetId = legacyAssetId,
        OwningCustomerUID = owningCustomerUID,
        ActionUTC = actionUTC
      };

      var createDeviceEvent = new CreateDeviceEvent()
      {
        DeviceUID = deviceUID,
        DeviceSerialNumber = deviceSerialNumber,
        DeviceType = deviceType,
        DeviceState = "active",
        ActionUTC = actionUTC
      };

      var associateDeviceAssetEvent = new AssociateDeviceAssetEvent()
      {
        AssetUID = createAssetEvent.AssetUID,
        DeviceUID = createDeviceEvent.DeviceUID,
        ActionUTC = actionUTC
      };

      assetContext.StoreEvent(createAssetEvent).Wait();
      deviceContext.StoreEvent(createDeviceEvent).Wait();
      deviceContext.StoreEvent(associateDeviceAssetEvent).Wait();
      var g = deviceContext.GetAssociatedAsset(createDeviceEvent.DeviceSerialNumber, createDeviceEvent.DeviceType); g.Wait();
      return (g.Result != null ? true : false);
    }
  }

}
 
 