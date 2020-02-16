using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AssetCreateTests : BssUnitTestBase
  {
    AssetCreate activity;
    Inputs inputs;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new AssetCreate();
      inputs = new Inputs();
    }

    [TestMethod]
    public void Execute_NullAsset_ReturnsErrorResult()
    {
      var serviceFake = new BssAssetServiceFake((Asset)null);
      Services.Assets = () => serviceFake;

      inputs.Add<AssetDeviceContext>(new AssetDeviceContext());

      var activityResult = activity.Execute(inputs);

      Assert.IsTrue(serviceFake.WasExecuted, "CreateAsset method should have been invoked.");
      StringAssert.Contains(activityResult.Summary, "Error");
    }

    [TestMethod]
    public void Execute_ThrowException_ReturnExceptionResult()
    {
      var serviceFake = new BssAssetServiceExceptionFake();
      Services.Assets = () => serviceFake;

      var context = new AssetDeviceContext { IBAsset = { SerialNumber = IdGen.GetId().ToString() } };

      inputs.Add<AssetDeviceContext>(context);

      var activityResult = activity.Execute(inputs);

      Assert.IsTrue(serviceFake.WasExecuted, "CreateAsset method should have been invoked.");
      StringAssert.Contains(activityResult.Summary, string.Format(AssetCreate.FAILURE_MESSAGE, context.IBAsset.MakeCode, context.IBAsset.SerialNumber));
    }

    [TestMethod]
    public void Execute_AssetCreate_Success()
    {
      var asset = new Asset
      {
        AssetID = IdGen.GetId(),
        SerialNumberVIN = IdGen.GetId().ToString(),
        fk_MakeCode = "CAT",
        InsertUTC = DateTime.UtcNow,
        ManufactureYear = DateTime.UtcNow.Year,
        Model = "CAT",
        Name = "TEST",
        ProductFamilyName = "CAT",
        fk_DeviceID = IdGen.GetId(),
        EquipmentVIN = IdGen.StringId()
      };

      var serviceFake = new BssAssetServiceFake(asset);
      Services.Assets = () => serviceFake;

      var context = new AssetDeviceContext
      {
        IBAsset =
        {
          Name = asset.Name,
          MakeCode = asset.fk_MakeCode,
          SerialNumber = asset.SerialNumberVIN,
          Model = asset.Model,
          ManufactureYear = asset.ManufactureYear,
          AssetVinSN = asset.EquipmentVIN
        },
        Device = { Id = IdGen.GetId() }
      };

      inputs.Add<AssetDeviceContext>(context);

      var activityResult = activity.Execute(inputs);
      Assert.IsTrue(serviceFake.WasExecuted, "AssetCreate should have been called.");
      Assert.AreEqual(asset.AssetID, context.Asset.AssetId, "Asset IDs should match.");
      Assert.AreEqual(asset.SerialNumberVIN, context.Asset.SerialNumber, "Asset Serial Numbers should match.");
      Assert.AreEqual(asset.fk_MakeCode, context.Asset.MakeCode, "Make Codes should match.");
      Assert.AreEqual(asset.EquipmentVIN, context.Asset.AssetVinSN, "Asset Vin Serial Number should match.");
      StringAssert.Contains(activityResult.Summary, "Success");
    }

    [TestMethod]
    public void Execute_AssetCreate_TrailingSpaceInSerialNumberIsRemoved_Success()
    {
      var asset = new Asset
      {
        AssetID = IdGen.GetId(),
        SerialNumberVIN = IdGen.GetId() + " ",
        fk_MakeCode = "CAT",
        InsertUTC = DateTime.UtcNow,
        ManufactureYear = DateTime.UtcNow.Year,
        Model = "CAT",
        Name = "TEST",
        ProductFamilyName = "CAT",
        fk_DeviceID = IdGen.GetId(),
        EquipmentVIN = IdGen.StringId()
      };

      var serviceFake = new BssAssetServiceFake(asset);
      Services.Assets = () => serviceFake;

      var context = new AssetDeviceContext
      {
        IBAsset =
        {
          Name = asset.Name,
          MakeCode = asset.fk_MakeCode,
          SerialNumber = asset.SerialNumberVIN,
          Model = asset.Model,
          ManufactureYear = asset.ManufactureYear,
          AssetVinSN = asset.EquipmentVIN
        },
        Device = { Id = IdGen.GetId() }
      };

      inputs.Add<AssetDeviceContext>(context);

      var activityResult = activity.Execute(inputs);
      Assert.IsTrue(serviceFake.WasExecuted, "AssetCreate should have been called.");
      Assert.AreEqual(asset.AssetID, context.Asset.AssetId, "Asset IDs should match.");
      Assert.AreEqual(asset.SerialNumberVIN.Trim(), context.Asset.SerialNumber, "Asset Serial Numbers should match.");
      Assert.AreEqual(asset.fk_MakeCode, context.Asset.MakeCode, "Make Codes should match.");
      Assert.AreEqual(asset.EquipmentVIN, context.Asset.AssetVinSN, "Asset Vin Serial Number should match.");
      StringAssert.Contains(activityResult.Summary, "Success");
    }
  }
}
