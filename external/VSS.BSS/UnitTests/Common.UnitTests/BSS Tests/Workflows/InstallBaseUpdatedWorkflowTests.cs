using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class InstallBaseUpdatedWorkflowTests : BssUnitTestBase
  {
    [DatabaseTest]
    [TestMethod]
    public void IBAssetExists_UpdateProperties_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(IBOwner.BSSID).IbKey(IdGen.StringId()).Save();
      var IBAsset = Entity.Asset.WithDevice(IBDevice).ModelName("OLD_MODEL_NAME").ManufactureYear(2000).Name("OLD_NAME").IsEngineStartStopSupported(true).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode).Build(); // IBAsset
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      var asset = Ctx.OpContext.AssetReadOnly.FirstOrDefault(x => x.AssetID == IBAsset.AssetID);
      Assert.AreEqual(message.EquipmentLabel, asset.Name);
      Assert.AreEqual(message.ModelYear, asset.ManufactureYear.ToString());
      Assert.AreEqual(message.EquipmentVIN, asset.EquipmentVIN);
      TestHelper.AssertEngineOnOffReset(asset.AssetID, true);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
    }

    [DatabaseTest]
    [TestMethod]
    public void IBAssetExists_UpdateProperties_SerialNumberHasTrailingSpace_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(IBOwner.BSSID).IbKey(IdGen.StringId()).Save();
      var IBAsset = Entity.Asset.WithDevice(IBDevice).ModelName("OLD_MODEL_NAME").ManufactureYear(2000).Name("OLD_NAME").IsEngineStartStopSupported(true).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN + " ").MakeCode(IBAsset.fk_MakeCode).Build(); // IBAsset
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      var asset = Ctx.OpContext.AssetReadOnly.FirstOrDefault(x => x.AssetID == IBAsset.AssetID);
      Assert.AreEqual(message.EquipmentLabel, asset.Name);
      Assert.AreEqual(message.ModelYear, asset.ManufactureYear.ToString());
      Assert.AreEqual(message.EquipmentVIN, asset.EquipmentVIN);
      TestHelper.AssertEngineOnOffReset(asset.AssetID, true);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
    }

    [DatabaseTest]
    [TestMethod]
    public void IBAssetExists_CaterpillarAsset_UpdateProperties_ModelInSalesModelTable_ModelDoesNotGetUpdated_Success()
    {
      //Add Sales Model
      var productFamily = Entity.ProductFamily.Save();
      var salesModel = Entity.SalesModel.ForProductFamily(productFamily).Save();
      var assetSerialNumber = string.Format("{0}00123", salesModel.SerialNumberPrefix);

      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(IBOwner.BSSID).IbKey(IdGen.StringId()).Save();
      var IBAsset = Entity.Asset.WithDevice(IBDevice).MakeCode("CAT").SerialNumberVin(assetSerialNumber).ModelName(salesModel.Description).ManufactureYear(2000).Name("OLD_NAME").IsEngineStartStopSupported(true).Save();

      // new model name
      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode)
        .EquipmentLabel("NEW_NAME").Model("NEW_MODEL_NAME").Build(); // IBAsset
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      var asset = Ctx.OpContext.AssetReadOnly.FirstOrDefault(x => x.AssetID == IBAsset.AssetID);
      Assert.AreEqual(message.EquipmentLabel, asset.Name);
      Assert.AreEqual(salesModel.Description, asset.Model, "The sales model should not have been updated");
      TestHelper.AssertEngineOnOffReset(asset.AssetID, true);
    }

    [DatabaseTest]
    [TestMethod]
    public void IBAssetExists_CaterpillarAsset_UpdateProperties_ModelNotInSalesModelTable_ModelDoesGetUpdated_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(IBOwner.BSSID).IbKey(IdGen.StringId()).Save();
      var IBAsset = Entity.Asset.WithDevice(IBDevice).MakeCode("CAT").ModelName("OLD_MODEL_NAME").ManufactureYear(2000).Name("OLD_NAME").IsEngineStartStopSupported(true).Save();

      // new model name
      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode)
        .EquipmentLabel("NEW_NAME").Model("NEW_MODEL_NAME").Build(); // IBAsset
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      var asset = Ctx.OpContext.AssetReadOnly.FirstOrDefault(x => x.AssetID == IBAsset.AssetID);
      Assert.AreEqual(message.EquipmentLabel, asset.Name);
      Assert.AreEqual("NEW_MODEL_NAME", asset.Model);
      TestHelper.AssertEngineOnOffReset(asset.AssetID, true);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
    }

    [DatabaseTest]
    [TestMethod]
    public void IBAssetExists_TataHitachiAsset_UpdateProperties_ModelDoesNotGetUpdated_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(IBOwner.BSSID).IbKey(IdGen.StringId()).Save();
      var IBAsset = Entity.Asset.WithDevice(IBDevice).MakeCode("THC").ModelName("OLD_MODEL_NAME").ManufactureYear(2000).Name("OLD_NAME").IsEngineStartStopSupported(true).Save();

      // new model name
      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode)
        .EquipmentLabel("NEW_NAME").Model("NEW_MODEL_NAME").Build(); // IBAsset
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      var asset = Ctx.OpContext.AssetReadOnly.FirstOrDefault(x => x.AssetID == IBAsset.AssetID);
      Assert.AreEqual(message.EquipmentLabel, asset.Name);
      Assert.AreEqual("OLD_MODEL_NAME", asset.Model);
      TestHelper.AssertEngineOnOffReset(asset.AssetID, true);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
    }

    [DatabaseTest]
    [TestMethod]
    public void IBAssetExists_NullEquipmentVIN_UpdateProperties_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(IBOwner.BSSID).IbKey(IdGen.StringId()).Save();
      // Create asset with NULL EquipmentVIN.
      var IBAsset = Entity.Asset.WithDevice(IBDevice).ModelName("OLD_MODEL_NAME").ManufactureYear(2000).Name("OLD_NAME").EquipmentVIN(null).IsEngineStartStopSupported(true).Save();

      // Update asset with empty ("") EquipmentVIN.
      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).EquipmentVIN(string.Empty).MakeCode(IBAsset.fk_MakeCode).Build(); // IBAsset
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      var asset = Ctx.OpContext.AssetReadOnly.FirstOrDefault(x => x.AssetID == IBAsset.AssetID);
      Assert.AreEqual(message.EquipmentLabel, asset.Name);
      Assert.AreEqual(message.ModelYear, asset.ManufactureYear.ToString());
      // Even though we tried to update the EquipmentVIN from NULL to "", it should remain NULL.
      Assert.IsNull(asset.EquipmentVIN, "EquipmentVIN should be NULL");
      TestHelper.AssertEngineOnOffReset(asset.AssetID, true);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
    }

    [DatabaseTest]
    [TestMethod]
    public void IBAssetDoesNotExist_CreateIBAssetAndInstallIBDevice_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(IBOwner.BSSID).IbKey(IdGen.StringId()).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      var asset = TestHelper.AssertAsset(message);
      var device = TestHelper.AssertDevice(message);
      Assert.AreEqual(device.ID, asset.fk_DeviceID, "Device not installed on asset.");
      TestHelper.AssertEngineOnOffReset(asset.AssetID, false);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
    }
    
    [DatabaseTest]
    [TestMethod]
    public void IBOwnerDoesNotExist_Failure()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var message = BSS.IBUpdated.PartNumber(partNumber).Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      TestHelper.AssertBssFailureCode(result, BssFailureCode.OwnerBssIdDoesNotExist);
      var failureMessage = string.Format(BssConstants.InstallBase.OWNER_BSSID_DOES_NOT_EXIST, message.OwnerBSSID);
      StringAssert.Contains(result.Summary, failureMessage, "Error Message");
    }

    [DatabaseTest]
    [TestMethod]
    public void IBDeviceDoesNotExist_Failure()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var message = BSS.IBUpdated.OwnerBssId(dealer.BSSID).PartNumber(partNumber).Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      TestHelper.AssertBssFailureCode(result, BssFailureCode.IbKeyDoesNotExist);
      var failureMessage = string.Format(BssConstants.IBKEY_DOES_NOT_EXISTS, message.IBKey);
      StringAssert.Contains(result.Summary, failureMessage, "Error Message");
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceTypeDoesNotExistForPartNumber_Failure()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.IBUpdated.OwnerBssId(dealer.BSSID).PartNumber("INVALID_PARTNUMBER").Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      TestHelper.AssertBssFailureCode(result, BssFailureCode.PartNumberDoesNotExist);
      string failureMessage = string.Format(BssConstants.InstallBase.PART_NUMBER_DOES_NOT_EXIST, message.PartNumber);
      StringAssert.Contains(result.Summary, failureMessage, "Error Message");
    }

    [DatabaseTest]
    [TestMethod]
    public void GpsDeviceIdsDoNotMatch_Failure()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series521);
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS521.OwnerBssId(dealer.BSSID).IbKey(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).IsEngineStartStopSupported(true).Save();
      
      var message = BSS.IBUpdated
        .OwnerBssId(dealer.BSSID)
        .IBKey(device.IBKey)
        .GpsDeviceId(IdGen.StringId())
        .PartNumber(partNumber)
        .EquipmentSN(asset.SerialNumberVIN)
        .MakeCode(asset.fk_MakeCode)
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      TestHelper.AssertBssFailureCode(result, BssFailureCode.GpsDeviceIdInvalid);
      string failureMessage = string.Format(BssConstants.InstallBase.GPS_DEVICEIDS_DO_NOT_MATCH, device.GpsDeviceID,message.GPSDeviceID);
      StringAssert.Contains(result.Summary, failureMessage, "Error Message");
      TestHelper.AssertEngineOnOffReset(asset.AssetID, true);
    }

    #region Device Transfer

    [DatabaseTest]
    [TestMethod]
    public void DeviceTransfer_IBDeviceInactiveAndNotInstalled_IBAssetWithoutDevice_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series523);
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS523.OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Provisioned).IbKey(IdGen.StringId()).Save();
      var IBAsset = Entity.Asset.Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode) //IB Asset
        .ImplyDeviceTransfer()
        .Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      var asset = TestHelper.AssertAsset(message);
      var device = TestHelper.AssertDevice(message);
      Assert.AreEqual(device.ID, asset.fk_DeviceID, "Device not installed on asset.");

      TestHelper.AssertAssetDeviceHistoryWasNotCreated();
      TestHelper.AssertEngineOnOffReset(asset.AssetID, false);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceTransfer_IBDeviceInactiveAndNotInstalled_IBAssetExistsWithInactiveManualDevice_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series523);
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS523.OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Provisioned).IbKey(IdGen.StringId()).Save();
      var IBAsset = Entity.Asset.WithDevice(Entity.Device.NoDevice.DeviceState(DeviceStateEnum.Provisioned).Save()).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode) //IB Asset
        .ImplyDeviceTransfer()
        .Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      var asset = TestHelper.AssertAsset(message);
      var device = TestHelper.AssertDevice(message);
      Assert.AreEqual(device.ID, asset.fk_DeviceID, "Device not installed on asset.");

      TestHelper.AssertAssetDeviceHistory(
        IBAsset.AssetID, 
        findByDeviceId: IBAsset.Device.ID,
        oldAssetId: IBAsset.AssetID, 
        oldDeviceId: IBAsset.Device.ID, 
        oldOwnerBssId: IBAsset.Device.OwnerBSSID);
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, false);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceTransfer_IBDeviceInactiveAndInstalledOnAsset_IBAssetExistsWithInactiveManualDevice_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series523);
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS523.OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Provisioned).IbKey(IdGen.StringId()).Save();
      var IBDeviceInstalledOnAsset = Entity.Asset.WithDevice(IBDevice).IsEngineStartStopSupported(true).Save();
      var IBAsset = Entity.Asset.WithDevice(Entity.Device.NoDevice.DeviceState(DeviceStateEnum.Provisioned).Save()).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode) //IB Asset
        .ImplyDeviceTransfer()
        .Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      var asset = TestHelper.AssertAsset(message);
      var device = TestHelper.AssertDevice(message);
      Assert.AreEqual(device.ID, asset.fk_DeviceID, "Device not installed on asset.");

      TestHelper.AssertAssetDeviceHistory(
        IBAsset.AssetID, 
        findByDeviceId: IBAsset.Device.ID,
        oldAssetId: IBAsset.AssetID, 
        oldDeviceId: IBAsset.Device.ID, 
        oldOwnerBssId: IBAsset.Device.OwnerBSSID);

      TestHelper.AssertAssetDeviceHistory(
        IBDeviceInstalledOnAsset.AssetID, 
        findByDeviceId: IBDevice.ID,
        oldAssetId: IBDeviceInstalledOnAsset.AssetID, 
        oldDeviceId: IBDevice.ID, 
        oldOwnerBssId: IBDevice.OwnerBSSID);
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(IBDeviceInstalledOnAsset.AssetID, false);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceTransfer_IBDeviceInactiveAndInstalledOnAsset_IBAssetExistsWithInactiveMts522_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series523);
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS523.OwnerBssId(IBOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBDeviceInstalledOnAsset = Entity.Asset.WithDevice(IBDevice).IsEngineStartStopSupported(true).Save();
      var IBAsset = Entity.Asset.WithDevice(Entity.Device.MTS522.DeviceState(DeviceStateEnum.Provisioned).Save()).IsEngineStartStopSupported(true).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode) //IB Asset
        .ImplyDeviceTransfer()
        .Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      var asset = TestHelper.AssertAsset(message);
      var device = TestHelper.AssertDevice(message);
      Assert.AreEqual(device.ID, asset.fk_DeviceID, "Device not installed on asset.");

      TestHelper.AssertAssetDeviceHistory(
        IBAsset.AssetID, 
        findByDeviceId: IBAsset.Device.ID,
        oldAssetId: IBAsset.AssetID, 
        oldDeviceId: IBAsset.Device.ID, 
        oldOwnerBssId: IBAsset.Device.OwnerBSSID);

      TestHelper.AssertAssetDeviceHistory(
        IBDeviceInstalledOnAsset.AssetID, 
        findByDeviceId: IBDevice.ID,
        oldAssetId: IBDeviceInstalledOnAsset.AssetID, 
        oldDeviceId: IBDevice.ID, 
        oldOwnerBssId: IBDevice.OwnerBSSID);
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(IBDeviceInstalledOnAsset.AssetID, false);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceTransfer_IBDeviceActive_Failure()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series523);
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS523.OwnerBssId(IBOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).Save();
      var IBDeviceInstalledOnAsset = Entity.Asset.WithDevice(IBDevice).IsEngineStartStopSupported(true).Save();
      var IBAsset = Entity.Asset.WithDevice(Entity.Device.MTS522.DeviceState(DeviceStateEnum.Provisioned).Save()).IsEngineStartStopSupported(true).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode) //IB Asset
        .ImplyDeviceTransfer()
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      TestHelper.AssertBssFailureCode(result, BssFailureCode.ActiveServiceExistsForDevice);
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, true);
      TestHelper.AssertEngineOnOffReset(IBDeviceInstalledOnAsset.AssetID, true);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceTransfer_IBAssetWithActiveDeviceInstalled_Failure()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series523);
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS523.OwnerBssId(IBOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBDeviceInstalledOnAsset = Entity.Asset.WithDevice(IBDevice).IsEngineStartStopSupported(true).Save();
      var IBAsset = Entity.Asset.WithDevice(Entity.Device.MTS522.DeviceState(DeviceStateEnum.Subscribed).Save()).IsEngineStartStopSupported(true).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode) //IB Asset
        .ImplyDeviceTransfer()
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      TestHelper.AssertBssFailureCode(result, BssFailureCode.DeviceTransferNotValid);
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, true);
      TestHelper.AssertEngineOnOffReset(IBDeviceInstalledOnAsset.AssetID, true);
    } 

    #endregion

    #region Device Replacement

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_InactiveDeviceNotInstalled_IBAssetWithActiveDeviceInstalled_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL121);

      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBAssetDevice522 = Entity.Device.MTS522.OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
      var IBAsset = Entity.Asset.WithDevice(IBAssetDevice522).IsEngineStartStopSupported(true).Save();
      var core = Entity.Service.Essentials.ForDevice(IBAssetDevice522)
        .WithView(x => x.ForCustomer(IBOwner).ForAsset(IBAsset)).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode) //IBAsset
        .ImplyDeviceReplacement()
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success should be false");
      var device = TestHelper.AssertDevice(message);
      var asset = TestHelper.AssertAsset(message);
      Assert.AreEqual(device.ID, asset.fk_DeviceID);

      TestHelper.AssertAssetDeviceHistory(
        IBAsset.AssetID,
        findByDeviceId: IBAsset.Device.ID,
        oldAssetId: IBAsset.AssetID,
        oldDeviceId: IBAsset.Device.ID,
        oldOwnerBssId: IBAsset.Device.OwnerBSSID);

      var viewHelper = new ServiceViewAPITestHelper();
      var actionUtcAsDate = DateTime.Parse(message.ActionUTC);

      var views = viewHelper.GetServiceViewsForCustomer(IBOwner.ID);
      Assert.AreEqual(1, views.Count, "Count");
      Assert.AreEqual(actionUtcAsDate.KeyDate(), views.First().EndKeyDate, "End Date");
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_InactiveDeviceInstalledOnAsset_IBAssetWithActiveDeviceInstalled_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL121);

      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Provisioned).Save();
      var AssetForIBDevice = Entity.Asset.WithDevice(IBDevice).IsEngineStartStopSupported(true).Save();
      var IBAssetDevice522 = Entity.Device.MTS522.OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
      var IBAsset = Entity.Asset.WithDevice(IBAssetDevice522).IsEngineStartStopSupported(true).Save();
      Entity.Service.Essentials.ForDevice(IBAssetDevice522)
        .WithView(x => x.ForCustomer(IBOwner).ForAsset(IBAsset)).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode) //IBAsset
        .ImplyDeviceReplacement()
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success should be false");
      var device = TestHelper.AssertDevice(message);
      var asset = TestHelper.AssertAsset(message);
      Assert.AreEqual(device.ID, asset.fk_DeviceID);

      TestHelper.AssertAssetDeviceHistory(
        IBAsset.AssetID,
        findByDeviceId: IBAsset.Device.ID,
        oldAssetId: IBAsset.AssetID,
        oldDeviceId: IBAsset.Device.ID,
        oldOwnerBssId: IBAsset.Device.OwnerBSSID);

      TestHelper.AssertAssetDeviceHistory(
        AssetForIBDevice.AssetID,
        findByDeviceId: IBDevice.ID,
        oldAssetId: AssetForIBDevice.AssetID,
        oldDeviceId: IBDevice.ID,
        oldOwnerBssId: IBDevice.OwnerBSSID);

      var viewHelper = new ServiceViewAPITestHelper();
      var actionUtcAsDate = DateTime.Parse(message.ActionUTC);

      var views = viewHelper.GetServiceViewsForCustomer(IBOwner.ID);
      Assert.AreEqual(1, views.Count, "Count");
      Assert.AreEqual(actionUtcAsDate.KeyDate(), views.First().EndKeyDate, "End Date");

      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(AssetForIBDevice.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_InactivePL121IBDeviceInstalledOnAsset_IBAssetWithActiveMts522Insalled_SupportedAddOn_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL121);

      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Provisioned).Save();
      var AssetForIBDevice = Entity.Asset.WithDevice(IBDevice).IsEngineStartStopSupported(true).Save();
      var IBAssetDevice522 = Entity.Device.MTS522.OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
      var IBAsset = Entity.Asset.WithDevice(IBAssetDevice522).IsEngineStartStopSupported(true).Save();
      Entity.Service.Essentials.ForDevice(IBAssetDevice522)
        .WithView(x => x.ForCustomer(IBOwner).ForAsset(IBAsset)).Save();
      Entity.Service.Maintenance.ForDevice(IBAssetDevice522)
        .WithView(x => x.ForCustomer(IBOwner).ForAsset(IBAsset)).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode) //IBAsset
        .ImplyDeviceReplacement()
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success should be false");
      var device = TestHelper.AssertDevice(message);
      var asset = TestHelper.AssertAsset(message);
      Assert.AreEqual(device.ID, asset.fk_DeviceID);
      
      TestHelper.AssertAssetDeviceHistory(
        IBAsset.AssetID,
        findByDeviceId: IBAsset.Device.ID,
        oldAssetId: IBAsset.AssetID,
        oldDeviceId: IBAsset.Device.ID,
        oldOwnerBssId: IBAsset.Device.OwnerBSSID);

      TestHelper.AssertAssetDeviceHistory(
        AssetForIBDevice.AssetID,
        findByDeviceId: IBDevice.ID,
        oldAssetId: AssetForIBDevice.AssetID,
        oldDeviceId: IBDevice.ID,
        oldOwnerBssId: IBDevice.OwnerBSSID);

      var viewHelper = new ServiceViewAPITestHelper();
      var actionUtcAsDate = DateTime.Parse(message.ActionUTC);

      var views = viewHelper.GetServiceViewsForCustomer(IBOwner.ID);
      Assert.AreEqual(2, views.Count, "Count");
      Assert.IsTrue(views.All(x => x.EndKeyDate == actionUtcAsDate.KeyDate()), "End Date");

      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(AssetForIBDevice.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_InactivePL121IBDeviceInstalledOnAsset_IBAssetWithActiveMts522Insalled_UnsupportedAddOn_Failure()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL121);
      
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBAssetDevice522 = Entity.Device.MTS522.OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
      var IBAsset = Entity.Asset.WithDevice(IBAssetDevice522).IsEngineStartStopSupported(true).Save();
      Entity.Service.Essentials.ForDevice(IBAssetDevice522).Save();
      Entity.Service.OneMinuteRate.ForDevice(IBAssetDevice522).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode) //IBAsset
        .ImplyDeviceReplacement()
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Success should be false");
      TestHelper.AssertBssFailureCode(result, BssFailureCode.DeviceReplaceNotValid);

      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, true);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_InactiveIBDeviceInstalledOnAsset_IBAssetWithoutDevice_Failure()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL121);

      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBAsset = Entity.Asset.Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode) //IBAsset
        .ImplyDeviceReplacement()
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Success should be false");
      TestHelper.AssertBssFailureCode(result, BssFailureCode.DeviceReplaceNotValid);
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_InactiveIBDeviceInstalledOnAsset_IBAssetWithInactiveDevice_Failure()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL121);

      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBAssetDevice522 = Entity.Device.MTS522.OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBAsset = Entity.Asset.WithDevice(IBAssetDevice522).IsEngineStartStopSupported(true).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode) //IBAsset
        .ImplyDeviceReplacement()
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Success should be false");
      TestHelper.AssertBssFailureCode(result, BssFailureCode.DeviceReplaceNotValid);
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, true);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_ActiveIBDevice_Failure()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL121);

      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.PL121.IbKey(IdGen.StringId()).OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
      var IBAssetDevice522 = Entity.Device.MTS522.OwnerBssId(IBOwner.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
      var IBAsset = Entity.Asset.WithDevice(IBAssetDevice522).IsEngineStartStopSupported(true).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode) //IBAsset
        .ImplyDeviceReplacement()
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Success should be false");
      TestHelper.AssertBssFailureCode(result, BssFailureCode.ActiveServiceExistsForDevice);
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, true);
    }

    #endregion

    #region Ownership Transfer

    [DatabaseTest]
    [TestMethod]
    public void OwnershipTransfer_InactiveDevice_DealerToDealer_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series523);
      var registeredDealerAndOwner = Entity.Customer.Dealer.Save();

      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();

      var IBDevice = Entity.Device.MTS523.OwnerBssId(registeredDealerAndOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBDeviceAsset = Entity.Asset.WithDevice(IBDevice).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBDeviceAsset.SerialNumberVIN).MakeCode(IBDeviceAsset.fk_MakeCode) //IBDeviceAsset
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success);
      var asset = TestHelper.AssertAsset(message);
      var device = TestHelper.AssertDevice(message);

      TestHelper.AssertAssetDeviceHistory(
        IBDeviceAsset.AssetID,
        findByDeviceId: IBDevice.ID,
        oldAssetId: IBDeviceAsset.AssetID,
        oldDeviceId: IBDevice.ID,
        oldOwnerBssId: registeredDealerAndOwner.BSSID);
    }

    [DatabaseTest]
    [TestMethod]
    public void OwnershipTransfer_ActiveDevice_DealerToCustomersAccount_SameRegisteredDealer_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series523);
      var registeredDealerAndOwner = Entity.Customer.Dealer.Save();
      var customer = Entity.Customer.EndCustomer.Save();
      
      var IBOwner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(registeredDealerAndOwner, IBOwner).Save();
      Entity.CustomerRelationship.Relate(customer, IBOwner).Save();

      var IBDevice = Entity.Device.MTS523.OwnerBssId(registeredDealerAndOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBDeviceAsset = Entity.Asset.WithDevice(IBDevice).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBDeviceAsset.SerialNumberVIN).MakeCode(IBDeviceAsset.fk_MakeCode) //IBDeviceAsset
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success);
      var asset = TestHelper.AssertAsset(message);
      var device = TestHelper.AssertDevice(message);

      TestHelper.AssertAssetDeviceHistory(
        IBDeviceAsset.AssetID,
        findByDeviceId: IBDevice.ID,
        oldAssetId: IBDeviceAsset.AssetID,
        oldDeviceId: IBDevice.ID,
        oldOwnerBssId: registeredDealerAndOwner.BSSID);
    }
    
    [DatabaseTest]
    [TestMethod]
    public void OwnershipTransfer_ActiveDevice_DifferentAccountOfSameCustomer_SameRegisteredDealer_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series523);
      var registeredDealer = Entity.Customer.Dealer.Save();
      var customer = Entity.Customer.EndCustomer.Save();

      var currentOwner = Entity.Customer.Account.Save();
      Entity.CustomerRelationship.Relate(registeredDealer, currentOwner).Save();
      Entity.CustomerRelationship.Relate(customer, currentOwner).Save();
      
      var IBOwner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(registeredDealer, IBOwner).Save();
      Entity.CustomerRelationship.Relate(customer, IBOwner).Save();

      var IBDevice = Entity.Device.MTS523.OwnerBssId(currentOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBDeviceAsset = Entity.Asset.WithDevice(IBDevice).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBDeviceAsset.SerialNumberVIN).MakeCode(IBDeviceAsset.fk_MakeCode) //IBDeviceAsset
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success);
      var asset = TestHelper.AssertAsset(message);
      var device = TestHelper.AssertDevice(message);

      TestHelper.AssertAssetDeviceHistory(
        IBDeviceAsset.AssetID, 
        findByDeviceId: IBDevice.ID,
        oldAssetId: IBDeviceAsset.AssetID, 
        oldDeviceId: IBDevice.ID, 
        oldOwnerBssId: currentOwner.BSSID);
    }

    [DatabaseTest]
    [TestMethod]
    public void OwnershipTransfer_ActiveDevice_ServiceViewsTerminatedForOldOwnerAndCreatedForNewOwner()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series523);
      var registeredDealerAndOwner = Entity.Customer.Dealer.Save();
      var customer = Entity.Customer.EndCustomer.Save();

      var IBOwner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(registeredDealerAndOwner, IBOwner).Save();
      Entity.CustomerRelationship.Relate(customer, IBOwner).Save();

      var IBDevice = Entity.Device.MTS523.OwnerBssId(registeredDealerAndOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBDeviceAsset = Entity.Asset.WithDevice(IBDevice).Save();

      var core = Entity.Service.Essentials.ForDevice(IBDevice)
        .WithView(x => x.ForCustomer(registeredDealerAndOwner).ForAsset(IBDeviceAsset)).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBDeviceAsset.SerialNumberVIN).MakeCode(IBDeviceAsset.fk_MakeCode) //IBDeviceAsset
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success);

      var viewHelper = new ServiceViewAPITestHelper();
      var actionUtcAsDate = DateTime.Parse(message.ActionUTC);

      var oldOwnerViews = viewHelper.GetServiceViewsForCustomer(registeredDealerAndOwner.ID);
      viewHelper.AssertServiceViewIsTerminated(oldOwnerViews.Where(x => x.EndKeyDate != 99991231).ToList(), core.ID, actionUtcAsDate.KeyDate(), "View not terminated correctly");
      viewHelper.AssertServiceViewIsCreated(oldOwnerViews.Where(x => x.EndKeyDate == 99991231).ToList(), core, actionUtcAsDate.KeyDate(), "Dealer view not created correctly");

      var newOwnerCustomerViews = viewHelper.GetServiceViewsForCustomer(customer.ID);
      viewHelper.AssertServiceViewIsCreated(newOwnerCustomerViews, core, actionUtcAsDate.KeyDate(), "Customer view not created correctly");

      var corporateServiceViews = viewHelper.GetServiceViewsForCustomer(viewHelper.GetCorporateCustomer((DealerNetworkEnum)registeredDealerAndOwner.fk_DealerNetworkID).ID);
      viewHelper.AssertServiceViewIsCreated(newOwnerCustomerViews, core, actionUtcAsDate.KeyDate(), "Corporate Service views not created correctly");

    }

    [DatabaseTest]
    [TestMethod]
    public void OwnershipTransfer_ActiveDevice_DifferentRegisteredDealer_Failure()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series523);
      var registeredDealer = Entity.Customer.Dealer.Save(); 
      var newRegisterDealer = Entity.Customer.Dealer.Save();
      var customer = Entity.Customer.EndCustomer.Save();

      var currentOwner = Entity.Customer.Account.Save();
      Entity.CustomerRelationship.Relate(registeredDealer, currentOwner).Save();
      Entity.CustomerRelationship.Relate(customer, currentOwner).Save();
      
      // Relate to other dealer
      var IBOwner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      Entity.CustomerRelationship.Relate(newRegisterDealer, IBOwner).Save();
      Entity.CustomerRelationship.Relate(customer, IBOwner).Save();

      var IBDevice = Entity.Device.MTS523.OwnerBssId(currentOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).Save();
      var IBDeviceAsset = Entity.Asset.WithDevice(IBDevice).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID) // IBOwner
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
        .EquipmentSN(IBDeviceAsset.SerialNumberVIN).MakeCode(IBDeviceAsset.fk_MakeCode) //IBDeviceAsset
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      TestHelper.AssertBssFailureCode(result, BssFailureCode.ActiveDeviceRegisteredDlrXfer);
    }

    [DatabaseTest]
    [TestMethod]
    public void OwnershipTransfer_AccountoAccount_ActiveDevice_ServiceViewsTerminatedForOldOwnerAndCreatedForNewOwner()
    {
        var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series523);
        var viewHelper = new ServiceViewAPITestHelper();

        var registeredDealerAndOwner = Entity.Customer.Dealer.DealerNetwork(DealerNetworkEnum.TRMB).Save();   // Set Up Dealer
        var customer1 = Entity.Customer.EndCustomer.Save();                                                   // Set up Customer1

        var account1 = Entity.Customer.Account.BssId(IdGen.StringId()).Save();                                // Set up Account1
        Entity.CustomerRelationship.Relate(registeredDealerAndOwner, account1).Save();
        Entity.CustomerRelationship.Relate(customer1, account1).Save();

        var customer2 = Entity.Customer.EndCustomer.Save();                                                   // Set up Customer2     
        var account2 = Entity.Customer.Account.BssId(IdGen.StringId()).Save();                                // Set up Account2
        Entity.CustomerRelationship.Relate(registeredDealerAndOwner, account2).Save();
        Entity.CustomerRelationship.Relate(customer2, account2).Save();

        var corpUser = viewHelper.GetCorporateCustomer((DealerNetworkEnum)registeredDealerAndOwner.fk_DealerNetworkID);  

        var IBDevice = Entity.Device.MTS523.OwnerBssId(account1.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save(); // Set Account1 to be IBOwner
        var IBDeviceAsset = Entity.Asset.WithDevice(IBDevice).Save();

        var core = Entity.Service.Essentials.ForDevice(IBDevice)                                               // Set up Service Views for Dealer,Customer1 and Corp User..
          .WithView(x => x.ForCustomer(registeredDealerAndOwner).ForAsset(IBDeviceAsset))
          .WithView(x => x.ForCustomer(customer1).ForAsset(IBDeviceAsset))
          .WithView(x => x.ForCustomer(corpUser).ForAsset(IBDeviceAsset))
          .Save();

        var message = BSS.IBUpdated                                                                            // Change Ownership from Account1 to Account2    
          .OwnerBssId(account2.BSSID) // IBOwner
          .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber) // IBDevice
          .EquipmentSN(IBDeviceAsset.SerialNumberVIN).MakeCode(IBDeviceAsset.fk_MakeCode) //IBDeviceAsset
          .Build();

        var result = TestHelper.ExecuteWorkflow(message);

        Assert.IsTrue(result.Success);

        
        var actionUtcAsDate = DateTime.Parse(message.ActionUTC);

        var dealerServiceViews = viewHelper.GetServiceViewsForCustomer(registeredDealerAndOwner.ID);                                     // Verify Dealer still has service views
        viewHelper.AssertServiceViewIsTerminated(dealerServiceViews.Where(x => x.EndKeyDate != 99991231).ToList(), core.ID, actionUtcAsDate.KeyDate(), "Dealer View not terminated correctly");
        viewHelper.AssertServiceViewIsCreated(dealerServiceViews.Where(x => x.EndKeyDate == 99991231).ToList(), core, actionUtcAsDate.KeyDate(), "Dealer view not created correctly");

        var customer1ServiceViews = viewHelper.GetServiceViewsForCustomer(customer1.ID);                                                // Verify Customer1's service views are terminated
        viewHelper.AssertServiceViewIsTerminated(customer1ServiceViews.Where(x => x.EndKeyDate != 99991231).ToList(), core.ID, actionUtcAsDate.KeyDate(), "Customer1 View not terminated correctly");

        var customer2ServiceViews = viewHelper.GetServiceViewsForCustomer(customer2.ID);                                                // Verify Customer2's service views are created
        viewHelper.AssertServiceViewIsCreated(customer2ServiceViews, core, actionUtcAsDate.KeyDate(), "Customer2 view not created correctly");

        var corporateServiceViews = viewHelper.GetServiceViewsForCustomer(corpUser.ID);                                                 // Verify Corporate still has service views
        viewHelper.AssertServiceViewIsTerminated(corporateServiceViews.Where(x => x.EndKeyDate != 99991231).ToList(), core.ID, actionUtcAsDate.KeyDate(), "Corporate View not terminated correctly");
        viewHelper.AssertServiceViewIsCreated(corporateServiceViews.Where(x => x.EndKeyDate == 99991231).ToList(), core, actionUtcAsDate.KeyDate(), "Corporate View not created correctly");

    }
    #endregion

    #region Device Transfer and Ownership Transfer

    [DatabaseTest]
    [TestMethod]
    public void DeviceTransferAndOwnershipTransfer_InactiveIBDeviceNotInstalled_IBAssetDoesNotExist_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var oldOwner = Entity.Customer.Dealer.Save();
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID)
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber)
        .EquipmentSN("NEW_ASSET_SN").MakeCode("CAT")
        .ImplyDeviceTransfer()
        .Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success, "Success");
      var asset = TestHelper.AssertAsset(message);
      var device = TestHelper.AssertDevice(message);
      Assert.AreEqual(IBDevice.ID, asset.fk_DeviceID, "Installed Device");
      Assert.AreEqual(IBOwner.BSSID, device.OwnerBSSID, "Owner");

      TestHelper.AssertAssetDeviceHistoryWasNotCreated();
      TestHelper.AssertEngineOnOffReset(asset.AssetID, false);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceTransferAndOwnershipTransfer_InactiveIBDeviceNotInstalled_IBAssetWithoutDeviceInstalled_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var oldOwner = Entity.Customer.Dealer.Save();
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBAsset = Entity.Asset.IsEngineStartStopSupported(true).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID)
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber)
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode)
        .ImplyDeviceTransfer()
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success");
      var asset = TestHelper.AssertAsset(message);
      var device = TestHelper.AssertDevice(message);
      Assert.AreEqual(IBDevice.ID, asset.fk_DeviceID, "Installed Device");
      Assert.AreEqual(IBOwner.BSSID, device.OwnerBSSID, "Owner");
      TestHelper.AssertAssetDeviceHistoryWasNotCreated();
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceTransferAndOwnershipTransfer_InactiveIBDeviceInstalledOnAsset_IBAssetWithoutDeviceInstalled_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var oldOwner = Entity.Customer.Dealer.Save();
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBDeviceAsset = Entity.Asset.WithDevice(IBDevice).IsEngineStartStopSupported(true).Save();
      var IBAsset = Entity.Asset.Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID)
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber)
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode)
        .ImplyDeviceTransfer()
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success");
      var asset = TestHelper.AssertAsset(message);
      var device = TestHelper.AssertDevice(message);
      
      var ibDeviceAsset = Ctx.OpContext.AssetReadOnly.Single(x => x.AssetID == IBDeviceAsset.AssetID);
      Assert.AreEqual(0, ibDeviceAsset.fk_DeviceID, "Device not removed from Asset");
      Assert.AreEqual(IBDevice.ID, asset.fk_DeviceID, "Device not installed on IBAsset");
      Assert.AreEqual(IBOwner.BSSID, device.OwnerBSSID, "Owner not changed");
      
      TestHelper.AssertAssetDeviceHistory(
         IBDeviceAsset.AssetID,
         findByDeviceId: IBDevice.ID,
         oldAssetId: IBDeviceAsset.AssetID,
         oldDeviceId: IBDevice.ID,
         oldOwnerBssId: oldOwner.BSSID);

      TestHelper.AssertEngineOnOffReset(IBDeviceAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, false);

    }

    [Ignore]
    [DatabaseTest]
    [TestMethod]
    public void DeviceTransferAndOwnershipTransfer_InactiveIBDeviceInstalledOnAsset_IBAssetWithInactiveDeviceInstalled_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var oldOwner = Entity.Customer.Dealer.Save();
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBDeviceAsset = Entity.Asset.WithDevice(IBDevice).Save();
      var IBAssetDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).Save();
      var IBAsset = Entity.Asset.WithDevice(IBAssetDevice).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID)
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber)
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode)
        .ImplyDeviceTransfer()
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success");
      var asset = TestHelper.AssertAsset(message);
      var device = TestHelper.AssertDevice(message);

      var ibDeviceAsset = Ctx.OpContext.AssetReadOnly.Single(x => x.AssetID == IBDeviceAsset.AssetID);
      Assert.AreEqual(0, ibDeviceAsset.fk_DeviceID, "Device not removed from Asset");
      Assert.AreEqual(IBDevice.ID, asset.fk_DeviceID, "Device not installed on IBAsset");
      Assert.AreEqual(IBOwner.BSSID, device.OwnerBSSID, "Owner not changed");

      TestHelper.AssertAssetDeviceHistory(
         IBDeviceAsset.AssetID,
         findByDeviceId: IBDevice.ID,
         oldAssetId: IBDeviceAsset.AssetID,
         oldDeviceId: IBDevice.ID,
         oldOwnerBssId: oldOwner.BSSID);

      TestHelper.AssertAssetDeviceHistory(
        IBAsset.AssetID,
        findByDeviceId: IBAssetDevice.ID,
        oldAssetId: IBAsset.AssetID,
        oldDeviceId: IBAssetDevice.ID,
        oldOwnerBssId: oldOwner.BSSID);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceTransferAndOwnershipTransfer_ActiveIBDeviceInstalledOnAsset_IBAssetWithInactiveDeviceInstalled_Failure()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var oldOwner = Entity.Customer.Dealer.Save();
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).Save();
      var IBDeviceAsset = Entity.Asset.WithDevice(IBDevice).IsEngineStartStopSupported(true).Save();
      var IBAssetDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).Save();
      var IBAsset = Entity.Asset.WithDevice(IBAssetDevice).IsEngineStartStopSupported(true).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID)
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber)
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode)
        .ImplyDeviceTransfer()
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Success");
      TestHelper.AssertBssFailureCode(result, BssFailureCode.ActiveServiceExistsForDevice);

      TestHelper.AssertEngineOnOffReset(IBDeviceAsset.AssetID, true);
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, true);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceTransferAndOwnershipTransfer_InactiveIBDeviceInstalledOnAsset_IBAssetWithActiveDeviceInstalled_Failure()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var oldOwner = Entity.Customer.Dealer.Save();
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBDeviceAsset = Entity.Asset.WithDevice(IBDevice).Save();
      var IBAssetDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
      var IBAsset = Entity.Asset.WithDevice(IBAssetDevice).IsEngineStartStopSupported(true).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID)
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber)
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode)
        .ImplyDeviceTransfer()
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Success");
      TestHelper.AssertBssFailureCode(result, BssFailureCode.DeviceTransferNotValid);

      TestHelper.AssertEngineOnOffReset(IBDeviceAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, true);
    }

    #endregion

    #region Ownership Transfer and Device Replacement

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacementAndOwnershipTransfer_InactiveDeviceNotInstalled_IBAssetWithActiveDeviceInstalled_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var oldOwner = Entity.Customer.Dealer.Save();
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBAssetDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).Save();
      var IBAsset = Entity.Asset.WithDevice(IBAssetDevice).IsEngineStartStopSupported(true).Save();
      var core = Entity.Service.Essentials.ForDevice(IBAssetDevice)
        .WithView(x => x.ForCustomer(oldOwner).ForAsset(IBAsset)).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID)
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber)
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode)
        .ImplyDeviceReplacement()
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      var device = TestHelper.AssertDevice(message);
      var asset = TestHelper.AssertAsset(message);
      Assert.AreEqual(IBOwner.BSSID, device.OwnerBSSID, "Owner");
      Assert.AreEqual(IBDevice.ID, asset.fk_DeviceID, "Installed Device");

      TestHelper.AssertAssetDeviceHistory(
         IBAsset.AssetID,
         findByDeviceId: IBAssetDevice.ID,
         oldAssetId: IBAsset.AssetID,
         oldDeviceId: IBAssetDevice.ID,
         oldOwnerBssId: oldOwner.BSSID);

      var viewHelper = new ServiceViewAPITestHelper();
      var actionUtcAsDate = DateTime.Parse(message.ActionUTC);
      var oldOwnerViews = viewHelper.GetServiceViewsForCustomer(oldOwner.ID);
      viewHelper.AssertServiceViewIsTerminated(oldOwnerViews, core.ID, actionUtcAsDate.KeyDate(), "views terminated");

      var newOwnerViews = viewHelper.GetServiceViewsForCustomer(IBOwner.ID);
      Assert.AreEqual(0, newOwnerViews.Count, "Views should be created when the DR message comes");
            
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, false);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacementAndOwnershipTransfer_InactiveDeviceInstalledOnAsset_IBAssetWithActiveDeviceInstalled_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var oldOwner = Entity.Customer.Dealer.Save();
      var IBOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var IBDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Provisioned).Save();
      var IBDeviceAsset = Entity.Asset.WithDevice(IBDevice).IsEngineStartStopSupported(true).Save();
      var IBAssetDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).Save();
      var IBAsset = Entity.Asset.WithDevice(IBAssetDevice).IsEngineStartStopSupported(true).Save();
      var core = Entity.Service.Essentials.ForDevice(IBAssetDevice)
        .WithView(x => x.ForCustomer(oldOwner).ForAsset(IBAsset)).Save();

      var message = BSS.IBUpdated
        .OwnerBssId(IBOwner.BSSID)
        .IBKey(IBDevice.IBKey).GpsDeviceId(IBDevice.GpsDeviceID).PartNumber(partNumber)
        .EquipmentSN(IBAsset.SerialNumberVIN).MakeCode(IBAsset.fk_MakeCode)
        .ImplyDeviceReplacement()
        .Build();

      var result = TestHelper.ExecuteWorkflow(message);
      Assert.IsTrue(result.Success);

      var device = TestHelper.AssertDevice(message);
      var asset = TestHelper.AssertAsset(message);
      Assert.AreEqual(IBOwner.BSSID, device.OwnerBSSID, "Owner");
      Assert.AreEqual(IBDevice.ID, asset.fk_DeviceID, "Installed Device");

      TestHelper.AssertAssetDeviceHistory(
         IBAsset.AssetID,
         findByDeviceId: IBAssetDevice.ID,
         oldAssetId: IBAsset.AssetID,
         oldDeviceId: IBAssetDevice.ID,
         oldOwnerBssId: oldOwner.BSSID);

      TestHelper.AssertAssetDeviceHistory(
         IBDeviceAsset.AssetID,
         findByDeviceId: IBDevice.ID,
         oldAssetId: IBDeviceAsset.AssetID,
         oldDeviceId: IBDevice.ID,
         oldOwnerBssId: oldOwner.BSSID);

      var viewHelper = new ServiceViewAPITestHelper();
      var actionUtcAsDate = DateTime.Parse(message.ActionUTC);
      var oldOwnerViews = viewHelper.GetServiceViewsForCustomer(oldOwner.ID);
      viewHelper.AssertServiceViewIsTerminated(oldOwnerViews, core.ID, actionUtcAsDate.KeyDate(), "views terminated");

      var newOwnerViews = viewHelper.GetServiceViewsForCustomer(IBOwner.ID);
      Assert.AreEqual(0, newOwnerViews.Count, "Views should be created when the DR message comes");

      TestHelper.AssertEngineOnOffReset(IBDeviceAsset.AssetID, false);
      TestHelper.AssertEngineOnOffReset(IBAsset.AssetID, false);
    }

    #endregion

  }
}
