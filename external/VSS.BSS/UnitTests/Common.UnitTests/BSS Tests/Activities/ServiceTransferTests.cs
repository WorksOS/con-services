using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ServiceTransferTests : BssUnitTestBase
  {
    ServiceTransfer activity;
    Inputs inputs;
    DeviceAssetContext context;
    ActivityResult result = null;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new ServiceTransfer();
      inputs = new Inputs();

      context = new DeviceAssetContext
      {
        OldIBKey = IdGen.GetId().ToString(),
        NewIBKey = IdGen.GetId().ToString(),
        NewDeviceAsset = { DeviceId = IdGen.GetId(), GpsDeviceId = IdGen.GetId().ToString(), Type = DeviceTypeEnum.PL321 },
        OldDeviceAsset = { DeviceId = IdGen.GetId(), GpsDeviceId = IdGen.GetId().ToString(), Type = DeviceTypeEnum.PL321 },
      };

      inputs.Add<DeviceAssetContext>(context);
    }

    [TestCleanup]
    public void TestCleanup()
    {
      if (result == null) return;
      Console.WriteLine(result.Summary);
    }

    [TestMethod]
    public void Execute_ServiceTransfer_EmptyList_Error()
    {
      var fake = new BssServiceViewServiceFake(new List<Service>());
      Services.ServiceViews = () => fake;
      
      result = activity.Execute(inputs);
      
      Assert.AreEqual(ResultType.Warning, result.Type, "Result type should be Error.");
      Assert.IsTrue(fake.WasExecuted);
      StringAssert.Contains(result.Summary, "Warning: ");
    }

    [TestMethod]
    public void Execute_ServiceTransfer_Null_Error()
    {
      var fake = new BssServiceViewServiceFake(services: null);
      Services.ServiceViews = () => fake;

      result = activity.Execute(inputs);

      Assert.AreEqual(ResultType.Warning, result.Type, "Result type should be Error.");
      Assert.IsTrue(fake.WasExecuted);
      StringAssert.Contains(result.Summary, "Warning: ");
    }

    [TestMethod]
    public void Execute_ServiceTransfer_Success()
    {
      var fake = new BssServiceViewServiceFake(new List<Service> { new Service { fk_DeviceID = IdGen.GetId() } });
      Services.ServiceViews = () => fake;
      
      result = activity.Execute(inputs);
      
      Assert.AreEqual(ResultType.Information, result.Type, "Result type should be Information.");
      Assert.IsTrue(fake.WasExecuted);
      StringAssert.Contains(result.Summary, "Success: ");
    }

    [TestMethod]
    public void Execute_ServiceTransfer_Exception()
    {
      var fake = new BssServiceViewServiceExceptionFake(new NotImplementedException());
      Services.ServiceViews = () => fake;
      
      result = activity.Execute(inputs);
      
      Assert.AreEqual(ResultType.Exception, result.Type, "Result type should be Exception.");
      StringAssert.Contains(result.Summary, "Exception: ");
    }
  }
}
