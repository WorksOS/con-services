using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Configuration;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.NH_OPMockObjectSet;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.UnitTest.Common;

namespace UnitTests
{

  /// <summary>
  ///This is a test class for ServiceViewAPITest and is intended
  ///to contain all ServiceViewAPITest Unit Tests
  ///</summary>
  [TestClass]
  public class ServiceViewAPITest : UnitTestBase
  {
    [DatabaseTest]
    [TestMethod]
    public void Create_RegisteredDealer_WithoutDealerNetwork_SuccessTest()
    {
      var target = new ServiceViewAPI();

      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.None).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(dealer.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var currentDateTime = DateTime.UtcNow;

      var service = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "", currentDateTime, ServiceTypeEnum.ManualMaintenanceLog);

      var actual = (Ctx.OpContext.ServiceViewReadOnly.Where(s => s.fk_ServiceID == service.ID)).ToList();
      Assert.IsNotNull(actual, "ServiceView was not saved successfully.");
      Assert.AreEqual(1, actual.Count, "Incorrect number of service views returned.");
      Assert.AreEqual(1, actual.Count(x => x.fk_CustomerID == dealer.ID), "Did not find a service view for the dealer.");
      Assert.AreEqual(1, actual.Count(x => x.fk_AssetID == asset.AssetID), "ServiceView AssetIDs do not match.");
    }

    [DatabaseTest]
    [TestMethod]
    public void Create_RegisteredDealer_WithParent_WithoutDealerNetwork_SuccessTest()
    {
      var target = new ServiceViewAPI();

      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.None).Save();
      Entity.CustomerRelationship.Relate(dealer, account).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.None).Save();
      Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var currentDateTime = DateTime.UtcNow;

      var service = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "", currentDateTime, ServiceTypeEnum.ManualMaintenanceLog);

      var actual = (Ctx.OpContext.ServiceViewReadOnly.Where(s => s.fk_ServiceID == service.ID)).ToList();
      Assert.IsNotNull(actual, "ServiceView was not saved successfully.");
      Assert.AreEqual(2, actual.Count, "Incorrect number of service views returned.");
      Assert.AreEqual(1, actual.Count(x => x.fk_CustomerID == dealer.ID), "Did not find a service view for the dealer.");
      Assert.AreEqual(1, actual.Count(x => x.fk_CustomerID == parentDealer.ID), "Did not find a service view for the dealer.");
      Assert.AreEqual(2, actual.Count(x => x.fk_AssetID == asset.AssetID), "ServiceView AssetIDs do not match.");
    }

    [DatabaseTest]
    [TestMethod]
    public void Create_SuccessWithEndUTCTest()
    {
      var target = new ServiceViewAPI();
      var customer = TestData.TestCustomer;
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;

      var service = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "", currentDateTime, ServiceTypeEnum.ManualMaintenanceLog);

      var actual = (Ctx.OpContext.ServiceViewReadOnly.Where(s => s.fk_ServiceID == service.ID)).ToList();
      Assert.IsNotNull(actual, "ServiceView was not saved successfully.");
      Assert.AreEqual(3, actual.Count, "Incorrect number of service views returned.");
      Assert.AreEqual(1, actual.Count(x => x.fk_CustomerID == customer.ID), "Did not find a service view for the customer.");
      Assert.AreEqual(3, actual.Count(x => x.fk_AssetID == asset.AssetID), "ServiceView AssetIDs do not match.");
    }

    [DatabaseTest]
    [TestMethod]
    public void Create_SuccessWithoutEndUTCTest()
    {
      var target = new ServiceViewAPI();
      var customer = TestData.TestCustomer;
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;
      DateTime? nullDateTime = null;

      var service = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "", currentDateTime, ServiceTypeEnum.ManualMaintenanceLog);


      var actual = (Ctx.OpContext.ServiceViewReadOnly.Where(s => s.fk_ServiceID == service.ID)).ToList();
      Assert.IsNotNull(actual, "ServiceView was not saved successfully.");
      Assert.AreEqual(3, actual.Count, "Incorrect number of service views returned.");
      Assert.AreEqual(1, actual.Count(x => x.fk_CustomerID == customer.ID), "Did not find a service view for the customer.");
      Assert.AreEqual(3, actual.Count(x => x.fk_AssetID == asset.AssetID), "ServiceView AssetIDs do not match.");
      Assert.AreEqual(3, actual.Count(x => x.EndKeyDate == nullDateTime.KeyDate()), "End Date is not null.");
    }

    [DatabaseTest]
    [TestMethod]
    public void Create_SuccessWithManualCorePlanTest()
    {
      var target = new ServiceViewAPI();
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;

      var service = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "", currentDateTime, ServiceTypeEnum.ManualMaintenanceLog);

      var actual = (Ctx.OpContext.ServiceViewReadOnly.Where(s => s.fk_ServiceID == service.ID)).ToList();
      Assert.IsNotNull(actual, "ServiceView with core plan is null");
    }

    [DatabaseTest]
    [TestMethod]
    public void Create_SuccessWithoutEssentialsPlanTest()
    {
      var target = new ServiceViewAPI();
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;

      var service = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "", currentDateTime, ServiceTypeEnum.CATUtilization);
      var actual = (Ctx.OpContext.ServiceViewReadOnly.Where(s => s.fk_ServiceID == service.ID)).ToList();
      Assert.IsNotNull(actual, "ServiceView with non core plan is null");
    }

    [TestMethod]
    [DatabaseTest]
    public void Terminate_SuccessTest()
    {
      var target = new ServiceViewAPI();
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;
      const string bssid = "-BogusBSSID";
      target.CreateService(Ctx.OpContext, asset.fk_DeviceID, bssid, currentDateTime, ServiceTypeEnum.ManualMaintenanceLog);

      var terminationUTC = currentDateTime.AddDays(1);
      var serviceTerminated = target.TerminateService(Ctx.OpContext, bssid, terminationUTC);
      Assert.IsTrue(serviceTerminated, "ServiceView should be terminated");
    }

    [TestMethod]
    [DatabaseTest]
    public void ReleaseAssetTest()
    {
      var target = new ServiceViewAPI();
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;

      var service = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "TestBssID", currentDateTime, ServiceTypeEnum.Essentials);
      service.CancellationKeyDate = currentDateTime.KeyDate();

      Ctx.OpContext.SaveChanges();

      var storeReseted = target.ReleaseAsset(Ctx.OpContext, asset.fk_DeviceID);
      Assert.IsTrue(storeReseted, "Store ID should be reset to default value");

      var modifiedAsset = Ctx.OpContext.AssetReadOnly.Where(t => t.AssetID == asset.AssetID).Select(t => t).SingleOrDefault();

      Assert.IsNotNull(modifiedAsset);
      Assert.AreEqual(0, modifiedAsset.fk_StoreID, "The asset should be mapped to NoStore");
    }

    [TestMethod]
    [DatabaseTest]
    public void ReleaseAssetTest_MultipleServicePlans()
    {
      var target = new ServiceViewAPI();
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;

      var essentials = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "TestBssID", currentDateTime, ServiceTypeEnum.Essentials);
      var health = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "TestBssID", currentDateTime, ServiceTypeEnum.CATHealth);
      var catMain = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "TestBssID", currentDateTime, ServiceTypeEnum.CATMAINT);
      var util = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "TestBssID", currentDateTime, ServiceTypeEnum.StandardUtilization);

      var storeReseted = target.ReleaseAsset(Ctx.OpContext, asset.fk_DeviceID);
      Assert.IsFalse(storeReseted, "Store ID should be reset to default value");

      //cancel essentials
      essentials.CancellationKeyDate = currentDateTime.KeyDate();
      Ctx.OpContext.SaveChanges();

      storeReseted = target.ReleaseAsset(Ctx.OpContext, asset.fk_DeviceID);
      Assert.IsFalse(storeReseted, "Store ID should be reset to default value");

      //cancel health
      health.CancellationKeyDate = currentDateTime.KeyDate();
      Ctx.OpContext.SaveChanges();

      storeReseted = target.ReleaseAsset(Ctx.OpContext, asset.fk_DeviceID);
      Assert.IsFalse(storeReseted, "Store ID should be reset to default value");

      //cancel catMain
      catMain.CancellationKeyDate = currentDateTime.KeyDate();
      Ctx.OpContext.SaveChanges();

      storeReseted = target.ReleaseAsset(Ctx.OpContext, asset.fk_DeviceID);
      Assert.IsFalse(storeReseted, "Store ID should be reset to default value");

      //cancel util
      util.CancellationKeyDate = currentDateTime.KeyDate();
      Ctx.OpContext.SaveChanges();


      storeReseted = target.ReleaseAsset(Ctx.OpContext, asset.fk_DeviceID);
      Assert.IsTrue(storeReseted, "Store ID should be reset to default value");

      var modifiedAsset = Ctx.OpContext.AssetReadOnly.Where(t => t.AssetID == asset.AssetID).Select(t => t).SingleOrDefault();

      Assert.IsNotNull(modifiedAsset);
      Assert.AreEqual(0, modifiedAsset.fk_StoreID, "The asset should be mapped to NoStore");
    }

    [TestMethod]
    [DatabaseTest]
    public void Terminate_SuccessToResetFirstReportUTCOnCorePlanTerminationTest()
    {
      var target = new ServiceViewAPI();
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;
      asset.FirstReportUTC = currentDateTime;

      const string bssid = "-BogusBSSID";
      target.CreateService(Ctx.OpContext, asset.fk_DeviceID, bssid, currentDateTime, ServiceTypeEnum.ManualMaintenanceLog);

      var terminationUTC = currentDateTime.AddDays(1);
      terminationUTC.KeyDate();
      var serviceTerminated = target.TerminateService(Ctx.OpContext, bssid, terminationUTC);

      Assert.IsTrue(serviceTerminated, "ServiceView should be terminated");
    }

    [TestMethod]
    [DatabaseTest]
    public void Terminate_SuccessForNoPlansToTerminateTest()
    {
      var target = new ServiceViewAPI();
      var currentDateTime = DateTime.UtcNow;

      var terminationUTC = currentDateTime.AddDays(1);

      var serviceTerminated = target.TerminateService(Ctx.OpContext, "", terminationUTC);
      Assert.IsFalse(serviceTerminated, "ServiceView should be terminated");
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_SuccessForCorePlanTestAndCallsNG()
    {
      var mockCustomerService = new Mock<ICustomerService>();
      mockCustomerService.Setup(x => x.AssociateCustomerAsset(It.IsAny<AssociateCustomerAssetEvent>())).Returns(true);
      var target = new ServiceViewAPI(mockCustomerService.Object);
      var customer = TestData.TestCustomer;
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;

      var expected = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, customer.BSSID, currentDateTime, ServiceTypeEnum.Essentials);
      Assert.IsNotNull(expected, "Service Record should be created");
      Assert.IsTrue(expected.IsFirstReportNeeded, "FirstReportNeeded flag should be true");
      mockCustomerService.Verify(x => x.AssociateCustomerAsset(It.IsAny<AssociateCustomerAssetEvent>()), Times.AtLeastOnce());
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_SuccessForNonCorePlanTest()
    {
      var target = new ServiceViewAPI();
      var customer = TestData.TestCustomer;
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;

      var expected = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, customer.BSSID, currentDateTime, ServiceTypeEnum.CATHealth);
      Assert.IsNotNull(expected, "Service Record should be created");
      Assert.IsFalse(expected.IsFirstReportNeeded, "FirstReportNeeded flag should be false");
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateService_SuccessForNonBSSPlanLineIDTest()
    {
      var target = new ServiceViewAPI();
      var customer = TestData.TestCustomer;
      customer.BSSID = "-" + customer.BSSID;
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;

      var expected = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, customer.BSSID, currentDateTime, ServiceTypeEnum.Essentials);
      Assert.IsNotNull(expected, "Service Record should be created");
      Assert.IsFalse(expected.IsFirstReportNeeded, "FirstReportNeeded flag should be false");
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_SuccessTest()
    {
      var target = new ServiceViewAPI();
      var customer = TestData.TestCustomer;
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;

      var created = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, customer.BSSID, currentDateTime, ServiceTypeEnum.Essentials);
      Assert.IsNotNull(created, "Service Record should be created");
      Assert.IsTrue(created.IsFirstReportNeeded, "FirstReportNeeded flag should be true");

      var terminationUTC = currentDateTime.AddDays(1);
      var expected = target.TerminateService(Ctx.OpContext, customer.BSSID, terminationUTC);
      Assert.IsTrue(expected, "Service should be terminated");

      var serviceCancellationDate = (from s in Ctx.OpContext.ServiceReadOnly
                                     where s.BSSLineID == customer.BSSID
                                     select s.CancellationKeyDate).FirstOrDefault();
      Assert.IsTrue(serviceCancellationDate > 0, "ServiceCancellationDate should not be 0");
      Assert.AreEqual(terminationUTC.KeyDate(), serviceCancellationDate, "ServiceCancellationDate does not match");
    }

    [TestMethod]
    [DatabaseTest]
    public void TerminateService_SuccessWhenNoServicePresentTest()
    {
      var target = new ServiceViewAPI();
      var customer = TestData.TestCustomer;
      var currentDateTime = DateTime.UtcNow;

      var terminationUTC = currentDateTime.AddDays(1);
      var expected = target.TerminateService(Ctx.OpContext, customer.BSSID, terminationUTC);
      Assert.IsFalse(expected, "No service should be terminated since none exists");
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceSupportsServiceTest()
    {
      Assert.IsTrue(API.ServiceView.DeviceSupportsService(Ctx.OpContext, ServiceTypeEnum.CATUtilization, DeviceTypeEnum.Series522), "The MTS522 should support the CATUTIL service plan");
      Assert.IsTrue(API.ServiceView.DeviceSupportsService(Ctx.OpContext, ServiceTypeEnum.StandardHealth, DeviceTypeEnum.SNM940), "The SNM940 should support the STDHEALTH service plan");
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceHasCoreServiceTest_ServicePresent()
    {
      var target = new ServiceViewAPI();
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;

      target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "", currentDateTime, ServiceTypeEnum.ManualMaintenanceLog);

      Assert.IsTrue(API.ServiceView.DeviceHasActiveCoreService(Ctx.OpContext, asset.fk_DeviceID), "The device should have service present.");
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceHasCoreServiceTest_ServiceNotPresent()
    {
      var asset = TestData.TestAssetMTS521;
      Assert.IsFalse(API.ServiceView.DeviceHasActiveCoreService(Ctx.OpContext, asset.fk_DeviceID), "The device should not have service present.");
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceHasCoreServiceTest_NonCorePlanOnly()
    {
      var target = new ServiceViewAPI();
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;
      target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "", currentDateTime, ServiceTypeEnum.StandardHealth);
      Assert.IsFalse(API.ServiceView.DeviceHasActiveCoreService(Ctx.OpContext, asset.fk_DeviceID), "The device should not have core service present.");
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceHasAnyActiveServiceTest_ServicePresent()
    {
      var target = new ServiceViewAPI();
      var asset = TestData.TestAssetMTS521;
      var currentDateTime = DateTime.UtcNow;

      target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "", currentDateTime, ServiceTypeEnum.ManualMaintenanceLog);

      Assert.IsTrue(API.ServiceView.DeviceHasAnActiveService(Ctx.OpContext, asset.fk_DeviceID), "The device should have service present.");
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceHasAnyActiveServiceTest_ServiceNotPresent()
    {
      var asset = TestData.TestAssetMTS521;
      Assert.IsFalse(API.ServiceView.DeviceHasAnActiveService(Ctx.OpContext, asset.fk_DeviceID), "The device should not have service present.");
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceHasAnyActiveServiceTest_NonCorePlanOnly()
    {
      var target = new ServiceViewAPI();
      var asset = TestData.TestAssetMTS521;
      target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "", DateTime.UtcNow, ServiceTypeEnum.StandardHealth);
      Assert.IsTrue(API.ServiceView.DeviceHasAnActiveService(Ctx.OpContext, asset.fk_DeviceID), "The device should have a non-core plan present.");
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateAssetServiceViews_Account_Customer_Dealer()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.None).Save();
      Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device = Entity.Device.MTS521.OwnerBssId(dealer.BSSID).IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      Entity.Service.Essentials.BssPlanLineId(IdGen.StringId()).ForDevice(device).OwnerVisibilityDate(DateTime.UtcNow).Save();

      var serviceViews = API.ServiceView.CreateAssetServiceViews(asset.AssetID, DateTime.UtcNow);

      Assert.AreEqual(3, serviceViews.Count());
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateAssetServiceViews_Dealer_PreviouslyOnboarded()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.None).Save();
      Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device =
        Entity.Device.MTS521.OwnerBssId(dealer.BSSID).IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      Entity.Asset.WithDevice(device).Save();

      var activationDate = DateTime.UtcNow.AddMonths(-20);
      var terminationDate = DateTime.UtcNow.AddMonths(-1);
      Entity.Service.Essentials.BssPlanLineId(IdGen.StringId()).ForDevice(device).OwnerVisibilityDate(activationDate).
        CancellationDate(terminationDate).Save();

      var device2 =
        Entity.Device.MTS521.OwnerBssId(dealer.BSSID).IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      Entity.Asset.WithDevice(device2).Save();
      var newServiceViews = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID,
                                                                  (DeviceTypeEnum) device.fk_DeviceTypeID,
                                                                  IdGen.StringId(),
                                                                  DateTime.UtcNow, null,
                                                                  ServiceTypeEnum.Essentials);

      var expectedDate = DateTime.UtcNow.KeyDate();
      foreach (var serviceView in newServiceViews.Item2)
      {
        Assert.AreEqual(expectedDate, serviceView.StartKeyDate);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateAssetServiceViews_Dealer_BackDateDealer()
    {
      var expectedDate = DateTime.UtcNow.AddMonths(-13).KeyDate();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
      var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.None).Save();
      Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

      var device =
        Entity.Device.MTS521.OwnerBssId(dealer.BSSID).IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).Save();
      Entity.Asset.WithDevice(device).Save();
      var service =
        Entity.Service.Essentials.BssPlanLineId(IdGen.StringId()).ForDevice(device).OwnerVisibilityDate(DateTime.UtcNow)
          .Save();

      var serviceViews = API.ServiceView.CreateServiceAndServiceViews(Ctx.OpContext, device.ID,
                                                                      (DeviceTypeEnum) device.fk_DeviceTypeID,
                                                                      service.BSSLineID,
                                                                      service.ActivationKeyDate.FromKeyDate(), null,
                                                                      (ServiceTypeEnum) service.fk_ServiceTypeID);

      foreach (var serviceView in serviceViews.Item2)
      {
        Assert.AreEqual(serviceView.StartKeyDate, expectedDate);
      }
    }



    [TestMethod]
    public void TerminateVisibilityTest_Success()
    {
        var mockNhOpContext = new Mock<INH_OP>();
        var serviceViewApi = new ServiceViewAPI();
        var serviceViewData = new MockObjectSet<ServiceView>();
        serviceViewData.AddObject(new ServiceView
        {
             fk_AssetID = 1,
             fk_CustomerID = 2,
             fk_ServiceID = 3,
             StartKeyDate = DateTime.UtcNow.AddYears(-2).KeyDate(),
             EndKeyDate = DateTime.UtcNow.AddYears(2).KeyDate()            
        });

				SetupServiceMocks(mockNhOpContext);

        mockNhOpContext.SetupGet(o => o.ServiceView).Returns(serviceViewData);
        mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

        var result = serviceViewApi.TerminateVisibility(2, 1, 3, DateTime.UtcNow, mockNhOpContext.Object);
        var endKeyDate = mockNhOpContext.Object.ServiceView.FirstOrDefault().EndKeyDate;
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), endKeyDate);
        mockNhOpContext.Verify(o => o.SaveChanges(), Times.Exactly(1));
        Assert.AreEqual(true, result);
    }

		private void SetupServiceMocks(Mock<INH_OP> mockNhOpContext)
	  {
			var assetData = new MockObjectSet<Asset>();
			assetData.AddObject(new Asset()
			{
				AssetID = 1,
				AssetUID = Guid.NewGuid()
			});

			var custData = new MockObjectSet<Customer>();
			custData.AddObject(new Customer()
			{
				ID = 2,
				CustomerUID = Guid.NewGuid()
			});

			mockNhOpContext.SetupGet(c => c.CustomerReadOnly).Returns(custData);
			mockNhOpContext.SetupGet(a => a.AssetReadOnly).Returns(assetData);	

			var serviceTypeData = new MockObjectSet<ServiceType>();
			serviceTypeData.AddObject(new ServiceType()
			{
				ID = 1,
				IsCore = true,
				Name = "Essential"
			});

			var serviceData = new MockObjectSet<Service>();
			serviceData.AddObject(new Service()
			{
				ID = 3,
				fk_ServiceTypeID = 1
			});

			mockNhOpContext.SetupGet(o => o.ServiceTypeReadOnly).Returns(serviceTypeData);
			mockNhOpContext.SetupGet(o => o.ServiceReadOnly).Returns(serviceData);
	  }

    [TestMethod]
    public void TerminateVisibilityTest_Failure()
    {
      var mockNhOpContext = new Mock<INH_OP>();
      var serviceViewApi = new ServiceViewAPI();

			var serviceViewData = new MockObjectSet<ServiceView>();
			serviceViewData.AddObject(new ServiceView
			{
				fk_AssetID = 1,
				fk_CustomerID = 2,
				fk_ServiceID = 3,
				StartKeyDate = DateTime.UtcNow.AddYears(-2).KeyDate(),
				EndKeyDate = DateTime.UtcNow.AddYears(2).KeyDate()
			});
	    SetupServiceMocks(mockNhOpContext);
	    
			mockNhOpContext.SetupGet(o => o.ServiceView).Returns(serviceViewData);
      mockNhOpContext.Setup(o => o.SaveChanges()).Returns(0);

      var result = serviceViewApi.TerminateVisibility(2, 1, 3, DateTime.UtcNow, mockNhOpContext.Object);
      mockNhOpContext.Verify(o => o.SaveChanges(), Times.Exactly(1));
      Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void TerminateVisibilityTest_ServiceViewAlreadyCancelled_Failure()
    {
      var mockNhOpContext = new Mock<INH_OP>();
      var serviceViewApi = new ServiceViewAPI();
      var serviceViewData = new MockObjectSet<ServiceView>();
      serviceViewData.AddObject(new ServiceView
      {
        fk_AssetID = 1,
        fk_CustomerID = 2,
        fk_ServiceID = 3,
        StartKeyDate = DateTime.UtcNow.AddYears(-2).KeyDate(),
        EndKeyDate = DateTime.UtcNow.AddYears(-1).KeyDate()
      });
			SetupServiceMocks(mockNhOpContext);

      mockNhOpContext.SetupGet(o => o.ServiceView).Returns(serviceViewData);
      mockNhOpContext.Setup(o => o.SaveChanges()).Returns(0);

      var result = serviceViewApi.TerminateVisibility(2, 1, 3, DateTime.UtcNow, mockNhOpContext.Object);
      mockNhOpContext.Verify(o => o.SaveChanges(), Times.Exactly(1));
      Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void TerminateVisibilityTest_MultipleServiceViews_OneActiveOneCancelled_Success()
    {
      var mockNhOpContext = new Mock<INH_OP>();
      var serviceViewApi = new ServiceViewAPI();
      var serviceViewData = new MockObjectSet<ServiceView>();
      //Cancelled ServiceView
      serviceViewData.AddObject(new ServiceView
      {
        fk_AssetID = 1,
        fk_CustomerID = 2,
        fk_ServiceID = 3,
        StartKeyDate = DateTime.UtcNow.AddYears(-2).KeyDate(),
        EndKeyDate = DateTime.UtcNow.AddYears(-1).KeyDate()
      });
      //Active ServiceView
      serviceViewData.AddObject(new ServiceView
      {
        fk_AssetID = 1,
        fk_CustomerID = 2,
        fk_ServiceID = 3,
        StartKeyDate = DateTime.UtcNow.AddYears(-1).KeyDate(),
        EndKeyDate = DateTime.UtcNow.AddYears(2).KeyDate()
      });
			SetupServiceMocks(mockNhOpContext);

	    

      mockNhOpContext.SetupGet(o => o.ServiceView).Returns(serviceViewData);
      mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

      var result = serviceViewApi.TerminateVisibility(2, 1, 3, DateTime.UtcNow, mockNhOpContext.Object);
      var serviceViews = mockNhOpContext.Object.ServiceView;
      foreach (var serviceView in serviceViews)
      {
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), serviceView.EndKeyDate);
      }
      mockNhOpContext.Verify(o => o.SaveChanges(), Times.Exactly(1));
      Assert.AreEqual(true, result);
    }

    [TestMethod]
    public void TestServiceViewBlockSizeValue_ValidTimeoutValue()
    {
      ConfigurationManager.AppSettings["ServiceViewBlockSize"] = "120";
      ServiceViewAPI.ResetServiceViewBlockSizeValue();
      Assert.AreEqual(120, ServiceViewAPI.GetServiceViewBlockSizeValue());
    }

    [TestMethod]
    public void TestServiceViewBlockSizeValue_MissingTimeoutValue()
    {
      ConfigurationManager.AppSettings["ServiceViewBlockSize"] = null;
      ServiceViewAPI.ResetServiceViewBlockSizeValue();
      Assert.AreEqual(80, ServiceViewAPI.GetServiceViewBlockSizeValue());
    }

    [TestMethod]
    public void TestServiceViewBlockSizeValue_InvalidTimeoutValue()
    {
      ConfigurationManager.AppSettings["ServiceViewBlockSize"] = "X";
      ServiceViewAPI.ResetServiceViewBlockSizeValue();
      Assert.AreEqual(80, ServiceViewAPI.GetServiceViewBlockSizeValue());
    }

	  [TestMethod]
    [DatabaseTest]
	  public void TestService_CreateServiceView_FiresNextGen()
	  {
			var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.CAT).Save();
			var parentDealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).DealerNetwork(DealerNetworkEnum.None).Save();
			Entity.CustomerRelationship.Relate(parentDealer, dealer).Save();

			var device = Entity.Device.MTS521.OwnerBssId(dealer.BSSID).IbKey(IdGen.StringId()).GpsDeviceId(IdGen.StringId()).Save();
			var asset = Entity.Asset.WithDevice(device).Save();

			Entity.Service.Essentials.BssPlanLineId(IdGen.StringId()).ForDevice(device).OwnerVisibilityDate(DateTime.UtcNow).Save();
			Entity.Service.Health.BssPlanLineId(IdGen.StringId()).ForDevice(device).OwnerVisibilityDate(DateTime.UtcNow).Save();


			var mockCustomerService = new Mock<ICustomerService>();
		  mockCustomerService.Setup(x => x.AssociateCustomerAsset(It.IsAny<AssociateCustomerAssetEvent>())).Returns(true);
      var serviceViews = new ServiceViewAPI(mockCustomerService.Object).CreateAssetServiceViews(asset.AssetID, DateTime.UtcNow);
		  //verify our associate is called same number of times as serviceView creations
		  mockCustomerService.Verify(x => x.AssociateCustomerAsset(It.IsAny<AssociateCustomerAssetEvent>()), Times.Exactly(2)); //one for essentials and none for health and excluding Corporate Customer's service plan
	  }

    
  }
}
