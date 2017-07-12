using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositories;
using RepositoryTests.Internal;
using System;
using VSS.GenericConfiguration;
using VSS.Productivity3D.Repo.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace RepositoryTests
{
  [TestClass]
  public class DeviceRepositoryTests : TestControllerBase
  {
    DeviceRepository deviceContext;
    AssetRepository assetContext;

    [TestInitialize]
    public void Init()
    {
      SetupLogging();

      deviceContext = new DeviceRepository(ServiceProvider.GetService<IConfigurationStore>(), ServiceProvider.GetService<ILoggerFactory>());
      assetContext = new AssetRepository(ServiceProvider.GetService<IConfigurationStore>(), ServiceProvider.GetService<ILoggerFactory>());
    }

    /// <summary>
    /// Happy path i.e. device doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateDevice_HappyPath()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var deviceEvent = new CreateDeviceEvent()
      {
        DeviceUID = Guid.NewGuid(),
        DeviceSerialNumber = "Device radio serial",
        DeviceType = "SNM940",
        DeviceState = "active",
        ActionUTC = firstCreatedUTC
      };

      var device = new Device
      {
        DeviceUID = deviceEvent.DeviceUID.ToString(),
        DeviceSerialNumber = deviceEvent.DeviceSerialNumber,
        DeviceType = deviceEvent.DeviceType,
        DeviceState = deviceEvent.DeviceState,
        LastActionedUtc = deviceEvent.ActionUTC
      };

      deviceContext.InRollbackTransactionAsync<object>(async o =>
      {
        var g = await deviceContext.GetDevice(device.DeviceUID);
        Assert.IsNull(g, "Device shouldn't be there yet");

        var s = await deviceContext.StoreEvent(deviceEvent);
        Assert.AreEqual(1, s, "Device event not written");

        g = await deviceContext.GetDevice(device.DeviceUID);
        Assert.IsNotNull(g, "Unable to retrieve Device from DeviceRepo");
        Assert.AreEqual(device, g, "Device details are incorrect from DeviceRepo");
        return null;
      }).Wait();
    }


    /// <summary>
    /// ActionUTC is set already to an earlier date
    /// this could have been from a Create or Update - does it matter?
    /// </summary>
    [TestMethod]
    public void UpdateDevice_HappyPath()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var deviceEventCreate = new CreateDeviceEvent()
      {
        DeviceUID = Guid.NewGuid(),
        DeviceSerialNumber = "A radio serial",
        DeviceType = "PL121",
        DeviceState = "active",
        ModuleType = "whatIsModuleType",
        ActionUTC = firstCreatedUTC
      };

      var deviceEventUpdate = new UpdateDeviceEvent()
      {
        DeviceUID = deviceEventCreate.DeviceUID,
        DeviceSerialNumber = "A radio serial changed",
        DeviceType = "PL221",
        DeviceState = "active still",
        ModuleType = "moduleTypeUpdated",
        ActionUTC = firstCreatedUTC.AddMinutes(10)
      };

      var deviceFinal = new Device
      {
        DeviceUID = deviceEventCreate.DeviceUID.ToString(),
        DeviceSerialNumber = deviceEventUpdate.DeviceSerialNumber,
        DeviceType = deviceEventUpdate.DeviceType,
        DeviceState = deviceEventUpdate.DeviceState,
        ModuleType = deviceEventUpdate.ModuleType,
        LastActionedUtc = deviceEventUpdate.ActionUTC
      };

      deviceContext.InRollbackTransactionAsync<object>(async o =>
      {
        await deviceContext.StoreEvent(deviceEventCreate);
        await deviceContext.StoreEvent(deviceEventUpdate);
        var g = await deviceContext.GetDevice(deviceFinal.DeviceUID);
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(deviceFinal, g, "Asset details are incorrect from AssetRepo");
        return null;
      }).Wait();
    }


    /// <summary>
    /// Kafka update can come in only with fields to be updated - doh
    /// </summary>
    [TestMethod]
    public void UpdateDevice_IgnoreNullValues()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var deviceEventCreate = new CreateDeviceEvent()
      {
        DeviceUID = Guid.NewGuid(),
        DeviceSerialNumber = "A radio serial",
        DeviceType = "PL121",
        DeviceState = "active",
        ModuleType = "whatIsModuleType",
        DataLinkType = "DL type",
        DeregisteredUTC = firstCreatedUTC.AddMonths(-1),
        GatewayFirmwarePartNumber = "gwNumber",
        MainboardSoftwareVersion = "MBnumber",
        RadioFirmwarePartNumber = "rnumber",
        ActionUTC = firstCreatedUTC
      };

      var deviceEventUpdate = new UpdateDeviceEvent()
      {
        DeviceUID = deviceEventCreate.DeviceUID,
        DeviceSerialNumber = null,
        DeviceType = null,
        DeviceState = null,
        ModuleType = null,
        DataLinkType = null,
        DeregisteredUTC = null,
        GatewayFirmwarePartNumber = null,
        MainboardSoftwareVersion = null,
        RadioFirmwarePartNumber = null,
        ActionUTC = firstCreatedUTC.AddMinutes(10)
      };

      var deviceFinal = new Device
      {
        DeviceUID = deviceEventCreate.DeviceUID.ToString(),
        DeviceSerialNumber = deviceEventCreate.DeviceSerialNumber,
        DeviceType = deviceEventCreate.DeviceType,
        DeviceState = deviceEventCreate.DeviceState,
        ModuleType = deviceEventCreate.ModuleType,
        DataLinkType = deviceEventCreate.DataLinkType,
        DeregisteredUTC = deviceEventCreate.DeregisteredUTC,
        GatewayFirmwarePartNumber = deviceEventCreate.GatewayFirmwarePartNumber,
        MainboardSoftwareVersion = deviceEventCreate.MainboardSoftwareVersion,
        RadioFirmwarePartNumber = deviceEventCreate.RadioFirmwarePartNumber,
        LastActionedUtc = deviceEventUpdate.ActionUTC
      };

      deviceContext.InRollbackTransactionAsync<object>(async o =>
      {
        await deviceContext.StoreEvent(deviceEventCreate);
        await deviceContext.StoreEvent(deviceEventUpdate);
        var g = await deviceContext.GetDevice(deviceFinal.DeviceUID);
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(deviceFinal, g, "Asset details are incorrect from AssetRepo");
        return null;
      }).Wait();
    }


    /// <summary>
    /// Device exists, with a later ActionUTC
    /// Potentially device has already had an Update applied
    /// </summary>
    [TestMethod]
    public void UpdateDevice_ExistsFromMoreRecentUpdate()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var deviceEventCreate = new CreateDeviceEvent()
      {
        DeviceUID = Guid.NewGuid(),
        DeviceSerialNumber = "A radio serial",
        DeviceType = "PL121",
        DeviceState = "active",
        ActionUTC = firstCreatedUTC
      };

      var deviceEventUpdateEarlier = new UpdateDeviceEvent()
      {
        DeviceUID = deviceEventCreate.DeviceUID,
        DeviceSerialNumber = "A radio serial changed",
        DeviceType = "PL221",
        DeviceState = "active earlier",
        ActionUTC = firstCreatedUTC.AddMinutes(10)
      };

      var deviceEventUpdateLater = new UpdateDeviceEvent()
      {
        DeviceUID = deviceEventCreate.DeviceUID,
        DeviceSerialNumber = "A radio serial changed later",
        DeviceType = "PL221",
        DeviceState = "active later",
        ActionUTC = firstCreatedUTC.AddMinutes(20)
      };

      var deviceFinal = new Device
      {
        DeviceUID = deviceEventCreate.DeviceUID.ToString(),
        DeviceSerialNumber = deviceEventUpdateLater.DeviceSerialNumber,
        DeviceType = deviceEventUpdateLater.DeviceType,
        DeviceState = deviceEventUpdateLater.DeviceState,
        LastActionedUtc = deviceEventUpdateLater.ActionUTC
      };

      deviceContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await deviceContext.StoreEvent(deviceEventCreate);
        s = await deviceContext.StoreEvent(deviceEventUpdateLater);
        s = await deviceContext.StoreEvent(deviceEventUpdateEarlier);

        var g = await deviceContext.GetDevice(deviceFinal.DeviceUID);
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(deviceFinal, g, "Asset details are incorrect from AssetRepo");
        return null;
      }).Wait();
    }


    #region AssociateDeviceWithAsset

    /// <summary>
    ///  AssociateDeviceWithAsset - Happy path
    ///    assoc doesn't exist. 
    /// </summary>
    [TestMethod]
    public void AssociateDeviceWithAsset_HappyPath()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createAssetEvent = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "The asset Name",
        AssetType = "unknown",
        SerialNumber = "3453gg",
        LegacyAssetId = 4357869785,
        OwningCustomerUID = Guid.NewGuid(),
        ActionUTC = actionUTC
      };

      var deviceUID = Guid.NewGuid();
      var createDeviceEvent = new CreateDeviceEvent()
      {
        DeviceUID = deviceUID,
        DeviceSerialNumber = "The radio serial " + deviceUID.ToString(),
        DeviceType = "SNM940",
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
      var s = deviceContext.StoreEvent(associateDeviceAssetEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "DeviceAsset event not written");

      var g = deviceContext.GetAssociatedAsset(createDeviceEvent.DeviceSerialNumber, createDeviceEvent.DeviceType);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve AssetDevicePlus from CustomerRepo");
      Assert.AreEqual(associateDeviceAssetEvent.DeviceUID.ToString(), g.Result.DeviceUID, "DeviceUID is incorrect from DeviceRepo");
      Assert.AreEqual(associateDeviceAssetEvent.AssetUID.ToString(), g.Result.AssetUID, "AssetUID is incorrect from DeviceRepo");
      Assert.AreEqual(createAssetEvent.LegacyAssetId, g.Result.LegacyAssetID, "LegacyAssetID is incorrect from DeviceRepo");
      Assert.AreEqual(createAssetEvent.OwningCustomerUID.ToString(), g.Result.OwningCustomerUID, "OwningCustomerUID is incorrect from DeviceRepo");
    }


    /// <summary>
    ///  DissociateDeviceAsset - Happy path
    ///    assoc exists, delete it
    /// </summary>
    [TestMethod]
    public void DissociateDeviceAssetEvent_HappyPath()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var associateDeviceAssetEvent = new AssociateDeviceAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        DeviceUID = Guid.NewGuid(),
        ActionUTC = actionUTC
      };

      var dissociateDeviceAssetEvent = new DissociateDeviceAssetEvent()
      {
        AssetUID = associateDeviceAssetEvent.AssetUID,
        DeviceUID = associateDeviceAssetEvent.DeviceUID,
        ActionUTC = actionUTC.AddDays(1)
      };

      deviceContext.StoreEvent(associateDeviceAssetEvent).Wait();
      var g = deviceContext.GetAssociatedAsset("RadioSerial", "unknown");
      Assert.IsNull(g.Result, "There should be no DeviceAsset association from DeviceRepo");

      deviceContext.StoreEvent(dissociateDeviceAssetEvent).Wait();
      g = deviceContext.GetAssociatedAsset("RadioSerial", "SNM940");
      Assert.IsNull(g.Result, "There should be no DeviceAsset association from DeviceRepo");
    }

    #endregion

  }

}

