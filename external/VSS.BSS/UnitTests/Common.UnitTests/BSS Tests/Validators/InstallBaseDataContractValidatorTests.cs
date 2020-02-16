using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class InstallBaseDataContractValidatorTests : BssUnitTestBase
  {
    private InstallBaseDataContractValidator validator;

    [TestInitialize]
    public void InstallBaseDataContractValidatorTestsInit()
    {
      validator = new InstallBaseDataContractValidator();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void NullMessage_Exception()
    {
      validator.Validate(null);
    }

    #region IBKey

    [TestMethod]
    public void NumericIBKey_Success()
    {
      validator.Validate(BSS.IBCreated.Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void EmptyIBKey_Error()
    {
      validator.Validate(BSS.IBCreated.IBKey(string.Empty).Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, validator.Errors[0].Item1);
    }

    [TestMethod]
    public void AlphaNumericIBKey_Error()
    {
      validator.Validate(BSS.IBCreated.IBKey("xyz123").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, validator.Errors[0].Item1);
    }

    #endregion

    #region Owner BSSID

    [TestMethod]
    public void NumericOwnerBSSID_Success()
    {
      validator.Validate(BSS.IBCreated.Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void NegativeNumericOwnerBSSID_Success()
    {
      validator.Validate(BSS.IBCreated.OwnerBssId("-1234").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void EmptyOwnerBSSID_Error()
    {
      validator.Validate(BSS.IBCreated.OwnerBssId(string.Empty).Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.OwnerBssIdInalid, validator.Errors[0].Item1);
    }

    [TestMethod]
    public void AlphaNumericOwnerBSSID_Error()
    {
      validator.Validate(BSS.IBCreated.OwnerBssId("xyz123").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.OwnerBssIdInalid, validator.Errors[0].Item1);
    }

    #endregion

    #region ModelYear

    [TestMethod]
    public void EmptyModelYear_Success()
    {
      validator.Validate(BSS.IBCreated.ModelYear(string.Empty).Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }
    
    [TestMethod]
    public void NumericModelYear_Success()
    {
      validator.Validate(BSS.IBCreated.ModelYear(string.Empty).Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void AlphaNumericModelYear_Error()
    {
      validator.Validate(BSS.IBCreated.ModelYear("xyz123").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.ModelyearInvalid, validator.Errors[0].Item1);
    }

    [TestMethod]
    public void NegativeNumericModelYear_Error()
    {
      validator.Validate(BSS.IBCreated.ModelYear("-2001").Build());
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.ModelyearInvalid, validator.Errors[0].Item1);
    }

    #endregion
  }
}
