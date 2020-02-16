using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ServicePlanActivatedValidatorTests : BssUnitTestBase
  {
    ServicePlanActivatedValidator validator;
    DeviceServiceContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new ServicePlanActivatedValidator();

      context = new DeviceServiceContext
      {
        IBKey = IdGen.GetId().ToString(),
        OwnerVisibilityDate = DateTime.UtcNow,
        ServiceType = ServiceTypeEnum.Essentials,
        ExistingDeviceAsset = { Type = DeviceTypeEnum.PL321, OwnerBSSID = IdGen.GetId().ToString(), DeviceId = IdGen.GetId() },
        ActionUTC = DateTime.UtcNow
      };
    }

    [TestMethod]
    public void Validate_ServceTypeDoesNotExistsForPlanName_Error()
    {
      context.ServiceType = ServiceTypeEnum.Unknown;
      var fake = new BssServiceViewServiceFake(false);
      Services.ServiceViews = () => fake;
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceTypeDoesNotExists).Count(), "Service type does not exists error is expected.");
      Assert.AreEqual(string.Format(BssConstants.ServicePlan.SERVICE_TYPE_DOES_NOT_EXISTS, context.PartNumber),
          validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceTypeDoesNotExists).First().Item2, "Error messages are expected to match.");
    }

    [TestMethod]
    public void Validate_ActivateManualMaintWithPL321_Error()
    {
      var fake = new BssServiceViewServiceFake(false);
      Services.ServiceViews = () => fake;

      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.DeviceDoesNotSupportService).Count(), "Service type not supported by device error is expected.");
      Assert.AreEqual(string.Format(BssConstants.ServicePlan.SERVICE_TYPE_NOT_SUPPORTED_FOR_DEVICE_TYPE, context.ServiceType, context.ExistingDeviceAsset.Type),
          validator.Errors.Where(t => t.Item1 == BssFailureCode.DeviceDoesNotSupportService).First().Item2, "Error messages are expected to match.");
    }

    [TestMethod]
    public void Validate_ServiceExists_Error()
    {
      context.ExistingService = new ServiceDto { ServiceID = IdGen.GetId(), ServiceType = ServiceTypeEnum.Essentials };
      var fake = new BssServiceViewServiceFake(true);
      Services.ServiceViews = () => fake;

      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceExists).Count(), "Service Exists error is expected.");
      Assert.AreEqual(string.Format(BssConstants.ServicePlan.SERVICE_EXISTS, context.PlanLineID),
          validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceExists).First().Item2, "Error messages are expected to match.");
    }

    [TestMethod]
    public void Validate_DeviceNotAssociatedToValidCustomer_Error()
    {
      var fake = new BssServiceViewServiceFake(true);
      Services.ServiceViews = () => fake;

      context.ExistingDeviceAsset.OwnerBSSID = null;
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.OwnerBssIdDoesNotExist).Count(), "Owner BSSID doesn't exist error is expected.");
      Assert.AreEqual(string.Format(BssConstants.DEVICE_NOT_ASSOCIATED_WITH_VALID_CUSTOMER, context.IBKey), 
          validator.Errors.Where(t => t.Item1 == BssFailureCode.OwnerBssIdDoesNotExist).First().Item2, "Error messages are expected to match.");
    }

    [TestMethod]
    public void Validate_TerminationDateExists_Failure()
    {
      context.ServiceTerminationDate = DateTime.UtcNow;
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceCancelDateDefined).Count(), "Service Termination Date defined error is expected.");
      Assert.AreEqual(string.Format(BssConstants.ServicePlan.SERVICE_TERMINATION_DATE, string.Empty, "Activated"),
          validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceCancelDateDefined).First().Item2, "Error messages are expected to match.");
    }

    [TestMethod]
    public void Validate_DeviceAlreadyHasSameActiveService_Failure()
    {
      var fake = new BssServiceViewServiceFake(true);
      Services.ServiceViews = () => fake;

      context.ExistingService.DifferentServicePlanLineID = IdGen.StringId();
      context.ExistingDeviceAsset.AssetId = IdGen.GetId();

      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.SameServiceExists).Count(), "Service exists error is expected.");
    }
  }
}
