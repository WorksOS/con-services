using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class RegisterDeviceStateTests: BssUnitTestBase
  {
    RegisterDeviceState activity;
    Inputs inputs;
    DeviceStatusContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new RegisterDeviceState();
      inputs = new Inputs();
      context = new DeviceStatusContext
      {
        IBKey = IdGen.StringId(),
        DeviceAsset = { GpsDeviceId = IdGen.StringId(), Type = DeviceTypeEnum.PL121, DeviceId = IdGen.GetId() },
      };
      inputs.Add<DeviceStatusContext>(context);
    }

    [TestMethod]
    public void RegisterDeviceState_Success()
    {
      var fake = new BssDeviceServiceFake(true);
      Services.Devices = () => fake;

      var activityResult = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Information, activityResult.Type, "Result type should be information.");
      StringAssert.Contains(activityResult.Summary, string.Format(RegisterDeviceState.SUCCESS_MESSAGE, context.IBKey), "Summary should contain the success message.");
    }

    [TestMethod]
    public void RegisterDeviceState_Exception()
    {
      var fake = new BssDeviceServiceExceptionFake();
      Services.Devices = () => fake;

      var activityResult = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Exception, activityResult.Type, "Result type should be Exception.");
      StringAssert.Contains(activityResult.Summary, string.Format(RegisterDeviceState.EXCEPTION_MESSAGE, context.IBKey), "Summary should contain the exception message.");
    }
  }
}
