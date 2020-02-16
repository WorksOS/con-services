using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceReplacementSwappedWorkflowTests : BssUnitTestBase
  {

    WorkflowResult result;
    DeviceReplacement message;

    [TestCleanup]
    public void TestCleanup()
    {
      if (result == null) return;

      new ConsoleResultProcessor().Process(message, result);
    }

    [TestMethod]
    public void SwapDevice_NewAssetDoesNotExists_Failure()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var asset = Entity.Asset.WithDevice(oldDevice).IsEngineStartStopSupported(true).Save();
      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);

      Assert.IsFalse(result.Success, "The message is expected to be failed.");
      StringAssert.Contains(result.Summary, "No Asset is associated with");
      TestHelper.AssertEngineOnOffReset(asset.AssetID, true);
    }

    [TestMethod]
    public void SwapDevice_OldAssetDoesNotExists_Failure()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var asset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);

      Assert.IsFalse(result.Success, "The message is expected to be failed.");
      StringAssert.Contains(result.Summary, "No Asset is associated with");
      TestHelper.AssertEngineOnOffReset(asset.AssetID, true);
    }

    [TestMethod]
    public void SwapDevice_OwnerBSSIDsDifferent_Failure()
    {
      var oldOwner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(oldOwner.BSSID).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).IsEngineStartStopSupported(true).Save();

      var newOwner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(newOwner.BSSID).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).Save();

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);

      Assert.IsFalse(result.Success, "The message is expected to be failed.");
      StringAssert.Contains(result.Summary, "OwnerBssID is different for");
      TestHelper.AssertEngineOnOffReset(oldAsset.AssetID, true);
      TestHelper.AssertEngineOnOffReset(newAsset.AssetID, false);
    }

    #region same device type

    [DatabaseTest]
    [TestMethod]
    public void SwapDevice_AssetWithDeviceExists_NoActiveServiceViewsExists_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).IsEngineStartStopSupported(true).Save();

      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      AssertAssetSwap(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID);
      AssertAssetDeviceHistory(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID, owner.BSSID);
      AssertServiceViews(oldAsset.AssetID, newAsset.AssetID, owner.ID, 0);

      TestHelper.AssertEngineOnOffReset(oldAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(newAsset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void SwapDevice_AssetWithDeviceExists_OldAssetHasActiveServiceViewsExists_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).IsEngineStartStopSupported(true).Save();
      var essentials = Entity.Service.Essentials.ForDevice(oldDevice).WithView(t => t.ForAsset(oldAsset).ForCustomer(owner)).Save();

      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      AssertAssetSwap(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID);
      AssertAssetDeviceHistory(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID, owner.BSSID);
      AssertServiceViews(oldAsset.AssetID, newAsset.AssetID, owner.ID, 1);

      TestHelper.AssertEngineOnOffReset(oldAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(newAsset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void SwapDevice_AssetWithDeviceExists_NewAssetHasActiveServiceViewsExists_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).IsEngineStartStopSupported(true).Save();

      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();
      var essentials = Entity.Service.Essentials.ForDevice(newDevice).WithView(t => t.ForAsset(newAsset).ForCustomer(owner)).Save();

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      AssertAssetSwap(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID);
      AssertAssetDeviceHistory(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID, owner.BSSID);
      AssertServiceViews(oldAsset.AssetID, newAsset.AssetID, owner.ID, 2);

      TestHelper.AssertEngineOnOffReset(oldAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(newAsset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void SwapDevice_AssetWithDeviceExists_BothAssetsHasActiveServiceViewsExists_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).IsEngineStartStopSupported(true).Save();
      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();

      var sp = BSS.SPActivated.IBKey(newDevice.IBKey).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(sp);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");
      
      sp = BSS.SPActivated.IBKey(oldDevice.IBKey).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(sp);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      AssertAssetSwap(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID);
      AssertAssetDeviceHistory(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID, owner.BSSID);
      AssertServiceViews(oldAsset.AssetID, newAsset.AssetID, owner.ID, flag: 0);
      AssertDeviceState(oldAsset.AssetID, newAsset.AssetID, (int)DeviceStateEnum.Subscribed, (int)DeviceStateEnum.Subscribed);
      
      TestHelper.AssertEngineOnOffReset(oldAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(newAsset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void SwapDevice_AssetWithDeviceExists_OldAssetHasActiveMultipleServiceViewsExists_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).IsEngineStartStopSupported(true).Save();
      var essentials = Entity.Service.Essentials.ForDevice(oldDevice).WithView(t => t.ForAsset(oldAsset).ForCustomer(owner)).Save();
      var health = Entity.Service.Health.ForDevice(oldDevice).WithView(t => t.ForAsset(oldAsset).ForCustomer(owner)).Save();

      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      AssertAssetSwap(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID);
      AssertAssetDeviceHistory(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID, owner.BSSID);
      AssertServiceViews(oldAsset.AssetID, newAsset.AssetID, owner.ID, 1, health.ID, 0);

      TestHelper.AssertEngineOnOffReset(oldAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(newAsset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void SwapDevice_AssetWithDeviceExists_NewAssetHasActiveMultipleServiceViewsExists_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).IsEngineStartStopSupported(true).Save();

      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();
      var essentials = Entity.Service.Essentials.ForDevice(newDevice).WithView(t => t.ForAsset(newAsset).ForCustomer(owner)).Save();
      var health = Entity.Service.Health.ForDevice(newDevice).WithView(t => t.ForAsset(newAsset).ForCustomer(owner)).Save();

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      AssertAssetSwap(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID);
      AssertAssetDeviceHistory(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID, owner.BSSID);
      AssertServiceViews(oldAsset.AssetID, newAsset.AssetID, owner.ID, 2, 0, health.ID);

      TestHelper.AssertEngineOnOffReset(oldAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(newAsset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void SwapDevice_AssetWithDeviceExists_BothAssetsHasActiveMultipleServiceViewsExists_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).IsEngineStartStopSupported(true).Save();
      var oldEssentials = Entity.Service.Essentials.ForDevice(oldDevice).WithView(t => t.ForAsset(oldAsset).ForCustomer(owner)).Save();
      var oldHealth = Entity.Service.Health.ForDevice(oldDevice).WithView(t => t.ForAsset(oldAsset).ForCustomer(owner)).Save();

      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();
      var newEssentials = Entity.Service.Essentials.ForDevice(newDevice).WithView(t => t.ForAsset(newAsset).ForCustomer(owner)).Save();
      var newHealth = Entity.Service.Health.ForDevice(newDevice).WithView(t => t.ForAsset(newAsset).ForCustomer(owner)).Save();

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      AssertAssetSwap(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID);
      AssertAssetDeviceHistory(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID, owner.BSSID);
      AssertServiceViews(oldAsset.AssetID, newAsset.AssetID, owner.ID, 3, oldEssentials.ID, newEssentials.ID, oldHealth.ID, newHealth.ID);

      TestHelper.AssertEngineOnOffReset(oldAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(newAsset.AssetID, false);
    }

    #endregion

    #region different device type
    [DatabaseTest]
    [TestMethod]
    public void SwapDevice_AssetWithDeviceExists_OldAssetHasActiveServiceViewsExists_DifferentDeviceType_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.PL121.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).IsEngineStartStopSupported(true).Save();
      var essentials = Entity.Service.Essentials.ForDevice(oldDevice).WithView(t => t.ForAsset(oldAsset).ForCustomer(owner)).Save();

      var newDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      AssertAssetSwap(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID);
      AssertAssetDeviceHistory(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID, owner.BSSID);
      AssertServiceViews(oldAsset.AssetID, newAsset.AssetID, owner.ID, 1);

      TestHelper.AssertEngineOnOffReset(oldAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(newAsset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void SwapDevice_AssetWithDeviceExists_NewAssetHasActiveServiceViewsExists_DifferentDeviceType_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).IsEngineStartStopSupported(true).Save();

      var newDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();
      var essentials = Entity.Service.Essentials.ForDevice(newDevice).WithView(t => t.ForAsset(newAsset).ForCustomer(owner)).Save();

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      AssertAssetSwap(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID);
      AssertAssetDeviceHistory(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID, owner.BSSID);
      AssertServiceViews(oldAsset.AssetID, newAsset.AssetID, owner.ID, 2);

      TestHelper.AssertEngineOnOffReset(oldAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(newAsset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void SwapDevice_AssetWithDeviceExists_BothAssetsHasActiveServiceViewsExists_DifferentDeviceType_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).IsEngineStartStopSupported(true).Save();
      var oldEssentials = Entity.Service.Essentials.ForDevice(oldDevice).WithView(t => t.ForAsset(oldAsset).ForCustomer(owner)).Save();

      var newDevice = Entity.Device.PL121.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();
      var newEssentials = Entity.Service.Essentials.ForDevice(newDevice).WithView(t => t.ForAsset(newAsset).ForCustomer(owner)).Save();

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      AssertAssetSwap(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID);
      AssertAssetDeviceHistory(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID, owner.BSSID);
      AssertServiceViews(oldAsset.AssetID, newAsset.AssetID, owner.ID, 3, oldEssentials.ID, newEssentials.ID);

      TestHelper.AssertEngineOnOffReset(oldAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(newAsset.AssetID, false);
    }
    #endregion

    #region multiple actions

    [DatabaseTest]
    [TestMethod]
    public void SwapDevice_AssetWithDeviceExists_NewAssetHasActiveServiceViewsExists_DeregisterOldDevice_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.GetId().ToString()).Save();
      var oldDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var oldAsset = Entity.Asset.WithDevice(oldDevice).IsEngineStartStopSupported(true).Save();

      var newDevice = Entity.Device.PL321.IbKey(IdGen.GetId().ToString()).OwnerBssId(owner.BSSID).Save();
      var newAsset = Entity.Asset.WithDevice(newDevice).IsEngineStartStopSupported(true).Save();
      var sp = BSS.SPActivated.IBKey(newDevice.IBKey).ServicePlanName("89500-00").Build();
      result = TestHelper.ExecuteWorkflow<ServicePlan>(sp);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      var deregister = BSS.DRBDeRegistered.IBKey(oldDevice.IBKey).Status(DeviceRegistrationStatusEnum.DEREG_TECH).Build();
      result = TestHelper.ExecuteWorkflow<DeviceRegistration>(deregister);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      message = BSS.DRSwapped.OldIBKey(oldDevice.IBKey).NewIBKey(newDevice.IBKey).Build();
      result = TestHelper.ExecuteWorkflow<DeviceReplacement>(message);
      Assert.IsTrue(result.Success, "The message is expected to be pass.");

      AssertAssetSwap(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID);
      AssertAssetDeviceHistory(oldAsset.AssetID, newAsset.AssetID, oldDevice.ID, newDevice.ID, owner.BSSID);
      AssertServiceViews(oldAsset.AssetID, newAsset.AssetID, owner.ID, flag: 0);
      AssertDeviceState(oldAsset.AssetID, newAsset.AssetID, (int)DeviceStateEnum.Subscribed, (int)DeviceStateEnum.DeregisteredTechnician);

      TestHelper.AssertEngineOnOffReset(oldAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(newAsset.AssetID, false);
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceSwap_ExistingDeivceOnAsset_Subscribed_HasActiveCore_IBDeviceInstalledOnAsset_ServiceSwapped()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(customer, account).Save();
      var device1 = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).Save();
      var asset1 = Entity.Asset.WithDevice(device1).IsEngineStartStopSupported(true).Save();

      var sp = BSS.SPActivated.IBKey(device1.IBKey).ServicePlanName(TestHelper.GetPartNumberByServiceType(ServiceTypeEnum.Essentials)).Build();
      var result = TestHelper.ExecuteWorkflow(sp);
      Assert.IsTrue(result.Success, "Success should be true");

      var device2 = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).Save();
      var asset2 = Entity.Asset.WithDevice(device2).IsEngineStartStopSupported(true).Save();

      sp = BSS.SPActivated.IBKey(device2.IBKey).ServicePlanName(TestHelper.GetPartNumberByServiceType(ServiceTypeEnum.Essentials)).Build();
      result = TestHelper.ExecuteWorkflow(sp);
      Assert.IsTrue(result.Success, "Success should be true");

      var dr = BSS.DRSwapped.NewIBKey(device2.IBKey).OldIBKey(device1.IBKey).Build();
      result = TestHelper.ExecuteWorkflow(dr);
      Assert.IsTrue(result.Success, "Success should be true");

      var oldAndNew = (from oldD in Ctx.OpContext.DeviceReadOnly
                       from newD in Ctx.OpContext.DeviceReadOnly
                       where oldD.ID == device1.ID && newD.ID == device2.ID
                       select new
                       {
                         oldState = oldD.fk_DeviceStateID,
                         newState = newD.fk_DeviceStateID
                       }).Single();

      Assert.AreEqual((int)DeviceStateEnum.Subscribed, oldAndNew.oldState);
      Assert.AreEqual((int)DeviceStateEnum.Subscribed, oldAndNew.newState);

      //after swapping, the asset1 should be holding device2, and asset2 should be holding the device1
      var assetDeviceIDs = (from oldA in Ctx.OpContext.AssetReadOnly
                    from newA in Ctx.OpContext.AssetReadOnly
                    where oldA.AssetID == asset1.AssetID && newA.AssetID == asset2.AssetID
                    select new
                    {
                      oldDeviceID = oldA.fk_DeviceID,
                      newDeviceID = newA.fk_DeviceID
                    }).Single();

      Assert.AreEqual(assetDeviceIDs.oldDeviceID, device2.ID);
      Assert.AreEqual(assetDeviceIDs.newDeviceID, device1.ID);

      TestHelper.AssertEngineOnOffReset(asset1.AssetID, false);
      TestHelper.AssertEngineOnOffReset(asset2.AssetID, false);
    }

    [TestMethod]
    [DatabaseTest]
    public void DeviceSwap_ExistingDeivceOnAsset_Deregistered_HasActiveCore_IBDeviceInstalledOnAsset_ServiceSwapped()
    {
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var account = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(customer, account).Save();
      var device1 = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).DeviceState(DeviceStateEnum.DeregisteredTechnician).Save();
      var asset1 = Entity.Asset.WithDevice(device1).IsEngineStartStopSupported(true).Save();

      var device2 = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(account.BSSID).Save();
      var asset2 = Entity.Asset.WithDevice(device2).IsEngineStartStopSupported(true).Save();

      var sp = BSS.SPActivated.IBKey(device2.IBKey).ServicePlanName(TestHelper.GetPartNumberByServiceType(ServiceTypeEnum.Essentials)).Build();
      var result = TestHelper.ExecuteWorkflow(sp);
      Assert.IsTrue(result.Success, "Success should be true");

      var dr = BSS.DRSwapped.NewIBKey(device2.IBKey).OldIBKey(device1.IBKey).Build();
      result = TestHelper.ExecuteWorkflow(dr);
      Assert.IsTrue(result.Success, "Success should be true");

      var oldAndNew = (from oldD in Ctx.OpContext.DeviceReadOnly
                       from newD in Ctx.OpContext.DeviceReadOnly
                       where oldD.ID == device1.ID && newD.ID == device2.ID
                       select new
                       {
                         oldState = oldD.fk_DeviceStateID,
                         newState = newD.fk_DeviceStateID
                       }).Single();
      Assert.AreEqual((int)DeviceStateEnum.DeregisteredTechnician, oldAndNew.oldState);
      Assert.AreEqual((int)DeviceStateEnum.Subscribed, oldAndNew.newState);

      //after swapping, the asset1 should be holding device2, and asset2 should be holding the device1
      var assetDeviceIDs = (from oldA in Ctx.OpContext.AssetReadOnly
                            from newA in Ctx.OpContext.AssetReadOnly
                            where oldA.AssetID == asset1.AssetID && newA.AssetID == asset2.AssetID
                            select new
                            {
                              oldDeviceID = oldA.fk_DeviceID,
                              newDeviceID = newA.fk_DeviceID
                            }).Single();

      Assert.AreEqual(assetDeviceIDs.oldDeviceID, device2.ID);
      Assert.AreEqual(assetDeviceIDs.newDeviceID, device1.ID);

      TestHelper.AssertEngineOnOffReset(asset1.AssetID, false);
      TestHelper.AssertEngineOnOffReset(asset2.AssetID, false);
    }

    #endregion

    #region Private Methods

    private void AssertAssetSwap(long oldAssetID, long newAssetID, long oldDeviceID, long newDeviceID)
    {
      var assetIds = new List<long> { oldAssetID, newAssetID };

      var assets = (from a in Ctx.OpContext.AssetReadOnly
                    where assetIds.Contains(a.AssetID)
                    select new { a.AssetID, a.fk_DeviceID }).ToList();

      var asset = assets.Where(t => t.AssetID == oldAssetID).FirstOrDefault();
      Assert.IsNotNull(asset);
      Assert.AreEqual(asset.fk_DeviceID, newDeviceID, "Deive IDs should match");

      asset = assets.Where(t => t.AssetID == newAssetID).FirstOrDefault();
      Assert.IsNotNull(asset);
      Assert.AreEqual(asset.fk_DeviceID, oldDeviceID, "Deive IDs should match");
    }

    private void AssertAssetDeviceHistory(long oldAssetID, long newAssetID, long oldDeviceID, long newDeviceID, string ownerBSSID)
    {
      var assetIds = new List<long> { oldAssetID, newAssetID };
      var deviceIds = new List<long> { oldDeviceID, newDeviceID };

      var assetHistory = (from ah in Ctx.OpContext.AssetDeviceHistoryReadOnly
                          where assetIds.Contains(ah.fk_AssetID) && deviceIds.Contains(ah.fk_DeviceID)
                          select ah).ToList();

      var ahs = assetHistory.Where(t => t.fk_AssetID == oldAssetID && t.fk_DeviceID == oldDeviceID && t.OwnerBSSID == ownerBSSID).FirstOrDefault();
      Assert.IsNotNull(ahs);

      ahs = assetHistory.Where(t => t.fk_AssetID == newAssetID && t.fk_DeviceID == newDeviceID && t.OwnerBSSID == ownerBSSID).FirstOrDefault();
      Assert.IsNotNull(ahs);
    }

    private void AssertServiceViewsTerminated(List<ServiceView> svs, long assetID, long svID)
    {
      var sv = svs.Where(t => t.fk_AssetID == assetID && t.fk_ServiceID == svID).FirstOrDefault();
      Assert.IsNotNull(sv);
      Assert.AreEqual(DateTime.UtcNow.KeyDate(), sv.EndKeyDate, "End date expected be today.");
    }

    private void AssertServiceViewsCreated(List<ServiceView> svs, long assetID, long svID)
    {
      var sv = svs.Where(t => t.fk_AssetID == assetID && t.fk_ServiceID == svID).FirstOrDefault();
      Assert.IsNotNull(sv);
      Assert.AreEqual(DotNetExtensions.NullKeyDate, sv.EndKeyDate, "End date is expected to be equal to null keydate.");
      Assert.AreEqual(DateTime.UtcNow.KeyDate(), sv.StartKeyDate, "Start date is expected to be today.");
    }

    private void AssertServiceViews(long oldAssetID, long newAssetID, long customerID, int flag, long oldessentialID = 0, long newessentialID = 0, long oldhealthid = 0, long newhealthid = 0)
    {
      var assetIds = new List<long> { oldAssetID, newAssetID };

      var svs = (from s in Ctx.OpContext.ServiceViewReadOnly
                 where assetIds.Contains(s.fk_AssetID) && s.fk_CustomerID == customerID
                 select s).ToList();

      if (flag == 0)
        Assert.AreEqual(0, svs.Count());
      else
      {
        Assert.AreNotEqual(0, svs.Count());
        if (flag == 1)
        {
          if (oldessentialID != 0)
            AssertServiceViewsTerminated(svs, oldAssetID, oldessentialID);

          if (newessentialID != 0)
            AssertServiceViewsCreated(svs, newAssetID, newessentialID);
          if (oldhealthid != 0)
            AssertServiceViewsTerminated(svs, oldAssetID, oldhealthid);
          if (newhealthid != 0)
            AssertServiceViewsCreated(svs, newAssetID, newhealthid);
        }
        else if (flag == 2)
        {
          if (oldessentialID != 0)
            AssertServiceViewsCreated(svs, oldAssetID, oldessentialID);
          if (newessentialID != 0)
            AssertServiceViewsTerminated(svs, newAssetID, newessentialID);

          if (oldhealthid != 0)
            AssertServiceViewsCreated(svs, oldAssetID, oldhealthid);
          if (newhealthid != 0)
            AssertServiceViewsTerminated(svs, newAssetID, newhealthid);
        }
        else if (flag == 3)
        {
          AssertServiceViewsTerminated(svs, oldAssetID, oldessentialID);
          AssertServiceViewsCreated(svs, oldAssetID, newessentialID);
          AssertServiceViewsTerminated(svs, newAssetID, newessentialID);
          AssertServiceViewsCreated(svs, newAssetID, oldessentialID);

          if (oldhealthid != 0 && newhealthid != 0)
          {
            AssertServiceViewsTerminated(svs, oldAssetID, oldhealthid);
            AssertServiceViewsCreated(svs, oldAssetID, newhealthid);
            AssertServiceViewsTerminated(svs, newAssetID, newhealthid);
            AssertServiceViewsCreated(svs, newAssetID, oldhealthid);
          }
        }
      }
    }

    private void AssertDeviceState(long oldAssetID, long newAssetID, int expectedOldDeviceState, int expectedNewDeviceState)
    {
      var assetIDs = new List<long> { newAssetID, oldAssetID };
      var devices = (from d in Ctx.OpContext.DeviceReadOnly
                     join a in Ctx.OpContext.AssetReadOnly on d.ID equals a.fk_DeviceID
                     where assetIDs.Contains(a.AssetID)
                     select new { d.fk_DeviceStateID, a.AssetID }).ToList();
      var device = devices.Where(t => t.AssetID == oldAssetID).Single();
      Assert.AreEqual(device.fk_DeviceStateID, expectedOldDeviceState, "DeviceStates are expected to match.");
      device = devices.Where(t => t.AssetID == newAssetID).Single();
      Assert.AreEqual(device.fk_DeviceStateID, expectedNewDeviceState, "DeviceStates are expected to match.");
    }

    #endregion
  }
}
