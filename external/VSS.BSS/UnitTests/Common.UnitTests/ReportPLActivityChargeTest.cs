using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.UnitTest.Common;
using VSS.UnitTest.Common.EntityBuilder;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
  [TestClass]
  public class ReportPLActivityChargeTest : UnitTestBase
  {
    #region Unit Tests
    [DatabaseTest]
    [TestMethod]
    public void TestPL121NullDeregisteredDateWorks()
    {
      SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", null, DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-3), false);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow);
      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(1, deviceInfo.Count, "Expected one PL device to be returned");
      ConfirmCorrectDeviceInfo(deviceInfo[0], "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID");
      ConfirmDevicesArePLs(deviceInfo);
    }

    [DatabaseTest]
    [TestMethod]
    public void TestPL121DeregisteredDateAfterEndDateWorks()
    {
      SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", DateTime.UtcNow.AddDays(-1), DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-10), false);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow.AddDays(-5));
      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(1, deviceInfo.Count, "Expected one PL device to be returned");
      ConfirmCorrectDeviceInfo(deviceInfo[0], "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID");
      ConfirmDevicesArePLs(deviceInfo);
    }

    [DatabaseTest]
    [TestMethod]
    public void TestPL321Works()
    {
      SetUpTestData(Entity.Device.PL321, "pl321IBKey", "pl321AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", null, DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-3), false);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow);
      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(1, deviceInfo.Count, "Expected one PL device to be returned");
      ConfirmCorrectDeviceInfo(deviceInfo[0], "pl321IBKey", "pl321AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID");
      ConfirmDevicesArePLs(deviceInfo);
    }

    [DatabaseTest]
    [TestMethod]
    public void TestOnlyPLDevicesAreReturned()
    {
      SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", null, DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-3), false);
      SetUpTestData(Entity.Device.PL321, "pl321IBKey", "pl321AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", null, DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-3), false);
      SetUpTestData(Entity.Device.MTS521, "mts521IBKey", "mts521AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", null, DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-3), false);
      SetUpTestData(Entity.Device.MTS522, "mts522IBKey", "mts522AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", null, DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-3), false);
      SetUpTestData(Entity.Device.MTS523, "mts523IBKey", "mts523AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", null, DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-3), false);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow);
      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(2, deviceInfo.Count, "Expected the two PL devices to be returned");
      ConfirmDevicesArePLs(deviceInfo);
    }

    [DatabaseTest]
    [TestMethod]
    public void TestSubscribedStateWorks()
    {
      SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", null, DeviceStateEnum.Subscribed, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-3), false);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow);
      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(1, deviceInfo.Count, "Expected one PL device to be returned");
      ConfirmCorrectDeviceInfo(deviceInfo[0], "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID");
      ConfirmDevicesArePLs(deviceInfo);
    }

    [DatabaseTest]
    [TestMethod]
    public void TestDealerOwnedWorks()
    {
      SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "registeredDealerBSSID", "registeredDealerBSSID", null, DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-3), true);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow);
      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(1, deviceInfo.Count, "Expected one PL device to be returned");
      ConfirmCorrectDeviceInfo(deviceInfo[0], "pl121IBKey", "pl121AssetSN", "CAT", "registeredDealerBSSID", "registeredDealerBSSID");
      ConfirmDevicesArePLs(deviceInfo);
    }

    [DatabaseTest]
    [TestMethod]
    public void TestTwoOwners_MessageCameInWhenOwner1Owned_ReturnsOwner1()
    {
      Device pl121 = SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID1", "registeredDealerBSSID1", DateTime.UtcNow.AddDays(-16), DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-75), DateTime.UtcNow.AddDays(-18), false);

      Ctx.OpContext.AssetDeviceHistory.AddObject(new AssetDeviceHistory
      {
        fk_AssetID = pl121.Asset.First().AssetID,
        fk_DeviceID = pl121.ID,
        OwnerBSSID = pl121.OwnerBSSID,
        StartUTC = DateTime.UtcNow.AddDays(-31),
        EndUTC = DateTime.UtcNow.AddDays(-16)
      });
      Customer customerAccount = Entity.Customer.Account.BssId("ownerBSSID2").Save();
      Customer dealer = Entity.Customer.Dealer.BssId("registeredDealerBSSID2").Save();
      CustomerRelationship relationship = Entity.CustomerRelationship.Relate(dealer, customerAccount).Save();
      pl121.OwnerBSSID = "ownerBSSID2";
      pl121.DeregisteredUTC = DateTime.UtcNow;
      Ctx.OpContext.SaveChanges();
      Service service = Entity.Service.Essentials.ForDevice(pl121).ActivationDate(DateTime.UtcNow.AddDays(-75)).CancellationDate(DateTime.UtcNow.AddDays(-60)).Save();
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow.AddDays(-1));

      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(1, deviceInfo.Count, "Expected one PL device to be returned");
      ConfirmCorrectDeviceInfo(deviceInfo[0], "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID1", "registeredDealerBSSID1");
      ConfirmDevicesArePLs(deviceInfo);
    }

    [DatabaseTest]
    [TestMethod]
    public void TestTwoOwners_MessageCameInWhenOwner2Owned_ReturnsOwner2()
    {
      Device pl121 = SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID1", "registeredDealerBSSID1", DateTime.UtcNow.AddDays(-16), DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-75), null, false);

      Ctx.OpContext.AssetDeviceHistory.AddObject(new AssetDeviceHistory
      {
        fk_AssetID = pl121.Asset.First().AssetID,
        fk_DeviceID = pl121.ID,
        OwnerBSSID = pl121.OwnerBSSID,
        StartUTC = DateTime.UtcNow.AddDays(-31),
        EndUTC = DateTime.UtcNow.AddDays(-16)
      });
      Customer customerAccount = Entity.Customer.Account.BssId("ownerBSSID2").Save();
      Customer dealer = Entity.Customer.Dealer.BssId("registeredDealerBSSID2").Save();
      CustomerRelationship relationship = Entity.CustomerRelationship.Relate(dealer, customerAccount).Save();
      pl121.OwnerBSSID = "ownerBSSID2";
      pl121.DeregisteredUTC = DateTime.UtcNow;
      Ctx.OpContext.SaveChanges();
      Service service = Entity.Service.Essentials.ForDevice(pl121).ActivationDate(DateTime.UtcNow.AddDays(-75)).CancellationDate(DateTime.UtcNow.AddDays(-60)).Save();
      CreateOEMDataMessage(DateTime.UtcNow.AddDays(-14), "pl121AssetSN", "CAT", pl121.GpsDeviceID, pl121.fk_DeviceTypeID);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow.AddDays(-1));

      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(1, deviceInfo.Count, "Expected one PL device to be returned");
      ConfirmCorrectDeviceInfo(deviceInfo[0], "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID2", "registeredDealerBSSID2");
      ConfirmDevicesArePLs(deviceInfo);
    }

    [DatabaseTest]
    [TestMethod]
    public void TestTwoOwners_MessagesCameInWhenOwner1Owned_AndWhenOwner2Owned_ReturnsOwner2()
    {
      Device pl121 = SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID1", "registeredDealerBSSID1", DateTime.UtcNow.AddDays(-16), DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-75), DateTime.UtcNow.AddDays(-18), false);

      Ctx.OpContext.AssetDeviceHistory.AddObject(new AssetDeviceHistory
      {
        fk_AssetID = pl121.Asset.First().AssetID,
        fk_DeviceID = pl121.ID,
        OwnerBSSID = pl121.OwnerBSSID,
        StartUTC = DateTime.UtcNow.AddDays(-31),
        EndUTC = DateTime.UtcNow.AddDays(-16)
      });
      Customer customerAccount = Entity.Customer.Account.BssId("ownerBSSID2").Save();
      Customer dealer = Entity.Customer.Dealer.BssId("registeredDealerBSSID2").Save();
      CustomerRelationship relationship = Entity.CustomerRelationship.Relate(dealer, customerAccount).Save();
      pl121.OwnerBSSID = "ownerBSSID2";
      pl121.DeregisteredUTC = DateTime.UtcNow;
      Ctx.OpContext.SaveChanges();
      Service service = Entity.Service.Essentials.ForDevice(pl121).ActivationDate(DateTime.UtcNow.AddDays(-75)).CancellationDate(DateTime.UtcNow.AddDays(-60)).Save();
      CreateOEMDataMessage(DateTime.UtcNow.AddDays(-14), "pl121AssetSN", "CAT", pl121.GpsDeviceID, pl121.fk_DeviceTypeID);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow.AddDays(-1));

      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(1, deviceInfo.Count, "Expected one PL device to be returned");
      ConfirmCorrectDeviceInfo(deviceInfo[0], "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID2", "registeredDealerBSSID2");
      ConfirmDevicesArePLs(deviceInfo);
    }

    [DatabaseTest]
    [TestMethod]
    public void TestTwoOwners_MessageCameInWhenOwner1Owned_ReturnsOwner1_AndBothOwnersAreDealers()
    {
      Device pl121 = SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "registeredDealerBSSID1", "registeredDealerBSSID1", DateTime.UtcNow.AddDays(-16), DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-75), DateTime.UtcNow.AddDays(-18), true);

      Ctx.OpContext.AssetDeviceHistory.AddObject(new AssetDeviceHistory
      {
        fk_AssetID = pl121.Asset.First().AssetID,
        fk_DeviceID = pl121.ID,
        OwnerBSSID = pl121.OwnerBSSID,
        StartUTC = DateTime.UtcNow.AddDays(-31),
        EndUTC = DateTime.UtcNow.AddDays(-16)
      });
      Customer dealer = Entity.Customer.Dealer.BssId("registeredDealerBSSID2").Save();
      pl121.OwnerBSSID = "registeredDealerBSSID2";
      pl121.DeregisteredUTC = DateTime.UtcNow;
      Ctx.OpContext.SaveChanges();
      Service service = Entity.Service.Essentials.ForDevice(pl121).ActivationDate(DateTime.UtcNow.AddDays(-75)).CancellationDate(DateTime.UtcNow.AddDays(-60)).Save();
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow.AddDays(-1));

      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(1, deviceInfo.Count, "Expected one PL device to be returned");
      ConfirmCorrectDeviceInfo(deviceInfo[0], "pl121IBKey", "pl121AssetSN", "CAT", "registeredDealerBSSID1", "registeredDealerBSSID1");
      ConfirmDevicesArePLs(deviceInfo);
    }

    [DatabaseTest]
    [TestMethod]
    public void TestTwoOwners_MessageCameInWhenOwner2Owned_ReturnsOwner2_AndBothOwnersAreDealers()
    {
      Device pl121 = SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "registeredDealerBSSID1", "registeredDealerBSSID1", DateTime.UtcNow.AddDays(-16), DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-75), null, true);

      Ctx.OpContext.AssetDeviceHistory.AddObject(new AssetDeviceHistory
      {
        fk_AssetID = pl121.Asset.First().AssetID,
        fk_DeviceID = pl121.ID,
        OwnerBSSID = pl121.OwnerBSSID,
        StartUTC = DateTime.UtcNow.AddDays(-31),
        EndUTC = DateTime.UtcNow.AddDays(-16)
      });
      Customer dealer = Entity.Customer.Dealer.BssId("registeredDealerBSSID2").Save();
      pl121.OwnerBSSID = "registeredDealerBSSID2";
      pl121.DeregisteredUTC = DateTime.UtcNow;
      Ctx.OpContext.SaveChanges();
      Service service = Entity.Service.Essentials.ForDevice(pl121).ActivationDate(DateTime.UtcNow.AddDays(-75)).CancellationDate(DateTime.UtcNow.AddDays(-60)).Save();
      CreateOEMDataMessage(DateTime.UtcNow.AddDays(-14), "pl121AssetSN", "CAT", pl121.GpsDeviceID, pl121.fk_DeviceTypeID);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow.AddDays(-1));

      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(1, deviceInfo.Count, "Expected one PL device to be returned");
      ConfirmCorrectDeviceInfo(deviceInfo[0], "pl121IBKey", "pl121AssetSN", "CAT", "registeredDealerBSSID2", "registeredDealerBSSID2");
      ConfirmDevicesArePLs(deviceInfo);
    }

    [DatabaseTest]
    [TestMethod]
    public void TestTwoOwners_MessageCameInWhenOwner1Owned_AndWhenOwner2Owned_ReturnsOwner2_AndBothOwnersAreDealers()
    {
      Device pl121 = SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "registeredDealerBSSID1", "registeredDealerBSSID1", DateTime.UtcNow.AddDays(-16), DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-75), DateTime.UtcNow.AddDays(-18), true);

      Ctx.OpContext.AssetDeviceHistory.AddObject(new AssetDeviceHistory
      {
        fk_AssetID = pl121.Asset.First().AssetID,
        fk_DeviceID = pl121.ID,
        OwnerBSSID = pl121.OwnerBSSID,
        StartUTC = DateTime.UtcNow.AddDays(-31),
        EndUTC = DateTime.UtcNow.AddDays(-16)
      });
      Customer dealer = Entity.Customer.Dealer.BssId("registeredDealerBSSID2").Save();
      pl121.OwnerBSSID = "registeredDealerBSSID2";
      pl121.DeregisteredUTC = DateTime.UtcNow;
      Ctx.OpContext.SaveChanges();
      Service service = Entity.Service.Essentials.ForDevice(pl121).ActivationDate(DateTime.UtcNow.AddDays(-75)).CancellationDate(DateTime.UtcNow.AddDays(-60)).Save();
      CreateOEMDataMessage(DateTime.UtcNow.AddDays(-14), "pl121AssetSN", "CAT", pl121.GpsDeviceID, pl121.fk_DeviceTypeID);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow.AddDays(-1));

      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(1, deviceInfo.Count, "Expected one PL device to be returned");
      ConfirmCorrectDeviceInfo(deviceInfo[0], "pl121IBKey", "pl121AssetSN", "CAT", "registeredDealerBSSID2", "registeredDealerBSSID2");
      ConfirmDevicesArePLs(deviceInfo);
    }

    [DatabaseTest]
    [TestMethod]
    public void TestReportedDuringMonth_AndDeregisteredDuringSameMonth_ReturnsNoDeviceData()
    {
      SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", DateTime.UtcNow.AddDays(-6), DeviceStateEnum.Subscribed, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-26), false);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow);
      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(0, deviceInfo.Count, "Expected no PL device to be returned");
    }

    [DatabaseTest]
    [TestMethod]
    public void TestActiveSubscriptionReturnsNoDeviceData()
    {
      SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", null, DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), new DateTime(9999, 12, 31), DateTime.UtcNow.AddDays(-3), false);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow);
      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(0, deviceInfo.Count, "Expected no PL device to be returned");
    }

    [DatabaseTest]
    [TestMethod]
    public void TestNoMessagesReturnsNoDeviceData()
    {
      SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", null, DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), null, false);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow);
      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(0, deviceInfo.Count, "Expected no PL device to be returned");
    }

    [DatabaseTest]
    [TestMethod]
    public void TestPL121MessageDateAfterEndDateReturnsNoData()
    {
      SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", DateTime.UtcNow.AddDays(-1), DeviceStateEnum.Provisioned, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-3), false);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow.AddDays(-5));
      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(0, deviceInfo.Count, "Expected no PL device to be returned");
    }

    [DatabaseTest]
    [TestMethod]
    public void TestDeregisteredDateBeforeStartDateReturnsNoDeviceData()
    {
      SetUpTestData(Entity.Device.PL121, "pl121IBKey", "pl121AssetSN", "CAT", "ownerBSSID", "registeredDealerBSSID", DateTime.UtcNow.AddDays(-32), DeviceStateEnum.DeregisteredStore, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-3), false);
      List<ActivityChargeDeviceInfo> deviceInfo = GetActivityChargeDeviceInfo(DateTime.UtcNow.AddDays(-31), DateTime.UtcNow);
      Assert.IsNotNull(deviceInfo, "Did not expect a null return");
      Assert.AreEqual(0, deviceInfo.Count, "Expected no PL device to be returned");
    }
    #endregion

    #region Helpers
    private Device SetUpTestData(DeviceBuilder deviceIn,
      string deviceIBKey, string equipmentSN, string makeCode, string ownerBSSID, string registeredDealerBSSID,
      DateTime? deregisteredUTC, DeviceStateEnum deviceState,
      DateTime serviceActivationDate, DateTime serviceCancellationDate,
      DateTime? timestampOfOEMDataMessage, bool dealerIsOwner)
    {
      if (dealerIsOwner) {
        Customer dealer = Entity.Customer.Dealer.BssId(registeredDealerBSSID).Save();
      } else {
        Customer customerAccount = Entity.Customer.Account.BssId(ownerBSSID).Save();
        Customer dealer = Entity.Customer.Dealer.BssId(registeredDealerBSSID).Save();
        CustomerRelationship relationship =Entity.CustomerRelationship.Relate(dealer, customerAccount).Save();
      }
      Device device = deviceIn.IbKey(deviceIBKey).OwnerBssId(ownerBSSID).DeviceState(deviceState).DeregisteredUTC(deregisteredUTC).Save();
      Asset equipment = Entity.Asset.SerialNumberVin(equipmentSN).WithDevice(device).MakeCode(makeCode).Save();
      Service service = Entity.Service.Essentials.ForDevice(device).ActivationDate(serviceActivationDate).CancellationDate(serviceCancellationDate).Save();
      if (timestampOfOEMDataMessage.HasValue)
      {
        CreateOEMDataMessage(timestampOfOEMDataMessage.Value, equipmentSN, makeCode, device.GpsDeviceID, device.fk_DeviceTypeID);
      }

      return device;
    }

    private void CreateOEMDataMessage(DateTime timestampOfOEMDataMessage, String equipmentSN, String makeCode, string gpsDeviceID, int deviceTypeID)
    {
      Ctx.OemDataContext.CAT_SMULoc.AddObject(new CAT_SMULoc
      {
        SerialNumber = equipmentSN,
        GpsDeviceID = gpsDeviceID,
        EventUTC = timestampOfOEMDataMessage,
        DeviceTypeID = deviceTypeID,
        MessageID = IdGen.GetId(),
        RecordID = IdGen.GetId(),
        MakeCode = makeCode
      });
      Ctx.OemDataContext.SaveChanges();
    }

    private List<ActivityChargeDeviceInfo> GetActivityChargeDeviceInfo(DateTime startDate, DateTime endDate)
    {
      List<ActivityChargeDeviceInfo> devices = new List<ActivityChargeDeviceInfo>();

      StoredProcDefinition proc = new StoredProcDefinition("NH_OEMData", "uspRpt_Cat_ActivityCharge");
      proc.AddInput("@startDate", startDate);
      proc.AddInput("@endDate", endDate);
      using (SqlDataReader reader = SqlAccessMethods.ExecuteReader(proc))
      {
        while (reader.Read())
        {
          devices.Add(new ActivityChargeDeviceInfo
          {
            deviceIBKey = reader.GetString(reader.GetOrdinal("IBKey")),
            equipmentSN = reader.GetString(reader.GetOrdinal("EquipmentSN")),
            makeCode = reader.GetString(reader.GetOrdinal("MakeCode")),
            ownerBSSID = reader.GetString(reader.GetOrdinal("OwnerBSSID")),
            registeredDealerBSSID = reader.GetString(reader.GetOrdinal("RegisteredDealerBSSID"))
          });
        }
      }
      return devices;
    }

    private void ConfirmDevicesArePLs(List<ActivityChargeDeviceInfo> deviceInfo)
    {
      foreach (ActivityChargeDeviceInfo device in deviceInfo)
      {
        int deviceTypeFromDB = (from d in Ctx.OpContext.DeviceReadOnly
                                where d.IBKey == device.deviceIBKey
                                select d.fk_DeviceTypeID).FirstOrDefault<int>();
        Assert.IsTrue((deviceTypeFromDB == (int)DeviceTypeEnum.PL121) || (deviceTypeFromDB == (int)DeviceTypeEnum.PL321), "Expected a PL device");
      }
    }

    private void ConfirmCorrectDeviceInfo(ActivityChargeDeviceInfo deviceInfo, string deviceIBKey, string equipmentSN, string makeCode, string ownerBSSID, string registeredDealerBSSID)
    {
      Assert.AreEqual(deviceIBKey, deviceInfo.deviceIBKey, "Wrong device IB Key");
      Assert.AreEqual(equipmentSN, deviceInfo.equipmentSN, "Wrong asset SN");
      Assert.AreEqual(makeCode, deviceInfo.makeCode, "Wrong Make Code");
      Assert.AreEqual(ownerBSSID, deviceInfo.ownerBSSID, "Wrong Owner BSS ID");
      Assert.AreEqual(registeredDealerBSSID, deviceInfo.registeredDealerBSSID, "Wrong Registered Dealer BSS ID");
    }
    #endregion
  }

  class ActivityChargeDeviceInfo
  {
    public string deviceIBKey;
    public string equipmentSN;
    public string makeCode;
    public string ownerBSSID;
    public string registeredDealerBSSID;
  }
}
