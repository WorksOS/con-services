using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.UnitTest.Common;
using VSS.UnitTest.Common.Contexts;
using VSS.UnitTest.Common.EntityBuilder;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  public class ServiceViewAPITestHelper
  {
    public IContextContainer Ctx { get { return ContextContainer.Current; } }

    public Customer CatCorp;
    public Customer SitechCorp;
    public Customer ParentDealer;
    public Customer ParentDealer2;
    public Customer Dealer;
    public Customer Dealer2;
    public Customer ParentCustomer;
    public Customer ParentCustomer2;
    public Customer Customer;
    public Customer Customer2;
    public Customer Account;
    public Customer Account2;
    public Device Pl321;
    public Device Series522;
    public Device Series523;
    public Asset AssetPl321;
    public Asset AssetSeries522;
    public Asset AssetSeries523;

    //#region Utility Methods
    public IList<ServiceView> GetServiceViewsForCustomer(long customerId)
    {
      return (from sv in Ctx.OpContext.ServiceViewReadOnly
              where sv.fk_CustomerID == customerId
              select sv).ToList();
    }

    public Customer GetCorporateCustomer(DealerNetworkEnum dealerNetwork)
    {
      return (from c in Ctx.OpContext.Customer
              where c.fk_DealerNetworkID == (int)dealerNetwork
                && c.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate
              select c).Single();
    }
    //#endregion

    //#region Assert Helpers
    public void AssertServiceViewIsCreated(IList<ServiceView> actualServiceViews, Service basedOnService, int startKeyDate, string prompt)
    {
      var serviceViews = actualServiceViews.Where(x => x.fk_ServiceID == basedOnService.ID);
      Assert.AreEqual(1, serviceViews.Count(), prompt + " - Incorrect number of ServiceViews created.");
      var actualServiceView = serviceViews.First();
      Assert.AreEqual(startKeyDate, actualServiceView.StartKeyDate, prompt + " - Incorrect StartKeyDate");
      Assert.AreEqual(basedOnService.CancellationKeyDate, actualServiceView.EndKeyDate, prompt + " - Incorrect EndKeyDate");
    }

    public void AssertServiceViewWasNotCreated(IList<ServiceView> serviceViews, long serviceId, string prompt)
    {
      Assert.IsFalse(serviceViews.Any(x => x.fk_ServiceID == serviceId), prompt);
    }

    public void AssertServiceViewIsTerminated(IList<ServiceView> serviceViews, long serviceId, int expectedEndDate, string prompt)
    {
      Assert.IsTrue(serviceViews.Count > 0, prompt);
      Assert.IsTrue(serviceViews.Where(x => x.fk_ServiceID == serviceId).All(x => x.EndKeyDate == expectedEndDate), prompt);
    }

    public void AssertServiceViewIsUnchanged(IList<ServiceView> serviceViews, long serviceId, int expectedEndDate, string prompt)
    {
      Assert.IsTrue(serviceViews.Where(x => x.fk_ServiceID == serviceId).All(x => x.EndKeyDate == expectedEndDate), prompt);
    }
    //#endregion

    //#region Scenario Setup

    public ServiceBuilder SetupPl321WithService(Customer owner, ServiceTypeEnum serviceType, DateTime? ownerVisibilityDate = null)
    {
      Pl321 = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(owner.BSSID).Save();
      AssetPl321 = Entity.Asset.WithDevice(Pl321).Save();
      var servicebuilder = Entity.Service.ServiceType(serviceType).ForDevice(Pl321).OwnerVisibilityDate(ownerVisibilityDate);
      return servicebuilder;
    }

    public ServiceBuilder SetupSeries522WithService(Customer owner, ServiceTypeEnum serviceType, DateTime? ownerVisibilityDate = null)
    {
      Series522 = Entity.Device.MTS522.IbKey(IdGen.StringId()).OwnerBssId(owner.BSSID).Save();
      AssetSeries522 = Entity.Asset.WithDevice(Series522).Save();
      var servicebuilder = Entity.Service.ServiceType(serviceType).ForDevice(Series522).OwnerVisibilityDate(ownerVisibilityDate);
      return servicebuilder;
    }

    public void SetupHierarchy_OneCustomerHierarchy_OneCatDealer_CatParentDealer()
    {
      CatCorp = GetCorporateCustomer(DealerNetworkEnum.CAT);
      ParentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      Dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      Entity.CustomerRelationship.Relate(ParentDealer, Dealer).Save();

      ParentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      Customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(ParentCustomer, Customer).Save();

      Account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(Dealer, Account).Save();
      Entity.CustomerRelationship.Relate(Customer, Account).Save();

      Account2 = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(Dealer, Account2).Save();
      Entity.CustomerRelationship.Relate(Customer, Account2).Save();
    }

    public void SetupHierarchy_OneCustomerHierarchy_OneCatDealer_SitechParentDealer()
    {
      SitechCorp = GetCorporateCustomer(DealerNetworkEnum.SITECH);
      CatCorp = GetCorporateCustomer(DealerNetworkEnum.CAT);
      ParentDealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.SITECH).Save();
      Dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).Save();
      Entity.CustomerRelationship.Relate(ParentDealer, Dealer).Save();

      ParentCustomer = Entity.Customer.EndCustomer.Save();
      Customer = Entity.Customer.EndCustomer.Save();
      Entity.CustomerRelationship.Relate(ParentCustomer, Customer).Save();

      Account = Entity.Customer.Account.Save();
      Entity.CustomerRelationship.Relate(Dealer, Account).Save();
      Entity.CustomerRelationship.Relate(Customer, Account).Save();

      Account2 = Entity.Customer.Account.Save();
      Entity.CustomerRelationship.Relate(Dealer, Account2).Save();
      Entity.CustomerRelationship.Relate(Customer, Account2).Save();
    }

    public void SetupHierarchy_OneCustomerHierarchy_TwoCatDealers_SitechParentDealer2()
    {
      CatCorp = GetCorporateCustomer(DealerNetworkEnum.CAT);
      ParentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      Dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      Entity.CustomerRelationship.Relate(ParentDealer, Dealer).Save();

      SitechCorp = GetCorporateCustomer(DealerNetworkEnum.SITECH);
      ParentDealer2 = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      Dealer2 = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      Entity.CustomerRelationship.Relate(ParentDealer2, Dealer2).Save();

      ParentCustomer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      Customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(ParentCustomer, Customer).Save();

      Account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(Dealer, Account).Save();
      Entity.CustomerRelationship.Relate(Customer, Account).Save();

      Account2 = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(Dealer2, Account2).Save();
      Entity.CustomerRelationship.Relate(Customer, Account2).Save();
    }

    public void SetupHierarchy_OneCustomerHierarchy_TwoCatDealers()
    {
      CatCorp = GetCorporateCustomer(DealerNetworkEnum.CAT);
      Dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      Dealer2 = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();

      Customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();

      Account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(Dealer, Account).Save();
      Entity.CustomerRelationship.Relate(Customer, Account).Save();

      Account2 = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(Dealer2, Account2).Save();
      Entity.CustomerRelationship.Relate(Customer, Account2).Save();
    }

    public void SetupHierarchy_TwoCustomerHierarchies_TwoCatDealers()
    {
      CatCorp = GetCorporateCustomer(DealerNetworkEnum.CAT);
      Dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      Dealer2 = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();

      Customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      Customer2 = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();

      Account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(Dealer, Account).Save();
      Entity.CustomerRelationship.Relate(Customer, Account).Save();

      Account2 = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(Dealer2, Account2).Save();
      Entity.CustomerRelationship.Relate(Customer2, Account2).Save();
    }

    public void SetupHierarchy_TwoCustomerHierarchies_OneCatDealer()
    {
      CatCorp = GetCorporateCustomer(DealerNetworkEnum.CAT);
      Dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      Customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();

      Account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(Dealer, Account).Save();
      Entity.CustomerRelationship.Relate(Customer, Account).Save();

      Customer2 = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();

      Account2 = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(Dealer, Account2).Save();
      Entity.CustomerRelationship.Relate(Customer2, Account2).Save();
    }

    public void SetupHierarchy_OneCustomerHierarchy_OneCatDealer_OneSitechDealer()
    {
      CatCorp = GetCorporateCustomer(DealerNetworkEnum.CAT);
      SitechCorp = GetCorporateCustomer(DealerNetworkEnum.SITECH);

      Dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      Dealer2 = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.SITECH).Save();
      Customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();

      Account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(Dealer, Account).Save();
      Entity.CustomerRelationship.Relate(Customer, Account).Save();

      Account2 = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(Dealer2, Account2).Save();
      Entity.CustomerRelationship.Relate(Customer, Account2).Save();
    }
    //#endregion
  }
}
