using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class CustomerContextValidatorTests : BssUnitTestBase
  {
    protected CustomerContextValidator Validator;

    [TestInitialize]
    public void CustomerStateValidatorTests_Init()
    {
      Validator = new CustomerContextValidator();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Validate_CustomerContextIsNull_Throws()
    {
      Validator.Validate(null);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Validate_CustomerContextNewIsNull_Throws()
    {
      Validator.Validate(new CustomerContext { New = null });
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Validate_CustomerContextNewParentIsNull_Throws()
    {
      Validator.Validate(new CustomerContext{NewParent = null});
    }

    [TestMethod]
    public void Validate_CustomerContextIsValid_NoErrorsAndNoWarnings()
    {
      var customer = new CustomerContext{BssId = "BSSID"};

      Validator.Validate(customer);
      
      Assert.AreEqual(0, Validator.Warnings.Count);
      Assert.AreEqual(0, Validator.Errors.Count);
    }

    /*
		 * Future parent does not exist
		 */
    [TestMethod]
    public void Validate_NewParentDoesNotExist_Error()
    {
      var context = new CustomerContext();
      context.BssId = "BSSID";
      context.NewParent.Id = 0;
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";
      context.NewParent.RelationshipId = IdGen.GetId().ToString();

      Validator.Validate(context);

      Assert.AreEqual(string.Format(BssConstants.Hierarchy.PARENT_BSSID_DOES_NOT_EXIST, 
        context.NewParent.Type, context.NewParent.BssId), Validator.Errors[0].Item2);
      Assert.AreEqual(BssFailureCode.ParentDoesNotExist, Validator.FirstFailureCode());
    }

    /*
		 * Invalid Relationship
     * We are only testing that we are excercising the relationship validation logic
     * There are already tests around the logic elsewhere
		 */
    [TestMethod]
    public void Validate_NewParentRelationshipIsInvalid_Error()
    {
      var context = new CustomerContext();
      context.New.Type = CustomerTypeEnum.Account;
      context.NewParent.Id = IdGen.GetId();
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";
      context.NewParent.Type = CustomerTypeEnum.Account;

      Validator.Validate(context);

      Assert.AreEqual(string.Format(BssConstants.Hierarchy.RELATIONSHIP_INVALID,
        context.New.Type, context.NewParent.Type), Validator.Errors[0].Item2);
      Assert.AreEqual(BssFailureCode.RelationshipInvalid, Validator.FirstFailureCode());
    }

    /*
		 * Customer to Customer relationship
     * Warning
		 */
    [TestMethod]
    public void Validate_NewParentRelationshipIsCustomerToCustomer_Warning()
    {
      var context = new CustomerContext();
      context.New.Type = CustomerTypeEnum.Customer;
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";
      context.NewParent.Type = CustomerTypeEnum.Customer;

      Validator.Validate(context);

      Assert.AreEqual(BssConstants.Hierarchy.CUSTOMER_WITH_PARENT_CUSTOMER, Validator.Warnings[0]);
    }
  }
}
