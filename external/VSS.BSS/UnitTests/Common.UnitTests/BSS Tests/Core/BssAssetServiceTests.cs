using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Services.Interfaces;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class BssAssetServiceTests : BssUnitTestBase
  {
    [DatabaseTest]
    [TestMethod]
    public void GetAssetById_ExistingAsset_ReturnsAssetAndInstalledDeviceAndDeviceOwner()
    {
      var testOwner = Entity.Customer.Dealer.Save();
      var testDevice = Entity.Device.MTS521.OwnerBssId(testOwner.BSSID).Save();
      var testAsset = Entity.Asset.WithDevice(testDevice).Save();

      var asset = Services.Assets().GetAssetById(testAsset.AssetID);

      Assert.IsTrue(asset.Exists, "Asset does not exist");
      Assert.AreEqual(testAsset.Name, asset.Name, "Name not equal");
      Assert.AreEqual(testAsset.SerialNumberVIN, asset.SerialNumber, "SerialNumber not equal");
      Assert.AreEqual(testAsset.fk_MakeCode, asset.MakeCode, "MakeCode not equal");
      Assert.AreEqual(testAsset.Model, asset.Model, "Mdoel not equal");
      Assert.AreEqual(testAsset.ManufactureYear, asset.ManufactureYear, "ManufactureYear not equal");
      Assert.AreEqual(testAsset.EquipmentVIN, asset.AssetVinSN, "Equipment VIN is not equal");

      Assert.IsTrue(asset.DeviceExists, "Device does not exist");
      Assert.AreEqual(testDevice.ID, asset.DeviceId, "Id not equal");
      Assert.AreEqual(testDevice.GpsDeviceID, asset.Device.GpsDeviceId, "GpsDeviceId not equal");
      Assert.AreEqual(DeviceTypeEnum.Series521, asset.Device.Type);
      Assert.AreEqual(testDevice.OwnerBSSID, asset.Device.OwnerBssId, "OwnerBssId not equal.");

      Assert.IsTrue(asset.DeviceOwnerExists, "Owner does not exist");
      Assert.AreEqual(testOwner.ID, asset.DeviceOwnerId, "OwnerId not equal");
      Assert.AreEqual(testOwner.BSSID, asset.DeviceOwner.BssId, "Owner BssId not equal");
      Assert.AreEqual(testOwner.Name, asset.DeviceOwner.Name, "Owner name not equal");
      Assert.AreEqual(testOwner.fk_CustomerTypeID, (int)asset.DeviceOwner.Type, "Owner type not equal");
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateAsset_Success()
    {
      var device = Entity.Device.MTS521.Save();
      var context = new AssetDeviceContext();

      context.IBAsset.SerialNumber = IdGen.GetId().ToString();
      context.IBAsset.MakeCode = "CAT";
      context.IBAsset.Name = "ASSET_NAME";
      context.IBAsset.Model = "CAT_MODEL";
      context.IBAsset.ManufactureYear = DateTime.UtcNow.Year;
      context.IBAsset.AssetVinSN = IdGen.StringId();
      // Exisiting Device
      context.Device.Id = device.ID;
      context.Device.Type = (DeviceTypeEnum)device.fk_DeviceTypeID;

      var asset = Services.Assets().CreateAsset(context.IBAsset, context.Device.Id, context.Device.Type);

      Assert.IsNotNull(asset);
      var savedAsset = Ctx.OpContext.AssetReadOnly.Where(t => t.AssetID == asset.AssetID).SingleOrDefault();
      Assert.IsNotNull(savedAsset, "Asset should have been saved successfully.");
      Assert.AreEqual(context.IBAsset.SerialNumber, savedAsset.SerialNumberVIN, "Serial numbers should match.");
      Assert.AreEqual(context.IBAsset.MakeCode, savedAsset.fk_MakeCode, "Make codes should match.");
      Assert.AreEqual(context.Device.Id, savedAsset.fk_DeviceID, "Device ids should match.");
      Assert.AreEqual(context.IBAsset.AssetVinSN, savedAsset.EquipmentVIN, "Equipment VIN ids should match.");
      Assert.AreEqual((int)StoreEnum.Trimble, savedAsset.fk_StoreID, "Incorrect StoreId");
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateAsset_SerialNumberHasTrailingSpace_Success()
    {
      var device = Entity.Device.MTS521.Save();
      var context = new AssetDeviceContext();

      context.IBAsset.SerialNumber = IdGen.GetId() + " ";
      context.IBAsset.MakeCode = "CAT";
      context.IBAsset.Name = "ASSET_NAME";
      context.IBAsset.Model = "CAT_MODEL";
      context.IBAsset.ManufactureYear = DateTime.UtcNow.Year;
      context.IBAsset.AssetVinSN = IdGen.StringId();
      // Exisiting Device
      context.Device.Id = device.ID;
      context.Device.Type = (DeviceTypeEnum)device.fk_DeviceTypeID;

      var asset = Services.Assets().CreateAsset(context.IBAsset, context.Device.Id, context.Device.Type);

      Assert.IsNotNull(asset);
      var savedAsset = Ctx.OpContext.AssetReadOnly.Where(t => t.AssetID == asset.AssetID).SingleOrDefault();
      Assert.IsNotNull(savedAsset, "Asset should have been saved successfully.");
      Assert.AreEqual(context.IBAsset.SerialNumber.Trim(), savedAsset.SerialNumberVIN, "Serial numbers should match.");
      Assert.AreEqual(context.IBAsset.MakeCode, savedAsset.fk_MakeCode, "Make codes should match.");
      Assert.AreEqual(context.Device.Id, savedAsset.fk_DeviceID, "Device ids should match.");
      Assert.AreEqual(context.IBAsset.AssetVinSN, savedAsset.EquipmentVIN, "Equipment VIN ids should match.");
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateAsset_AssociatedDevice_Success()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var device = Entity.Device.PL121.OwnerBssId(dealer.BSSID).IbKey(IdGen.GetId().ToString()).Save();
      var asset = Entity.Asset.WithDevice(device).WithCoreService().Save();
      var newDevice = Entity.Device.MTS521.OwnerBssId(dealer.BSSID).Save();

      var modifiedProperties = new List<Param> { new Param { Name = "fk_DeviceID", Value = newDevice.ID } };

      var result = Services.Assets().UpdateAsset(asset.AssetID, modifiedProperties);
      Assert.IsTrue(result, "Asset Update should have been successfull.");
      var savedAsset = Ctx.OpContext.AssetReadOnly.Where(t => t.AssetID == asset.AssetID).Single();
      Assert.AreEqual(newDevice.ID, savedAsset.fk_DeviceID, "Device ids should match.");
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateAsset_Properties_Success()
    {
      var asset = Entity.Asset.Save();
      var equipmentVIN = IdGen.StringId();
      var modifiedProperties = new List<Param> 
      { 
        new Param { Name="Name", Value = "Test123" },
        new Param { Name = "Model", Value = "ABC" },
        new Param { Name = "ManufactureYear", Value = 1987 },
        new Param { Name = "EquipmentVIN", Value = equipmentVIN }
      };
      var result = Services.Assets().UpdateAsset(asset.AssetID, modifiedProperties);
      Assert.IsTrue(result, "Asset Update should have been successful.");
      var savedAsset = Ctx.OpContext.AssetReadOnly.Where(t => t.AssetID == asset.AssetID).Single();
      Assert.AreEqual(equipmentVIN, savedAsset.EquipmentVIN, "Equipment VIN ids should match.");
    }

    [TestMethod]
    public void AddAssetReference_Success()
    {
      var mockAddBssReference = new Mock<IBssReference>();
      const long storeId = (long) StoreEnum.CAT;
      const string alias = "MakeCode_SN";
      const string value = "CAT_5YW00051";
      var uid = Guid.NewGuid();
      Services.Assets().AddAssetReference(mockAddBssReference.Object, storeId, alias, value, uid);
      mockAddBssReference.Verify(o => o.AddAssetReference(storeId, alias, value, uid), Times.Once());
    }
  }
}
