using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceServiceContextValidatorTests : BssUnitTestBase
  {
    DeviceServiceContextValidator validator;
    DeviceServiceContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new DeviceServiceContextValidator();
      var deviceId = IdGen.GetId();
      var ibkey = IdGen.GetId().ToString();
      context = new DeviceServiceContext
      {
        ServiceType = ServiceTypeEnum.Essentials,
        PlanLineID = IdGen.GetId().ToString(),
        ExistingService = { ServiceID = IdGen.GetId(), ServiceType = ServiceTypeEnum.Essentials },
        ExistingDeviceAsset = { DeviceId = IdGen.GetId() }
      };
    }

    [TestMethod]
    public void Validate_DeviceDoesNotExist_Error()
    {
      context.ExistingDeviceAsset.DeviceId = 0;
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyDoesNotExist).Count(), "IBKey not found error is expected.");
      Assert.AreEqual(string.Format(BssConstants.IBKEY_DOES_NOT_EXISTS, context.IBKey), 
          validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyDoesNotExist).First().Item2, "Error messages are expected to match.");
    }

    [TestMethod]
    public void Validate_DeviceExists_Success()
    {
      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected");
      Assert.AreEqual(0, validator.Errors.Count, "No Errors expected.");
    }

    public void Validate_ServiceTypeDifferent_Error()
    {
      context.ServiceType = ServiceTypeEnum.ManualMaintenanceLog;
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.ServiceTypesDoesNotMatch).Count(), "Service Types not match error is expected.");
      Assert.AreEqual(string.Format(BssConstants.ServicePlan.SERVICE_TYPES_ARE_NOT_EQUAL, context.ServiceType, context.PlanLineID, context.ExistingService.ServiceType),
          validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyDoesNotExist).First().Item2, "Error messages are expected to match.");
    }

    public void Validate_ServiceTypeSame_Error()
    {
      validator.Validate(context);
      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected");
      Assert.AreEqual(0, validator.Errors.Count, "No Errors expected.");
    }
  }
}
