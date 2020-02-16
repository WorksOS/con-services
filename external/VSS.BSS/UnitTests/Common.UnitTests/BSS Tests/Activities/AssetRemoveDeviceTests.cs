using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AssetRemoveDeviceTests : BssUnitTestBase
  {
    private Inputs Inputs;
    private AssetDeviceContext Context;
    private AssetRemoveDevice Activity;

    [TestInitialize]
    public void DeviceRemoveFromAssetTests_Init()
    {
      Inputs = new Inputs();
      Context = new AssetDeviceContext();
      Inputs.Add<AssetDeviceContext>(Context);
      Activity = new AssetRemoveDevice();
    }

    [TestMethod]
    public void Execute_Success_SuccessResultWithSummary()
    {
      var serviceFake = new BssAssetServiceFake(true);
      Services.Assets = () => serviceFake;

      long deviceId = IdGen.GetId();
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = deviceId;
      Context.Asset.Device.Type = DeviceTypeEnum.Series521;
      
      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Success");
      StringAssert.Contains(result.Summary, string.Format(AssetRemoveDevice.SUCCESS_MESSAGE, Context.Asset.Device.Type, deviceId, Context.Asset.AssetId));
      Assert.IsFalse(Context.Asset.DeviceExists, "Device should no longer be installed on an Asset.");

      Assert.IsTrue(serviceFake.WasExecuted, "Should have been executed.");
      Assert.AreEqual(Context.Asset.AssetId, serviceFake.AssetIdArg, "Update called with wrong AssetId argument.");
      Assert.AreEqual("fk_DeviceID", serviceFake.ModifiedPropertiesArg[0].Name, "Update called with wrong modified property name argument.");
      Assert.AreEqual(0, serviceFake.ModifiedPropertiesArg[0].Value, "Update called with wrong modified property value argument.");
    }

    [TestMethod]
    public void Execute_AssetDoesNotExist_CancelledResultWithSummary()
    {
      var serviceFake = new BssAssetServiceFake(true);
      Services.Assets = () => serviceFake;

      var context = new AssetDeviceContext();
      context.Asset.AssetId = 0;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Cancelled");
      StringAssert.Contains(result.Summary, AssetRemoveDevice.CANCELLED_ASSET_DOES_NOT_EXIST_MESSAGE);
      Assert.IsFalse(serviceFake.WasExecuted, "Should not have been executed.");
    }

    [TestMethod]
    public void Execute_DeviceNotInstalledOnAsset_CancelledResultWithSummary()
    {
      var serviceFake = new BssAssetServiceFake(true);
      Services.Assets = () => serviceFake;

      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = 0;
      
      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Cancelled");
      StringAssert.Contains(result.Summary, string.Format(AssetRemoveDevice.CANCELLED_DEVICE_NOT_INSTALLED_ON_ASSET_MESSAGE, Context.Asset.AssetId));
      Assert.IsFalse(serviceFake.WasExecuted, "Should not have been executed.");
    }

    [TestMethod]
    public void Execute_UpdateAssetReturnsFalse_ErrorResultWithSummary()
    {
      var serviceFake = new BssAssetServiceFake(false);
      Services.Assets = () => serviceFake;

      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.Type = DeviceTypeEnum.Series521;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Error");
      StringAssert.Contains(result.Summary, string.Format(AssetRemoveDevice.RETURNED_FALSE_MESSAGE, Context.Asset.Device.Type, Context.Asset.DeviceId, Context.Asset.AssetId));
      Assert.IsTrue(serviceFake.WasExecuted, "Should not have been executed.");
    }

    [TestMethod]
    public void Execute_UpdateAssetThrowsException_ExceptionResultWithSummary()
    {
      var serviceFake = new BssAssetServiceExceptionFake();
      Services.Assets = () => serviceFake;

      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.Type = DeviceTypeEnum.Series521;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Exception");
      StringAssert.Contains(result.Summary, string.Format(AssetRemoveDevice.EXCEPTION_MESSAGE, Context.Asset.Device.Type, Context.Asset.DeviceId, Context.Asset.AssetId));
      Assert.IsTrue(serviceFake.WasExecuted, "Should not have been executed.");
    }
  }

  
}
