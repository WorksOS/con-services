using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceRegistrationRegisteredWorkflowTests : BssUnitTestBase
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
    public void DR_IBKeyDoesNotExist_Failure()
    {
      message = BSS.DRBRegistered.Build();
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
      message = BSS.DRBRegistered.IBKey(device.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.DEVICE_NOT_ASSOCIATED_WITH_VALID_CUSTOMER, message.IBKey), "Summary is expected to contain the exception.");
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_DeviceAlreadyRegistered_Subscribed_Failure()
    {
      var device = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      message = BSS.DRBRegistered.IBKey(device.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.DeviceRegistration.DEVICE_ALREADY_DEREGISTERED, message.IBKey, "Registered"), "Summary is expected to contain the exception.");
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_DeviceAlreadyRegistered_Provisioned_Failure()
    {
      var device = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      message = BSS.DRBRegistered.IBKey(device.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.DeviceRegistration.DEVICE_ALREADY_DEREGISTERED, message.IBKey, "Registered"), "Summary is expected to contain the exception.");
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_RegisterNonPLDevice_Failure()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.IbKey(IdGen.StringId()).OwnerBssId(owner.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      message = BSS.DRBRegistered.IBKey(device.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.DeviceRegistration.DEVICE_REGISTRATION_NOT_SUPPORTED, DeviceTypeEnum.Series521), "Summary is expected to contain the exception.");
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_Status_DEREG_STORE_Invalid_ForDeviceRegister_Failure()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(owner.BSSID).DeviceState(DeviceStateEnum.DeregisteredStore).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      message = BSS.DRBRegistered.IBKey(device.IBKey).Status(DeviceRegistrationStatusEnum.DEREG_STORE).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.DeviceRegistration.DEVICE_STATUS_NOT_VALID, message.Status, "Registered"), "Summary is expected to contain the exception.");
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_Status_DEREG_TECH_Invalid_ForDeviceRegister_Failure()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(owner.BSSID).DeviceState(DeviceStateEnum.DeregisteredTechnician).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      message = BSS.DRBRegistered.IBKey(device.IBKey).Status(DeviceRegistrationStatusEnum.DEREG_TECH).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary, string.Format(BssConstants.DeviceRegistration.DEVICE_STATUS_NOT_VALID, message.Status, "Registered"), "Summary is expected to contain the exception.");
    }


    [DatabaseTest]
    [TestMethod]
    public void DR_NoServicePlanExists_StateUpdatedToProvisioned_Success()
    {
      var savedDevice = UpdateDevice(createService: false);
      Assert.AreEqual((int)DeviceStateEnum.Provisioned, savedDevice.fk_DeviceStateID);
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_ExpiredCore_StateUpdatedToProvisioned_Success()
    {
      var savedDevice = UpdateDevice(isCore: true, isActiveService: false);
      Assert.AreEqual((int)DeviceStateEnum.Provisioned, savedDevice.fk_DeviceStateID);
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_ActiveCore_StateUpdatedToSubscribed_Success()
    {
      var savedDevice = UpdateDevice();
      Assert.AreEqual((int)DeviceStateEnum.Subscribed, savedDevice.fk_DeviceStateID);
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_ExpiredHealth_StateUpdatedToProvisioned_Success()
    {
      var savedDevice = UpdateDevice(isCore: false, isActiveService: false);
      Assert.AreEqual((int)DeviceStateEnum.Provisioned, savedDevice.fk_DeviceStateID);
    }

    [DatabaseTest]
    [TestMethod]
    public void DR_ActiveHealth_StateUpdatedToProvisioned_Success()
    {
      var savedDevice = UpdateDevice(isCore: false);
      Assert.AreEqual((int)DeviceStateEnum.Provisioned, savedDevice.fk_DeviceStateID);
    }

    private Device UpdateDevice(bool createService = true, bool isCore = true, bool isActiveService = true)
    {
      //set up the entities in the system
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var owner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, owner).Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(owner.BSSID).DeviceState(DeviceStateEnum.DeregisteredTechnician).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      Service service;

      //create a service as per the requirement
      if (createService)
      {
        if (isCore)
        {
          if (isActiveService)
            service = Entity.Service.Essentials.BssPlanLineId(IdGen.StringId()).ForDevice(device)
              .WithView(t => t.ForAsset(asset).ForCustomer(customer)).Save();
          else
            service = Entity.Service.Essentials.BssPlanLineId(IdGen.StringId()).ForDevice(device).CancellationDate(DateTime.UtcNow.AddDays(-20))
              .WithView(t => t.ForAsset(asset).ForCustomer(customer).StartsOn(DateTime.UtcNow.AddDays(-20)).EndsOn(DateTime.UtcNow.AddDays(-10))).Save();
        }
        else
        {
          if (isActiveService)
            service = Entity.Service.Health.BssPlanLineId(IdGen.StringId()).ForDevice(device)
              .WithView(t => t.ForAsset(asset).ForCustomer(customer)).Save();
          else
            service = Entity.Service.Health.BssPlanLineId(IdGen.StringId()).ForDevice(device).CancellationDate(DateTime.UtcNow.AddDays(-20))
              .WithView(t => t.ForAsset(asset).ForCustomer(customer).StartsOn(DateTime.UtcNow.AddDays(-20)).EndsOn(DateTime.UtcNow.AddDays(-10))).Save();
        }
      }

      //adding the service plan might changed the device state to provisioned/subscribed, 
      //so update the device state to deregistered, and verfiy the same
      device = Ctx.OpContext.Device.Where(t => t.ID == device.ID).Single();
      device.fk_DeviceStateID = (int)DeviceStateEnum.DeregisteredTechnician;
      Ctx.OpContext.SaveChanges();

      device = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
      Assert.AreEqual((int)DeviceStateEnum.DeregisteredTechnician, device.fk_DeviceStateID);

      //invoke the workflow the the message
      message = BSS.DRBRegistered.IBKey(device.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(message);
      Assert.IsTrue(result.Success);

      //retun the updated device
      return Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
    }
  }
}
