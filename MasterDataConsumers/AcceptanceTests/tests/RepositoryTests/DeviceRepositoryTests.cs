using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryTests.Internal;
using System;
using System.Threading;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
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
        DeregisteredUTC = firstCreatedUTC,
        ModuleType = "theModule Type",
        MainboardSoftwareVersion = "mb45",
        RadioFirmwarePartNumber = "rd 567",
        GatewayFirmwarePartNumber = "them or us",
        DataLinkType = "CAT",
        ActionUTC = firstCreatedUTC,
        ReceivedUTC = firstCreatedUTC
      };

      var device = new Device
      {
        DeviceUID = deviceEvent.DeviceUID.ToString(),
        DeviceSerialNumber = deviceEvent.DeviceSerialNumber,
        DeviceType = deviceEvent.DeviceType,
        DeviceState = deviceEvent.DeviceState,
        DeregisteredUTC = deviceEvent.DeregisteredUTC,
        ModuleType = deviceEvent.ModuleType,
        MainboardSoftwareVersion = deviceEvent.MainboardSoftwareVersion,
        RadioFirmwarePartNumber = deviceEvent.RadioFirmwarePartNumber,
        GatewayFirmwarePartNumber = deviceEvent.GatewayFirmwarePartNumber,
        DataLinkType = deviceEvent.DataLinkType,
        OwningCustomerUID = null,
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
        DeviceSerialNumber = "Device radio serial",
        DeviceType = "SNM940",
        DeviceState = "active",
        DeregisteredUTC = firstCreatedUTC,
        ModuleType = "theModule Type",
        MainboardSoftwareVersion = "mb45",
        RadioFirmwarePartNumber = "rd 567",
        GatewayFirmwarePartNumber = "them or us",
        DataLinkType = "CAT",
        ActionUTC = firstCreatedUTC,
        ReceivedUTC = firstCreatedUTC
      };

      var deviceEventUpdate = new UpdateDeviceEvent()
      {
        DeviceUID = deviceEventCreate.DeviceUID,
        DeviceSerialNumber = "A radio serial changed",
        DeviceType = "PL221",
        DeviceState = "active still",
        DeregisteredUTC = firstCreatedUTC.AddDays(-1),
        ModuleType = "moduleTypeUpdated",
        MainboardSoftwareVersion = "mb45 changed",
        RadioFirmwarePartNumber = "rd 567 changed",
        GatewayFirmwarePartNumber = "them or us changed",
        DataLinkType = "CAT changed",
        OwningCustomerUID = Guid.NewGuid(),
        ActionUTC = firstCreatedUTC.AddMinutes(10)
      };

      var deviceFinal = new Device
      {
        DeviceUID = deviceEventCreate.DeviceUID.ToString(),
        DeviceSerialNumber = deviceEventUpdate.DeviceSerialNumber,
        DeviceType = deviceEventUpdate.DeviceType,
        DeviceState = deviceEventUpdate.DeviceState,
        DeregisteredUTC = deviceEventUpdate.DeregisteredUTC,
        ModuleType = deviceEventUpdate.ModuleType,
        MainboardSoftwareVersion = deviceEventUpdate.MainboardSoftwareVersion,
        RadioFirmwarePartNumber = deviceEventUpdate.RadioFirmwarePartNumber,
        GatewayFirmwarePartNumber = deviceEventUpdate.GatewayFirmwarePartNumber,
        DataLinkType = deviceEventUpdate.DataLinkType,
        OwningCustomerUID = deviceEventUpdate.OwningCustomerUID.ToString(),
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
      var deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      var createDeviceEvent = new CreateDeviceEvent()
      {
        DeviceUID = deviceUID,
        DeviceSerialNumber = deviceSerialNumber,
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

      var dissociateDeviceAssetEvent = new DissociateDeviceAssetEvent()
      {
        AssetUID = createAssetEvent.AssetUID,
        DeviceUID = createDeviceEvent.DeviceUID,
        ActionUTC = actionUTC.AddDays(1)
      };

      assetContext.StoreEvent(createAssetEvent).Wait();
      deviceContext.StoreEvent(createDeviceEvent).Wait();
      deviceContext.StoreEvent(associateDeviceAssetEvent).Wait();
      var g = deviceContext.GetAssociatedAsset(deviceSerialNumber, "SNM940");
      g.Wait();
      Assert.IsNotNull(g.Result, "There should be no DeviceAsset association from DeviceRepo");

      deviceContext.StoreEvent(dissociateDeviceAssetEvent).Wait();
      g = deviceContext.GetAssociatedAsset(deviceSerialNumber, "SNM940");
      g.Wait();
      Assert.IsNull(g.Result, "There should be no DeviceAsset association from DeviceRepo");
    }

    /// <summary>
    ///  DissociateDeviceAsset - 
    ///    assoc exists, dissociation is for an earlier date and should be ignored.
    ///  Note: As of this writing, no DeviceAsset association history is kept, only the most recent association.
    ///        To this end, consumers (ours and others) delete the DeviceAsset assoc from Db when a Dissociate is received.
    ///        This could create a dilema if Assoc/Dissasoc are received out of order. 
    ///             We are assured by Gowthaman (VS but#63526) that this will NEVER occur in the kafka que, 
    ///                as related assoc will occur in LastActiveUtc order.
    /// </summary>
    [TestMethod]
    public void DissociateDeviceAssetEvent_LateArrivingDissociate()
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
      var deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      var createDeviceEvent = new CreateDeviceEvent()
      {
        DeviceUID = deviceUID,
        DeviceSerialNumber = deviceSerialNumber,
        DeviceType = "SNM940",
        DeviceState = "active",
        ActionUTC = actionUTC
      };

      var associateDeviceAssetEvent = new AssociateDeviceAssetEvent()
      {
        AssetUID = createAssetEvent.AssetUID,
        DeviceUID = createDeviceEvent.DeviceUID,
        ActionUTC = actionUTC.AddDays(2)
      };

      var dissociateDeviceAssetEvent = new DissociateDeviceAssetEvent()
      {
        AssetUID = createAssetEvent.AssetUID,
        DeviceUID = createDeviceEvent.DeviceUID,
        ActionUTC = actionUTC.AddDays(1)
      };

      assetContext.StoreEvent(createAssetEvent).Wait();
      deviceContext.StoreEvent(createDeviceEvent).Wait();
      deviceContext.StoreEvent(associateDeviceAssetEvent).Wait();
      var g = deviceContext.GetAssociatedAsset(deviceSerialNumber, "SNM940");
      g.Wait();
      Assert.IsNotNull(g.Result, "There should be no DeviceAsset association from DeviceRepo");

      deviceContext.StoreEvent(dissociateDeviceAssetEvent).Wait();
      g = deviceContext.GetAssociatedAsset(deviceSerialNumber, "SNM940");
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve AssetDevicePlus from CustomerRepo");
      Assert.AreEqual(associateDeviceAssetEvent.DeviceUID.ToString(), g.Result.DeviceUID, "DeviceUID is incorrect from DeviceRepo");
      Assert.AreEqual(associateDeviceAssetEvent.AssetUID.ToString(), g.Result.AssetUID, "AssetUID is incorrect from DeviceRepo");
      Assert.AreEqual(createAssetEvent.LegacyAssetId, g.Result.LegacyAssetID, "LegacyAssetID is incorrect from DeviceRepo");
      Assert.AreEqual(createAssetEvent.OwningCustomerUID.ToString(), g.Result.OwningCustomerUID, "OwningCustomerUID is incorrect from DeviceRepo");
    }


    #endregion

  }

}

