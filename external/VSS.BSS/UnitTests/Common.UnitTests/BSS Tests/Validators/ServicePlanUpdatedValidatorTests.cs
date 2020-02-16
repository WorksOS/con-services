using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ServicePlanUpdatedValidatorTests : BssUnitTestBase
  {
    ServicePlanUpdatedValidator validator;
    DeviceServiceContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new ServicePlanUpdatedValidator();

      var ibkey = IdGen.GetId().ToString();
      context = new DeviceServiceContext
      {
        IBKey = ibkey,
        ServiceType = ServiceTypeEnum.Essentials,
        OwnerVisibilityDate = DateTime.UtcNow,
        ExistingDeviceAsset = { Type = DeviceTypeEnum.PL321, OwnerBSSID = IdGen.GetId().ToString(), DeviceId = IdGen.GetId(), IbKey = ibkey },
        ExistingService = { ServiceID = IdGen.GetId(), ServiceType = ServiceTypeEnum.Essentials, ActivationKeyDate = DateTime.UtcNow.KeyDate(), DeviceID = IdGen.GetId(), IBKey = IdGen.StringId(), GPSDeviceID = IdGen.StringId() }
      };
    }

    [TestMethod]
    public void Validate_TerminationDateExists_Failure()
    {
      context.ServiceTerminationDate = DateTime.UtcNow;
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceCancelDateDefined).Count(), "Service Termination Date defined error is expected.");
      Assert.AreEqual(string.Format(BssConstants.ServicePlan.SERVICE_TERMINATION_DATE, string.Empty, "Updated"),
          validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceCancelDateDefined).First().Item2, "Error messages are expected to match.");
    }

    [TestMethod]
    public void Validate_ContextIBKeyAndServiceIBKeyDoesNotMatch_Error()
    {
      context.PlanLineID = IdGen.StringId();
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceNotAssociatedWithDevice).Count(), "Service not associated with the device error is expected.");
      Assert.AreEqual(string.Format(BssConstants.ServicePlan.SERVICE_NOT_ASSOCIATED_WITH_DEVICE, context.PlanLineID, context.ExistingService.GPSDeviceID, context.ExistingService.IBKey),
          validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceNotAssociatedWithDevice).First().Item2, "Error messages are expected to match.");
    }

    [TestMethod]
    public void Validate_ServiceDoesNotExists_Error()
    {
      context.ExistingService.ServiceID = 0;
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceDoesNotExist).Count(), "Service not Exists error is expected.");
      Assert.AreEqual(string.Format(BssConstants.ServicePlan.SERVICE_DOES_NOT_EXISTS, context.PlanLineID),
          validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceDoesNotExist).First().Item2, "Error messages are expected to match.");
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
  }
}
