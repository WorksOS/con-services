using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
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
  public class AccountHierarchyDeleteWorkflowTests : BssUnitTestBase
  {
    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_NewAccount_ContactDefined_Failure()
    {
      var message = BSS.AHDeleted.ForAccount().BssId(IdGen.StringId()).ContactDefined().Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.AreEqual(1, result.ActivityResults.Count);
      Assert.IsTrue(result.ActivityResults[0].Summary.Contains("PrimaryContact is defined."));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_NewCustomer_ContactDefined_Failure()
    {
      var message = BSS.AHDeleted.ForAccount().BssId(IdGen.StringId()).ContactDefined().Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.AreEqual(1, result.ActivityResults.Count);
      Assert.IsTrue(result.ActivityResults[0].Summary.Contains("PrimaryContact is defined."));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_NewDealer_ContactDefined_Failure()
    {
      var message = BSS.AHDeleted.ForDealer().BssId(IdGen.StringId()).ContactDefined().Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.AreEqual(1, result.ActivityResults.Count);
      Assert.IsTrue(result.ActivityResults[0].Summary.Contains("PrimaryContact is defined."));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_NewAccount_Failure()
    {
      var message = BSS.AHDeleted.ForAccount().BssId(IdGen.StringId()).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.IsTrue(result.ActivityResults.Any(t => t.Summary.Contains("No customer found for BSSID")));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_NewCustomer_Failure()
    {
      var message = BSS.AHDeleted.ForCustomer().BssId(IdGen.StringId()).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.IsTrue(result.ActivityResults.Any(t => t.Summary.Contains("ParentBSSID is not defined")));
      Assert.IsTrue(result.ActivityResults.Any(t => t.Summary.Contains("RelationshipID is not defined")));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_NewDealer_Failure()
    {
      var message = BSS.AHDeleted.ForDealer().BssId(IdGen.StringId()).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.IsTrue(result.ActivityResults.Any(t => t.Summary.Contains("ParentBSSID is not defined")));
      Assert.IsTrue(result.ActivityResults.Any(t => t.Summary.Contains("RelationshipID is not defined")));
    }

    [TestMethod]
    public void AccountHierarchy_Delete_NewCustomer_ParentBSSIDDefined_Failure()
    {
      var message = BSS.AHDeleted.ForCustomer().BssId(IdGen.StringId()).ParentBssId(IdGen.StringId()).RelationshipId(null).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.AreEqual(1, result.ActivityResults.Count);
      Assert.IsTrue(result.ActivityResults[0].Summary.Contains("RelationshipID is not defined."));
    }

    [TestMethod]
    public void AccountHierarchy_Delete_NewAccount_ParentBSSIDDefined_Failure()
    {
      var message = BSS.AHDeleted.ForAccount().BssId(IdGen.StringId()).ParentBssId(IdGen.StringId()).RelationshipId(null).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.AreEqual(1, result.ActivityResults.Count);
      Assert.IsTrue(result.ActivityResults[0].Summary.Contains("RelationshipID is not defined."));
    }

    [TestMethod]
    public void AccountHierarchy_Delete_NewDealer_ParentBSSIDDefined_Failure()
    {
      var message = BSS.AHDeleted.ForDealer().BssId(IdGen.StringId()).ParentBssId(IdGen.StringId()).RelationshipId(null).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.AreEqual(1, result.ActivityResults.Count);
      Assert.IsTrue(result.ActivityResults[0].Summary.Contains("RelationshipID is not defined."));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_NewDealer_RelationshipIDDefined_Failure()
    {
      var message = BSS.AHDeleted.ForDealer().BssId(IdGen.StringId()).ParentBssId(IdGen.StringId()).RelationshipId(null).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.AreEqual(1, result.ActivityResults.Count);
      Assert.IsTrue(result.ActivityResults[0].Summary.Contains("RelationshipID is not defined."));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_NewCustomer_RelationshipIDDefined_Failure()
    {
      var message = BSS.AHDeleted.ForCustomer().BssId(IdGen.StringId()).ParentBssId(IdGen.StringId()).RelationshipId(null).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.AreEqual(1, result.ActivityResults.Count);
      Assert.IsTrue(result.ActivityResults[0].Summary.Contains("RelationshipID is not defined."));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_NewAccount_RelationshipIDDefined_Failure()
    {
      var message = BSS.AHDeleted.ForAccount().BssId(IdGen.StringId()).ParentBssId(IdGen.StringId()).RelationshipId(null).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.AreEqual(1, result.ActivityResults.Count);
      Assert.IsTrue(result.ActivityResults[0].Summary.Contains("RelationshipID is not defined."));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_ExistingDealerAndAccount_RelationshipIDNotDefined_Failure()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

      Entity.CustomerRelationship.Relate(dealer, account).Save();

      var message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).ParentBssId(dealer.BSSID).RelationshipId(IdGen.StringId()).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      //Assert.AreEqual(1, result.ActivityResults.Count);
      StringAssert.Contains(result.Summary, string.Format("CustomerRelationship does not exist with RelationshipID: {0}.", message.RelationshipID));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_ExistingCustomerAndAccount_RelationshipIDNotDefined_Failure()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

      Entity.CustomerRelationship.Relate(customer, account).Save();

      var message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).ParentBssId(customer.BSSID).RelationshipId(IdGen.StringId()).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      //Assert.AreEqual(1, result.ActivityResults.Count);
      StringAssert.Contains(result.Summary, string.Format("CustomerRelationship does not exist with RelationshipID: {0}.", message.RelationshipID));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_ExistingDealerAndDealer_RelationshipIDNotDefined_Failure()
    {
      var parentDealer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

      var relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var message = BSS.AHDeleted.ForAccount().BssId(dealer.BSSID).ParentBssId(parentDealer.BSSID).RelationshipId(IdGen.StringId()).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      //Assert.AreEqual(1, result.ActivityResults.Count);
      StringAssert.Contains(result.Summary, string.Format("CustomerRelationship does not exist with RelationshipID: {0}.", message.RelationshipID));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_ExistingDealerAndAccount_ParentBSSIDNotDefined_Failure()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).ParentBssId(IdGen.StringId()).RelationshipId(relationship.BSSRelationshipID).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format("ParentDealer does not exist with BSSID: {0}", message.ParentBSSID));
      StringAssert.Contains(result.Summary, string.Format("ParentAccount does not exist with BSSID: {0}", message.BSSID));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_ExistingCustomerAndAccount_ParentBSSIDIDNotDefined_Failure()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).ParentBssId(IdGen.StringId()).RelationshipId(relationship.BSSRelationshipID).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format("ParentDealer does not exist with BSSID: {0}", message.ParentBSSID));
      StringAssert.Contains(result.Summary, string.Format("ParentAccount does not exist with BSSID: {0}", message.BSSID));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_ExistingDealerAndDealer_ParentBSSIDNotDefined_Failure()
    {
      var parentDealer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

      var relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var message = BSS.AHDeleted.ForDealer().BssId(dealer.BSSID).ParentBssId(IdGen.StringId()).RelationshipId(relationship.BSSRelationshipID).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format("ParentDealer does not exist with BSSID: {0}", message.ParentBSSID));
      StringAssert.Contains(result.Summary, string.Format("ParentDealer does not exist with BSSID: {0}", message.BSSID));
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_ExistingDealerAndAccount_ContactDefined_Failure()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).ParentBssId(IdGen.StringId()).RelationshipId(relationship.BSSRelationshipID).ContactDefined().Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.AreEqual(1, result.ActivityResults.Count);
      StringAssert.Contains(result.ActivityResults[0].Summary, "PrimaryContact is defined.");
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_ExistingCustomerAndAccount_ContactDefined_Failure()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).ParentBssId(IdGen.StringId()).RelationshipId(relationship.BSSRelationshipID).ContactDefined().Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.AreEqual(1, result.ActivityResults.Count);
      StringAssert.Contains(result.ActivityResults[0].Summary, "PrimaryContact is defined.");
    }

    [DatabaseTest]
    [TestMethod]
    public void AccountHierarchy_Delete_ExistingDealerAndDealer_ContactDefined_Failure()
    {
      var parentDealer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

      var relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var message = BSS.AHDeleted.ForAccount().BssId(dealer.BSSID).ParentBssId(IdGen.StringId()).RelationshipId(relationship.BSSRelationshipID).ContactDefined().Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      Assert.AreEqual(1, result.ActivityResults.Count);
      StringAssert.Contains(result.ActivityResults[0].Summary, "PrimaryContact is defined.");
    }

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_Delete_ExistingDealerAndAccount_ContactDefined_Failure()
    //{
    //  var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
    //  var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

    //  var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

    //  var message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).ParentBssId(dealer.BSSID).RelationshipId(relationship.BSSRelationshipID).ContactDefined().Build();
    //  WorkflowResult result = ExecuteWorkflow(message);
    //  Assert.IsTrue(result.Success);
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_Delete_ExistingCustomerAndAccount_ContactDefined_Failure()
    //{
    //  var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
    //  var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

    //  var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

    //  var message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).ParentBssId(customer.BSSID).RelationshipId(relationship.BSSRelationshipID).ContactDefined().Build();
    //  WorkflowResult result = ExecuteWorkflow(message);
    //  Assert.IsTrue(result.Success);
    //}

    //[DatabaseTest]
    //[TestMethod]
    //public void AccountHierarchy_Delete_ExistingDealerAndDealer_ContactDefined_Failure()
    //{
    //  var parentDealer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
    //  var dealer = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

    //  var relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

    //  var message = BSS.AHDeleted.ForAccount().BssId(dealer.BSSID).ParentBssId(parentDealer.BSSID).RelationshipId(relationship.BSSRelationshipID).ContactDefined().Build();
    //  WorkflowResult result = ExecuteWorkflow(message);
    //  Assert.IsTrue(result.Success);
    //}

    #region Service View Terminate on Relationship Deletion

    private WorkflowResult ExecuteWorkflow(AccountHierarchy message)
    {
      var workflow = new BssWorkflowFactory(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object)).Create(message);
      WorkflowResult result = new WorkflowRunner().Run(workflow);
      new ConsoleResultProcessor().Process(message, result);
      return result;
    }

    [TestMethod]
    [DatabaseTest]
    public void Account_Customer_Relationship_Delete()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();
      var device = Entity.Device.MTS522.OwnerBssId(account.BSSID).Save();
      Entity.Asset.WithDevice(device).WithCoreService().WithService(ServiceTypeEnum.CATHealth).Save();
      var message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).ParentBssId(customer.BSSID).RelationshipId(rel.BSSRelationshipID).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      var relationshipQuery =
                  (from r in Ctx.OpContext.CustomerRelationshipReadOnly
                   where r.fk_ParentCustomerID == customer.ID
                   select r).SingleOrDefault();

      var serviceViewQuery = (from c in Ctx.OpContext.CustomerReadOnly
                              join s in Ctx.OpContext.ServiceViewReadOnly
                                on c.ID equals s.fk_CustomerID
                              where c.ID == customer.ID
                              select s).ToList();
      Assert.IsNull(relationshipQuery);
      Assert.AreNotEqual(0, serviceViewQuery.Count());
      foreach (var item in serviceViewQuery)
      {
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), item.EndKeyDate);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void Account_Customer_Dealer_CustomerRelationship_Delete()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();
      
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(dealer, account).Save();
      
      var device = Entity.Device.MTS522.OwnerBssId(account.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      Entity.Service.Essentials.ForDevice(device)
        .WithView(x => x.ForAsset(asset).ForCustomer(customer))
        .WithView(x => x.ForAsset(asset).ForCustomer(dealer)).Save();

      Entity.Service.Health.WithView(x => x.ForAsset(asset).ForCustomer(dealer));

      //delete the relationship between account and customer
      var message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).ParentBssId(customer.BSSID).RelationshipId(rel.BSSRelationshipID).Build();
      
      var result = ExecuteWorkflow(message);
      
      Assert.IsTrue(result.Success);

      var ids = new List<long> { account.ID, customer.ID, dealer.ID };

      //var views = Ctx.OpContext.ServiceViewReadOnly.Where(x => x.fk_CustomerID == customer.ID).ToList();
      //Assert.IsFalse(views.All(x => x.EndKeyDate == DotNetExtensions.NullKeyDate));
      
      var query = (from c in Ctx.OpContext.CustomerReadOnly
                   from r in Ctx.OpContext.CustomerRelationshipReadOnly
                   from s in Ctx.OpContext.ServiceViewReadOnly
                   where ids.Contains(c.ID) && ids.Contains(r.fk_ParentCustomerID) && ids.Contains(s.fk_CustomerID)
                   select new { c, r, s }).ToList();

      var relationship = query.Where(t => t.r.fk_ParentCustomerID == customer.ID).Select(t => t.r).SingleOrDefault();
      Assert.IsNull(relationship);

      var svs = query.Where(t => t.s.fk_CustomerID == customer.ID).Select(t => t.s).Distinct().ToList();
      Assert.AreNotEqual(0, svs.Count());
      foreach (var item in svs)
      {
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), item.EndKeyDate);
      }

      svs = query.Where(t => t.s.fk_CustomerID == dealer.ID).Select(t => t.s).Distinct().ToList();
      Assert.AreNotEqual(0, svs.Count());
      foreach (var item in svs)
      {
        Assert.AreEqual(DotNetExtensions.NullKeyDate, item.EndKeyDate);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void Account_Customer_Dealer_DealerRelationship_Delete()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();
      var device = Entity.Device.MTS522.OwnerBssId(account.BSSID).Save();
      Entity.Asset.WithDevice(device).WithCoreService().WithService(ServiceTypeEnum.CATHealth).Save();

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();

      //create a relationship between account and dealer
      var message = BSS.AHUpdated.ForAccount().BssId(account.BSSID).DealerNetwork("CAT").ParentBssId(dealer.BSSID).RelationshipId(IdGen.StringId()).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      //delete the relationship between account and dealer
      message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).DealerNetwork("CAT").ParentBssId(dealer.BSSID).RelationshipId(rel.BSSRelationshipID).Build();
      result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      var ids = new List<long> { account.ID, customer.ID, dealer.ID };

      var query =
                  (from c in Ctx.OpContext.CustomerReadOnly
                   from r in Ctx.OpContext.CustomerRelationshipReadOnly
                   from s in Ctx.OpContext.ServiceViewReadOnly
                   where ids.Contains(c.ID) && ids.Contains(r.fk_ParentCustomerID) && ids.Contains(s.fk_CustomerID)
                   select new { c, r, s }).ToList();

      Assert.IsNotNull(query);

      var relationship = query.Where(t => t.r.fk_ParentCustomerID == dealer.ID).Select(t => t.r).SingleOrDefault();
      Assert.IsNull(relationship);

      var svs = query.Where(t => t.s.fk_CustomerID == customer.ID).Select(t => t.s).Distinct().ToList();
      Assert.IsNotNull(svs);
      Assert.AreNotEqual(0, svs.Count());
      foreach (var item in svs)
      {
        Assert.AreEqual(DotNetExtensions.NullKeyDate, item.EndKeyDate);
      }

      svs = query.Where(t => t.s.fk_CustomerID == dealer.ID).Select(t => t.s).Distinct().ToList();
      Assert.IsNotNull(svs);
      Assert.AreNotEqual(0, svs.Count());
      foreach (var item in svs)
      {
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), item.EndKeyDate);
      }
    }

    [Ignore]
    [TestMethod]
    [DatabaseTest]
    public void Account_Customer_Dealer_DealerAndCustomerRelationship_Delete()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();
      var device = Entity.Device.MTS522.OwnerBssId(account.BSSID).Save();
      Entity.Asset.WithDevice(device).WithCoreService().WithService(ServiceTypeEnum.CATHealth).Save();

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();

      //create a relationship between account and dealer
      var message = BSS.AHUpdated.ForAccount().BssId(account.BSSID).ParentBssId(dealer.BSSID).RelationshipId(IdGen.StringId()).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      //delete the relationship between account and dealer
      message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).ParentBssId(dealer.BSSID).RelationshipId(rel.BSSRelationshipID).Build();
      result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      //delete the relationship between account and customer
      message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).ParentBssId(customer.BSSID).RelationshipId(rel.BSSRelationshipID).Build();
      result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      var ids = new List<long> { account.ID, customer.ID, dealer.ID };

      var query = (from r in Ctx.OpContext.CustomerRelationshipReadOnly
                   where
                    ids.Contains(r.fk_ParentCustomerID)
                   select r).SingleOrDefault();

      Assert.IsNull(query);

      var svs = (from s in Ctx.OpContext.ServiceViewReadOnly
                 where ids.Contains(s.fk_CustomerID)
                 select s).ToList();

      Assert.IsNotNull(svs);
      var serviceViews = svs.Where(t => t.fk_CustomerID == customer.ID).Distinct().ToList();
      Assert.IsNotNull(svs);
      Assert.AreNotEqual(0, svs.Count());
      foreach (var item in serviceViews)
      {
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), item.EndKeyDate);
      }

      serviceViews = svs.Where(t => t.fk_CustomerID == dealer.ID).Distinct().ToList();
      Assert.IsNotNull(svs);
      Assert.AreNotEqual(0, svs.Count());
      foreach (var item in serviceViews)
      {
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), item.EndKeyDate);
      }
    }

    [Ignore]
    [TestMethod]
    [DatabaseTest]
    public void Account_Customer_Dealer_Dealer_ChildDealerRelationship_Delete()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();
      var device = Entity.Device.MTS522.OwnerBssId(account.BSSID).Save();
      Entity.Asset.WithDevice(device).WithCoreService().WithService(ServiceTypeEnum.CATHealth).Save();

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();

      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();

      //create a relationship between account and dealer
      var message = BSS.AHUpdated.ForAccount().BssId(account.BSSID).ParentBssId(dealer.BSSID).RelationshipId(IdGen.StringId()).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      //create a relationship between dealer and parent dealer
      message = BSS.AHUpdated.ForDealer().BssId(dealer.BSSID).ParentBssId(parentDealer.BSSID).RelationshipId(IdGen.StringId()).Build();
      result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      //delete the relationship between account and dealer
      message = BSS.AHDeleted.ForAccount().BssId(account.BSSID).ParentBssId(dealer.BSSID).RelationshipId(rel.BSSRelationshipID).Build();
      result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      var ids = new List<long> { account.ID, customer.ID, dealer.ID, parentDealer.ID };

      var query = (from r in Ctx.OpContext.CustomerRelationshipReadOnly
                   where ids.Contains(r.fk_ParentCustomerID)
                   select r).ToList();

      Assert.IsNotNull(query);

      var relationship = query.SingleOrDefault(t => t.fk_ParentCustomerID == dealer.ID);
      Assert.IsNull(relationship);

      relationship = query.Where(t => t.fk_ParentCustomerID == parentDealer.ID && t.fk_ClientCustomerID == dealer.ID).Distinct().SingleOrDefault();
      Assert.IsNotNull(relationship);

      relationship = query.SingleOrDefault(t => t.fk_ParentCustomerID == customer.ID);
      Assert.IsNotNull(relationship);

      var svs = (from s in Ctx.OpContext.ServiceViewReadOnly
                 where ids.Contains(s.fk_CustomerID)
                 select s).ToList();

      Assert.IsNotNull(svs);
      var serviceViews = svs.Where(t => t.fk_CustomerID == customer.ID).Distinct().ToList();
      Assert.IsNotNull(serviceViews);
      Assert.AreNotEqual(0, serviceViews.Count());
      foreach (var item in serviceViews)
      {
        Assert.AreEqual(DotNetExtensions.NullKeyDate, item.EndKeyDate);
      }

      serviceViews = svs.Where(t => t.fk_CustomerID == dealer.ID).Distinct().ToList();
      Assert.IsNotNull(serviceViews);
      Assert.AreNotEqual(0, serviceViews.Count());
      foreach (var item in serviceViews)
      {
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), item.EndKeyDate);
      }

      serviceViews = svs.Where(t => t.fk_CustomerID == parentDealer.ID).Distinct().ToList();
      Assert.IsNotNull(serviceViews);
      Assert.AreNotEqual(0, serviceViews.Count());
      foreach (var item in serviceViews)
      {
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), item.EndKeyDate);
      }
    }

    [Ignore]
    [TestMethod]
    [DatabaseTest]
    public void Account_Customer_Dealer_Dealer_Dealer_ParentDealerRelationship_Delete()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(customer, account).Save();
      var device = Entity.Device.MTS522.OwnerBssId(account.BSSID).Save();
      Entity.Asset.WithDevice(device).WithCoreService().WithService(ServiceTypeEnum.CATHealth).Save();

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();

      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();

      var grandParentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();

      //create a relationship between account and dealer
      var message = BSS.AHUpdated.ForAccount().BssId(account.BSSID).ParentBssId(dealer.BSSID).RelationshipId(IdGen.StringId()).Build();
      WorkflowResult result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      //create a relationship between dealer and parent dealer
      var relId = IdGen.StringId();
      message = BSS.AHUpdated.ForDealer().BssId(dealer.BSSID).ParentBssId(parentDealer.BSSID).RelationshipId(relId).Build();
      result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      //create a relationship between parent dealer and grand parent dealer
      message = BSS.AHUpdated.ForDealer().BssId(parentDealer.BSSID).ParentBssId(grandParentDealer.BSSID).RelationshipId(IdGen.StringId()).Build();
      result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      //delete the relationship between dealer and parent dealer
      message = BSS.AHDeleted.ForDealer().BssId(dealer.BSSID).ParentBssId(parentDealer.BSSID).RelationshipId(relId).Build();
      result = ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      var ids = new List<long> { account.ID, customer.ID, dealer.ID, parentDealer.ID, grandParentDealer.ID };

      var query = (from r in Ctx.OpContext.CustomerRelationshipReadOnly
                   where ids.Contains(r.fk_ParentCustomerID)
                   select r).ToList();

      Assert.IsNotNull(query);

      var relationship = query.SingleOrDefault(t => t.fk_ParentCustomerID == dealer.ID);
      Assert.IsNotNull(relationship);

      relationship = query.Where(t => t.fk_ParentCustomerID == parentDealer.ID).Distinct().SingleOrDefault();
      Assert.IsNull(relationship);

      relationship = query.Where(t => t.fk_ParentCustomerID == grandParentDealer.ID).Distinct().SingleOrDefault();
      Assert.IsNotNull(relationship);

      relationship = query.SingleOrDefault(t => t.fk_ParentCustomerID == customer.ID);
      Assert.IsNotNull(relationship);

      var svs = (from s in Ctx.OpContext.ServiceViewReadOnly
                 where ids.Contains(s.fk_CustomerID)
                 select s).ToList();

      Assert.IsNotNull(svs);
      var serviceViews = svs.Where(t => t.fk_CustomerID == customer.ID || t.fk_CustomerID == dealer.ID).Distinct().ToList();
      Assert.IsNotNull(serviceViews);
      Assert.AreNotEqual(0, serviceViews.Count());
      foreach (var item in serviceViews)
      {
        Assert.AreEqual(DotNetExtensions.NullKeyDate, item.EndKeyDate);
      }

      serviceViews = svs.Where(t => t.fk_CustomerID == parentDealer.ID || t.fk_CustomerID == grandParentDealer.ID).Distinct().ToList();
      Assert.IsNotNull(serviceViews);
      Assert.AreNotEqual(0, serviceViews.Count());
      foreach (var item in serviceViews)
      {
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), item.EndKeyDate);
      }
    }

    //Deleting relationship between D3 & D2 should not terminate the CAT Corp Service Views
    //Dealer3(SITECH)
    //        \X
    // Dealer2(CAT)
    //         \
    //  Dealer1(CAT)       Customer
    //           \       /
    //            Account
    //               |
    //            Asset/Device (VLCore)
    //
    [DatabaseTest]
    [TestMethod]
    public void CorpServiceViewNotTerminated_GrandParentDealerRelationDeleted_DifferentDealerNetwork()
    {
      var bssTestHelper = new BssTestHelper();

      //create customer
      var customer = BSS.AHCreated.ForCustomer().ContactDefined().BssId(IdGen.StringId()).Build();
      var result = bssTestHelper.ExecuteWorkflow(customer);
      Assert.IsTrue(result.Success);

      //create dealer
      var dealer = BSS.AHCreated.ForDealer().ContactDefined().BssId(IdGen.StringId()).DealerNetwork("CAT").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      //create account and associate dealer to it
      var account = BSS.AHCreated.ForAccount().BssId(IdGen.StringId()).ParentBssId(dealer.BSSID).Build();
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      //associate account and customer
      account.ParentBSSID = customer.BSSID;
      account.RelationshipID = IdGen.StringId();
      account.Action = ActionEnum.Updated.ToString();
      account.HierarchyType = "TCS Customer";
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      //create asset and device
      var asset =
        BSS.IBCreated.IBKey(IdGen.StringId()).PartNumber(bssTestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL121)).
          OwnerBssId(account.BSSID).Build();
      result = bssTestHelper.ExecuteWorkflow(asset);
      Assert.IsTrue(result.Success);

      //active core service plan for the asset/device
      var service =
        BSS.SPActivated.IBKey(asset.IBKey).OwnerVisibilityDate(DateTime.UtcNow).ServicePlanName(
          bssTestHelper.GetPartNumberByServiceType(ServiceTypeEnum.Essentials)).ServicePlanlineID(IdGen.StringId()).Build();
      result = bssTestHelper.ExecuteWorkflow(service);
      Assert.IsTrue(result.Success);

      //create another dealer
      var dealer1 = BSS.AHCreated.ForDealer().ContactDefined().BssId(IdGen.StringId()).DealerNetwork("CAT").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer1);
      Assert.IsTrue(result.Success);

      //make the dealer1 as parent to the dealer
      dealer.ParentBSSID = dealer1.BSSID;
      dealer.RelationshipID = IdGen.StringId();
      dealer.Action = ActionEnum.Updated.ToString();
      dealer.contact = null;
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      //create another dealer
      var dealer2 = BSS.AHCreated.ForDealer().ContactDefined().BssId(IdGen.StringId()).DealerNetwork("SITECH").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer2);
      Assert.IsTrue(result.Success);

      //make the dealer2 as parent to the dealer1
      dealer1.ParentBSSID = dealer2.BSSID;
      dealer1.RelationshipID = IdGen.StringId();
      dealer1.Action = ActionEnum.Updated.ToString();
      dealer1.contact = null;
      result = bssTestHelper.ExecuteWorkflow(dealer1);
      Assert.IsTrue(result.Success);

      //terminate the relationship between dealer2 and dealer1
      dealer1.Action = ActionEnum.Deleted.ToString();
      dealer1.contact = null;
      result = bssTestHelper.ExecuteWorkflow(dealer1);
      Assert.IsTrue(result.Success);

      var serviceViews = (from sv in Ctx.OpContext.ServiceViewReadOnly
                          join s in Ctx.OpContext.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                          join c in Ctx.OpContext.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                          where s.BSSLineID == service.ServicePlanlineID
                          select new { sv.EndKeyDate, c.ID, c.BSSID, c.fk_CustomerTypeID, c.fk_DealerNetworkID }).ToList();

      //SITECH corp service views should have been terminated
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //CAT corp service views should be active
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.EndKeyDate == DotNetExtensions.NullKeyDate));

      //service views should have been terminated for the SITECH dealer2
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer && t.BSSID == dealer2.BSSID && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //service views should be active for the CAT dealer1
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer && t.BSSID == dealer1.BSSID && t.EndKeyDate == DotNetExtensions.NullKeyDate));

      //service views should be active for the CAT dealer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer && t.BSSID == dealer.BSSID && t.EndKeyDate == DotNetExtensions.NullKeyDate));

      //service views should remain active for customer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.None &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer && t.EndKeyDate == DotNetExtensions.NullKeyDate));
    }

    //Deleting relationship between D1 & D2 should not terminate the CAT Corp Service Views
    //Dealer3(SITECH)
    //        \
    // Dealer2(CAT)
    //         \X
    //  Dealer1(CAT)       Customer
    //           \       /
    //            Account
    //               |
    //            Asset/Device (VLCore)
    //
    [DatabaseTest]
    [TestMethod]
    public void CorpServiceViewNotTerminated_GrandAndParentDealerRelationDeleted_DifferentDealerNetwork()
    {
      var bssTestHelper = new BssTestHelper();

      //create customer
      var customer = BSS.AHCreated.ForCustomer().ContactDefined().BssId(IdGen.StringId()).Build();
      var result = bssTestHelper.ExecuteWorkflow(customer);
      Assert.IsTrue(result.Success);

      //create dealer
      var dealer = BSS.AHCreated.ForDealer().ContactDefined().BssId(IdGen.StringId()).DealerNetwork("CAT").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      //create account and associate dealer to it
      var account = BSS.AHCreated.ForAccount().BssId(IdGen.StringId()).ParentBssId(dealer.BSSID).Build();
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      //associate account and customer
      account.ParentBSSID = customer.BSSID;
      account.RelationshipID = IdGen.StringId();
      account.Action = ActionEnum.Updated.ToString();
      account.HierarchyType = "TCS Customer";
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      //create asset and device
      var asset =
        BSS.IBCreated.IBKey(IdGen.StringId()).PartNumber(bssTestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL121)).
          OwnerBssId(account.BSSID).Build();
      result = bssTestHelper.ExecuteWorkflow(asset);
      Assert.IsTrue(result.Success);

      //active core service plan for the asset/device
      var service =
        BSS.SPActivated.IBKey(asset.IBKey).OwnerVisibilityDate(DateTime.UtcNow).ServicePlanName(
          bssTestHelper.GetPartNumberByServiceType(ServiceTypeEnum.Essentials)).ServicePlanlineID(IdGen.StringId()).Build();
      result = bssTestHelper.ExecuteWorkflow(service);
      Assert.IsTrue(result.Success);

      //create another dealer
      var dealer1 = BSS.AHCreated.ForDealer().ContactDefined().BssId(IdGen.StringId()).DealerNetwork("CAT").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer1);
      Assert.IsTrue(result.Success);

      //make the dealer1 as parent to the dealer
      dealer.ParentBSSID = dealer1.BSSID;
      dealer.RelationshipID = IdGen.StringId();
      dealer.Action = ActionEnum.Updated.ToString();
      dealer.contact = null;
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      //create another dealer
      var dealer2 = BSS.AHCreated.ForDealer().ContactDefined().BssId(IdGen.StringId()).DealerNetwork("SITECH").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer2);
      Assert.IsTrue(result.Success);

      //make the dealer2 as parent to the dealer1
      dealer1.ParentBSSID = dealer2.BSSID;
      dealer1.RelationshipID = IdGen.StringId();
      dealer1.Action = ActionEnum.Updated.ToString();
      dealer1.contact = null;
      result = bssTestHelper.ExecuteWorkflow(dealer1);
      Assert.IsTrue(result.Success);

      //terminate the relationship between dealer2 and dealer1
      dealer.Action = ActionEnum.Deleted.ToString();
      dealer.contact = null;
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      var serviceViews = (from sv in Ctx.OpContext.ServiceViewReadOnly
                          join s in Ctx.OpContext.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                          join c in Ctx.OpContext.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                          where s.BSSLineID == service.ServicePlanlineID
                          select new { sv.EndKeyDate, c.ID, c.BSSID, c.fk_CustomerTypeID, c.fk_DealerNetworkID }).ToList();

      //SITECH corp service views should have been terminated
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //CAT corp service views should be active
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.EndKeyDate == DotNetExtensions.NullKeyDate));

      //service views should have been terminated for the SITECH dealer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer && t.BSSID == dealer2.BSSID && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //service views should have been terminated for the CAT dealer1
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer && t.BSSID == dealer1.BSSID && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //service views should be active for the CAT dealer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer && t.BSSID == dealer.BSSID && t.EndKeyDate == DotNetExtensions.NullKeyDate));

      //service views should remain active for customer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.None &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer && t.EndKeyDate == DotNetExtensions.NullKeyDate));
    }

    //Deleting relationship between D2 & D1 should not terminate the CAT Corp Service Views
    //
    // Dealer2(CAT)
    //         \X
    //  Dealer1(CAT)       Customer
    //           \       /
    //            Account
    //               |
    //            Asset/Device (VLCore)
    //
    [DatabaseTest]
    [TestMethod]
    public void CorpServiceViewNotTerminated_ParentDealerRelationDeleted_SameDealerNetwork()
    {
      var bssTestHelper = new BssTestHelper();

      //create customer
      var customer = BSS.AHCreated.ForCustomer().ContactDefined().BssId(IdGen.StringId()).Build();
      var result = bssTestHelper.ExecuteWorkflow(customer);
      Assert.IsTrue(result.Success);

      //create dealer
      var dealer = BSS.AHCreated.ForDealer().ContactDefined().BssId(IdGen.StringId()).DealerNetwork("CAT").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      //create account and associate dealer to it
      var account = BSS.AHCreated.ForAccount().BssId(IdGen.StringId()).ParentBssId(dealer.BSSID).Build();
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      //associate account and customer
      account.ParentBSSID = customer.BSSID;
      account.RelationshipID = IdGen.StringId();
      account.Action = ActionEnum.Updated.ToString();
      account.HierarchyType = "TCS Customer";
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      //create asset and device
      var asset =
        BSS.IBCreated.IBKey(IdGen.StringId()).PartNumber(bssTestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL121)).
          OwnerBssId(account.BSSID).Build();
      result = bssTestHelper.ExecuteWorkflow(asset);
      Assert.IsTrue(result.Success);

      //active core service plan for the asset/device
      var service =
        BSS.SPActivated.IBKey(asset.IBKey).OwnerVisibilityDate(DateTime.UtcNow).ServicePlanName(
          bssTestHelper.GetPartNumberByServiceType(ServiceTypeEnum.Essentials)).ServicePlanlineID(IdGen.StringId()).Build();
      result = bssTestHelper.ExecuteWorkflow(service);
      Assert.IsTrue(result.Success);

      //create another dealer
      var dealer1 = BSS.AHCreated.ForDealer().ContactDefined().BssId(IdGen.StringId()).DealerNetwork("CAT").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer1);
      Assert.IsTrue(result.Success);

      //make the dealer1 as parent to the dealer
      dealer.ParentBSSID = dealer1.BSSID;
      dealer.RelationshipID = IdGen.StringId();
      dealer.Action = ActionEnum.Updated.ToString();
      dealer.contact = null;
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      //terminate the relationship between dealer and dealer1
      dealer.Action = ActionEnum.Deleted.ToString();
      dealer.contact = null;
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      var serviceViews = (from sv in Ctx.OpContext.ServiceViewReadOnly
                          join s in Ctx.OpContext.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                          join c in Ctx.OpContext.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                          where s.BSSLineID == service.ServicePlanlineID
                          select new { sv.EndKeyDate, c.ID, c.BSSID, c.fk_CustomerTypeID, c.fk_DealerNetworkID }).ToList();

      //CAT corp service views should be active
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.EndKeyDate == DotNetExtensions.NullKeyDate));

      //service views should have been terminated for the CAT parent dealer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer && t.BSSID == dealer1.BSSID && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //service views should have been terminated for the CAT dealer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer && t.BSSID == dealer.BSSID && t.EndKeyDate == DotNetExtensions.NullKeyDate));

      //service views should remain active for customer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.None &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer && t.EndKeyDate == DotNetExtensions.NullKeyDate));
    }

    //Deleting relationship between Account & D1 should not terminate the CAT Corp Service Views
    //
    // Dealer2(CAT)
    //         \
    //  Dealer1(CAT)       Customer
    //           \X       /
    //            Account
    //               |
    //            Asset/Device (VLCore)
    //
    [DatabaseTest]
    [TestMethod]
    public void CorpServiceViewNotTerminated_DealerRelationDeleted_SameDealerNetwork()
    {
      var bssTestHelper = new BssTestHelper();

      //create customer
      var customer = BSS.AHCreated.ForCustomer().ContactDefined().BssId(IdGen.StringId()).Name("Customer").Build();
      var result = bssTestHelper.ExecuteWorkflow(customer);
      Assert.IsTrue(result.Success);

      //create dealer
      var dealer = BSS.AHCreated.ForDealer().ContactDefined().BssId(IdGen.StringId()).DealerNetwork("CAT").Name("Dealer").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      //create account and associate dealer to it

      var account = BSS.AHCreated.ForAccount().BssId(IdGen.StringId()).ParentBssId(dealer.BSSID).Name("Account").Build();
      var relId = account.RelationshipID;
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      //associate account and customer
      account.ParentBSSID = customer.BSSID;
      account.RelationshipID = IdGen.StringId();
      account.Action = ActionEnum.Updated.ToString();
      account.HierarchyType = "TCS Customer";
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      //create asset and device
      var asset =
        BSS.IBCreated.IBKey(IdGen.StringId()).PartNumber(bssTestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL121)).
          OwnerBssId(account.BSSID).Build();
      result = bssTestHelper.ExecuteWorkflow(asset);
      Assert.IsTrue(result.Success);

      //active core service plan for the asset/device
      var service =
        BSS.SPActivated.IBKey(asset.IBKey).OwnerVisibilityDate(DateTime.UtcNow).ServicePlanName(
          bssTestHelper.GetPartNumberByServiceType(ServiceTypeEnum.Essentials)).ServicePlanlineID(IdGen.StringId()).Build();
      result = bssTestHelper.ExecuteWorkflow(service);
      Assert.IsTrue(result.Success);

      //create another dealer
      var dealer1 = BSS.AHCreated.ForDealer().ContactDefined().BssId(IdGen.StringId()).DealerNetwork("CAT").Name("ParentDealer").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer1);
      Assert.IsTrue(result.Success);

      //make the dealer1 as parent to the dealer
      dealer.ParentBSSID = dealer1.BSSID;
      dealer.RelationshipID = IdGen.StringId();
      dealer.Action = ActionEnum.Updated.ToString();
      dealer.contact = null;
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      //terminate the relationship between dealer and dealer1
      account.ParentBSSID = dealer.BSSID;
      account.RelationshipID = relId;
      account.HierarchyType = "TCS Dealer";
      account.Action = ActionEnum.Deleted.ToString();
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      var serviceViews = (from sv in Ctx.OpContext.ServiceViewReadOnly
                          join s in Ctx.OpContext.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                          join c in Ctx.OpContext.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                          where s.BSSLineID == service.ServicePlanlineID
                          select new { sv.EndKeyDate, c.ID, c.BSSID, c.fk_CustomerTypeID, c.fk_DealerNetworkID }).ToList();

      //CAT corp service views should have been terminated
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //service views should have been terminated for the CAT dealer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //service views should remain active for customer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.None &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer && t.EndKeyDate == DotNetExtensions.NullKeyDate));
    }


    //Deleting relationship between Account & D1 should not terminate the CAT Corp Service Views
    //
    // Dealer2(CAT)
    //         \
    //  Dealer1(TRMB)       Customer
    //           \X       /
    //            Account
    //               |
    //            Asset/Device (VLCore)
    //
    [DatabaseTest]
    [TestMethod]
    public void CorpServiceViewNotTerminated_DealerRelationDeleted_DifferentDealerNetwork()
    {
      var bssTestHelper = new BssTestHelper();

      //create customer
      var customer = BSS.AHCreated.ForCustomer().ContactDefined().BssId(IdGen.StringId()).Name("Customer").Build();
      var result = bssTestHelper.ExecuteWorkflow(customer);
      Assert.IsTrue(result.Success);

      //create dealer
      var dealer = BSS.AHCreated.ForDealer().ContactDefined().BssId(IdGen.StringId()).DealerNetwork("CAT").Name("Dealer").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      //create account and associate dealer to it

      var account = BSS.AHCreated.ForAccount().BssId(IdGen.StringId()).ParentBssId(dealer.BSSID).Name("Account").Build();
      var relId = account.RelationshipID;
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      //associate account and customer
      account.ParentBSSID = customer.BSSID;
      account.RelationshipID = IdGen.StringId();
      account.Action = ActionEnum.Updated.ToString();
      account.HierarchyType = "TCS Customer";
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      //create asset and device
      var asset =
        BSS.IBCreated.IBKey(IdGen.StringId()).PartNumber(bssTestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL121)).
          OwnerBssId(account.BSSID).Build();
      result = bssTestHelper.ExecuteWorkflow(asset);
      Assert.IsTrue(result.Success);

      //active core service plan for the asset/device
      var service =
        BSS.SPActivated.IBKey(asset.IBKey).OwnerVisibilityDate(DateTime.UtcNow).ServicePlanName(
          bssTestHelper.GetPartNumberByServiceType(ServiceTypeEnum.Essentials)).ServicePlanlineID(IdGen.StringId()).Build();
      result = bssTestHelper.ExecuteWorkflow(service);
      Assert.IsTrue(result.Success);

      //create another dealer
      var dealer1 = BSS.AHCreated.ForDealer().BssId(IdGen.StringId()).ContactDefined().DealerNetwork("SITECH").Name("ParentDealer").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer1);
      Assert.IsTrue(result.Success);

      //make the dealer1 as parent to the dealer
      dealer.ParentBSSID = dealer1.BSSID;
      dealer.RelationshipID = IdGen.StringId();
      dealer.Action = ActionEnum.Updated.ToString();
      dealer.contact = null;
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      //terminate the relationship between dealer and dealer1
      account.ParentBSSID = dealer.BSSID;
      account.RelationshipID = relId;
      account.HierarchyType = "TCS Dealer";
      account.Action = ActionEnum.Deleted.ToString();
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      var serviceViews = (from sv in Ctx.OpContext.ServiceViewReadOnly
                          join s in Ctx.OpContext.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                          join c in Ctx.OpContext.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                          where s.BSSLineID == service.ServicePlanlineID
                          select new { sv.EndKeyDate, c.ID, c.BSSID, c.fk_CustomerTypeID, c.fk_DealerNetworkID }).ToList();

      //SITECH corp service views should have been terminated
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //CAT corp service views should have been terminated
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //service views should have been terminated for the CAT dealer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //service views should have been terminated for the SITECH dealer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //service views should remain active for customer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.None &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer && t.EndKeyDate == DotNetExtensions.NullKeyDate));
    }

    //Deleting relationship between D2 & D1 should not terminate the CAT Corp Service Views
    //
    // Dealer2(SITECH)
    //         \X
    //  Dealer1(CAT)       Customer
    //           \       /
    //            Account
    //               |
    //            Asset/Device (VLCore)
    //
    [DatabaseTest]
    [TestMethod]
    public void CorpServiceViewNotTerminated_ParentDealerRelationDeleted_DifferentDealerNetwork()
    {
      var bssTestHelper = new BssTestHelper();

      //create customer
      var customer = BSS.AHCreated.ForCustomer().ContactDefined().BssId(IdGen.StringId()).Build();
      var result = bssTestHelper.ExecuteWorkflow(customer);
      Assert.IsTrue(result.Success);

      //create dealer
      var dealer = BSS.AHCreated.ForDealer().ContactDefined().BssId(IdGen.StringId()).DealerNetwork("CAT").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      //create account and associate dealer to it
      var account = BSS.AHCreated.ForAccount().BssId(IdGen.StringId()).ParentBssId(dealer.BSSID).Build();
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      //associate account and customer
      account.ParentBSSID = customer.BSSID;
      account.RelationshipID = IdGen.StringId();
      account.Action = ActionEnum.Updated.ToString();
      account.HierarchyType = "TCS Customer";
      result = bssTestHelper.ExecuteWorkflow(account);
      Assert.IsTrue(result.Success);

      //create asset and device
      var asset =
        BSS.IBCreated.IBKey(IdGen.StringId()).PartNumber(bssTestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL121)).
          OwnerBssId(account.BSSID).Build();
      result = bssTestHelper.ExecuteWorkflow(asset);
      Assert.IsTrue(result.Success);

      //active core service plan for the asset/device
      var service =
        BSS.SPActivated.IBKey(asset.IBKey).OwnerVisibilityDate(DateTime.UtcNow).ServicePlanName(
          bssTestHelper.GetPartNumberByServiceType(ServiceTypeEnum.Essentials)).ServicePlanlineID(IdGen.StringId()).Build();
      result = bssTestHelper.ExecuteWorkflow(service);
      Assert.IsTrue(result.Success);

      //create another dealer
      var dealer1 = BSS.AHCreated.ForDealer().ContactDefined().BssId(IdGen.StringId()).DealerNetwork("SITECH").Build();
      result = bssTestHelper.ExecuteWorkflow(dealer1);
      Assert.IsTrue(result.Success);

      //make the dealer1 as parent to the dealer
      dealer.ParentBSSID = dealer1.BSSID;
      dealer.RelationshipID = IdGen.StringId();
      dealer.Action = ActionEnum.Updated.ToString();
      dealer.contact = null;
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      //terminate the relationship between dealer and dealer1
      dealer.Action = ActionEnum.Deleted.ToString();
      dealer.contact = null;
      result = bssTestHelper.ExecuteWorkflow(dealer);
      Assert.IsTrue(result.Success);

      var serviceViews = (from sv in Ctx.OpContext.ServiceViewReadOnly
                          join s in Ctx.OpContext.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                          join c in Ctx.OpContext.CustomerReadOnly on sv.fk_CustomerID equals c.ID
                          where s.BSSLineID == service.ServicePlanlineID
                          select new { sv.EndKeyDate, c.ID, c.BSSID, c.fk_CustomerTypeID, c.fk_DealerNetworkID }).ToList();

      //SITECH corp service views should have been terminated
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //CAT corp service views should be active
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.EndKeyDate == DotNetExtensions.NullKeyDate));

      //Service views for CAT dealer(s) should be active
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer && t.EndKeyDate == DotNetExtensions.NullKeyDate));

      //service views should have been terminated for the SITECH dealer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer && t.EndKeyDate == DateTime.UtcNow.KeyDate()));

      //service views should remain active for customer
      Assert.IsTrue(
        serviceViews.Any(
          t => t.fk_DealerNetworkID == (int)DealerNetworkEnum.None &&
               t.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer && t.EndKeyDate == DotNetExtensions.NullKeyDate));
    }

    #endregion
  }
}
