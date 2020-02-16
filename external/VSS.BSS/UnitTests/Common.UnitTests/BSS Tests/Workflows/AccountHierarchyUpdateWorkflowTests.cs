using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Nighthawk.NHBssSvc;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AccountHierarchyUpdateWorkflowTests : BssUnitTestBase
  {
    #region Dealer

    List<string> dealerNetwork = new List<string> { "CAT", "TRIMBLE", "SITECH", "NONE" };

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_Dealer_BssIdDoesNotExist_Failure()
    {

      foreach (var item in dealerNetwork)
      {
        var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
        var parentDealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
        var rel = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();
        AccountHierarchy message = BSS.AHUpdated.ForDealer()
          .BssId(IdGen.GetId().ToString())
          .RelationshipId(rel.BSSRelationshipID)
          .ParentBssId(parentDealer.BSSID)
          .DealerNetwork(item.ToString())
          .Build();

        WorkflowResult result = ExecuteWorkflow(message);
        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.BSSID_DOES_NOT_EXIST, CustomerTypeEnum.Dealer, message.BSSID));
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_Dealer_BssIdExists_ParentBssIdNotDefined_Success()
    {

      foreach (var item in dealerNetwork)
      {
        var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
        AccountHierarchy message = BSS.AHUpdated.ForDealer()
          .BssId(dealer.BSSID)
          .DealerNetwork(item.ToString())
          .Build();

        WorkflowResult result = ExecuteWorkflow(message);
        Assert.IsTrue(result.Success);
        var customer = (from c in Ctx.OpContext.CustomerReadOnly
                        join ct in Ctx.OpContext.CustomerTypeReadOnly on c.fk_CustomerTypeID equals ct.ID
                       where c.BSSID == dealer.BSSID
                       select new { customer = c, customertype = ct.Name }).FirstOrDefault();
        Assert.IsNotNull(customer);
        Assert.AreEqual(message.CustomerType.ToUpper(), customer.customertype.ToUpper(),"Failed to map BSS hierarchy type to CustomerType");
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_Dealer_BssIdExists_ParentBssIdDefined_DoesNotExist_Failure()
    {

      foreach (var item in dealerNetwork)
      {
        var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
        var parentDealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
        var rel = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

        AccountHierarchy message = BSS.AHUpdated.ForDealer()
          .BssId(dealer.BSSID)
          .RelationshipId(IdGen.GetId().ToString())
          .ParentBssId(IdGen.GetId().ToString())
          .DealerNetwork(item.ToString())
          .Build();

        WorkflowResult result = ExecuteWorkflow(message);
        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.PARENT_BSSID_DOES_NOT_EXIST, CustomerTypeEnum.Dealer, message.ParentBSSID));
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_ExistingBssID_ExistingParentBSSID_ExistingRelationshipID_Dealer_Success()
    {
      foreach (var item in dealerNetwork)
      {
        var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
        var parentDealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
        var rel = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

        AccountHierarchy message = BSS.AHUpdated.ForDealer()
          .BssId(dealer.BSSID)
          .RelationshipId(rel.BSSRelationshipID)
          .ParentBssId(parentDealer.BSSID)
          .DealerNetwork(item.ToString())
          .DealerNetwork("CAT")
          .Build();

        WorkflowResult result = ExecuteWorkflow(message);
        Assert.IsTrue(result.Success);
        //StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.RELATIONSHIPID_EXISTS, message.RelationshipID));
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_ExistingBssID_NewParentBSSID_ExistingRelationshipID_Dealer_Failure()
    {

      foreach (var item in dealerNetwork)
      {
        var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
        var parentDealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
        var rel = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

        AccountHierarchy message = BSS.AHUpdated.ForDealer()
          .BssId(dealer.BSSID)
          .RelationshipId(rel.BSSRelationshipID)
          .ParentBssId(IdGen.GetId().ToString())
          .DealerNetwork(item.ToString())
          .Build();

        WorkflowResult result = ExecuteWorkflow(message);
        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.PARENT_BSSID_DOES_NOT_EXIST, CustomerTypeEnum.Dealer, message.ParentBSSID));
      }
    }

    #endregion Dealer

    #region Customer

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_NewBssID_Customer_Failure()
    {
      AccountHierarchy message = BSS.AHUpdated.ForCustomer()
        .BssId(IdGen.GetId().ToString())
        .Build();

      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.BSSID_DOES_NOT_EXIST, CustomerTypeEnum.Customer, message.BSSID));
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_ExistingBssID_Customer_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();

      AccountHierarchy message = BSS.AHUpdated.ForAccount().ParentBssId(customer.BSSID)
        .BssId(account.BSSID).RelationshipId(IdGen.GetId().ToString())
        .Build();

      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);
      var query = (from c in Ctx.OpContext.CustomerReadOnly
                   join ct in Ctx.OpContext.CustomerTypeReadOnly on c.fk_CustomerTypeID equals ct.ID
                      where c.BSSID == account.BSSID
                      select new { customer = c, customertype = ct.Name }).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    #endregion Customer

    #region AccountCustomer

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_NewBssID_ExistingParentBSSID_ExistingRelationshipID_CustomerAccount_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      AccountHierarchy message = BSS.AHUpdated.ForAccount()
        .BssId(IdGen.GetId().ToString())
        .RelationshipId(rel.BSSRelationshipID)
        .ParentBssId(customer.BSSID)
        .Build();

      message.DealerNetwork = null;
      message.NetworkDealerCode = null;
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);

      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.BSSID_DOES_NOT_EXIST, CustomerTypeEnum.Account, message.BSSID));
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_ExistingBssID_ExistingParentBSSID_ExistingRelationshipID_CustomerAccount_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      AccountHierarchy message = BSS.AHUpdated.ForAccount()
        .BssId(account.BSSID)
        .RelationshipId(rel.BSSRelationshipID)
        .ParentBssId(customer.BSSID)
        .DealerNetwork("CAT")
        .Build();

      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);
    }

    [TestMethod]
    [DatabaseTest]
    public void RelateAccountToCustomerCreatedByCatStoreWithRelationshipIdUpdate_Success()
    {
      var id = IdGen.GetId().ToString(CultureInfo.InvariantCulture);
      var bssRelationshipId = "StoreAPI_" + id;
      var account = Entity.Customer.Account
        .BssId(IdGen.GetId().ToString(CultureInfo.InvariantCulture))
        .Save();
      var customer = Entity.Customer.EndCustomer
        .BssId(IdGen.GetId().ToString(CultureInfo.InvariantCulture))
        .Save();
      Entity.CustomerRelationship
        .Relate(customer, account)
        .BssRelationshipId(bssRelationshipId)
        .Save();

      AccountHierarchy message = BSS.AHUpdated.ForAccount()
        .BssId(account.BSSID)
        .RelationshipId(id)
        .HierarchyType("TCS Customer")
        .ParentBssId(customer.BSSID)
        .DealerNetwork("CAT")
        .Build();

      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      var relationships =
        (from r in Ctx.OpContext.CustomerRelationshipReadOnly
         where r.fk_ParentCustomerID == customer.ID
            && r.fk_ClientCustomerID == account.ID
            && r.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSCustomer
         select r).ToList();
      Assert.AreEqual(1, relationships.Count);
      Assert.AreEqual(id, relationships.First().BSSRelationshipID);
    }

    #endregion

    #region Account

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_NewBssID_ExistingParentBSSID_ExistingRelationshipID_Account_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      AccountHierarchy message = BSS.AHUpdated.ForAccount()
        .BssId(IdGen.GetId().ToString())
        .RelationshipId(rel.BSSRelationshipID)
        .ParentBssId(customer.BSSID)
        .Build();

      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);

      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.BSSID_DOES_NOT_EXIST, CustomerTypeEnum.Account, message.BSSID));
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_NewBssID_NewParentBSSID_NewRelationshipID_Account_Failure()
    {
      AccountHierarchy message = BSS.AHUpdated.ForAccount()
        .BssId(IdGen.GetId().ToString())
        .RelationshipId(IdGen.GetId().ToString())
        .ParentBssId(IdGen.GetId().ToString())
        .Build();

      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.BSSID_DOES_NOT_EXIST, CustomerTypeEnum.Account, message.BSSID));
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_NewBssID_NewParentBSSID_ExistingRelationshipID_Account_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      AccountHierarchy message = BSS.AHUpdated.ForAccount()
        .BssId(IdGen.GetId().ToString())
        .RelationshipId(rel.BSSRelationshipID)
        .ParentBssId(IdGen.GetId().ToString())
        .Build();

      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.BSSID_DOES_NOT_EXIST, CustomerTypeEnum.Account, message.BSSID));
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_NewBssID_ExistingParentBSSID_NewRelationshipID_Account_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      AccountHierarchy message = BSS.AHUpdated.ForAccount()
        .BssId(IdGen.GetId().ToString())
        .RelationshipId(IdGen.GetId().ToString())
        .ParentBssId(customer.BSSID)
        .Build();

      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.BSSID_DOES_NOT_EXIST, CustomerTypeEnum.Account, message.BSSID));
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_NewBssID_NoParentBSSID_NoRelationshipID_Account_Failure()
    {
      AccountHierarchy message = BSS.AHUpdated.ForAccount().BssId(IdGen.GetId().ToString()).Build();

      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.BSSID_DOES_NOT_EXIST, CustomerTypeEnum.Account, message.BSSID));
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_ExistingBssID_NoParentBSSID_NoRelationshipID_Account_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();

      AccountHierarchy message = BSS.AHUpdated.ForAccount()
        .BssId(account.BSSID)
        .ParentBssId(null)
        .RelationshipId(null)
        .Build();

      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);
      var query = (from c in Ctx.OpContext.CustomerReadOnly
                   join ct in Ctx.OpContext.CustomerTypeReadOnly on c.fk_CustomerTypeID equals ct.ID
                      where c.BSSID == account.BSSID
                      select new { customer = c, customertype = ct.Name }).FirstOrDefault();
      Assert.IsNotNull(query);
      Assert.AreEqual(message.CustomerType.ToString().ToLower(), query.customertype.ToLower(), "Failed to map hierarchy type to customer type");
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_ExistingBssID_NewParentBSSID_NewRelationshipID_Account_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      AccountHierarchy message = BSS.AHUpdated.ForAccount()
        .BssId(account.BSSID)
        .RelationshipId(IdGen.GetId().ToString())
        .ParentBssId(IdGen.GetId().ToString())
        .Build();

      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.PARENT_BSSID_DOES_NOT_EXIST, CustomerTypeEnum.Dealer, message.ParentBSSID));
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_ExistingBssID_ExistingParentBSSID_ExistingRelationshipID_Account_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      AccountHierarchy message = BSS.AHUpdated.ForAccount()
        .BssId(account.BSSID)
        .RelationshipId(rel .BSSRelationshipID)
        .ParentBssId(customer.BSSID)
        .DealerNetwork("CAT")
        .Build();

      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);
    }

    [TestMethod]
    [DatabaseTest]
    public void CustomerUpdate_ExistingBssID_NewParentBSSID_ExistingRelationshipID_Account_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      AccountHierarchy message = BSS.AHUpdated.ForAccount()
        .BssId(account.BSSID)
        .RelationshipId(rel.BSSRelationshipID)
        .ParentBssId(IdGen.GetId().ToString())
        .Build();

      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.PARENT_BSSID_DOES_NOT_EXIST, CustomerTypeEnum.Dealer, message.ParentBSSID));
    }

    #endregion Account

    #region Admin User

    [TestMethod]
    [DatabaseTest]
    public void AdminUserUpdate_ExistingCustomerNonVerifiedUser_NewEmailUpdate_Success()
    {      
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var user = Entity.User.ForCustomer(customer).CreatedBy("System").EmailValidated(false).WithLanguage(new Language() { ID = 1 }).Save();
      AccountHierarchy message = BSS.AHUpdated.ForCustomer()
        .BssId(customer.BSSID)
        .ParentBssId(null)
        .RelationshipId(null)
        .ContactDefined()
        .Build();
      var firstAdminUser = Services.Customers().GetFirstAdminUser(customer.ID);

      Assert.IsFalse(string.Equals(firstAdminUser.EmailContact, message.contact.Email, System.StringComparison.InvariantCultureIgnoreCase));
      WorkflowResult result = ExecuteWorkflow(message);

      firstAdminUser = Services.Customers().GetFirstAdminUser(customer.ID);
      Assert.IsTrue(result.Success);
      Assert.IsTrue(string.Equals(firstAdminUser.EmailContact, message.contact.Email, System.StringComparison.InvariantCultureIgnoreCase));
      Assert.IsTrue(string.Equals(firstAdminUser.FirstName, message.contact.FirstName, System.StringComparison.InvariantCultureIgnoreCase));
      Assert.IsTrue(string.Equals(firstAdminUser.LastName, message.contact.LastName, System.StringComparison.InvariantCultureIgnoreCase));    
    }

    [TestMethod]
    [DatabaseTest]
    public void AdminUserUpdate_WithNoEmailUpdateRequest_Success()
    {      
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();

      AccountHierarchy message = BSS.AHUpdated.ForCustomer()
        .BssId(customer.BSSID)
        .ParentBssId(null)
        .RelationshipId(null)
        .ContactNotDefined()
        .Build();
            
      WorkflowResult result = ExecuteWorkflow(message);
            
      Assert.IsTrue(result.Success);      
    }
    #endregion

    private WorkflowResult ExecuteWorkflow(AccountHierarchy message)
    {
      var workflow = new BssWorkflowFactory(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object)).Create(message);
      WorkflowResult result = new WorkflowRunner().Run(workflow);
      new ConsoleResultProcessor().Process(message, result);
      return result;
    }
  }
}
