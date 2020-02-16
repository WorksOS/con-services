using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ServicePlanUpdatedWorkflowTests : BssUnitTestBase
  {
    WorkflowResult result;
    ServicePlan message;

    [TestCleanup]
    public void TestCleanup()
    {
      if (result == null)
        return;
      new ConsoleResultProcessor().Process(message, result);
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanUpdated_DeviceTransfer_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      Entity.CustomerRelationship.Relate(dealer, account).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(customer, account).Save();

      var device1 = Entity.Device.MTS522.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).Save();
      Entity.Asset.WithDevice(device1).WithCoreService().Save();
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device1.GpsDeviceID, DeviceTypeEnum.Series522, DeviceStateEnum.Subscribed);

      var service = Ctx.OpContext.ServiceReadOnly.SingleOrDefault(t => t.fk_DeviceID == device1.ID && t.fk_ServiceTypeID == (int)ServiceTypeEnum.Essentials);
      Assert.IsNotNull(service);

      var device2 = Entity.Device.MTS523.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).Save();
      Entity.Asset.WithDevice(device2).Save();
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device2.GpsDeviceID, DeviceTypeEnum.Series523, DeviceStateEnum.Subscribed);

      message = BSS.SPUpdated.IBKey(device2.IBKey).OwnerVisibilityDate(null).ServicePlanlineID(service.BSSLineID).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.SERVICE_NOT_ASSOCIATED_WITH_DEVICE,
        service.BSSLineID, device1.GpsDeviceID, device1.IBKey));
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanUpdated_ServicePlanLineIDDoesNotExists_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var device = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(account.BSSID).Save();

      message = BSS.SPUpdated.IBKey(device.IBKey).ServicePlanlineID(IdGen.GetId().ToString()).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format("Service: {0} does not exists", message.ServicePlanlineID), "Summary is expected to contain the message.");
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanUpdated_ServiceTerminationDateSpecified_Failure()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var device = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(customer.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var service = Entity.Service.Essentials.BssPlanLineId(IdGen.GetId().ToString()).ForDevice(device).Save();

      message = BSS.SPUpdated.IBKey(device.IBKey).ServicePlanlineID(service.BSSLineID).ServiceTerminationDate(DateTime.UtcNow).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success, "Result status should be false.");
      StringAssert.Contains(result.Summary, string.Format("Service Termination Date {0} Defined for Action {1}.", string.Empty, "Updated"), "Summary is expected to contain the message.");
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanUpdated_TerminateCustomerServiceViews_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var dealer_account_rel = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var customer_account_rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(account.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).WithCoreService().Save();
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series521, DeviceStateEnum.Subscribed);

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.fk_DeviceID == device.ID && t.fk_ServiceTypeID == (int)ServiceTypeEnum.Essentials).SingleOrDefault();
      Assert.IsNotNull(service);

      message = BSS.SPUpdated.IBKey(device.IBKey).OwnerVisibilityDate(null).ServicePlanlineID(service.BSSLineID).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var updatedService = Ctx.OpContext.ServiceReadOnly.Where(t => t.fk_DeviceID == device.ID && t.fk_ServiceTypeID == (int)ServiceTypeEnum.Essentials).SingleOrDefault();
      Assert.IsNotNull(updatedService);
      Assert.IsNull(updatedService.OwnerVisibilityKeyDate);
      ServiceView serviceView = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_AssetID == asset.AssetID && t.fk_CustomerID == customer.ID && t.fk_ServiceID == updatedService.ID).SingleOrDefault();
      Assert.IsNotNull(serviceView);
      Assert.AreEqual(DateTime.UtcNow.KeyDate(), serviceView.EndKeyDate, "End key date should be today as the service and service views are terminated.");
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanUpdated_CreateCustomerServiceViews_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var dealer_account_rel = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var customer_account_rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(account.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var servicePlanLineID = IdGen.StringId();
      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(servicePlanLineID).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);

      var date = DateTime.UtcNow.AddMonths(-6);
      message = BSS.SPUpdated.IBKey(device.IBKey).OwnerVisibilityDate(date).ServicePlanlineID(servicePlanLineID).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var updatedService = Ctx.OpContext.ServiceReadOnly.Where(t => t.fk_DeviceID == device.ID && t.fk_ServiceTypeID == (int)ServiceTypeEnum.Essentials).SingleOrDefault();
      Assert.IsNotNull(updatedService);
      Assert.AreEqual(date.KeyDate(), updatedService.OwnerVisibilityKeyDate);
      ServiceView serviceView = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_AssetID == asset.AssetID && t.fk_CustomerID == customer.ID && t.fk_ServiceID == updatedService.ID).SingleOrDefault();
      Assert.IsNotNull(serviceView);
      Assert.AreEqual(date.KeyDate(), serviceView.StartKeyDate, "Start key date should be today as the service and service views are terminated.");
      Assert.AreEqual(DotNetExtensions.NullKeyDate, serviceView.EndKeyDate, "End key date should be today as the service and service views are terminated.");
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanUpdated_UpdateCustomerServiceViews_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var dealer_account_rel = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var customer_account_rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(account.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).WithCoreService().Save();
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series521, DeviceStateEnum.Subscribed);

      var service = Ctx.OpContext.Service.Where(t => t.fk_DeviceID == device.ID && t.fk_ServiceTypeID == (int)ServiceTypeEnum.Essentials).SingleOrDefault();
      Assert.IsNotNull(service);
      service.OwnerVisibilityKeyDate = DateTime.UtcNow.KeyDate();
      Ctx.OpContext.SaveChanges();
      service = Ctx.OpContext.Service.Where(t => t.fk_DeviceID == device.ID && t.fk_ServiceTypeID == (int)ServiceTypeEnum.Essentials).SingleOrDefault();
      Assert.AreEqual(DateTime.UtcNow.KeyDate(), service.OwnerVisibilityKeyDate);

      var date = DateTime.UtcNow.AddMonths(-6);
      message = BSS.SPUpdated.IBKey(device.IBKey).OwnerVisibilityDate(date).ServicePlanlineID(service.BSSLineID).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var updatedService = Ctx.OpContext.ServiceReadOnly.Where(t => t.fk_DeviceID == device.ID && t.fk_ServiceTypeID == (int)ServiceTypeEnum.Essentials).SingleOrDefault();
      Assert.IsNotNull(updatedService);
      Assert.AreEqual(date.KeyDate(), updatedService.OwnerVisibilityKeyDate);
      ServiceView serviceView = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_AssetID == asset.AssetID && t.fk_CustomerID == customer.ID && t.fk_ServiceID == updatedService.ID).SingleOrDefault();
      Assert.IsNotNull(serviceView);
      Assert.AreEqual(date.KeyDate(), serviceView.StartKeyDate, "Start key date should be today as the service and service views are terminated.");
      Assert.AreEqual(DotNetExtensions.NullKeyDate, serviceView.EndKeyDate, "End key date should be today as the service and service views are terminated.");
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanUpdated_ReduceVisbilityDateCustomerServiceViews_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var dealer_account_rel = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var customer_account_rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(account.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).WithCoreService().Save();
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series521, DeviceStateEnum.Subscribed);

      var service = Ctx.OpContext.Service.Where(t => t.fk_DeviceID == device.ID && t.fk_ServiceTypeID == (int)ServiceTypeEnum.Essentials).SingleOrDefault();
      Assert.IsNotNull(service);
      service.OwnerVisibilityKeyDate = DateTime.UtcNow.AddYears(-1).KeyDate();
      Ctx.OpContext.SaveChanges();
      service = Ctx.OpContext.Service.Where(t => t.fk_DeviceID == device.ID && t.fk_ServiceTypeID == (int)ServiceTypeEnum.Essentials).SingleOrDefault();
      Assert.AreEqual(DateTime.UtcNow.AddYears(-1).KeyDate(), service.OwnerVisibilityKeyDate);

      var date = DateTime.UtcNow.AddMonths(-6);
      message = BSS.SPUpdated.IBKey(device.IBKey).OwnerVisibilityDate(date).ServicePlanlineID(service.BSSLineID).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var updatedService = Ctx.OpContext.ServiceReadOnly.Where(t => t.fk_DeviceID == device.ID && t.fk_ServiceTypeID == (int)ServiceTypeEnum.Essentials).SingleOrDefault();
      Assert.IsNotNull(updatedService);
      Assert.AreEqual(date.KeyDate(), updatedService.OwnerVisibilityKeyDate);
      ServiceView serviceView = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_AssetID == asset.AssetID && t.fk_CustomerID == customer.ID && t.fk_ServiceID == updatedService.ID).SingleOrDefault();
      Assert.IsNotNull(serviceView);
      Assert.AreEqual(date.KeyDate(), serviceView.StartKeyDate, "Start key date should be today as the service and service views are terminated.");
      Assert.AreEqual(DotNetExtensions.NullKeyDate, serviceView.EndKeyDate, "End key date should be today as the service and service views are terminated.");
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanUpdated_NullOwnerVisibilityDate_UpdateToProperValue()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.MTS521.OwnerBssId(account.BSSID).IbKey(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId()).ActivationDate(DateTime.UtcNow.AddDays(-30)).OwnerVisibilityDate(null).Save();
      Assert.IsNull(essentials.OwnerVisibilityKeyDate);
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series521, DeviceStateEnum.Subscribed);

      var message = BSS.SPUpdated.IBKey(device.IBKey).ServicePlanName("89500-00").ServicePlanlineID(essentials.BSSLineID).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.fk_DeviceID == device.ID && t.BSSLineID == message.ServicePlanlineID).Single();
      Assert.AreEqual(service.ActivationKeyDate, essentials.ActivationKeyDate);
      Assert.AreEqual(service.CancellationKeyDate, essentials.CancellationKeyDate);
      Assert.AreEqual(service.OwnerVisibilityKeyDate, DateTime.UtcNow.KeyDate());

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == essentials.ID).ToList();
      foreach (var sv in svs)
      {
        Assert.AreEqual(sv.StartKeyDate, DateTime.UtcNow.KeyDate());
        Assert.AreEqual(sv.EndKeyDate, essentials.CancellationKeyDate);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanUpdated_NonNullOwnerVisibilityDate_UpdateToNullValue()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.MTS521.OwnerBssId(account.BSSID).IbKey(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId())
        .ActivationDate(DateTime.UtcNow.AddDays(-30)).
        OwnerVisibilityDate(DateTime.UtcNow).WithView(x => x.ForAsset(asset).ForCustomer(customer)).Save();
      Assert.IsNotNull(essentials.OwnerVisibilityKeyDate);
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series521, DeviceStateEnum.Subscribed);

      var message = BSS.SPUpdated.IBKey(device.IBKey).ServicePlanName("89500-00").ServicePlanlineID(essentials.BSSLineID).OwnerVisibilityDate(null).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.fk_DeviceID == device.ID && t.BSSLineID == message.ServicePlanlineID).Single();
      Assert.AreEqual(service.ActivationKeyDate, essentials.ActivationKeyDate);
      Assert.AreEqual(service.CancellationKeyDate, essentials.CancellationKeyDate);
      Assert.AreEqual(null, service.OwnerVisibilityKeyDate);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == essentials.ID && t.fk_CustomerID == customer.ID).ToList();
      foreach (var sv in svs)
      {
        Assert.AreEqual(sv.EndKeyDate, DateTime.UtcNow.KeyDate());
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Update_NotUpdating_OwnerVisibilityDate_Properly_Test()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).Save();
      var rel = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_CustomerID == customer.ID && t.fk_AssetID == asset.AssetID).ToList();
      Assert.AreNotEqual(0, svs.Count());
      var keydate = Convert.ToDateTime(message.OwnerVisibilityDate).KeyDate();
      foreach (var sv in svs)
      {
        Assert.AreEqual(sv.StartKeyDate, keydate);
        Assert.AreEqual(sv.EndKeyDate, DotNetExtensions.NullKeyDate);
      }

      message = BSS.SPUpdated.IBKey(device.IBKey).ServicePlanlineID(message.ServicePlanlineID).ServicePlanName(message.ServicePlanName).OwnerVisibilityDate(null).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_CustomerID == customer.ID && t.fk_AssetID == asset.AssetID).ToList();
      Assert.AreNotEqual(0, svs.Count());
      var endkeydate = Convert.ToDateTime(message.ActionUTC).KeyDate();
      foreach (var sv in svs)
      {
        Assert.AreEqual(sv.StartKeyDate, keydate);
        Assert.AreEqual(sv.EndKeyDate, endkeydate);
      }

      message = BSS.SPUpdated.IBKey(device.IBKey).ServicePlanlineID(message.ServicePlanlineID).ServicePlanName(message.ServicePlanName).OwnerVisibilityDate(DateTime.UtcNow.AddDays(3)).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_CustomerID == customer.ID && t.fk_AssetID == asset.AssetID).ToList();
      Assert.AreNotEqual(0, svs.Count());
      keydate = Convert.ToDateTime(message.OwnerVisibilityDate).KeyDate();
      foreach (var sv in svs)
      {
        Assert.AreEqual(sv.StartKeyDate, keydate);
        Assert.AreEqual(sv.EndKeyDate, DotNetExtensions.NullKeyDate);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_PL321_WithAsset_OwnedByCATDealer_WithCustomer_OwnerVisibilityDateIs13MonthsOfStartKeyDate_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var rel = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      message = BSS.SPActivated.IBKey(device.IBKey).OwnerVisibilityDate(DateTime.UtcNow).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_CustomerID == customer.ID || t.fk_CustomerID == dealer.ID && t.fk_AssetID == asset.AssetID).ToList();
      Assert.AreNotEqual(0, svs.Count);

      foreach (var sv in svs.Where(t=>t.fk_CustomerID == dealer.ID))
      {
        Assert.AreEqual(sv.StartKeyDate, DateTime.UtcNow.AddMonths(-13).KeyDate());
        Assert.AreEqual(sv.EndKeyDate, DotNetExtensions.NullKeyDate);
      }

      foreach (var sv in svs.Where(t=>t.fk_CustomerID == customer.ID))
      {
        Assert.AreEqual(sv.StartKeyDate, DateTime.UtcNow.KeyDate());
        Assert.AreEqual(sv.EndKeyDate, DotNetExtensions.NullKeyDate);
      }

      message = BSS.SPUpdated.IBKey(device.IBKey).OwnerVisibilityDate(DateTime.UtcNow.AddMonths(-13)).ServicePlanlineID(message.ServicePlanlineID).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_CustomerID == customer.ID || t.fk_CustomerID == dealer.ID && t.fk_AssetID == asset.AssetID).ToList();
      Assert.AreNotEqual(0, svs.Count);

      foreach (var sv in svs)
      {
        Assert.AreEqual(sv.StartKeyDate, DateTime.UtcNow.AddMonths(-13).KeyDate());
        Assert.AreEqual(sv.EndKeyDate, DotNetExtensions.NullKeyDate);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_PL321_WithAsset_OwnedByCATDealer_WithCustomer_OwnerVisibilityDateIsIn13MonthsOfStartKeyDate_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var rel = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      message = BSS.SPActivated.IBKey(device.IBKey).OwnerVisibilityDate(DateTime.UtcNow).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_CustomerID == customer.ID || t.fk_CustomerID == dealer.ID && t.fk_AssetID == asset.AssetID).ToList();
      Assert.AreNotEqual(0, svs.Count);

      foreach (var sv in svs.Where(t => t.fk_CustomerID == dealer.ID))
      {
        Assert.AreEqual(sv.StartKeyDate, DateTime.UtcNow.AddMonths(-13).KeyDate());
        Assert.AreEqual(sv.EndKeyDate, DotNetExtensions.NullKeyDate);
      }

      foreach (var sv in svs.Where(t => t.fk_CustomerID == customer.ID))
      {
        Assert.AreEqual(sv.StartKeyDate, DateTime.UtcNow.KeyDate());
        Assert.AreEqual(sv.EndKeyDate, DotNetExtensions.NullKeyDate);
      }

      message = BSS.SPUpdated.IBKey(device.IBKey).OwnerVisibilityDate(DateTime.UtcNow.AddMonths(-11)).ServicePlanlineID(message.ServicePlanlineID).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_CustomerID == customer.ID || t.fk_CustomerID == dealer.ID && t.fk_AssetID == asset.AssetID).ToList();
      Assert.AreNotEqual(0, svs.Count);

      foreach (var sv in svs.Where(t => t.fk_CustomerID == customer.ID))
      {
        Assert.AreEqual(sv.StartKeyDate, DateTime.UtcNow.AddMonths(-11).KeyDate());
        Assert.AreEqual(sv.EndKeyDate, DotNetExtensions.NullKeyDate);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_PL321_WithAsset_OwnedByCATDealer_WithCustomer_OwnerVisibilityDateIsAfterStartKeyDate_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var rel = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      message = BSS.SPActivated.IBKey(device.IBKey).OwnerVisibilityDate(DateTime.UtcNow).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_CustomerID == customer.ID || t.fk_CustomerID == dealer.ID && t.fk_AssetID == asset.AssetID).ToList();
      Assert.AreNotEqual(0, svs.Count);

      foreach (var sv in svs.Where(t => t.fk_CustomerID == dealer.ID))
      {
        Assert.AreEqual(sv.StartKeyDate, DateTime.UtcNow.AddMonths(-13).KeyDate());
        Assert.AreEqual(sv.EndKeyDate, DotNetExtensions.NullKeyDate);
      }

      foreach (var sv in svs.Where(t => t.fk_CustomerID == customer.ID))
      {
        Assert.AreEqual(sv.StartKeyDate, DateTime.UtcNow.KeyDate());
        Assert.AreEqual(sv.EndKeyDate, DotNetExtensions.NullKeyDate);
      }

      message = BSS.SPUpdated.IBKey(device.IBKey).OwnerVisibilityDate(DateTime.UtcNow.AddMonths(1)).ServicePlanlineID(message.ServicePlanlineID).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_CustomerID == customer.ID || t.fk_CustomerID == dealer.ID && t.fk_AssetID == asset.AssetID).ToList();
      Assert.AreNotEqual(0, svs.Count);

      foreach (var sv in svs.Where(t => t.fk_CustomerID == customer.ID))
      {
        Assert.AreEqual(sv.StartKeyDate, DateTime.UtcNow.AddMonths(1).KeyDate());
        Assert.AreEqual(sv.EndKeyDate, DotNetExtensions.NullKeyDate);
      }
    }


    [TestMethod]
    [DatabaseTest]
    public void ServicePlanUpdated_CancelledServicePlan_Failure()
    {
        var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
        var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.TRMB).BssId(IdGen.StringId()).SyncWithRpt().Save();
        var accountDealerRel = Entity.CustomerRelationship.Relate(dealer, account).Save();

        var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).SyncWithRpt().Save();
        var accountCustomerRel = Entity.CustomerRelationship.Relate(customer, account).Save();

        var device = Entity.Device.MTS522.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(account.BSSID).SyncWithNhRaw().Save();
        var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
    
        //
        //var service = new ServiceViewAPI().CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.Series522, IdGen.StringId(), date, date, ServiceTypeEnum.Essentials);
        //DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, DeviceStateEnum.Subscribed);

        var servicePlanLineID = IdGen.StringId();
        message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(servicePlanLineID).ServicePlanName("89500-00").Build();
        result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
        Assert.IsTrue(result.Success, "Data setup failure. service activation failed");

        var date = DateTime.UtcNow;
        message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(servicePlanLineID).ServicePlanName("89500-00").ServiceTerminationDate(date).ActionUtc(date).Build();
        result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
        Assert.IsTrue(result.Success, "Data setup failure. service cancellation failed");


        message = BSS.SPUpdated.IBKey(device.IBKey).OwnerVisibilityDate(DateTime.UtcNow).ServicePlanlineID(servicePlanLineID).ServicePlanName("89500-00").Build();
        result = TestHelper.ExecuteWorkflow<ServicePlan>(message);

        Assert.IsFalse(result.Success,"Service Plan updates succeeded despite there is no active service. Expected to Fail!");

    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanUpdated_DeRegisteredDeviceWithActiveServices_Success()
    {
        var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
        var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.TRMB).BssId(IdGen.StringId()).SyncWithRpt().Save();
        var accountDealerRel = Entity.CustomerRelationship.Relate(dealer, account).Save();

        var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).SyncWithRpt().Save();
        var accountCustomerRel = Entity.CustomerRelationship.Relate(customer, account).Save();

        var device = Entity.Device.PL321.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(account.BSSID).SyncWithNhRaw().Save();
        var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

        var servicePlanLineID = IdGen.StringId();
        message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(servicePlanLineID).ServicePlanName("89500-00").Build();
        result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
        Assert.IsTrue(result.Success, "Data setup failure. service activation failed");

        var date = DateTime.UtcNow;
        message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(servicePlanLineID).ServicePlanName("89500-00").ServiceTerminationDate(date).ActionUtc(date).Build();
        result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
        Assert.IsTrue(result.Success, "Data setup failure. service cancellation failed");

        DeviceRegistration devicemessage = BSS.DRBDeRegistered.IBKey(device.IBKey).Build();
        result = TestHelper.ExecuteWorkflow<DeviceRegistration>(devicemessage);
        Assert.IsTrue(result.Success,"Data setup failure. device dereg failed");

        servicePlanLineID = IdGen.StringId();
        message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(servicePlanLineID).ServicePlanName("89500-00").Build();
        result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
        Assert.IsTrue(result.Success, "Data setup failure. service activation failed");

        message = BSS.SPUpdated.IBKey(device.IBKey).OwnerVisibilityDate(DateTime.UtcNow).ServicePlanlineID(servicePlanLineID).ServicePlanName("89500-00").Build();
        result = TestHelper.ExecuteWorkflow<ServicePlan>(message);

        Assert.IsTrue(result.Success, "Service Plan updates failed on a deregistered device with active service");

    }

 }
}