using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeActivateDeviceStateTests : BssUnitTestBase
  {
    DeActivateDeviceState activity;
    Inputs inputs;
    DeviceServiceContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new DeActivateDeviceState();
      inputs = new Inputs();
      context = new DeviceServiceContext
      {
        IBKey = IdGen.GetId().ToString(),
      };
      inputs.Add<DeviceServiceContext>(context);
    }

    [TestMethod]
    public void Execute_DeActivateDeviceState_Success()
    {
      var fake = new BssServiceViewServiceFake(true);
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Information, result.Type, "Result type should be Information.");
      StringAssert.Contains(result.Summary, string.Format(DeActivateDeviceState.SUCCESS_MESSAGE, context.IBKey), "Summary should match.");
    }

    [TestMethod]
    public void Execute_DeActivateDeviceState_Failure()
    {
      var fake = new BssServiceViewServiceFake(false);
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);

      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Error, result.Type, "Result type should be Error.");
      StringAssert.Contains(result.Summary, string.Format(DeActivateDeviceState.RETURNED_FALSE_MESSAGE, context.IBKey), "Summary should match.");
    }

    [TestMethod]
    public void Execute_DeActivateDeviceState_Exception()
    {
      var fake = new BssServiceViewServiceExceptionFake(new NotImplementedException());
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);

      Assert.AreEqual(ResultType.Exception, result.Type, "Result type should be Exception.");
      StringAssert.Contains(result.Summary, string.Format(DeActivateDeviceState.EXCEPTION_MESSAGE, context.IBKey), "Summary should match.");
    }
  }
}
