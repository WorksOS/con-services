using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.WebApi;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass]
  public class AssetIDCacheTest : UnitTestBase
  {
    [TestMethod]
    public void FindInvalid()
    {
      Asset asset1 = TestData.TestAssetMTS522;
      asset1.Device.fk_DeviceStateID = (int)DeviceStateEnum.Subscribed;

      AssetIDCache.Init(true);
      long? assetID = AssetIDCache.GetAssetID("WRONG111ABC", DeviceTypeEnum.Series522);
      Assert.IsNull(assetID, "Expect to not find asset with wrong gpsdeviceid");

      assetID = AssetIDCache.GetAssetID(asset1.Device.GpsDeviceID, DeviceTypeEnum.PL121);
      Assert.IsNull(assetID, "Expect not to find the asset ID when specifying the wrong device type");

      assetID = AssetIDCache.GetAssetID(asset1.Device.GpsDeviceID, DeviceTypeEnum.Series522);
      Assert.IsNotNull(assetID, "Expect to find it now - positive case");
      Assert.AreEqual(asset1.AssetID, assetID, "Wrong asset ID");
    }

    [TestMethod]
    public void FindPL()
    {
      TestData.TestAssetPL121.Device.fk_DeviceStateID = (int)DeviceStateEnum.Subscribed;

      AssetIDCache.Init(true);
      long? assetID = AssetIDCache.GetAssetID(TestData.TestAssetPL121.Device.GpsDeviceID, DeviceTypeEnum.PL121);
      Assert.IsNotNull(assetID, "assetID should not be null");
      long? deviceTypeID = AssetIDCache.GetDeviceTypeID(TestData.TestPL121.GpsDeviceID, DeviceTypeEnum.PL121, assetID.Value);
      Assert.IsNotNull(assetID, "Expect to find asset with general PL type");
      Assert.AreEqual(TestData.TestAssetPL121.AssetID, assetID, "Wrong asset ID");
      Assert.IsNotNull(deviceTypeID, "Expect to find asset with specific PL type");
      Assert.AreEqual((long)DeviceTypeEnum.PL121, deviceTypeID, "Wrong DeviceTypeID");
    }

    [TestMethod]
    public void FindPL321()
    {
      TestData.TestAssetPL321.Device.fk_DeviceStateID = (int)DeviceStateEnum.Subscribed;

      AssetIDCache.Init(true);
      long? assetID = AssetIDCache.GetAssetID(TestData.TestAssetPL321.Device.GpsDeviceID, DeviceTypeEnum.PL121);
      Assert.IsNotNull(assetID, "assetID should not be null");
      long? deviceTypeID = AssetIDCache.GetDeviceTypeID(TestData.TestPL321.GpsDeviceID, DeviceTypeEnum.PL121, assetID.Value);
      Assert.IsNotNull(assetID, "Expect to find asset with general PL type");
      Assert.AreEqual(TestData.TestAssetPL321.AssetID, assetID, "Wrong asset ID");
      Assert.IsNotNull(deviceTypeID, "Expect to find asset with specific PL type");
      Assert.AreEqual((long)DeviceTypeEnum.PL321, deviceTypeID, "Wrong DeviceTypeID");
    }

    [TestMethod]
    public void FindTT()
    {
      Asset asset = TestData.TestAssetTrimTrac;
      TestData.TestTrimTrac.fk_DeviceStateID = (int)DeviceStateEnum.Subscribed;

      AssetIDCache.Init(true);
      long? assetID = AssetIDCache.GetAssetID(TestData.TestAssetTrimTrac.Device.GpsDeviceID, DeviceTypeEnum.TrimTrac);
      Assert.IsNotNull(assetID, "assetID should not be null");
      long? deviceTypeID = AssetIDCache.GetDeviceTypeID(TestData.TestTrimTrac.GpsDeviceID, DeviceTypeEnum.TrimTrac, assetID.Value);
      Assert.IsNotNull(assetID, "Expect to find asset with general TrimTrac type");
      Assert.AreEqual(asset.AssetID, assetID, "Wrong asset ID");
      Assert.IsNotNull(deviceTypeID, "Expect to find asset with specific TrimTrac type");
      Assert.AreEqual((long)DeviceTypeEnum.TrimTrac, deviceTypeID, "Wrong DeviceTypeID");
    }

    [TestMethod]
    public void FindAssetByAssetIDTest_Empty_NothingReturned()
    {
      Asset asset = TestData.TestAssetPL321;
      asset.Device.fk_DeviceStateID = (int)DeviceStateEnum.Subscribed;

      long? assetID = AssetIDCache.GetAssetID(string.Empty, DeviceTypeEnum.Series522, 15);
      Assert.IsNull(assetID, "Expect to not find asset with bad AssetID");

      AssetIDCache.Init(true);
      assetID = AssetIDCache.GetAssetID(string.Empty, DeviceTypeEnum.PL321, asset.AssetID);
      Assert.AreEqual(asset.AssetID, assetID, "Expect to find the asset ID");
    }

		[TestMethod]
		public void FindAssetByAssetIDTest_RecentlyAdded_PickedUpOnCacheMiss()
		{
			TestData.TestAssetPL321.Device.fk_DeviceStateID = (int)DeviceStateEnum.Subscribed;

			AssetIDCache.Init(true); //init
			long? assetID = AssetIDCache.GetAssetID(TestData.TestAssetTrimTrac.Device.GpsDeviceID, DeviceTypeEnum.TrimTrac);
			Assert.IsNull(assetID, "assetID should be null");

			Asset asset = TestData.TestAssetTrimTrac;
			TestData.TestTrimTrac.fk_DeviceStateID = (int)DeviceStateEnum.Subscribed;
			//gets picked up now on cache miss
			assetID = AssetIDCache.GetAssetID(TestData.TestAssetTrimTrac.Device.GpsDeviceID, DeviceTypeEnum.TrimTrac);
			Assert.IsNotNull(assetID, "assetID should not be null");
			
		}

    [TestMethod]
    public void FindAssetByAssetIDNoDeviceTest()
    {
      Device device = Entity.Device.NoDevice.Save();
      Asset asset = Entity.Asset.SerialNumberVin("TESTASSET").MakeCode("CAT").WithDevice(device).Save();
      asset.Device.fk_DeviceStateID = (int)DeviceStateEnum.Subscribed;
      AssetIDCache.Init(true);
      long? assetID = AssetIDCache.GetAssetID(device.GpsDeviceID, DeviceTypeEnum.MANUALDEVICE, asset.AssetID);
      Assert.IsNotNull(assetID, "Expect to find asset with Manual Watch");
    }

    [TestMethod]
    public void CacheUpdate()
    {
      Asset asset1 = TestData.TestAssetPL321;
      asset1.Device.fk_DeviceStateID = (int)DeviceStateEnum.Subscribed;
      AssetIDCache.Init(true);
      long? assetID = AssetIDCache.GetAssetID(TestData.TestPL321.GpsDeviceID, DeviceTypeEnum.PL321, asset1.AssetID);
      Assert.AreEqual(asset1.AssetID, assetID, "Expect to find the asset ID");

      // New asset gets added...
      Asset asset2 = TestData.TestAssetMTS522;
      asset2.Device.fk_DeviceStateID = (int)DeviceStateEnum.Subscribed;

      // ...emulate update due
      AssetIDCache.Init(true);
      assetID = AssetIDCache.GetAssetID(TestData.TestMTS522.GpsDeviceID, DeviceTypeEnum.Series522, asset2.AssetID);
      Assert.AreEqual(asset2.AssetID, assetID, "Expect to find the asset ID after a cache update");
    }

    [TestMethod]
    public void TestAssetKeysNullGpsDeviceID()
    {
      AssetIDCache.AssetKeys assetKeys = new AssetIDCache.AssetKeys();
      assetKeys.AssetID = 123;
      assetKeys.GpsDeviceID = null;
      assetKeys.DeviceTypeID = (int) DeviceTypeEnum.Series522;
      assetKeys.Make = "CAT";
      string key;
      AssertEx.Throws<InvalidOperationException>(() => key = assetKeys.Key, "GPS Device ID cannot be null");
    }

    [TestMethod]
    [DatabaseTest]    
    public void TestAssetKeyNotFoundInAssetIDCacheWhenSwappingDevicesOnAsset()
    {
      //WHY THIS UNIT TEST WAS WRITTEN: 
      //Bugg: 22579 Asset with PL321 Device swapped with PL522 and the old device kept reporting on different Asset, messing up reporting. 
      //New code added to AssetIDCache.GetCacheItem to return "False" on matching Asset => Devices solely by AssetID 

      //arrange
      //Create Asset in NH_OP with 321 and Active Service
      Service service = Entity.Service.Essentials.Save();
      Device device = Entity.Device.PL321.GpsDeviceId("DeviceUnitTest").IbKey("123").WithService(service).DeviceState(DeviceStateEnum.Subscribed).Save();
      Asset asset = Entity.Asset.SerialNumberVin("UnitTest").MakeCode("CAT").WithDevice(device).Save();
       
      //act
      AssetIDCache.Init(true);

      //validate that the cache finds the asset with this Device on it. Any data coming to us from CAT via the Telematic sync will have a DeviceType established upstream (hence the hard-coded 121 devicetype)
      AssetIDCache.AssetKeys keys = AssetIDCache.GetCacheItem(device.GpsDeviceID, DeviceTypeEnum.PL321, asset.AssetID);
      Assert.IsNotNull(keys);
      Assert.IsTrue(keys.AssetID == asset.AssetID);
      Assert.IsTrue(keys.GpsDeviceID == asset.Device.GpsDeviceID);
      Assert.IsTrue(keys.DeviceTypeID == asset.Device.fk_DeviceTypeID);

      //swap the 321 with a 522 device (SAVE a new Device to our Asset)
      Device device522 = Entity.Device.MTS522.GpsDeviceId("DeviceUnitTest522").IbKey("456").WithService(service).DeviceState(DeviceStateEnum.Subscribed).Save();
      asset.Device = device522;   
      //update the db directly with this to enforce the Asset's Device change to a MTS522 
      var modifiedProperties = new List<Param>();
      modifiedProperties.Add(new Param { Name = "fk_DeviceID", Value = device522.ID  });
      Services.Assets().UpdateAsset(asset.AssetID, modifiedProperties);

      //re-initialize the cache => confirm the 522 device is IN the cache! Any device data coming to us from the MTSGateway does not have an AssetID established (hence the hard-coded deviceType of 0)
      AssetIDCache.Init(true);
      AssetIDCache.AssetKeys newKeys = AssetIDCache.GetCacheItem(device522.GpsDeviceID, (DeviceTypeEnum)device522.fk_DeviceTypeID, asset.AssetID);
      //confirm the 522 Device is in the cache and associated with the correct Asset
      Assert.IsNotNull(newKeys);
      Assert.IsTrue(newKeys.AssetID == asset.AssetID);
      Assert.IsTrue(newKeys.GpsDeviceID == asset.Device.GpsDeviceID);
      Assert.IsTrue(newKeys.DeviceTypeID == asset.Device.fk_DeviceTypeID);

      //confirm the 321 device is NOT IN the cache?
      AssetIDCache.AssetKeys newKeysFinal = AssetIDCache.GetCacheItem(device.GpsDeviceID, DeviceTypeEnum.PL321, 0);
      Assert.IsNull(newKeysFinal);
    }

  }
}
