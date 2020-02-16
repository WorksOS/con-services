using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class BssServiceViewServiceTests : BssUnitTestBase
  {
    [TestMethod]
    public void GetServices_EssentialsExists()
    {
      var essentials = Entity.Service.Essentials.Save();
      var device = Entity.Device.MTS521.WithService(essentials).Save();

      var essentialsExists = Services.ServiceViews().DeviceHasAnActiveService(device.ID);

      Assert.IsTrue(essentialsExists, "Expected a true value");
    }

    [TestMethod]
    public void GetServices_EssentialsNotExists()
    {
      var device = Entity.Device.MTS521.Save();

      var essentialsExists = Services.ServiceViews().DeviceHasAnActiveService(device.ID);

      Assert.IsFalse(essentialsExists, "Expected a false value");
    }

    [TestMethod]
    public void GetServices_EssentialsAndOtherServicesExists()
    {
      var device = Entity.Device.MTS521.Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).Save();
      var catHealth = Entity.Service.Health.ForDevice(device).Save();

      var essentialsExists = Services.ServiceViews().DeviceHasAnActiveService(device.ID);

      Assert.IsTrue(essentialsExists, "Expected a true value");
    }

    [DatabaseTest]
    [TestMethod]
    public void GetServiceTypeByPartNumber()
    {
      var serviceType = Services.ServiceViews().GetServiceTypeByPartNumber("89500-00");
      AssertServiceType(ServiceTypeEnum.Essentials, serviceType);
    }

    [DatabaseTest]
    [TestMethod]
    public void GetServiceTypeByPartNumber_PartNumberDoesNotExists()
    {
      var serviceType = Services.ServiceViews().GetServiceTypeByPartNumber("89500-11");
      AssertServiceType(ServiceTypeEnum.Unknown, serviceType);
    }

    [TestMethod]
    [DatabaseTest]
    public void GetServiceByPlanLineID()
    {
      var device = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).Save();
      var service = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.GetId().ToString()).Save();

      var serviceDto = Services.ServiceViews().GetServiceByPlanLineID(service.BSSLineID);

      Assert.IsTrue(serviceDto.ServiceExists, "Service should exist.");
      Assert.AreEqual(device.ID, serviceDto.DeviceID, "Device IDs are expected to match.");
      Assert.AreEqual(service.BSSLineID, serviceDto.PlanLineID, "Service Plan Line IDs are expected to match.");
    }

    [TestMethod]
    [DatabaseTest]
    public void GetServiceByPlanLineID_PlanLineIDDoesNotExists()
    {
      var service = Services.ServiceViews().GetServiceByPlanLineID(IdGen.GetId().ToString());
      Assert.IsNull(service);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateServiceAndServiceViews_CustomerDoesNotExsts()
    {
      var result = Services.ServiceViews().UpdateServiceAndServiceViews(IdGen.GetId(), IdGen.StringId(), IdGen.GetId(), DateTime.UtcNow, ServiceTypeEnum.Essentials);
      Assert.AreEqual(0, result.Count, "No service Views are expected in return as the service and customer doesn't exist.");
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateServiceAndServiceViews_NoServiceExistForDevce()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL121.GpsDeviceId(IdGen.GetId().ToString()).OwnerBssId(account.BSSID).IbKey(IdGen.GetId().ToString()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var result = Services.ServiceViews().UpdateServiceAndServiceViews(asset.AssetID, account.BSSID, IdGen.GetId(), DateTime.UtcNow, ServiceTypeEnum.Essentials);

      Assert.IsNotNull(result, "result shouldn't be null.");
      Assert.AreEqual(0, result.Count, "Count should be 0 as there are no services exists for the device.");
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateServiceAndServiceViews_ServiceExistForDevce_CreateServiceViewsForCustomer()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL121.GpsDeviceId(IdGen.GetId().ToString()).OwnerBssId(account.BSSID).IbKey(IdGen.GetId().ToString()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var essentials = Entity.Service.Essentials.BssPlanLineId(IdGen.GetId().ToString()).ForDevice(device).Save();

      var result = Services.ServiceViews().UpdateServiceAndServiceViews(asset.AssetID, account.BSSID, essentials.ID, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      AssertServiceViewResults(result, essentials.ID, customer.ID, asset.AssetID);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateServiceAndServiceViews_ServiceExistWithViewForDevce_CreateServiceViewsForCustomer()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL121.GpsDeviceId(IdGen.GetId().ToString()).OwnerBssId(account.BSSID).IbKey(IdGen.GetId().ToString()).Save();
      var asset = Entity.Asset.WithDevice(device).WithCoreService().Save();

      var essentials = Ctx.OpContext.ServiceReadOnly.Where(t => t.fk_DeviceID == device.ID).SingleOrDefault();
      var sv = Ctx.OpContext.ServiceView.Where(t => t.fk_ServiceID == essentials.ID).SingleOrDefault();
      sv.UpdateUTC = DateTime.UtcNow.AddDays(-20);
      Ctx.OpContext.SaveChanges();

      var result = Services.ServiceViews().UpdateServiceAndServiceViews(asset.AssetID, account.BSSID, essentials.ID, DateTime.UtcNow, ServiceTypeEnum.Essentials);

      AssertServiceViewResults(result, essentials.ID, customer.ID, asset.AssetID);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateServiceAndServiceViews_ServiceExistWithViewForDevce_NullOwnerVisibilityDate_TerminateServiceViewsForCustomer()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.GetId().ToString()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var relationship = Entity.CustomerRelationship.Relate(customer, account).Save();

      var device = Entity.Device.PL121.GpsDeviceId(IdGen.GetId().ToString()).OwnerBssId(account.BSSID).IbKey(IdGen.GetId().ToString()).Save();
      var asset = Entity.Asset.WithDevice(device).WithCoreService().Save();

      var essentials = Ctx.OpContext.ServiceReadOnly.Where(t => t.fk_DeviceID == device.ID).SingleOrDefault();
      var sv = Ctx.OpContext.ServiceView.Where(t => t.fk_ServiceID == essentials.ID).SingleOrDefault();
      sv.UpdateUTC = DateTime.UtcNow.AddDays(-20);
      sv.StartKeyDate = sv.UpdateUTC.KeyDate();
      Ctx.OpContext.SaveChanges();

      var result = Services.ServiceViews().UpdateServiceAndServiceViews(asset.AssetID, account.BSSID, essentials.ID, null, ServiceTypeEnum.Essentials);

      AssertServiceViewResults(result, essentials.ID, customer.ID, asset.AssetID, false);
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceHasSameActiveService_PastServiceTerminated_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.OwnerBssId(account.BSSID).IbKey(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId()).ActivationDate(DateTime.UtcNow.AddDays(-30)).CancellationDate(DateTime.UtcNow.AddDays(-1)).Save();

      var result = Services.ServiceViews().DeviceHasSameActiveService(device.ID, ServiceTypeEnum.Essentials);
      Assert.IsNull(result, "Service active status expected to be false.");
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceHasSameActiveService_TodayServiceTerminated_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.OwnerBssId(account.BSSID).IbKey(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId()).ActivationDate(DateTime.UtcNow.AddDays(-30)).CancellationDate(DateTime.UtcNow).Save();

      var result = Services.ServiceViews().DeviceHasSameActiveService(device.ID, ServiceTypeEnum.Essentials);
      Assert.IsNull(result, "Service active status expected to be false.");
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceHasSameActiveService_TomorrowServiceTerminated_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.OwnerBssId(account.BSSID).IbKey(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId()).ActivationDate(DateTime.UtcNow.AddDays(-30)).CancellationDate(DateTime.UtcNow.AddDays(1)).Save();

      var result = Services.ServiceViews().DeviceHasSameActiveService(device.ID, ServiceTypeEnum.Essentials);
      Assert.AreEqual(essentials.BSSLineID, result, "Service active status expected to be false.");
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceHasSameActiveService_FutureServiceTerminated_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.OwnerBssId(account.BSSID).IbKey(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var essentials = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId()).ActivationDate(DateTime.UtcNow.AddDays(-30)).CancellationDate(DateTime.UtcNow.AddDays(30)).Save();

      var result = Services.ServiceViews().DeviceHasSameActiveService(device.ID, ServiceTypeEnum.Essentials);
      Assert.AreEqual(essentials.BSSLineID, result, "Service active status expected to be false.");
    }

    private void AssertServiceViewResults(IList<ServiceViewInfoDto> result, long servceID, long customerID, long assetID, bool ownerVisibilityKeyDateNotNull = true)
    {
      Assert.IsNotNull(result, "result shouldn't be null.");
      Assert.AreEqual(1, result.Count, "Count should be 1 as there are no services exists for the device.");

      var service = Ctx.OpContext.ServiceReadOnly.Where(t => t.ID == servceID).SingleOrDefault();
      Assert.IsNotNull(service);

      var serviceView = Ctx.OpContext.ServiceViewReadOnly.Where(t => t.fk_ServiceID == service.ID && t.fk_CustomerID == customerID && t.fk_AssetID == assetID).SingleOrDefault();
      
      if (ownerVisibilityKeyDateNotNull)
      {
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), service.OwnerVisibilityKeyDate, "Keydate should match.");
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), serviceView.StartKeyDate, "Start key dates should match.");
        Assert.AreEqual(DotNetExtensions.NullKeyDate, serviceView.EndKeyDate, "End key dates should match.");
      }
      else
      {
        Assert.AreEqual(null, service.OwnerVisibilityKeyDate, "Keydate should match.");
        Assert.AreEqual(DateTime.UtcNow.KeyDate(), serviceView.EndKeyDate, "Start key dates should match.");
      }

      Assert.IsNotNull(serviceView);
      //Assert.AreEqual(DateTime.UtcNow.KeyDate(), serviceView.StartKeyDate, "Start key dates should match.");
      //Assert.AreEqual(DotNetExtensions.NullKeyDate, serviceView.EndKeyDate, "End key dates should match.");
      Assert.AreEqual(DateTime.UtcNow.KeyDate(), serviceView.UpdateUTC.KeyDate(), "Updated utc should be today.");
    }

    private void AssertServiceType(ServiceTypeEnum expected, ServiceTypeEnum? actual)
    {
      Assert.IsNotNull(actual);
      Assert.AreEqual(expected, actual, "Service Types are expected to match.");
    }
  }
}
