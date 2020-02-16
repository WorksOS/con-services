using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceReplacementReplacedWorkflowTests : BssUnitTestBase
  {
    WorkflowResult result;
    DeviceReplacement message;

    [TestMethod]
    [DatabaseTest]
    public void DeviceReplacement_NewDevice_OldDevice_HasSameIbKey_Failure()
    {
      var owner = Entity.Customer.Dealer.Save();
      var oldDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).Save();

      var newDevice = Entity.Device.PL121.IbKey(IdGen.GetId().ToString()).Save();
      var asset = Entity.Asset.WithDevice(newDevice).Save();

      message = BSS.DRReplaced.OldIBKey(oldDevice.IBKey).NewIBKey(oldDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary,
            string.Format(BssConstants.DeviceReplacement.OLD_IBKEY_AND_NEW_IBKEY_ARE_EQUAL, message.OldIBKey, message.NewIBKey));
      TestHelper.AssertEngineOnOffReset(asset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_NewDeviceIsInactive_OldDeviceIsActive_Success()
    {
      var owner = Entity.Customer.Dealer.Save();
      var oldDevice = Entity.Device.MTS522.IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).Save();
      Entity.Service.Essentials.ForDevice(oldDevice).Save();

      var newDevice = Entity.Device.MTS522.OwnerBssId(owner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var asset = Entity.Asset.WithDevice(newDevice).Save();

      message = BSS.DRReplaced
        .OldIBKey(oldDevice.IBKey)
        .NewIBKey(newDevice.IBKey)
        .Build();

      result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success");
      AssertServicesTransferred(1, newDevice.ID, oldDevice.ID);
      AssertServiceViewsCreated(asset.AssetID);
      AssertDeviceState(oldDevice.ID, DeviceStateEnum.Provisioned);
      AssertDeviceState(newDevice.ID, DeviceStateEnum.Subscribed);
      TestHelper.AssertEngineOnOffReset(asset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_NewDeviceIsActive_OldDeviceIsActive_Failure()
    {
      var owner = Entity.Customer.Dealer.Save();
      var oldDevice = Entity.Device.MTS522.IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).Save();
      Entity.Service.Essentials.ForDevice(oldDevice).Save();

      var newDevice = Entity.Device.MTS522.OwnerBssId(owner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).Save();
      Entity.Service.Essentials.ForDevice(newDevice).Save();
      var asset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();

      message = BSS.DRReplaced
        .OldIBKey(oldDevice.IBKey)
        .NewIBKey(newDevice.IBKey)
        .Build();

      result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Failure");
      StringAssert.Contains(result.Summary,
            string.Format(BssConstants.DeviceReplacement.NEW_DEVICE_HAS_ACTIVE_SERVICES, message.NewIBKey));
      TestHelper.AssertEngineOnOffReset(asset.AssetID, true);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_NewDeviceIsInactive_OldDeviceIsInactive_Failure()
    {
      var owner = Entity.Customer.Dealer.Save();
      var oldDevice = Entity.Device.MTS522.IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();

      var newDevice = Entity.Device.MTS522.OwnerBssId(owner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var asset = Entity.Asset.WithDevice(newDevice).Save();

      message = BSS.DRReplaced
        .OldIBKey(oldDevice.IBKey)
        .NewIBKey(newDevice.IBKey)
        .Build();

      result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Failure");
      StringAssert.Contains(result.Summary,
            string.Format(BssConstants.DeviceReplacement.OLD_DEVICE_DOES_NOT_HAVE_ACTIVE_SERVICE, message.OldIBKey));
      TestHelper.AssertEngineOnOffReset(asset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_NewDeviceIsActive_OldDeviceIsInactive_Failure()
    {
      var owner = Entity.Customer.Dealer.Save();
      var oldDevice = Entity.Device.MTS522.IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();

      var newDevice = Entity.Device.MTS522.OwnerBssId(owner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).Save();
      Entity.Service.Essentials.ForDevice(newDevice).Save();
      var asset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();

      message = BSS.DRReplaced
        .OldIBKey(oldDevice.IBKey)
        .NewIBKey(newDevice.IBKey)
        .Build();

      result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Failure");
      StringAssert.Contains(result.Summary, 
            string.Format(BssConstants.DeviceReplacement.NEW_DEVICE_HAS_ACTIVE_SERVICES, message.NewIBKey));
      TestHelper.AssertEngineOnOffReset(asset.AssetID, true);
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceReplacement_DeviceType_DownGrade_WithIncompatiableActiveServices_Failure()
    {
      var owner = Entity.Customer.Dealer.Save();
      var oldDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).DeviceState(DeviceStateEnum.Subscribed).Save();
      Entity.Service.Essentials.ForDevice(oldDevice).Save();
      Entity.Service.Health.ForDevice(oldDevice).Save();

      var newDevice = Entity.Device.PL121.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var asset = Entity.Asset.WithDevice(newDevice).Save();

      message = BSS.DRReplaced.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.Summary,
            string.Format(BssConstants.DeviceReplacement.NEW_DEVICE_DOES_NOT_SUPPORT_OLD_DEVICE_SERVICES, message.NewIBKey, message.OldIBKey));
      TestHelper.AssertEngineOnOffReset(asset.AssetID, false);
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceReplacement_DeviceType_DownGrade_WithCompatiableActiveServices_Success()
    {
      var owner = Entity.Customer.Dealer.Save();
      var oldDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).DeviceState(DeviceStateEnum.Subscribed).Save();
      Entity.Service.Essentials.ForDevice(oldDevice).Save();

      var newDevice = Entity.Device.PL121.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var asset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();

      message = BSS.DRReplaced.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success);
      AssertServicesTransferred(1, newDevice.ID, oldDevice.ID);
      AssertServiceViewsCreated(asset.AssetID);
      AssertDeviceState(oldDevice.ID, DeviceStateEnum.Provisioned);
      AssertDeviceState(newDevice.ID, DeviceStateEnum.Subscribed);
      TestHelper.AssertEngineOnOffReset(asset.AssetID, false);
    }

    [Ignore]
    [TestMethod]
    [DatabaseTest]
    public void DeviceReplacement_NoDeviceTypeChange_WithCompatiableActiveServices_Success()
    {
      var owner = Entity.Customer.Dealer.Save();
      var oldDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
      Entity.Service.Essentials.ForDevice(oldDevice).Save();
      Entity.Service.Health.ForDevice(oldDevice).Save();

      var newDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var asset = Entity.Asset.WithDevice(newDevice).Save();
      
      message = BSS.DRReplaced.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success);
      AssertServicesTransferred(2, newDevice.ID, oldDevice.ID);
      AssertServiceViewsCreated(asset.AssetID);
      AssertDeviceState(oldDevice.ID, DeviceStateEnum.Provisioned);
      AssertDeviceState(newDevice.ID, DeviceStateEnum.Subscribed);      
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_NewDeviceIsInactive_DeregStore_OldDeviceIsActive_Success()
    {
      var owner = Entity.Customer.Dealer.Save();
      var oldDevice = Entity.Device.MTS522.IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).Save();
      Entity.Service.Essentials.ForDevice(oldDevice).Save();

      var newDevice = Entity.Device.MTS522.OwnerBssId(owner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.DeregisteredStore).Save();
      var asset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();

      message = BSS.DRReplaced
        .OldIBKey(oldDevice.IBKey)
        .NewIBKey(newDevice.IBKey)
        .Build();

      result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success");
      AssertServicesTransferred(1, newDevice.ID, oldDevice.ID);
      AssertServiceViewsCreated(asset.AssetID);
      AssertDeviceState(oldDevice.ID, DeviceStateEnum.Provisioned);
      AssertDeviceState(newDevice.ID, DeviceStateEnum.DeregisteredStore);
      TestHelper.AssertEngineOnOffReset(asset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_NewDeviceIsInactive_DeregTech_OldDeviceIsActive_Success()
    {
      var owner = Entity.Customer.Dealer.Save();
      var oldDevice = Entity.Device.MTS522.IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).Save();
      Entity.Service.Essentials.ForDevice(oldDevice).Save();

      var newDevice = Entity.Device.MTS522.OwnerBssId(owner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.DeregisteredTechnician).Save();
      var asset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();

      message = BSS.DRReplaced
        .OldIBKey(oldDevice.IBKey)
        .NewIBKey(newDevice.IBKey)
        .Build();

      result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success");
      AssertServicesTransferred(1, newDevice.ID, oldDevice.ID);
      AssertServiceViewsCreated(asset.AssetID);
      AssertDeviceState(oldDevice.ID, DeviceStateEnum.Provisioned);
      AssertDeviceState(newDevice.ID, DeviceStateEnum.DeregisteredTechnician);
      TestHelper.AssertEngineOnOffReset(asset.AssetID, false);
    }

    #region Private Methods

    private void AssertServicesTransferred(int expectedTransferredServicesCount, long newDeviceID, long oldDeviceID)
    {
      var transferredServices = Ctx.OpContext.ServiceReadOnly.Where(x => x.fk_DeviceID == newDeviceID).ToList();

      Assert.AreEqual(expectedTransferredServicesCount, transferredServices.Count, "Expected Transferred Services");

      foreach (var service in transferredServices)
      {
        Assert.AreEqual(service.CancellationKeyDate, DotNetExtensions.NullKeyDate, "CancellationKeyDate");
      }

      var oldDeviceServicesExist = Ctx.OpContext.ServiceReadOnly.Any(x => x.fk_DeviceID == oldDeviceID);

      Assert.IsFalse(oldDeviceServicesExist, "Old Device Services");

    }

    private void AssertServiceViewsCreated(long assetId)
    {
      var views = Ctx.OpContext.ServiceViewReadOnly.Where(x => x.fk_AssetID == assetId).ToList();

      Assert.IsTrue(views.Count > 0, "View Count");

      foreach (var view in views)
      {
        Assert.AreEqual(DotNetExtensions.NullKeyDate, view.EndKeyDate);
      }
    }    

    private void AssertDeviceState(long deviceId, DeviceStateEnum expectedDeviceState)
    {
      var actualDeviceState = (DeviceStateEnum)Ctx.OpContext.DeviceReadOnly.First(x => x.ID == deviceId).fk_DeviceStateID;
      Assert.AreEqual(expectedDeviceState, actualDeviceState, "Device State");
    }

    #endregion
  }
}
