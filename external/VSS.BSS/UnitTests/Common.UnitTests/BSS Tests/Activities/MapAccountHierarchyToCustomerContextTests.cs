using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class MapAccountHierarchyToCustomerContextTests : BssUnitTestBase
  {

    protected Inputs Inputs;
    protected MapAccountHierarchyToCustomerContext Activity;

    [TestInitialize]
    public void MapAccountHierarchyToNewCustomerContextTests_Init()
    {
      Inputs = new Inputs();
      Activity = new MapAccountHierarchyToCustomerContext();
    }

    #region New

    [TestMethod]
    public void Execute_MappedToNew()
    {
      var message = BSS.AHCreated.CustomerType(AccountHierarchy.BSSCustomerTypeEnum.DEALER).Build();
      Inputs.Add<AccountHierarchy>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<CustomerContext>();

      Assert.AreEqual(message.BSSID.ToString(), context.New.BssId);
      Assert.AreEqual(message.CustomerType.ToEnum<CustomerTypeEnum>(), context.New.Type);
      Assert.AreEqual(message.CustomerName, context.New.Name);
      Assert.AreEqual(message.DealerNetwork.ToDealerNetworkEnum(), context.New.DealerNetwork);
      Assert.AreEqual(message.NetworkDealerCode, context.New.NetworkDealerCode);
      Assert.AreEqual(message.NetworkCustomerCode, context.New.NetworkCustomerCode);
      Assert.AreEqual(message.DealerAccountCode, context.New.DealerAccountCode);
    }

    #endregion

    #region NewParent

    [TestMethod]
    public void Execute_ParentBssIdNotDefined_NewParentContextNotMapped()
    {
      var serviceFake = new BssCustomerServiceFake((Customer)null);
      Services.Customers = () => serviceFake;

      var message = BSS.AHCreated.CustomerType(AccountHierarchy.BSSCustomerTypeEnum.DEALER).Build();
      Inputs.Add<AccountHierarchy>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<CustomerContext>();

      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
      Assert.IsFalse(context.NewParent.Exists);
      Assert.AreEqual(message.ParentBSSID, context.NewParent.BssId);
      Assert.AreEqual(message.RelationshipID, context.NewParent.RelationshipId);
      Assert.AreEqual(CustomerRelationshipTypeEnum.Unknown, context.NewParent.RelationshipType);
    }

    [TestMethod]
    public void Execute_ParentBssIdDefined_NewParentContextMapped()
    {
      var dealer = Entity.Customer.Dealer.BssId("11111").DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentDealer = Entity.Customer.Dealer.BssId("22222").DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();
      var serviceFake = new BssCustomerServiceFake(parentDealer);
      Services.Customers = () => serviceFake;

      var message = BSS.AHCreated.ForDealer().BssId(11111.ToString())
        .ParentBssId(parentDealer.BSSID)
        .RelationshipId(relationship.BSSRelationshipID)
        .Build();
      Inputs.Add<AccountHierarchy>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<CustomerContext>();

      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
      Assert.IsTrue(context.NewParent.Exists);
      Assert.IsTrue(context.NewParent.IsActive);
      Assert.AreEqual(message.ParentBSSID.ToString(), context.NewParent.BssId);
      Assert.AreEqual(message.RelationshipID.ToString(), context.NewParent.RelationshipId);
      Assert.AreEqual(message.HierarchyType.ToCustomerRelationshipTypeEnum(), context.NewParent.RelationshipType);
    }

    #endregion

    #region NewAdminUser

    [TestMethod]
    public void Execute_AdminUserNotDefined_NewAdminUserNotMapped()
    {
      var message = BSS.AHCreated.ForDealer().Build();
      Inputs.Add<AccountHierarchy>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<CustomerContext>();

      Assert.AreEqual(null, context.AdminUser.FirstName);
      Assert.AreEqual(null, context.AdminUser.LastName);
      Assert.AreEqual(null, context.AdminUser.Email);
    }

    [TestMethod]
    public void Execute_AdminUserIsDefined_NewAdminUserMapped()
    {
      var message = BSS.AHCreated.ForDealer().ContactDefined().Build();
      Inputs.Add<AccountHierarchy>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<CustomerContext>();

      Assert.AreEqual(message.contact.FirstName, context.AdminUser.FirstName);
      Assert.AreEqual(message.contact.LastName, context.AdminUser.LastName);
      Assert.AreEqual(message.contact.Email, context.AdminUser.Email);
    }

    #endregion

    #region Current Customer

    [TestMethod]
    public void Execute_BssIdDoesNotExist_ExistsIsFalse()
    {
      var serviceFake = new BssCustomerServiceFake((Customer)null);
      Services.Customers = () => serviceFake;

      var message = BSS.AHCreated.CustomerType(AccountHierarchy.BSSCustomerTypeEnum.DEALER).Build();
      Inputs.Add<AccountHierarchy>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<CustomerContext>();

      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
      Assert.IsFalse(context.Exists, "Exists");
      Assert.AreEqual(null, context.BssId);
      Assert.AreEqual(null, context.Name);
      Assert.AreEqual(null, context.NetworkDealerCode);
      Assert.AreEqual(null, context.NetworkCustomerCode);
      Assert.AreEqual(null, context.DealerAccountCode);
    }

    [TestMethod]
    public void Execute_BssIdExists_ExistsIsTrue()
    {
      var dealer = Entity.Customer.Dealer.Id(IdGen.GetId()).BssId(12345.ToString()).DealerNetwork(DealerNetworkEnum.CAT).Save();

      var serviceFake = new BssCustomerServiceFake(dealer);
      Services.Customers = () => serviceFake;

      var message = BSS.AHCreated.ForDealer().BssId(12345.ToString()).Build();
      Inputs.Add<AccountHierarchy>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<CustomerContext>();

      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
      Assert.IsTrue(context.Exists, "Exists");
      Assert.IsTrue(context.IsActive, "IsActive");
      Assert.AreEqual(dealer.BSSID, context.BssId);
      Assert.AreEqual((CustomerTypeEnum)dealer.fk_CustomerTypeID, context.Type);
      Assert.AreEqual(dealer.Name, context.Name);
      Assert.AreEqual((DealerNetworkEnum)dealer.fk_DealerNetworkID, context.DealerNetwork);
      Assert.AreEqual(dealer.NetworkDealerCode, context.NetworkDealerCode);
      Assert.AreEqual(dealer.NetworkCustomerCode, context.NetworkCustomerCode);
      Assert.AreEqual(dealer.DealerAccountCode, context.DealerAccountCode);
    }

    #endregion
  }
}
