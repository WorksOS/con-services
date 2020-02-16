using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AssetUpdateTests : BssUnitTestBase
  {
    AssetUpdate activity;
    Inputs inputs;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new AssetUpdate();
      inputs = new Inputs();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Execute_NullIBAsset_Exception()
    {
      var context = new AssetDeviceContext { IBAsset = null };
      inputs.Add<AssetDeviceContext>(context);
      activity.Execute(inputs);
    }

    [TestMethod]
    public void Execute_AssetPropertiesChanges_Success()
    {
      var serviceFake = new BssAssetServiceFake(true);
      Services.Assets = () => serviceFake;

      var context = new AssetDeviceContext
      {
        IBAsset = { Model = "TT", ManufactureYear = DateTime.UtcNow.Year - 1, Name = "Test1", AssetVinSN = IdGen.StringId() },
        Asset = { Model = "CAT", ManufactureYear = DateTime.UtcNow.Year, Name = "Test", AssetVinSN = IdGen.StringId(), StoreID = 0 }
      };

      inputs.Add<AssetDeviceContext>(context);
      var result =activity.Execute(inputs);

      Assert.AreEqual(ResultType.Information, result.Type, "Activity is expected to run with out any error.");
      Assert.AreEqual(context.IBAsset.Model, context.Asset.Model, "After update the Model of the asset is expected to be equal.");
      Assert.AreEqual(context.IBAsset.ManufactureYear, context.Asset.ManufactureYear, "After update the ManufactureYear of the asset is expected to be equal.");
      Assert.AreEqual(context.IBAsset.Name, context.Asset.Name, "After update the Name of the asset is expected to be equal.");
      Assert.AreEqual(context.IBAsset.AssetVinSN, context.Asset.AssetVinSN, "After update the Asset VIN SN of the asset is expected to be equal.");
      Assert.AreEqual(1, context.Asset.StoreID, "StoreId Not Changed");
    }

    [TestMethod]
    public void Execute_NoPropertiesModified_ReturnsCancelledMessage_AssetNotUpdated()
    {
      var deviceID = IdGen.GetId();

      var context = new AssetDeviceContext
      {
        Device = { Id = deviceID },
        Asset = { DeviceId = deviceID }
      };

      inputs.Add<AssetDeviceContext>(context);

      var result = activity.Execute(inputs);

      StringAssert.Contains(result.Summary, "cancelled");
    }

    [TestMethod]
    public void Execute_AssetUpdate_DeviceReplacement_ReturnsWarningMessageAndUpdateDevice()
    {
      var serviceFake = new BssAssetServiceFake(true);
      Services.Assets = () => serviceFake;

      var context = new AssetDeviceContext
      {
        Device = { Id = IdGen.GetId() },
        Asset = { DeviceId = IdGen.GetId() }
      };

      inputs.Add<AssetDeviceContext>(context);

      var result = activity.Execute(inputs);

      Assert.AreEqual(context.Device.Id, context.Asset.DeviceId);
      StringAssert.Contains(result.Summary, string.Format("Installing new {0} IBKey: {1} on Asset", context.Device.Type, context.Device.IbKey));
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_AssetUpdate_ExceptionMessage()
    {
      var serviceFake = new BssAssetServiceExceptionFake();
      Services.Assets = () => serviceFake;

      var context = new AssetDeviceContext
      {
        Device = { Id = IdGen.GetId() },
        Asset = { DeviceId = IdGen.GetId() }
      };

      inputs.Add<AssetDeviceContext>(context);

      var result = activity.Execute(inputs);
      StringAssert.Contains(result.Summary, "Failed to update");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_AssetUpdate_ReturnedFalse_ErrorMessasge()
    {
      var serviceFake = new BssAssetServiceFake(false);
      Services.Assets = () => serviceFake;

      var context = new AssetDeviceContext
      {
        Device = { Id = IdGen.GetId() },
        Asset = { DeviceId = IdGen.GetId() }
      };

      inputs.Add<AssetDeviceContext>(context);

      var result = activity.Execute(inputs);
      StringAssert.Contains(result.Summary, "Update of asset");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }
  }
}
