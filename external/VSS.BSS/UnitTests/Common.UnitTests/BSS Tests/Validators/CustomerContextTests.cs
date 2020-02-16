using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class CustomerContextTests : BssUnitTestBase
  {
    /*
		 * CustomerType is Dealer
     * Future Parent is Dealer
		 */
    [TestMethod]
    public void ParentChildRelationshipIsValid_DealerAndNewParentIsDealer_Valid()
    {
      var context = new CustomerContext();
      context.New.Type = CustomerTypeEnum.Dealer;
      context.NewParent.Type = CustomerTypeEnum.Dealer;
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";

      Assert.IsTrue(context.ParentChildRelationshipIsValid());
    }

    /*
		 * CustomerType is Dealer
     * Future Parent is Customer
		 */
    [TestMethod]
    public void ParentChildRelationshipIsValid__DealerAndNewParentIsCustomer_Error()
    {
      var context = new CustomerContext();
      context.New.Type = CustomerTypeEnum.Dealer;
      context.NewParent.Type = CustomerTypeEnum.Customer;
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";

      Assert.IsFalse(context.ParentChildRelationshipIsValid());
    }

    /*
		 * CustomerType is Dealer
     * Future Parent is Account
		 */
    [TestMethod]
    public void ParentChildRelationshipIsValid__DealerAndNewParentIsAccount_Error()
    {
      var context = new CustomerContext();
      context.New.Type = CustomerTypeEnum.Dealer;
      context.NewParent.Type = CustomerTypeEnum.Account;
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";

      Assert.IsFalse(context.ParentChildRelationshipIsValid());
    }

    /*
		 * CustomerType is Customer
     * Future Parent is Customer
		 */
    [TestMethod]
    public void ParentChildRelationshipIsValid__CustomerAndNewParentIsCustomer_Valid()
    {
      var context = new CustomerContext();
      context.New.Type = CustomerTypeEnum.Customer;
      context.NewParent.Type = CustomerTypeEnum.Customer;
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";

      Assert.IsTrue(context.ParentChildRelationshipIsValid());
    }

    /*
		 * CustomerType is Customer
     * Future Parent is Dealer
		 */
    [TestMethod]
    public void ParentChildRelationshipIsValid__CustomerAndNewParentIsDealer_Error()
    {
      var context = new CustomerContext();
      context.New.Type = CustomerTypeEnum.Customer;
      context.NewParent.Type = CustomerTypeEnum.Dealer;
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";

      Assert.IsFalse(context.ParentChildRelationshipIsValid());
    }

    /*
		 * CustomerType is Customer
     * Future Parent is Account
		 */
    [TestMethod]
    public void ParentChildRelationshipIsValid_CustomerAndNewParentIsAccount_Error()
    {
      var context = new CustomerContext();
      context.New.Type = CustomerTypeEnum.Customer;
      context.NewParent.Type = CustomerTypeEnum.Account;
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";

      Assert.IsFalse(context.ParentChildRelationshipIsValid());
    }

    /*
		 * CustomerType is Account
     * Future Parent is Dealer
		 */
    [TestMethod]
    public void ParentChildRelationshipIsValid__AccountAndNewParentIsDealer_Valid()
    {
      var context = new CustomerContext();
      context.New.Type = CustomerTypeEnum.Account;
      context.NewParent.Type = CustomerTypeEnum.Dealer;
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";

      Assert.IsTrue(context.ParentChildRelationshipIsValid());
    }

    /*
		 * CustomerType is Account
     * Future Parent is Customer
		 */
    [TestMethod]
    public void ParentChildRelationshipIsValid__AccountAndNewParentIsCustomer_Valid()
    {
      var context = new CustomerContext();
      context.New.Type = CustomerTypeEnum.Account;
      context.NewParent.Type = CustomerTypeEnum.Customer;
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";

      Assert.IsTrue(context.ParentChildRelationshipIsValid());
    }

    /*
		 * CustomerType is Account
     * Future Parent is Account
		 */
    [TestMethod]
    public void ParentChildRelationshipIsValid__AccountAndNewParentIsAccount_Error()
    {
      var context = new CustomerContext();
      context.New.Type = CustomerTypeEnum.Account;
      context.NewParent.Type = CustomerTypeEnum.Account;
      context.NewParent.BssId = "PARENT_CUSTOMER_BSS_ID";

      Assert.IsFalse(context.ParentChildRelationshipIsValid());
    }
  }
}
