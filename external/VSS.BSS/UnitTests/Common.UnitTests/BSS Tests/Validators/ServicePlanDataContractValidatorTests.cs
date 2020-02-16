using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using UnitTests.BSS_Tests;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests.Validators
{
  [TestClass]
  public class ServicePlanDataContractValidatorTests : BssUnitTestBase
  {
    ServicePlanDataContractValidator _validator;

    [TestInitialize]
    public void TestInitialize()
    {
      _validator = new ServicePlanDataContractValidator();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void NullMessage_Exception()
    {
      _validator.Validate(null);
    }

    #region Action

    [TestMethod]
    public void InvalidAction_Exception()
    {
      var message = BSS.SPUpdated.Build();
      message.Action = "Created";
      _validator.Validate(message);
      Assert.AreEqual(0, _validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(BssFailureCode.ActionInvalid, _validator.Errors.First(t => t.Item1 == BssFailureCode.ActionInvalid).Item1);
      Assert.AreEqual(string.Format(BssConstants.ACTION_INVALID_FOR_MESSAGE, "Created", "ServicePlan"), _validator.Errors.First(t => t.Item1 == BssFailureCode.ActionInvalid).Item2);
    }

    [TestMethod]
    public void ValidAction_Success()
    {
      _validator.Validate(BSS.SPUpdated.Build());
      Assert.AreEqual(0, _validator.Warnings.Count, "No Warnings Expected.");
      Assert.AreEqual(0, _validator.Errors.Count(t => t.Item1 == BssFailureCode.ActionInvalid));
    }

    #endregion

    #region IBKey

    [TestMethod]
    public void NumericIBKey_Success()
    {
      _validator.Validate(BSS.SPUpdated.Build());
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(0, _validator.Errors.Count(t => t.Item1 == BssFailureCode.IbKeyInvalid));
    }

    [TestMethod]
    public void EmptyOldIBKey_Error()
    {
      _validator.Validate(BSS.SPUpdated.IBKey(string.Empty).Build());
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, _validator.Errors.First(t => t.Item1 == BssFailureCode.IbKeyInvalid).Item1);
    }

    [TestMethod]
    public void AlphaNumericOldIBKey_Error()
    {
      _validator.Validate(BSS.SPUpdated.IBKey("test123").Build());
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.IbKeyInvalid, _validator.Errors.First(t => t.Item1 == BssFailureCode.IbKeyInvalid).Item1);
    }

    #endregion

    #region Service Termination Date

    [TestMethod]
    public void EmptyServiceTerminationDate_Error()
    {
      var message = BSS.SPUpdated.Build();
      message.ServiceTerminationDate = string.Empty;
      _validator.Validate(message);
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(0, _validator.Errors.Count);
    }

    [TestMethod]
    public void InvalidDateServiceTerminationDate_Error()
    {
      var message = BSS.SPCancelled.Build();
      message.ServiceTerminationDate = "111/111/111";
      _validator.Validate(message);
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(1, _validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.ServiceCancelDateInvalid, _validator.Errors[0].Item1);
    }

    [TestMethod]
    public void ValidServiceTerminationDate_UTC_Success()
    {
      var message = BSS.SPActivated.Build();
      _validator.Validate(message);
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(0, _validator.Errors.Count);
    }

    [TestMethod]
    public void ValidServiceTerminationDate_ISO_Success()
    {
      var message = BSS.SPUpdated.Build();
      message.ServiceTerminationDate = DateTime.UtcNow.ToIso8601DateTimeString();
      _validator.Validate(message);
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(0, _validator.Errors.Count);
    }

    #endregion

    #region Owner Visibility Date

    [TestMethod]
    public void EmptyOwnerVisibilityDate_Error()
    {
      var message = BSS.SPUpdated.Build();
      message.OwnerVisibilityDate = string.Empty;
      _validator.Validate(message);
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(0, _validator.Errors.Count);
    }

    [TestMethod]
    public void InvalidDateOwnerVisibilityDate_Error()
    {
      var message = BSS.SPCancelled.Build();
      message.OwnerVisibilityDate = "111/111/111";
      _validator.Validate(message);
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(1, _validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.OwnerVisibilityDateInvalid, _validator.Errors[0].Item1);
    }

    [TestMethod]
    public void ValidOwnerVisibilityDate_UTC_Success()
    {
      var message = BSS.SPActivated.Build();
      _validator.Validate(message);
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(0, _validator.Errors.Count);
    }

    [TestMethod]
    public void ValidOwnerVisibilityDate_ISO_Success()
    {
      var message = BSS.SPUpdated.Build();
      message.OwnerVisibilityDate = DateTime.UtcNow.ToIso8601DateTimeString();
      _validator.Validate(message);
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(0, _validator.Errors.Count);
    }

    [TestMethod]
    public void InvalidOwnerVisibilityDate_PriorToYear2009()
    {
      var message = BSS.SPActivated.Build();
      message.OwnerVisibilityDate = new DateTime(2008, 12, 31).ToIso8601DateTimeString();
      _validator.Validate(message);
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(1, _validator.Errors.Count);
    }

    [TestMethod]
    public void InvalidOwnerVisibilityDate_EqualTo20290101()
    {
      var message = BSS.SPActivated.Build();
      message.OwnerVisibilityDate = new DateTime(2029, 01, 01).ToIso8601DateTimeString();
      _validator.Validate(message);
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(0, _validator.Errors.Count);
    }

    [TestMethod]
    public void InvalidOwnerVisibilityDate_SubsequentTo20290101()
    {
      var message = BSS.SPActivated.Build();
      message.OwnerVisibilityDate = new DateTime(2029, 01, 02).ToIso8601DateTimeString();
      _validator.Validate(message);
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(1, _validator.Errors.Count);
    }

    #endregion

    #region Service Plan Name

    [TestMethod]
    public void Validate_ServicePlanNameNotDefined_Error()
    {
      _validator.Validate(BSS.SPUpdated.ServicePlanName(string.Empty).Build());
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(1, _validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.ServicePlanNameNotDefined, _validator.Errors[0].Item1);
      Assert.AreEqual(BssConstants.ServicePlan.SERVICE_PLAN_NAME_NOT_DEFINED, _validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_ServicePlanNameDefined_Success()
    {
      _validator.Validate(BSS.SPUpdated.ServicePlanName("89500-00").Build());
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(0, _validator.Errors.Count);
    }

    #endregion

    #region Service Plan Line ID

    [TestMethod]
    public void EmptyServicePlanLineID_Failure()
    {
      _validator.Validate(BSS.SPUpdated.ServicePlanlineID(string.Empty).Build());
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(1, _validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.ServicePlanLineIdNotDefined, _validator.Errors[0].Item1);
      Assert.AreEqual(BssConstants.ServicePlan.SERVICE_PLAN_LINE_ID_NOT_DEFINED, _validator.Errors[0].Item2);
    }

    [TestMethod]
    public void NumericServicePlanLineID_Success()
    {
      _validator.Validate(BSS.SPUpdated.Build());
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(0, _validator.Errors.Count);
    }

    [TestMethod]
    public void AlphaNumericServicePlanLineID_Error()
    {
      _validator.Validate(BSS.SPUpdated.ServicePlanlineID("123test").Build());
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.ServicePlanLineIdInvalid, _validator.Errors[0].Item1);
      Assert.AreEqual(BssConstants.ServicePlan.SERVICE_PLAN_LINE_ID_INVALID, _validator.Errors[0].Item2);
    }

    [TestMethod]
    public void NegativeNumericServicePlanLineID_Success()
    {
      _validator.Validate(BSS.SPUpdated.ServicePlanlineID("-1234").Build());
      Assert.AreEqual(0, _validator.Warnings.Count);
      Assert.AreEqual(0, _validator.Errors.Count);
    }

    #endregion

  }
}