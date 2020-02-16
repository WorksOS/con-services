using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class UpdateDeviceStateTests : BssUnitTestBase
  {
    UpdateDeviceState activity;
    Inputs inputs;
    DeviceServiceContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new UpdateDeviceState();
      inputs = new Inputs();
      context = new DeviceServiceContext
      {
        IBKey = IdGen.GetId().ToString(),
        ExistingDeviceAsset = { GpsDeviceId = IdGen.GetId().ToString(), Type = DeviceTypeEnum.Series521 },
        SequenceNumber = IdGen.GetId(),
        PlanLineID = IdGen.GetId().ToString()
      };
      inputs.Add<DeviceServiceContext>(context);
    }

    [TestMethod]
    public void Execute_UpdateDeviceState_Success()
    {
      var fake = new BssDeviceServiceFake();
      Services.Devices = () => fake;

      var result = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Information, result.Type, "Result type should be Information.");
      StringAssert.Contains(result.Summary, string.Format(UpdateDeviceState.SUCCESS_MESSAGE, context.IBKey), "Summary should match.");
    }
   
    [TestMethod]
    public void Execute_UpdateDeviceState_Exception()
    {
      var fake = new BssDeviceServiceExceptionFake();
      Services.Devices = () => fake;

      var result = activity.Execute(inputs);

      Assert.AreEqual(ResultType.Exception, result.Type, "Result type should be Exception.");
      StringAssert.Contains(result.Summary, string.Format(UpdateDeviceState.EXCEPTION_MESSAGE, context.IBKey), "Summary should match.");
    }
  }
}
