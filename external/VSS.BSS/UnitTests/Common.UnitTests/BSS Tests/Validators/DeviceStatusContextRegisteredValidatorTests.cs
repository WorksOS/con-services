using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceStatusContextRegisteredValidatorTests : BssUnitTestBase
  {
    DeviceStatusContextRegisteredValidator validator;
    DeviceStatusContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new DeviceStatusContextRegisteredValidator();
      context = new DeviceStatusContext
      {
        IBKey = IdGen.StringId(),
        DeviceAsset = { DeviceState = DeviceStateEnum.DeregisteredTechnician },
        Status = DeviceRegistrationStatusEnum.REG.ToString(),
      };
    }

    [TestMethod]
    public void Validate_DeviceStateSubscribed_Error()
    {
      context.DeviceAsset.DeviceState = DeviceStateEnum.Subscribed;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
      Assert.AreEqual(string.Format(BssConstants.DeviceRegistration.DEVICE_ALREADY_DEREGISTERED, context.IBKey, "Registered"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_DeviceStateProvisioned_Error()
    {
      context.DeviceAsset.DeviceState = DeviceStateEnum.Provisioned;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
      Assert.AreEqual(string.Format(BssConstants.DeviceRegistration.DEVICE_ALREADY_DEREGISTERED, context.IBKey, "Registered"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_DeviceStateDeRegisteredStore_Success()
    {
      context.DeviceAsset.DeviceState = DeviceStateEnum.DeregisteredStore;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(0, validator.Errors.Count(), "No Errors expected");
    }

    [TestMethod]
    public void Validate_DeviceStateDeRegisteredTechnician_Success()
    {
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(0, validator.Errors.Count(), "No Errors expected");
    }

    [TestMethod]
    public void Validate_StatusInvalid_DeregStore_Error()
    {
      context.Status = DeviceRegistrationStatusEnum.DEREG_STORE.ToString();
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
      Assert.AreEqual(string.Format(BssConstants.DeviceRegistration.DEVICE_STATUS_NOT_VALID, context.Status, "Registered"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_StatusInvalid_DeregTech_Error()
    {
      context.Status = DeviceRegistrationStatusEnum.DEREG_TECH.ToString();
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
      Assert.AreEqual(string.Format(BssConstants.DeviceRegistration.DEVICE_STATUS_NOT_VALID, context.Status, "Registered"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_StatusValid_Reg_Success()
    {
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(0, validator.Errors.Count(), "No Errors expected");
    }
  }
}
