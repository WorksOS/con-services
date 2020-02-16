using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.Interfaces;
using Moq;
using System;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class BssCustomerServiceTests : BssUnitTestBase
  {
    [DatabaseTest]
    [TestMethod]
    public void GetCustomerByBssId_CustomerExistsWithBssId_ReturnCustomer()
    {
      var dealer = TestData.TestDealer;
      var customer = TestData.TestCustomer;
      var account = TestData.TestAccount;

      var result = Services.Customers().GetCustomerByBssId(customer.BSSID);

      Assert.IsNotNull(result);
      Assert.AreEqual(customer.ID, result.ID);
    }

    [DatabaseTest]
    [TestMethod]
    public void GetCustomerByBssId_CustomerDoesNotExistWithBssId_ReturnNull()
    {
      var dealer = TestData.TestDealer;
      var customer = TestData.TestCustomer;
      var account = TestData.TestAccount;

      var result = Services.Customers().GetCustomerByBssId("NON_EXISTING_BSSID");

      Assert.IsNull(result);
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateCustomer_NewDealer_DealerCreated()
    {
      var context = new CustomerContext();
      context.New.BssId = IdGen.GetId().ToString();
      context.New.Type = CustomerTypeEnum.Dealer;
      context.New.Name = "NEW_DEALER";
      context.New.DealerNetwork = DealerNetworkEnum.None;
      context.New.NetworkDealerCode = "NETWORK_DEALER_CODE";

      Services.Customers().CreateCustomer(context);

      var newDealer = Ctx.OpContext.CustomerReadOnly.FirstOrDefault(x => x.BSSID == context.New.BssId);

      Assert.IsNotNull(newDealer);
      Assert.IsTrue(newDealer.ID > 0);
      Assert.AreEqual(context.New.Type, (CustomerTypeEnum)newDealer.fk_CustomerTypeID);
      Assert.AreEqual(context.New.Name, newDealer.Name);
      Assert.AreEqual(context.New.DealerNetwork, (DealerNetworkEnum)newDealer.fk_DealerNetworkID);
      Assert.AreEqual(context.New.NetworkDealerCode, newDealer.NetworkDealerCode);
      Assert.AreEqual(null, newDealer.NetworkCustomerCode);
      Assert.AreEqual(null, newDealer.DealerAccountCode);
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateCustomer_NewCustomer_CustomerCreated()
    {
      var context = new CustomerContext();
      context.New.BssId = IdGen.GetId().ToString();
      context.New.Type = CustomerTypeEnum.Customer;
      context.New.Name = "NEW_CUSTOMER";

      Services.Customers().CreateCustomer(context);

      var newCustomer = Ctx.OpContext.CustomerReadOnly.FirstOrDefault(x => x.BSSID == context.New.BssId);

      Assert.IsNotNull(newCustomer);
      Assert.IsTrue(newCustomer.ID > 0);
      Assert.AreEqual(context.New.Type, (CustomerTypeEnum)newCustomer.fk_CustomerTypeID);
      Assert.AreEqual(context.New.Name, newCustomer.Name);
      Assert.AreEqual(DealerNetworkEnum.None, (DealerNetworkEnum)newCustomer.fk_DealerNetworkID);
      Assert.AreEqual(null, newCustomer.NetworkDealerCode);
      Assert.AreEqual(null, newCustomer.NetworkCustomerCode);
      Assert.AreEqual(null, newCustomer.DealerAccountCode);
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateCustomer_NewAccount_AccountCreated()
    {
      var context = new CustomerContext();
      context.New.BssId = IdGen.GetId().ToString();
      context.New.Type = CustomerTypeEnum.Account;
      context.New.Name = "NEW_ACCOUNT";
      context.New.NetworkCustomerCode = "NETWORK_DEALER_CODE";
      context.New.DealerAccountCode = "DEALER_ACCOUNT_CODE";

      Services.Customers().CreateCustomer(context);

      var newAccount = Ctx.OpContext.CustomerReadOnly.FirstOrDefault(x => x.BSSID == context.New.BssId);

      Assert.IsNotNull(newAccount);
      Assert.IsTrue(newAccount.ID > 0);
      Assert.AreEqual(context.New.Type, (CustomerTypeEnum)newAccount.fk_CustomerTypeID);
      Assert.AreEqual(context.New.Name, newAccount.Name);
      Assert.AreEqual(DealerNetworkEnum.None, (DealerNetworkEnum)newAccount.fk_DealerNetworkID);
      Assert.AreEqual(null, newAccount.NetworkDealerCode);
      Assert.AreEqual(context.New.NetworkCustomerCode, newAccount.NetworkCustomerCode);
      Assert.AreEqual(context.New.DealerAccountCode, newAccount.DealerAccountCode);
    }

    [DatabaseTest]
    [TestMethod]
    public void Execute_AdminUserDefined_CreateAdminUser()
    {
      var dealer = Entity.Customer.Dealer.Save();

      var context = new CustomerContext();
      context.Id = dealer.ID;
      context.New.BssId = dealer.BSSID;
      context.New.Name = dealer.Name;
      context.New.Type = (CustomerTypeEnum)dealer.fk_CustomerTypeID;

      string firstName = "FIRST_NAME";
      string lastName = "LAST_NAME";
      string email = IdGen.GetId() + "@DOMAIN.COM";

      Services.Customers().CreateAdminUser(dealer.ID, firstName, lastName, email);

      User newUser = (from user in Ctx.OpContext.UserReadOnly
                      join customer in Ctx.OpContext.CustomerReadOnly on user.fk_CustomerID equals customer.ID
                      where customer.BSSID == context.New.BssId
                      select user).FirstOrDefault();

      Assert.IsNotNull(newUser);
      Assert.AreEqual(firstName, newUser.FirstName);
      Assert.AreEqual(lastName, newUser.LastName);
      Assert.AreEqual(email, newUser.EmailContact);
      Assert.AreEqual(10, newUser.Name.Length);
      Assert.AreEqual((int)LanguageEnum.enUS, newUser.fk_LanguageID);

      EmailQueue emailElement = (from mail in Ctx.OpContext.EmailQueueReadOnly where mail.MailTo == email select mail).FirstOrDefault();

      Assert.IsNotNull(emailElement);      
    }

    [DatabaseTest]
    [TestMethod]
    public void Execute_AdminUserNotDefined_CreateAdminUser()
    {
      var dealer = Entity.Customer.Dealer.Save();

      var context = new CustomerContext();
      context.Id = dealer.ID;
      context.New.BssId = dealer.BSSID;
      context.New.Name = dealer.Name;
      context.New.Type = (CustomerTypeEnum)dealer.fk_CustomerTypeID;

      string firstName = "";
      string lastName = "";
      string email = IdGen.GetId() + "@DOMAIN.COM"; // Email Address must be specified.

      Services.Customers().CreateAdminUser(dealer.ID, firstName, lastName, email);

      User newUser = (from user in Ctx.OpContext.UserReadOnly
                      join customer in Ctx.OpContext.CustomerReadOnly on user.fk_CustomerID equals customer.ID
                      where customer.BSSID == context.New.BssId
                      select user).FirstOrDefault();

      Assert.IsNotNull(newUser);
      Assert.AreEqual(firstName, newUser.FirstName);
      Assert.AreEqual(lastName, newUser.LastName);
      Assert.AreEqual(email, newUser.EmailContact);
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateCustomerRelationship_ParentDefined_CreateCustomerRelationship()
    {
      var dealer = Entity.Customer.Dealer.Id(IdGen.GetId()).BssId("PARENT_BSS_ID").Save();
      var account = Entity.Customer.Account.Id(IdGen.GetId()).BssId("CLIENT_BSS_ID").Save();

      var context = new CustomerContext();
      context.Id = account.ID;
      context.New.BssId = account.BSSID;
      context.New.Type = CustomerTypeEnum.Account;
      context.New.Name = "TEST_CLIENT_NAME";
      context.NewParent.Id = dealer.ID;
      context.NewParent.Type = CustomerTypeEnum.Dealer;
      context.NewParent.BssId = dealer.BSSID;
      context.NewParent.Name = "TEST_PARENT_NAME";
      context.NewParent.RelationshipId = IdGen.GetId().ToString();
      context.NewParent.RelationshipType = CustomerRelationshipTypeEnum.TCSDealer;

      Services.Customers().CreateCustomerRelationship(context);

      CustomerRelationship customerRelationship = (from relationship in Ctx.OpContext.CustomerRelationshipReadOnly
                                                   join customer in Ctx.OpContext.CustomerReadOnly on relationship.fk_ClientCustomerID equals customer.ID
                                                   where customer.BSSID == context.New.BssId
                                                   select relationship).SingleOrDefault();

      Assert.IsNotNull(customerRelationship);
      Assert.AreEqual(context.Id, customerRelationship.fk_ClientCustomerID);
      Assert.AreEqual(context.NewParent.Id, customerRelationship.fk_ParentCustomerID);
      Assert.AreEqual(context.NewParent.RelationshipId, customerRelationship.BSSRelationshipID);
    }

    [DatabaseTest]
    [TestMethod]
    public void CustomerRelationship_ParentDefined_DeleteCustomerRelationship()
    {
      var dealer = Entity.Customer.Dealer.Id(IdGen.GetId()).BssId("PARENT_BSS_ID").Save();
      var account = Entity.Customer.Account.Id(IdGen.GetId()).BssId("CLIENT_BSS_ID").Save();

      var context = new CustomerContext();
      context.Id = account.ID;
      context.New.BssId = account.BSSID;
      context.New.Type = CustomerTypeEnum.Account;
      context.New.Name = "TEST_CLIENT_NAME";
      context.NewParent.Id = dealer.ID;
      context.NewParent.Type = CustomerTypeEnum.Dealer;
      context.NewParent.BssId = dealer.BSSID;
      context.NewParent.Name = "TEST_PARENT_NAME";
      context.NewParent.RelationshipId = IdGen.GetId().ToString();
      context.NewParent.RelationshipType = CustomerRelationshipTypeEnum.TCSDealer;

      Services.Customers().CreateCustomerRelationship(context);

      CustomerRelationship customerRelationship = (from relationship in Ctx.OpContext.CustomerRelationshipReadOnly
                                                   join customer in Ctx.OpContext.CustomerReadOnly on relationship.fk_ClientCustomerID equals customer.ID
                                                   where customer.BSSID == context.New.BssId
                                                   select relationship).SingleOrDefault();

      Assert.IsNotNull(customerRelationship);
      Assert.AreEqual(context.Id, customerRelationship.fk_ClientCustomerID);
      Assert.AreEqual(context.NewParent.Id, customerRelationship.fk_ParentCustomerID);
      Assert.AreEqual(context.NewParent.RelationshipId, customerRelationship.BSSRelationshipID);

      Services.Customers().DeleteCustomerRelationship(dealer.ID, account.ID);

      customerRelationship = (from relationship in Ctx.OpContext.CustomerRelationshipReadOnly
                                                   join customer in Ctx.OpContext.CustomerReadOnly on relationship.fk_ClientCustomerID equals customer.ID
                                                   where customer.BSSID == context.New.BssId
                                                   select relationship).SingleOrDefault();

      Assert.IsNull(customerRelationship);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeactivateCustomer()
    {
      var dealer = Entity.Customer.Dealer.Id(IdGen.GetId()).BssId("PARENT_BSS_ID").Save();

      Services.Customers().DeactivateCustomer(dealer.ID);

      var cust = (from customer in Ctx.OpContext.CustomerReadOnly 
                                                   where customer.ID == dealer.ID
                                                   select customer).SingleOrDefault();

      Assert.IsNotNull(cust);
      Assert.IsFalse(cust.IsActivated);
    }

    [DatabaseTest]
    [TestMethod]
    public void ReactivateCustomer()
    {
      var dealer = Entity.Customer.Dealer.Id(IdGen.GetId()).BssId("PARENT_BSS_ID").Save();

      Services.Customers().ReactivateCustomer(dealer.ID);

      var cust = (from customer in Ctx.OpContext.CustomerReadOnly
                  where customer.ID == dealer.ID
                  select customer).SingleOrDefault();

      Assert.IsNotNull(cust);
      Assert.IsTrue(cust.IsActivated);
    }

    [TestMethod]
    public void AddCustomerReference_Success()
    {
      var mockAddBssReference = new Mock<IBssReference>();
      const long storeId = (long)StoreEnum.CAT;
      const string alias = "MakeCode_SN";
      const string value = "CAT_5YW00051";
      var uid = Guid.NewGuid();
      Services.Customers().AddCustomerReference(mockAddBssReference.Object, storeId, alias, value, uid);
      mockAddBssReference.Verify(o => o.AddCustomerReference(storeId, alias, value, uid), Times.Once());
    }

    [TestMethod]
    [DatabaseTest]
    public void HasStore_Test()
    {
      var dealer = Entity.Customer.Dealer.Id(IdGen.GetId()).DealerNetwork(DealerNetworkEnum.CAT).BssId("PARENT_BSS_ID").Save();
      var custStore = Services.Customers().HasStore(dealer.ID);
      Assert.IsFalse(custStore);
      Services.Customers().CreateStore(dealer.ID);
      custStore = Services.Customers().HasStore(dealer.ID);
      Assert.IsTrue(custStore);
    }
  }
}
