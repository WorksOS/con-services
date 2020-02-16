using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceSwapRecordAssetHistoryTests : BssUnitTestBase
  {
    DeviceSwapRecordAssetHistory activity;
    Inputs inputs;
    DeviceAssetContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new DeviceSwapRecordAssetHistory();
      inputs = new Inputs();
      context = new DeviceAssetContext
      {
        OldDeviceAsset = { AssetId = IdGen.GetId(), DeviceId = IdGen.GetId(), OwnerBSSID = IdGen.GetId().ToString(), InsertUTC = DateTime.UtcNow, Type = DeviceTypeEnum.Series521 },
        NewDeviceAsset = { AssetId = IdGen.GetId(), DeviceId = IdGen.GetId(), OwnerBSSID = IdGen.GetId().ToString(), InsertUTC = DateTime.UtcNow, Type = DeviceTypeEnum.Series521 }
      };

      inputs.Add<DeviceAssetContext>(context);
    }

    [TestMethod]
    public void Execute_DeviceSwapRecordAssetHistory_NullResult_Error()
    {
      var fake = new BssAssetDeviceHistoryServiceFake(null);
      Services.AssetDeviceHistory = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Error, result.Type, "Result Type should be Error.");
      StringAssert.Contains(result.Summary, DeviceSwapRecordAssetHistory.RETURN_NULL_MESSAGE);
    }

    [TestMethod]
    public void Execute_DeviceSwapRecordAssetHistory_ExceptionResult_Error()
    {
      var fake = new BssAssetDeviceHistoryServiceExceptionFake();
      Services.AssetDeviceHistory = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Exception, result.Type, "Result Type should be Exception.");
      StringAssert.Contains(result.Summary, DeviceSwapRecordAssetHistory.EXCEPTION_MESSAGE);
    }

    [TestMethod]
    public void Execute_DeviceSwapRecordAssetHistory_ExceptionResult_Success()
    {
      var fake = new BssAssetDeviceHistoryServiceFake(new AssetDeviceHistory
      {
        fk_AssetID = IdGen.GetId(),
        fk_DeviceID = IdGen.GetId(),
        OwnerBSSID = IdGen.GetId().ToString(),
        StartUTC = DateTime.UtcNow,
        EndUTC = DateTime.UtcNow.AddYears(1),
        ID = IdGen.GetId()
      });

      Services.AssetDeviceHistory = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Information, result.Type, "Result Type should be Information.");
      StringAssert.Contains(result.Summary, "DeviceAssetHistory created for IBDevice");
    }
  }
}
