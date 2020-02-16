using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class BssAssetDeviceHistoryServiceTests : BssUnitTestBase
  {
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_AssetIdNotDefined_Exception()
    {
      new BssAssetDeviceHistoryService().CreateAssetDeviceHistory(0, IdGen.GetId(), "OwnerBssId", DateTime.UtcNow);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_DeviceIdNotDefined_Exception()
    {
      new BssAssetDeviceHistoryService().CreateAssetDeviceHistory(IdGen.GetId(), 0, "OwnerBssId", DateTime.UtcNow);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_OwnerBssIdNotDefined_Exception()
    {
      new BssAssetDeviceHistoryService().CreateAssetDeviceHistory(IdGen.GetId(), IdGen.GetId(), string.Empty, DateTime.UtcNow);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_StartUtcNotDefined_Exception()
    {
      new BssAssetDeviceHistoryService().CreateAssetDeviceHistory(IdGen.GetId(), IdGen.GetId(), "OwnerBssId", DateTime.MinValue);
    }

    [TestMethod]
    public void Create_HistoryForDeviceDoesNotExist_ReturnsNewAssetDeviceHistory()
    {
      var device = Entity.Device.MTS522.Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var ash = new BssAssetDeviceHistoryService().CreateAssetDeviceHistory(asset.AssetID, device.ID, device.OwnerBSSID, asset.InsertUTC.Value);

      Assert.AreEqual(asset.AssetID, ash.fk_AssetID, "AssetId no equal.");
      Assert.AreEqual(device.ID, ash.fk_DeviceID, "DeviceId no equal.");
      Assert.AreEqual(device.OwnerBSSID, ash.OwnerBSSID, "OwnerBssId no equal.");
      Assert.AreEqual(asset.InsertUTC, ash.StartUTC, "InsertUtc no equal.");
    }

    [TestMethod]
    public void Create_HistoryForDeviceExists_AssetDeviceHistoryStartUtcIsExistingEndUtc()
    {
      var endDate = DateTime.UtcNow.AddDays(-10);
      var device = Entity.Device.MTS522.Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var existingAdh = new AssetDeviceHistory
      {
        fk_AssetID = asset.AssetID,
        fk_DeviceID = device.ID,
        StartUTC = DateTime.UtcNow.AddDays(-11),
        EndUTC = endDate
      };
      Ctx.OpContext.AssetDeviceHistory.AddObject(existingAdh);
      Ctx.OpContext.SaveChanges();

      var ash = new BssAssetDeviceHistoryService().CreateAssetDeviceHistory(asset.AssetID, device.ID, device.OwnerBSSID, asset.InsertUTC.Value);

      Assert.AreEqual(asset.AssetID, ash.fk_AssetID, "AssetId no equal.");
      Assert.AreEqual(device.ID, ash.fk_DeviceID, "DeviceId no equal.");
      Assert.AreEqual(device.OwnerBSSID, ash.OwnerBSSID, "OwnerBssId no equal.");
      Assert.AreEqual(endDate, ash.StartUTC, "startUtc is not existing end date.");
    }
  }
}
