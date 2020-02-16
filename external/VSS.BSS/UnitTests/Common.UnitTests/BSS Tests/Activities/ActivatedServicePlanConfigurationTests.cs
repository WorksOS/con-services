using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ActivatedServicePlanConfigurationTests : BssUnitTestBase
  {
    ActivatedServicePlanConfiguration activity;
    Inputs inputs;
    DeviceServiceContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new ActivatedServicePlanConfiguration();
      inputs = new Inputs();
      context = new DeviceServiceContext
      {
        ActionUTC = DateTime.UtcNow,
        ServiceType = ServiceTypeEnum.Essentials,
        ExistingDeviceAsset = { AssetId = IdGen.GetId(), GpsDeviceId = IdGen.GetId().ToString(), Type = DeviceTypeEnum.Series521 }
      };
      inputs.Add<DeviceServiceContext>(context);
    }

    [TestMethod]
    public void Execute_ServicePlanConfigure_Success()
    {
      var fake = new BssServiceViewServiceFake(true);
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Information, result.Type, "Result type should be Information.");
      StringAssert.Contains(result.Summary, string.Format(ActivatedServicePlanConfiguration.SUCCESS_MESSAGE, context.ExistingDeviceAsset.AssetId), "Summary should match.");
    }

    [TestMethod]
    public void Execute_ServicePlanConfigure_Error()
    {
      var fake = new BssServiceViewServiceFake(false);
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Error, result.Type, "Result type should be Error.");
      StringAssert.Contains(result.Summary, ActivatedServicePlanConfiguration.RETURNED_FALSE_MESSAGE, "Summary should match.");
    }

    [TestMethod]
    public void Execute_ServicePlanConfigure_Exception()
    {
      var fake = new BssServiceViewServiceExceptionFake(new NotImplementedException());
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Exception, result.Type, "Result type should be Exception.");
      StringAssert.Contains(result.Summary, string.Format(ActivatedServicePlanConfiguration.EXCEPTION_MESSAGE, context.ExistingDeviceAsset.AssetId), "Summary should match.");
    }
  }
}