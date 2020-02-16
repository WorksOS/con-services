using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ServicePlanCancelledWorkflowTests : BssUnitTestBase
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
    public void ServicePlanCancelled_ServiceIsActiveOnAnotherDevice_Failure()
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

      message = BSS.SPCancelled.IBKey(device2.IBKey).OwnerVisibilityDate(null).ServicePlanlineID(service.BSSLineID).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.SERVICE_NOT_ASSOCIATED_WITH_DEVICE,
        service.BSSLineID, device1.GpsDeviceID, device1.IBKey));
    }

    [DatabaseTest]
    [TestMethod]
    public void ServicePlanCancelled_IBKeyDoesNotExist_Failure()
    {
      message = BSS.SPCancelled.ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.IBKEY_DOES_NOT_EXISTS, message.IBKey), "Summary is expected to contain the exception.");
    }

    [DatabaseTest]
    [TestMethod]
    public void ServicePlanCancelled_ServiceDoesNotExist_Failure()
    {
      message = BSS.SPCancelled.ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.SERVICE_DOES_NOT_EXISTS, message.ServicePlanlineID), "Summary is expected to contain the exception.");
    }

    [DatabaseTest]
    [TestMethod]
    public void ServicePlanCancelled_TerminationDatePriorToActivationDate_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).SyncWithRpt().Save();
      Entity.CustomerRelationship.Relate(customer, account).Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(account.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).WithCoreService().SyncWithRpt().Save();

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.fk_DeviceID == device.ID && t.fk_ServiceTypeID == (int)ServiceTypeEnum.Essentials).Single();
      var date = service.ActivationKeyDate.FromKeyDate();
      message = BSS.SPCancelled.ServicePlanName("89500-00").ServicePlanlineID(service.BSSLineID)
        .ServiceTerminationDate(date.AddDays(-2)).ActionUtc(DateTime.UtcNow).IBKey(device.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.SERVICE_TERMINATION_DATE_IS_PRIOR_TO_ACTIVATION_DATE,
          date.AddDays(-2).KeyDate(), date.KeyDate()), "Summary is expected to contain the exception.");
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_CancelTerminatedService_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(account.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var service = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId())
        .WithView(t => t.ForAsset(asset).ForCustomer(account)).ActivationDate(DateTime.UtcNow.AddDays(-20)).CancellationDate(DateTime.UtcNow.AddDays(-2)).SyncWithRpt().Save();

      message = BSS.SPCancelled.ServicePlanName("89500-00").IBKey(device.IBKey).ServicePlanlineID(service.BSSLineID).ServiceTerminationDate(DateTime.UtcNow).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.SERVICE_TERMINATION_NOT_VALID,
          DateTime.UtcNow.KeyDate(), service.CancellationKeyDate), "Summary is expected to contain the exception.");
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_CancelTerminatedService_WithSameCancellationDate_Failure()
    {
        var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
        var device = Entity.Device.PL321.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(account.BSSID).SyncWithNhRaw().Save();
        var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

        var service = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId())
          .WithView(t => t.ForAsset(asset).ForCustomer(account)).ActivationDate(DateTime.UtcNow.AddDays(-20)).CancellationDate(DateTime.UtcNow.AddDays(-2)).SyncWithRpt().Save();

        message = BSS.SPCancelled.ServicePlanName("89500-00").IBKey(device.IBKey).ServicePlanlineID(service.BSSLineID).ServiceTerminationDate(DateTime.UtcNow.AddDays(-2)).Build();
        result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.SERVICE_TERMINATION_NOT_VALID,
            DateTime.UtcNow.AddDays(-2).KeyDate(), service.CancellationKeyDate), "Summary is expected to contain the exception.");
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_PL321_CancelEssential_OwnedByCATDealer_Succes()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(dealer.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
      var corpCustomer = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).First();
      Assert.IsNotNull(corpCustomer);

      var date = DateTime.UtcNow;
      var service = new ServiceViewAPI().CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), date, date, ServiceTypeEnum.Essentials);
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.PL321, DeviceStateEnum.Subscribed);
      
      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(service.Item1.BSSLineID).ServiceTerminationDate(date).ActionUtc(date).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var updatedService = AssertService(deviceID: device.ID, planLineID: message.ServicePlanlineID, activationKeyDate: service.Item1.ActivationKeyDate,
            terminationKeyDate: date.KeyDate());
      //as core service plan is cancled the device state is moved to provisioned, irrespective of other plans active status.
      OPDeviceAssert(deviceID: device.ID);
      AssertRawPLDevice(device.GpsDeviceID);
      AssertServiceView(service: updatedService, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.CAT, terminationDate: date);
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_PL321_CancelEssential_OwnedByAccount_CATDealer_Succes()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(account.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
      var corpCustomer = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).First();
      Assert.IsNotNull(corpCustomer);
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var date = DateTime.UtcNow;
      var service = new ServiceViewAPI().CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), date, date, ServiceTypeEnum.Essentials);
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.PL321, DeviceStateEnum.Subscribed);

      
      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(service.Item1.BSSLineID).ServicePlanName("89500-00").ActionUtc(date).ServiceTerminationDate(date).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var updatedService = AssertService(deviceID: device.ID, planLineID: message.ServicePlanlineID, activationKeyDate: service.Item1.ActivationKeyDate,
            terminationKeyDate: date.KeyDate());
      //as core service plan is cancled the device state is moved to provisioned, irrespective of other plans active status.
      OPDeviceAssert(deviceID: device.ID);
      AssertRawPLDevice(device.GpsDeviceID);
      AssertServiceView(service: updatedService, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.CAT, terminationDate: date, accountID: account.ID);
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_MTS522_CancelEssentials_OwnedByAccount_TRMBDealer_Customer_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.TRMB).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var accountDealerRel = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var accountCustomerRel = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.MTS522.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(account.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
      var corpCustomer = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.TRMB).First();
      Assert.IsNotNull(corpCustomer);

      var date = DateTime.UtcNow;
      var service = new ServiceViewAPI().CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.Series522, IdGen.StringId(), date, date, ServiceTypeEnum.Essentials);
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, DeviceStateEnum.Subscribed);

      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(service.Item1.BSSLineID).ServicePlanName("89500-00").ServiceTerminationDate(date).ActionUtc(date).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var updatedService = AssertService(deviceID: device.ID, planLineID: message.ServicePlanlineID, activationKeyDate: service.Item1.ActivationKeyDate,
            terminationKeyDate: date.KeyDate());
      //as core service plan is cancled the device state is moved to provisioned, irrespective of other plans active status.
      OPDeviceAssert(deviceID: device.ID);
      AssertRawMTSDevice(gpsDeviceID: device.GpsDeviceID, sampleRate: ServiceType.DefaultSamplingInterval.TotalSeconds, updateRate: ServiceType.DefaultReportingInterval.TotalSeconds);
      AssertServiceView(service: updatedService, assetID: asset.AssetID, dealerID: dealer.ID,
          dealerNetwork: DealerNetworkEnum.TRMB, terminationDate: date,
          customerID: customer.ID, accountID: account.ID);
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_MTS522_CancelEssentials_OwnedByTRMBDealer_Customer_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.TRMB).BssId(IdGen.StringId()).SyncWithRpt().Save();

      var device = Entity.Device.MTS522.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(dealer.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
      var corpCustomer = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.TRMB).First();
      Assert.IsNotNull(corpCustomer);

      var date = DateTime.UtcNow;
      var service = new ServiceViewAPI().CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.Series522, IdGen.StringId(), date, date, ServiceTypeEnum.Essentials);
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, DeviceStateEnum.Subscribed);

      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(service.Item1.BSSLineID).ServiceTerminationDate(date).ActionUtc(date).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var updatedService = AssertService(deviceID: device.ID, planLineID: message.ServicePlanlineID, activationKeyDate: service.Item1.ActivationKeyDate,
            terminationKeyDate: date.KeyDate());
      //as core service plan is cancled the device state is moved to provisioned, irrespective of other plans active status.
      OPDeviceAssert(deviceID: device.ID);
      AssertRawMTSDevice(gpsDeviceID: device.GpsDeviceID, sampleRate: ServiceType.DefaultSamplingInterval.TotalSeconds, updateRate: ServiceType.DefaultReportingInterval.TotalSeconds);
      AssertServiceView(service: updatedService, assetID: asset.AssetID, dealerID: dealer.ID,
          dealerNetwork: DealerNetworkEnum.TRMB, terminationDate: date);
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_ManualDevice_CancelManualMaint_OwnerByAccount_CATDealer_Customer_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var accountDealerRel = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var accountCustomerRel = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.NoDevice.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(account.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
      var corpCustomer = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).First();
      Assert.IsNotNull(corpCustomer);

      var date = DateTime.UtcNow;
      var service = new ServiceViewAPI().CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.MANUALDEVICE, IdGen.StringId(), date, date, ServiceTypeEnum.ManualMaintenanceLog);
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.MANUALDEVICE, DeviceStateEnum.Subscribed);

      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(service.Item1.BSSLineID).ServicePlanName("89550-00").ServiceTerminationDate(date).ActionUtc(date).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var updatedService = AssertService(deviceID: device.ID, planLineID: message.ServicePlanlineID, activationKeyDate: service.Item1.ActivationKeyDate,
            terminationKeyDate: date.KeyDate(), serviceType: ServiceTypeEnum.ManualMaintenanceLog);
      AssertManualDevice(deviceID: device.ID);
      AssertServiceView(service: updatedService, assetID: asset.AssetID, dealerID: dealer.ID,
          dealerNetwork: DealerNetworkEnum.CAT, terminationDate: date,
          customerID: customer.ID, accountID: account.ID);
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_ManualDevice_CancelManualMaint_OwnerByCATDealer_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();

      var device = Entity.Device.NoDevice.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(dealer.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
      var corpCustomer = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).First();
      Assert.IsNotNull(corpCustomer);

      var date = DateTime.UtcNow;
      var service = new ServiceViewAPI().CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.MANUALDEVICE, IdGen.StringId(), date, date, ServiceTypeEnum.ManualMaintenanceLog);
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.MANUALDEVICE, DeviceStateEnum.Subscribed);

      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(service.Item1.BSSLineID).ServicePlanName("89550-00").ActionUtc(date).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var updatedService = AssertService(deviceID: device.ID, planLineID: message.ServicePlanlineID, activationKeyDate: service.Item1.ActivationKeyDate,
            terminationKeyDate: date.KeyDate(), serviceType: ServiceTypeEnum.ManualMaintenanceLog);
      AssertManualDevice(deviceID: device.ID);
      AssertServiceView(service: updatedService, assetID: asset.AssetID, dealerID: dealer.ID,
          dealerNetwork: DealerNetworkEnum.CAT, terminationDate: date);
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_ManualDevice_CancelManualMaint_OwnerByTRMBDealer_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.TRMB).BssId(IdGen.StringId()).SyncWithRpt().Save();

      var device = Entity.Device.NoDevice.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(dealer.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
      var corpCustomer = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.TRMB).First();
      Assert.IsNotNull(corpCustomer);

      var date = DateTime.UtcNow;
      var service = new ServiceViewAPI().CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.MANUALDEVICE, IdGen.StringId(), date, date, ServiceTypeEnum.ManualMaintenanceLog);
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.MANUALDEVICE, DeviceStateEnum.Subscribed);

      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(service.Item1.BSSLineID).ServicePlanName("89550-00").ActionUtc(date).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var updatedService = AssertService(deviceID: device.ID, planLineID: message.ServicePlanlineID, activationKeyDate: service.Item1.ActivationKeyDate,
            terminationKeyDate: date.KeyDate(), serviceType: ServiceTypeEnum.ManualMaintenanceLog);
      AssertManualDevice(deviceID: device.ID);
      AssertServiceView(service: updatedService, assetID: asset.AssetID, dealerID: dealer.ID,
          dealerNetwork: DealerNetworkEnum.TRMB, terminationDate: date);
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_MTS522_CancelUtilization_OwnedByCATDealer_EssentialsActive_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();

      var device = Entity.Device.MTS522.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(dealer.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
      var corpCustomer = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).First();
      Assert.IsNotNull(corpCustomer);

      var date = DateTime.UtcNow;
      var api = new ServiceViewAPI();
      var essentials = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.Series522, IdGen.StringId(), date, date, ServiceTypeEnum.Essentials);
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, true, ServiceTypeEnum.Essentials, new List<DeviceConfig.ServicePlanIDs> { new DeviceConfig.ServicePlanIDs { IsCore = true, PlanID = (int)ServiceTypeEnum.Essentials } });
      var utilization = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.Series522, IdGen.StringId(), date, date, ServiceTypeEnum.CATUtilization);
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, true, ServiceTypeEnum.CATUtilization, new List<DeviceConfig.ServicePlanIDs> { new DeviceConfig.ServicePlanIDs { IsCore = false, PlanID = (int)ServiceTypeEnum.CATUtilization } });
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, DeviceStateEnum.Subscribed);

      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(utilization.Item1.BSSLineID).ServiceTerminationDate(date).ActionUtc(date).ServicePlanName("89220-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = AssertService(deviceID: device.ID, planLineID: message.ServicePlanlineID, activationKeyDate: essentials.Item1.ActivationKeyDate,
            terminationKeyDate: date.KeyDate(), serviceType: ServiceTypeEnum.CATUtilization);
      OPDeviceAssert(deviceID: device.ID, isBlackListed: false);
      AssertRawMTSDevice(device.GpsDeviceID, sampleRate: ServiceType.DefaultSamplingInterval.TotalSeconds, updateRate: ServiceType.DefaultReportingInterval.TotalSeconds, isBlackListed: false);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID,
          dealerNetwork: DealerNetworkEnum.CAT, terminationDate: date);
      AssertActiveService(deviceID: device.ID, activationKeyDate: essentials.Item1.ActivationKeyDate);
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_CancelUtilization_MTS522_WithAsset_CATDealer_OwnerByAccount_SITECHParentDealer_WithEssentialsActive_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var dealer_account_rel = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var parentDealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.SITECH).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var parentdealer_dealer_rel = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device = Entity.Device.MTS522.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var date = DateTime.UtcNow;
      var api = new ServiceViewAPI();
      var essentials = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.Series522, IdGen.StringId(), date, date, ServiceTypeEnum.Essentials);
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, true, ServiceTypeEnum.Essentials, new List<DeviceConfig.ServicePlanIDs> { new DeviceConfig.ServicePlanIDs { IsCore = true, PlanID = (int)ServiceTypeEnum.Essentials } });
      var utilization = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.Series522, IdGen.StringId(), date, date, ServiceTypeEnum.CATUtilization);
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, true, ServiceTypeEnum.CATUtilization, new List<DeviceConfig.ServicePlanIDs> { new DeviceConfig.ServicePlanIDs { IsCore = false, PlanID = (int)ServiceTypeEnum.CATUtilization } });
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, DeviceStateEnum.Subscribed);

      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(utilization.Item1.BSSLineID).ServiceTerminationDate(date).ActionUtc(date).ServicePlanName("89220-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      var service = AssertService(deviceID: device.ID, planLineID: message.ServicePlanlineID, activationKeyDate: essentials.Item1.ActivationKeyDate,
      terminationKeyDate: date.KeyDate(), serviceType: ServiceTypeEnum.CATUtilization);
      OPDeviceAssert(deviceID: device.ID, isBlackListed: false);
      AssertRawMTSDevice(device.GpsDeviceID, sampleRate: ServiceType.DefaultSamplingInterval.TotalSeconds, updateRate: ServiceType.DefaultReportingInterval.TotalSeconds, isBlackListed: false);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID,
      dealerNetwork: DealerNetworkEnum.CAT, terminationDate: date, accountID: account.ID, parentDealerNetwork: DealerNetworkEnum.SITECH);
      AssertActiveService(deviceID: device.ID, activationKeyDate: essentials.Item1.ActivationKeyDate);
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_MTS522_CancelRapidReporting_OwnedByCATDealer_ActiveEssentialsAndUtilization_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var delaer_account_rel = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var customer_account_rel = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.MTS522.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(account.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
      var corpCustomer = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).First();
      Assert.IsNotNull(corpCustomer);

      var date = DateTime.UtcNow;
      var api = new ServiceViewAPI();

      var essentials = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.Series522, IdGen.StringId(), date, date, ServiceTypeEnum.Essentials);
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, true, ServiceTypeEnum.Essentials, new List<DeviceConfig.ServicePlanIDs> { new DeviceConfig.ServicePlanIDs { IsCore = true, PlanID = (int)ServiceTypeEnum.Essentials } });

      var utilization = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.Series522, IdGen.StringId(), date, date, ServiceTypeEnum.CATUtilization);
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, true, ServiceTypeEnum.CATUtilization, new List<DeviceConfig.ServicePlanIDs> { new DeviceConfig.ServicePlanIDs { IsCore = false, PlanID = (int)ServiceTypeEnum.CATUtilization } });

      var rapidreporting = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.Series522, IdGen.StringId(), date, date, ServiceTypeEnum.e1minuteUpdateRateUpgrade);
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, true, ServiceTypeEnum.e1minuteUpdateRateUpgrade, new List<DeviceConfig.ServicePlanIDs> { new DeviceConfig.ServicePlanIDs { IsCore = false, PlanID = (int)ServiceTypeEnum.e1minuteUpdateRateUpgrade } });

      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, DeviceStateEnum.Subscribed);

      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(rapidreporting.Item1.BSSLineID).ServiceTerminationDate(date).ActionUtc(date).ServicePlanName("89540-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = AssertService(deviceID: device.ID, planLineID: message.ServicePlanlineID, activationKeyDate: essentials.Item1.ActivationKeyDate,
            terminationKeyDate: date.KeyDate(), serviceType: ServiceTypeEnum.e1minuteUpdateRateUpgrade);
      OPDeviceAssert(deviceID: device.ID, isBlackListed: false);
      AssertRawMTSDevice(device.GpsDeviceID, sampleRate: ServiceType.PerformanceSamplingInterval.TotalSeconds, updateRate: ServiceType.PerformanceReportingInterval.TotalSeconds, isBlackListed: false);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID,
      dealerNetwork: DealerNetworkEnum.CAT, terminationDate: date, accountID: account.ID, customerID: customer.ID);
      AssertActiveService(deviceID: device.ID, activationKeyDate: essentials.Item1.ActivationKeyDate);
      AssertActiveService(deviceID: device.ID, activationKeyDate: rapidreporting.Item1.ActivationKeyDate, serviceType: ServiceTypeEnum.CATUtilization);
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_PL321_CancelUtilization_OwnedByCATDealer_EssentialsActive_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(dealer.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
      var corpCustomer = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).First();
      Assert.IsNotNull(corpCustomer);

      var date = DateTime.UtcNow;
      var api = new ServiceViewAPI();
      var essentials = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), date, date, ServiceTypeEnum.Essentials);
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.PL321, true, ServiceTypeEnum.Essentials, new List<DeviceConfig.ServicePlanIDs> { new DeviceConfig.ServicePlanIDs { IsCore = true, PlanID = (int)ServiceTypeEnum.Essentials } });
      var utilization = api.CreateServiceAndServiceViews(Ctx.OpContext, device.ID, DeviceTypeEnum.PL321, IdGen.StringId(), date, date, ServiceTypeEnum.CATUtilization);
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.PL321, true, ServiceTypeEnum.CATUtilization, new List<DeviceConfig.ServicePlanIDs> { new DeviceConfig.ServicePlanIDs { IsCore = false, PlanID = (int)ServiceTypeEnum.CATUtilization } });
      DeviceConfig.UpdateDeviceState(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.PL321, DeviceStateEnum.Subscribed);

      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(utilization.Item1.BSSLineID).ServiceTerminationDate(date).ActionUtc(date).ServicePlanName("89220-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = AssertService(deviceID: device.ID, planLineID: message.ServicePlanlineID, activationKeyDate: essentials.Item1.ActivationKeyDate,
            terminationKeyDate: date.KeyDate(), serviceType: ServiceTypeEnum.CATUtilization);
      OPDeviceAssert(deviceID: device.ID, isBlackListed: false);
      AssertRawPLDevice(device.GpsDeviceID, isBlackListed: false);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID,
          dealerNetwork: DealerNetworkEnum.CAT, terminationDate: date);
      AssertActiveService(deviceID: device.ID, activationKeyDate: essentials.Item1.ActivationKeyDate);
    }

    [TestMethod]
    [DatabaseTest]
    public void ServicePlanCancelled_CancelEssential_PL321_NoAsset_OwnedByCATDealer_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).OwnerBssId(dealer.BSSID).SyncWithNhRaw().Save();
      var corpCustomer = Ctx.OpContext.CustomerReadOnly.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && t.fk_DealerNetworkID == (int)DealerNetworkEnum.CAT).First();
      Assert.IsNotNull(corpCustomer);

      var date = DateTime.UtcNow;
      var essentials = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId()).OwnerVisibilityDate(date).ActivationDate(date).Save();
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, device.GpsDeviceID, DeviceTypeEnum.Series522, true, ServiceTypeEnum.Essentials, new List<DeviceConfig.ServicePlanIDs> { new DeviceConfig.ServicePlanIDs { IsCore = true, PlanID = (int)ServiceTypeEnum.Essentials } });

      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(essentials.BSSLineID).ServiceTerminationDate(date).ActionUtc(date).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      AssertService(deviceID: device.ID, planLineID: message.ServicePlanlineID, activationKeyDate: essentials.ActivationKeyDate, terminationKeyDate: date.KeyDate(), serviceType: ServiceTypeEnum.Essentials);
    }

    [DatabaseTest]
    [TestMethod]
    public void SPCancelled_Utilization_DeviceStateNotModified()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName(TestHelper.GetPartNumberByServiceType(ServiceTypeEnum.Essentials)).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      var planlineID = IdGen.StringId();
      var partNumber = TestHelper.GetPartNumberByServiceType(ServiceTypeEnum.CATUtilization);
      message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(planlineID).ServicePlanName(partNumber).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(planlineID).ServicePlanName(partNumber).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      OPDeviceAssert(device.ID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void SPCancelled_Essentials_DeviceStateSetToProvisioned()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var planlineID = IdGen.StringId();
      var partNumber = TestHelper.GetPartNumberByServiceType(ServiceTypeEnum.Essentials);
      message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(planlineID).ServicePlanName(partNumber).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName(TestHelper.GetPartNumberByServiceType(ServiceTypeEnum.CATUtilization)).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      message = BSS.SPCancelled.IBKey(device.IBKey).ServicePlanlineID(planlineID).ServicePlanName(partNumber).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      OPDeviceAssert(device.ID, true);
    }

    #region Assertion Helper Methods

    private Service AssertService(long deviceID, string planLineID, int activationKeyDate, int terminationKeyDate, ServiceTypeEnum serviceType = ServiceTypeEnum.Essentials)
    {
      //service assertion
      var service = (from s in Ctx.OpContext.ServiceReadOnly where s.fk_DeviceID == deviceID && s.fk_ServiceTypeID == (int)serviceType && s.BSSLineID == planLineID select s).SingleOrDefault();
      Assert.IsNotNull(service);
      Assert.AreEqual(activationKeyDate, service.ActivationKeyDate);
      Assert.AreEqual(terminationKeyDate, service.CancellationKeyDate);
      Assert.AreEqual(activationKeyDate, service.OwnerVisibilityKeyDate);
      return service;
    }

    private void AssertActiveService(long deviceID, int activationKeyDate, ServiceTypeEnum serviceType = ServiceTypeEnum.Essentials)
    {
      var service = (from s in Ctx.OpContext.ServiceReadOnly where s.fk_DeviceID == deviceID && s.fk_ServiceTypeID == (int)serviceType select s).SingleOrDefault();
      Assert.IsNotNull(service);
      Assert.AreEqual(activationKeyDate, service.ActivationKeyDate);
      Assert.AreEqual(DotNetExtensions.NullKeyDate, service.CancellationKeyDate);
      Assert.AreEqual(activationKeyDate, service.OwnerVisibilityKeyDate);

      var serviceViews = (from sv in Ctx.OpContext.ServiceViewReadOnly where sv.fk_ServiceID == service.ID select new { sv, sv.Customer.fk_CustomerTypeID }).ToList();
      Assert.AreNotEqual(0, serviceViews.Count);

      var customerServiceViews = serviceViews.Where(t => t.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer).Select(t => t.sv).ToList();
      var dealerServiceViews = serviceViews.Where(t => t.fk_CustomerTypeID != (int)CustomerTypeEnum.Customer).Select(t => t.sv).ToList();

      foreach (var serviceView in customerServiceViews)
      {
        Assert.AreEqual(service.OwnerVisibilityKeyDate, serviceView.StartKeyDate);
        Assert.AreEqual(DotNetExtensions.NullKeyDate, serviceView.EndKeyDate);
      }

      foreach (var serviceView in dealerServiceViews)
      {
        Assert.AreEqual(activationKeyDate.FromKeyDate().AddMonths(-13).KeyDate(), serviceView.StartKeyDate);
        Assert.AreEqual(DotNetExtensions.NullKeyDate, serviceView.EndKeyDate);
      }
    }

    private void OPDeviceAssert(long deviceID, bool isBlackListed = true)
    {
      //nh_op device assertion
      var opDevice = (from d in Ctx.OpContext.DeviceReadOnly where d.ID == deviceID select d).SingleOrDefault();
      Assert.IsNotNull(opDevice);
      if (isBlackListed)
        Assert.AreEqual((int)DeviceStateEnum.Provisioned, opDevice.fk_DeviceStateID);
      else
        Assert.AreEqual((int)DeviceStateEnum.Subscribed, opDevice.fk_DeviceStateID);
    }

    private void AssertManualDevice(long deviceID)
    {
      var device = (from d in Ctx.OpContext.DeviceReadOnly where d.ID == deviceID select d).SingleOrDefault();
      Assert.IsNotNull(device);
      Assert.AreEqual((int)DeviceStateEnum.Provisioned, device.fk_DeviceStateID);
    }

    private void AssertRawPLDevice(string gpsDeviceID, bool isBlackListed = true)
    {
      //nh_raw device assertion
      var rawDevice = (from d in Ctx.RawContext.PLDeviceReadOnly where d.ModuleCode == gpsDeviceID select d).SingleOrDefault();
      Assert.IsNotNull(rawDevice);
    }

    private void AssertRawMTSDevice(string gpsDeviceID, double sampleRate, double updateRate, bool isBlackListed = true)
    {
      //nh_raw device assertion
      var rawDevice = (from d in Ctx.RawContext.MTSDeviceReadOnly where d.SerialNumber == gpsDeviceID select d).SingleOrDefault();
      Assert.IsNotNull(rawDevice);

      //assert reporting intervals for core
      Assert.AreEqual(sampleRate, rawDevice.SampleRate);
      Assert.AreEqual(updateRate, rawDevice.UpdateRate);
    }

    private void AssertServiceView(Service service, long assetID, long dealerID, DealerNetworkEnum dealerNetwork, DateTime terminationDate, long? customerID = null, long? accountID = null, DealerNetworkEnum? parentDealerNetwork = null)
    {
      var activationkeyDate = service.ActivationKeyDate.FromKeyDate().AddMonths(-13).KeyDate();
      var cancellatoinkeyDate = terminationDate.KeyDate();

      //assert service views
      var serviceViews = (from sv in Ctx.OpContext.ServiceViewReadOnly where sv.fk_ServiceID == service.ID && sv.fk_AssetID == assetID && sv.fk_ServiceID == service.ID select sv).ToList();
      var corpCustomer = (from c in Ctx.OpContext.CustomerReadOnly where c.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && c.fk_DealerNetworkID == (int)dealerNetwork select c).SingleOrDefault();

      //assert service views for dealer
      var dealerServiceView = serviceViews.Where(t => t.fk_CustomerID == dealerID).SingleOrDefault();
      Assert.IsNotNull(dealerServiceView);
      Assert.AreEqual(dealerServiceView.StartKeyDate, activationkeyDate);
      Assert.AreEqual(dealerServiceView.EndKeyDate, cancellatoinkeyDate);

      //assert service views for corp dealer
      var corpServiceView = serviceViews.Where(t => t.fk_CustomerID == corpCustomer.ID).SingleOrDefault();
      Assert.IsNotNull(corpServiceView);
      Assert.AreEqual(corpServiceView.StartKeyDate, activationkeyDate);
      Assert.AreEqual(corpServiceView.EndKeyDate, cancellatoinkeyDate);

      if (customerID.HasValue)
      {
        var customerServiceView = serviceViews.Where(t => t.fk_CustomerID == customerID).SingleOrDefault();
        Assert.IsNotNull(customerServiceView);
        Assert.AreEqual(customerServiceView.StartKeyDate, service.OwnerVisibilityKeyDate.Value);
        Assert.AreEqual(customerServiceView.EndKeyDate, cancellatoinkeyDate);
      }

      if (accountID.HasValue)
      {
        var accountServiceViews = serviceViews.Where(t => t.fk_CustomerID == accountID).SingleOrDefault();
        Assert.IsNull(accountServiceViews);
      }

      //assert grand parent service views
      if (parentDealerNetwork.HasValue)
      {
        var parentCorpDealer = (from c in Ctx.OpContext.CustomerReadOnly where c.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && c.fk_DealerNetworkID == (int)parentDealerNetwork select c).SingleOrDefault();
        var parentCorpServiceView = serviceViews.Where(t => t.fk_CustomerID == parentCorpDealer.ID).SingleOrDefault();
        Assert.IsNotNull(parentCorpServiceView);
        Assert.AreEqual(parentCorpServiceView.StartKeyDate, activationkeyDate);
        Assert.AreEqual(parentCorpServiceView.EndKeyDate, cancellatoinkeyDate);
      }
    }

    #endregion
  }
}
