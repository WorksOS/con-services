using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceStatusContextDeRegisteredValidatorTests : BssUnitTestBase
  {
    DeviceStatusContextDeRegisteredValidator validator;
    DeviceStatusContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new DeviceStatusContextDeRegisteredValidator();
      context = new DeviceStatusContext
      {
        IBKey = IdGen.StringId(),
        DeviceAsset = new DeviceAssetDto
        {
          AssetId = IdGen.GetId(),
          DeviceId = IdGen.GetId(),
          DeviceState = DeviceStateEnum.Provisioned,
        },
        Status = "dereg_tech",
      };
    }

    [TestMethod]
    public void Validae_DeviceDeregisteredStore_RequestForDeregister_Failure()
    {
      context.DeviceAsset.DeviceState = DeviceStateEnum.DeregisteredTechnician;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
      Assert.AreEqual(string.Format(BssConstants.DeviceRegistration.DEVICE_ALREADY_DEREGISTERED, context.IBKey, "DeRegistered"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validae_DeregisteredTechnician_RequestForDeregister_Failure()
    {
      context.DeviceAsset.DeviceState = DeviceStateEnum.DeregisteredTechnician;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
      Assert.AreEqual(string.Format(BssConstants.DeviceRegistration.DEVICE_ALREADY_DEREGISTERED, context.IBKey, "DeRegistered"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validae_DeviceDeregistered_RequestForRegister_Success()
    {
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(0, validator.Errors.Count(), "No Errors expected");
    }

    [TestMethod]
    public void Validate_InvalidStatus_Failure()
    {
      context.Status = "reg";
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
      Assert.AreEqual(string.Format(BssConstants.DeviceRegistration.DEVICE_STATUS_NOT_VALID, context.Status, "DeRegistered"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_ValidStatus_Success()
    {
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(0, validator.Errors.Count(), "No Errors expected");
    }

    [TestMethod]
    public void Validate_DeviceHasActiveServicePlan_Failure()
    {
      var fake = new BssServiceViewServiceFake(true);
      fake.HasActiveService = true;
      Services.ServiceViews = () => fake;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Count(), "One Error is Expected");
      Assert.AreEqual(string.Format(BssConstants.ACTIVE_SERVICE_EXISTS_FOR_DEVICE, context.IBKey), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_DeviceDoesNotHaveActiveServicePlan_Failure()
    {
      var fake = new BssServiceViewServiceFake(false);
      Services.ServiceViews = () => fake;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(0, validator.Errors.Count(), "No Errors expected");
    }
  }
}
