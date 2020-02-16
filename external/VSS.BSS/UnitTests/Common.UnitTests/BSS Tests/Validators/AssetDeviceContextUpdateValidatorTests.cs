using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AssetDeviceContextUpdateValidatorTests : BssUnitTestBase
  {
    protected AssetDeviceContextUpdateValidator Validator;
    protected AssetDeviceContext Context;

    [TestInitialize]
    public void TestInitialize()
    {
      Context = new AssetDeviceContext();
      Validator = new AssetDeviceContextUpdateValidator();
    }

    [TestMethod]
    public void Validate_DeviceExists_Success()
    {
      Context.Device.Id = IdGen.GetId();

      Validator.Validate(Context);

      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(0, Validator.Errors.Count, "Errors");
    }

    [TestMethod]
    public void Validate_DeviceDoesNotExists_Error()
    {
      Context.Device.Id = 0;

      Validator.Validate(Context);

      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(1, Validator.Errors.Count, "Errors");
      Assert.AreEqual(BssFailureCode.IbKeyDoesNotExist, Validator.Errors[0].Item1);
      var errorMessage = string.Format(BssConstants.IBKEY_DOES_NOT_EXISTS, Context.IBDevice.IbKey);
      Assert.AreEqual(errorMessage, Validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_GpsDeviceIdsAreSame_Success()
    {
      Context.Device.Id = IdGen.GetId();
      Context.IBDevice.GpsDeviceId = IdGen.StringId();
      Context.Device.GpsDeviceId = Context.IBDevice.GpsDeviceId;

      Validator.Validate(Context);

      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(0, Validator.Errors.Count, "Errors");
    }

    [TestMethod]
    public void Validate_GpsDeviceIdsAreDifferent_Error()
    {
      Context.Device.Id = IdGen.GetId();
      Context.IBDevice.GpsDeviceId = IdGen.StringId();
      Context.Device.GpsDeviceId = IdGen.StringId();

      Validator.Validate(Context);

      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(1, Validator.Errors.Count, "Errors");
      Assert.AreEqual(BssFailureCode.GpsDeviceIdInvalid, Validator.Errors[0].Item1, "Error Code");
      var errorMessage = string.Format(BssConstants.InstallBase.GPS_DEVICEIDS_DO_NOT_MATCH, Context.Device.GpsDeviceId, Context.IBDevice.GpsDeviceId);
      Assert.AreEqual(errorMessage, Validator.Errors[0].Item2, "Error Message");
    }

    [TestMethod]
    public void Validate_ImpliedActionIsDeviceReplacement_Matches_Success()
    {
      Context.ImpliedAction = BssImpliedAction.DeviceReplacement;
      Context.IBDevice.Type = DeviceTypeEnum.PL121;

      Context.Device.Id = IdGen.GetId();
      Context.Device.DeviceState = DeviceStateEnum.Provisioned;

      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.DeviceState = DeviceStateEnum.Subscribed;

      Validator.Validate(Context);

      Assert.IsTrue(Context.IsDeviceReplacement());
      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(0, Validator.Errors.Count, "Errors");
    }

    [TestMethod]
    public void Validate_ImpliedActionIsDeviceReplacement_DoesNotMatch_Error()
    {
      Context.ImpliedAction = BssImpliedAction.DeviceReplacement;
      Context.IBDevice.Type = DeviceTypeEnum.PL121;

      Context.Device.Id = IdGen.GetId();
      Context.Device.DeviceState = DeviceStateEnum.Provisioned;

      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.DeviceState = DeviceStateEnum.Provisioned;

      Validator.Validate(Context);

      Assert.IsFalse(Context.IsDeviceReplacement());
      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(1, Validator.Errors.Count, "Errors");
      Assert.AreEqual(BssFailureCode.DeviceReplaceNotValid, Validator.Errors[0].Item1, "Error Code");
      var errorMessage = BssConstants.InstallBase.IMPLIED_ACTION_IS_DEVICE_REPLACEMENT;
      Assert.AreEqual(errorMessage, Validator.Errors[0].Item2, "Error Message");

    }

    [TestMethod]
    public void Validate_ImpliedActionIsDeviceTransfer_Matches_Success()
    {
      Context.ImpliedAction = BssImpliedAction.DeviceTransfer;
      Context.IBDevice.Type = DeviceTypeEnum.PL121;

      Context.Device.Id = IdGen.GetId();
      Context.Device.DeviceState = DeviceStateEnum.Provisioned;

      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.DeviceState = DeviceStateEnum.Provisioned;

      Validator.Validate(Context);

      Assert.IsTrue(Context.IsDeviceTransfer());
      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(0, Validator.Errors.Count, "Errors");
    }

    [TestMethod]
    public void Validate_ImpliedActionIsDeviceTransfer_DoesNotMatch_Error()
    {
      Context.ImpliedAction = BssImpliedAction.DeviceTransfer;
      Context.IBDevice.Type = DeviceTypeEnum.PL121;

      Context.Device.Id = IdGen.GetId();
      Context.Device.DeviceState = DeviceStateEnum.Provisioned;

      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.DeviceState = DeviceStateEnum.Subscribed; // Looks like a Device Replacement

      Validator.Validate(Context);

      Assert.IsFalse(Context.IsDeviceTransfer());
      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(1, Validator.Errors.Count, "Errors");
      Assert.AreEqual(BssFailureCode.DeviceTransferNotValid, Validator.Errors[0].Item1, "Error Code");
      var errorMessage = BssConstants.InstallBase.IMPLIED_ACTION_IS_DEVICE_TRANSFER;
      Assert.AreEqual(errorMessage, Validator.Errors[0].Item2, "Error Message");
    }

    [TestMethod]
    public void Validate_IsOwnershipTransfer_ToNewRegisteredDealer_ActiveServiceExists_Error()
    {
      Context.Asset.AssetId = IdGen.GetId();
      Context.Device.Id = IdGen.GetId();
      Context.Device.DeviceState = DeviceStateEnum.Subscribed;
      Context.Device.AssetId = Context.Asset.AssetId;
      Context.Owner.Id = IdGen.GetId();
      Context.Device.OwnerId = IdGen.GetId();
      Context.Device.Owner.IsActive = true;
      Context.Device.Owner.RegisteredDealerId = IdGen.GetId();
      Context.Owner.RegisteredDealerId = IdGen.GetId();

      Validator.Validate(Context);

      Assert.IsTrue(Context.IsOwnershipTransfer());
      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(1, Validator.Errors.Count, "Errors");
      Assert.AreEqual(BssFailureCode.ActiveDeviceRegisteredDlrXfer, Validator.Errors[0].Item1, "Error Code");
      var errorMessage = BssConstants.InstallBase.DEVICE_WITH_ACTIVE_SERVICE_TRANSFER_TO_DIFFERENT_REGISTERED_DEALER;
      Assert.AreEqual(errorMessage, Validator.Errors[0].Item2, "Error Message");
    }

    [TestMethod]
    public void Validate_DeviceTransfer_ExistingDeviceHasCoreService_Error()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Device.IbKey = IdGen.StringId();
      Context.Device.Type = DeviceTypeEnum.PL121;
      Context.Device.DeviceState = DeviceStateEnum.Subscribed;
      Context.ImpliedAction = BssImpliedAction.DeviceTransfer;

      Validator.Validate(Context);

      Assert.IsTrue(Context.IsDeviceTransfer());
      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings.");
      Assert.AreEqual(1, Validator.Errors.Count, "Errors");
      Assert.AreEqual(BssFailureCode.ActiveServiceExistsForDevice, Validator.Errors[0].Item1, "Error Code");
      var errorMessage = string.Format(BssConstants.InstallBase.ACTIVE_SERVICE_EXISTS_FOR_DEVICE_ACTION_NOT_VALID, "Transfer", Context.Device.IbKey);
      Assert.AreEqual(errorMessage, Validator.Errors[0].Item2, "Error Message");
    }

    [TestMethod]
    public void Validate_DeviceTransfer_ExistingDeviceDoesNotHaveCoreService_Success()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Device.IbKey = IdGen.StringId();
      Context.Device.Type = DeviceTypeEnum.PL121;
      Context.Device.DeviceState = DeviceStateEnum.Provisioned;
      Context.ImpliedAction = BssImpliedAction.DeviceTransfer;

      Validator.Validate(Context);

      Assert.IsTrue(Context.IsDeviceTransfer());
      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings.");
      Assert.AreEqual(0, Validator.Errors.Count, "Errors");
    }

    [TestMethod]
    public void Validate_DeviceReplacement_ExistingDeviceIsActive_Error()
    {
      Context = new AssetDeviceContext();
      Context.IBDevice.Type = DeviceTypeEnum.PL121; // Only matters for the message
      Context.IBDevice.IbKey = "IBKEY";

      Context.Device.Id = IdGen.GetId();
      Context.Device.Type = DeviceTypeEnum.PL321;
      Context.Device.DeviceState = DeviceStateEnum.Subscribed;

      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.DeviceState = DeviceStateEnum.Subscribed;

      Context.ImpliedAction = BssImpliedAction.DeviceReplacement;

      var fake = new BssServiceViewServiceFake(true);
      Services.ServiceViews = () => fake;

      Validator.Validate(Context);

      Assert.IsTrue(Context.IsDeviceReplacement());
      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(1, Validator.Errors.Count, "Errors");
      Assert.AreEqual(BssFailureCode.ActiveServiceExistsForDevice, Validator.Errors[0].Item1, "Error Code");
      var errorDetail = string.Format(BssConstants.InstallBase.ACTIVE_SERVICE_EXISTS_FOR_DEVICE_ACTION_NOT_VALID, "Replacement", Context.IBDevice.IbKey);
      StringAssert.Contains(Validator.Errors[0].Item2, errorDetail, "Error Detail");
      Assert.IsTrue(fake.WasExecuted, "Service Called");
    }

    [TestMethod]
    public void Validate_DeviceReplacement_ExistingDeviceDoesNotSupportServices_Error()
    {
      Context = new AssetDeviceContext();
      Context.IBDevice.Type = DeviceTypeEnum.PL321; // Only matters for the error message

      Context.Device.Id = IdGen.GetId();
      Context.Device.Type = DeviceTypeEnum.PL121;
      Context.Device.DeviceState = DeviceStateEnum.Provisioned;

      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.DeviceState = DeviceStateEnum.Subscribed;

      Context.ImpliedAction = BssImpliedAction.DeviceReplacement;

      var fake = new BssServiceViewServiceFake(false);
      Services.ServiceViews = () => fake;

      Validator.Validate(Context);

      Assert.IsTrue(Context.IsDeviceReplacement());
      Assert.AreEqual(0, Validator.Warnings.Count, "Warning");
      Assert.AreEqual(1, Validator.Errors.Count, "Error");
      Assert.AreEqual(BssFailureCode.DeviceReplaceNotValid, Validator.Errors[0].Item1, "Error Code");
      Assert.IsTrue(fake.WasExecuted, "Service Called");
    }

    [TestMethod]
    public void Validate_DeviceReplacement_ExistingDeviceSupportsServices_Success()
    {
      Context.IBDevice.IbKey = IdGen.StringId();
      Context.IBDevice.Type = DeviceTypeEnum.PL121;

      Context.Device.Id = IdGen.GetId();
      Context.Device.DeviceState = DeviceStateEnum.Provisioned;

      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.DeviceState = DeviceStateEnum.Subscribed;

      var fake = new BssServiceViewServiceFake(true);
      Services.ServiceViews = () => fake;

      Validator.Validate(Context);

      Assert.IsTrue(Context.IsDeviceReplacement());

      Assert.AreEqual(0, Validator.Warnings.Count, "Warning");
      Assert.AreEqual(0, Validator.Errors.Count, "Error");
      Assert.IsTrue(fake.WasExecuted, "Service Called");
    }

    [TestMethod]
    public void validate_DeviceReplacement_Ownershiptransfer_Success()
    {
      Context = new AssetDeviceContext();

      Context.Owner.Id = IdGen.GetId();

      Context.IBDevice.Type = DeviceTypeEnum.PL121;

      Context.Device.Id = IdGen.GetId();
      Context.Device.DeviceState = DeviceStateEnum.Provisioned;
      Context.Device.OwnerId = IdGen.GetId();

      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.DeviceState = DeviceStateEnum.Subscribed;

      var fake = new BssServiceViewServiceFake(true);
      Services.ServiceViews = () => fake;

      Validator.Validate(Context);

      Assert.IsTrue(Context.IsDeviceReplacement() && Context.IsOwnershipTransfer());

      Assert.AreEqual(0, Validator.Warnings.Count, "NWarning");
      Assert.AreEqual(0, Validator.Errors.Count, "Error");
      Assert.IsTrue(fake.WasExecuted, "Service Called");
    }

    [TestMethod]
    public void Validate_DeviceHasInvalidStore()
    {
      Context = new AssetDeviceContext();
      Context.Device.Id = IdGen.GetId();
      Context.IBDevice.GpsDeviceId = IdGen.StringId();
      Context.Device.GpsDeviceId = Context.IBDevice.GpsDeviceId;

      Context.Device.Asset.StoreID = 2;
      Validator.Validate(Context);

      Assert.AreEqual(1, Validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.DeviceRelatedToDifferentStore, Validator.Errors[0].Item1, "Error Code");
    }

    [TestMethod]
    public void Validate_AssetHasInvalidStore()
    {
      Context = new AssetDeviceContext();
      Context.Device.Id = IdGen.GetId();
      Context.IBDevice.GpsDeviceId = IdGen.StringId();
      Context.Device.GpsDeviceId = Context.IBDevice.GpsDeviceId;

      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.StoreID = 2;
      Validator.Validate(Context);

      Assert.AreEqual(1, Validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.AssetRelatedToDifferentStore, Validator.Errors[0].Item1, "Error Code");
    }

    [TestMethod]
    public void Validate_DeviceHasValidStore()
    {
      Context = new AssetDeviceContext();
      Context.Device.Id = IdGen.GetId();
      Context.IBDevice.GpsDeviceId = IdGen.StringId();
      Context.Device.GpsDeviceId = Context.IBDevice.GpsDeviceId;

      Context.Device.Asset.StoreID = 1;
      Validator.Validate(Context);

      Assert.AreEqual(0, Validator.Errors.Count);
    }

    [TestMethod]
    public void Validate_AssetHasValidStore()
    {
      Context = new AssetDeviceContext();
      Context.Device.Id = IdGen.GetId();
      Context.IBDevice.GpsDeviceId = IdGen.StringId();
      Context.Device.GpsDeviceId = Context.IBDevice.GpsDeviceId;

      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.StoreID = 0;
      Validator.Validate(Context);

      Assert.AreEqual(0, Validator.Errors.Count);
    }
  }
}
