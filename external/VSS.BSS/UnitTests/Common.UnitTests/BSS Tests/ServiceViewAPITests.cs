using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ServiceViewAPITests : UnitTestBase
  {
    private ServiceViewAPITestHelper _helper;
    public ServiceViewAPITestHelper Helper { get { return _helper ?? (_helper = new ServiceViewAPITestHelper()); } }

    #region Relationship Creation Tests

    #region Dealer to Dealer

    [DatabaseTest]
    [TestMethod]
    public void CreateRelationship_CatDealerToCatParentDealer_AccountDeviceWithCore_DealerDeviceWithCore_AndTestNG()
    {
      Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_CatParentDealer();

      DateTime activationDate = DateTime.UtcNow.AddDays(-30);
      DateTime ownerVisibilityDate = activationDate;
      DateTime historicalActivation = activationDate.AddMonths(-13);
      Service pl321Essentials = Helper.SetupPl321WithService(Helper.Account, ServiceTypeEnum.Essentials, ownerVisibilityDate)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Dealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation)).Save();

      Service s522Essentials = Helper.SetupSeries522WithService(Helper.Dealer, ServiceTypeEnum.Essentials)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.Dealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation)).Save();

      var mockCustomerService = new Mock<ICustomerService>();
      mockCustomerService.Setup(x => x.AssociateCustomerAsset(It.IsAny<AssociateCustomerAssetEvent>())).Returns(true);
     
     
      var serviceViewApi = new ServiceViewAPI(mockCustomerService.Object);
      var createdViews = serviceViewApi.CreateRelationshipServiceViews(Helper.ParentDealer.ID, Helper.Dealer.ID);

      Assert.AreEqual(2, createdViews.Count, "There should be 2 created ServiceViews.");
      mockCustomerService.Verify(x => x.AssociateCustomerAsset(It.IsAny<AssociateCustomerAssetEvent>()), Times.Exactly(2));
      IList<ServiceView> parentDealerViews = Helper.GetServiceViewsForCustomer(Helper.ParentDealer.ID);

      Helper.AssertServiceViewIsCreated(parentDealerViews, pl321Essentials, historicalActivation.KeyDate(), "pl321Essentials related view for ParentDealer");
      Helper.AssertServiceViewIsCreated(parentDealerViews, s522Essentials, historicalActivation.KeyDate(), "s522Essentials related view for ParentDealer");
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateRelationship_CatDealerToSitechParentDealer_AccountDeviceWithCore_DealerDeviceWithCore()
    {
      Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_SitechParentDealer();

      DateTime activationDate = DateTime.UtcNow.AddDays(-30);
      DateTime ownerVisibilityDate = activationDate;
      DateTime historicalActivation = activationDate.AddMonths(-13);
      Service pl321Essentials = Helper.SetupPl321WithService(Helper.Account, ServiceTypeEnum.Essentials, ownerVisibilityDate)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Dealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation)).Save();

      Service s522Essentials = Helper.SetupSeries522WithService(Helper.Dealer, ServiceTypeEnum.Essentials)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.Dealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation)).Save();

      var createdViews = API.ServiceView.CreateRelationshipServiceViews(Helper.ParentDealer.ID, Helper.Dealer.ID);

      Assert.AreEqual(4, createdViews.Count, "There should be 4 created ServiceViews.");

      IList<ServiceView> parentDealerViews = Helper.GetServiceViewsForCustomer(Helper.ParentDealer.ID);
      Helper.AssertServiceViewIsCreated(parentDealerViews, pl321Essentials, historicalActivation.KeyDate(), "pl321Essentials related view for ParentDealer");
      Helper.AssertServiceViewIsCreated(parentDealerViews, s522Essentials, historicalActivation.KeyDate(), "s522Essentials related view for ParentDealer");

      IList<ServiceView> sitechCorpViews = Helper.GetServiceViewsForCustomer(Helper.SitechCorp.ID);
      Helper.AssertServiceViewIsCreated(sitechCorpViews, pl321Essentials, historicalActivation.KeyDate(), "pl321Essentials related view for SitechCorp");
      Helper.AssertServiceViewIsCreated(sitechCorpViews, s522Essentials, historicalActivation.KeyDate(), "s522Essentials related view for SitechCorp");
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateRelationship_CatDealerToSitechParentDealer_AccountDeviceWithCancelledCore_DealerDeviceWithCore()
    {
      Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_SitechParentDealer();

      DateTime activationDate = DateTime.UtcNow.AddDays(-30);
      DateTime ownerVisibilityDate = activationDate;
      DateTime cancellationDate = DateTime.UtcNow.AddDays(-2);
      DateTime historicalActivation = activationDate.AddMonths(-13);
      Service pl321Essentials = Helper.SetupPl321WithService(Helper.Account, ServiceTypeEnum.Essentials, ownerVisibilityDate)
        .ActivationDate(activationDate).CancellationDate(cancellationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Dealer).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation).EndsOn(cancellationDate)).Save();

      Service s522Essentials = Helper.SetupSeries522WithService(Helper.Dealer, ServiceTypeEnum.Essentials)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.Dealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation)).Save();

      var createdViews = API.ServiceView.CreateRelationshipServiceViews(Helper.ParentDealer.ID, Helper.Dealer.ID);

      Assert.AreEqual(2, createdViews.Count, "There should be 2 created ServiceViews.");

      IList<ServiceView> parentDealerViews = Helper.GetServiceViewsForCustomer(Helper.ParentDealer.ID);
      Helper.AssertServiceViewIsCreated(parentDealerViews, s522Essentials, historicalActivation.KeyDate(), "s522Essentials related view for ParentDealer");
      Helper.AssertServiceViewWasNotCreated(parentDealerViews, pl321Essentials.ID, "pl321Essentials should not have been created for ParentDealer.");

      IList<ServiceView> sitechCorpViews = Helper.GetServiceViewsForCustomer(Helper.SitechCorp.ID);
      Helper.AssertServiceViewIsCreated(sitechCorpViews, s522Essentials, historicalActivation.KeyDate(), "s522Essentials related view for SitechCorp");
      Helper.AssertServiceViewWasNotCreated(sitechCorpViews, pl321Essentials.ID, "pl321Essentials should not have been created for SitechCorp.");
    }

   
    #endregion

    #region Dealer to Account

    [DatabaseTest]
    [TestMethod]
    public void CreateRelationship_CatDealerToAccount_AccountDevice_ActiveEssentials_CancelledHealth()
    {
      Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_CatParentDealer();

      DateTime activationDate = DateTime.UtcNow.AddDays(-30);
      DateTime ownerVisibilityDate = activationDate;
      DateTime cancellationDate = DateTime.UtcNow.AddDays(-2);
      DateTime historicalActivation = activationDate.AddMonths(-13);

      Service pl321Essentials = Helper.SetupPl321WithService(Helper.Account, ServiceTypeEnum.Essentials, ownerVisibilityDate)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate))
        .Save();

      Service pl321Health = Entity.Service.Health.ForDevice(Helper.Pl321)
        .ActivationDate(activationDate).CancellationDate(cancellationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .Save();

      var createdViews = API.ServiceView.CreateRelationshipServiceViews(Helper.Dealer.ID, Helper.Account.ID);

      Assert.AreEqual(3, createdViews.Count, "There should be 3 created ServiceViews.");

      IList<ServiceView> dealerViews = Helper.GetServiceViewsForCustomer(Helper.Dealer.ID);
      Helper.AssertServiceViewIsCreated(dealerViews, pl321Essentials, historicalActivation.KeyDate(), "pl321Essentials related view for Dealer");
      Helper.AssertServiceViewWasNotCreated(dealerViews, pl321Health.ID, "pl321Health should not be created for Dealer.");

      IList<ServiceView> parentDealerViews = Helper.GetServiceViewsForCustomer(Helper.ParentDealer.ID);
      Helper.AssertServiceViewIsCreated(parentDealerViews, pl321Essentials, historicalActivation.KeyDate(), "pl321Essentials related view for ParentDealer");
      Helper.AssertServiceViewWasNotCreated(parentDealerViews, pl321Health.ID, "pl321Health should not be created for ParentDealer.");

      IList<ServiceView> catCorpViews = Helper.GetServiceViewsForCustomer(Helper.CatCorp.ID);
      Helper.AssertServiceViewIsCreated(catCorpViews, pl321Essentials, historicalActivation.KeyDate(), "pl321Essentials related view for CatCorp");
      Helper.AssertServiceViewWasNotCreated(catCorpViews, pl321Health.ID, "pl321Health should not be created for CatCorp.");
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateRelationship_CatDealerWithSitechParentToAccount_AccountDevice_ActiveEssentials_CancelledHealth()
    {
      Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_SitechParentDealer();

      DateTime activationDate = DateTime.UtcNow.AddDays(-30);
      DateTime ownerVisibilityDate = activationDate;
      DateTime cancellationDate = DateTime.UtcNow.AddDays(-2);
      DateTime historicalActivation = activationDate.AddMonths(-13);

      Service pl321Essentials = Helper.SetupPl321WithService(Helper.Account, ServiceTypeEnum.Essentials, ownerVisibilityDate)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate))
        .Save();

      Service pl321Health = Entity.Service.Health.ForDevice(Helper.Pl321)
        .ActivationDate(activationDate).CancellationDate(cancellationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .Save();

      var createdViews = API.ServiceView.CreateRelationshipServiceViews(Helper.Dealer.ID, Helper.Account.ID);

      Assert.AreEqual(4, createdViews.Count, "There should be 3 created ServiceViews.");

      IList<ServiceView> dealerViews = Helper.GetServiceViewsForCustomer(Helper.Dealer.ID);
      Helper.AssertServiceViewIsCreated(dealerViews, pl321Essentials, historicalActivation.KeyDate(), "pl321Essentials related view for Dealer");
      Helper.AssertServiceViewWasNotCreated(dealerViews, pl321Health.ID, "pl321Health should not be created for Dealer.");

      IList<ServiceView> parentDealerViews = Helper.GetServiceViewsForCustomer(Helper.ParentDealer.ID);
      Helper.AssertServiceViewIsCreated(parentDealerViews, pl321Essentials, historicalActivation.KeyDate(), "pl321Essentials related view for ParentDealer");
      Helper.AssertServiceViewWasNotCreated(parentDealerViews, pl321Health.ID, "pl321Health should not be created for ParentDealer.");

      IList<ServiceView> catCorpViews = Helper.GetServiceViewsForCustomer(Helper.CatCorp.ID);
      Helper.AssertServiceViewIsCreated(catCorpViews, pl321Essentials, historicalActivation.KeyDate(), "pl321Essentials related view for CatCorp");
      Helper.AssertServiceViewWasNotCreated(catCorpViews, pl321Health.ID, "pl321Health should not be created for CatCorp.");

      IList<ServiceView> sitechCorpViews = Helper.GetServiceViewsForCustomer(Helper.SitechCorp.ID);
      Helper.AssertServiceViewIsCreated(sitechCorpViews, pl321Essentials, historicalActivation.KeyDate(), "pl321Essentials related view for SitechCorp");
      Helper.AssertServiceViewWasNotCreated(sitechCorpViews, pl321Health.ID, "pl321Health should not be created for SitechCorp.");
    }


    [DatabaseTest]
    [TestMethod]
    public void CreateRelationship_CatDealerToAccount_WithPriorCatDealer_ConfirmCorporateViews()
    {


      
      Customer dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      Customer dealer2 = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();

      Customer customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();

      Customer account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(customer, account).Save();


      DateTime activationDate = DateTime.UtcNow.AddDays(-30);
      DateTime ownerVisibilityDate = activationDate;
      DateTime cancellationDate = DateTime.UtcNow.AddDays(-10);
      DateTime historicalActivation = activationDate.AddMonths(-13);

      Service pl321Essentials = Helper.SetupPl321WithService(account, ServiceTypeEnum.Essentials, ownerVisibilityDate)
        .ActivationDate(DateTime.UtcNow.AddDays(-30))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(customer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(dealer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.GetCorporateCustomer(DealerNetworkEnum.CAT)).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .Save();

      // Relate to new dealer
      Entity.CustomerRelationship.Relate(dealer2, account);

      IList<ServiceView> createdViews = API.ServiceView.CreateRelationshipServiceViews(dealer2.ID, account.ID);

      IList<ServiceView> corpViews = Helper.GetServiceViewsForCustomer(Helper.GetCorporateCustomer(DealerNetworkEnum.CAT).ID);

      Assert.AreEqual(2, corpViews.Count, "Should have 2 views");
      // Max StartDate == Min EndDate => Independent of sort order returned from db.
      Assert.AreEqual(corpViews.Select(x => x.StartKeyDate).Max(), corpViews.Select(x => x.EndKeyDate).Min(), "Views should be adjacent"); 


    }

    #endregion

    #region Customer to Account

    [DatabaseTest]
    [TestMethod]
    public void CreateRelationship_CustomerToAccount_AccountDevice_ActiveEssentialsWithVisibility_CancelledHealth()
    {
      Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_CatParentDealer();

      DateTime activationDate = DateTime.UtcNow.AddDays(-30);
      DateTime ownerVisibilityDate = activationDate;
      DateTime cancellationDate = DateTime.UtcNow.AddDays(-2);
      DateTime historicalActivation = activationDate.AddMonths(-13);

      Service pl321Essentials = Helper.SetupPl321WithService(Helper.Account, ServiceTypeEnum.Essentials, ownerVisibilityDate)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Dealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentDealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation))
        .Save();

      Service pl321Health = Entity.Service.Health.ForDevice(Helper.Pl321)
        .ActivationDate(activationDate).CancellationDate(cancellationDate).OwnerVisibilityDate(ownerVisibilityDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Dealer).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentDealer).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .Save();

      var createdViews = API.ServiceView.CreateRelationshipServiceViews(Helper.Customer.ID, Helper.Account.ID);

      Assert.AreEqual(2, createdViews.Count, "There should be 2 created ServiceViews.");

      IList<ServiceView> customerViews = Helper.GetServiceViewsForCustomer(Helper.Customer.ID);
      Helper.AssertServiceViewIsCreated(customerViews, pl321Essentials, ownerVisibilityDate.KeyDate(), "pl321Essentials related view for Customer");
      Helper.AssertServiceViewWasNotCreated(customerViews, pl321Health.ID, "pl321Health should not be created for Customer.");

      IList<ServiceView> parentCustomerViews = Helper.GetServiceViewsForCustomer(Helper.ParentCustomer.ID);
      Helper.AssertServiceViewIsCreated(parentCustomerViews, pl321Essentials, ownerVisibilityDate.KeyDate(), "pl321Essentials related view for ParentCustomer");
      Helper.AssertServiceViewWasNotCreated(parentCustomerViews, pl321Health.ID, "pl321Health should not be created for ParentCustomer.");
    }

    #endregion


    // Customer to Customer needed

    #endregion

    #region Relationship Termination Tests

    #region Dealer to Dealer

    [DatabaseTest]
    [TestMethod]
    public void TerminateRelationship_CatDealerToSitechParentDealer_AccountDeviceWithCore_DealerDeviceWithCore()
    {
      Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_SitechParentDealer();

      DateTime activationDate = DateTime.UtcNow.AddDays(-30);
      DateTime ownerVisibilityDate = activationDate;
      DateTime historicalActivation = activationDate.AddMonths(-13);
      Service pl321Essentials = Helper.SetupPl321WithService(Helper.Account, ServiceTypeEnum.Essentials, ownerVisibilityDate)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Dealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentDealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.SitechCorp).StartsOn(historicalActivation)).Save();

      Service s522Essentials = Helper.SetupSeries522WithService(Helper.Dealer, ServiceTypeEnum.Essentials)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.Dealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.ParentDealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.SitechCorp).StartsOn(historicalActivation)).Save();

      DateTime terminationDate = DateTime.UtcNow.AddDays(-2);
      var terminatedViews = API.ServiceView.TerminateRelationshipServiceViews(Helper.ParentDealer.ID, Helper.Dealer.ID, terminationDate);

      Assert.AreEqual(4, terminatedViews.Count, "There should be 4 terminated ServiceViews.");

      var parentDealerViews = Helper.GetServiceViewsForCustomer(Helper.ParentDealer.ID);
      Helper.AssertServiceViewIsTerminated(parentDealerViews, pl321Essentials.ID, terminationDate.KeyDate(), "pl321Essentials should be terminated for ParentDealer");
      Helper.AssertServiceViewIsTerminated(parentDealerViews, s522Essentials.ID, terminationDate.KeyDate(), "s522Essentials should be terminated for ParentDealer");

      var sitechCorpViews = Helper.GetServiceViewsForCustomer(Helper.SitechCorp.ID);
      Helper.AssertServiceViewIsTerminated(sitechCorpViews, pl321Essentials.ID, terminationDate.KeyDate(), "pl321Essentials should be terminated for SitechCorp");
      Helper.AssertServiceViewIsTerminated(sitechCorpViews, s522Essentials.ID, terminationDate.KeyDate(), "s522Essentials should be terminated for SitechCorp");
    }

    [DatabaseTest]
    [TestMethod]
    public void TerminateRelationship_CatDealerToSitechParentDealer_AccountDeviceWithCancelledCore_DealerDeviceWithCore()
    {
      Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_SitechParentDealer();

      DateTime activationDate = DateTime.UtcNow.AddDays(-30);
      DateTime ownerVisibilityDate = activationDate;
      DateTime cancellationDate = DateTime.UtcNow.AddDays(-2);
      DateTime historicalActivation = activationDate.AddMonths(-13);
      Service pl321Essentials = Helper.SetupPl321WithService(Helper.Account, ServiceTypeEnum.Essentials, ownerVisibilityDate)
        .ActivationDate(activationDate).CancellationDate(cancellationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Dealer).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentDealer).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.SitechCorp).StartsOn(historicalActivation).EndsOn(cancellationDate)).Save();

      Service s522Essentials = Helper.SetupSeries522WithService(Helper.Dealer, ServiceTypeEnum.Essentials)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.Dealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.ParentDealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetSeries522).ForCustomer(Helper.SitechCorp).StartsOn(historicalActivation)).Save();

      DateTime terminationDate = DateTime.UtcNow.AddDays(-2);
      var terminatedViews = API.ServiceView.TerminateRelationshipServiceViews(Helper.ParentDealer.ID, Helper.Dealer.ID, terminationDate);

      Assert.AreEqual(2, terminatedViews.Count, "There should be 2 terminated ServiceViews.");

      var parentDealerViews = Helper.GetServiceViewsForCustomer(Helper.ParentDealer.ID);
      Helper.AssertServiceViewIsTerminated(parentDealerViews, pl321Essentials.ID, terminationDate.KeyDate(), "pl321Essentials should be terminated for ParentDealer");
      Helper.AssertServiceViewIsUnchanged(parentDealerViews, s522Essentials.ID, terminationDate.KeyDate(), "s522Essentials should be unchanged for ParentDealer");

      var sitechCorpViews = Helper.GetServiceViewsForCustomer(Helper.SitechCorp.ID);
      Helper.AssertServiceViewIsTerminated(sitechCorpViews, pl321Essentials.ID, terminationDate.KeyDate(), "pl321Essentials should be terminated for SitechCorp");
      Helper.AssertServiceViewIsUnchanged(sitechCorpViews, s522Essentials.ID, terminationDate.KeyDate(), "s522Essentials should be unchanged for SitechCorp");
    }

    [DatabaseTest]
    [TestMethod]
    public void TerminateRelationship_DealerWithSitechParent_DealerNetworkIsNotSpecifiedForDealer()
    {
      // Bug 25226
      ServiceViewAPI target = new ServiceViewAPI();

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.None).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      DateTime currentDateTime = DateTime.UtcNow.AddDays(-2);

      Service service = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "", currentDateTime, ServiceTypeEnum.Essentials);

      List<ServiceView> actual = (Ctx.OpContext.ServiceViewReadOnly.Where(s => s.fk_ServiceID == service.ID)).ToList();
      Assert.IsNotNull(actual, "ServiceView was not saved successfully.");
      Assert.AreEqual(3, actual.Count, "Incorrect number of service views returned.");
      Assert.AreEqual(1, actual.Count(x => x.fk_CustomerID == dealer.ID), "Did not find a service view for the dealer.");
      Assert.AreEqual(1, actual.Count(x => x.fk_CustomerID == parentDealer.ID), "Did not find a service view for the parent dealer.");
      Assert.AreEqual(3, actual.Count(x => x.fk_AssetID == asset.AssetID), "ServiceView AssetIDs do not match.");

      DateTime terminationDate = DateTime.UtcNow;
      var terminatedViews = API.ServiceView.TerminateRelationshipServiceViews(parentDealer.ID, dealer.ID, terminationDate);

      Assert.AreEqual(1, terminatedViews.Count, "There should be 1 terminated ServiceView.");
      Assert.AreEqual(parentDealer.ID, terminatedViews.First().fk_CustomerID, "Wrong service view terminated.");
    }

    [DatabaseTest]
    [TestMethod]
    public void TerminateRelationship_SitechDealerWithSitechParent_DealerNetworkIsNotSpecifiedForParent()
    {
      // Bug 25226
      ServiceViewAPI target = new ServiceViewAPI();

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.None).Save();
      Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      DateTime serviceDate = DateTime.UtcNow.AddDays(-2);

      Service service = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "", serviceDate, ServiceTypeEnum.Essentials);

      List<ServiceView> actual = (Ctx.OpContext.ServiceViewReadOnly.Where(s => s.fk_ServiceID == service.ID)).ToList();
      Assert.IsNotNull(actual, "ServiceView was not saved successfully.");
      Assert.AreEqual(3, actual.Count, "Incorrect number of service views returned.");
      Assert.AreEqual(1, actual.Count(x => x.fk_CustomerID == dealer.ID), "Did not find a service view for the dealer.");
      Assert.AreEqual(1, actual.Count(x => x.fk_CustomerID == parentDealer.ID), "Did not find a service view for the parent dealer.");
      Assert.AreEqual(3, actual.Count(x => x.fk_AssetID == asset.AssetID), "ServiceView AssetIDs do not match.");

      DateTime terminationDate = DateTime.UtcNow;
      var terminatedViews = API.ServiceView.TerminateRelationshipServiceViews(parentDealer.ID, dealer.ID, terminationDate);

      Assert.AreEqual(1, terminatedViews.Count, "There should be 1 terminated ServiceView.");
      Assert.AreEqual(parentDealer.ID, terminatedViews.First().fk_CustomerID, "Wrong service view terminated.");
    }

    [DatabaseTest]
    [TestMethod]
    public void TerminateRelationship_DealerWithSitechParent_DealerNetworkIsNotSpecifiedForBothDealerAndParent()
    {
      // Bug 25226
      ServiceViewAPI target = new ServiceViewAPI();

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.None).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.None).Save();
      Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      DateTime serviceDate = DateTime.UtcNow.AddDays(-2);

      Service service = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "", serviceDate, ServiceTypeEnum.Essentials);

      List<ServiceView> actual = (Ctx.OpContext.ServiceViewReadOnly.Where(s => s.fk_ServiceID == service.ID)).ToList();
      Assert.IsNotNull(actual, "ServiceView was not saved successfully.");
      Assert.AreEqual(2, actual.Count, "Incorrect number of service views returned.");
      Assert.AreEqual(1, actual.Count(x => x.fk_CustomerID == dealer.ID), "Did not find a service view for the dealer.");
      Assert.AreEqual(1, actual.Count(x => x.fk_CustomerID == parentDealer.ID), "Did not find a service view for the parent dealer.");
      Assert.AreEqual(2, actual.Count(x => x.fk_AssetID == asset.AssetID), "ServiceView AssetIDs do not match.");

      DateTime terminationDate = DateTime.UtcNow;
      var terminatedViews = API.ServiceView.TerminateRelationshipServiceViews(parentDealer.ID, dealer.ID, terminationDate);

      Assert.AreEqual(1, terminatedViews.Count, "There should be 1 terminated ServiceView.");
      Assert.AreEqual(parentDealer.ID, terminatedViews.First().fk_CustomerID, "Wrong service view terminated.");
    }

    #endregion

    #region Dealer to Account

    [DatabaseTest]
    [TestMethod]
    public void TerminateRelationship_CatDealerToAccount_AccountDevice_ActiveEssentials_CancelledHealth()
    {
      Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_CatParentDealer();

      DateTime activationDate = DateTime.UtcNow.AddDays(-30);
      DateTime ownerVisibilityDate = activationDate;
      DateTime cancellationDate = DateTime.UtcNow.AddDays(-2);
      DateTime historicalActivation = activationDate.AddMonths(-13);

      Service pl321Essentials = Helper.SetupPl321WithService(Helper.Account, ServiceTypeEnum.Essentials, ownerVisibilityDate)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Dealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentDealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation))
        .Save();

      Service pl321Health = Entity.Service.Health.ForDevice(Helper.Pl321)
        .ActivationDate(activationDate).CancellationDate(cancellationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Dealer).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentDealer).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .Save();

      DateTime terminationDate = DateTime.UtcNow.AddDays(-2);
      var terminatedViews = API.ServiceView.TerminateRelationshipServiceViews(Helper.Dealer.ID, Helper.Account.ID, terminationDate);

      Assert.AreEqual(3, terminatedViews.Count, "There should be 3 terminated ServiceViews.");

      IList<ServiceView> dealerViews = Helper.GetServiceViewsForCustomer(Helper.Dealer.ID);
      Helper.AssertServiceViewIsTerminated(dealerViews, pl321Essentials.ID, terminationDate.KeyDate(), "pl321Essentials terminated for Dealer");
      Helper.AssertServiceViewIsUnchanged(dealerViews, pl321Health.ID, cancellationDate.KeyDate(), "pl321Health should be unchanged for Dealer.");

      IList<ServiceView> parentDealerViews = Helper.GetServiceViewsForCustomer(Helper.ParentDealer.ID);
      Helper.AssertServiceViewIsTerminated(parentDealerViews, pl321Essentials.ID, terminationDate.KeyDate(), "pl321Essentials terminated for ParentDealer");
      Helper.AssertServiceViewIsUnchanged(parentDealerViews, pl321Health.ID, cancellationDate.KeyDate(), "pl321Health should be unchanged for ParentDealer.");

      IList<ServiceView> catCorpViews = Helper.GetServiceViewsForCustomer(Helper.CatCorp.ID);
      Helper.AssertServiceViewIsTerminated(catCorpViews, pl321Essentials.ID, terminationDate.KeyDate(), "pl321Essentials terminated for CatCorp");
      Helper.AssertServiceViewIsUnchanged(catCorpViews, pl321Health.ID, cancellationDate.KeyDate(), "pl321Health should be unchanged for CatCorp.");
    }

    [DatabaseTest]
    [TestMethod]
    public void TerminateRelationship_CatDealerWithSitechParentToAccount_AccountDevice_ActiveEssentials_CancelledHealth()
    {
      Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_SitechParentDealer();

      DateTime activationDate = DateTime.UtcNow.AddDays(-30);
      DateTime ownerVisibilityDate = activationDate;
      DateTime cancellationDate = DateTime.UtcNow.AddDays(-2);
      DateTime historicalActivation = activationDate.AddMonths(-13);

      Service pl321Essentials = Helper.SetupPl321WithService(Helper.Account, ServiceTypeEnum.Essentials, ownerVisibilityDate)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Dealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentDealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.SitechCorp).StartsOn(historicalActivation))
        .Save();

      Service pl321Health = Entity.Service.Health.ForDevice(Helper.Pl321)
        .ActivationDate(activationDate).CancellationDate(cancellationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Dealer).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentDealer).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.SitechCorp).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .Save();

      DateTime terminationDate = DateTime.UtcNow.AddDays(-2);
      var terminatedViews = API.ServiceView.TerminateRelationshipServiceViews(Helper.Dealer.ID, Helper.Account.ID, terminationDate);

      Assert.AreEqual(4, terminatedViews.Count, "There should be 4 terminated ServiceViews.");

      IList<ServiceView> dealerViews = Helper.GetServiceViewsForCustomer(Helper.Dealer.ID);
      Helper.AssertServiceViewIsTerminated(dealerViews, pl321Essentials.ID, terminationDate.KeyDate(), "pl321Essentials terminated for Dealer");
      Helper.AssertServiceViewIsUnchanged(dealerViews, pl321Health.ID, cancellationDate.KeyDate(), "pl321Health should be unchanged for Dealer.");

      IList<ServiceView> parentDealerViews = Helper.GetServiceViewsForCustomer(Helper.ParentDealer.ID);
      Helper.AssertServiceViewIsTerminated(parentDealerViews, pl321Essentials.ID, terminationDate.KeyDate(), "pl321Essentials terminated for ParentDealer");
      Helper.AssertServiceViewIsUnchanged(parentDealerViews, pl321Health.ID, cancellationDate.KeyDate(), "pl321Health should be unchanged for ParentDealer.");

      IList<ServiceView> catCorpViews = Helper.GetServiceViewsForCustomer(Helper.CatCorp.ID);
      Helper.AssertServiceViewIsTerminated(catCorpViews, pl321Essentials.ID, terminationDate.KeyDate(), "pl321Essentials terminated for CatCorp");
      Helper.AssertServiceViewIsUnchanged(catCorpViews, pl321Health.ID, cancellationDate.KeyDate(), "pl321Health should be unchanged for CatCorp.");

      IList<ServiceView> sitechCorpViews = Helper.GetServiceViewsForCustomer(Helper.SitechCorp.ID);
      Helper.AssertServiceViewIsTerminated(sitechCorpViews, pl321Essentials.ID, terminationDate.KeyDate(), "pl321Essentials terminated for SitechCorp");
      Helper.AssertServiceViewIsUnchanged(sitechCorpViews, pl321Health.ID, cancellationDate.KeyDate(), "pl321Health should be unchanged for SitechCorp.");
    }

    #endregion

    #region Customer to Account

    [DatabaseTest]
    [TestMethod]
    public void TermianteRelationship_CustomerToAccount_AccountDevice_ActiveEssentialsWithVisibility_CancelledHealth()
    {
      Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_CatParentDealer();

      DateTime activationDate = DateTime.UtcNow.AddDays(-30);
      DateTime ownerVisibilityDate = activationDate;
      DateTime cancellationDate = DateTime.UtcNow.AddDays(-2);
      DateTime historicalActivation = activationDate.AddMonths(-13);

      Service pl321Essentials = Helper.SetupPl321WithService(Helper.Account, ServiceTypeEnum.Essentials, ownerVisibilityDate)
        .ActivationDate(activationDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Dealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentDealer).StartsOn(historicalActivation))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation))
        .Save();

      Service pl321Health = Entity.Service.Health.ForDevice(Helper.Pl321)
        .ActivationDate(activationDate).CancellationDate(cancellationDate).OwnerVisibilityDate(ownerVisibilityDate)
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Customer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentCustomer).StartsOn(ownerVisibilityDate).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.Dealer).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.ParentDealer).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .WithView(x => x.ForAsset(Helper.AssetPl321).ForCustomer(Helper.CatCorp).StartsOn(historicalActivation).EndsOn(cancellationDate))
        .Save();

      DateTime terminationDate = DateTime.UtcNow.AddDays(-2);
      var terminatedViews = API.ServiceView.TerminateRelationshipServiceViews(Helper.Customer.ID, Helper.Account.ID, terminationDate);

      Assert.AreEqual(2, terminatedViews.Count, "There should be 2 terminated ServiceViews.");

      IList<ServiceView> customerViews = Helper.GetServiceViewsForCustomer(Helper.Customer.ID);
      Helper.AssertServiceViewIsTerminated(customerViews, pl321Essentials.ID, terminationDate.KeyDate(), "pl321Essentials should be terminated for Customer");
      Helper.AssertServiceViewIsUnchanged(customerViews, pl321Health.ID, cancellationDate.KeyDate(), "pl321Health should be unchanged for Customer.");

      IList<ServiceView> parentCustomerViews = Helper.GetServiceViewsForCustomer(Helper.ParentCustomer.ID);
      Helper.AssertServiceViewIsTerminated(parentCustomerViews, pl321Essentials.ID, terminationDate.KeyDate(), "pl321Essentials should be terminated for ParentCustomer");
      Helper.AssertServiceViewIsUnchanged(parentCustomerViews, pl321Health.ID, cancellationDate.KeyDate(), "pl321Health should be unchanged for ParentCustomer.");
    }

    #endregion

    // Customer to Customer needed

    #endregion

    #region Device Transfer Tests

    [DatabaseTest]
    [TestMethod]
		[Ignore]
    public void DeviceTransfer_AccountOwnsDevices_OldDeviceHasNoServices_NewDeviceHasEssentials()
    {
      Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_SitechParentDealer();

      Helper.Pl321 = Entity.Device.PL321.OwnerBssId(Helper.Account.BSSID).Save();
      Helper.Series522 = Entity.Device.MTS522.OwnerBssId(Helper.Account.BSSID).Save();

      Helper.AssetPl321 = Entity.Asset.WithDevice(Helper.Pl321).Save();
      Helper.AssetSeries522 = Entity.Asset.WithDevice(Helper.Series522).Save();

      var series522Essentials = Entity.Service.Essentials.ForDevice(Helper.Series522)
        .WithView(x => x.ForCustomer(Helper.CatCorp).ForAsset(Helper.AssetSeries522))
        .WithView(x => x.ForCustomer(Helper.Dealer).ForAsset(Helper.AssetSeries522))
        .WithView(x => x.ForCustomer(Helper.Customer).ForAsset(Helper.AssetSeries522)).Save();

      var terminatedViews = API.ServiceView.TerminateAssetServiceViews(Helper.AssetSeries522.AssetID, DateTime.UtcNow);
      var createdViews = API.ServiceView.CreateAssetServiceViews(Helper.AssetSeries522.AssetID, DateTime.UtcNow);

      Assert.AreEqual(3, terminatedViews.Count, "There should be 3 terminated ServiceViews.");
      Assert.IsTrue(terminatedViews.All(x => x.fk_AssetID == Helper.AssetSeries522.AssetID));

      Assert.AreEqual(3, createdViews.Count, "There should be 3 created ServiceViews.");
      Assert.IsTrue(createdViews.All(x => x.fk_AssetID == Helper.AssetPl321.AssetID), "All of the created views should be for AssetPL321.");

      var services = (from service in Ctx.OpContext.ServiceReadOnly where service.fk_DeviceID == Helper.Series522.ID || service.fk_DeviceID == Helper.Pl321.ID select service).ToList();
      Assert.AreEqual(1, services.Count, "There should still only be one service.");

      var views = (from view in Ctx.OpContext.ServiceViewReadOnly where view.fk_ServiceID == series522Essentials.ID select view).ToList();
      Assert.AreEqual(6, views.Count, "There should be 6 total views for series522Essentials.");

      var actualTerminatedViews = (from view in views where view.EndKeyDate != 99991231 select view).ToList();
      Assert.IsTrue(actualTerminatedViews.Any(x => x.fk_CustomerID == Helper.Customer.ID), "Customer should have a terminated view.");
      Assert.IsTrue(actualTerminatedViews.Any(x => x.fk_CustomerID == Helper.Dealer.ID), "Dealer should have a terminated view.");
      Assert.IsTrue(actualTerminatedViews.Any(x => x.fk_CustomerID == Helper.CatCorp.ID), "Customer should have a terminated view.");

      var actualCreatedViews = (from view in views where view.EndKeyDate == 99991231 select view).ToList();
      Assert.IsTrue(actualCreatedViews.Any(x => x.fk_CustomerID == Helper.Customer.ID), "Customer should have a created view.");
      Assert.IsTrue(actualCreatedViews.Any(x => x.fk_CustomerID == Helper.Dealer.ID), "Dealer should have a created view.");
      Assert.IsTrue(actualCreatedViews.Any(x => x.fk_CustomerID == Helper.CatCorp.ID), "CatCorp should have a created view.");
    }

		[DatabaseTest]
		[TestMethod]
		public void DeviceTransfer_AccountOwnsDevices_OldDeviceHasEssentials_NewDeviceHasNoServices_NextGenSyncs()
		{
			Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_SitechParentDealer();

			Helper.Pl321 = Entity.Device.PL321.OwnerBssId(Helper.Account.BSSID).Save();
			Helper.Series522 = Entity.Device.MTS522.OwnerBssId(Helper.Account.BSSID).Save();

			Helper.AssetPl321 = Entity.Asset.WithDevice(Helper.Pl321).Save();
			Helper.AssetSeries522 = Entity.Asset.WithDevice(Helper.Series522).Save();

			var pl321Essentials = Entity.Service.Essentials.ForDevice(Helper.Pl321)
				.WithView(x => x.ForCustomer(Helper.CatCorp).ForAsset(Helper.AssetPl321))
				.WithView(x => x.ForCustomer(Helper.Dealer).ForAsset(Helper.AssetPl321))
				.WithView(x => x.ForCustomer(Helper.Customer).ForAsset(Helper.AssetPl321)).Save();

			var NewDevice = Helper.Series522;

			
			var mockCustomerService = new Mock<ICustomerService>();
			mockCustomerService.Setup(x => x.DissociateCustomerAsset(It.IsAny<DissociateCustomerAssetEvent>())).Returns(true);
			var serviceViewApi = new ServiceViewAPI(mockCustomerService.Object);
			

			var terminatedViews = serviceViewApi.TerminateAssetServiceViews(Helper.AssetPl321.AssetID, DateTime.UtcNow);
			var createdViews = serviceViewApi.CreateAssetServiceViews(Helper.AssetSeries522.AssetID, DateTime.UtcNow);

			Assert.AreEqual(3, terminatedViews.Count, "There should be 3 terminated ServiceViews.");
			Assert.IsTrue(terminatedViews.All(x => x.fk_AssetID == Helper.AssetPl321.AssetID));

			Assert.AreEqual(0, createdViews.Count, "There should be no created ServiceViews.");

			var services = (from service in Ctx.OpContext.ServiceReadOnly where service.fk_DeviceID == Helper.Series522.ID || service.fk_DeviceID == Helper.Pl321.ID select service).ToList();
			Assert.AreEqual(1, services.Count, "There should still only be one service.");

			var views = (from view in Ctx.OpContext.ServiceViewReadOnly where view.fk_ServiceID == pl321Essentials.ID select view).ToList();
			Assert.AreEqual(3, views.Count, "There should be 3 total views for pl321Essentials.");

			mockCustomerService.Verify(x => x.DissociateCustomerAsset(It.IsAny<DissociateCustomerAssetEvent>()), Times.Exactly(2)); // With the Exclusion of Corporate Customer's service plan

			var actualTerminatedViews = (from view in views where view.EndKeyDate != 99991231 select view).ToList();
			Assert.IsTrue(actualTerminatedViews.Any(x => x.fk_CustomerID == Helper.Customer.ID), "Customer should have a terminated view.");
			Assert.IsTrue(actualTerminatedViews.Any(x => x.fk_CustomerID == Helper.Dealer.ID), "Dealer should have a terminated view.");
			Assert.IsTrue(actualTerminatedViews.Any(x => x.fk_CustomerID == Helper.CatCorp.ID), "Customer should have a terminated view.");
		}

    [DatabaseTest]
    [TestMethod]
    [Ignore]
    public void DeviceTransfer_AccountOwnsDevices_OldDeviceHasEssentials_NewDeviceHasEssentials()
    {
      Helper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_SitechParentDealer();

      Helper.Pl321 = Entity.Device.PL321.OwnerBssId(Helper.Account.BSSID).Save();
      Helper.Series522 = Entity.Device.MTS522.OwnerBssId(Helper.Account.BSSID).Save();

      Helper.AssetPl321 = Entity.Asset.WithDevice(Helper.Pl321).Save();
      Helper.AssetSeries522 = Entity.Asset.WithDevice(Helper.Series522).Save();

      var pl321Essentials = Entity.Service.Essentials.ForDevice(Helper.Pl321)
        .WithView(x => x.ForCustomer(Helper.CatCorp).ForAsset(Helper.AssetPl321))
        .WithView(x => x.ForCustomer(Helper.Dealer).ForAsset(Helper.AssetPl321))
        .WithView(x => x.ForCustomer(Helper.Customer).ForAsset(Helper.AssetPl321)).Save();

      var series522Essentials = Entity.Service.Essentials.ForDevice(Helper.Series522)
        .WithView(x => x.ForCustomer(Helper.CatCorp).ForAsset(Helper.AssetSeries522))
        .WithView(x => x.ForCustomer(Helper.Dealer).ForAsset(Helper.AssetSeries522))
        .WithView(x => x.ForCustomer(Helper.Customer).ForAsset(Helper.AssetSeries522)).Save();

      var NewDevice = Helper.Series522;

      var terminatedViews = API.ServiceView.TerminateAssetServiceViews(Helper.AssetSeries522.AssetID, DateTime.UtcNow);
      var createdViews = API.ServiceView.CreateAssetServiceViews(Helper.AssetSeries522.AssetID, DateTime.UtcNow);

      Assert.AreEqual(6, terminatedViews.Count, "There should be 6 terminated ServiceViews.");
      Assert.AreEqual(3, terminatedViews.Count(x => x.fk_AssetID == Helper.AssetSeries522.AssetID));
      Assert.AreEqual(3, terminatedViews.Count(x => x.fk_AssetID == Helper.AssetPl321.AssetID));

      Assert.AreEqual(6, createdViews.Count, "There should be 6 created ServiceViews.");
      Assert.IsTrue(createdViews.All(x => x.fk_AssetID == Helper.AssetPl321.AssetID));

      var services = (from service in Ctx.OpContext.ServiceReadOnly where service.fk_DeviceID == Helper.Series522.ID || service.fk_DeviceID == Helper.Pl321.ID select service).ToList();
      Assert.AreEqual(2, services.Count, "There should still only be two services.");

      var pl321Views = (from view in Ctx.OpContext.ServiceViewReadOnly where view.fk_ServiceID == series522Essentials.ID select view).ToList();
      Assert.AreEqual(6, pl321Views.Count, "There should be 6 total views for series522Essentials.");

      var actualPL321TermedViews = (from view in pl321Views where view.EndKeyDate != 99991231 select view).ToList();
      Assert.IsTrue(actualPL321TermedViews.Any(x => x.fk_CustomerID == Helper.Customer.ID), "Customer should have a terminated view.");
      Assert.IsTrue(actualPL321TermedViews.Any(x => x.fk_CustomerID == Helper.Dealer.ID), "Dealer should have a terminated view.");
      Assert.IsTrue(actualPL321TermedViews.Any(x => x.fk_CustomerID == Helper.CatCorp.ID), "Customer should have a terminated view.");

      var actualPL321CreatedViews = (from view in pl321Views where view.EndKeyDate == 99991231 select view).ToList();
      Assert.IsTrue(actualPL321CreatedViews.Any(x => x.fk_CustomerID == Helper.Customer.ID), "Customer should have a created view.");
      Assert.IsTrue(actualPL321CreatedViews.Any(x => x.fk_CustomerID == Helper.Dealer.ID), "Dealer should have a created view.");
      Assert.IsTrue(actualPL321CreatedViews.Any(x => x.fk_CustomerID == Helper.CatCorp.ID), "CatCorp should have a created view.");

      var series522Views = (from view in Ctx.OpContext.ServiceViewReadOnly where view.fk_ServiceID == pl321Essentials.ID select view).ToList();
      Assert.AreEqual(3, series522Views.Count, "There should be 3 total views for pl321Essentials.");

      var actual522TermedViews = (from view in series522Views where view.EndKeyDate != 99991231 select view).ToList();
      Assert.IsTrue(actual522TermedViews.Any(x => x.fk_CustomerID == Helper.Customer.ID), "Customer should have a terminated view.");
      Assert.IsTrue(actual522TermedViews.Any(x => x.fk_CustomerID == Helper.Dealer.ID), "Dealer should have a terminated view.");
      Assert.IsTrue(actual522TermedViews.Any(x => x.fk_CustomerID == Helper.CatCorp.ID), "Customer should have a terminated view.");
    }

    #endregion

    #region Ownership Transfer Tests

    #region Dealer to Dealer

    #endregion

    // Need Dealer to Account

    // Need Account to Account

    // Need Account to Dealer

    #endregion

    #region BSS V2 Service and ServiceView tests

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Account_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);
      AssertService(result.Item1, device.ID);
      Assert.AreEqual(0, result.Item2.Count);
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Customer_Success()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(customer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);
      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      Assert.AreEqual(0, result.Item2.Count);
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Account_Customer_Success()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(1, svs.Count);
      foreach (long id in new List<long> { customer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, isCustomer: true);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Account_Customer_Customer_Success()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var parentRelationship = Entity.CustomerRelationship.Relate(parentCustomer, customer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(2, svs.Count);
      foreach (long id in new List<long> { customer.ID, parentCustomer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, isCustomer: true);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Account_Customer_Customer_Customer_Success()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var parentRelationship = Entity.CustomerRelationship.Relate(parentCustomer, customer).Save();

      var grandParentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var grandParentRelationship = Entity.CustomerRelationship.Relate(grandParentCustomer, parentCustomer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(3, svs.Count);
      foreach (long id in new List<long> { customer.ID, parentCustomer.ID, grandParentCustomer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, isCustomer: true);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentals_PL321_Account_Customer_OwnerVisibilityDateNull_Success()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, null, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID, true);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(0, svs.Count);
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(2, svs.Count);
      foreach (long id in new List<long> { dealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Dealer_OTAcrossDealerNetworks_Success()
    {
      var catCorp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(catCorp);

      var trimbleCorp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.TRMB).SingleOrDefault();
      Assert.IsNotNull(trimbleCorp);

      var oldDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(oldDealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();            
      var asset = Entity.Asset.WithDevice(device).Save();

      var cancelledService = Entity.Service.Essentials.ForDevice(device).ActivationDate(DateTime.UtcNow).OwnerVisibilityDate(DateTime.UtcNow).CancellationDate(DateTime.UtcNow).Save();
      
      var newDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.TRMB).Save();

      //Mimic Ownership Transfer
      device.OwnerBSSID = newDealer.BSSID;

      AssetDeviceHistory history = new AssetDeviceHistory();
      history.fk_AssetID = asset.AssetID;
      history.fk_DeviceID = device.ID;
      history.OwnerBSSID = oldDealer.BSSID;
      history.StartUTC = DateTime.UtcNow;

      Ctx.OpContext.AssetDeviceHistory.AddObject(history);
      Ctx.OpContext.SaveChanges();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);
      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(2, svs.Count);
      foreach (long id in new List<long> { newDealer.ID, trimbleCorp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, false, false);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Dealer_NoDealerNetwork_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.None).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(1, svs.Count);
      foreach (long id in new List<long> { dealer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Dealer_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(3, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Dealer_Dealer_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var grandParentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentrelationship = Entity.CustomerRelationship.Relate(grandParentDealer, parentDealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(4, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Account_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(2, svs.Count);
      foreach (long id in new List<long> { dealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Account_Dealer_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(3, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Account_Dealer_Dealer_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var grandParentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      relationship = Entity.CustomerRelationship.Relate(grandParentDealer, parentDealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(4, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Account_Customer_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(3, svs.Count);
      foreach (long id in new List<long> { dealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      foreach (long id in new List<long> { customer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, true);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Account_Customer_Customer_Dealer_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(parentCustomer, customer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(5, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      foreach (long id in new List<long> { customer.ID, parentCustomer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, isCustomer: true);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_CATDealer_SITECHDealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate &&
        (t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT || t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH)).ToList();
      Assert.AreEqual(2, corp.Count);

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      var relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(4, svs.Count);
      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_CATDealer_SITECHDealer_TRMBDealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate &&
        (t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT || t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH ||
        t.fk_DealerNetworkID == (int)DealerNetworkEnum.TRMB)).ToList();
      Assert.AreEqual(3, corp.Count);

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      var relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var grandParentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.TRMB).Save();
      relationship = Entity.CustomerRelationship.Relate(grandParentDealer, parentDealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(6, svs.Count);
      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Account_CATDealer_SITECHDealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate &&
        (t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT || t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH)).ToList();
      Assert.AreEqual(2, corp.Count);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();


      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(4, svs.Count);
      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Account_CATDealer_SITECHDealer_TRMBDealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate &&
        (t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT || t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH ||
        t.fk_DealerNetworkID == (int)DealerNetworkEnum.TRMB)).ToList();
      Assert.AreEqual(3, corp.Count);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var grandParentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.TRMB).Save();
      relationship = Entity.CustomerRelationship.Relate(grandParentDealer, parentDealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(6, svs.Count);
      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Account_Customer_CATDealer_SITECHDealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate &&
        (t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT || t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH)).ToList();
      Assert.AreEqual(2, corp.Count);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(5, svs.Count);
      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      foreach (long id in new List<long> { customer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, isCustomer: true);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_Essentials_PL321_Account_Customer_CATDealer_SITECHDealer_TRMBDealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate &&
        (t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT || t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH ||
        t.fk_DealerNetworkID == (int)DealerNetworkEnum.TRMB)).ToList();
      Assert.AreEqual(3, corp.Count);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var grandParentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.TRMB).Save();
      relationship = Entity.CustomerRelationship.Relate(grandParentDealer, parentDealer).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(7, svs.Count);
      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      foreach (long id in new List<long> { customer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, true);
      }
    }

    private void AssertService(Service service, long deviceID, bool ownerVisibilityDateNull = false)
    {
      var keydate = DateTime.UtcNow.KeyDate();
      Assert.IsNotNull(service);
      Assert.AreEqual(deviceID, service.fk_DeviceID);
      Assert.AreEqual((int)ServiceTypeEnum.Essentials, service.fk_ServiceTypeID);
      Assert.AreEqual(DotNetExtensions.NullKeyDate, service.CancellationKeyDate);
      Assert.AreEqual(keydate, service.ActivationKeyDate);
      if (ownerVisibilityDateNull)
        Assert.AreEqual(null, service.OwnerVisibilityKeyDate);
      else
        Assert.AreEqual(keydate, service.OwnerVisibilityKeyDate);
    }

    private void AssertServiceView(ServiceView sv, long serviceID, long customerID, long assetID, bool isCustomer = false, bool isBackDated = true)
    {       
      Assert.AreEqual(customerID, sv.fk_CustomerID);
      Assert.AreEqual(sv.fk_ServiceID, serviceID);
      Assert.AreEqual(assetID, sv.fk_AssetID);
      Assert.AreEqual(DotNetExtensions.NullKeyDate, sv.EndKeyDate);
      if (isCustomer || !isBackDated)
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), sv.StartKeyDate);
      else
        Assert.AreEqual(DateTime.UtcNow.AddMonths(-13).KeyDate(), sv.StartKeyDate);
      
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Account_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);
      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);
      Assert.AreEqual(0, result.Item2.Count);

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(0, svs.Count);
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Customer_Success()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(customer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);
      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      Assert.AreEqual(0, result.Item2.Count);

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(0, svs.Count);
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Account_Customer_Success()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(1, svs.Count);
      foreach (long id in new List<long> { customer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, isCustomer: true);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(1, svs.Count);
      foreach (long id in new List<long> { customer.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, isCustomer: true);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Account_Customer_Customer_Success()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var parentRelationship = Entity.CustomerRelationship.Relate(parentCustomer, customer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(2, svs.Count);
      foreach (long id in new List<long> { customer.ID, parentCustomer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, isCustomer: true);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(2, svs.Count);
      foreach (long id in new List<long> { customer.ID, parentCustomer.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, isCustomer: true);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Account_Customer_Customer_Customer_Success()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var parentRelationship = Entity.CustomerRelationship.Relate(parentCustomer, customer).Save();

      var grandParentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var grandParentRelationship = Entity.CustomerRelationship.Relate(grandParentCustomer, parentCustomer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(3, svs.Count);
      foreach (long id in new List<long> { customer.ID, parentCustomer.ID, grandParentCustomer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, isCustomer: true);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(3, svs.Count);
      foreach (long id in new List<long> { customer.ID, parentCustomer.ID, grandParentCustomer.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, isCustomer: true);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentals_PL321_Account_Customer_OwnerVisibilityDateNull_Success()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, null, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID, true);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(0, svs.Count);

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID, true);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(0, svs.Count);
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(2, svs.Count);
      foreach (long id in new List<long> { dealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(2, svs.Count);
      foreach (long id in new List<long> { dealer.ID, corp.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Dealer_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(3, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(3, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, corp.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Dealer_Dealer_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var grandParentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentrelationship = Entity.CustomerRelationship.Relate(grandParentDealer, parentDealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(4, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(4, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID, corp.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Account_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(2, svs.Count);
      foreach (long id in new List<long> { dealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(2, svs.Count);
      foreach (long id in new List<long> { dealer.ID, corp.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Account_Dealer_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(3, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(3, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, corp.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Account_Dealer_Dealer_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var grandParentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      relationship = Entity.CustomerRelationship.Relate(grandParentDealer, parentDealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(4, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(4, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID, corp.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Account_Customer_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(3, svs.Count);
      foreach (long id in new List<long> { dealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      foreach (long id in new List<long> { customer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, true);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(3, svs.Count);

      foreach (long id in new List<long> { dealer.ID, corp.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      foreach (long id in new List<long> { customer.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, true);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Account_Customer_Customer_Dealer_Dealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).SingleOrDefault();
      Assert.IsNotNull(corp);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(parentCustomer, customer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(5, svs.Count);
      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, corp.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      foreach (long id in new List<long> { customer.ID, parentCustomer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, true);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(5, svs.Count);

      foreach (long id in new List<long> { dealer.ID, parentDealer.ID, corp.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      foreach (long id in new List<long> { customer.ID, parentCustomer.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, true);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_CATDealer_SITECHDealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate &&
        (t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT || t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH)).ToList();
      Assert.AreEqual(2, corp.Count);

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      var relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(4, svs.Count);
      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(4, svs.Count);

      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_CATDealer_SITECHDealer_TRMBDealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate &&
        (t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT || t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH ||
        t.fk_DealerNetworkID == (int)DealerNetworkEnum.TRMB)).ToList();
      Assert.AreEqual(3, corp.Count);

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      var relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var grandParentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.TRMB).Save();
      relationship = Entity.CustomerRelationship.Relate(grandParentDealer, parentDealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(6, svs.Count);
      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(6, svs.Count);

      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Account_CATDealer_SITECHDealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate &&
        (t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT || t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH)).ToList();
      Assert.AreEqual(2, corp.Count);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();


      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(4, svs.Count);
      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(4, svs.Count);

      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Account_CATDealer_SITECHDealer_TRMBDealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate &&
        (t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT || t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH ||
        t.fk_DealerNetworkID == (int)DealerNetworkEnum.TRMB)).ToList();
      Assert.AreEqual(3, corp.Count);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var grandParentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.TRMB).Save();
      relationship = Entity.CustomerRelationship.Relate(grandParentDealer, parentDealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(6, svs.Count);
      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(6, svs.Count);

      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Account_Customer_CATDealer_SITECHDealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate &&
        (t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT || t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH)).ToList();
      Assert.AreEqual(2, corp.Count);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(5, svs.Count);
      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      foreach (long id in new List<long> { customer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, true);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(5, svs.Count);

      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      foreach (long id in new List<long> { customer.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, true);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_Essentials_PL321_Account_Customer_CATDealer_SITECHDealer_TRMBDealer_Success()
    {
      var corp = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate &&
        (t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT || t.fk_DealerNetworkID == (int)DealerNetworkEnum.SITECH ||
        t.fk_DealerNetworkID == (int)DealerNetworkEnum.TRMB)).ToList();
      Assert.AreEqual(3, corp.Count);

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      relationship = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var grandParentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.TRMB).Save();
      relationship = Entity.CustomerRelationship.Relate(grandParentDealer, parentDealer).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertService(service, device.ID);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(7, svs.Count);
      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      foreach (long id in new List<long> { customer.ID })
      {
        AssertServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, true);
      }

      var success = api.TerminateServiceAndServiceViews(Ctx.OpContext, result.Item1.BSSLineID, DateTime.UtcNow);
      Assert.IsTrue(success);

      service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).SingleOrDefault();
      AssertTerminatedService(service, device.ID);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID).ToList();
      Assert.AreEqual(7, svs.Count);

      foreach (long id in (new List<long> { dealer.ID, parentDealer.ID, grandParentDealer.ID }.Concat(corp.Select(t => t.ID))))
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID);
      }

      foreach (long id in new List<long> { customer.ID })
      {
        AssertTerminatedServiceView(svs.Where(t => t.fk_CustomerID == id).Single(), service.ID, id, asset.AssetID, true);
      }
    }

    private void AssertTerminatedService(Service service, long deviceID, bool ownerVisibilityDateNull = false)
    {
      var keydate = DateTime.UtcNow.KeyDate();
      Assert.IsNotNull(service);
      Assert.AreEqual(deviceID, service.fk_DeviceID);
      Assert.AreEqual((int)ServiceTypeEnum.Essentials, service.fk_ServiceTypeID);
      Assert.AreEqual(keydate, service.CancellationKeyDate);
      Assert.AreEqual(keydate, service.ActivationKeyDate);
      if (ownerVisibilityDateNull)
        Assert.AreEqual(null, service.OwnerVisibilityKeyDate);
      else
        Assert.AreEqual(keydate, service.OwnerVisibilityKeyDate);
    }

    private void AssertTerminatedServiceView(ServiceView sv, long serviceID, long customerID, long assetID, bool isCustomer = false)
    {
      var keydate = DateTime.UtcNow.KeyDate();
      Assert.AreEqual(customerID, sv.fk_CustomerID);
      Assert.AreEqual(sv.fk_ServiceID, serviceID);
      Assert.AreEqual(assetID, sv.fk_AssetID);
      Assert.AreEqual(keydate, sv.EndKeyDate);
      if (isCustomer)
        Assert.AreEqual(keydate, sv.StartKeyDate);
      else
        Assert.AreEqual(keydate.FromKeyDate().AddMonths(-13).KeyDate(), sv.StartKeyDate);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateService_Essentials_PL321_Account_AddCustomer_Update_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var date = DateTime.UtcNow.AddDays(-10);
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();
      var updatedResult = api.UpdateServiceAndServiceViews(Ctx.OpContext, date, account.BSSID, result.Item1.ID, asset.AssetID, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).Single();
      Assert.AreEqual(date.KeyDate(), service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(1, svs.Count);

      foreach (var sv in svs)
      {
        Assert.AreEqual(date.KeyDate(), sv.StartKeyDate);
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), sv.UpdateUTC.KeyDate());
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateService_Essentials_PL321_Account_AddCustomer_Customer_Update_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var date = DateTime.UtcNow.AddDays(-10);
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(parentCustomer, customer).Save();

      var updatedResult = api.UpdateServiceAndServiceViews(Ctx.OpContext, date, account.BSSID, result.Item1.ID, asset.AssetID, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).Single();
      Assert.AreEqual(date.KeyDate(), service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(2, svs.Count);

      foreach (var sv in svs)
      {
        Assert.AreEqual(date.KeyDate(), sv.StartKeyDate);
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), sv.UpdateUTC.KeyDate());
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateService_Essentials_PL321_Account_AddCustomer_Terminate_Null_OwnerVisibilityDate_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var updatedResult = api.UpdateServiceAndServiceViews(Ctx.OpContext, null, account.BSSID, result.Item1.ID, asset.AssetID, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).Single();
      Assert.AreEqual(null, service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(1, svs.Count);

      foreach (var sv in svs)
      {
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), sv.EndKeyDate);
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), sv.UpdateUTC.KeyDate());
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateService_Essentials_PL321_Account_AddCustomer_Customer_Terminate_Null_OwnerVisibilityDate_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(parentCustomer, customer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var updatedResult = api.UpdateServiceAndServiceViews(Ctx.OpContext, null, account.BSSID, result.Item1.ID, asset.AssetID, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).Single();
      Assert.AreEqual(null, service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(2, svs.Count);

      foreach (var sv in svs)
      {
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), sv.EndKeyDate);
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), sv.UpdateUTC.KeyDate());
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateService_Essentials_PL321_Account_AddCustomer_Update_NoServiceViews_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();
      var updatedResult = api.UpdateServiceAndServiceViews(Ctx.OpContext, null, account.BSSID, result.Item1.ID, asset.AssetID, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).Single();
      Assert.AreEqual(null, service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(0, svs.Count);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateService_Essentials_PL321_Account_AddCustomer_Customer_Update_NoServiceViews_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(parentCustomer, customer).Save();

      var updatedResult = api.UpdateServiceAndServiceViews(Ctx.OpContext, null, account.BSSID, result.Item1.ID, asset.AssetID, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).Single();
      Assert.AreEqual(null, service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(0, svs.Count);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateService_Essentials_PL321_Dealer_Update_NoServiceViews_Success()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var updatedResult = api.UpdateServiceAndServiceViews(Ctx.OpContext, DateTime.UtcNow.AddDays(-10), dealer.BSSID, result.Item1.ID, asset.AssetID, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).Single();
      Assert.AreEqual(DateTime.UtcNow.AddDays(-10).KeyDate(), service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(2, svs.Count);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateService_Essentials_PL321_Customer_Update_NoServiceViews_Success()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(customer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(parentCustomer, customer).Save();

      var updatedResult = api.UpdateServiceAndServiceViews(Ctx.OpContext, DateTime.UtcNow, customer.BSSID, result.Item1.ID, asset.AssetID, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).Single();
      Assert.AreEqual(DateTime.UtcNow.KeyDate(), service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID).ToList();
      Assert.AreEqual(0, svs.Count);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateService_Essentials_PL321_Account_Dealer_AddCustomer_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var date = DateTime.UtcNow;
      api.UpdateServiceAndServiceViews(Ctx.OpContext, date, account.BSSID, result.Item1.ID, asset.AssetID, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).Single();
      Assert.AreEqual(date.KeyDate(), service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID && t.Customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer).ToList();
      Assert.AreEqual(1, svs.Count);

      foreach (var sv in svs)
      {
        Assert.AreEqual(date.KeyDate(), sv.StartKeyDate);
        Assert.AreEqual(date.KeyDate(), sv.UpdateUTC.KeyDate());
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateService_Essentials_PL321_Account_Dealer_Customer_Update_OwnerVisibilityDeate_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var date = DateTime.UtcNow.AddDays(10);
      api.UpdateServiceAndServiceViews(Ctx.OpContext, date, account.BSSID, result.Item1.ID, asset.AssetID, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).Single();
      Assert.AreEqual(date.KeyDate(), service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID && t.Customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer).ToList();
      Assert.AreEqual(1, svs.Count);

      foreach (var sv in svs)
      {
        Assert.AreEqual(date.KeyDate(), sv.StartKeyDate);
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), sv.UpdateUTC.KeyDate());
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateService_Essentials_PL321_Account_Dealer_Customer_Update_Null_OwnerVisibilityDeate_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var date = DateTime.UtcNow;
      api.UpdateServiceAndServiceViews(Ctx.OpContext, null, account.BSSID, result.Item1.ID, asset.AssetID, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).Single();
      Assert.AreEqual(null, service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID && t.Customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer).ToList();
      Assert.AreEqual(0, svs.Count);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateService_Essentials_PL321_Account_Dealer_Customer_AddCustomer_Update_OwnerVisibilityDate_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(parentCustomer, customer).Save();

      var date = DateTime.UtcNow.AddDays(-10);
      api.UpdateServiceAndServiceViews(Ctx.OpContext, date, account.BSSID, result.Item1.ID, asset.AssetID, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).Single();
      Assert.AreEqual(date.KeyDate(), service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID && t.Customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer).ToList();
      Assert.AreEqual(2, svs.Count);

      foreach (var sv in svs)
      {
        Assert.AreEqual(date.KeyDate(), sv.StartKeyDate);
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), sv.UpdateUTC.KeyDate());
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateService_Essentials_PL321_Account_Dealer_Customer_AddCustomer_Update_Null_OwnerVisibilityDate_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var api = new ServiceViewAPI();
      var result = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), DateTime.UtcNow, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      var parentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(parentCustomer, customer).Save();

      api.UpdateServiceAndServiceViews(Ctx.OpContext, null, account.BSSID, result.Item1.ID, asset.AssetID, ServiceTypeEnum.Essentials);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == result.Item1.ID).Single();
      Assert.AreEqual(null, service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == result.Item1.ID && t.Customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer).ToList();
      Assert.AreEqual(1, svs.Count);

      foreach (var sv in svs)
      {
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), sv.EndKeyDate);
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), sv.UpdateUTC.KeyDate());
      }
    }

    #endregion

    #region BSS V2 DeviceReplacement and Swap tests

    [DatabaseTest]
    [TestMethod]
    public void SwapServiceViewsBetweenOldAndNewAsset_BothAssetsDoesNotHaveActiveServiceViews_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).GpsDeviceId(IdGen.GetId().ToString()).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).Save();

      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).GpsDeviceId(IdGen.GetId().ToString()).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).Save();

      var context = new DeviceAssetContext
      {
        OldDeviceAsset = { DeviceId = oldDevice.ID, Type = (DeviceTypeEnum)oldDevice.fk_DeviceTypeID, AssetId = oldAsset.AssetID },
        NewDeviceAsset = { DeviceId = newDevice.ID, Type = (DeviceTypeEnum)newDevice.fk_DeviceTypeID, AssetId = newAsset.AssetID }
      };

      var result = API.ServiceView.SwapServiceViewsBetweenOldAndNewAsset(Ctx.OpContext, context.OldDeviceAsset.AssetId, context.NewDeviceAsset.AssetId, DateTime.UtcNow.AddDays(10));

      Assert.IsTrue(result);

      AssertDBResults(oldAsset.AssetID, newAsset.AssetID, DateTime.UtcNow.AddDays(10), 0);
    }

    [DatabaseTest]
    [TestMethod]
    public void SwapServiceViewsBetweenOldAndNewAsset_NewAssetsAloneHasActiveServiceViews_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();

      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).GpsDeviceId(IdGen.GetId().ToString()).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).Save();

      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).GpsDeviceId(IdGen.GetId().ToString()).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).Save();
      var newService = Entity.Service.Essentials.ForDevice(newDevice).WithView(x => x.ForAsset(newAsset).ForCustomer(owner)).Save();

      var context = new DeviceAssetContext
      {
        OldDeviceAsset = { DeviceId = oldDevice.ID, Type = (DeviceTypeEnum)oldDevice.fk_DeviceTypeID, AssetId = oldAsset.AssetID },
        NewDeviceAsset = { DeviceId = newDevice.ID, Type = (DeviceTypeEnum)newDevice.fk_DeviceTypeID, AssetId = newAsset.AssetID }
      };

      var result = API.ServiceView.SwapServiceViewsBetweenOldAndNewAsset(Ctx.OpContext, context.OldDeviceAsset.AssetId, context.NewDeviceAsset.AssetId, DateTime.UtcNow.AddDays(-20));

      Assert.IsTrue(result);

      AssertDBResults(oldAsset.AssetID, newAsset.AssetID, DateTime.UtcNow.AddDays(-20), 1);
    }

    [DatabaseTest]
    [TestMethod]
    public void SwapServiceViewsBetweenOldAndNewAsset_OldAssetsAloneHasActiveServiceViews_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).GpsDeviceId(IdGen.GetId().ToString()).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).Save();
      var oldService = Entity.Service.Essentials.ForDevice(oldDevice).WithView(x => x.ForAsset(oldAsset).ForCustomer(owner)).Save();

      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).GpsDeviceId(IdGen.GetId().ToString()).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).Save();


      var context = new DeviceAssetContext
      {
        OldDeviceAsset = { DeviceId = oldDevice.ID, Type = (DeviceTypeEnum)oldDevice.fk_DeviceTypeID, AssetId = oldAsset.AssetID },
        NewDeviceAsset = { DeviceId = newDevice.ID, Type = (DeviceTypeEnum)newDevice.fk_DeviceTypeID, AssetId = newAsset.AssetID }
      };

      var result = API.ServiceView.SwapServiceViewsBetweenOldAndNewAsset(Ctx.OpContext, context.OldDeviceAsset.AssetId, context.NewDeviceAsset.AssetId, DateTime.UtcNow.AddDays(-10));

      Assert.IsTrue(result);

      AssertDBResults(oldAsset.AssetID, newAsset.AssetID, DateTime.UtcNow.AddDays(-10), 2);
    }

    [DatabaseTest]
    [TestMethod]
    public void SwapServiceViewsBetweenOldAndNewAsset_BothAssetsHaveActiveServiceViews_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();

      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).GpsDeviceId(IdGen.GetId().ToString()).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).Save();
      var oldService = Entity.Service.Essentials.ForDevice(oldDevice).WithView(x => x.ForAsset(oldAsset).ForCustomer(owner)).Save();

      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).GpsDeviceId(IdGen.GetId().ToString()).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).Save();
      var newService = Entity.Service.Essentials.ForDevice(newDevice).WithView(x => x.ForAsset(newAsset).ForCustomer(owner)).Save();

      var context = new DeviceAssetContext
      {
        OldDeviceAsset = { DeviceId = oldDevice.ID, Type = (DeviceTypeEnum)oldDevice.fk_DeviceTypeID, AssetId = oldAsset.AssetID },
        NewDeviceAsset = { DeviceId = newDevice.ID, Type = (DeviceTypeEnum)newDevice.fk_DeviceTypeID, AssetId = newAsset.AssetID }
      };

      var result = API.ServiceView.SwapServiceViewsBetweenOldAndNewAsset(Ctx.OpContext, context.OldDeviceAsset.AssetId, context.NewDeviceAsset.AssetId, DateTime.UtcNow.AddDays(-5));

      Assert.IsTrue(result);

      AssertDBResults(oldAsset.AssetID, newAsset.AssetID, DateTime.UtcNow.AddDays(-5));
    }

    [TestMethod]
    [DatabaseTest]
    public void TransferServices_DeviceDoesNotExist_Failure()
    {
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).Save();
      var newDevice = Entity.Device.PL121.IbKey(IdGen.GetId().ToString()).Save();

      var result = API.ServiceView.TransferServices
                  (Ctx.OpContext, oldDevice.ID,
                  newDevice.ID,
                  DateTime.UtcNow.AddDays(-10));

      Assert.AreEqual(0, result.Count(), "Update should have been failed.");
    }

    [TestMethod]
    [DatabaseTest]
    public void TransferServices_PL321_NoServicesToTransfer_Success()
    {
      var oldDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();
      var newDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();

      var result = API.ServiceView.TransferServices
                  (Ctx.OpContext, oldDevice.ID,
                  newDevice.ID,
                  DateTime.UtcNow.AddDays(-10));

      var savedServices = (from s in Ctx.OpContext.ServiceReadOnly
                           where s.fk_DeviceID == newDevice.ID
                           select s).ToList();

      Assert.AreEqual(0, savedServices.Count(), "No services should have been moved from old device to new device.");
      Assert.AreEqual(savedServices.Count(), result.Count(), "count should match as the service transer is successful.");

      foreach (var item in savedServices)
      {
        var s = result.Where(t => t.fk_DeviceID == item.fk_DeviceID && t.fk_ServiceTypeID == item.fk_ServiceTypeID).SingleOrDefault();
        Assert.IsNotNull(s);
      }

      AssertPLRawResults(oldDevice.GpsDeviceID, newDevice.GpsDeviceID);
      AssertDeviceState(oldDevice.GpsDeviceID, (int)DeviceStateEnum.Provisioned, newDevice.GpsDeviceID, (int)DeviceStateEnum.Provisioned);
    }

    [TestMethod]
    [DatabaseTest]
    public void TransferServices_PL321_TransferHealthAndEssentials_OldDeviceDeRegistered_Success()
    {
      var oldDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();
      API.Device.UpdateOpDeviceState(Ctx.OpContext, oldDevice.GpsDeviceID, DeviceStateEnum.DeregisteredTechnician, (int)DeviceTypeEnum.PL321);
      var essentials = Entity.Service.Essentials.ForDevice(oldDevice).SyncWithRpt().Save();
      var health = Entity.Service.Health.ForDevice(oldDevice).SyncWithRpt().Save();

      var newDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();      

      var result = API.ServiceView.TransferServices
                  (Ctx.OpContext, oldDevice.ID,
                  newDevice.ID,
                  DateTime.UtcNow);

      var savedServices = (from s in Ctx.OpContext.ServiceReadOnly
                           where s.fk_DeviceID == newDevice.ID
                           select s).ToList();

      Assert.AreNotEqual(0, savedServices.Count(), "No services should have been moved from old device to new device.");
      Assert.AreEqual(savedServices.Count(), result.Count(), "count should match as the service transer is successful.");

      foreach (var item in savedServices)
      {
        var s = result.Where(t => t.fk_DeviceID == item.fk_DeviceID && t.fk_ServiceTypeID == item.fk_ServiceTypeID).SingleOrDefault();
        Assert.IsNotNull(s);
      }

      AssertPLRawResults(oldDevice.GpsDeviceID, newDevice.GpsDeviceID);
      AssertDeviceState(oldDevice.GpsDeviceID, (int)DeviceStateEnum.DeregisteredTechnician, newDevice.GpsDeviceID, (int)DeviceStateEnum.Subscribed);
    }

    [TestMethod]
    [DatabaseTest]
    public void TransferServices_PL321_TransferHealthAndEssentials_NewDeviceDeRegistered_Success()
    {
      var oldDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();      
      var essentials = Entity.Service.Essentials.ForDevice(oldDevice).SyncWithRpt().Save();
      var health = Entity.Service.Health.ForDevice(oldDevice).SyncWithRpt().Save();

      var newDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();
      API.Device.UpdateOpDeviceState(Ctx.OpContext, newDevice.GpsDeviceID, DeviceStateEnum.DeregisteredTechnician, (int)DeviceTypeEnum.PL321);

      var result = API.ServiceView.TransferServices
                  (Ctx.OpContext, oldDevice.ID,
                  newDevice.ID,
                  DateTime.UtcNow);

      var savedServices = (from s in Ctx.OpContext.ServiceReadOnly
                           where s.fk_DeviceID == newDevice.ID
                           select s).ToList();

      Assert.AreNotEqual(0, savedServices.Count(), "No services should have been moved from old device to new device.");
      Assert.AreEqual(savedServices.Count(), result.Count(), "count should match as the service transer is successful.");

      foreach (var item in savedServices)
      {
        var s = result.Where(t => t.fk_DeviceID == item.fk_DeviceID && t.fk_ServiceTypeID == item.fk_ServiceTypeID).SingleOrDefault();
        Assert.IsNotNull(s);
      }

      AssertPLRawResults(oldDevice.GpsDeviceID, newDevice.GpsDeviceID);
      AssertDeviceState(oldDevice.GpsDeviceID, (int)DeviceStateEnum.Provisioned, newDevice.GpsDeviceID, (int)DeviceStateEnum.DeregisteredTechnician);
    }

    [TestMethod]
    [DatabaseTest]
    public void TransferServices_PL321_TransferHealthAndEssentials_Success()
    {
      var oldDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();
      var essentials = Entity.Service.Essentials.ForDevice(oldDevice).SyncWithRpt().Save();
      var health = Entity.Service.Health.ForDevice(oldDevice).SyncWithRpt().Save();
      var newDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();

      var result = API.ServiceView.TransferServices
                  (Ctx.OpContext, oldDevice.ID,
                  newDevice.ID,
                  DateTime.UtcNow);

      var savedServices = (from s in Ctx.OpContext.ServiceReadOnly
                           where s.fk_DeviceID == newDevice.ID
                           select s).ToList();

      Assert.AreNotEqual(0, savedServices.Count(), "Some services should have been moved from old device to new device.");
      Assert.AreEqual(savedServices.Count(), result.Count(), "count should match as the service transer is successful.");

      foreach (var item in savedServices)
      {
        var s = result.Where(t => t.fk_DeviceID == item.fk_DeviceID && t.fk_ServiceTypeID == item.fk_ServiceTypeID).SingleOrDefault();
        Assert.IsNotNull(s);
      }

      AssertPLRawResults(oldDevice.GpsDeviceID, newDevice.GpsDeviceID);
      AssertDeviceState(oldDevice.GpsDeviceID, (int)DeviceStateEnum.Provisioned, newDevice.GpsDeviceID, (int)DeviceStateEnum.Subscribed);
    }

    [TestMethod]
    [DatabaseTest]
    public void TransferServices_PL321_TransferEssentials_Success()
    {
      var oldDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();
      var essentials = Entity.Service.Essentials.ForDevice(oldDevice).SyncWithRpt().Save();
      var newDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();

      var result = API.ServiceView.TransferServices
                  (Ctx.OpContext, oldDevice.ID,
                  newDevice.ID,
                  DateTime.UtcNow);

      var savedServices = (from s in Ctx.OpContext.ServiceReadOnly
                           where s.fk_DeviceID == newDevice.ID
                           select s).ToList();

      Assert.AreNotEqual(0, savedServices.Count(), "Some services should have been moved from old device to new device.");
      Assert.AreEqual(savedServices.Count(), result.Count(), "count should match as the service transer is successful.");

      foreach (var item in savedServices)
      {
        var s = result.Where(t => t.fk_DeviceID == item.fk_DeviceID && t.fk_ServiceTypeID == item.fk_ServiceTypeID).SingleOrDefault();
        Assert.IsNotNull(s);
      }

      AssertPLRawResults(oldDevice.GpsDeviceID, newDevice.GpsDeviceID);
      AssertDeviceState(oldDevice.GpsDeviceID, (int)DeviceStateEnum.Provisioned, newDevice.GpsDeviceID, (int)DeviceStateEnum.Subscribed);
    }

    [TestMethod]
    [DatabaseTest]
    public void TransferServices_PL321_TransferHealth_Success()
    {
      var oldDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();
      var health = Entity.Service.Health.ForDevice(oldDevice).SyncWithRpt().Save();
      var newDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();

      var result = API.ServiceView.TransferServices
                  (Ctx.OpContext, oldDevice.ID,
                  newDevice.ID,
                  DateTime.UtcNow);

      var savedServices = (from s in Ctx.OpContext.ServiceReadOnly
                           where s.fk_DeviceID == newDevice.ID
                           select s).ToList();

      Assert.AreNotEqual(0, savedServices.Count(), "Some services should have been moved from old device to new device.");
      Assert.AreEqual(savedServices.Count(), result.Count(), "count should match as the service transer is successful.");

      foreach (var item in savedServices)
      {
        var s = result.Where(t => t.fk_DeviceID == item.fk_DeviceID && t.fk_ServiceTypeID == item.fk_ServiceTypeID).SingleOrDefault();
        Assert.IsNotNull(s);
      }

      AssertPLRawResults(oldDevice.GpsDeviceID, newDevice.GpsDeviceID);
      AssertDeviceState(oldDevice.GpsDeviceID, (int)DeviceStateEnum.Provisioned, newDevice.GpsDeviceID, (int)DeviceStateEnum.Provisioned);
    }

    [TestMethod]
    [DatabaseTest]
    public void TransferServices_MTS521_TransferHealthAndEssentials_Success()
    {
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();
      var essentials = Entity.Service.Essentials.ForDevice(oldDevice).SyncWithRpt().Save();
      var utilization = Entity.Service.Utilization.ForDevice(oldDevice).SyncWithRpt().Save();
      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();

      var result = API.ServiceView.TransferServices
                  (Ctx.OpContext, oldDevice.ID,
                  newDevice.ID,
                  DateTime.UtcNow);

      var savedServices = (from s in Ctx.OpContext.ServiceReadOnly
                           where s.fk_DeviceID == newDevice.ID
                           select s).ToList();

      Assert.AreNotEqual(0, savedServices.Count(), "Some services should have been moved from old device to new device.");
      Assert.AreEqual(savedServices.Count(), result.Count(), "count should match as the service transer is successful.");

      foreach (var item in savedServices)
      {
        var s = result.Where(t => t.fk_DeviceID == item.fk_DeviceID && t.fk_ServiceTypeID == item.fk_ServiceTypeID).SingleOrDefault();
        Assert.IsNotNull(s);
      }

      AssertDeviceState(oldDevice.GpsDeviceID, (int)DeviceStateEnum.Provisioned, newDevice.GpsDeviceID, (int)DeviceStateEnum.Subscribed);
    }

    [TestMethod]
    [DatabaseTest]
    public void TransferServices_MTS521_TransferEssentials_Success()
    {
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();
      var essentials = Entity.Service.Essentials.ForDevice(oldDevice).SyncWithRpt().Save();
      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();

      var result = API.ServiceView.TransferServices
                  (Ctx.OpContext, oldDevice.ID,
                  newDevice.ID,
                  DateTime.UtcNow);

      var savedServices = (from s in Ctx.OpContext.ServiceReadOnly
                           where s.fk_DeviceID == newDevice.ID
                           select s).ToList();

      Assert.AreNotEqual(0, savedServices.Count(), "Some services should have been moved from old device to new device.");
      Assert.AreEqual(savedServices.Count(), result.Count(), "count should match as the service transer is successful.");

      foreach (var item in savedServices)
      {
        var s = result.Where(t => t.fk_DeviceID == item.fk_DeviceID && t.fk_ServiceTypeID == item.fk_ServiceTypeID).SingleOrDefault();
        Assert.IsNotNull(s);
      }

      AssertMTSRawResults(oldDevice.GpsDeviceID, newDevice.GpsDeviceID,
        ServiceType.DefaultReportingInterval.TotalSeconds, ServiceType.DefaultSamplingInterval.TotalSeconds);
      AssertDeviceState(oldDevice.GpsDeviceID, (int)DeviceStateEnum.Provisioned, newDevice.GpsDeviceID, (int)DeviceStateEnum.Subscribed);
    }

    [TestMethod]
    [DatabaseTest]
    public void TransferServices_MTS521_TransferHealth_Success()
    {
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();
      var utilization = Entity.Service.Utilization.ForDevice(oldDevice).SyncWithRpt().Save();
      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).GpsDeviceId(IdGen.GetId().ToString()).SyncWithNhRaw().Save();

      var result = API.ServiceView.TransferServices
                  (Ctx.OpContext, oldDevice.ID,
                  newDevice.ID,
                  DateTime.UtcNow);

      var savedServices = (from s in Ctx.OpContext.ServiceReadOnly
                           where s.fk_DeviceID == newDevice.ID
                           select s).ToList();

      Assert.AreNotEqual(0, savedServices.Count(), "Some services should have been moved from old device to new device.");
      Assert.AreEqual(savedServices.Count(), result.Count(), "count should match as the service transer is successful.");

      foreach (var item in savedServices)
      {
        var s = result.Where(t => t.fk_DeviceID == item.fk_DeviceID && t.fk_ServiceTypeID == item.fk_ServiceTypeID).SingleOrDefault();
        Assert.IsNotNull(s);
      }

      AssertDeviceState(oldDevice.GpsDeviceID, (int)DeviceStateEnum.Provisioned, newDevice.GpsDeviceID, (int)DeviceStateEnum.Provisioned);
    }

    [TestMethod]
    [DatabaseTest]
    public void IsDeviceTransferValid_Success()
    {
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).Save();
      var essentials = Entity.Service.Essentials.ForDevice(oldDevice).Save();
      var health = Entity.Service.Health.ForDevice(oldDevice).Save();
      var newDevice = Entity.Device.MTS522.IbKey(IdGen.GetId().ToString()).Save();

      var result = API.ServiceView.IsDeviceTransferValid(Ctx.OpContext, oldDevice.ID, (DeviceTypeEnum)newDevice.fk_DeviceTypeID);
      Assert.IsTrue(result, "Transfer should be valid");
    }

    [TestMethod]
    [DatabaseTest]
    public void IsDeviceTransferValid_Failure()
    {
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).Save();
      var essentials = Entity.Service.Essentials.ForDevice(oldDevice).Save();
      var health = Entity.Service.Health.ForDevice(oldDevice).Save();
      var newDevice = Entity.Device.PL121.IbKey(IdGen.GetId().ToString()).Save();

      var result = API.ServiceView.IsDeviceTransferValid(Ctx.OpContext, oldDevice.ID, (DeviceTypeEnum)newDevice.fk_DeviceTypeID);
      Assert.IsFalse(result, "Transfer should be valid");
    }

    [TestMethod]
    public void CreateAssetDeviceHistory_HistoryForDeviceDoesNotExist_ReturnsNewAssetDeviceHistory()
    {
      var device = Entity.Device.MTS522.Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var ash = API.ServiceView.CreateAssetDeviceHistory(Ctx.OpContext, asset.AssetID, device.ID, device.OwnerBSSID, asset.InsertUTC.Value);

      Assert.AreEqual(asset.AssetID, ash.fk_AssetID, "AssetId no equal.");
      Assert.AreEqual(device.ID, ash.fk_DeviceID, "DeviceId no equal.");
      Assert.AreEqual(device.OwnerBSSID, ash.OwnerBSSID, "OwnerBssId no equal.");
      Assert.AreEqual(asset.InsertUTC, ash.StartUTC, "InsertUtc no equal.");
    }

    [TestMethod]
    public void CreateAssetDeviceHistory_HistoryForDeviceExists_AssetDeviceHistoryStartUtcIsExistingEndUtc()
    {
      var endDate = DateTime.UtcNow.AddDays(-10);
      var device = Entity.Device.MTS522.Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var existingAdh = new AssetDeviceHistory
      {
        fk_AssetID = asset.AssetID,
        fk_DeviceID = device.ID,
        StartUTC = DateTime.UtcNow.AddDays(-11),
        EndUTC = endDate
      };
      Ctx.OpContext.AssetDeviceHistory.AddObject(existingAdh);
      Ctx.OpContext.SaveChanges();

      var ash = API.ServiceView.CreateAssetDeviceHistory(Ctx.OpContext, asset.AssetID, device.ID, device.OwnerBSSID, asset.InsertUTC.Value);

      Assert.AreEqual(asset.AssetID, ash.fk_AssetID, "AssetId no equal.");
      Assert.AreEqual(device.ID, ash.fk_DeviceID, "DeviceId no equal.");
      Assert.AreEqual(device.OwnerBSSID, ash.OwnerBSSID, "OwnerBssId no equal.");
      Assert.AreEqual(endDate, ash.StartUTC, "startUtc is not existing end date.");
    }

    private void AssertDeviceState(string oldGpsDeviceID, int modifiedStateForOldDevice, string newGpsDeviceID, int modifiedStateForNewDevice)
    {
      using (var op = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var oldDeviceState = op.DeviceReadOnly.Where(t => t.GpsDeviceID == oldGpsDeviceID).Select(t => t.fk_DeviceStateID).FirstOrDefault();
        Assert.IsNotNull(oldDeviceState, "Old Device State is NULL");
        Assert.AreEqual(modifiedStateForOldDevice, oldDeviceState, "Device State for Old Device not set correctly");

        var newDeviceState = op.DeviceReadOnly.Where(t => t.GpsDeviceID == newGpsDeviceID).Select(t => t.fk_DeviceStateID).FirstOrDefault();
        Assert.IsNotNull(newDeviceState, "New Device State is NULL");
        Assert.AreEqual(modifiedStateForNewDevice, newDeviceState, "Device State for New Device not set correctly");
      }
    }

    private void AssertPLRawResults(string oldGpsDeviceID, string newGpsDeviceID)
    {
      using (var opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
      {
          var plDefaults = opCtx1.PLDevice.Where(t => t.ModuleCode == oldGpsDeviceID || t.ModuleCode == newGpsDeviceID).ToList();

        Assert.AreEqual(2, plDefaults.Count);
      }
    }

    private void AssertMTSRawResults(string oldGpsDeviceID, string newGpsDeviceID, double reportingInterval, double samplingInterval)
    {
      using (var op = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var mtsDefaults = op.MTSDevice.Where(t => t.SerialNumber == oldGpsDeviceID || t.SerialNumber == newGpsDeviceID).ToList();

        Assert.AreEqual(2, mtsDefaults.Count);
        var oldRawDevice = mtsDefaults.Where(t => t.SerialNumber == oldGpsDeviceID).Single();
        Assert.AreEqual(ServiceType.DefaultReportingInterval.TotalSeconds, oldRawDevice.UpdateRate);
        Assert.AreEqual(ServiceType.DefaultSamplingInterval.TotalSeconds, oldRawDevice.SampleRate);

        var newRawDevice = mtsDefaults.Where(t => t.SerialNumber == newGpsDeviceID).Single();
        Assert.AreEqual(reportingInterval, newRawDevice.UpdateRate);
        Assert.AreEqual(samplingInterval, newRawDevice.SampleRate);
      }
    }

    private void AssertDBResults(long oldAssetID, long newAssetID, DateTime actionUTC, int flag = 3)
    {
      var ids = new List<long> { oldAssetID, newAssetID }.OrderBy(t => t);
      var keyDate = actionUTC.KeyDate();

      var serviceViews = (from sv in Ctx.OpContext.ServiceViewReadOnly
                          where ids.Contains(sv.fk_AssetID)
                          select sv).ToList();

      if (flag == 0) //both assets doesn't have active service views
      {
        Assert.AreEqual(0, serviceViews.Count(), "No Service Views are expected to be present.");
      }
      else
      {
        Assert.AreNotEqual(0, serviceViews.Count(), "Some Service Views are expected to be present.");
        if (flag == 1) //new asset has active service views
        {
          var svs = serviceViews.Where(t => t.EndKeyDate == keyDate).Select(t => t.fk_AssetID).ToList();
          Assert.AreEqual(svs.Intersect(ids.Where(t => t == newAssetID)).Count(), ids.Where(t => t == newAssetID).Count(), "Expected to match the cound of the ids");

          svs = serviceViews.Where(t => t.EndKeyDate == DotNetExtensions.NullKeyDate && t.StartKeyDate == keyDate).Select(t => t.fk_AssetID).ToList();
          Assert.AreEqual(svs.Intersect(ids.Where(t => t == oldAssetID)).Count(), ids.Where(t => t == oldAssetID).Count(), "Expected to match the cound of the ids");
        }
        else if (flag == 2) //old asset has active service views
        {
          var svs = serviceViews.Where(t => t.EndKeyDate == keyDate).Select(t => t.fk_AssetID).ToList();
          Assert.AreEqual(svs.Intersect(ids.Where(t => t == oldAssetID)).Count(), ids.Where(t => t == oldAssetID).Count(), "Expected to match the cound of the ids");

          svs = serviceViews.Where(t => t.EndKeyDate == DotNetExtensions.NullKeyDate && t.StartKeyDate == keyDate).Select(t => t.fk_AssetID).ToList();
          Assert.AreEqual(svs.Intersect(ids.Where(t => t == newAssetID)).Count(), ids.Where(t => t == newAssetID).Count(), "Expected to match the cound of the ids");
        }
        else if (flag == 3) //both the assets have active service views
        {
          var svs = serviceViews.Where(t => t.EndKeyDate == keyDate).Select(t => t.fk_AssetID).ToList();
          Assert.AreEqual(svs.Intersect(ids).Count(), ids.Count(), "Expected to match the cound of the ids");

          svs = serviceViews.Where(t => t.EndKeyDate == DotNetExtensions.NullKeyDate && t.StartKeyDate == keyDate).Select(t => t.fk_AssetID).ToList();
          Assert.AreEqual(svs.Intersect(ids).Count(), ids.Count(), "Expected to match the cound of the ids");
        }
      }
    }
    #endregion
  }
}
