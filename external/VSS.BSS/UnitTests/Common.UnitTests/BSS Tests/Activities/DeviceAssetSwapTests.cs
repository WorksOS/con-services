using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceAssetSwapTests : BssUnitTestBase
  {
    DeviceAssetSwap activity;
    Inputs inputs;
    DeviceAssetContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new DeviceAssetSwap();
      inputs = new Inputs();

      context = new DeviceAssetContext
      {
        OldDeviceAsset = { AssetId = IdGen.GetId(), DeviceId = IdGen.GetId() },
        NewDeviceAsset = { AssetId = IdGen.GetId(), DeviceId = IdGen.GetId() }
      };

      inputs.Add<DeviceAssetContext>(context);
    }

    [TestMethod]
    public void Execute_DeviceAssetSwap_ReturnFalse_Exception()
    {
      var fake = new BssAssetServiceFake(false);
      Services.Assets = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Error, result.Type, "Result Type Error is expected.");
      StringAssert.Contains(result.Summary, DeviceAssetSwap.RETURNED_FALSE_MESSAGE);
    }

    [TestMethod]
    public void Execute_DeviceAssetSwap_ReturnException_Exception()
    {
      var fake = new BssAssetServiceExceptionFake();
      Services.Assets = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Exception, result.Type, "Result Type Exception is expected.");
      StringAssert.Contains(result.Summary, "Failed to swap Asset");
    }

    [TestMethod]
    public void Execute_DeviceAssetSwap_ReturnTrue_Success()
    {
      var fake = new BssAssetServiceFake(true);
      Services.Assets = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Information, result.Type, "Result Type Information is expected.");
      StringAssert.Contains(result.Summary, DeviceAssetSwap.SUCCESS_MESSAGE);
    }

  }
}
