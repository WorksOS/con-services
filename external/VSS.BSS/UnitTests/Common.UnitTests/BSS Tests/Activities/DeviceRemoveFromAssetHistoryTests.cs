using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceRemoveFromAssetHistoryTests : BssUnitTestBase
  {
    private Inputs Inputs;
    private AssetDeviceContext Context;
    private DeviceRemoveFromOldAssetHistory Activity;

    [TestInitialize]
    public void RemoveDeviceFromAssetHistoryTests_Init()
    {
      Inputs = new Inputs();
      Context = new AssetDeviceContext();
      Inputs.Add<AssetDeviceContext>(Context);
      Activity = new DeviceRemoveFromOldAssetHistory();
    }

    [TestMethod]
    public void Execute_CalledWithCorrectArgs_SuccessMessage()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Device.OwnerBssId = "OWNER_BSSID";
      Context.Device.AssetId = IdGen.GetId();
      Context.Device.Asset.InsertUtc = DateTime.UtcNow.AddDays(-10);

      var adh = new AssetDeviceHistory();
      adh.fk_AssetID = Context.Device.AssetId;
      adh.fk_DeviceID = Context.Device.Id;
      adh.OwnerBSSID = Context.Device.OwnerBssId;
      adh.StartUTC = Context.Device.Asset.InsertUtc.Value;
      adh.EndUTC = DateTime.UtcNow;

      var serviceFake = new BssAssetDeviceHistoryServiceFake(adh);
      Services.AssetDeviceHistory = () => serviceFake;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Success");
      StringAssert.Contains(result.Summary, string.Format(DeviceRemoveFromOldAssetHistory.SUCCESS_MESSAGE,
        Context.Device.Type, Context.Device.Id, Context.Device.AssetId, Context.Device.Asset.InsertUtc, adh.EndUTC));

      Assert.IsTrue(serviceFake.WasExecuted, "Should have been executed.");
      Assert.AreEqual(Context.Device.AssetId, serviceFake.AssetIdArg, "AssetIdArg not equal");
      Assert.AreEqual(Context.Device.Id, serviceFake.DeviceIdArg, "DeviceIdArg not equal");
      Assert.AreEqual(Context.Device.OwnerBssId, serviceFake.OwnerBssIdArg, "OwnerBssIdArg not equal");
      Assert.AreEqual(Context.Device.Asset.InsertUtc, serviceFake.StartUtcArg, "StartUtcArg not equal");
    }

    [TestMethod]
    public void Execute_DeviceDoesNotExist_CancelledSummary()
    {
      Context.Device.Id = 0;

      var serviceFake = new BssAssetDeviceHistoryServiceFake(new AssetDeviceHistory());
      Services.AssetDeviceHistory = () => serviceFake;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Cancelled");
      StringAssert.Contains(result.Summary, DeviceRemoveFromOldAssetHistory.CANCELLED_DEVICE_DOES_NOT_EXIST_MESSAGE);
      Assert.IsFalse(serviceFake.WasExecuted, "Should not have been executed.");
    }

    [TestMethod]
    public void Execute_DeviceIsNotInstalledOnAnAsset_CancelledSummary()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Device.OwnerBssId = "OWNER_BSSID";
      Context.Device.AssetId = 0;

      var serviceFake = new BssAssetDeviceHistoryServiceFake(new AssetDeviceHistory());
      Services.AssetDeviceHistory = () => serviceFake;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Cancelled");
      StringAssert.Contains(result.Summary, DeviceRemoveFromOldAssetHistory.CANCELLED_DEVICE_NOT_INSTALLED_ON_ASSET_MESSAGE);
      Assert.IsFalse(serviceFake.WasExecuted, "Should not have been executed.");
    }

    [TestMethod]
    public void Execute_ServiceReturnsNull_ErrorSummary()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Device.OwnerBssId = "OWNER_BSSID";
      Context.Device.AssetId = IdGen.GetId();
      Context.Device.Asset.InsertUtc = DateTime.UtcNow.AddDays(-10);

      var serviceFake = new BssAssetDeviceHistoryServiceFake(null);
      Services.AssetDeviceHistory = () => serviceFake;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Error");
      StringAssert.Contains(result.Summary, DeviceRemoveFromOldAssetHistory.RETURN_NULL_MESSAGE);
      Assert.IsTrue(serviceFake.WasExecuted, "Should have been executed.");
    }

    [TestMethod]
    public void Execute_ServiceThrowsException_ExceptionSummary()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Device.OwnerBssId = "OWNER_BSSID";
      Context.Device.AssetId = IdGen.GetId();
      Context.Device.Asset.InsertUtc = DateTime.UtcNow.AddDays(-10);

      var serviceFake = new BssAssetDeviceHistoryServiceExceptionFake();
      Services.AssetDeviceHistory = () => serviceFake;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Exception");
      StringAssert.Contains(result.Summary, "Failed to create AssetDeviceHistory.");
      StringAssert.Contains(result.Summary, DeviceRemoveFromOldAssetHistory.EXCEPTION_MESSAGE);
      Assert.IsTrue(serviceFake.WasExecuted, "Should have been executed.");
    }
  }
}
