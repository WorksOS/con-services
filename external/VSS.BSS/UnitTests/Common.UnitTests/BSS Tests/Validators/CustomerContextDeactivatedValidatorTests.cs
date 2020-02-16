using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class CustomerContextDeactivatedValidatorTests : BssUnitTestBase
  {

    protected CustomerContextDeactivatedValidator Validator;

    [TestInitialize]
    public void CustomerContextDeactivatedValidatorTests_Init()
    {
      Validator = new CustomerContextDeactivatedValidator();
    }

    [TestMethod]
    public void Validate_ValidContext_NoErrorsAndNoWarnings()
    {
      var context = new CustomerContext();
      context.Id = IdGen.GetId();
      context.IsActive = true;
      Validator.Validate(context);

      Assert.AreEqual(0, Validator.Warnings.Count);
      Assert.AreEqual(0, Validator.Errors.Count);
    }

    [TestMethod]
    public void Validate_CustomerDoesNotExist_Error()
    {
      var context = new CustomerContext();
      context.IsActive = true;
      Validator.Validate(context);

      Assert.AreEqual(0, Validator.Warnings.Count);
      Assert.AreEqual(1, Validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.CustomerDoesNotExist, Validator.Errors[0].Item1);
    }

    [TestMethod]
    public void Validate_CustomerInactive_Warning()
    {
      var context = new CustomerContext();
      context.Id = IdGen.GetId();
      context.IsActive = false;
      Validator.Validate(context);

      Assert.AreEqual(1, Validator.Warnings.Count);
      Assert.AreEqual(0, Validator.Errors.Count);
      StringAssert.Contains(Validator.Warnings[0], string.Format(BssConstants.Hierarchy.CUSTOMER_IS_INACTIVE, context.New.BssId));
    }
  }
}
