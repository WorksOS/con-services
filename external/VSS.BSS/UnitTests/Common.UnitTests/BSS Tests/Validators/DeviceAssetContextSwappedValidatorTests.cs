using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceAssetContextSwappedValidatorTests : BssUnitTestBase
  {

    DeviceAssetContextSwappedValidator validator;
    DeviceAssetContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new DeviceAssetContextSwappedValidator();
      var oldIBKey = IdGen.GetId().ToString();
      var newIBKey = IdGen.GetId().ToString();
      var ownerBSSID = IdGen.GetId().ToString();
      var type = DeviceTypeEnum.Series521;

      context = new DeviceAssetContext
      {
        OldIBKey = oldIBKey,
        NewIBKey = newIBKey,
        OldDeviceAsset = { IbKey = oldIBKey, OwnerBSSID = ownerBSSID, AssetId = IdGen.GetId(), Type = type },
        NewDeviceAsset = { IbKey = newIBKey, OwnerBSSID = ownerBSSID, AssetId = IdGen.GetId(), Type = type }
      };
    }

    [TestMethod]
    public void Validate_DifferentOwnerBSSID_Failure()
    {
      context.NewDeviceAsset.OwnerBSSID = IdGen.GetId().ToString();
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
      Assert.AreEqual(string.Format(BssConstants.DeviceReplacement.OWNER_BSSID_DIFFERENT_FOR_OLDIBKEY_AND_NEWIBKEY, context.OldIBKey, context.NewIBKey), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_SameOwnerBSSID_Success()
    {
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings Expected");
      Assert.AreEqual(0, validator.Errors.Count(), "No Errorss Expected");
    }

    [TestMethod]
    public void Validate_OldIBKeyDoesNotHaveAssociatedAsset_Failure()
    {
      context.OldDeviceAsset.AssetId = 0;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
      Assert.AreEqual(string.Format(BssConstants.ASSET_NOT_ASSOCIATED_WITH_DEVICE, "Old", context.OldIBKey), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_NewIBKeyDoesNotHaveAssoicatedAsset_Failure()
    {
      context.NewDeviceAsset.AssetId = 0;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
      Assert.AreEqual(string.Format(BssConstants.ASSET_NOT_ASSOCIATED_WITH_DEVICE, "New", context.NewIBKey), validator.Errors[0].Item2);    
    }

    //[TestMethod]
    //public void Validate_OldDeviceHasDifferentDeviceTypeThanNewDevice_Failure()
    //{
    //  context.NewDeviceAsset.Type = DeviceTypeEnum.Series522;
    //  validator.Validate(context);
    //  Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
    //  Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
    //  Assert.AreEqual(string.Format(BssConstants.DeviceReplacement.DEVICE_SWAP_NOT_VALID, context.OldDeviceAsset.Type, context.NewDeviceAsset.Type), validator.Errors[0].Item2);
    //}

    //[TestMethod]
    //public void Validate_BothIBKeysHaveAssociatedAssets_Success()
    //{
    //  validator.Validate(context);
    //  Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
    //  Assert.AreEqual(0, validator.Errors.Count(), "One Errors Expected");
    //}
  }
}
