using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using log4netExtensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.GenericConfiguration;
using VSS.Masterdata;
using MasterDataConsumer;

namespace RepositoryTests
{
  [TestClass]
  public class AssetRepositoryTests
  {
    public IServiceProvider serviceProvider = null;

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
    }

    /// <summary>
    /// Happy path asset, device and assetDevice exist
    /// </summary>
    [TestMethod]
    public void GetAssetCurrentlyAssociatedWithDevice_HappyPath()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var assetEvent = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "AnAssetName",
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "J82", // looks like we only get the code, not the full desc 'JLG INDUSTRIES, INC'
        OwningCustomerUID = Guid.NewGuid(),
        ActionUTC = firstCreatedUTC
      };

      var deviceEvent = new CreateDeviceEvent()
      {
        DeviceUID = Guid.NewGuid(),
        DeviceSerialNumber = "R89agR1",
        DeviceType = "SNM940"
      };

      var deviceAssetEvent = new AssociateDeviceAssetEvent()
      {
        DeviceUID = deviceEvent.DeviceUID,
        AssetUID = assetEvent.AssetUID
      };

      //var expectedAssetDevice = new AssetDevice
      //{
      //  AssetUid = assetEvent.AssetUID.ToString(),
      //  LegacyAssetId = assetEvent.LegacyAssetId,
      //  OwningCustomerUid = assetEvent.OwningCustomerUID.ToString(),
      //  DeviceUid = deviceEvent.DeviceUID.ToString(),
      //  DeviceType = deviceEvent.DeviceType,
      //  RadioSerial = deviceEvent.DeviceSerialNumber
      //};

      //var assetContext = new AssetRepository(serviceProvider.GetService<IConfigurationStore>().GetConnectionString("VSPDB"), serviceProvider.GetService<ILoggerFactory>());
      //var deviceContext = new DeviceRepository(serviceProvider.GetService<IConfigurationStore>().GetConnectionString("VSPDB"), serviceProvider.GetService<ILoggerFactory>());

      //var a = assetContext.StoreAsset(assetEvent);
      //a.Wait();
      //Assert.AreEqual(1, a.Result, "Asset event not written");

      //var g = deviceContext.GetAssociatedAsset(deviceEvent.DeviceSerialNumber, deviceEvent.DeviceType);
      //g.Wait();
      //Assert.IsNull(g.Result, "Device shouldn't be there yet");

      //var d = deviceContext.StoreDevice(deviceEvent);
      //d.Wait();
      //Assert.AreEqual(1, d.Result, "Device event not written");

      //g = deviceContext.GetAssociatedAsset(deviceEvent.DeviceSerialNumber, deviceEvent.DeviceType);
      //g.Wait();
      //Assert.IsNull(g.Result, "Device shouldn't be there yet");

      //d = deviceContext.StoreDevice(deviceAssetEvent);
      //d.Wait();
      //Assert.AreEqual(1, d.Result, "DeviceAssetAssociation event not written");

      //g = deviceContext.GetAssociatedAsset(deviceEvent.DeviceSerialNumber, deviceEvent.DeviceType);
      //g.Wait();
      //Assert.IsNotNull(g.Result, "DeviceAssetAssociation should be there");
      //Assert.AreEqual(expectedAssetDevice, g.Result, "DeviceAsset retrieved not as expected");
    }


    //[TestMethod]
    //public void CanCallAssetIDExecutorWithProjectId()
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
    //  var ttt = serviceProvider.GetRequiredService<IRepositoryFactory>().GetAssetRepository();
    //  var storeResult = ttt.StoreAsset(asset);
    //  Assert.IsNotNull(storeResult, "store mock Asset failed");
    //  Assert.AreEqual(1, storeResult.Result, "unable to store Asset");

    //  GetAssetIdResult assetIdResult = new GetAssetIdResult();
    //  var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

    //  var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory).Process(assetIdRequest) as GetAssetIdResult;
    //  Assert.IsNotNull(result, "executor returned nothing");
    //  Assert.AreEqual(legacyAssetID, result.assetId, "executor returned incorrect AssetId");
    //  Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    //}

  }

}
 
 