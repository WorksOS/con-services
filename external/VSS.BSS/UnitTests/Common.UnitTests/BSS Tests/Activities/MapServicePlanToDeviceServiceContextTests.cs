using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class MapServicePlanToDeviceServiceContextTests : BssUnitTestBase
  {
    MapServicePlanToDeviceServiceContext _activity;
    Inputs _inputs;
    ActivityResult _result;

    [TestInitialize]
    public void TestInitialize()
    {
      _activity = new MapServicePlanToDeviceServiceContext();
      _inputs = new Inputs();
    }

    [TestCleanup]
    public void TestCleanup()
    {
      Console.WriteLine(_result.Summary);
    }

    [DatabaseTest]
    [TestMethod]
    public void Execute_MapMessageToDeviceServiceContext()
    {
      var message = BSS.SPActivated.ServicePlanName("89510-00").Build();
      _inputs.Add<ServicePlan>(message);
      _result = _activity.Execute(_inputs);
      AssertContextResults(ServiceTypeEnum.StandardHealth, message);
    }

    [DatabaseTest]
    [TestMethod]
    public void Execute_MapMessageToDeviceServiceContext_WrongServicePlanName()
    {
      var message = BSS.SPActivated.ServicePlanName("89510-11").Build();
      _inputs.Add<ServicePlan>(message);
      _result = _activity.Execute(_inputs);

      AssertContextResults(ServiceTypeEnum.Unknown, message);
    }

    [DatabaseTest]
    [TestMethod]
    public void Execute_MapExistingDeviceToDeviceServiceContext()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.IbKey(IdGen.StringId()).OwnerBssId(owner.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var message = BSS.SPActivated.ServicePlanName("89510-00").IBKey(device.IBKey).Build();
      _inputs.Add<ServicePlan>(message);
      _result = _activity.Execute(_inputs);

      AssertContextResults(ServiceTypeEnum.StandardHealth, message);
      AssertDeviceAssetResults(device, asset, owner);
    }

    [DatabaseTest]
    [TestMethod]
    public void Execute_MapExistingDeviceToDeviceServiceContext_OwnerDoesNotExists()
    {
      //var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var device = Entity.Device.MTS521.IbKey(IdGen.StringId()).OwnerBssId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var message = BSS.SPActivated.ServicePlanName("89510-00").IBKey(device.IBKey).Build();
      _inputs.Add<ServicePlan>(message);
      _result = _activity.Execute(_inputs);

      AssertContextResults(ServiceTypeEnum.StandardHealth, message);
      AssertDeviceAssetResults(device, asset);
    }

    [DatabaseTest]
    [TestMethod]
    public void Execute_MapExistingDeviceToDeviceServiceContext_AssetDoesNotExists()
    {
      var device = Entity.Device.MTS521.IbKey(IdGen.StringId()).Save();

      var message = BSS.SPActivated.ServicePlanName("89510-00").IBKey(device.IBKey).Build();
      _inputs.Add<ServicePlan>(message);
      _result = _activity.Execute(_inputs);

      AssertContextResults(ServiceTypeEnum.StandardHealth, message);
      AssertDeviceAssetResults(device, null);
    }

    [DatabaseTest]
    [TestMethod]
    public void Execute_MapExistingDeviceToDeviceServiceContext_DeviceDoesNotExists()
    {
      var message = BSS.SPActivated.ServicePlanName("89510-00").IBKey(IdGen.StringId()).Build();
      _inputs.Add<ServicePlan>(message);
      _result = _activity.Execute(_inputs);

      AssertContextResults(ServiceTypeEnum.StandardHealth, message);
      AssertDeviceAssetResults(null, null);
    }

    [TestMethod]
    [DatabaseTest]
    public void Execute_MapExistingServiceToServiceContext()
    {
      var device = Entity.Device.MTS521.IbKey(IdGen.StringId()).Save();
      var service = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId()).Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(service.BSSLineID).ServicePlanName("89510-00").Build();
      _inputs.Add<ServicePlan>(message);
      _result = _activity.Execute(_inputs);

      AssertContextResults(ServiceTypeEnum.StandardHealth, message);
      AssertDeviceAssetResults(device, null);
      AssertServceResults(service, device);
    }

    [TestMethod]
    [DatabaseTest]
    public void Execute_MapExistingServiceToServiceContext_SameServiceExistsWithDifferentPlanLineID()
    {
      var device = Entity.Device.MTS521.IbKey(IdGen.StringId()).Save();
      var planLineID = IdGen.StringId();
      Entity.Service.Essentials.ForDevice(device).BssPlanLineId(planLineID).SyncWithRpt().Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89500-00").Build();
      _inputs.Add<ServicePlan>(message);
      _result = _activity.Execute(_inputs);

      AssertContextResults(ServiceTypeEnum.Essentials, message);
      Assert.AreEqual(planLineID, _inputs.Get<DeviceServiceContext>().ExistingService.DifferentServicePlanLineID);
    }

    [TestMethod]
    [DatabaseTest]
    public void Execute_MapExistingServiceToServiceContext_ServiceDoesNotExists()
    {
      var device = Entity.Device.MTS521.IbKey(IdGen.StringId()).Save();

      var message = BSS.SPActivated.IBKey(device.IBKey).ServicePlanlineID(IdGen.StringId()).ServicePlanName("89510-00").Build();
      _inputs.Add<ServicePlan>(message);
      _result = _activity.Execute(_inputs);

      AssertContextResults(ServiceTypeEnum.StandardHealth, message);
      AssertDeviceAssetResults(device, null);
      AssertServceResults(null, null);
    }

    private void AssertContextResults(ServiceTypeEnum? serviceType, ServicePlan message)
    {
      var context = _inputs.Get<DeviceServiceContext>();

      Assert.AreEqual(context.IBKey, message.IBKey, "IBKey should have been mapped correctly.");
      Assert.AreEqual(context.OwnerVisibilityDate.ToString(), message.OwnerVisibilityDate, "OwnerVisibilityDate should have been mapped correctly.");
      Assert.AreEqual(context.PartNumber, message.ServicePlanName, "ServicePlanName should have been mapped correctly.");
      Assert.AreEqual(context.PlanLineID, message.ServicePlanlineID, "ServicePlanlineID should have been mapped correctly.");
      Assert.AreEqual(context.ServiceTerminationDate.ToString(), message.ServiceTerminationDate, "ServiceTerminationDate should have been mapped correctly.");
      Assert.AreEqual(context.ServiceType, serviceType, "Service Type should have been mapped correctly.");
      Assert.AreEqual(message.ActionUTC, context.ActionUTC.ToString(), "ActionUTC should have been mapped correctly.");
      Assert.AreEqual(message.SequenceNumber, context.SequenceNumber, "Sequence Number should have mapped correctly.");
    }

    private void AssertDeviceAssetResults(Device device, Asset asset, Customer customer = null)
    {
      var context = _inputs.Get<DeviceServiceContext>();

      if (asset == null && device == null)
      {
        Assert.IsFalse(context.ExistingDeviceAsset.AssetExists, "Asset should not exist.");
        Assert.IsFalse(context.ExistingDeviceAsset.DeviceExists, "Device should not exist.");
        return;
      }
      if (asset != null)
        Assert.IsTrue(context.ExistingDeviceAsset.AssetExists, "Asset should exist.");
      else
        Assert.IsFalse(context.ExistingDeviceAsset.AssetExists, "Asset should not exist.");

      Assert.IsTrue(context.ExistingDeviceAsset.DeviceExists, "Asset should exist.");
      Assert.AreEqual(device.ID, context.ExistingDeviceAsset.DeviceId, "Device IDs are expected to match.");
      Assert.AreEqual(device.GpsDeviceID, context.ExistingDeviceAsset.GpsDeviceId, "GPSDeviceIDs are expected to match.");
      Assert.AreEqual(device.IBKey, context.ExistingDeviceAsset.IbKey, "IBkeys are expected to match.");
      Assert.AreEqual((DeviceTypeEnum)device.fk_DeviceTypeID, context.ExistingDeviceAsset.Type, "DevieTypes are expected to match.");

      if (customer != null)
        Assert.AreEqual(customer.BSSID, context.ExistingDeviceAsset.OwnerBSSID, "Owner BSSIDs are expected to match.");
      else
        Assert.IsNull(context.ExistingDeviceAsset.OwnerBSSID, "OwnerBSSID should not exist.");
    }

    private void AssertServceResults(Service service, Device device)
    {
      var context = _inputs.Get<DeviceServiceContext>();

      if (service != null)
      {
        Assert.IsTrue(context.ExistingService.ServiceExists, "Service should exist.");
        Assert.AreEqual(device.ID, context.ExistingService.DeviceID, "Device IDs are expected to match.");
        Assert.AreEqual(service.BSSLineID, context.ExistingService.PlanLineID, "Service Plan Line IDs are expected to match.");
        Assert.AreEqual(ServiceTypeEnum.Essentials, context.ExistingService.ServiceType, "Service Types are expected to match.");
      }
      else
        Assert.IsFalse(context.ExistingService.ServiceExists, "Service should not exist.");
    }
  }
}
