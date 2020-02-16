using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceAssetContextReplacedValidatorTests : BssUnitTestBase
  {
    DeviceAssetContextReplacedValidator Validator;
    DeviceAssetContext Context;

    [TestInitialize]
    public void TestInitialize()
    {
      Validator = new DeviceAssetContextReplacedValidator();
      Context = new DeviceAssetContext();

      Context.NewIBKey = IdGen.StringId();
      Context.NewDeviceAsset.IbKey = Context.NewIBKey;
      Context.NewDeviceAsset.Type = DeviceTypeEnum.Series523;
      Context.NewDeviceAsset.DeviceState = DeviceStateEnum.Provisioned;
      Context.NewDeviceAsset.AssetId = IdGen.GetId();

      Context.OldIBKey = IdGen.StringId();
      Context.OldDeviceAsset.IbKey = Context.OldIBKey;
      Context.OldDeviceAsset.DeviceState = DeviceStateEnum.Subscribed;
    }

    [TestMethod]
    public void Validate_NoErrors_Success()
    {
      Validator.Validate(Context);

      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(0, Validator.Errors.Count, "Errors");
    }

    [TestMethod]
    public void Validate_OldIBKeySameAsNewIBKey_Error()
    {
      Context.NewDeviceAsset.IbKey = IdGen.StringId();
      Context.OldDeviceAsset.IbKey = Context.NewDeviceAsset.IbKey;

      Validator.Validate(Context);

      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(1, Validator.Errors.Count, "Errors");
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, Validator.Errors.First().Item1, "Error Code");
      var errorMessage = string.Format(BssConstants.DeviceReplacement.OLD_IBKEY_AND_NEW_IBKEY_ARE_EQUAL, Context.OldIBKey, Context.NewIBKey);
      Assert.AreEqual(errorMessage, Validator.Errors.First().Item2, "Error Message");
    }

    [TestMethod]
    public void Validate_NewDeviceIsNotInstalledOnAnAsset_Error()
    {
      Context.NewDeviceAsset.AssetId = 0;

      Validator.Validate(Context);

      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(1, Validator.Errors.Count, "Errors");
      Assert.AreEqual(BssFailureCode.DeviceReplaceNotValid, Validator.Errors.First().Item1, "Error Code");
      var errorMessage = string.Format(BssConstants.DeviceReplacement.NEW_DEVICE_NOT_INSTALLED_OR_OLD_DEVICE_NOT_REMOVED);
      Assert.AreEqual(errorMessage, Validator.Errors.First().Item2, "Error Message");
    }

    [TestMethod]
    public void Validate_OldDeviceWasNotRemovedFromAsset_Error()
    {
      Context.OldDeviceAsset.AssetId = IdGen.GetId();

      Validator.Validate(Context);

      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(1, Validator.Errors.Count, "Errors");
      Assert.AreEqual(BssFailureCode.DeviceReplaceNotValid, Validator.Errors.First().Item1, "Error Code");
      var errorMessage = string.Format(BssConstants.DeviceReplacement.NEW_DEVICE_NOT_INSTALLED_OR_OLD_DEVICE_NOT_REMOVED);
      Assert.AreEqual(errorMessage, Validator.Errors.First().Item2, "Error Message");
    }

    [TestMethod]
    public void Validate_NewDeviceIsActive_Error()
    {
      Context.NewDeviceAsset.DeviceState = DeviceStateEnum.Subscribed;

      Validator.Validate(Context);

      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(1, Validator.Errors.Count, "Errors");
      Assert.AreEqual(BssFailureCode.NewDeviceHasServices, Validator.Errors.First().Item1, "Error Code");
      var errorMessage = string.Format(BssConstants.DeviceReplacement.NEW_DEVICE_HAS_ACTIVE_SERVICES, Context.NewIBKey);
      Assert.AreEqual(errorMessage, Validator.Errors.First().Item2, "Error Message");
    }

    [DatabaseTest]
    [TestMethod]
    public void Validate_OldDeviceIsNotActive_Error()
    {
      Context.OldDeviceAsset.DeviceState = DeviceStateEnum.Provisioned;

      Validator.Validate(Context);

      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(1, Validator.Errors.Count, "Errors");
      Assert.AreEqual(BssFailureCode.DeviceReplaceNotValid, Validator.Errors.First().Item1, "Error Code");
      var errorMessage = string.Format(BssConstants.DeviceReplacement.OLD_DEVICE_DOES_NOT_HAVE_ACTIVE_SERVICE);
      Assert.AreEqual(errorMessage, Validator.Errors.First().Item2, "Error Message");
    }

    [TestMethod]
    public void Validate_NewDeviceDoesNotSupportOldDeviceServices_Error()
    {
      var fake = new BssServiceViewServiceFake(false);
      Services.ServiceViews = () => fake;

      Validator.Validate(Context);

      Assert.AreEqual(0, Validator.Warnings.Count, "Warnings");
      Assert.AreEqual(1, Validator.Errors.Count, "Errors");
      Assert.AreEqual(BssFailureCode.DeviceReplaceNotValid, Validator.Errors.First().Item1, "Error Code");
      var errorMessage = string.Format(BssConstants.DeviceReplacement.NEW_DEVICE_DOES_NOT_SUPPORT_OLD_DEVICE_SERVICES, Context.NewIBKey, Context.OldIBKey);
      Assert.AreEqual(errorMessage, Validator.Errors.First().Item2, "Error Message");
      Assert.IsTrue(fake.WasExecuted, "Was Executed");
    }
  }
}
