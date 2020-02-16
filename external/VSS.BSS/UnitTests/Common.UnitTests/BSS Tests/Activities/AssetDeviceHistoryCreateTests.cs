using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AssetDeviceHistoryCreateTests : BssUnitTestBase
  {
    AssetDeviceHistoryCreate activity;
    Inputs inputs;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new AssetDeviceHistoryCreate();
      inputs = new Inputs();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Execute_NullIBAssetContext_Failure()
    {
      var context = new AssetDeviceContext();
      context.IBAsset = null;
      inputs.Add<AssetDeviceContext>(context);
      activity.Execute(inputs);
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

    [Ignore]
    [TestMethod]
    public void Execute_NullAssetDeviceHistory_Failure()
    {
      var serviceFake = new BssDeviceServiceFake((AssetDeviceHistory)null);
      Services.Devices = () => serviceFake;

      inputs.Add<AssetDeviceContext>(new AssetDeviceContext());

      var activityResult = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Error, activityResult.Type, "Activity result should be Error.");
      StringAssert.Contains(activityResult.Summary, AssetDeviceHistoryCreate.ASSET_DEVICE_HISTORY_NULL_MESSAGE, "Summaries should match.");
      Assert.IsTrue(serviceFake.WasExecuted, "The code should have been executed.");
    }

    [Ignore]
    [TestMethod]
    public void Execute_AssetDeviceHistory_Exception_Failure()
    {
      var serviceFake = new BssDeviceServiceExceptionFake();
      Services.Devices = () => serviceFake;
      var context = new AssetDeviceContext
      {
        IBDevice = new DeviceDto
        {
          IbKey = IdGen.GetId().ToString(),
        },
        IBAsset = new AssetDto 
        {
          SerialNumber = IdGen.GetId().ToString(),
        }
      };
      inputs.Add<AssetDeviceContext>(context);

      var activityResult = activity.Execute(inputs);

      Assert.AreEqual(ResultType.Exception, activityResult.Type, "Activity result should be Exception.");
      StringAssert.Contains(activityResult.Summary, string.Format(AssetDeviceHistoryCreate.FAILURE_MESSAGE, 
        context.IBDevice.IbKey, context.IBAsset.SerialNumber), "Summaries should match.");
      Assert.IsTrue(serviceFake.WasExecuted, "The code should have been executed.");
    }

    [Ignore]
    [TestMethod]
    public void Execute_AssetDeviceHistory_Success()
    {
      var assetDeviceHistory = new AssetDeviceHistory
      {
        fk_AssetID = IdGen.GetId(), 
        fk_DeviceID = IdGen.GetId(), 
        OwnerBSSID = IdGen.GetId().ToString(), 
        StartUTC = DateTime.UtcNow, 
        EndUTC = DateTime.UtcNow
      };

      var serviceFake = new BssDeviceServiceFake(assetDeviceHistory);

      inputs.Add<AssetDeviceContext>(new AssetDeviceContext());

      Services.Devices = () => serviceFake;

      var activityResult = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Information, activityResult.Type, "Activity result should be Information.");
      StringAssert.Contains(activityResult.Summary,
        string.Format(AssetDeviceHistoryCreate.SUCCESS_MESSAGE, 
        assetDeviceHistory.fk_DeviceID, assetDeviceHistory.fk_AssetID,
        assetDeviceHistory.OwnerBSSID, assetDeviceHistory.StartUTC, 
        assetDeviceHistory.EndUTC), "Summaries should match.");
      Assert.IsTrue(serviceFake.WasExecuted, "The code should have been executed.");
    }
  }
}
