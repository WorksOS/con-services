using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
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
  public class AccountHierarchyCreateWorkflowTests : BssUnitTestBase
  {

    List<string> dealerNetwork = new List<string> { "CAT", "TRIMBLE", "SITECH", "NONE" };

    #region Dealer test cases

    /// <summary>
    /// Send a AH message for CAT, TRMB, SITECH, None and make sure the proper fault is returned.
    /// </summary>
    [DatabaseTest]
    [TestMethod]
    public void DealerCustomerType_ForEachDealerNetwork_ContactDefined_Success()
    {
      foreach (var item in dealerNetwork)
      {
        var message = BSS.AHCreated.ForDealer().DealerNetwork(item.ToString()).ContactDefined().Email(item+"@domain.com").Build();

        var result = ExecuteWorkFlow(message);

        Assert.IsTrue(result.Success);
      }
    }

    [DatabaseTest]
    [TestMethod]
    public void DealerCustomerType_ForEachDealerNetwork_ContactNotDefined_Failure()
    {
      foreach (var item in dealerNetwork)
      {
        var message = BSS.AHCreated.ForDealer().DealerNetwork(item.ToString()).Build();
        message.contact = null;

        var result = ExecuteWorkFlow(message);

        Assert.IsFalse(result.Success);
      }
    }

    [DatabaseTest]
    [TestMethod]
    public void Dealer_ParentDealerExists_ContactDefined_Success()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.AHCreated.ForDealer()
        .ParentBssId(dealer.BSSID)
        .RelationshipId(IdGen.GetId().ToString())
        .ContactDefined()
        .Build();

      var result = ExecuteWorkFlow(message);

      Assert.IsTrue(result.Success);
    }

    [DatabaseTest]
    [TestMethod]
    public void Dealer_ParentDealerExists_ContactNotDefined_Success()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.AHCreated.ForDealer().ContactDefined()
        .ParentBssId(dealer.BSSID)
        .RelationshipId(IdGen.GetId().ToString()).Build();

      var result = ExecuteWorkFlow(message);

      Assert.IsTrue(result.Success);
    }

    [DatabaseTest]
    [TestMethod]
    public void Dealer_Exists_Failure()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.AHCreated.ForDealer().ContactDefined()
        .BssId(dealer.BSSID)
        .Build();
      var result = ExecuteWorkFlow(message);

      Assert.IsFalse(result.Success);

      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.BSSID_EXISTS, "Dealer", message.BSSID));
    }
    
    [DatabaseTest]
    [TestMethod]
    public void Dealer_ParentDoesNotExist_Failure()
    {
      var message = BSS.AHCreated.ForDealer().ContactDefined()
        .ParentBssId(IdGen.GetId().ToString())
        .RelationshipId(IdGen.GetId().ToString())
        .Build();

      var result = ExecuteWorkFlow(message);

      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.PARENT_BSSID_DOES_NOT_EXIST, "Dealer", message.ParentBSSID));
    }

    [DatabaseTest]
    [TestMethod]
    public void Dealer_CreatedByCatStore_Success()
    {
      var mockCustomerLookup = new Mock<ICustomerLookup>();
      var id = IdGen.GetId().ToString(CultureInfo.InvariantCulture);
      var trimbleStoreBssId = id;
      var catStoreBssId = "StoreAPI_" + id;
      var networkDealerCode = "NetworkDealerCode_" + id;
      var catStoreCustomer = Entity.Customer.Dealer
        .BssId(catStoreBssId)
        .NetworkDealerCode(networkDealerCode)
        .Save();

      #region manual db setup

      // NH_OP..User
      Ctx.OpContext.User.AddObject(new User
      {
        fk_CustomerID = catStoreCustomer.ID,
        Name = "Name_" + id,
        PasswordHash = "PasswordHash_" + id,
        Salt = "Salt_" + id,
        EmailContact = id + "@email.com",
        UpdateUTC = DateTime.UtcNow,
        FirstName = "FirstName_" + id,
        LastName = "LastName_" + id,
        Active = true,
        GlobalID = "GlobalID_" + id
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());
      var existingUser =
        (from u in Ctx.OpContext.UserReadOnly where u.fk_CustomerID == catStoreCustomer.ID select u).First();

      // NH_OP..Device
      Ctx.OpContext.Device.AddObject(new Device
      {
        DeviceUID = Guid.NewGuid(),
        fk_DeviceStateID = (int) DeviceStateEnum.Provisioned,
        fk_DeviceTypeID = (int) DeviceTypeEnum.PL631,
        IBKey = "IBKey_" + id,
        GpsDeviceID = "GpsDeviceID_" + id,
        OwnerBSSID = catStoreCustomer.BSSID,
        UpdateUTC = DateTime.UtcNow
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());
      var existingDevice =
        (from d in Ctx.OpContext.DeviceReadOnly where d.OwnerBSSID == catStoreBssId select d).First();

      // NH_OP..Asset
      Ctx.OpContext.Asset.AddObject(new Asset
      {
        AssetID = long.Parse(id),
        AssetUID = Guid.NewGuid(),
        fk_DeviceID = existingDevice.ID,
        fk_MakeCode = "CAT",
        InsertUTC = DateTime.UtcNow,
        Name = "Name_" + id,
        SerialNumberVIN = "SerialNumberVIN_" + id,
        UpdateUTC = DateTime.UtcNow
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());
      var existingAsset =
        (from a in Ctx.OpContext.AssetReadOnly where a.fk_DeviceID == existingDevice.ID select a).First();

      // NH_OP..AssetDeviceHistory
      Ctx.OpContext.AssetDeviceHistory.AddObject(new AssetDeviceHistory
      {
        fk_AssetID = existingAsset.AssetID,
        fk_DeviceID = existingDevice.ID,
        OwnerBSSID = catStoreCustomer.BSSID,
        StartUTC = DateTime.UtcNow
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());

      // NH_OP..AssetAlias
      Ctx.OpContext.AssetAlias.AddObject(new AssetAlias
      {
        fk_AssetID = existingAsset.AssetID,
        fk_UserID = existingUser.ID,
        fk_CustomerID = catStoreCustomer.ID,
        IBKey = "IBKey_" + id,
        InsertUTC = DateTime.UtcNow,
        Name = "Name_" + id,
        NetworkDealerCode = networkDealerCode,
        OwnerBSSID = catStoreCustomer.BSSID
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());

      #endregion

      var ahCreatemessage =
        BSS.AHCreated.ForDealer()
        .ContactDefined()
        .BssId(trimbleStoreBssId)
        .NetworkDealerCode(networkDealerCode)
        .HierarchyType("TCS Dealer")
        .Build();
      var ahCreateResult = ExecuteWorkFlow(ahCreatemessage, mockCustomerLookup);

      Assert.IsTrue(ahCreateResult.Success);
      var customers =
        (from c in Ctx.OpContext.CustomerReadOnly where c.NetworkDealerCode == networkDealerCode select c).ToList();
      Assert.AreEqual(1, customers.Count);
      var updatedCustomer = customers.First();
      Assert.AreEqual(catStoreCustomer.CustomerUID, updatedCustomer.CustomerUID);
      Assert.AreEqual(trimbleStoreBssId, updatedCustomer.BSSID);

      mockCustomerLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()),
        Times.Once());
      mockCustomerLookup.Verify(
        o =>
          o.Add((long) StoreEnum.Trimble, "DealerCode", networkDealerCode,
            catStoreCustomer.CustomerUID.Value));

      var devices =
        (from d in Ctx.OpContext.DeviceReadOnly
          where d.ID == existingDevice.ID && d.OwnerBSSID == trimbleStoreBssId
          select d).ToList();
      Assert.AreEqual(1, devices.Count);

      var assetDeviceHistories =
        (from a in Ctx.OpContext.AssetDeviceHistoryReadOnly where a.OwnerBSSID == trimbleStoreBssId select a).ToList();
      Assert.AreEqual(1, devices.Count);
      Assert.AreEqual(existingDevice.ID, assetDeviceHistories.First().fk_DeviceID);
      Assert.AreEqual(existingAsset.AssetID, assetDeviceHistories.First().fk_AssetID);

      var assetAliases =
        (from a in Ctx.OpContext.AssetAliasReadOnly where a.OwnerBSSID == trimbleStoreBssId select a).ToList();
      Assert.AreEqual(1, assetAliases.Count);
      Assert.AreEqual(existingAsset.AssetID, assetAliases.First().fk_AssetID);
      Assert.AreEqual(updatedCustomer.ID, assetAliases.First().fk_CustomerID);

      var users =
        (from u in Ctx.OpContext.UserReadOnly where u.fk_CustomerID == updatedCustomer.ID select u).ToList();
      Assert.AreEqual(1, users.Count);
      Assert.AreEqual(existingUser.ID, users.First().ID);
    }

    #endregion Dealer Customer Type test cases

    #region Customer test cases

    [DatabaseTest]
    [TestMethod]
    public void Customer_ContactDefined_Success()
    {
      var message = BSS.AHCreated.ForCustomer().ContactDefined().Build();
      var result = ExecuteWorkFlow(message);
      Assert.IsTrue(result.Success);
    }

    [DatabaseTest]
    [TestMethod]
    public void Customer_ContactNotDefined_Failure()
    {
      var message = BSS.AHCreated.ForCustomer().Build();
      var result = ExecuteWorkFlow(message);
      Assert.IsFalse(result.Success);
    }

    [DatabaseTest]
    [TestMethod]
    public void Customer_ParentCustomerExists_Success()
    {
      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.AHCreated.ForCustomer().ContactDefined()
        .ParentBssId(parentCustomer.BSSID)
        .RelationshipId(IdGen.GetId().ToString())
        .Build();

      var result = ExecuteWorkFlow(message);

      Assert.IsTrue(result.Success);
    }
    
    [DatabaseTest]
    [TestMethod]
    public void Customer_Exists_Failure()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.AHCreated.ForCustomer().ContactDefined().BssId(customer.BSSID).Build();

      var result = ExecuteWorkFlow(message);

      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.BSSID_EXISTS, "Customer", message.BSSID));
    }

    [DatabaseTest]
    [TestMethod]
    public void Customer_CreatedByCatStore_Success()
    {
      var id = IdGen.GetId().ToString(CultureInfo.InvariantCulture);
      var trimbleStoreBssId = id;
      var catStoreBssId = "StoreAPI_" + id;
      var networkCustomerCode = "NetworkCustomerCode_" + id;
      var networkDealerCode = "NetworkDealerCode" + id;
      var catStoreCustomer = Entity.Customer.EndCustomer
        .BssId(catStoreBssId)
        .NetworkCustomerCode(networkCustomerCode)
        .Save();

      #region manual db setup

      // NH_OP..User
      Ctx.OpContext.User.AddObject(new User
      {
        fk_CustomerID = catStoreCustomer.ID,
        Name = "Name_" + id,
        PasswordHash = "PasswordHash_" + id,
        Salt = "Salt_" + id,
        EmailContact = id + "@email.com",
        UpdateUTC = DateTime.UtcNow,
        FirstName = "FirstName_" + id,
        LastName = "LastName_" + id,
        Active = true,
        GlobalID = "GlobalID_" + id
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());
      var existingUser =
        (from u in Ctx.OpContext.UserReadOnly where u.fk_CustomerID == catStoreCustomer.ID select u).First();

      // NH_OP..Device
      Ctx.OpContext.Device.AddObject(new Device
      {
        DeviceUID = Guid.NewGuid(),
        fk_DeviceStateID = (int)DeviceStateEnum.Provisioned,
        fk_DeviceTypeID = (int)DeviceTypeEnum.PL631,
        IBKey = "IBKey_" + id,
        GpsDeviceID = "GpsDeviceID_" + id,
        OwnerBSSID = catStoreCustomer.BSSID,
        UpdateUTC = DateTime.UtcNow
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());
      var existingDevice =
        (from d in Ctx.OpContext.DeviceReadOnly where d.OwnerBSSID == catStoreBssId select d).First();

      // NH_OP..Asset
      Ctx.OpContext.Asset.AddObject(new Asset
      {
        AssetID = long.Parse(id),
        AssetUID = Guid.NewGuid(),
        fk_DeviceID = existingDevice.ID,
        fk_MakeCode = "CAT",
        InsertUTC = DateTime.UtcNow,
        Name = "Name_" + id,
        SerialNumberVIN = "SerialNumberVIN_" + id,
        UpdateUTC = DateTime.UtcNow
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());
      var existingAsset =
        (from a in Ctx.OpContext.AssetReadOnly where a.fk_DeviceID == existingDevice.ID select a).First();

      // NH_OP..AssetDeviceHistory
      Ctx.OpContext.AssetDeviceHistory.AddObject(new AssetDeviceHistory
      {
        fk_AssetID = existingAsset.AssetID,
        fk_DeviceID = existingDevice.ID,
        OwnerBSSID = catStoreCustomer.BSSID,
        StartUTC = DateTime.UtcNow
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());

      // NH_OP..AssetAlias
      Ctx.OpContext.AssetAlias.AddObject(new AssetAlias
      {
        fk_AssetID = existingAsset.AssetID,
        fk_UserID = existingUser.ID,
        fk_CustomerID = catStoreCustomer.ID,
        IBKey = "IBKey_" + id,
        InsertUTC = DateTime.UtcNow,
        Name = "Name_" + id,
        NetworkCustomerCode = networkCustomerCode,
        OwnerBSSID = catStoreCustomer.BSSID
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());

      #endregion

      var ahCreatemessage =
        BSS.AHCreated.ForCustomer()
        .ContactDefined()
        .BssId(trimbleStoreBssId)
        .NetworkCustomerCode(networkCustomerCode)
        .HierarchyType("TCS Customer")
        .DealerNetwork("CAT")
        .NetworkDealerCode(networkDealerCode)
        .Build();
      var ahCreateResult = ExecuteWorkFlow(ahCreatemessage);

      Assert.IsTrue(ahCreateResult.Success);
      var customers =
        (from c in Ctx.OpContext.CustomerReadOnly where c.NetworkCustomerCode == networkCustomerCode select c).ToList();
      Assert.AreEqual(1, customers.Count);
      var updatedCustomer = customers.First();
      Assert.AreEqual(catStoreCustomer.CustomerUID, updatedCustomer.CustomerUID);
      Assert.AreEqual(trimbleStoreBssId, updatedCustomer.BSSID);

      var devices =
        (from d in Ctx.OpContext.DeviceReadOnly
         where d.ID == existingDevice.ID && d.OwnerBSSID == trimbleStoreBssId
         select d).ToList();
      Assert.AreEqual(1, devices.Count);

      var assetDeviceHistories =
        (from a in Ctx.OpContext.AssetDeviceHistoryReadOnly where a.OwnerBSSID == trimbleStoreBssId select a).ToList();
      Assert.AreEqual(1, devices.Count);
      Assert.AreEqual(existingDevice.ID, assetDeviceHistories.First().fk_DeviceID);
      Assert.AreEqual(existingAsset.AssetID, assetDeviceHistories.First().fk_AssetID);

      var assetAliases =
        (from a in Ctx.OpContext.AssetAliasReadOnly where a.OwnerBSSID == trimbleStoreBssId select a).ToList();
      Assert.AreEqual(1, assetAliases.Count);
      Assert.AreEqual(existingAsset.AssetID, assetAliases.First().fk_AssetID);
      Assert.AreEqual(updatedCustomer.ID, assetAliases.First().fk_CustomerID);

      var users =
        (from u in Ctx.OpContext.UserReadOnly where u.fk_CustomerID == updatedCustomer.ID select u).ToList();
      Assert.AreEqual(1, users.Count);
      Assert.AreEqual(existingUser.ID, users.First().ID);
    }

    #endregion Customer Customer Type Test cases

    #region Account test cases
    
    [DatabaseTest]
    [TestMethod]
    public void Account_ParentDoesNotExists_Failure()
    {
      var message = BSS.AHCreated.ForAccount().Build();

      var result = ExecuteWorkFlow(message);

      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.PARENT_BSSID_DOES_NOT_EXIST, "Dealer", message.ParentBSSID));     
    }

    [DatabaseTest]
    [TestMethod]
    public void Account_Exists_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.AHCreated.ForAccount()
        .BssId(account.BSSID)
        .Build();

      var result = ExecuteWorkFlow(message);

      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.BSSID_EXISTS, "Account", message.BSSID));
    }

    [DatabaseTest]
    [TestMethod]
    public void Account_RelationshipIdExists_Failure()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var rel = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var message = BSS.AHCreated.ForAccount()
        .ParentBssId(dealer.BSSID)
        .RelationshipId(rel.BSSRelationshipID)
        .Build();

      var result = ExecuteWorkFlow(message);

      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.Hierarchy.RELATIONSHIPID_EXISTS, message.RelationshipID));
    }

    [DatabaseTest]
    [TestMethod]
    public void Account_ParentDealerExists_Success()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.AHCreated.ForAccount()
        .ParentBssId(dealer.BSSID)
        .RelationshipId(IdGen.GetId().ToString())
        .Build();
      
      var result = ExecuteWorkFlow(message);

      Assert.IsTrue(result.Success);
    }

    [DatabaseTest]
    [TestMethod]
    public void Account_ParentCustomerExists_Success()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).NetworkDealerCode("5555").Save();
      var message = BSS.AHCreated.ForAccount()
        .ParentBssId(customer.BSSID)
        .RelationshipId(IdGen.GetId().ToString())
        .HierarchyType("TCS Customer").Build();
      
      var result = ExecuteWorkFlow(message);
      
      Assert.IsTrue(result.Success);
    }

    [DatabaseTest]
    [TestMethod]
    public void Account_CreatedByCatStore_ParentCustomer_Success()
    {
      var mockCustomerLookup = new Mock<ICustomerLookup>();
      var id = IdGen.GetId().ToString(CultureInfo.InvariantCulture);
      var trimbleStoreAccountBssId = id;
      var catStoreAccountBssId = "StoreAPI_" + id;
      var dealerAccountCode = "DealerAccountCode_" + id;
      var networkCustomerCode = "NetworkCustomerCode_" + id;
      var networkDealerCode = "NetworkDealerCode" + id;
      var parentCustomer = Entity.Customer.EndCustomer.BssId((int.Parse(id) + 1).ToString(CultureInfo.InvariantCulture))
        .NetworkCustomerCode(networkCustomerCode)
        .Save();
      var parentDealer = Entity.Customer.Dealer.BssId((int.Parse(id) + 2).ToString(CultureInfo.InvariantCulture))
        .NetworkDealerCode(networkDealerCode)
        .Save();
      var catStoreCustomer = Entity.Customer.Account
        .BssId(catStoreAccountBssId)
        .DealerAccountCode(dealerAccountCode)
        .Save();
      Entity.CustomerRelationship.Relate(parentCustomer, catStoreCustomer).Save();
      Entity.CustomerRelationship.Relate(parentDealer, catStoreCustomer).Save();

      var ahCreatemessage =
        BSS.AHCreated.ForAccount()
          .ContactDefined()
          .BssId(trimbleStoreAccountBssId)
          .DealerAccountCode(dealerAccountCode)
          .NetworkCustomerCode(networkCustomerCode)
          .ParentBssId(parentCustomer.BSSID)
          .RelationshipId(id)
          .HierarchyType("TCS Customer")
          .Build();
      var ahCreateResult = ExecuteWorkFlow(ahCreatemessage, mockCustomerLookup);

      Assert.IsTrue(ahCreateResult.Success);
      var customers = (from c in Ctx.OpContext.CustomerReadOnly where c.DealerAccountCode == dealerAccountCode select c).ToList();
      Assert.AreEqual(2, customers.Count);
      var newCustomer = customers.FirstOrDefault(o => o.BSSID == trimbleStoreAccountBssId);
      Assert.IsNotNull(newCustomer);
    }

    [DatabaseTest]
    [TestMethod]
    public void Account_CreatedByCatStore_InactiveParentDealer_Success()
    {
      var mockCustomerLookup = new Mock<ICustomerLookup>();
      var id = IdGen.GetId().ToString(CultureInfo.InvariantCulture);
      var trimbleStoreAccountBssId = id;
      var catStoreAccountBssId = "StoreAPI_" + id;
      var dealerAccountCode = "DealerAccountCode_" + id;
      var networkCustomerCode = "NetworkCustomerCode_" + id;
      var networkDealerCode = "NetworkDealerCode" + id;
      var parentCustomer = Entity.Customer.EndCustomer.BssId((int.Parse(id) + 1).ToString(CultureInfo.InvariantCulture))
        .NetworkCustomerCode(networkCustomerCode)
        .Save();
      var parentDealer = Entity.Customer.Dealer.BssId((int.Parse(id) + 2).ToString(CultureInfo.InvariantCulture))
        .NetworkDealerCode(networkDealerCode)
        .IsActivated(false)
        .Save();
      var catStoreCustomer = Entity.Customer.Account
        .BssId(catStoreAccountBssId)
        .DealerAccountCode(dealerAccountCode)
        .Save();
      Entity.CustomerRelationship.Relate(parentCustomer, catStoreCustomer).Save();
      Entity.CustomerRelationship.Relate(parentDealer, catStoreCustomer).Save();

      var ahCreatemessage =
        BSS.AHCreated.ForAccount()
          .ContactDefined()
          .BssId(trimbleStoreAccountBssId)
          .DealerAccountCode(dealerAccountCode)
          .NetworkCustomerCode(networkCustomerCode)
          .ParentBssId(parentDealer.BSSID)
          .RelationshipId(id)
          .HierarchyType("TCS Dealer")
          .Build();
      var ahCreateResult = ExecuteWorkFlow(ahCreatemessage, mockCustomerLookup);

      Assert.IsTrue(ahCreateResult.Success);
      var customers = (from c in Ctx.OpContext.CustomerReadOnly where c.DealerAccountCode == dealerAccountCode select c).ToList();
      Assert.AreEqual(2, customers.Count);
      var newCustomer = customers.FirstOrDefault(o => o.BSSID == trimbleStoreAccountBssId);
      Assert.IsNotNull(newCustomer);
    }

    [DatabaseTest]
    [TestMethod]
    public void Account_CreatedByCatStore_NoParentDealerRelationship_Success()
    {
      var mockCustomerLookup = new Mock<ICustomerLookup>();
      var id = IdGen.GetId().ToString(CultureInfo.InvariantCulture);
      var trimbleStoreAccountBssId = id;
      var catStoreAccountBssId = "StoreAPI_" + id;
      var dealerAccountCode = "DealerAccountCode_" + id;
      var networkCustomerCode = "NetworkCustomerCode_" + id;
      var networkDealerCode = "NetworkDealerCode" + id;
      var parentCustomer = Entity.Customer.EndCustomer.BssId((int.Parse(id) + 1).ToString(CultureInfo.InvariantCulture))
        .NetworkCustomerCode(networkCustomerCode)
        .Save();
      var parentDealer = Entity.Customer.Dealer.BssId((int.Parse(id) + 2).ToString(CultureInfo.InvariantCulture))
        .NetworkDealerCode(networkDealerCode)
        .Save();
      var catStoreCustomer = Entity.Customer.Account
        .BssId(catStoreAccountBssId)
        .DealerAccountCode(dealerAccountCode)
        .Save();
      Entity.CustomerRelationship.Relate(parentCustomer, catStoreCustomer).Save();

      var ahCreatemessage =
        BSS.AHCreated.ForAccount()
          .ContactDefined()
          .BssId(trimbleStoreAccountBssId)
          .DealerAccountCode(dealerAccountCode)
          .NetworkCustomerCode(networkCustomerCode)
          .ParentBssId(parentDealer.BSSID)
          .RelationshipId(id)
          .HierarchyType("TCS Dealer")
          .Build();
      var ahCreateResult = ExecuteWorkFlow(ahCreatemessage, mockCustomerLookup);

      Assert.IsTrue(ahCreateResult.Success);
      var customers = (from c in Ctx.OpContext.CustomerReadOnly where c.DealerAccountCode == dealerAccountCode select c).ToList();
      Assert.AreEqual(2, customers.Count);
      var newCustomer = customers.FirstOrDefault(o => o.BSSID == trimbleStoreAccountBssId);
      Assert.IsNotNull(newCustomer);
    }

    [DatabaseTest]
    [TestMethod]
    public void Account_CreatedByCatStore_MultipleParentDealerRelationships_Failure()
    {
      var mockCustomerLookup = new Mock<ICustomerLookup>();
      var id = IdGen.GetId().ToString(CultureInfo.InvariantCulture);
      var trimbleStoreAccountBssId = id;
      var catStoreAccountBssId = "StoreAPI_" + id;
      var dealerAccountCode = "DealerAccountCode_" + id;
      var networkCustomerCode = "NetworkCustomerCode_" + id;
      var networkDealerCode = "NetworkDealerCode" + id;
      var parentCustomer = Entity.Customer.EndCustomer.BssId((int.Parse(id) + 1).ToString(CultureInfo.InvariantCulture))
        .NetworkCustomerCode(networkCustomerCode)
        .Save();
      var parentDealer = Entity.Customer.Dealer.BssId((int.Parse(id) + 2).ToString(CultureInfo.InvariantCulture))
        .NetworkDealerCode(networkDealerCode)
        .Save();
      var catStoreCustomer = Entity.Customer.Account
        .BssId(catStoreAccountBssId)
        .DealerAccountCode(dealerAccountCode)
        .Save();
      var catStoreCustomer2 = Entity.Customer.Account
        .BssId(catStoreAccountBssId)
        .DealerAccountCode(dealerAccountCode)
        .Save();
      Entity.CustomerRelationship.Relate(parentCustomer, catStoreCustomer).Save();
      Entity.CustomerRelationship.Relate(parentDealer, catStoreCustomer).Save();
      Entity.CustomerRelationship.Relate(parentDealer, catStoreCustomer2).Save();

      var ahCreatemessage =
        BSS.AHCreated.ForAccount()
          .ContactDefined()
          .BssId(trimbleStoreAccountBssId)
          .DealerAccountCode(dealerAccountCode)
          .NetworkCustomerCode(networkCustomerCode)
          .ParentBssId(parentDealer.BSSID)
          .RelationshipId(id)
          .HierarchyType("TCS Dealer")
          .Build();

      var caughtException = false;
      try
      {
        ExecuteWorkFlow(ahCreatemessage, mockCustomerLookup);
      }
      catch (Exception ex)
      {
        caughtException = true;
        Assert.IsTrue(ex.Message.Contains(dealerAccountCode));
        Assert.IsTrue(ex.Message.Contains(parentDealer.BSSID));
      }

      Assert.IsTrue(caughtException);
    }

    [DatabaseTest]
    [TestMethod]
    public void Account_CreatedByCatStore_ParentDealer_Success()
    {
      var mockCustomerLookup = new Mock<ICustomerLookup>();
      var id = IdGen.GetId().ToString(CultureInfo.InvariantCulture);
      var trimbleStoreAccountBssId = id;
      var catStoreAccountBssId = "StoreAPI_" + id;
      var dealerAccountCode = "DealerAccountCode_" + id;
      var networkCustomerCode = "NetworkCustomerCode_" + id;
      var networkDealerCode = "NetworkDealerCode" + id;
      var parentCustomer = Entity.Customer.EndCustomer.BssId((int.Parse(id) + 1).ToString(CultureInfo.InvariantCulture))
        .NetworkCustomerCode(networkCustomerCode)
        .Save();
      var parentDealer = Entity.Customer.Dealer.BssId((int.Parse(id) + 2).ToString(CultureInfo.InvariantCulture))
        .NetworkDealerCode(networkDealerCode)
        .Save();
      var catStoreCustomer = Entity.Customer.Account
        .BssId(catStoreAccountBssId)
        .DealerAccountCode(dealerAccountCode)
        .Save();
      Entity.CustomerRelationship.Relate(parentCustomer, catStoreCustomer).Save();
      Entity.CustomerRelationship.Relate(parentDealer, catStoreCustomer).Save();

      #region manual db setup

      // NH_OP..User
      Ctx.OpContext.User.AddObject(new User
      {
        fk_CustomerID = catStoreCustomer.ID,
        Name = "Name_" + id,
        PasswordHash = "PasswordHash_" + id,
        Salt = "Salt_" + id,
        EmailContact = id + "@email.com",
        UpdateUTC = DateTime.UtcNow,
        FirstName = "FirstName_" + id,
        LastName = "LastName_" + id,
        Active = true,
        GlobalID = "GlobalID_" + id
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());
      var existingUser =
        (from u in Ctx.OpContext.UserReadOnly where u.fk_CustomerID == catStoreCustomer.ID select u).First();

      // NH_OP..Device
      Ctx.OpContext.Device.AddObject(new Device
      {
        DeviceUID = Guid.NewGuid(),
        fk_DeviceStateID = (int)DeviceStateEnum.Provisioned,
        fk_DeviceTypeID = (int)DeviceTypeEnum.PL631,
        IBKey = "IBKey_" + id,
        GpsDeviceID = "GpsDeviceID_" + id,
        OwnerBSSID = catStoreCustomer.BSSID,
        UpdateUTC = DateTime.UtcNow
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());
      var existingDevice =
        (from d in Ctx.OpContext.DeviceReadOnly where d.OwnerBSSID == catStoreAccountBssId select d).First();

      // NH_OP..Asset
      Ctx.OpContext.Asset.AddObject(new Asset
      {
        AssetID = long.Parse(id),
        AssetUID = Guid.NewGuid(),
        fk_DeviceID = existingDevice.ID,
        fk_MakeCode = "CAT",
        InsertUTC = DateTime.UtcNow,
        Name = "Name_" + id,
        SerialNumberVIN = "SerialNumberVIN_" + id,
        UpdateUTC = DateTime.UtcNow
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());
      var existingAsset =
        (from a in Ctx.OpContext.AssetReadOnly where a.fk_DeviceID == existingDevice.ID select a).First();

      // NH_OP..AssetDeviceHistory
      Ctx.OpContext.AssetDeviceHistory.AddObject(new AssetDeviceHistory
      {
        fk_AssetID = existingAsset.AssetID,
        fk_DeviceID = existingDevice.ID,
        OwnerBSSID = catStoreCustomer.BSSID,
        StartUTC = DateTime.UtcNow
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());

      // NH_OP..AssetAlias
      Ctx.OpContext.AssetAlias.AddObject(new AssetAlias
      {
        fk_AssetID = existingAsset.AssetID,
        fk_UserID = existingUser.ID,
        fk_CustomerID = catStoreCustomer.ID,
        IBKey = "IBKey_" + id,
        InsertUTC = DateTime.UtcNow,
        Name = "Name_" + id,
        DealerAccountCode = dealerAccountCode,
        OwnerBSSID = catStoreCustomer.BSSID
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());

      #endregion

      var ahCreatemessage =
        BSS.AHCreated.ForAccount()
          .ContactDefined()
          .BssId(trimbleStoreAccountBssId)
          .DealerAccountCode(dealerAccountCode)
          .NetworkCustomerCode(networkCustomerCode)
          .ParentBssId(parentDealer.BSSID)
          .RelationshipId(id)
          .HierarchyType("TCS Dealer")
          .Build();
      var ahCreateResult = ExecuteWorkFlow(ahCreatemessage, mockCustomerLookup);

      Assert.IsTrue(ahCreateResult.Success);
      var customers =
        (from c in Ctx.OpContext.CustomerReadOnly where c.DealerAccountCode == dealerAccountCode select c).ToList();
      Assert.AreEqual(1, customers.Count);
      var updatedCustomer = customers.First();
      Assert.AreEqual(catStoreCustomer.CustomerUID, updatedCustomer.CustomerUID);
      Assert.AreEqual(trimbleStoreAccountBssId, updatedCustomer.BSSID);

      mockCustomerLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()),
        Times.Once());
      mockCustomerLookup.Verify(
        o =>
          o.Add((long)StoreEnum.Trimble, "DealerCode_DCN",
            string.Format("{0}_{1}", networkDealerCode, dealerAccountCode), catStoreCustomer.CustomerUID.Value));

      var devices =
        (from d in Ctx.OpContext.DeviceReadOnly
         where d.ID == existingDevice.ID && d.OwnerBSSID == trimbleStoreAccountBssId
         select d).ToList();
      Assert.AreEqual(1, devices.Count);

      var assetDeviceHistories =
        (from a in Ctx.OpContext.AssetDeviceHistoryReadOnly where a.OwnerBSSID == trimbleStoreAccountBssId select a)
          .ToList();
      Assert.AreEqual(1, devices.Count);
      Assert.AreEqual(existingDevice.ID, assetDeviceHistories.First().fk_DeviceID);
      Assert.AreEqual(existingAsset.AssetID, assetDeviceHistories.First().fk_AssetID);

      var assetAliases =
        (from a in Ctx.OpContext.AssetAliasReadOnly where a.OwnerBSSID == trimbleStoreAccountBssId select a).ToList();
      Assert.AreEqual(1, assetAliases.Count);
      Assert.AreEqual(existingAsset.AssetID, assetAliases.First().fk_AssetID);
      Assert.AreEqual(updatedCustomer.ID, assetAliases.First().fk_CustomerID);

      var users =
        (from u in Ctx.OpContext.UserReadOnly where u.fk_CustomerID == updatedCustomer.ID select u).ToList();
      Assert.AreEqual(1, users.Count);
      Assert.AreEqual(existingUser.ID, users.First().ID);

      var relationships =
        (from r in Ctx.OpContext.CustomerRelationshipReadOnly
         where r.fk_ParentCustomerID == parentDealer.ID
            && r.fk_ClientCustomerID == catStoreCustomer.ID
            && r.fk_CustomerRelationshipTypeID == (int)CustomerRelationshipTypeEnum.TCSDealer
         select r).ToList();
      Assert.AreEqual(1, relationships.Count);
      Assert.AreEqual(id, relationships.First().BSSRelationshipID);
    }

    [DatabaseTest]
    [TestMethod]
    public void Account_CreatedByCatStore_ParentDealer_ParellelHierarchy_Success()
    {
      var mockCustomerLookup = new Mock<ICustomerLookup>();
      
      #region First Hierarchy

      var id = IdGen.GetId().ToString(CultureInfo.InvariantCulture);
      var trimbleStoreAccountBssId = id;
      var catStoreAccountBssId = "StoreAPI_" + id;
      var dealerAccountCode = "DealerAccountCode_" + id;
      var networkCustomerCode = "NetworkCustomerCode_" + id;
      var networkDealerCode = "NetworkDealerCode" + id;
      var parentCustomer = Entity.Customer.EndCustomer.BssId((int.Parse(id) + 1).ToString(CultureInfo.InvariantCulture))
        .NetworkCustomerCode(networkCustomerCode)
        .Save();
      var parentDealer = Entity.Customer.Dealer.BssId((int.Parse(id) + 2).ToString(CultureInfo.InvariantCulture))
        .NetworkDealerCode(networkDealerCode)
        .Save();
      var catStoreCustomer = Entity.Customer.Account
        .BssId(catStoreAccountBssId)
        .DealerAccountCode(dealerAccountCode)
        .Save();
      Entity.CustomerRelationship.Relate(parentCustomer, catStoreCustomer).Save();
      Entity.CustomerRelationship.Relate(parentDealer, catStoreCustomer).Save();

      #endregion

      #region Second Hierarchy (same dcn)

      var id2 = IdGen.GetId().ToString(CultureInfo.InvariantCulture);
      var catStoreAccountBssId2 = "StoreAPI_" + id2;
      var networkCustomerCode2 = "NetworkCustomerCode_" + id2;
      var networkDealerCode2 = "NetworkDealerCode" + id2;
      var parentCustomer2 =
        Entity.Customer.EndCustomer.BssId((int.Parse(id2) + 1).ToString(CultureInfo.InvariantCulture))
          .NetworkCustomerCode(networkCustomerCode2)
          .Save();
      var parentDealer2 = Entity.Customer.Dealer.BssId((int.Parse(id2) + 2).ToString(CultureInfo.InvariantCulture))
        .NetworkDealerCode(networkDealerCode2)
        .Save();
      var catStoreCustomer2 = Entity.Customer.Account
        .BssId(catStoreAccountBssId2)
        .DealerAccountCode(dealerAccountCode)
        .Save();
      Entity.CustomerRelationship.Relate(parentCustomer2, catStoreCustomer2).Save();
      Entity.CustomerRelationship.Relate(parentDealer2, catStoreCustomer2).Save();

      #endregion

      #region manual db setup

      // NH_OP..User
      Ctx.OpContext.User.AddObject(new User
      {
        fk_CustomerID = catStoreCustomer.ID,
        Name = "Name_" + id,
        PasswordHash = "PasswordHash_" + id,
        Salt = "Salt_" + id,
        EmailContact = id + "@email.com",
        UpdateUTC = DateTime.UtcNow,
        FirstName = "FirstName_" + id,
        LastName = "LastName_" + id,
        Active = true,
        GlobalID = "GlobalID_" + id
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());
      var existingUser =
        (from u in Ctx.OpContext.UserReadOnly where u.fk_CustomerID == catStoreCustomer.ID select u).First();

      // NH_OP..Device
      Ctx.OpContext.Device.AddObject(new Device
      {
        DeviceUID = Guid.NewGuid(),
        fk_DeviceStateID = (int) DeviceStateEnum.Provisioned,
        fk_DeviceTypeID = (int) DeviceTypeEnum.PL631,
        IBKey = "IBKey_" + id,
        GpsDeviceID = "GpsDeviceID_" + id,
        OwnerBSSID = catStoreCustomer.BSSID,
        UpdateUTC = DateTime.UtcNow
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());
      var existingDevice =
        (from d in Ctx.OpContext.DeviceReadOnly where d.OwnerBSSID == catStoreAccountBssId select d).First();

      // NH_OP..Asset
      Ctx.OpContext.Asset.AddObject(new Asset
      {
        AssetID = long.Parse(id),
        AssetUID = Guid.NewGuid(),
        fk_DeviceID = existingDevice.ID,
        fk_MakeCode = "CAT",
        InsertUTC = DateTime.UtcNow,
        Name = "Name_" + id,
        SerialNumberVIN = "SerialNumberVIN_" + id,
        UpdateUTC = DateTime.UtcNow
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());
      var existingAsset =
        (from a in Ctx.OpContext.AssetReadOnly where a.fk_DeviceID == existingDevice.ID select a).First();

      // NH_OP..AssetDeviceHistory
      Ctx.OpContext.AssetDeviceHistory.AddObject(new AssetDeviceHistory
      {
        fk_AssetID = existingAsset.AssetID,
        fk_DeviceID = existingDevice.ID,
        OwnerBSSID = catStoreCustomer.BSSID,
        StartUTC = DateTime.UtcNow
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());

      // NH_OP..AssetAlias
      Ctx.OpContext.AssetAlias.AddObject(new AssetAlias
      {
        fk_AssetID = existingAsset.AssetID,
        fk_UserID = existingUser.ID,
        fk_CustomerID = catStoreCustomer.ID,
        IBKey = "IBKey_" + id,
        InsertUTC = DateTime.UtcNow,
        Name = "Name_" + id,
        DealerAccountCode = dealerAccountCode,
        OwnerBSSID = catStoreCustomer.BSSID
      });
      Assert.AreEqual(1, Ctx.OpContext.SaveChanges());

      #endregion

      var ahCreatemessage =
        BSS.AHCreated.ForAccount()
          .ContactDefined()
          .BssId(trimbleStoreAccountBssId)
          .DealerAccountCode(dealerAccountCode)
          .NetworkCustomerCode(networkCustomerCode)
          .ParentBssId(parentDealer.BSSID)
          .RelationshipId(id)
          .HierarchyType("TCS Dealer")
          .Build();
      var ahCreateResult = ExecuteWorkFlow(ahCreatemessage, mockCustomerLookup);

      Assert.IsTrue(ahCreateResult.Success);
      var customers =
        (from c in Ctx.OpContext.CustomerReadOnly where c.DealerAccountCode == dealerAccountCode select c).ToList();
      Assert.AreEqual(2, customers.Count);
      var updatedCustomer = customers.SingleOrDefault(o => o.ID == catStoreCustomer.ID);
      Assert.IsNotNull(updatedCustomer);
      Assert.AreEqual(catStoreCustomer.CustomerUID, updatedCustomer.CustomerUID);
      Assert.AreEqual(trimbleStoreAccountBssId, updatedCustomer.BSSID);

      mockCustomerLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()),
        Times.Once());
      mockCustomerLookup.Verify(
        o =>
          o.Add((long) StoreEnum.Trimble, "DealerCode_DCN",
            string.Format("{0}_{1}", networkDealerCode, dealerAccountCode), catStoreCustomer.CustomerUID.Value));

      var devices =
        (from d in Ctx.OpContext.DeviceReadOnly
          where d.ID == existingDevice.ID && d.OwnerBSSID == trimbleStoreAccountBssId
          select d).ToList();
      Assert.AreEqual(1, devices.Count);

      var assetDeviceHistories =
        (from a in Ctx.OpContext.AssetDeviceHistoryReadOnly where a.OwnerBSSID == trimbleStoreAccountBssId select a)
          .ToList();
      Assert.AreEqual(1, devices.Count);
      Assert.AreEqual(existingDevice.ID, assetDeviceHistories.First().fk_DeviceID);
      Assert.AreEqual(existingAsset.AssetID, assetDeviceHistories.First().fk_AssetID);

      var assetAliases =
        (from a in Ctx.OpContext.AssetAliasReadOnly where a.OwnerBSSID == trimbleStoreAccountBssId select a).ToList();
      Assert.AreEqual(1, assetAliases.Count);
      Assert.AreEqual(existingAsset.AssetID, assetAliases.First().fk_AssetID);
      Assert.AreEqual(updatedCustomer.ID, assetAliases.First().fk_CustomerID);

      var users =
        (from u in Ctx.OpContext.UserReadOnly where u.fk_CustomerID == updatedCustomer.ID select u).ToList();
      Assert.AreEqual(1, users.Count);
      Assert.AreEqual(existingUser.ID, users.First().ID);

      var relationships =
        (from r in Ctx.OpContext.CustomerRelationshipReadOnly
          where r.fk_ParentCustomerID == parentDealer.ID
                && r.fk_ClientCustomerID == catStoreCustomer.ID
                && r.fk_CustomerRelationshipTypeID == (int) CustomerRelationshipTypeEnum.TCSDealer
          select r).ToList();
      Assert.AreEqual(1, relationships.Count);
      Assert.AreEqual(id, relationships.First().BSSRelationshipID);
    }

    #endregion Account Customer Type Test cases

    #region Customer Customer Type Test Cases

    /*
      1. Create a relationship between customer and customer and make sure not errors reported.
    */
    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Customer_Customer_Success()
    {
      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.AHCreated.ForCustomer().ContactDefined()
        .ParentBssId(parentCustomer.BSSID)
        .RelationshipId(IdGen.GetId().ToString())
        .Build();

      var result = ExecuteWorkFlow(message);
      Assert.IsTrue(result.Success);

      var relQuery = (from r in Ctx.OpContext.CustomerRelationshipReadOnly
                      where r.fk_ParentCustomerID == parentCustomer.ID
                      select r).ToList();

      Assert.IsNotNull(relQuery);
      Assert.AreEqual(1, relQuery.Count());
    }

    #endregion

    private static WorkflowResult ExecuteWorkFlow(AccountHierarchy message, Mock<ICustomerLookup> mockCustomerLookup = null)
    {
      var customerLookup = mockCustomerLookup ?? new Mock<ICustomerLookup>();
      var workFlow = new BssWorkflowFactory(new BssReference(new Mock<IAssetLookup>().Object, customerLookup.Object)).Create(message);
      var result = new WorkflowRunner().Run(workFlow);
      new ConsoleResultProcessor().Process(message, result);
      return result;
    }
  }
}
