using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceStatusContextValidatorTests : BssUnitTestBase
  {
    DeviceStatusContextValidator validator;
    DeviceStatusContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new DeviceStatusContextValidator();
      context = new DeviceStatusContext
      {
        IBKey = IdGen.StringId(),
        DeviceAsset = new DeviceAssetDto
        {
          AssetId = IdGen.GetId(),
          DeviceId = IdGen.GetId(),
          OwnerBSSID = IdGen.StringId(),
          Type = DeviceTypeEnum.PL121,
        }
      };
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Validate_Null_Context_Exception()
    {
      context = null;
      validator.Validate(context);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Validate_NullAssetDevice_Context_Exception()
    {
      context.DeviceAsset = null;
      validator.Validate(context);
    }

    [TestMethod]
    public void Validate_AssetAndDeviceDoesNotExists_Error()
    {
      context.DeviceAsset.DeviceId = 0;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Count(), "Two Errors are Expected");
      Assert.AreEqual(string.Format(BssConstants.IBKEY_DOES_NOT_EXISTS, context.IBKey, string.Empty), validator.Errors[0].Item2);
    }

    [TestMethod]
    [DatabaseTest]
    public void Validate_AssetAndDeviceExists_Success()
    {
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(0, validator.Errors.Count(), "One Error is Expected");
    }

    [TestMethod]
    [DatabaseTest]
    public void Validate_DeviceNotAssoicatedToValidOwner_Error()
    {
      context.DeviceAsset.OwnerBSSID = string.Empty;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
      Assert.AreEqual(string.Format(BssConstants.DEVICE_NOT_ASSOCIATED_WITH_VALID_CUSTOMER, context.IBKey), validator.Errors[0].Item2);
    }

    [TestMethod]
    [DatabaseTest]
    public void Validate_DeviceAssoicatedToValidOwner_Success()
    {
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(0, validator.Errors.Count(), "No Errors Expected");
    }

    [TestMethod]
    public void Validate_DeviceTypeDoesNotSupport_DeviceStateChange_Error()
    {
      context.DeviceAsset.Type = DeviceTypeEnum.Series521;
      validator.Validate(context);
      Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
      Assert.AreEqual(string.Format(BssConstants.DeviceRegistration.DEVICE_REGISTRATION_NOT_SUPPORTED, context.DeviceAsset.Type), validator.Errors[0].Item2);
    }

    [TestMethod]
    [DatabaseTest]
    public void Validate_DeviceTypeSupports_DeviceStateChange_PL121_Success()
    {
      context.DeviceAsset.Type = DeviceTypeEnum.PL121;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(0, validator.Errors.Count(), "No Errors Expected");
    }

    [TestMethod]
    [DatabaseTest]
    public void Validate_DeviceTypeSupports_DeviceStateChange_PL321_Success()
    {
      context.DeviceAsset.Type = DeviceTypeEnum.PL321;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(0, validator.Errors.Count(), "No Errors Expected");
    }
  }
}
