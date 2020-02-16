using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class CustomerContextCreatedValidatorTests : BssUnitTestBase
  {

    protected CustomerContextCreatedValidator Validator;

    [TestInitialize]
    public void CustomerContextCreateValidatorTests_Init()
    {
      Validator = new CustomerContextCreatedValidator();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Validate_CustomerContextIsNull_Throws()
    {
     Validator.Validate(null);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Validate_CustomerContextNewParentIsNull_Throws()
    {
      var context = new CustomerContext {NewParent = null};
      Validator.Validate(context);
    }

    [TestMethod]
    public void Validate_ValidContext_NoErrorsAndNoWarnings()
    {
      var context = new CustomerContext();

      Validator.Validate(context);

      Assert.AreEqual(0, Validator.Warnings.Count);
      Assert.AreEqual(0, Validator.Errors.Count);
    }

    /*
		* A customer cannot exist for the BssId
		*/
    [TestMethod]
    public void Validate_CustomerExists_ReturnsError()
    {
      var context = new CustomerContext { Id = IdGen.GetId(), New = {BssId = "EXISTING_BSSID"} };
      var validator = new CustomerContextCreatedValidator();

      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.BSSID_EXISTS, context.Type, context.New.BssId), validator.Errors[0].Item2);
      Assert.AreEqual(BssFailureCode.CustomerExists, validator.FirstFailureCode());
    }

    /*
		* When RelationshipId is defined
		* Then that RelationshipId can't exist anywhere in the system
		*/
    [TestMethod]
    public void Validate_RelationshipIdExists_ReturnsError()
    {
      Services.Customers = () => new BssCustomerServiceFake(new CustomerRelationship());

      var context = new CustomerContext { NewParent = {RelationshipId = "A_RELATIONSHIP_ID_THAT_EXITS" }};

      Validator.Validate(context);

      Assert.AreEqual(0, Validator.Warnings.Count);
      Assert.AreEqual(1, Validator.Errors.Count);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.RELATIONSHIPID_EXISTS, context.NewParent.RelationshipId), Validator.Errors[0].Item2);
      Assert.AreEqual(BssFailureCode.RelationshipIdExists, Validator.FirstFailureCode());
    }
  }
}
