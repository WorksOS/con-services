using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AccountHierarchyDataContractValidatorTests : BssUnitTestBase
  {
    private AccountHierarchyDataContractValidator validator;
    
    [TestInitialize]
    public void AccountHierarchyDataContractValidatorTestsInit()
    {
      validator = new AccountHierarchyDataContractValidator();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void NullMessage_Exception()
    {
      validator.Validate(null);
    }

    #region BSSID

    [TestMethod]
    public void NumericBSSID_Success()
    {
      validator.Validate(BSS.AHCreated.BssId("1234").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void NegativeNumericBSSID_Success()
    {
      validator.Validate(BSS.AHCreated.BssId("-1234").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void EmptyBSSID_Error()
    {
      validator.Validate(BSS.AHCreated.BssId(string.Empty).Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.BssIdInvalid, validator.Errors[0].Item1);
    }

    [TestMethod]
    public void AlphaNumericBSSID_Error()
    {
      validator.Validate(BSS.AHCreated.BssId("abc123").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.BssIdInvalid, validator.Errors[0].Item1);
    }

    #endregion

    #region ParentBSSID

    [TestMethod]
    public void NumericParentBSSID_Success()
    {
      validator.Validate(BSS.AHCreated.ParentBssId("1234").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void NegativeNumericParentBSSID_Success()
    {
      validator.Validate(BSS.AHCreated.ParentBssId("-1234").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void AlphaNumericParentBSSID_Error()
    {
      validator.Validate(BSS.AHCreated.ParentBssId("abc123").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.ParentBssIdInvalid, validator.Errors[0].Item1);
    }

    #endregion

    #region RelationshipID

    [TestMethod]
    public void NumericRelationshipID_Success()
    {
      validator.Validate(BSS.AHCreated.RelationshipId("1234").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void NegativeNumericRelationshipID_Success()
    {
      validator.Validate(BSS.AHCreated.RelationshipId("-1234").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void AlphaNumericRelationshipID_Error()
    {
      validator.Validate(BSS.AHCreated.RelationshipId("abc123").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.RelationshipIdInvalid, validator.Errors[0].Item1);
    }

    #endregion

    #region Customer Type

    [TestMethod]
    public void EmptyCustomerType_Error()
    {
      var message = BSS.AHCreated.Build();
      message.CustomerType = string.Empty;
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.CustomerTypeInvalid, validator.Errors[0].Item1);
    }

    [TestMethod]
    public void StringCustomerType_Success()
    {
      var message = BSS.AHCreated.Build();
      message.CustomerType = "abc";
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void AlphaNumericCustomerType_Error()
    {
      var message = BSS.AHCreated.Build();
      message.CustomerType = "abc123";
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.CustomerTypeInvalid, validator.Errors[0].Item1);
    }

    [TestMethod]
    public void WhitespaceCustomerType_Error()
    {
      var message = BSS.AHCreated.Build();
      message.CustomerType = "white space";
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.CustomerTypeInvalid, validator.Errors[0].Item1);
    }

    #endregion
  }
}
