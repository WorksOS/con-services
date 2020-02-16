using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceRemoveFromAssetTests : BssUnitTestBase
  {
    private Inputs Inputs;
    private AssetDeviceContext Context;
    private DeviceRemoveFromOldAsset Activity;

    [TestInitialize]
    public void DeviceRemoveFromAssetTests_Init()
    {
      Inputs = new Inputs();
      Context = new AssetDeviceContext();
      Inputs.Add<AssetDeviceContext>(Context);
      Activity = new DeviceRemoveFromOldAsset();
    }

    [TestMethod]
    public void Execute_Success_SuccessResultWithSummary()
    {
      var serviceFake = new BssAssetServiceFake(true);
      Services.Assets = () => serviceFake;

      long assetId = IdGen.GetId();
      Context.Device.Id = IdGen.GetId();
      Context.Device.Type = DeviceTypeEnum.Series521;
      Context.Device.AssetId = assetId;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Success");
      StringAssert.Contains(result.Summary, string.Format(DeviceRemoveFromOldAsset.SUCCESS_MESSAGE, Context.Device.Type, Context.Device.Id, assetId));
      Assert.IsFalse(Context.Device.AssetExists, "Device should no longer be installed on an Asset.");

      Assert.IsTrue(serviceFake.WasExecuted, "Should have been executed.");
      Assert.AreEqual(assetId, serviceFake.AssetIdArg, "Update called with wrong AssetId argument.");
      Assert.AreEqual("fk_DeviceID", serviceFake.ModifiedPropertiesArg[0].Name, "Update called with wrong modified property name argument.");
      Assert.AreEqual(0, serviceFake.ModifiedPropertiesArg[0].Value, "Update called with wrong modified property value argument.");
    }

    [TestMethod]
    public void Execute_DeviceDoesNotExist_CancelledResultWithSummary()
    {
      var serviceFake = new BssAssetServiceFake(true);
      Services.Assets = () => serviceFake;

      var context = new AssetDeviceContext();
      context.Device.Id = 0;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Cancelled");
      StringAssert.Contains(result.Summary, DeviceRemoveFromOldAsset.CANCELLED_DEVICE_DOES_NOT_EXIST_MESSAGE);
      Assert.IsFalse(serviceFake.WasExecuted, "Should not have been executed.");
    }

    [TestMethod]
    public void Execute_DeviceNotInstalledOnAsset_CancelledResultWithSummary()
    {
      var serviceFake = new BssAssetServiceFake(true);
      Services.Assets = () => serviceFake;

      Context.Device.Id = IdGen.GetId();
      Context.Device.Type = DeviceTypeEnum.Series521;
      Context.Device.AssetId = 0;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Cancelled");
      StringAssert.Contains(result.Summary, string.Format(DeviceRemoveFromOldAsset.CANCELLED_DEVICE_NOT_INSTALLED_ON_ASSET_MESSAGE, Context.Device.Type, Context.Device.Id));
      Assert.IsFalse(serviceFake.WasExecuted, "Should not have been executed.");
    }

    [TestMethod]
    public void Execute_UpdateAssetReturnsFalse_ErrorResultWithSummary()
    {
      var serviceFake = new BssAssetServiceFake(false);
      Services.Assets = () => serviceFake;

      Context.Device.Id = IdGen.GetId();
      Context.Device.Type = DeviceTypeEnum.Series521;
      Context.Device.AssetId = IdGen.GetId();

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Error");
      StringAssert.Contains(result.Summary, string.Format(DeviceRemoveFromOldAsset.RETURNED_FALSE_MESSAGE, Context.Device.Type, Context.Device.Id, Context.Device.AssetId));
      Assert.IsTrue(serviceFake.WasExecuted, "Should not have been executed.");
    }

    [TestMethod]
    public void Execute_UpdateAssetThrowsException_ExceptionResultWithSummary()
    {
      var serviceFake = new BssAssetServiceExceptionFake();
      Services.Assets = () => serviceFake;

      Context.Device.Id = IdGen.GetId();
      Context.Device.Type = DeviceTypeEnum.Series521;
      Context.Device.AssetId = IdGen.GetId();

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Exception");
      StringAssert.Contains(result.Summary, string.Format(DeviceRemoveFromOldAsset.EXCEPTION_MESSAGE, Context.Device.Type, Context.Device.Id, Context.Device.AssetId));
      Assert.IsTrue(serviceFake.WasExecuted, "Should not have been executed.");
    }
  }
}
