using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceRegistrationDataContractValidatorTests : BssUnitTestBase
  {
    DeviceRegistrationDataContractValidator validator;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new DeviceRegistrationDataContractValidator();
    }

    #region Action

    [TestMethod]
    [ExpectedException(typeof(InvalidCastException))]
    public void InvalidAction_Exception()
    {
      var message = new DeviceRegistration { Action = "Test" };
      validator.Validate(message);
    }

    [TestMethod]
    public void InvalidAction2_Exception()
    {
      var message = new DeviceRegistration { Action = "Test 1" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(BssFailureCode.ActionInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.ActionInvalid).First().Item1);
      Assert.AreEqual(BssConstants.ACTION_NOT_VALID, validator.Errors.Where(t => t.Item1 == BssFailureCode.ActionInvalid).First().Item2);
    }

    [TestMethod]
    public void ValidAction_Success()
    {
      var message = new DeviceRegistration { Action = "Registered" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.ActionInvalid).Count());
    }

    [TestMethod]
    public void ValidAction2_Success()
    {
      var message = new DeviceRegistration { Action = "DeRegistered" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.ActionInvalid).Count());
    }

    #endregion

    #region Status

    [TestMethod]
    public void EmptyStatus_Exception()
    {
      var message = new DeviceRegistration { Status = string.Empty };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(BssFailureCode.DeviceRegistrationStateInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.DeviceRegistrationStateInvalid).First().Item1);
      Assert.AreEqual(string.Format(BssConstants.DeviceRegistration.DEVICE_REGISTRATION_STATUS_NOT_VALID, string.Empty), validator.Errors.Where(t => t.Item1 == BssFailureCode.DeviceRegistrationStateInvalid).First().Item2);
    }

    [TestMethod]
    public void InvalidStatus_Exception()
    {
      var message = new DeviceRegistration { Status = "Test 1" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(BssFailureCode.DeviceRegistrationStateInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.DeviceRegistrationStateInvalid).First().Item1);
      Assert.AreEqual(string.Format(BssConstants.DeviceRegistration.DEVICE_REGISTRATION_STATUS_NOT_VALID, "Test 1"), validator.Errors.Where(t => t.Item1 == BssFailureCode.DeviceRegistrationStateInvalid).First().Item2);
    }

    [TestMethod]
    public void InvalidStatus2_Exception()
    {
      var message = new DeviceRegistration { Status = "Test" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(BssFailureCode.DeviceRegistrationStateInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.DeviceRegistrationStateInvalid).First().Item1);
      Assert.AreEqual(string.Format(BssConstants.DeviceRegistration.DEVICE_REGISTRATION_STATUS_NOT_VALID, "Test"), validator.Errors.Where(t => t.Item1 == BssFailureCode.DeviceRegistrationStateInvalid).First().Item2);
    }

    [TestMethod]
    public void ValidStatus_Success()
    {
      var message = new DeviceRegistration { Status = "DEREG_TECH" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.DeviceRegistrationStateInvalid).Count());
    }

    [TestMethod]
    public void ValidStatus2_Success()
    {
      var message = new DeviceRegistration { Status = "DEREG_STORE" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.DeviceRegistrationStateInvalid).Count());
    }

    [TestMethod]
    public void ValidStatus3_Success()
    {
      var message = new DeviceRegistration { Status = "REG" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.DeviceRegistrationStateInvalid).Count());
    }

    #endregion

    #region IBKey

    [TestMethod]
    public void NumericIBKey_Success()
    {
      var message = new DeviceRegistration { IBKey = IdGen.StringId() };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyInvalid).Count());
    }

    [TestMethod]
    public void EmptyIBKey_Error()
    {
      var message = new DeviceRegistration { IBKey = string.Empty };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyInvalid).First().Item1);
    }

    [TestMethod]
    public void ZeroIBKey_Error()
    {
      var message = new DeviceRegistration { IBKey = "0" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyInvalid).First().Item1);
    }

    [TestMethod]
    public void AlphaNumericIBKey_Error()
    {
      var message = new DeviceRegistration { IBKey = "xyz123" };
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyInvalid).First().Item1);
    }

    #endregion

  }
}