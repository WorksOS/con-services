using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ServicePlanCancelledValidatorTests : BssUnitTestBase
  {
    ServicePlanCancelledValidator validator;
    DeviceServiceContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new ServicePlanCancelledValidator();

      var ibkey = IdGen.StringId();

      context = new DeviceServiceContext
      {
        IBKey = ibkey,
        ServiceTerminationDate = DateTime.UtcNow,
        ExistingDeviceAsset = { DeviceId = IdGen.GetId(), IbKey = ibkey },
        ExistingService = { ServiceID = IdGen.GetId(), ActivationKeyDate = DateTime.UtcNow.KeyDate(), CancellationKeyDate = DateTime.UtcNow.KeyDate(), DeviceID = IdGen.GetId(), IBKey = IdGen.StringId(), GPSDeviceID = IdGen.StringId() }
      };
    }

    [TestMethod]
    public void Validate_OwnerVisibilityDateExists_Failure()
    {
      context.OwnerVisibilityDate = DateTime.UtcNow;
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.OwnerVisibilityDateDefined).Count(), "Owner Visibility Date defined error is expected.");
      Assert.AreEqual(string.Format(BssConstants.ServicePlan.OWNER_VISIBILITY_DATE, string.Empty, "Cancelled"),
          validator.Errors.Where(t => t.Item1 == BssFailureCode.OwnerVisibilityDateDefined).First().Item2, "Error messages are expected to match.");
    }

    [TestMethod]
    public void Validate_OwnerVisibilityDateDoesNotExists_Success()
    {
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.OwnerVisibilityDateDefined).Count(), "Owner Visibility Date defined error is not expected.");
    }

    [TestMethod]
    public void Validate_TerminationDateDoesNotExists_Error()
    {
      context.ServiceTerminationDate = null;
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceCancelDateNotDefined).Count(), "Service Termination Date not defined error is expected.");
      Assert.AreEqual(string.Format(BssConstants.ServicePlan.SERVICE_TERMINATION_DATE, "not", "Cancelled"),
          validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceCancelDateNotDefined).First().Item2, "Error messages are expected to match.");
    }

    [TestMethod]
    public void Validate_TerminationDateExists_Success()
    {
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceCancelDateNotDefined).Count(), "Service Termination Date not defined error is not expected.");
    }

    [TestMethod]
    public void Validate_TerminationDate_PriorToActicationDate_Falure()
    {
      context.ServiceTerminationDate = DateTime.UtcNow.AddDays(-1);
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceCancelDateBeforeActDate).Count(), "Service Termination Date is prior to activation date error is expected.");
      Assert.AreEqual(string.Format(BssConstants.ServicePlan.SERVICE_TERMINATION_DATE_IS_PRIOR_TO_ACTIVATION_DATE, context.ServiceTerminationDate.KeyDate(), context.ExistingService.ActivationKeyDate),
          validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceCancelDateBeforeActDate).First().Item2, "Error messages are expected to match.");
    }

    [TestMethod]
    public void Validate_TerminationDate_EqualToActicationDate_Success()
    {
      context.ServiceTerminationDate = DateTime.UtcNow;
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceCancelDateBeforeActDate).Count(), "Service Termination Date is prior to activation date error is not expected.");
    }

    [TestMethod]
    public void Validate_TerminationDate_AfterToActicationDate_Success()
    {
      context.ServiceTerminationDate = DateTime.UtcNow.AddDays(1);
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceCancelDateBeforeActDate).Count(), "Service Termination Date is prior to activation date error is not expected.");
    }

    [TestMethod]
    public void Validate_CancelService_WhichIsAlreadyTerminated_Failure()
    {
      context.ExistingService.ServiceID = IdGen.GetId();
      context.ExistingService.CancellationKeyDate = DateTime.UtcNow.AddDays(-3).KeyDate();
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServceTerminationInvalid).Count(), "Service Termination invalid error is expected.");
      Assert.AreEqual(string.Format(BssConstants.ServicePlan.SERVICE_TERMINATION_NOT_VALID, context.ServiceTerminationDate.KeyDate(), context.ExistingService.CancellationKeyDate),
          validator.Errors.Where(t => t.Item1 == BssFailureCode.ServceTerminationInvalid).First().Item2, "Error messages are expected to match.");
    }

    [TestMethod]
    public void Validate_CancelService_ValidTerminationDate_Success()
    {
      context.ExistingService.ServiceID = IdGen.GetId();
      context.ServiceTerminationDate = DateTime.UtcNow.AddDays(-2);
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServceTerminationInvalid).Count(), "Service Termination invalid error is not expected.");
    }

    [TestMethod]
    public void Validate_CancelService_ValidFutureTerminationDate_Success()
    {
      context.ExistingService.ServiceID = IdGen.GetId();
      context.ExistingService.CancellationKeyDate = DateTime.UtcNow.AddDays(2).KeyDate();
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServceTerminationInvalid).Count(), "Service Termination invalid error is not expected.");
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
    public void Validate_ContextIBKeyAndServiceIBKeyMatch_Success()
    {
      context.ExistingService.DeviceID = context.ExistingDeviceAsset.DeviceId;
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceNotAssociatedWithDevice).Count(), "Service not associated with the device error is not expected.");
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
    public void Validate_ServiceExists_Success()
    {
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceDoesNotExist).Count(), "Service not Exists error is not expected.");
    }
  }
}
