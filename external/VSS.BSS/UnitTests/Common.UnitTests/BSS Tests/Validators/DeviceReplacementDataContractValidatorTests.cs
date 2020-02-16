using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceReplacementDataContractValidatorTests : BssUnitTestBase
  {
    DeviceReplacementDataContractValidator validator;

    [TestInitialize]
    public void DeviceReplacementDataContractValidatorInit()
    {
      validator = new DeviceReplacementDataContractValidator();
    }

    #region Action

    [TestMethod]
    public void InvalidAction_Exception()
    {
      var message = new DeviceReplacement { Action = "Activated" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(BssFailureCode.ActionInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.ActionInvalid).First().Item1);
      Assert.AreEqual(string.Format(BssConstants.ACTION_INVALID_FOR_MESSAGE, "Activated", "DeviceReplacement"), validator.Errors.Where(t => t.Item1 == BssFailureCode.ActionInvalid).First().Item2);
    }

    [TestMethod]
    public void ValidAction_Success()
    {
      var message = new DeviceReplacement { Action = "Swapped" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.ActionInvalid).Count());
    }

    #endregion

    #region Old IBKey

    [TestMethod]
    public void NumericOldIBKey_Success()
    {
      var message = new DeviceReplacement { OldIBKey = IdGen.GetId().ToString(), NewIBKey = IdGen.GetId().ToString() };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyInvalid).Count());
    }

    [TestMethod]
    public void EmptyOldIBKey_Error()
    {
      var message = new DeviceReplacement { };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyInvalid).First().Item1);
    }

    [TestMethod]
    public void ZeroOldIBKey_Error()
    {
      var message = new DeviceReplacement { OldIBKey = "0" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyInvalid).First().Item1);
    }

    [TestMethod]
    public void AlphaNumericOldIBKey_Error()
    {
      var message = new DeviceReplacement { OldIBKey = "xyz123" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyInvalid).First().Item1);
    }

    #endregion

    #region New IBKey

    [TestMethod]
    public void NumericNewIBKey_Success()
    {
      var message = new DeviceReplacement { NewIBKey = IdGen.GetId().ToString(), OldIBKey = IdGen.GetId().ToString() };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyInvalid).Count());
    }

    [TestMethod]
    public void EmptyNewIBKey_Error()
    {
      var message = new DeviceReplacement { };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyInvalid).First().Item1);
    }

    [TestMethod]
    public void ZeroNewIBKey_Error()
    {
      var message = new DeviceReplacement { NewIBKey = "0" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyInvalid).First().Item1);
    }

    [TestMethod]
    public void AlphaNumericNewIBKey_Error()
    {
      var message = new DeviceReplacement { NewIBKey = "xyz123" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyInvalid).First().Item1);
    }

    #endregion
  }
}
