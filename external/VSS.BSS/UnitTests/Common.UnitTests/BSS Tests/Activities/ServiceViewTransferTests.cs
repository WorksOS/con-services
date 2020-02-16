using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ServiceViewTransferTests : BssUnitTestBase
  {
    ServiceViewTransfer activity;
    Inputs inputs;
    DeviceAssetContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new ServiceViewTransfer();
      inputs = new Inputs();

      context = new DeviceAssetContext
      {
        OldDeviceAsset = { DeviceId = IdGen.GetId(), Type = DeviceTypeEnum.Series521, AssetId = IdGen.GetId() },
        NewDeviceAsset = { DeviceId = IdGen.GetId(), Type = DeviceTypeEnum.Series521, AssetId = IdGen.GetId() }
      };
      inputs.Add<DeviceAssetContext>(context);
    }

    [TestMethod]
    public void Execute_ServiceViewTransferReturnedTrue_Success()
    {
      var fake = new BssServiceViewServiceFake(true);
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Information, result.Type, "Result Type should be Information");
      StringAssert.Contains(result.Summary, "Success");
    }

    [TestMethod]
    public void Execute_ServiceViewTransferReturnedFalse_Error()
    {
      var fake = new BssServiceViewServiceFake(false);
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Information, result.Type, "Result Type should be Error");
      StringAssert.Contains(result.Summary, ServiceViewTransfer.COUNT_IS_ZERO_MESSAGE);
    }

    [TestMethod]
    public void Execute_ServiceViewTransfer_Exception()
    {
      var fake = new BssServiceViewServiceExceptionFake(new NotImplementedException());
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Exception, result.Type, "Result Type should be Exception");
      StringAssert.Contains(result.Summary, string.Format(ServiceViewTransfer.FAILURE_MESSAGE, context.OldDeviceAsset.AssetId, context.NewDeviceAsset.AssetId));
    }
  }
}
