using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DevicePersonalityCreateTests : BssUnitTestBase
  {
    DevicePersonalityCreate activity;
    Inputs inputs;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new DevicePersonalityCreate();
      inputs = new Inputs();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Execute_NullIBDeviceContext_Failure()
    {
      var context = new AssetDeviceContext();
      context.IBDevice = null;
      inputs.Add<AssetDeviceContext>(context);
      activity.Execute(inputs);
    }

    [TestMethod]
    public void Execute_NullDevicePersonalitiesReturned_WarningResult_Failure()
    {
      var serviceFake = new BssDeviceServiceFake((List<DevicePersonality>)null);
      Services.Devices = () => serviceFake;

      var context = new AssetDeviceContext
      {
        IBDevice = 
        {
          IbKey = IdGen.GetId().ToString()
        }
      };
      inputs.Add<AssetDeviceContext>(context);

      var activityResult = activity.Execute(inputs);

      Assert.AreEqual(ResultType.Warning, activityResult.Type, "Result type should be Warning.");
      StringAssert.Contains(activityResult.Summary, string.Format(DevicePersonalityCreate.COUNT_IS_ZERO_MESSAGE, context.IBDevice.IbKey), "Summary of the result should match.");
      Assert.IsTrue(serviceFake.WasExecuted, "Expecting to be executed.");
    }

    [TestMethod]
    public void Execute_ZeroDevicePersonality_WarningResult_Failure()
    {
      var serviceFake = new BssDeviceServiceFake(new List<DevicePersonality>());
      Services.Devices = () => serviceFake;

      var context = new AssetDeviceContext
      {
        IBDevice = 
        {
          IbKey = IdGen.GetId().ToString()
        }
      };

      inputs.Add<AssetDeviceContext>(context);

      var activityResult = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Warning, activityResult.Type, "Result type should be Warning.");
      StringAssert.Contains(activityResult.Summary, string.Format(DevicePersonalityCreate.COUNT_IS_ZERO_MESSAGE, context.IBDevice.IbKey));
      Assert.IsTrue(serviceFake.WasExecuted, "Expecting to be executed.");
    }

    [TestMethod]
    public void Execute_ExceptionResult_Failure()
    {
      var serviceFake = new BssDeviceServiceExceptionFake();
      Services.Devices = () => serviceFake;

      var context = new AssetDeviceContext
      {
        IBDevice = 
        {
          IbKey = IdGen.GetId().ToString()
        }
      };
      inputs.Add<AssetDeviceContext>(context);

      var activityResult = activity.Execute(inputs);

      Assert.AreEqual(ResultType.Exception, activityResult.Type, "Result type should be Exception.");
      StringAssert.Contains(activityResult.Summary, string.Format(DevicePersonalityCreate.FAILURE_MESSAGE, context.IBDevice.IbKey, "The method or operation is not implemented."), "Summary of the exception result should match.");
      Assert.IsTrue(serviceFake.WasExecuted, "Expecting to be executed.");
    }

    [TestMethod]
    public void Execute_ValidDevice_Success()
    {
      var context = new AssetDeviceContext
      {
        Device =  new ExistingDeviceDto{ Id = IdGen.GetId() },
        IBDevice = new DeviceDto
        {
          IbKey = IdGen.GetId().ToString(),
          FirmwareVersionId = IdGen.GetId().ToString()
        }
      };

      var serviceFake = new BssDeviceServiceFake(new List<DevicePersonality> 
      { 
        new DevicePersonality { 
          fk_DeviceID = context.Device.Id, 
          fk_PersonalityTypeID = (int)PersonalityTypeEnum.Software, 
          Value = context.IBDevice.FirmwareVersionId, 
          UpdateUTC = DateTime.UtcNow}
      }
      );

      Services.Devices = () => serviceFake;

      inputs.Add<AssetDeviceContext>(context);
      var activityResult = activity.Execute(inputs);
      StringAssert.Contains(activityResult.Summary, string.Format(DevicePersonalityCreate.SUCCESS_MESSAGE, 1), "Device Personalities count should match.");
      Console.WriteLine(activityResult.Summary);
      Assert.IsTrue(serviceFake.WasExecuted, "Expecting to be executed.");
    }

  }
}
