using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class MapCurrentStateToCustomerContextTests : BssUnitTestBase
  {
    protected Inputs Inputs;
    protected MapCurrentStateToCustomerContext Activity;

    [TestInitialize]
    public void MapCurrentStateToCustomerContextTests_Init()
    {
      Inputs = new Inputs();
      Activity = new MapCurrentStateToCustomerContext();
    }

    [TestMethod]
    public void Execute_ParentsAndAdminUserDoNotExist_ParentsAndAdminUserNotMapped_ParentsAndAdminUserExistsIsFalse()
    {
      var dealer = Entity.Customer.Dealer.BssId("11111").Save();
      var context = new CustomerContext
      {
        Id = dealer.ID
      };
      Inputs.Add<CustomerContext>(context);

      Activity.Execute(Inputs);

      Assert.IsFalse(context.ParentDealer.Exists);
      Assert.IsFalse(context.ParentCustomer.Exists);
      Assert.IsFalse(context.AdminUserExists);
    }

    [TestMethod]
    public void Execute_ParentDealerExists_ParentDealerMapped_ParentDealerExistsIsTrue()
    {
      var account = Entity.Customer.Account.BssId("11111").Save();
      var parentDealer = Entity.Customer.Dealer.BssId("2222").Save();
      var relationship = Entity.CustomerRelationship.Relate(parentDealer, account).Save();

      var context = new CustomerContext
      {
        Id = account.ID
      };
      Inputs.Add<CustomerContext>(context);

      Activity.Execute(Inputs);

      Assert.IsTrue(context.ParentDealer.Exists);
      Assert.AreEqual(parentDealer.BSSID, context.ParentDealer.BssId);
      Assert.AreEqual((CustomerTypeEnum)parentDealer.fk_CustomerTypeID, context.ParentDealer.Type);
      Assert.AreEqual(parentDealer.Name, context.ParentDealer.Name);
      Assert.AreEqual((DealerNetworkEnum)parentDealer.fk_DealerNetworkID, context.ParentDealer.DealerNetwork);
      Assert.AreEqual(parentDealer.NetworkDealerCode, context.ParentDealer.NetworkDealerCode);
      Assert.AreEqual(parentDealer.NetworkCustomerCode, context.ParentDealer.NetworkCustomerCode);
      Assert.AreEqual(parentDealer.DealerAccountCode, context.ParentDealer.DealerAccountCode);

      Assert.AreEqual(relationship.BSSRelationshipID, context.ParentDealer.RelationshipId);
      Assert.AreEqual(CustomerRelationshipTypeEnum.TCSDealer, context.ParentDealer.RelationshipType);
    }

    [TestMethod]
    public void Execute_ParentCustomerExists_ParentCustomerMapped_ParentCustomerExistsIsTrue()
    {
      var account = Entity.Customer.Account.BssId("11111").Save();
      var parentCustomer = Entity.Customer.EndCustomer.BssId("2222").Save();
      var relationship = Entity.CustomerRelationship.Relate(parentCustomer, account).Save();

      var context = new CustomerContext
      {
        Id = account.ID
      };
      Inputs.Add<CustomerContext>(context);

      Activity.Execute(Inputs);

      Assert.IsTrue(context.ParentCustomer.Exists);
      Assert.AreEqual(parentCustomer.BSSID, context.ParentCustomer.BssId);
      Assert.AreEqual((CustomerTypeEnum)parentCustomer.fk_CustomerTypeID, context.ParentCustomer.Type);
      Assert.AreEqual(parentCustomer.Name, context.ParentCustomer.Name);
      Assert.AreEqual((DealerNetworkEnum)parentCustomer.fk_DealerNetworkID, context.ParentCustomer.DealerNetwork);
      Assert.AreEqual(parentCustomer.NetworkDealerCode, context.ParentCustomer.NetworkDealerCode);
      Assert.AreEqual(parentCustomer.NetworkCustomerCode, context.ParentCustomer.NetworkCustomerCode);
      Assert.AreEqual(parentCustomer.DealerAccountCode, context.ParentCustomer.DealerAccountCode);

      Assert.AreEqual(relationship.BSSRelationshipID, context.ParentCustomer.RelationshipId);
      Assert.AreEqual(CustomerRelationshipTypeEnum.TCSCustomer, context.ParentCustomer.RelationshipType);
    }

    [TestMethod]
    public void Execute_AdminUserExists_AdminUserExistsIsTrue()
    {

      var dealer = Entity.Customer.Dealer.BssId("2222").Save();
      Entity.User.ForCustomer(dealer).Save();

      var context = new CustomerContext
      {
        Id = dealer.ID
      };
      Inputs.Add<CustomerContext>(context);

      Activity.Execute(Inputs);

      Assert.IsTrue(context.AdminUserExists);
    }
  }
}
