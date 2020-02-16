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
  public class DeviceRegistrationDeregisteredWorkflowTests : BssUnitTestBase
  {
    WorkflowResult result;
    DeviceRegistration message;

    [TestCleanup]
    public void TestCleanup()
    {
      if (result == null)
        return;
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_Status_REG_Invalid_ForDeviceDeregister_Failure()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(owner.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      message = BSS.DRBDeRegistered.IBKey(device.IBKey).Status(DeviceRegistrationStatusEnum.REG).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.DeviceRegistration.DEVICE_STATUS_NOT_VALID, message.Status, "DeRegistered"), "Summary is expected to contain the exception.");

      var opDevice = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
      Assert.IsNull(opDevice.DeregisteredUTC);
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_IBKeyDoesNotExist_Failure()
    {
      message = BSS.DRBDeRegistered.Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.IBKEY_DOES_NOT_EXISTS, message.IBKey), "Summary is expected to contain the exception.");
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_DeviceIsNotAssociatedToAnyValidOwner_Failure()
    {
      var device = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      message = BSS.DRBDeRegistered.IBKey(device.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.DEVICE_NOT_ASSOCIATED_WITH_VALID_CUSTOMER, message.IBKey), "Summary is expected to contain the exception.");

      var opDevice = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
      Assert.IsNull(opDevice.DeregisteredUTC);
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_DeviceAlreadyDeRegisteredByStore_Failure()
    {
      var device = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(IdGen.StringId()).DeviceState(DeviceStateEnum.DeregisteredStore).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      message = BSS.DRBDeRegistered.IBKey(device.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.DeviceRegistration.DEVICE_ALREADY_DEREGISTERED, message.IBKey, "DeRegistered"), "Summary is expected to contain the exception.");

      var opDevice = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
      Assert.IsNull(opDevice.DeregisteredUTC);
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_DeviceAlreadyDeRegisteredByTechnecian_Failure()
    {
      var device = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(IdGen.StringId()).DeviceState(DeviceStateEnum.DeregisteredStore).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      message = BSS.DRBDeRegistered.IBKey(device.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.DeviceRegistration.DEVICE_ALREADY_DEREGISTERED, message.IBKey, "DeRegistered"), "Summary is expected to contain the exception.");

      var opDevice = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
      Assert.IsNull(opDevice.DeregisteredUTC);
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_DeviceHasActiveCoreServicePlan_Failure()
    {
      var device = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var service = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId()).Save();
      message = BSS.DRBDeRegistered.IBKey(device.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ACTIVE_SERVICE_EXISTS_FOR_DEVICE, message.IBKey), "Summary is expected to contain the exception.");

      var opDevice = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
      Assert.IsNull(opDevice.DeregisteredUTC);
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_DeviceHasActiveServicePlan_Failure()
    {
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var service = Entity.Service.Health.ForDevice(device).BssPlanLineId(IdGen.StringId()).Save();
      message = BSS.DRBDeRegistered.IBKey(device.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.ACTIVE_SERVICE_EXISTS_FOR_DEVICE, message.IBKey), "Summary is expected to contain the exception.");

      var opDevice = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
      Assert.IsNull(opDevice.DeregisteredUTC);
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_DeviceServicePlan_TerminatedYesterDay_Failure()
    {
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var service = Entity.Service.Health.ForDevice(device).BssPlanLineId(IdGen.StringId()).CancellationDate(DateTime.UtcNow.AddDays(-1)).Save();
      message = BSS.DRBDeRegistered.IBKey(device.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      Assert.IsTrue(!result.Summary.Contains(string.Format(BssConstants.ACTIVE_SERVICE_EXISTS_FOR_DEVICE, message.IBKey)), "Summary is not expected to contain the exception.");

      var opDevice = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
      Assert.IsNull(opDevice.DeregisteredUTC);
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_DeRegisterNonPLDevice_Failure()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.IbKey(IdGen.StringId()).OwnerBssId(owner.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      message = BSS.DRBDeRegistered.IBKey(device.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.DeviceRegistration.DEVICE_REGISTRATION_NOT_SUPPORTED, DeviceTypeEnum.Series521), "Summary is expected to contain the exception.");

      var opDevice = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
      Assert.IsNull(opDevice.DeregisteredUTC);
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_DeviceServicePlan_TerminatedToday_Success()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(customer, account).Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      var service = Entity.Service.Health.ForDevice(device).BssPlanLineId(IdGen.StringId()).CancellationDate(DateTime.UtcNow).Save();

      var actionUTC = DateTime.UtcNow;
      message = BSS.DRBDeRegistered.IBKey(device.IBKey).ActionUtc(actionUTC).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsTrue(result.Success);

      var opDevice = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
      Assert.IsTrue(opDevice.DeregisteredUTC.Value.IsExactlyEqualTo(actionUTC));
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_DeRegisterDevice_ByTechnecian_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var actionUTC = DateTime.UtcNow;
      message = BSS.DRBDeRegistered.IBKey(device.IBKey).Status(DeviceRegistrationStatusEnum.DEREG_TECH).ActionUtc(actionUTC).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsTrue(result.Success);

      var opDevice = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
      Assert.AreEqual((int)DeviceStateEnum.DeregisteredTechnician, opDevice.fk_DeviceStateID);
      Assert.IsTrue(opDevice.DeregisteredUTC.Value.IsExactlyEqualTo(actionUTC));

      var plOut = Ctx.RawContext.PLOutReadOnly.Where(t => t.ModuleCode == device.GpsDeviceID).SingleOrDefault();
      Assert.IsNull(plOut);
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_DeRegisterDevice_ByStore_Success()
    {
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).SyncWithRpt().Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).SyncWithNhRaw().Save();
      var asset = Entity.Asset.WithDevice(device).SyncWithRpt().Save();

      var actionUTC = DateTime.UtcNow;
      message = BSS.DRBDeRegistered.IBKey(device.IBKey).ActionUtc(actionUTC).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsTrue(result.Success);

      var opDevice = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
      Assert.AreEqual((int)DeviceStateEnum.DeregisteredStore, opDevice.fk_DeviceStateID);
      Assert.IsTrue(opDevice.DeregisteredUTC.Value.IsExactlyEqualTo(actionUTC));

      var plOut = Ctx.RawContext.PLOutReadOnly.Where(t => t.ModuleCode == device.GpsDeviceID).Single();
      Assert.IsNotNull(plOut);
      Assert.AreEqual(plOut.Body, "2");
    }
  }
}
