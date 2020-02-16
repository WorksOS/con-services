using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AssetDeviceContextCreateValidatorTests : BssUnitTestBase
  {
    private AssetDeviceContextCreateValidator _validator;

    [TestInitialize]
    public void TestInitialize()
    {
      _validator = new AssetDeviceContextCreateValidator();
    }

    [TestMethod]
    public void Validate_ValidContext_DeviceExists_Error()
    {
      var context = new AssetDeviceContext
      {
        Device = {Id = IdGen.GetId()}
      };

      _validator.Validate(context);

      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(1, _validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.IbKeyExists, _validator.Errors[0].Item1);
      StringAssert.Contains(_validator.Errors[0].Item2,
        string.Format(BssConstants.InstallBase.IBKEY_EXISTS, context.IBDevice.IbKey),
        "Summay is expected to contian IBKEY exists message.");
    }

    [TestMethod]
    public void Validate_GpsDeviceIdAndDeviceTypeCombinationExist_Error()
    {
      var existingDevice = Entity.Device.MTS521.Save();

      var context = new AssetDeviceContext
      {
        IBDevice =
        {
          IbKey = IdGen.GetId().ToString(),
          GpsDeviceId = existingDevice.GpsDeviceID,
          Type = (DeviceTypeEnum) existingDevice.fk_DeviceTypeID
        }
      };

      _validator.Validate(context);

      Assert.AreEqual(1, _validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.GpsDeviceIdExists, _validator.Errors[0].Item1);
      StringAssert.Contains(_validator.Errors[0].Item2,
        string.Format(BssConstants.InstallBase.GPS_DEVICEID_EXISTS, context.IBDevice.GpsDeviceId),
        "Summary is expected to contain GPSDevice ID exists message.");
    }

    [DatabaseTest]
    [TestMethod]
    public void Validate_DeviceReplacement_NewDeviceDoesNotSupportCurrentServices_Error()
    {
      var owner = Entity.Customer.Dealer.Save();
      var device = Entity.Device.MTS521.OwnerBssId(owner.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
      var asset =
        Entity.Asset.WithDevice(device).WithCoreService().WithService(ServiceTypeEnum.e1minuteUpdateRateUpgrade).Save();

      var context = new AssetDeviceContext {Asset = {AssetId = asset.AssetID}};
      context.Asset.MapAsset(asset);
      context.Asset.DeviceId = device.ID;
      context.Asset.Device.MapDevice(device);
      context.Asset.DeviceOwnerId = owner.ID;
      context.Asset.DeviceOwner.MapOwner(owner);
      context.IBDevice.IbKey = IdGen.StringId();
      context.IBDevice.GpsDeviceId = IdGen.StringId();
      context.IBDevice.Type = DeviceTypeEnum.PL121;
      context.Owner.Id = owner.ID;
      context.Owner.MapOwner(owner);

      _validator.Validate(context);

      Assert.AreEqual(1, _validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.DeviceReplaceNotValid, _validator.Errors[0].Item1);
      StringAssert.Contains(_validator.Errors[0].Item2,
        string.Format(BssConstants.DeviceReplacement.NEW_DEVICE_DOES_NOT_SUPPORT_OLD_DEVICE_SERVICES,
          context.IBDevice.IbKey, context.Asset.Device.IbKey));
    }

    [TestMethod]
    public void Validate_DeviceHasInvalidStore()
    {
      var context = new AssetDeviceContext {Device = {Id = IdGen.GetId()}, IBDevice = {GpsDeviceId = IdGen.StringId()}};
      context.Device.GpsDeviceId = context.IBDevice.GpsDeviceId;

      context.Device.Asset.StoreID = 2;
      _validator.Validate(context);

      Assert.AreEqual(2, _validator.Errors.Count);
      foreach (var a in _validator.Errors)
      {
        Assert.IsTrue(a.Item1 == BssFailureCode.DeviceRelatedToDifferentStore || a.Item1 == BssFailureCode.IbKeyExists);
      }
    }

    [TestMethod]
    public void Validate_AssetHasInvalidStore()
    {
      var context = new AssetDeviceContext {Asset = {AssetId = IdGen.GetId(), DeviceId = IdGen.GetId(), StoreID = 2}};

      _validator.Validate(context);

      Assert.AreEqual(1, _validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.AssetRelatedToDifferentStore, _validator.Errors.First().Item1);
    }

    [TestMethod]
    public void Validate_DeviceHasValidStore()
    {
      var context = new AssetDeviceContext {Device = {Id = IdGen.GetId()}, IBDevice = {GpsDeviceId = IdGen.StringId()}};
      context.Device.GpsDeviceId = context.IBDevice.GpsDeviceId;

      context.Device.Asset.StoreID = 1;
      _validator.Validate(context);

      Assert.AreEqual(1, _validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.IbKeyExists, _validator.Errors[0].Item1);
    }

    [TestMethod]
    public void Validate_AssetHasValidStore()
    {
      var context = new AssetDeviceContext {Asset = {AssetId = IdGen.GetId(), DeviceId = IdGen.GetId(), StoreID = 1}};

      _validator.Validate(context);

      Assert.AreEqual(0, _validator.Errors.Count);
    }
  }
}