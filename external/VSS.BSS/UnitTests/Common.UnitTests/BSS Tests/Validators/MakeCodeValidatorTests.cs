using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class MakeCodeValidatorTests : BssUnitTestBase
  {
    private MakeCodeValidator _validator;

    [TestInitialize]
    public void MakeCodeValidatorTests_Init()
    {
      _validator = new MakeCodeValidator();
    }

    [DatabaseTest]
    [TestMethod]
    public void Validate_MakeCodeDoesNotExist_Error()
    {
      var message = BSS.IBCreated.MakeCode("DOES NOT EXIST").Build();

      _validator.Validate(message);

      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(1, _validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.MakeCodeInvalid,_validator.Errors[0].Item1);
      StringAssert.Contains(_validator.Errors[0].Item2, string.Format(BssConstants.InstallBase.MAKE_CODE_NOT_VALID, message.MakeCode));
    }

    [DatabaseTest]
    [TestMethod]
    public void Validate_MakeCodeExists_Error()
    {
      var message = BSS.IBCreated.MakeCode("CAT").Build();

      _validator.Validate(message);

      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(0, _validator.Errors.Count);
    } 
  }
}
