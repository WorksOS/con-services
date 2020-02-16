using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ServicePlanActivatedWorkflowTests : BssUnitTestBase
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

    [DatabaseTest]
    [TestMethod]
    public void SP_IBKeyDoesNotExist_Failure()
    {
      message = BSS.SPActivated.ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.IBKEY_DOES_NOT_EXISTS, message.IBKey), "Summary is expected to contain the exception.");
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_ServicePlanNameDoesNotExist_Failure()
    {
      message = BSS.SPActivated.Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.SERVICE_TYPE_DOES_NOT_EXISTS, message.ServicePlanName), "Summary is expected to contain the exception.");
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_ServicePlanLineIDExists_Failure()
    {
      var servicePlan = Entity.Service.Essentials.BssPlanLineId(IdGen.StringId()).Save();
      message = BSS.SPActivated.ServicePlanlineID(servicePlan.BSSLineID).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.SERVICE_EXISTS, message.ServicePlanlineID), "Summary is expected to contain the exception.");
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_ServiceTerminatedDateDefined_Failure()
    {
      var message = BSS.SPActivated.ServiceTerminationDate(DateTime.UtcNow).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.SERVICE_TERMINATION_DATE, string.Empty, message.Action), "Summary is expected to contain the exception.");
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_ManualMain_PL321_Failure()
    {
      var device = Entity.Device.PL321.OwnerBssId(IdGen.StringId()).IbKey(IdGen.StringId()).Save();
      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanName("89550-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.SERVICE_TYPE_NOT_SUPPORTED_FOR_DEVICE_TYPE, ServiceTypeEnum.ManualMaintenanceLog, DeviceTypeEnum.PL321), "Summary is expected to contain the exception.");
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_ManualDevice_Failure()
    {
      var device = Entity.Device.NoDevice.OwnerBssId(IdGen.StringId()).IbKey(IdGen.StringId()).Save();
      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.SERVICE_TYPE_NOT_SUPPORTED_FOR_DEVICE_TYPE, ServiceTypeEnum.Essentials, DeviceTypeEnum.MANUALDEVICE), "Summary is expected to contain the exception.");
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_ManualMaint_PL321_CATDealer_Failure()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var device = Entity.Device.PL321.OwnerBssId(account.BSSID).IbKey(IdGen.StringId()).Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanName("89550-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.SERVICE_TYPE_NOT_SUPPORTED_FOR_DEVICE_TYPE, ServiceTypeEnum.ManualMaintenanceLog, DeviceTypeEnum.PL321), "Summary is expected to contain the exception.");
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_RapidReporting_PL121_CATDealer_Failure()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var device = Entity.Device.PL121.OwnerBssId(account.BSSID).IbKey(IdGen.StringId()).Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanName("89540-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.SERVICE_TYPE_NOT_SUPPORTED_FOR_DEVICE_TYPE, ServiceTypeEnum.e1minuteUpdateRateUpgrade, DeviceTypeEnum.PL121), "Summary is expected to contain the exception.");
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_PL321_Essentials_SameServiceTerminatedInThePast_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.OwnerBssId(account.BSSID).IbKey(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId()).ActivationDate(DateTime.UtcNow.AddDays(-30)).CancellationDate(DateTime.UtcNow).Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.fk_DeviceID == device.ID && t.BSSLineID == message.ServicePlanlineID).Single();
      Assert.AreEqual(service.ActivationKeyDate, DateTime.UtcNow.KeyDate());
      Assert.AreEqual(service.CancellationKeyDate, DotNetExtensions.NullKeyDate);
      Assert.AreEqual(service.OwnerVisibilityKeyDate, DateTime.UtcNow.KeyDate());
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_PL321_Essentials_SameServiceTerminatedToday_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.OwnerBssId(account.BSSID).IbKey(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId()).ActivationDate(DateTime.UtcNow.AddDays(-30)).CancellationDate(DateTime.UtcNow).Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.fk_DeviceID == device.ID && t.BSSLineID == message.ServicePlanlineID).Single();
      Assert.AreEqual(service.ActivationKeyDate, DateTime.UtcNow.KeyDate());
      Assert.AreEqual(service.CancellationKeyDate, DotNetExtensions.NullKeyDate);
      Assert.AreEqual(service.OwnerVisibilityKeyDate, DateTime.UtcNow.KeyDate());
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_PL321_Essentials_SameServiceValidTillTomorrow_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.OwnerBssId(account.BSSID).IbKey(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId()).ActivationDate(DateTime.UtcNow.AddDays(-30)).CancellationDate(DateTime.UtcNow.AddDays(30)).Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.DEVICE_HAS_SAME_ACTIVE_SERVICE, ServiceTypeEnum.Essentials, device.IBKey, essentials.BSSLineID), "Summary is expected to contain the exception.");
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_PL321_Essentials_SameServiceValidIntoFuture_Failure()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.OwnerBssId(account.BSSID).IbKey(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId()).ActivationDate(DateTime.UtcNow.AddDays(-30)).CancellationDate(DateTime.UtcNow.AddDays(30)).Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.DEVICE_HAS_SAME_ACTIVE_SERVICE, ServiceTypeEnum.Essentials, device.IBKey, essentials.BSSLineID), "Summary is expected to contain the exception.");
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_PL321_NoAsset_OwnedByCATDealer_Failure()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(dealer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      var result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ServicePlan.DEVICE_NOT_ASSOCIATED_WITH_ASSET, message.IBKey));
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_Series521_WithAsset_TRMBDealer_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.TRMB).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var device = Entity.Device.MTS522.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = AssertService(device.ID, message, ServiceTypeEnum.Essentials);
      OPDeviceAssert(device.ID, ServiceTypeEnum.Essentials);
      AssertRawMTSDevice(device.GpsDeviceID, sampleRate: ServiceType.DefaultSamplingInterval.TotalSeconds, updateRate: ServiceType.DefaultReportingInterval.TotalSeconds);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.TRMB, accountID: account.ID);
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_PL321_WithAsset_CATDealer_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = AssertService(device.ID, message, ServiceTypeEnum.Essentials);
      OPDeviceAssert(device.ID, ServiceTypeEnum.Essentials);
      AssertRawPLDevice(device.GpsDeviceID);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.CAT, accountID: account.ID);
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_Series521_WithAsset_OwnerByTRMBDealer_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.TRMB).BssId(IdGen.StringId()).SyncWithRpt().Save();

      var device = Entity.Device.MTS522.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = AssertService(device.ID, message, ServiceTypeEnum.Essentials);
      OPDeviceAssert(device.ID, ServiceTypeEnum.Essentials);
      AssertRawMTSDevice(device.GpsDeviceID, sampleRate: ServiceType.DefaultSamplingInterval.TotalSeconds, updateRate: ServiceType.DefaultReportingInterval.TotalSeconds);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.TRMB);
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_PL321_WithAsset_OwnerByCATDealer_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = AssertService(device.ID, message);
      OPDeviceAssert(device.ID);
      AssertRawPLDevice(device.GpsDeviceID);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.CAT);
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_Series521_WithAsset_TRMBDealer_Customer_OwnerByAccount_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.TRMB).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var accDealerRel = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var accCustRel = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.MTS522.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = AssertService(device.ID, message);
      OPDeviceAssert(device.ID);
      AssertRawMTSDevice(device.GpsDeviceID, sampleRate: ServiceType.DefaultSamplingInterval.TotalSeconds, updateRate: ServiceType.DefaultReportingInterval.TotalSeconds);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.TRMB, customerID: customer.ID, accountID: account.ID);
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_ManualMaint_ManualDevice_CATDealer_WithAsset_OwnerByAccount_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var device = Entity.Device.NoDevice.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89550-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = AssertService(device.ID, message, ServiceTypeEnum.ManualMaintenanceLog);
      OPDeviceAssert(device.ID, ServiceTypeEnum.ManualMaintenanceLog);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.CAT, accountID: account.ID);
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_ManualMaint_ManualDevice_WithAsset_OwnerByTRMBDealer_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.TRMB).BssId(IdGen.StringId()).SyncWithRpt().Save();

      var device = Entity.Device.NoDevice.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89550-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = AssertService(device.ID, message, ServiceTypeEnum.ManualMaintenanceLog);
      OPDeviceAssert(device.ID, ServiceTypeEnum.ManualMaintenanceLog);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.TRMB);
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Utilization_MTS522_WithAsset_OwnerByCATDealer_WithActiveEssentialsService_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();

      var device = Entity.Device.MTS522.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).WithView(t => t.ForAsset(asset).ForCustomer(dealer)).SyncWithRpt().Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89220-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      var service = AssertService(device.ID, message, ServiceTypeEnum.CATUtilization);
      OPDeviceAssert(device.ID, ServiceTypeEnum.Essentials);
      AssertRawMTSDevice(device.GpsDeviceID, sampleRate: ServiceType.PerformanceSamplingInterval.TotalSeconds, updateRate: ServiceType.PerformanceReportingInterval.TotalSeconds);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.CAT);
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Utilization_MTS522_WithAsset_CATDealer_OwnerByAccount_SITECHParentDealer_WithActiveEssentialsService_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var parentDealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.SITECH).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var accDealerRel = Entity.CustomerRelationship.Relate(dealer, account).Save();
      var delaerParentDealerRel = Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device = Entity.Device.MTS522.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).WithView(t => t.ForAsset(asset).ForCustomer(dealer)).SyncWithRpt().Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89220-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      var service = AssertService(device.ID, message, ServiceTypeEnum.CATUtilization);
      OPDeviceAssert(device.ID, ServiceTypeEnum.CATUtilization);
      AssertRawMTSDevice(device.GpsDeviceID, sampleRate: ServiceType.PerformanceSamplingInterval.TotalSeconds, updateRate: ServiceType.PerformanceReportingInterval.TotalSeconds);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.CAT, accountID: account.ID, parentDealerNetwork: DealerNetworkEnum.SITECH);
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_RapidReporting_MTS522_WithAsset_TRMBDealer_Customer_OwnerByAccount_WithActiveEssentialsAndUtiliation_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.TRMB).BssId(IdGen.StringId()).SyncWithRpt().Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var rel = Entity.CustomerRelationship.Relate(customer, account).Save();
      var relationship = Entity.CustomerRelationship.Relate(dealer, account).Save();

      var device = Entity.Device.MTS522.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).WithView(t => t.ForAsset(asset).ForCustomer(dealer)).SyncWithRpt().Save();
      var utilization = Entity.Service.Utilization.ForDevice(device).WithView(t => t.ForAsset(asset).ForCustomer(dealer)).SyncWithRpt().Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89540-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      var service = AssertService(device.ID, message, ServiceTypeEnum.e1minuteUpdateRateUpgrade);
      OPDeviceAssert(device.ID, ServiceTypeEnum.e1minuteUpdateRateUpgrade);
      AssertRawMTSDevice(device.GpsDeviceID, sampleRate: ServiceType.OneMinuteSamplingInterval.TotalSeconds, updateRate: ServiceType.TenMinuteReportingInterval.TotalSeconds);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.TRMB, accountID: account.ID, customerID: customer.ID);
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Utilization_PL321_WithAsset_OwnedByCATDealer_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89220-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = AssertService(device.ID, message, ServiceTypeEnum.CATUtilization);
      OPDeviceAssert(device.ID, ServiceTypeEnum.CATUtilization);
      AssertRawPLDevice(device.GpsDeviceID);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.CAT);
    }

    [TestMethod]
    [DatabaseTest]
    public void SP_Essentials_PL321_WithAsset_OwnedByCATDealer_WithCancelledEssentials_Success()
    {
      var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var essentials = Entity.Service.Essentials.ForDevice(device).ActivationDate(DateTime.UtcNow.AddMonths(-13))
        .WithView(t => t.ForAsset(asset).ForCustomer(dealer)).SyncWithRpt().Save();

      new ServiceViewAPI().TerminateService(Ctx.OpContext, essentials.BSSLineID, DateTime.UtcNow.AddDays(-2));

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);
      var service = AssertService(device.ID, message, ServiceTypeEnum.Essentials);
      OPDeviceAssert(device.ID, ServiceTypeEnum.Essentials);
      AssertRawPLDevice(device.GpsDeviceID);
      AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.CAT);
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

      message = BSS.SPActivated.IBKey(device.IBKey).OwnerVisibilityDate(DateTime.UtcNow.AddMonths(-13)).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_CustomerID == customer.ID || t.fk_CustomerID == dealer.ID && t.fk_AssetID == asset.AssetID).ToList();
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

      message = BSS.SPActivated.IBKey(device.IBKey).OwnerVisibilityDate(DateTime.UtcNow.AddMonths(-11)).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      var svs = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_CustomerID == customer.ID || t.fk_CustomerID == dealer.ID && t.fk_AssetID == asset.AssetID).ToList();
      Assert.AreNotEqual(0, svs.Count);

      foreach (var sv in svs.Where(t=>t.fk_CustomerID == dealer.ID))
      {
        Assert.AreEqual(sv.StartKeyDate, DateTime.UtcNow.AddMonths(-13).KeyDate());
        Assert.AreEqual(sv.EndKeyDate, DotNetExtensions.NullKeyDate);
      }

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

      message = BSS.SPActivated.IBKey(device.IBKey).OwnerVisibilityDate(DateTime.UtcNow.AddMonths(1)).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
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
        Assert.AreEqual(sv.StartKeyDate, DateTime.UtcNow.AddMonths(1).KeyDate());
        Assert.AreEqual(sv.EndKeyDate, DotNetExtensions.NullKeyDate);
      }
    }

    [DatabaseTest]
    [TestMethod]
    public void ManualDevice_ActivateCore_UpdateStateToSubscribed_Success()
    {
      var svHelper = new ServiceViewAPITestHelper();
      svHelper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_CatParentDealer();
      var manualDevice = Entity.Device.NoDevice.IbKey(IdGen.StringId())
        .OwnerBssId(svHelper.Dealer.BSSID).DeviceState(DeviceStateEnum.Provisioned).Save();
      Entity.Asset.WithDevice(manualDevice).Save();

      var ndCore = TestHelper.GetPartNumberByServiceType(ServiceTypeEnum.ManualMaintenanceLog);

      var message = BSS.SPActivated.IBKey(manualDevice.IBKey).ServicePlanName(ndCore).Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Workflow failed.");

      var device = (Ctx.OpContext.DeviceReadOnly.FirstOrDefault(x => x.IBKey == manualDevice.IBKey));
      Assert.AreEqual((int)DeviceStateEnum.Subscribed, device.fk_DeviceStateID, "DeviceState not equal.");
    }

    [DatabaseTest]
    [TestMethod]
    public void SP_Utilization_DeviceStateNotSetToSubscribed()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName(TestHelper.GetPartNumberByServiceType(ServiceTypeEnum.CATUtilization)).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      var service = AssertService(device.ID, message, ServiceTypeEnum.CATUtilization);
      OPDeviceAssert(device.ID, ServiceTypeEnum.CATUtilization);
    }

    [DatabaseTest]
    [TestMethod]
    public void SP_Essentials_DeviceStateSetToSubscribed()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName(TestHelper.GetPartNumberByServiceType(ServiceTypeEnum.Essentials)).Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
      Assert.IsTrue(result.Success);

      var service = AssertService(device.ID, message, ServiceTypeEnum.Essentials);
      OPDeviceAssert(device.ID, ServiceTypeEnum.Essentials);
    }

    [DatabaseTest]
    [TestMethod]
    public void SP_Essentials_ServicePlanAlreadyExists_Error()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(customer, account).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(dealer, account).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
      Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var essentials = Entity.Service.Essentials.ForDevice(device).SyncWithRpt().Save();

      message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName(TestHelper.GetPartNumberByServiceType(ServiceTypeEnum.Essentials)).Build();
      result = TestHelper.ExecuteWorkflow(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary,
                            string.Format(BssConstants.ServicePlan.DEVICE_HAS_SAME_ACTIVE_SERVICE,
                                          ServiceTypeEnum.Essentials, message.IBKey,
                                          essentials.BSSLineID));
    }

    //[TestMethod]
    //[DatabaseTest]
    //public void SP_Essentials_PL321_WithAsset_OwnerByCATDealer_WithCancelledEssentials_WithDefferentCATDealer_Success()
    //{
    //  var dealer = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.CAT).BssId(IdGen.StringId()).SyncWithRpt().Save();

    //  var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).SyncWithNhRaw().Save();
    //  var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

    //  var essentials = Entity.Service.Essentials.ForDevice(device).ActivationDate(DateTime.UtcNow.AddMonths(-13))
    //    .WithView(t => t.ForAsset(asset).ForCustomer(dealer)).SyncWithRpt().Save();

    //  new ServiceViewAPI().TerminateService(Ctx.OpContext, essentials.BSSLineID, DateTime.UtcNow.AddDays(-2));

    //  var newDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).SyncWithRpt().Save();
    //  Services.Devices().TransferOwnership(device.ID, newDealer.BSSID);
    //  Ctx.OpContext.SaveChanges();

    //  var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
    //  result = TestHelper.ExecuteWorkflow<ServicePlan>(message);
    //  Assert.IsTrue(result.Success);
    //  var service = AssertService(device.ID, message, ServiceTypeEnum.Essentials);
    //  OPDeviceAssert(device.ID, ServiceTypeEnum.Essentials);
    //  RawPLDeviceAssert(device.GpsDeviceID);
    //  AssertServiceView(service: service, assetID: asset.AssetID, dealerID: dealer.ID, dealerNetwork: DealerNetworkEnum.CAT);
      
    //}

    #region Assertion Helper Methods

    private Service AssertService(long deviceID, ServicePlan message, ServiceTypeEnum serviceType = ServiceTypeEnum.Essentials)
    {
      //service assertion
      var service = (from s in Ctx.OpContext.ServiceReadOnly where s.fk_DeviceID == deviceID && s.fk_ServiceTypeID == (int)serviceType && s.BSSLineID == message.ServicePlanlineID select s).SingleOrDefault();
      Assert.IsNotNull(service);
      Assert.AreEqual(Convert.ToDateTime(message.ActionUTC).KeyDate(), service.ActivationKeyDate);
      Assert.AreEqual(DotNetExtensions.NullKeyDate, service.CancellationKeyDate);
      Assert.AreEqual(DateTime.UtcNow.KeyDate(), service.OwnerVisibilityKeyDate);
      return service;
    }

    private void OPDeviceAssert(long deviceID, ServiceTypeEnum serviceType = ServiceTypeEnum.Essentials)
    {
      //nh_op device assertion
      var opDevice = (from d in Ctx.OpContext.DeviceReadOnly where d.ID == deviceID select d).SingleOrDefault();
      Assert.IsNotNull(opDevice);
      if (serviceType == ServiceTypeEnum.ManualMaintenanceLog || serviceType == ServiceTypeEnum.Essentials)
        Assert.AreEqual(opDevice.fk_DeviceStateID, (int)DeviceStateEnum.Subscribed);
      else
        Assert.AreEqual(opDevice.fk_DeviceStateID, (int)DeviceStateEnum.Provisioned);
    }

    private void AssertRawMTSDevice(string gpsDeviceID, double sampleRate, double updateRate)
    {
      //nh_raw device assertion
      var rawDevice = (from d in Ctx.RawContext.MTSDeviceReadOnly where d.SerialNumber == gpsDeviceID select d).SingleOrDefault();
      Assert.IsNotNull(rawDevice);
      
      //assert reporting intervals for core
      Assert.AreEqual(sampleRate, rawDevice.SampleRate);
      Assert.AreEqual(updateRate, rawDevice.UpdateRate);
    }

    private void AssertRawPLDevice(string gpsDeviceID)
    {
      //nh_raw device assertion
      var rawDevice = (from d in Ctx.RawContext.PLDeviceReadOnly where d.ModuleCode == gpsDeviceID select d).SingleOrDefault();
      Assert.IsNotNull(rawDevice);
    }

    private void AssertServiceView(Service service, long assetID, long dealerID, DealerNetworkEnum dealerNetwork, long? customerID = null, long? accountID = null, DealerNetworkEnum? parentDealerNetwork = null)
    {
      var activationkeyDate = service.ActivationKeyDate.FromKeyDate().AddMonths(-13).KeyDate();

      //assert service views
      var serviceViews = (from sv in Ctx.OpContext.ServiceViewReadOnly where sv.fk_ServiceID == service.ID && sv.fk_AssetID == assetID && sv.fk_ServiceID == service.ID select sv).ToList();
      var corpCustomer = (from c in Ctx.OpContext.CustomerReadOnly where c.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate && c.fk_DealerNetworkID == (int)dealerNetwork select c).SingleOrDefault();

      //assert service views for dealer
      var dealerServiceView = serviceViews.Where(t => t.fk_CustomerID == dealerID).SingleOrDefault();
      Assert.IsNotNull(dealerServiceView);
      Assert.AreEqual(dealerServiceView.EndKeyDate, DotNetExtensions.NullKeyDate);

      //assert service views for corp customer
      var corpServiceView = serviceViews.Where(t => t.fk_CustomerID == corpCustomer.ID).SingleOrDefault();
      Assert.IsNotNull(corpServiceView);
      Assert.AreEqual(corpServiceView.EndKeyDate, DotNetExtensions.NullKeyDate);

      if (customerID.HasValue)
      {
        var customerServiceView = serviceViews.Where(t => t.fk_CustomerID == customerID).SingleOrDefault();
        Assert.IsNotNull(customerServiceView);
        Assert.AreEqual(customerServiceView.StartKeyDate, service.OwnerVisibilityKeyDate.Value);
        Assert.AreEqual(customerServiceView.EndKeyDate, DotNetExtensions.NullKeyDate);
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
        Assert.AreEqual(parentCorpServiceView.EndKeyDate, DotNetExtensions.NullKeyDate);
      }
    }
    #endregion
  }
}
