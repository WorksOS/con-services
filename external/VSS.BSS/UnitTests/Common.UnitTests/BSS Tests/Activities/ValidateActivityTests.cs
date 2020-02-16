using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ValidateActivityTests : BssUnitTestBase
  {
    protected Inputs Inputs = new Inputs();
    protected Validator<BssMessageFake> Validator;
    protected Validate<BssMessageFake> Activity;

    [TestInitialize]
    public void ValidateMessageTests_Init()
    {
      Validator = new ValidatorFake<BssMessageFake>();

      Inputs.Add<BssMessageFake>(new BssMessageFake());

      Activity = new Validate<BssMessageFake>(Validator);
    }

    public class ValidatorFake<T> : Validator<T>
    {
      public override void Validate(T message) { }
    }
    public class BssMessageFake{}

    [TestMethod]
    public void CanFake_MessageValidator()
    {
      var result = Activity.Execute(Inputs);

      Assert.IsInstanceOfType(result, typeof(ActivityResult));
      Assert.AreEqual(string.Format(CoreConstants.VALIDATION_PASSED, typeof(BssMessageFake).Name), result.Summary);
    }

    [TestMethod]
    public void Execute_ValidationErrorExists_ReturnsErrorResultWithErrors()
    {
      string validationError = "This is a validation error.";
      Validator.AddError(BssFailureCode.MessageInvalid, validationError);

      var result = Activity.Execute(Inputs);

      Assert.IsInstanceOfType(result, typeof(BssErrorResult));
      var bssError = (BssErrorResult)result;

      Assert.AreEqual(0, Validator.Warnings.Count);
      Assert.AreEqual(1, Validator.Errors.Count);

      Assert.AreEqual(string.Format(
        CoreConstants.VALIDATION_FAILED, 
        typeof(BssMessageFake).Name, 
        validationError), 
        bssError.Summary);
      Assert.AreEqual(BssFailureCode.MessageInvalid, Validator.FirstFailureCode());
    }

    [TestMethod]
    public void Execute_ValidationWarningExists_ReturnsWarningResultWithWarnings()
    {
      string validationWarning = "This is a validation warning.";
      Validator.Warnings.Add(validationWarning);

      var result = Activity.Execute(Inputs);

      Assert.IsInstanceOfType(result, typeof(WarningResult));
      var warningResult = (WarningResult)result;

      Assert.AreEqual(0, Validator.Errors.Count);
      Assert.AreEqual(1, Validator.Warnings.Count);

      Assert.AreEqual(string.Format(
        CoreConstants.VALIDATION_PASSED_WITH_WARNINGS, 
        typeof(BssMessageFake).Name, 
        validationWarning), 
        warningResult.Summary);
    }

    [TestMethod]
    public void Execute_MulitpleValidationErrorExists_ReturnsErrorResultWithErrors()
    {
      string validationError1 = "This is validation error1.";
      string validationError2 = "This is validation error2.";
      Validator.AddError(BssFailureCode.MessageInvalid, validationError1);
      Validator.AddError(BssFailureCode.MessageInvalid, validationError2);

      var result = Activity.Execute(Inputs);

      Assert.IsInstanceOfType(result, typeof(BssErrorResult));
      var bssError = (BssErrorResult)result;

      Assert.AreEqual(0, Validator.Warnings.Count);
      Assert.AreEqual(2, Validator.Errors.Count);

      Assert.AreEqual(string.Format(
        CoreConstants.VALIDATION_FAILED, 
        typeof(BssMessageFake).Name, 
        Validator.Errors.Select(x => x.Item2).ToFormattedString()), 
        bssError.Summary);
      Assert.AreEqual(BssFailureCode.MessageInvalid, bssError.FailureCode);
    }
    
    [TestMethod]
    public void Execute_MulitpleValidationWarningExists_ReturnsWarningResultWithWarnings()
    {
      string validationWarning1 = "This is validation warning1.";
      string validationWarning2 = "This is validation warning2.";
      Validator.Warnings.Add(validationWarning1);
      Validator.Warnings.Add(validationWarning2);

      var result = Activity.Execute(Inputs);

      Assert.IsInstanceOfType(result, typeof(WarningResult));
      var warningResult = (WarningResult)result;

      Assert.AreEqual(0, Validator.Errors.Count);
      Assert.AreEqual(2, Validator.Warnings.Count);

      Assert.AreEqual(string.Format(
        CoreConstants.VALIDATION_PASSED_WITH_WARNINGS, 
        typeof(BssMessageFake).Name, 
        Validator.Warnings.ToFormattedString()), 
        warningResult.Summary);
    }
  }
}
