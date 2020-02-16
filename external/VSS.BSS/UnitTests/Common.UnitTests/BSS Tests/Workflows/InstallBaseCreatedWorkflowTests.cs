using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class InstallBaseCreatedWorkflowTests : BssUnitTestBase
  {
    #region Failure Scenarios

    [DatabaseTest]
    [TestMethod]
    public void OwnerDoesNotExist_FailureWithBssError()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var message = BSS.IBCreated.PartNumber(partNumber).Build();
      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      TestHelper.AssertBssFailureCode(result, BssFailureCode.OwnerBssIdDoesNotExist);
      var failureMessage = string.Format(BssConstants.InstallBase.OWNER_BSSID_DOES_NOT_EXIST, message.OwnerBSSID);
      StringAssert.Contains(result.Summary, failureMessage, "Summary should contain Owner bssid doesn't exists message.");
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceTypeDoesNotExistForPartNumber_FailureWithBssError()
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID).Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      TestHelper.AssertBssFailureCode(result, BssFailureCode.PartNumberDoesNotExist);
      string failureMessage = string.Format(BssConstants.InstallBase.PART_NUMBER_DOES_NOT_EXIST, message.PartNumber);
      StringAssert.Contains(result.Summary, failureMessage, "Summary should contain part number doesn't exists message.");      
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceWithGpsDeviceIdExists_FailureWithBssError()
    {
      var device = Entity.Device.MTS521.GpsDeviceId(IdGen.GetId().ToString()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID).GpsDeviceId(device.GpsDeviceID).PartNumber("78354-00").Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      TestHelper.AssertBssFailureCode(result, BssFailureCode.GpsDeviceIdExists);
      string failureMessage = string.Format(BssConstants.InstallBase.GPS_DEVICEID_EXISTS, message.GPSDeviceID);
      StringAssert.Contains(result.Summary, failureMessage, "Summary should contain GPSDevice ID exists message.");
    }

    [DatabaseTest]
    [TestMethod]
    public void IBKeyExists_FailureWithBssError()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series521);
      var device = Entity.Device.MTS521.GpsDeviceId(IdGen.GetId().ToString()).IbKey(IdGen.GetId().ToString()).Save();
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID).PartNumber(partNumber).IBKey(device.IBKey).Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      TestHelper.AssertBssFailureCode(result, BssFailureCode.IbKeyExists);
      string failureMessage = string.Format(BssConstants.InstallBase.IBKEY_EXISTS, message.IBKey);
      StringAssert.Contains(result.Summary, failureMessage, "Summary should contain IBKEY exists message.");
    }

    [DatabaseTest]
    [TestMethod]
    public void GpsDeviceIdNotDefined_NonManualDeviceType_FailureWithBssError()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID).PartNumber(partNumber)
        .GpsDeviceId(string.Empty).Build();

      var result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success);
      TestHelper.AssertBssFailureCode(result, BssFailureCode.GpsDeviceIdNotDefined);
      string failureMessage = string.Format("");
      StringAssert.Contains(result.Summary, failureMessage, "Summary not equal.");
    }

    #endregion

    #region Success Scenarios

    [DatabaseTest]
    [TestMethod]
    public void CreateAssetDevice_SNM451Device_Success()
    {
      var snm450PartNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.SNM451);
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID).PartNumber(snm450PartNumber).MakeCode("CAT").Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      TestHelper.AssertDevice(message);
      TestHelper.AssertAsset(message);
      TestHelper.AssertNhRawMtsDevice(message.GPSDeviceID);
      TestHelper.AssertAssetDeviceHistoryWasNotCreated();
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
    }
    
    [DatabaseTest]
    [TestMethod]
    public void CreateAssetDevice_PL431RDevice_Success()
    {
      var snm450RPartNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL431);
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID).PartNumber(snm450RPartNumber).MakeCode("CAT").Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      TestHelper.AssertDevice(message);
      TestHelper.AssertAsset(message);
      TestHelper.AssertNhRawMtsDevice(message.GPSDeviceID);
      TestHelper.AssertAssetDeviceHistoryWasNotCreated();
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateAssetDevice_CrossCheckDevice_Success()
    {
      var crosscheckPartNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.CrossCheck);
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID).PartNumber(crosscheckPartNumber).MakeCode("CAT").Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      TestHelper.AssertDevice(message);
      TestHelper.AssertAsset(message);
      TestHelper.AssertNhRawMtsDevice(message.GPSDeviceID);
      TestHelper.AssertAssetDeviceHistoryWasNotCreated();
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateAssetDevice_PL321Device_Success()
    {
      var pl321PartNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.PL321);
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID).PartNumber(pl321PartNumber).MakeCode("CAT").Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);
      
      Assert.IsTrue(result.Success);
      TestHelper.AssertDevice(message);
      TestHelper.AssertAsset(message);
      TestHelper.AssertNhRawPlDevice(message.GPSDeviceID);
      TestHelper.AssertAssetDeviceHistoryWasNotCreated();
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateAssetDevice_CATMTS521Device_Success()
    {
      var mts521PartNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series521);
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID).PartNumber(mts521PartNumber).MakeCode("CAT").Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);
      
      Assert.IsTrue(result.Success);
      TestHelper.AssertDevice(message);
      TestHelper.AssertAsset(message);
      TestHelper.AssertNhRawMtsDevice(message.GPSDeviceID);
      TestHelper.AssertAssetDeviceHistoryWasNotCreated();
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateAssetDevice_NonCATMTS521Device_Success()
    {
      var mts521PartNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series521);
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID).PartNumber(mts521PartNumber).MakeCode("TTT").Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);
      
      Assert.IsTrue(result.Success);
      TestHelper.AssertDevice(message);
      TestHelper.AssertAsset(message);
      TestHelper.AssertNhRawMtsDevice(message.GPSDeviceID);
      TestHelper.AssertAssetDeviceHistoryWasNotCreated();
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateAssetDevice_TrimTracDevice_Success()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.TrimTrac);
      var validTTGpsDeviceId = "01030700000000";
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID).PartNumber(partNumber).MakeCode("CAT").GpsDeviceId(validTTGpsDeviceId).Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      TestHelper.AssertDevice(message);
      TestHelper.AssertAsset(message);
      TestHelper.AssertNhRawTrimTracDevice(message.GPSDeviceID);
      TestHelper.AssertAssetDeviceHistoryWasNotCreated();
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateAssetDevice_ManualDevice_Success()
    {
      var manualDevicePartNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.MANUALDEVICE);
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID)
        .PartNumber(manualDevicePartNumber).GpsDeviceId(string.Empty)
        .MakeCode("CAT").Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);
      
      Assert.IsTrue(result.Success);
      TestHelper.AssertDevice(message);
      TestHelper.AssertAsset(message);
      TestHelper.AssertAssetDeviceHistoryWasNotCreated();
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once());
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_AssetExistsWithManualDevice_Success()
    {
      var manualDevicePartNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.MANUALDEVICE);
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var testDevice = Entity.Device.NoDevice.OwnerBssId(dealer.BSSID).IbKey(IdGen.StringId()).DeviceState(DeviceStateEnum.Subscribed).Save();
      var testAsset = Entity.Asset.WithDevice(testDevice).Save();
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID)
        .EquipmentSN(testAsset.SerialNumberVIN).MakeCode(testAsset.fk_MakeCode)
        .PartNumber(manualDevicePartNumber).GpsDeviceId(string.Empty).Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      TestHelper.AssertDevice(message);
      TestHelper.AssertAsset(message);
      TestHelper.AssertAssetDeviceHistory(
        testAsset.AssetID,
        findByDeviceId: testAsset.fk_DeviceID, 
        oldAssetId: testAsset.AssetID, 
        oldDeviceId: testAsset.Device.ID, 
        oldOwnerBssId: testAsset.Device.OwnerBSSID);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_AssetWithDeviceExists_Success()
    {
      var mts522PartNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var dealer = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      var testDevice = Entity.Device.PL121.OwnerBssId(dealer.BSSID).IbKey(IdGen.GetId().ToString()).DeviceState(DeviceStateEnum.Subscribed).Save();
      var testAsset = Entity.Asset.WithDevice(testDevice).WithCoreService().Save();
      
      var message = BSS.IBCreated.OwnerBssId(dealer.BSSID)
        .EquipmentSN(testAsset.SerialNumberVIN)
        .MakeCode(testAsset.fk_MakeCode)
        .PartNumber(mts522PartNumber).Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      TestHelper.AssertDevice(message);
      TestHelper.AssertAsset(message);
      TestHelper.AssertNhRawMtsDevice(message.GPSDeviceID);

      TestHelper.AssertAssetDeviceHistory(
        testAsset.AssetID,
        findByDeviceId: testAsset.fk_DeviceID, 
        oldAssetId: testAsset.AssetID,
        oldDeviceId: testAsset.Device.ID,
        oldOwnerBssId: testAsset.Device.OwnerBSSID);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
    }

    [DatabaseTest]
    [TestMethod]
    public void AssetExistsWithNoInstalledDevice_Success()
    {
      var mts522PartNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var testAsset = Entity.Asset.Save();

      var newOwner = Entity.Customer.Dealer.BssId(IdGen.GetId().ToString()).Save();
      
      var message = BSS.IBCreated.OwnerBssId(newOwner.BSSID)
        .EquipmentSN(testAsset.SerialNumberVIN)
        .MakeCode(testAsset.fk_MakeCode)
        .PartNumber(mts522PartNumber).Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      TestHelper.AssertDevice(message);
      TestHelper.AssertAsset(message);
      TestHelper.AssertNhRawMtsDevice(message.GPSDeviceID);
      TestHelper.AssertAssetDeviceHistoryWasNotCreated();
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceReplacement_NewDeviceDifferentOwner_Success()
    {
      var mtw522PartNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);

      var oldOwner = Entity.Customer.Dealer.Save();
      var oldDevice = Entity.Device.MTS522.OwnerBssId(oldOwner.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
      var asset = Entity.Asset.WithDevice(oldDevice).WithCoreService().Save();

      var newOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();

      var message = BSS.IBCreated.OwnerBssId(newOwner.BSSID)
        .MakeCode(asset.fk_MakeCode)
        .EquipmentSN(asset.SerialNumberVIN)
        .PartNumber(mtw522PartNumber).Build();
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();

      var result = TestHelper.ExecuteWorkflow(message, mockAssetLookup.Object, mockCustomerLookup.Object);

      Assert.IsTrue(result.Success);
      TestHelper.AssertDevice(message);
      TestHelper.AssertAssetDeviceHistory(asset.AssetID, findByDeviceId: oldDevice.ID, oldOwnerBssId: oldOwner.BSSID);
      mockAssetLookup.Verify(o => o.Add(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never());
    }

    #endregion
  }
}
