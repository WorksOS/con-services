using Microsoft.VisualStudio.TestTools.UnitTesting;


using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class MapDeviceRegistrationToDeviceAssetContextTests : BssUnitTestBase
  {
    MapDeviceRegistrationToDeviceAssetContext activity;
    Inputs inputs;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new MapDeviceRegistrationToDeviceAssetContext();
      inputs = new Inputs();
    }

    [TestMethod]
    public void Execute_MapMessageToDeviceStatusContext()
    {
      var message = BSS.DRBRegistered.Build();
      inputs.Add<DeviceRegistration>(message);
      activity.Execute(inputs);

      var context = inputs.Get<DeviceStatusContext>();

      Assert.AreEqual(message.IBKey, context.IBKey, "New IB key should have been updated in the context.");
      Assert.AreEqual(message.Status, context.Status, "Old IB key should have been updated in the context.");
      Assert.AreEqual(message.ActionUTC, context.ActionUTC.ToString(), "Sequence Number should have been updated in the context.");
    }

    [TestMethod]
    public void Execute_MapExistingDeviceToDeviceStatusContext()
    {
      var owner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.OwnerBssId(owner.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var message = BSS.DRBRegistered.IBKey(device.IBKey).Build();

      inputs.Add<DeviceRegistration>(message);
      activity.Execute(inputs);

      var context = inputs.Get<DeviceStatusContext>();

      AssertContext(asset, device, context.DeviceAsset, owner.BSSID);
    }

    [TestMethod]
    public void Execute_MapExistingDeviceToDeviceAssetContext_AssetNotExists()
    {
      var owner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.OwnerBssId(owner.BSSID).Save();
      var message = BSS.DRBRegistered.IBKey(device.IBKey).Build();

      inputs.Add<DeviceRegistration>(message);
      var activityResult = activity.Execute(inputs);

      var context = inputs.Get<DeviceStatusContext>();

      AssertContext(null, device, context.DeviceAsset, owner.BSSID);
    }

    [TestMethod]
    public void Excute_MapExistingDeviceToDeviceAssetContext_OwnerDoesNotExist()
    {
      var device = Entity.Device.MTS521.OwnerBssId(IdGen.StringId()).Save();
      var message = BSS.DRBRegistered.IBKey(device.IBKey).Build();

      inputs.Add<DeviceRegistration>(message);
      var activityResult = activity.Execute(inputs);

      var context = inputs.Get<DeviceStatusContext>();

      AssertContext(null, device, context.DeviceAsset);
    }

    private void AssertContext(Asset asset, Device device, DeviceAssetDto dto, string ownerBSSID = "")
    {
      if (asset != null)
      {
        Assert.IsTrue(dto.AssetExists, "Asset details should have been populated");
        Assert.AreEqual(asset.AssetID, dto.AssetId, "AssetID should have been updated in the context.");
        Assert.AreEqual(asset.fk_DeviceID, dto.DeviceId, "DeviceID should have been updated in the context.");
        Assert.AreEqual(asset.Name, dto.Name, "Asset Name should have been updated in the context.");
      }
      else
      {
        Assert.IsFalse(dto.AssetExists, "Asset details should should not been populated");
      }

      Assert.IsTrue(dto.DeviceExists, "Device details should have been populated");
      Assert.AreEqual(device.GpsDeviceID, dto.GpsDeviceId, "GPSDeviceID should have been updated in the context.");
      Assert.AreEqual(device.IBKey, dto.IbKey, "IB key should have been updated in the context.");
      Assert.AreEqual(device.fk_DeviceTypeID, (int)dto.Type, "Device Type should have been updated in the context.");
      Assert.AreEqual(device.fk_DeviceStateID, (int)dto.DeviceState, "Device State should have been updated in the context.");
      Assert.AreEqual(ownerBSSID, dto.OwnerBSSID, "OwnerBSSID should have been updated in the context.");
    }
  }
}
