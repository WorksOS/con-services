using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class CustomerContextDeletedValidatorTests : BssUnitTestBase
  {
    protected CustomerContextDeletedValidator Validator;

    [TestInitialize]
    public void CustomerContextDeleteValidatorTests_Init()
    {
      Validator = new CustomerContextDeletedValidator();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Validate_CustomerContextIsNull_Throws()
    {
      Validator.Validate(null);
    }

    [TestMethod]
    public void Validate_ValidContext_NoErrorsAndNoWarnings()
    {
      var relationshipId = IdGen.GetId().ToString();
      var context = new CustomerContext();
      context.NewParent.Id = IdGen.GetId();
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";
      context.NewParent.Type = CustomerTypeEnum.Dealer;
      context.NewParent.RelationshipId = relationshipId;
      context.ParentDealer.RelationshipId = relationshipId;

      Validator.Validate(context);
      
      Assert.AreEqual(0, Validator.Warnings.Count);
      Assert.AreEqual(0, Validator.Errors.Count);
    }

    /*
		 * Customer for ParentBssId does not exists
		 */
    [TestMethod]
    public void Validate_ParentDoesNotExist_Error()
    {
      var relationshipId = IdGen.GetId().ToString();
      var context = new CustomerContext();
      context.NewParent.Id = 0;
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";
      context.NewParent.Type = CustomerTypeEnum.Dealer;
      context.NewParent.RelationshipId = relationshipId;
      context.ParentDealer.RelationshipId = relationshipId;

      Validator.Validate(context);

      Assert.AreEqual(0, Validator.Warnings.Count);
      Assert.AreEqual(1, Validator.Errors.Count);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.PARENT_BSSID_DOES_NOT_EXIST, 
        context.New.Type, context.New.BssId), Validator.Errors[0].Item2);
      Assert.AreEqual(BssFailureCode.ParentDoesNotExist, Validator.FirstFailureCode());
    }

    /*
		 * CustomerRelationshipType does not exist between Parent/Child
		 */
    [TestMethod]
    public void Validate_CustomerRelationshipIdDoesNotExist_Error()
    {
      var context = new CustomerContext();
      context.NewParent.Id = IdGen.GetId();
      context.NewParent.Type = CustomerTypeEnum.Dealer;
      context.NewParent.RelationshipId = IdGen.GetId().ToString();

      Validator.Validate(context);

      Assert.AreEqual(0, Validator.Warnings.Count);
      Assert.AreEqual(1, Validator.Errors.Count);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.RELATIONSHIPID_DOES_NOT_EXIST,
        context.NewParent.RelationshipId), Validator.Errors[0].Item2);
      Assert.AreEqual(BssFailureCode.RelationshipIdDoesNotExist, Validator.FirstFailureCode());
    }
  }
}
