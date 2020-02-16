using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AssetRemoveDeviceHistoryTests : BssUnitTestBase
  {
    private Inputs Inputs;
    private AssetDeviceContext Context;
    private AssetRemoveDeviceHistory Activity;

    [TestInitialize]
    public void AssetRemoveDeviceHistoryTests_Init()
    {
      Inputs = new Inputs();
      Context = new AssetDeviceContext();
      Inputs.Add<AssetDeviceContext>(Context);
      Activity = new AssetRemoveDeviceHistory();
    }

    [TestMethod]
    public void Execute_CalledWithCorrectArgs_SuccessMessage()
    {
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.OwnerBssId = "OWNER_BSSID";
      Context.Asset.InsertUtc = DateTime.UtcNow.AddDays(-10);

      var adh = new AssetDeviceHistory();
      adh.fk_AssetID = Context.Asset.AssetId;
      adh.fk_DeviceID = Context.Asset.DeviceId;
      adh.OwnerBSSID = Context.Asset.Device.OwnerBssId;
      adh.StartUTC = Context.Asset.InsertUtc.Value;
      adh.EndUTC = DateTime.UtcNow;

      var serviceFake = new BssAssetDeviceHistoryServiceFake(adh);
      Services.AssetDeviceHistory = () => serviceFake;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Success");
      StringAssert.Contains(result.Summary, string.Format(AssetRemoveDeviceHistory.SUCCESS_MESSAGE,
        Context.Asset.AssetId, Context.Asset.Device.Type, Context.Asset.DeviceId, Context.Asset.InsertUtc, adh.EndUTC));

      Assert.IsTrue(serviceFake.WasExecuted, "Should have been executed.");
      Assert.AreEqual(Context.Asset.AssetId, serviceFake.AssetIdArg, "AssetIdArg not equal");
      Assert.AreEqual(Context.Asset.DeviceId, serviceFake.DeviceIdArg, "DeviceIdArg not equal");
      Assert.AreEqual(Context.Asset.Device.OwnerBssId, serviceFake.OwnerBssIdArg, "OwnerBssIdArg not equal");
      Assert.AreEqual(Context.Asset.InsertUtc, serviceFake.StartUtcArg, "StartUtcArg not equal");
    }

    [TestMethod]
    public void Execute_AssetDoesNotExist_CancelledSummary()
    {
      Context.Asset.AssetId = 0;

      var serviceFake = new BssAssetDeviceHistoryServiceFake(new AssetDeviceHistory());
      Services.AssetDeviceHistory = () => serviceFake;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Cancelled");
      StringAssert.Contains(result.Summary, AssetRemoveDeviceHistory.CANCELLED_DEVICE_DOES_NOT_EXIST_MESSAGE);
      Assert.IsFalse(serviceFake.WasExecuted, "Should not have been executed.");
    }

    [TestMethod]
    public void Execute_DeviceIsNotInstalledOnAnAsset_CancelledSummary()
    {
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = 0;

      var serviceFake = new BssAssetDeviceHistoryServiceFake(new AssetDeviceHistory());
      Services.AssetDeviceHistory = () => serviceFake;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Cancelled");
      StringAssert.Contains(result.Summary, AssetRemoveDeviceHistory.CANCELLED_DEVICE_NOT_INSTALLED_ON_ASSET_MESSAGE);
      Assert.IsFalse(serviceFake.WasExecuted, "Should not have been executed.");
    }

    [TestMethod]
    public void Execute_ServiceReturnsNull_ErrorSummary()
    {
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.OwnerBssId = "OWNER_BSSID";
      Context.Asset.InsertUtc = DateTime.UtcNow.AddDays(-10);

      var serviceFake = new BssAssetDeviceHistoryServiceFake(null);
      Services.AssetDeviceHistory = () => serviceFake;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Error");
      StringAssert.Contains(result.Summary, AssetRemoveDeviceHistory.RETURN_NULL_MESSAGE);
      Assert.IsTrue(serviceFake.WasExecuted, "Should have been executed.");
    }

    [TestMethod]
    public void Execute_ServiceThrowsException_ExceptionSummary()
    {
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.OwnerBssId = "OWNER_BSSID";
      Context.Asset.InsertUtc = DateTime.UtcNow.AddDays(-10);

      var serviceFake = new BssAssetDeviceHistoryServiceExceptionFake();
      Services.AssetDeviceHistory = () => serviceFake;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Exception");
      StringAssert.Contains(result.Summary, "Failed to create AssetDeviceHistory.");
      StringAssert.Contains(result.Summary, AssetRemoveDeviceHistory.EXCEPTION_MESSAGE);
      Assert.IsTrue(serviceFake.WasExecuted, "Should have been executed.");
    }

  }
}
