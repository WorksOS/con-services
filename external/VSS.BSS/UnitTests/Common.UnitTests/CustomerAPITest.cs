using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
  [TestClass()]
  public class CustomerAPITest : UnitTestBase
  {
    [DatabaseTest]
    [TestMethod()]
    public void Update_Success_SyncWithNG()
    {

      var mockCustomerService = new Mock<ICustomerService>();
      mockCustomerService.Setup(x => x.Update(It.IsAny<object>())).Returns(true);

      CustomerAPI target = new CustomerAPI(mockCustomerService.Object);

      Customer expected = target.CreateCustomer(Ctx.OpContext, "CUSTOMER_API1", "BSSID");

      Customer actual = (from c in Ctx.OpContext.CustomerReadOnly
                         where c.Name == "CUSTOMER_API1"
                         select c).FirstOrDefault();

      Assert.AreEqual(expected.Name, actual.Name);
      Assert.AreEqual(expected.BSSID, actual.BSSID);
      Assert.AreEqual(expected.fk_CustomerTypeID, actual.fk_CustomerTypeID);
      Assert.AreEqual(expected.NetworkDealerCode, null);
      Assert.AreEqual(expected.fk_DealerNetworkID, actual.fk_DealerNetworkID);

      //Update some properties
      Param p = new Param();
      p.Name = "BSSID";
      p.Value ="New BSS ID";
      List<Param> lst = new List<Param>();
      lst.Add(p);
      target.Update(Ctx.OpContext, actual.ID, lst);

      //get cust again to check updated value
      actual = (from c in Ctx.OpContext.CustomerReadOnly
                where c.Name == "CUSTOMER_API1"
                select c).FirstOrDefault();
      Assert.AreEqual(actual.BSSID, "New BSS ID", "Customer Update did not work");

      mockCustomerService.Verify(x => x.Update(It.IsAny<object>()), Times.Once());
    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateDealer_Success_SyncWithNG()
    {
      var mockCustomerService = new Mock<ICustomerService>();
      mockCustomerService.Setup(x => x.Create(It.IsAny<object>())).Returns(true);

      CustomerAPI target = new CustomerAPI(mockCustomerService.Object);

      Customer expected = target.CreateDealer(Ctx.OpContext, "DEALER", "BSSID", "CAT", DealerNetworkEnum.CAT);

      Customer actual = (from c in Ctx.OpContext.CustomerReadOnly
                         where c.Name == "DEALER"
                         select c).FirstOrDefault();

      Assert.AreEqual(expected.Name, actual.Name);
      Assert.AreEqual(expected.BSSID, actual.BSSID);
      Assert.AreEqual(expected.fk_CustomerTypeID, actual.fk_CustomerTypeID);
      Assert.AreEqual(expected.NetworkDealerCode, actual.NetworkDealerCode);
      Assert.AreEqual(expected.fk_DealerNetworkID, actual.fk_DealerNetworkID);

      mockCustomerService.Verify(x => x.Create(It.IsAny<object>()), Times.Once());
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateDealerAddsDefaultCustomerStoreRecord()
    {
      CustomerAPI target = new CustomerAPI();
      Customer customer = target.CreateDealer(Ctx.OpContext, "Dealer", "BSSID", "Cat", DealerNetworkEnum.CAT);
      CustomerStore customerStore = (from c in Ctx.OpContext.CustomerStoreReadOnly where c.fk_CustomerID == customer.ID select c).FirstOrDefault();
      Assert.IsNotNull(customerStore);
      Assert.AreEqual(1, customerStore.fk_StoreID);
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateDealerAddsCustomerStoreRecord()
    {
      Store store = new Store();
      store.Description = "Test";
      store.Name = "Test";
      Ctx.OpContext.Store.AddObject(store);
      Ctx.OpContext.SaveChanges();
      CustomerAPI target = new CustomerAPI();
      Customer customer = target.CreateDealer(Ctx.OpContext, "Dealer", "BSSID", "Cat", DealerNetworkEnum.CAT, store.ID);
      CustomerStore customerStore = (from c in Ctx.OpContext.CustomerStoreReadOnly where c.fk_CustomerID == customer.ID && c.fk_StoreID == store.ID select c).FirstOrDefault();
      Assert.IsNotNull(customerStore);
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateCustomerAddsDefaultCustomerStoreRecord()
    {
      CustomerAPI target = new CustomerAPI();
      Customer customer = target.CreateCustomer(Ctx.OpContext, "CUSTOMER_API1", "BSSID");
      CustomerStore customerStore = (from c in Ctx.OpContext.CustomerStoreReadOnly where c.fk_CustomerID == customer.ID select c).FirstOrDefault();
      Assert.IsNotNull(customerStore);
      Assert.AreEqual(1, customerStore.fk_StoreID);
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateCustomerAddsCustomerStoreRecord()
    {
      Store store = new Store();
      store.Description = "Test";
      store.Name = "Test";
      Ctx.OpContext.Store.AddObject(store);
      Ctx.OpContext.SaveChanges();
      CustomerAPI target = new CustomerAPI();
      Customer customer = target.CreateCustomer(Ctx.OpContext, "CUSTOMER_API1","BSSID", store.ID);
      CustomerStore customerStore = (from c in Ctx.OpContext.CustomerStoreReadOnly where c.fk_CustomerID == customer.ID && c.fk_StoreID == store.ID select c).FirstOrDefault();
      Assert.IsNotNull(customerStore);
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateAccountAddsDefaultCustomerStoreRecord()
    {
      CustomerAPI target = new CustomerAPI();
      Customer customer = target.CreateAccount(Ctx.OpContext, "Dealer_Account", "BSSID", "DealerAcctCode", "NetCustCode");
      CustomerStore customerStore = (from c in Ctx.OpContext.CustomerStoreReadOnly where c.fk_CustomerID == customer.ID select c).FirstOrDefault();
      Assert.IsNotNull(customerStore);
      Assert.AreEqual(1, customerStore.fk_StoreID);
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateAccountAddsCustomerStoreRecord()
    {
      Store store = new Store();
      store.Description = "Test";
      store.Name = "Test";
      Ctx.OpContext.Store.AddObject(store);
      Ctx.OpContext.SaveChanges();
      CustomerAPI target = new CustomerAPI();
      Customer customer = target.CreateAccount(Ctx.OpContext, "Dealer_Account", "BSSID", "DealerAcctCode", "NetCustCode", store.ID);
      CustomerStore customerStore = (from c in Ctx.OpContext.CustomerStoreReadOnly where c.fk_CustomerID == customer.ID && c.fk_StoreID == store.ID select c).FirstOrDefault();
      Assert.IsNotNull(customerStore);
    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateCustomer_Success()
    {
      CustomerAPI target = new CustomerAPI();

      Customer expected = target.CreateCustomer(Ctx.OpContext, "CUSTOMER_API1", "BSSID");

      Customer actual = (from c in Ctx.OpContext.CustomerReadOnly
                         where c.Name == "CUSTOMER_API1"
                         select c).FirstOrDefault();

      Assert.AreEqual(expected.Name, actual.Name);
      Assert.AreEqual(expected.BSSID, actual.BSSID);
      Assert.AreEqual(expected.fk_CustomerTypeID, actual.fk_CustomerTypeID);
      Assert.AreEqual(expected.NetworkDealerCode, null);
      Assert.AreEqual(expected.fk_DealerNetworkID, actual.fk_DealerNetworkID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateAccount_Success()
    {
      CustomerAPI target = new CustomerAPI();

      Customer expected = target.CreateAccount(Ctx.OpContext, "Dealer_Account", "BSSID", "DealerAcctCode", "NetCustCode");

      Customer actual = (from c in Ctx.OpContext.CustomerReadOnly
                         where c.Name == "Dealer_Account"
                         select c).FirstOrDefault();

      Assert.AreEqual(expected.Name, actual.Name);
      Assert.AreEqual(expected.BSSID, actual.BSSID);
      Assert.AreEqual(expected.fk_CustomerTypeID, actual.fk_CustomerTypeID);
      Assert.AreEqual(expected.DealerAccountCode, actual.DealerAccountCode);
      Assert.AreEqual(expected.NetworkCustomerCode, actual.NetworkCustomerCode);
      Assert.AreEqual(expected.fk_DealerNetworkID, actual.fk_DealerNetworkID);
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    [Ignore]
    public void CreateAccount_FailureSaveChangesException()
    {

    }

    [DatabaseTest]
    [TestMethod()]
    [Ignore]
    public void CreateCustomerRelationship_Success()
    {

    }

    [DatabaseTest]
    [TestMethod()]
    [Ignore]
    public void RemoveCustomerRelationship_Success()
    {

    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    [Ignore]
    public void Update_FailureCannotRenameTrimbleOperationsCustomer()
    {

    }

    [DatabaseTest]
    [TestMethod()]
    [Ignore]
    public void Delete_Success()
    {

    }

    [DatabaseTest]
    [TestMethod()]
    [Ignore]
    public void Delete_SuccessIfCustomerIsNull()
    {

    }

    [DatabaseTest]
    [TestMethod()]
    [Ignore]
    public void GetDeletedCustomerPrefix_Success()
    {

    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(Exception))]
    [Ignore]
    public void GetTrimbleOperationsCustomerID_FailureNoTrimbleOpsCustomerInDB()
    {

    }

    [DatabaseTest]
    [TestMethod()]
    public void Activate_Success()
    {
      CustomerAPI target = new CustomerAPI();

      Customer expected = target.CreateCustomer(Ctx.OpContext, "CUSTOMER_API1", "BSSID");

      Customer actual = (from c in Ctx.OpContext.CustomerReadOnly
                         where c.Name == "CUSTOMER_API1"
                         select c).FirstOrDefault();

      Assert.AreEqual(expected.Name, actual.Name);
      Assert.AreEqual(actual.IsActivated, true, "customer is not Activated properly");

      // deactivate the customer
      bool success = target.Deactivate(Ctx.OpContext, actual.ID);
      // get cust again
      actual = (from c in Ctx.OpContext.CustomerReadOnly
                where c.Name == "CUSTOMER_API1"
                select c).FirstOrDefault();

      Assert.AreEqual(actual.IsActivated, false, "customer is not DeActivated properly");
      //re activate
      success = target.Activate(Ctx.OpContext, actual.ID);
      //get cust again
      actual = (from c in Ctx.OpContext.CustomerReadOnly
                where c.Name == "CUSTOMER_API1"
                select c).FirstOrDefault();
      Assert.AreEqual(actual.IsActivated, true, "customer is not Activated properly");
    }

    [DatabaseTest]
    [TestMethod()]
    public void Deactivate_Success()
    {
      CustomerAPI target = new CustomerAPI();

      Customer expected = target.CreateCustomer(Ctx.OpContext, "CUSTOMER_API1", "BSSID");

      Customer actual = (from c in Ctx.OpContext.CustomerReadOnly
                         where c.Name == "CUSTOMER_API1"
                         select c).FirstOrDefault();

      Assert.AreEqual(expected.Name, actual.Name);
      Assert.AreEqual(actual.IsActivated, true, "customer is not Activated properly");

      // deactivate the customer
      bool success = target.Deactivate(Ctx.OpContext, actual.ID);

      //get cust again
      actual = (from c in Ctx.OpContext.CustomerReadOnly
                where c.ID == actual.ID
                         select c).FirstOrDefault();

      Assert.AreEqual(actual.IsActivated, false, "customer is not DeActivated properly");
    }

    [DatabaseTest]
    [TestMethod()]
    public void GetTrimbleOperationsCustomerID_Success()
    {
      CustomerAPI target = new CustomerAPI();
      long trimble_opID = target.GetTrimbleOperationsCustomerID();

      Assert.AreEqual(trimble_opID, 1);
    }

    [TestClass]
    public sealed class IsAssetViewableByCustomer : CustomerAPITest
    {
      private const string gpsPrefix = "IsAssetViewableByCustomer_";
      class TestDataAssetViewable
      {
        public string GpsDeviceId { get; set; }
        public DealerNetworkEnum OwnerDealerNetwork { get; set; }
        public DealerNetworkEnum UserDealerNetwork { get; set; }

        public void SaveToContext(out long customerId)
        {
          if (string.IsNullOrEmpty(this.GpsDeviceId)
            || OwnerDealerNetwork == DealerNetworkEnum.None || UserDealerNetwork == DealerNetworkEnum.None)
          {
            throw new ArgumentNullException("test not initialized properly");
          }
          var ownerOfDevice = Entity.Customer.Corporate.DealerNetwork(this.OwnerDealerNetwork).Save();
          var device = Entity.Device.MTS522.OwnerBssId(ownerOfDevice.BSSID).GpsDeviceId(this.GpsDeviceId).Save();
          var user = Entity.Customer.Corporate.DealerNetwork(this.UserDealerNetwork).Save();

          customerId = user.ID;
        }
      }

      [TestMethod]
      [DatabaseTest]
      public void FriendlyDealers_ShouldView()
      {

        var tests = new TestDataAssetViewable[]{
          new TestDataAssetViewable(){
            GpsDeviceId = gpsPrefix + "1",
            OwnerDealerNetwork = DealerNetworkEnum.CASE,
            UserDealerNetwork = DealerNetworkEnum.NEWHOLLAND
          },
          new TestDataAssetViewable(){
            GpsDeviceId = gpsPrefix + "2",
            OwnerDealerNetwork = DealerNetworkEnum.DOOSAN,
            UserDealerNetwork = DealerNetworkEnum.THC
          }
        };

        foreach (var test in tests)
        {
          long customerId;
          test.SaveToContext(out customerId);
          bool result = API.Customer.IsAssetViewableByCustomer(test.GpsDeviceId, customerId);
          Assert.IsTrue(result, string.Format("Asset of {0} is not viewable by customer of {1}", test.OwnerDealerNetwork.ToString(), test.UserDealerNetwork.ToString()));
        }
      }

      [TestMethod]
      [DatabaseTest]
      public void SameDealer_ShouldView()
      {
        var test = new TestDataAssetViewable()
        {
          GpsDeviceId = gpsPrefix + "3",
          OwnerDealerNetwork = DealerNetworkEnum.CAT,
          UserDealerNetwork = DealerNetworkEnum.CAT
        };
        long customerId;
        test.SaveToContext(out customerId);
        bool result = API.Customer.IsAssetViewableByCustomer(test.GpsDeviceId, customerId);
        Assert.IsTrue(result, string.Format("Asset of {0} is not viewable by customer of {1}", test.OwnerDealerNetwork.ToString(), test.UserDealerNetwork.ToString()));
      }

      [TestMethod]
      [DatabaseTest]
      public void DifferentUnfriendlyDealers_ShouldNotView()
      {
        var test = new TestDataAssetViewable()
        {
          GpsDeviceId = gpsPrefix + "4",
          OwnerDealerNetwork = DealerNetworkEnum.LEEBOY,
          UserDealerNetwork = DealerNetworkEnum.CAT
        };
        long customerId;
        test.SaveToContext(out customerId);
        //var session = Helpers.Sessions.GetContextFor(TestData.CustomerUserActiveUser);
        bool result = API.Customer.IsAssetViewableByCustomer(test.GpsDeviceId, customerId);
        Assert.IsFalse(result, string.Format("Asset of {0} viewable by customer of {1}", test.OwnerDealerNetwork.ToString(), test.UserDealerNetwork.ToString()));
      }

    }
  }
}
