using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeRegisterDeviceStateOTATests : BssUnitTestBase
  {
    DeRegisterDeviceStateOTA activity;
    Inputs inputs;
    DeviceStatusContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new DeRegisterDeviceStateOTA();
      inputs = new Inputs();
      context = new DeviceStatusContext
      {
        IBKey = IdGen.StringId(),
        DeviceAsset = { GpsDeviceId = IdGen.StringId() },
      };
      inputs.Add<DeviceStatusContext>(context);
    }

    [TestMethod]
    public void DeRegisterDeviceStateOTA_Success()
    {
      var fake = new BssPLOTAServiceFake(true);
      Services.OTAServices = () => fake;

      var activityResult = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Information, activityResult.Type, "Result type should be information.");
      StringAssert.Contains(activityResult.Summary, string.Format(DeRegisterDeviceStateOTA.SUCCESS_MESSAGE, context.IBKey), "Summary should contain the success message.");
    }

    [TestMethod]
    public void DeRegisterDeviceStateOTA_Failure()
    {
      var fake = new BssPLOTAServiceFake(false);
      Services.OTAServices = () => fake;

      var activityResult = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Error, activityResult.Type, "Result type should be Error.");
      StringAssert.Contains(activityResult.Summary, string.Format(DeRegisterDeviceStateOTA.RETURNED_FALSE_MESSAGE, context.IBKey), "Summary should contain the false message.");
    }

    [TestMethod]
    public void DeRegisterDeviceStateOTA_Exception()
    {
      var fake = new BssPLOTAServiceExceptionFake();
      Services.OTAServices = () => fake;

      var activityResult = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Exception, activityResult.Type, "Result type should be Exception.");
      StringAssert.Contains(activityResult.Summary, string.Format(DeRegisterDeviceStateOTA.EXCEPTION_MESSAGE, context.IBKey), "Summary should contain the exception message.");
    }
  }
}
