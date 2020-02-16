using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AccountHierarchyDeletedValidatorTests : BssUnitTestBase
  {
    protected AccountHierarchyDeletedValidator validator;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new AccountHierarchyDeletedValidator();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Validate_NullMessage_ThrowInvalidOperationException()
    {
      validator.Validate(null);
    }

    [TestMethod]
    public void Validate_ValidMessage_NoWarningsAndNoErrors()
    {
      var message = BSS.AHDeleted.ForDealer()
        .ParentBssId(IdGen.GetId().ToString())
        .RelationshipId(IdGen.GetId().ToString())
        .Build();

      validator.Validate(message);

      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    /*
		 * ParentBssId must be defined
		 */
    [TestMethod]
    public void Validate_ParentBssIdNotDefined_Error()
    {
      var message = BSS.AHDeleted.ForDealer()
        .RelationshipId(IdGen.GetId().ToString())
        .Build();

      validator.Validate(message);

      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.ParentBssIdNotDefined, validator.Errors[0].Item1);
      Assert.AreEqual(BssConstants.Hierarchy.PARENT_BSSID_NOT_DEFINED, validator.Errors[0].Item2);
    }

    /*
		 * RelationshipId must be defined
		 */
    [TestMethod]
    public void Validate_RelationshipIdNotDefined_Error()
    {
      var message = BSS.AHDeleted.ForDealer()
        .ParentBssId(IdGen.GetId().ToString())
        .Build();

      validator.Validate(message);

      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.RelationshipIdNotDefined, validator.Errors[0].Item1);
      Assert.AreEqual(BssConstants.Hierarchy.RELATIONSHIPID_NOT_DEFINED, validator.Errors[0].Item2);
    }
  }
}
