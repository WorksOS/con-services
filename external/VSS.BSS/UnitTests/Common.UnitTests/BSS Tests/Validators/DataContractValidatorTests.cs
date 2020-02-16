using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DataContractValidatorTests
  {

    private DataContractValidator<AccountHierarchy> validator;

    [TestInitialize]
    public void AccountHierarchyDataContractValidatorTestsInit()
    {
      validator = new DataContractValidator<AccountHierarchy>();
    }

    #region ControlNumber

    [TestMethod]
    public void NumericControlNumber_Success()
    {
      validator.Validate(BSS.AHCreated.ControlNumber("1234").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void NegativeNumericControlNumber_Success()
    {
      validator.Validate(BSS.AHCreated.ControlNumber("-1234").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void EmptyControlNumber_Error()
    {
      validator.Validate(BSS.AHCreated.ControlNumber(string.Empty).Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.ControlNumberInvalid, validator.Errors[0].Item1);
    }

    [TestMethod]
    public void AlphaNumericControlNumber_Error()
    {
      validator.Validate(BSS.AHCreated.ControlNumber("abc123").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.ControlNumberInvalid, validator.Errors[0].Item1);
    }

    [TestMethod]
    public void ZeroControlNumber_Error()
    {
      validator.Validate(BSS.AHCreated.ControlNumber("0").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.ControlNumberNotDefined, validator.Errors[0].Item1);
    }

    #endregion

    #region Action

    [TestMethod]
    public void StringAction_Success()
    {
      var message = BSS.AHCreated.Build();
      message.Action = "abc";
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count());
      Assert.AreEqual(0, validator.Errors.Count());
    }

    [TestMethod]
    public void EmptyAction_Exceptions()
    {
      var message = BSS.AHCreated.Build();
      message.Action = string.Empty;
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count());
      Assert.AreEqual(1, validator.Errors.Count());
      Assert.AreEqual(BssFailureCode.ActionInvalid, validator.Errors[0].Item1);
    }

    [TestMethod]
    public void AlphaNumericAction_Error()
    {
      var message = BSS.AHCreated.Build();
      message.Action = "abc123";
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count());
      Assert.AreEqual(1, validator.Errors.Count());
      Assert.AreEqual(BssFailureCode.ActionInvalid, validator.Errors[0].Item1);
    }

    [TestMethod]
    public void WhitespaceAction_Error()
    {
      var message = BSS.AHCreated.Build();
      message.Action = "white space";
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count());
      Assert.AreEqual(1, validator.Errors.Count());
      Assert.AreEqual(BssFailureCode.ActionInvalid, validator.Errors[0].Item1);
    }

    #endregion

    #region ActionUTC

    [TestMethod]
    public void EmptyActionUTC_Error()
    {
      var message = BSS.AHCreated.Build();
      message.ActionUTC = string.Empty;
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.ActionUtcInvalid, validator.Errors[0].Item1);
    }

    [TestMethod]
    public void InvalidDateActionUTC_Error()
    {
      var message = BSS.AHCreated.Build();
      message.ActionUTC = "111/111/111";
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.ActionUtcInvalid, validator.Errors[0].Item1);
    }

    [TestMethod]
    public void ValidActionUTC_UTC_Success()
    {
      var message = BSS.AHCreated.Build();
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void ValidActionUTC_ISO_Success()
    {
      var message = BSS.AHCreated.Build();
      message.ActionUTC = DateTime.UtcNow.ToIso8601DateTimeString();
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    #endregion

    #region SequenceNumber

    [TestMethod]
    public void SequenceNumberZero_Error()
    {
      validator.Validate(BSS.AHCreated.SequenceNumber(0).Build());
      Assert.AreEqual(0, validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(BssFailureCode.SequenceNumberNotDefined, validator.Errors.Where(t => t.Item1 == BssFailureCode.SequenceNumberNotDefined).First().Item1);
    }

    [TestMethod]
    public void ValidSequenceNumber_Success()
    {
      validator.Validate(BSS.AHCreated.SequenceNumber(IdGen.GetId()).Build());
      Assert.AreEqual(0, validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.SequenceNumberNotDefined).Count());
    }

    #endregion

  }
}
