using Microsoft.VisualStudio.TestTools.UnitTesting;


using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class MapDeviceReplacementToDeviceAssetContextTests : BssUnitTestBase
  {

    MapDeviceReplacementToDeviceAssetContext activity;
    Inputs inputs;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new MapDeviceReplacementToDeviceAssetContext();
      inputs = new Inputs();
    }

    [TestMethod]
    public void Execute_MapMessageToDeviceAssetContext()
    {
      var message = BSS.DRReplaced.Build();
      inputs.Add<DeviceReplacement>(message);
      activity.Execute(inputs);

      var context = inputs.Get<DeviceAssetContext>();

      Assert.AreEqual(message.NewIBKey, context.NewIBKey, "New IB key should have been updated in the context.");
      Assert.AreEqual(message.OldIBKey, context.OldIBKey, "Old IB key should have been updated in the context.");
      Assert.AreEqual(message.SequenceNumber, context.SequenceNumber, "Sequence Number should have been updated in the context.");
    }

    [TestMethod]
    public void Execute_MapExistingDeviceToDeviceAssetContext()
    {
      var owner = Entity.Customer.Dealer.Save();
      var oldDevice = Entity.Device.MTS521.OwnerBssId(owner.BSSID).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).Save();
      var newDevice = Entity.Device.MTS521.OwnerBssId(owner.BSSID).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).Save();
      var message = BSS.DRReplaced.NewIBKey(newDevice.IBKey).OldIBKey(oldDevice.IBKey).Build();

      inputs.Add<DeviceReplacement>(message);
      activity.Execute(inputs);

      var context = inputs.Get<DeviceAssetContext>();

      AssertContext(oldAsset, oldDevice, context.OldDeviceAsset);
      AssertContext(newAsset, newDevice, context.NewDeviceAsset);
    }

    [TestMethod]
    public void Execute_MapExistingDeviceToDeviceAssetContext_AssetNotExists()
    {
      var owner = Entity.Customer.Dealer.Save();
      var oldDevice = Entity.Device.MTS521.OwnerBssId(owner.BSSID).Save();
      var newDevice = Entity.Device.MTS521.OwnerBssId(owner.BSSID).Save();
      var message = BSS.DRReplaced.NewIBKey(newDevice.IBKey).OldIBKey(oldDevice.IBKey).Build();

      inputs.Add<DeviceReplacement>(message);
      var activityResult = activity.Execute(inputs);

      var context = inputs.Get<DeviceAssetContext>();

      AssertContext(null, oldDevice, context.OldDeviceAsset);
      AssertContext(null, newDevice, context.NewDeviceAsset);
    }

    private void AssertContext(Asset asset, Device device, DeviceAssetDto dto)
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
    }
  }
}
