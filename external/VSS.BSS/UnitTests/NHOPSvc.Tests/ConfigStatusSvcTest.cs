using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.MTSMessages;
using VSS.Hosted.VLCommon.PLMessages;
using VSS.Nighthawk.NHOPSvc.ConfigStatus;
using VSS.UnitTest.Common;

namespace NHOPSvc.Tests
{
  /// <summary>
  /// Summary description for ConfigStatusSvc
  /// </summary>
  [TestClass]
  public class ConfigStatusSvcTest : UnitTestBase
  {
    private readonly ConfigStatusSvc _svc;

    public ConfigStatusSvcTest()
    {
      _svc = new ConfigStatusSvc();
    }

    [TestMethod]
    public void GetDevice_PL121_PL321_Duplicate_GPSDeviceID()
    {
      var devicePL121 = TestData.TestPL121;
      devicePL121.DeregisteredUTC = DateTime.UtcNow;

      devicePL121.GpsDeviceID = "DUPLICATE";
      var devicePL321 = TestData.TestPL321;
      devicePL321.GpsDeviceID = "DUPLICATE";
      devicePL321.fk_DeviceStateID = (int) DeviceStateEnum.Subscribed;
      var po = new PrivateObject(typeof (ConfigStatusSvc));
      var expected =
        (Device)
          po.Invoke("GetDevice",
            new object[] {Ctx.OpContext, devicePL321.GpsDeviceID, (DeviceTypeEnum) devicePL321.fk_DeviceTypeID});

      Assert.AreEqual(devicePL321.ID, expected.ID, "GetDevice returned the wrong device.");
    }

    [TestMethod]
    [DatabaseTest]
    public void TestFirmwareUpdate()
    {
      SetUpForTest();
      var fv = (from dfv in Ctx.OpContext.DeviceFirmwareVersionReadOnly
        where dfv.fk_DeviceID == TestData.TestMTS522.ID
        select dfv).FirstOrDefault<DeviceFirmwareVersion>();
      Assert.IsNotNull(fv);
      Assert.AreEqual((int) FirmwareUpdateStatusEnum.NeverUpdated, fv.fk_FirmwareUpdateStatusID,
        "expected 'Never Updated' for status");
      _svc.UpdateFirmwareStatus(TestData.TestMTS522.GpsDeviceID, DeviceTypeEnum.Series522,
        FirmwareUpdateStatusEnum.Acknowledged);
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        fv = (from dfv in opCtx.DeviceFirmwareVersionReadOnly
          where dfv.fk_DeviceID == TestData.TestMTS522.ID
          select dfv).FirstOrDefault<DeviceFirmwareVersion>();
        Assert.IsNotNull(fv);
        Assert.AreEqual((int) FirmwareUpdateStatusEnum.Acknowledged, fv.fk_FirmwareUpdateStatusID,
          "expected 'Acknowledged' for status");
      }
    }

    [TestMethod]
    public void TestProcessAddressClaim_NoFoundMID()
    {
      SetUpForTest();
      var id = Guid.NewGuid().ToString();
      _svc.ProcessAddressClaim(id, false, 0, 0, 0, 0, 0, 0, 0, 0);
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var pac = (from mid in opCtx.MIDReadOnly
          from middesc in opCtx.MIDDescReadOnly
          where mid.MID1 == id
                && middesc.fk_MIDID == mid.ID
          select new
          {
            mid,
            middesc
          }).FirstOrDefault();
        Assert.IsNotNull(pac);
        Assert.AreEqual(id, pac.mid.MID1, "Incorrect MID");
        Assert.AreEqual("Function: 0", pac.middesc.Description, "IncorrectDescription");
        Assert.AreEqual((int) LanguageEnum.enUS, pac.middesc.fk_LanguageID, "Incorrect Language");
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TestProcessAddressClaim_SameFunctionOnly()
    {
      SetUpForTest();
      var id = Guid.NewGuid().ToString();
      var j1939DefaultMid1 = new J1939DefaultMIDDescription
      {
        J1939Function = 255,
        Name = "TESTFunction0",
        fk_LanguageID = (int) LanguageEnum.enUS
      };
      Ctx.OpContext.J1939DefaultMIDDescription.AddObject(j1939DefaultMid1);

      var j1939DefaultMid2 = new J1939DefaultMIDDescription
      {
        J1939Function = 255,
        IndustryGroup = 127,
        VehicleSystem = 127,
        Name = "TESTFunction0IndustryGroup1",
        fk_LanguageID = (int) LanguageEnum.enUS
      };
      Ctx.OpContext.J1939DefaultMIDDescription.AddObject(j1939DefaultMid2);

      var j1939DefaultMid3 = new J1939DefaultMIDDescription
      {
        J1939Function = 255,
        IndustryGroup = 0,
        VehicleSystem = 1,
        VehicleSystemInstance = 1,
        ArbitraryAddressCapable = false,
        ECUInstance = 1,
        FunctionInstance = 1,
        IdentityNumber = 1
      };
      j1939DefaultMid3.IndustryGroup = 1;
      j1939DefaultMid3.ManufacturerCode = 1;
      j1939DefaultMid3.Name = "TESTFunction0IndustryGroup2";
      j1939DefaultMid3.fk_LanguageID = (int) LanguageEnum.enUS;
      Ctx.OpContext.J1939DefaultMIDDescription.AddObject(j1939DefaultMid3);
      Ctx.OpContext.SaveChanges();

      _svc.ProcessAddressClaim(id, false, 127, 0, 0, 255, 0, 0, 0, 0);
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var pac = (from mid in opCtx.MIDReadOnly
          from middesc in opCtx.MIDDescReadOnly
          where mid.MID1 == id
                && middesc.fk_MIDID == mid.ID
          select new
          {
            mid,
            middesc
          }).FirstOrDefault();
        Assert.IsNotNull(pac);
        Assert.AreEqual(id, pac.mid.MID1, "Incorrect MID");
        Assert.AreEqual("TESTFunction0", pac.middesc.Description, "IncorrectDescription");
        Assert.AreEqual((int) LanguageEnum.enUS, pac.middesc.fk_LanguageID, "Incorrect Language");
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TestProcessAddressClaim_SameAll()
    {
      SetUpForTest();
      var id = Guid.NewGuid().ToString();
      var j1939DefaultMid1 = new J1939DefaultMIDDescription
      {
        J1939Function = 0,
        Name = "TESTFunction0",
        fk_LanguageID = (int) LanguageEnum.enUS
      };
      Ctx.OpContext.J1939DefaultMIDDescription.AddObject(j1939DefaultMid1);

      var j1939DefaultMid2 = new J1939DefaultMIDDescription
      {
        J1939Function = 0,
        IndustryGroup = 1,
        Name = "TESTFunction0IndustryGroup1",
        fk_LanguageID = (int) LanguageEnum.enUS
      };
      Ctx.OpContext.J1939DefaultMIDDescription.AddObject(j1939DefaultMid2);

      var j1939DefaultMid3 = new J1939DefaultMIDDescription
      {
        J1939Function = 0,
        IndustryGroup = 2,
        VehicleSystem = 1,
        VehicleSystemInstance = 1,
        ArbitraryAddressCapable = false,
        ECUInstance = 1,
        FunctionInstance = 1,
        IdentityNumber = 1,
        ManufacturerCode = 1,
        Name = "TESTFunction0IndustryGroup2",
        fk_LanguageID = (int) LanguageEnum.enUS
      };
      Ctx.OpContext.J1939DefaultMIDDescription.AddObject(j1939DefaultMid3);
      Ctx.OpContext.SaveChanges();

      _svc.ProcessAddressClaim(id, false, 2, 1, 1, 0, 1, 1, 1, 1);
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var pac = (from mid in opCtx.MIDReadOnly
          from middesc in opCtx.MIDDescReadOnly
          where mid.MID1 == id
                && middesc.fk_MIDID == mid.ID
          select new
          {
            mid,
            middesc
          }).FirstOrDefault();
        Assert.IsNotNull(pac);
        Assert.AreEqual(id, pac.mid.MID1, "Incorrect MID");
        Assert.AreEqual("TESTFunction0IndustryGroup2", pac.middesc.Description, "IncorrectDescription");
        Assert.AreEqual((int) LanguageEnum.enUS, pac.middesc.fk_LanguageID, "Incorrect Language");
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TestProcessAddressClaim_MIDAlreadyAdded()
    {
      SetUpForTest();
      const string id = "-1";

      var mid = new MID {MID1 = id};
      Ctx.OpContext.MID.AddObject(mid);
      Ctx.OpContext.SaveChanges();
      var midDesc = new MIDDesc {Description = "TEST", fk_MIDID = mid.ID, fk_LanguageID = (int) LanguageEnum.enUS};
      Ctx.OpContext.MIDDesc.AddObject(midDesc);
      Ctx.OpContext.SaveChanges();

      var j1939DefaultMid1 = new J1939DefaultMIDDescription
      {
        J1939Function = 0,
        Name = "TESTFunction0",
        fk_LanguageID = (int) LanguageEnum.enUS
      };
      Ctx.OpContext.J1939DefaultMIDDescription.AddObject(j1939DefaultMid1);

      var j1939DefaultMid2 = new J1939DefaultMIDDescription
      {
        J1939Function = 0,
        IndustryGroup = 1,
        Name = "TESTFunction0IndustryGroup1",
        fk_LanguageID = (int) LanguageEnum.enUS
      };
      Ctx.OpContext.J1939DefaultMIDDescription.AddObject(j1939DefaultMid2);

      var j1939DefaultMid3 = new J1939DefaultMIDDescription
      {
        J1939Function = 0,
        IndustryGroup = 2,
        VehicleSystem = 1,
        VehicleSystemInstance = 1,
        ArbitraryAddressCapable = false,
        ECUInstance = 1,
        FunctionInstance = 1,
        IdentityNumber = 1,
        ManufacturerCode = 1,
        Name = "TESTFunction0IndustryGroup2",
        fk_LanguageID = (int) LanguageEnum.enUS
      };
      Ctx.OpContext.J1939DefaultMIDDescription.AddObject(j1939DefaultMid3);
      Ctx.OpContext.SaveChanges();

      _svc.ProcessAddressClaim(id, false, 2, 1, 1, 0, 1, 1, 1, 1);
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var pac = (from m in opCtx.MIDReadOnly
          from md in opCtx.MIDDescReadOnly
          where m.MID1 == id
                && md.fk_MIDID == m.ID
                && md.fk_LanguageID == (int) LanguageEnum.enUS
          select new
          {
            m,
            md
          }).ToList();
        Assert.AreEqual(1, pac.Count, "Incorrect number ofr MIDs added");
        Assert.IsNotNull(pac);
        var firstOrDefault = pac.FirstOrDefault();
        Assert.IsNotNull(firstOrDefault);
        Assert.AreEqual(id, firstOrDefault.m.MID1, "Incorrect MID");
        Assert.AreEqual("TEST", firstOrDefault.md.Description, "IncorrectDescription");
        Assert.AreEqual((int) LanguageEnum.enUS, firstOrDefault.md.fk_LanguageID, "Incorrect Language");
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TestFirmwareUpdateWithSuccessCall()
    {
      SetUpForTest();
      var fv = (from dfv in Ctx.OpContext.DeviceFirmwareVersionReadOnly
        where dfv.fk_DeviceID == TestData.TestMTS522.ID
        select dfv).FirstOrDefault<DeviceFirmwareVersion>();
      Assert.IsNotNull(fv);
      Assert.AreNotEqual(fv.fk_MTS500FirmwareVersionIDInstalled, fv.fk_MTS500FirmwareVersionIDPending,
        "expected the two IDs to be different");
      _svc.UpdateFirmwareStatus(TestData.TestMTS522.GpsDeviceID, DeviceTypeEnum.Series522,
        FirmwareUpdateStatusEnum.Successful);
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        fv = (from dfv in opCtx.DeviceFirmwareVersionReadOnly
          where dfv.fk_DeviceID == TestData.TestMTS522.ID
          select dfv).FirstOrDefault<DeviceFirmwareVersion>();
        Assert.IsNotNull(fv);
        Assert.AreEqual(fv.fk_MTS500FirmwareVersionIDInstalled, fv.fk_MTS500FirmwareVersionIDPending,
          "expected the two IDs to be the same");
      }

      using (var opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
          var count = (from pr3Out in opCtx1.MTSOutReadOnly
          where pr3Out.SerialNumber == TestData.TestMTS522.GpsDeviceID
                && pr3Out.PacketID == RequestPersonalityMessage.kPacketID
                && pr3Out.DeviceType == TestData.TestMTS522.fk_DeviceTypeID
          select pr3Out).Count();
        Assert.AreEqual(1, count, "expected only one personality record");
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TestUpdatePersonalityForMessagesThorughBus()
    {
      var customer =
        Entity.Customer.EndCustomer.Name("GetAssetHealthHistoryTest_Customer").BssId("Bss1234").SyncWithRpt().Save();
      var user =
        Entity.ActiveUser.ForUser(Entity.User.ForCustomer(customer).LastName("GetAssetHealthHistoryTest_User").Save())
          .Save();
      var device = Entity.Device.PL641.OwnerBssId(customer.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
      var asset =
        Entity.Asset.EquipmentVIN("EV2")
          .SerialNumberVin("SN2")
          .Name("Asset2")
          .WithDevice(device)
          .WithCoreService()
          .SyncWithRpt()
          .Save();

      var assetIDs = new List<long> {asset.AssetID};
      Helpers.WorkingSet.Populate(user);
      Helpers.WorkingSet.Select(user, assetIDs);

      var devicePersonality = new DevicePersonality
      {
        AssetID = asset.AssetID,
        Description = "Test description",
        Value = "Test value",
        fk_PersonalityTypeID = (int) PersonalityTypeEnum.CellularRadioFirmware,
        GPSDeviceID = device.GpsDeviceID,
        DeviceType = (DeviceTypeEnum) device.fk_DeviceTypeID
      };

      AssetIDCache.Init(true);
      _svc.UpdatePersonality(devicePersonality);
      using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var dP = (from dp in ctx.DevicePersonality
          where dp.fk_DeviceID == device.ID
          select dp).ToList();
        Assert.IsNotNull(dP, "DevicePersonality is null");
        Assert.AreEqual(1, dP.Count(), "DevicePersonality should have one item");
        var firstOrDefault = dP.FirstOrDefault();
        Assert.IsNotNull(firstOrDefault);
        Assert.AreEqual((int) PersonalityTypeEnum.CellularRadioFirmware, firstOrDefault.fk_PersonalityTypeID);
        Assert.AreEqual("Test value", firstOrDefault.Value);
        Assert.AreEqual("Test description", firstOrDefault.Description);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TestUpdatePersonalityForMessagesThorughBus_UpdateExistingDevicePersonality()
    {
      var customer =
        Entity.Customer.EndCustomer.Name("GetAssetHealthHistoryTest_Customer").BssId("Bss1234").SyncWithRpt().Save();
      var user =
        Entity.ActiveUser.ForUser(Entity.User.ForCustomer(customer).LastName("GetAssetHealthHistoryTest_User").Save())
          .Save();
      var device = Entity.Device.PL641.OwnerBssId(customer.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
      var asset =
        Entity.Asset.EquipmentVIN("EV2")
          .SerialNumberVin("SN2")
          .Name("Asset2")
          .WithDevice(device)
          .WithCoreService()
          .SyncWithRpt()
          .Save();

      var assetIDs = new List<long> {asset.AssetID};
      Helpers.WorkingSet.Populate(user);
      Helpers.WorkingSet.Select(user, assetIDs);

      var devicePersonality = new DevicePersonality
      {
        fk_DeviceID = asset.fk_DeviceID,
        fk_PersonalityTypeID = (int) PersonalityTypeEnum.CellularRadioFirmware,
        Description = "Test Description",
        Value = "Initial Value",
        GPSDeviceID = device.GpsDeviceID,
        DeviceType = (DeviceTypeEnum) device.fk_DeviceTypeID
      };
      Ctx.OpContext.DevicePersonality.AddObject(devicePersonality);
      Ctx.OpContext.SaveChanges();

      var dp1 = (from d in Ctx.OpContext.DevicePersonality
        where d.fk_DeviceID == asset.fk_DeviceID
        select d).FirstOrDefault();

      Assert.IsNotNull(dp1, "DevicePersonality is null");
      Assert.AreEqual((int) PersonalityTypeEnum.CellularRadioFirmware, dp1.fk_PersonalityTypeID);
      Assert.AreEqual("Initial Value", dp1.Value);
      Assert.AreEqual("Test Description", dp1.Description);

      var devicePersonality2 = new DevicePersonality
      {
        AssetID = asset.AssetID,
        Description = "Test description",
        Value = "Test value",
        fk_PersonalityTypeID = (int) PersonalityTypeEnum.CellularRadioFirmware,
        GPSDeviceID = device.GpsDeviceID,
        DeviceType = (DeviceTypeEnum) device.fk_DeviceTypeID
      };

      _svc.UpdatePersonality(devicePersonality2);
      using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var dP = (from dp in ctx.DevicePersonality
          where dp.fk_DeviceID == device.ID
          select dp).ToList();
        Assert.IsNotNull(dP, "DevicePersonality is null");
        Assert.AreEqual(1, dP.Count(), "DevicePersonality hould have only one item");
        var firstOrDefault = dP.FirstOrDefault();
        Assert.IsNotNull(firstOrDefault);
        Assert.AreEqual((int) PersonalityTypeEnum.CellularRadioFirmware, firstOrDefault.fk_PersonalityTypeID);
        Assert.AreEqual("Test value", firstOrDefault.Value);
        Assert.AreEqual("Test description", firstOrDefault.Description);
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void TestUpdatePersonalityForMessagesThorughBus_Invalid()
    {
      var customer =
        Entity.Customer.EndCustomer.Name("GetAssetHealthHistoryTest_Customer").BssId("Bss1234").SyncWithRpt().Save();
      var user =
        Entity.ActiveUser.ForUser(Entity.User.ForCustomer(customer).LastName("GetAssetHealthHistoryTest_User").Save())
          .Save();
      var device = Entity.Device.PL641.OwnerBssId(customer.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
      var asset =
        Entity.Asset.EquipmentVIN("EV2")
          .SerialNumberVin("SN2")
          .Name("Asset2")
          .WithDevice(device)
          .WithCoreService()
          .SyncWithRpt()
          .Save();

      var assetIDs = new List<long> {asset.AssetID};
      Helpers.WorkingSet.Populate(user);
      Helpers.WorkingSet.Select(user, assetIDs);

      var devicePersonality = new DevicePersonality
      {
        AssetID = 12354676,
        Description = "Test description",
        Value = "Test value",
        fk_PersonalityTypeID = (int) PersonalityTypeEnum.CellularRadioFirmware
      };

      _svc.UpdatePersonality(devicePersonality);
      using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var dP = (from dp in ctx.DevicePersonality
          where dp.fk_DeviceID == device.ID
          select dp).ToList();
        Assert.AreEqual(0, dP.Count, "Did not expect any DevicePersonality records");
      }
    }

    [TestMethod]
    public void TestUpdatePersonality()
    {
      SetUpForTest();
      var fv = (from dfv in Ctx.OpContext.DeviceFirmwareVersionReadOnly
        where dfv.fk_DeviceID == TestData.TestMTS522.ID
        select dfv).FirstOrDefault<DeviceFirmwareVersion>();
      Assert.IsNotNull(fv);
      Assert.AreEqual("<tag>data</tag>", fv.CurrentFirmwareReport,
        "expected '<tag>data</tag>' for current firmware report");
      _svc.UpdatePersonality(TestData.TestMTS522.GpsDeviceID, DeviceTypeEnum.Series522,
        "<PersonalityReport><U_Boot>15</U_Boot></PersonalityReport>");
      var devicePersonality = (from d in Ctx.OpContext.DevicePersonalityReadOnly
        where d.fk_DeviceID == TestData.TestMTS522.ID
        select d).ToList();
      Assert.IsNotNull(devicePersonality, "DevicePersonality is null");
      Assert.AreEqual(1, devicePersonality.Count(), "DevicePersonality should only have one item");
      var firstOrDefault = devicePersonality.FirstOrDefault();
      Assert.IsNotNull(firstOrDefault);
      Assert.AreEqual((int) PersonalityTypeEnum.U_Boot,
        firstOrDefault.fk_PersonalityTypeID, "Personality Type should be U-Boot");
      Assert.AreEqual("15", firstOrDefault.Value, "Value should equal 15");
    }

    ///// <summary>
    /////A test for UpdateDeviceConfiguration
    /////</summary>
    [TestMethod]
    [DatabaseTest]
    public void UpdateDeviceConfigurationTest_Success()
    {
      var gpsDeviceID = TestData.TestPL121.GpsDeviceID;
      const DeviceTypeEnum deviceType = DeviceTypeEnum.PL121;
      var msg = new PL321OTAConfigMessages
      {
        RuntimeHoursAdj = TimeSpan.FromHours(15),
        ReportStartTimeUTC = DateTime.UtcNow,
        PositionReportConfig = 1,
        GlobalGramEnable = true,
        Level1Frequency = null,
        Level2Frequency = EventFrequency.Immediately,
        Level3Frequency = EventFrequency.Immediately,
        DiagnosticTransmissionFrequency = EventFrequency.Next,
        SmuFuelReporting = SMUFuelReporting.SMUFUEL,
        NextMessageInterval = TimeSpan.FromHours(15),
        EventIntervals = TimeSpan.FromHours(37)
      };
      var config = msg.GetPLConfigData(DateTime.UtcNow.AddDays(-2));

      // force this to be the oldest pending config date
      var generalConfig = config.OfType<PLConfigData.GeneralRegistry>().First();
      Assert.IsTrue(generalConfig.RuntimeHoursSentUTC.HasValue);
      generalConfig.RuntimeHoursSentUTC = generalConfig.RuntimeHoursSentUTC.Value.AddDays(-2);

      _svc.UpdatePLDeviceConfiguration(gpsDeviceID, deviceType, MessageStatusEnum.Sent, config);

      string deviceXML;
      const int devicetypeid = (int) deviceType;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var device = (from d in opCtx.Device
          where d.fk_DeviceTypeID == devicetypeid
                && d.GpsDeviceID == gpsDeviceID
          select d).FirstOrDefault();
        Assert.IsNotNull(device);
        deviceXML = device.DeviceDetailsXML;
        Assert.IsFalse(string.IsNullOrEmpty(deviceXML), "DeviceDetailsXML should have something in it");

        var pending = XElement.Parse(deviceXML).Elements("Pending").FirstOrDefault();
        Assert.IsNotNull(pending);
        var transmissionRegistry = pending.Elements("TransmissionRegistry");
        Assert.IsNotNull(transmissionRegistry);
        var eventReportingFrequencyElements =
          (from e in transmissionRegistry select e.Elements("EventReportingFrequency"));
        var eventReportingFrequency = eventReportingFrequencyElements.FirstOrDefault();
        Assert.IsNotNull(eventReportingFrequency);
        var reportingFrequency = eventReportingFrequency as IList<XElement> ?? eventReportingFrequency.ToList();
        var attributes = reportingFrequency.Attributes();
        Assert.IsNotNull(attributes);
        var newElement = reportingFrequency.Attributes();

        Assert.IsNotNull(newElement);
        var xAttributes = newElement as IList<XAttribute> ?? newElement.ToList();
        Assert.AreEqual(6, xAttributes.Count(), "Count Should be 6");
        foreach (var a in xAttributes)
        {
          Assert.IsFalse(a.Name == "level1EventFreqCode", "Level 1 Freq should be null");
        }

        Assert.IsNotNull(device.OldestPendingKeyDate, "Device.OldestPendingKeyDate should be populated");
        Assert.IsTrue(device.OldestPendingKeyDate.HasValue);
        Assert.AreEqual(generalConfig.RuntimeHoursSentUTC.Value.KeyDate(), device.OldestPendingKeyDate.Value,
          "Device.OldestPendingKeyDate should be the oldest pending config timestamp");
      }
      msg.Level1Frequency = EventFrequency.Never;
      msg.Level2Frequency = null;
      _svc.UpdatePLDeviceConfiguration(gpsDeviceID, deviceType, MessageStatusEnum.Sent,
        msg.GetPLConfigData(DateTime.UtcNow));
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var deviceXML1 = (from d in opCtx.Device
          where d.fk_DeviceTypeID == devicetypeid
                && d.GpsDeviceID == gpsDeviceID
          select d.DeviceDetailsXML).FirstOrDefault<string>();
        Assert.IsFalse(string.IsNullOrEmpty(deviceXML1), "DeviceDetailsXML should have something in it");
        Assert.IsFalse(deviceXML == deviceXML1, "XML Should Not Equal");
        var element = XElement.Parse(deviceXML1);
        var pending = element.Elements("Pending").FirstOrDefault();
        Assert.IsNotNull(pending);
        var transmissionRegistry = pending.Elements("TransmissionRegistry");
        Assert.IsNotNull(transmissionRegistry);
        var eventReportingFrequencyElements =
          (from e in transmissionRegistry select e.Elements("EventReportingFrequency"));
        var eventReportingFrequency = eventReportingFrequencyElements.FirstOrDefault();
        Assert.IsNotNull(eventReportingFrequency);
        var reportingFrequency = eventReportingFrequency as IList<XElement> ?? eventReportingFrequency.ToList();
        var attributes = reportingFrequency.Attributes();
        Assert.IsNotNull(attributes);
        var level1Element = reportingFrequency.Attributes("level1EventFreqCode");
        Assert.IsNotNull(level1Element);
        var xAttributes = level1Element as IList<XAttribute> ?? level1Element.ToList();
        Assert.IsNotNull(xAttributes);
        Assert.AreEqual(1, xAttributes.Count(), "Count Should be 1");
        var firstOrDefault = xAttributes.FirstOrDefault();
        Assert.IsNotNull(firstOrDefault);
        Assert.AreEqual(EventFrequency.Never.ToString().ToUpper(), firstOrDefault.Value.ToUpper(),
          "String should be never");
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateDeviceConfigurationTest_Success_PL321VIMS()
    {
      var gpsDeviceID = TestData.TestPL321.GpsDeviceID;
      const DeviceTypeEnum deviceType = DeviceTypeEnum.PL321;
      var msg = new PL121AndPL321VIMSOTAConfigMessages {SubType = 0X02, ReportStartTimeUTC = DateTime.UtcNow};
      msg.SetPositionReportConfig(1, DeviceTypeEnum.PL321);
      msg.GlobalGramEnable = true;
      msg.Level1Frequency = null;
      msg.Level2Frequency = EventFrequency.Immediately;
      msg.Level3Frequency = EventFrequency.Immediately;
      msg.DiagnosticTransmissionFrequency = EventFrequency.Next;
      msg.SmuFuelReporting = SMUFuelReporting.SMUFUEL;
      msg.NextMessageInterval = TimeSpan.FromHours(15);
      msg.EventIntervals = TimeSpan.FromHours(37);
      msg.RuntimeHoursAdj = TimeSpan.FromHours(15);
      var config = msg.GetPLConfigData(DateTime.UtcNow.AddDays(-2));

      // force this to be the oldest pending config date
      var generalConfig = config.OfType<PLConfigData.GeneralRegistry>().First();
      Assert.IsTrue(generalConfig.RuntimeHoursSentUTC.HasValue);
      generalConfig.RuntimeHoursSentUTC = generalConfig.RuntimeHoursSentUTC.Value.AddDays(-2);

      _svc.UpdatePLDeviceConfiguration(gpsDeviceID, deviceType, MessageStatusEnum.Sent, config);

      string deviceXML;
      const int devicetypeid = (int) deviceType;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var device = (from d in opCtx.Device
          where d.fk_DeviceTypeID == devicetypeid
                && d.GpsDeviceID == gpsDeviceID
          select d).FirstOrDefault();
        Assert.IsNotNull(device);
        deviceXML = device.DeviceDetailsXML;
        Assert.IsFalse(string.IsNullOrEmpty(deviceXML), "DeviceDetailsXML should have something in it");

        var pending = XElement.Parse(deviceXML).Elements("Pending").FirstOrDefault();
        Assert.IsNotNull(pending);
        var transmissionRegistry = pending.Elements("TransmissionRegistry");
        Assert.IsNotNull(transmissionRegistry);
        var eventReportingFrequencyElements =
          (from e in transmissionRegistry select e.Elements("EventReportingFrequency"));
        var eventReportingFrequency = eventReportingFrequencyElements.FirstOrDefault();
        Assert.IsNotNull(eventReportingFrequency);
        var reportingFrequency = eventReportingFrequency as IList<XElement> ?? eventReportingFrequency.ToList();
        var attributes = reportingFrequency.Attributes();
        Assert.IsNotNull(attributes);
        var newElement = reportingFrequency.Attributes();

        Assert.IsNotNull(newElement);
        var xAttributes = newElement as IList<XAttribute> ?? newElement.ToList();
        Assert.AreEqual(6, xAttributes.Count(), "Count Should be 6");
        foreach (var a in xAttributes)
        {
          Assert.IsFalse(a.Name == "level1EventFreqCode", "Level 1 Freq should be null");
        }

        Assert.IsNotNull(device.OldestPendingKeyDate, "Device.OldestPendingKeyDate should be populated");
        Assert.IsTrue(device.OldestPendingKeyDate.HasValue);
        Assert.AreEqual(generalConfig.RuntimeHoursSentUTC.Value.KeyDate(), device.OldestPendingKeyDate.Value,
          "Device.OldestPendingKeyDate should be the oldest pending config timestamp");
      }
      msg.Level1Frequency = EventFrequency.Never;
      msg.Level2Frequency = null;
      _svc.UpdatePLDeviceConfiguration(gpsDeviceID, deviceType, MessageStatusEnum.Sent,
        msg.GetPLConfigData(DateTime.UtcNow));
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var deviceXML1 = (from d in opCtx.Device
          where d.fk_DeviceTypeID == devicetypeid
                && d.GpsDeviceID == gpsDeviceID
          select d.DeviceDetailsXML).FirstOrDefault<string>();
        Assert.IsFalse(string.IsNullOrEmpty(deviceXML1), "DeviceDetailsXML should have something in it");
        Assert.IsFalse(deviceXML == deviceXML1, "XML Should Not Equal");
        var element = XElement.Parse(deviceXML1);
        var pending = element.Elements("Pending").FirstOrDefault();
        Assert.IsNotNull(pending);
        var transmissionRegistry = pending.Elements("TransmissionRegistry");
        Assert.IsNotNull(transmissionRegistry);
        var eventReportingFrequencyElements =
          (from e in transmissionRegistry select e.Elements("EventReportingFrequency"));
        var eventReportingFrequency = eventReportingFrequencyElements.FirstOrDefault();
        Assert.IsNotNull(eventReportingFrequency);
        var reportingFrequency = eventReportingFrequency as IList<XElement> ?? eventReportingFrequency.ToList();
        var attributes = reportingFrequency.Attributes();
        Assert.IsNotNull(attributes);
        var level1Element = reportingFrequency.Attributes("level1EventFreqCode");
        Assert.IsNotNull(level1Element);
        var xAttributes = level1Element as IList<XAttribute> ?? level1Element.ToList();
        Assert.AreEqual(1, xAttributes.Count(), "Count Should be 1");
        var firstOrDefault = xAttributes.FirstOrDefault();
        Assert.IsNotNull(firstOrDefault);
        Assert.AreEqual(EventFrequency.Never.ToString().ToUpper(), firstOrDefault.Value.ToUpper(),
          "String should be never");
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateDeviceConfigurationTest_Success_PL321VIMS_ACK()
    {
      var gpsDeviceID = TestData.TestPL321.GpsDeviceID;
      const DeviceTypeEnum deviceType = DeviceTypeEnum.PL321;

      var generalRegistryconfig = new List<PLConfigData.GeneralRegistry>
      {
        new PLConfigData.GeneralRegistry
        {
          GlobalGramEnable = true,
          ModuleType = "PL321VIMS"
        }
      };

      var config =
        generalRegistryconfig.Where(x => x as PLConfigData.PLConfigBase != null).ToList<PLConfigData.PLConfigBase>();

      _svc.UpdatePLDeviceConfiguration(gpsDeviceID, deviceType, MessageStatusEnum.Acknowledged, config);

      const int devicetypeid = (int) deviceType;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var device = (from d in opCtx.Device
          where d.fk_DeviceTypeID == devicetypeid
                && d.GpsDeviceID == gpsDeviceID
          select d).FirstOrDefault();
        Assert.IsNotNull(device);
        var deviceXML = device.DeviceDetailsXML;
        Assert.IsFalse(string.IsNullOrEmpty(deviceXML), "DeviceDetailsXML should have something in it");

        var dp = (from d in opCtx.DevicePersonalityReadOnly
          where d.fk_DeviceID == device.ID
                && d.fk_PersonalityTypeID == (int) PersonalityTypeEnum.PL321ModuleType
          select d
          ).FirstOrDefault();
        Assert.IsNotNull(dp, "There should be an entry in DevicePersonality table");
        Assert.AreEqual(dp.Value, "PL321VIMS");

        var current = XElement.Parse(deviceXML).Elements("Current").FirstOrDefault();
        Assert.IsNotNull(current);
        var generalRegistry = current.Elements("GeneralRegistry");
        Assert.IsNotNull(generalRegistry);
        var moduleTypes =
          (from e in generalRegistry select e.Elements("moduleType"));
        var selection = moduleTypes.FirstOrDefault();
        Assert.IsNotNull(selection);
        var newElement = selection.FirstOrDefault();

        Assert.IsNotNull(newElement);
        Assert.AreEqual(newElement.Value, "PL321VIMS");
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateDeviceConfigurationTest_Success_PL321VIMS_ACK_Update()
    {
      var gpsDeviceID = TestData.TestPL321.GpsDeviceID;
      const DeviceTypeEnum deviceType = DeviceTypeEnum.PL321;

      var generalRegistryconfig = new List<PLConfigData.GeneralRegistry>
      {
        new PLConfigData.GeneralRegistry
        {
          GlobalGramEnable = true,
          ModuleType = "PL321VIMS"
        }
      };

      var config =
        generalRegistryconfig.Where(x => x as PLConfigData.PLConfigBase != null).ToList<PLConfigData.PLConfigBase>();

      _svc.UpdatePLDeviceConfiguration(gpsDeviceID, deviceType, MessageStatusEnum.Acknowledged, config);

      const int devicetypeid = (int) deviceType;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var device = (from d in opCtx.DeviceReadOnly
          where d.fk_DeviceTypeID == devicetypeid
                && d.GpsDeviceID == gpsDeviceID
          select d).FirstOrDefault();
        Assert.IsNotNull(device);
        var deviceXML = device.DeviceDetailsXML;
        Assert.IsFalse(string.IsNullOrEmpty(deviceXML), "DeviceDetailsXML should have something in it");

        var dp = (from d in opCtx.DevicePersonalityReadOnly
          where d.fk_DeviceID == device.ID
                && d.fk_PersonalityTypeID == (int) PersonalityTypeEnum.PL321ModuleType
          select d
          ).FirstOrDefault();
        Assert.IsNotNull(dp, "There should be an entry in DevicePersonality table");
        Assert.AreEqual(dp.Value, "PL321VIMS");

        var current = XElement.Parse(deviceXML).Elements("Current").FirstOrDefault();
        Assert.IsNotNull(current);
        var generalRegistry = current.Elements("GeneralRegistry");
        Assert.IsNotNull(generalRegistry);
        var moduleTypes =
          (from e in generalRegistry select e.Elements("moduleType"));
        var selection = moduleTypes.FirstOrDefault();
        Assert.IsNotNull(selection);
        var newElement = selection.FirstOrDefault();

        Assert.IsNotNull(newElement);
        Assert.AreEqual(newElement.Value, "PL321VIMS");

        var generalRegistryconfig2 = new List<PLConfigData.GeneralRegistry>
        {
          new PLConfigData.GeneralRegistry
          {
            GlobalGramEnable = false,
            ModuleType = "PL321SR"
          }
        };

        config =
          generalRegistryconfig2.Where(x => x as PLConfigData.PLConfigBase != null).ToList<PLConfigData.PLConfigBase>();


        _svc.UpdatePLDeviceConfiguration(gpsDeviceID, deviceType, MessageStatusEnum.Acknowledged, config);
        device = (from d in opCtx.DeviceReadOnly
          where d.fk_DeviceTypeID == devicetypeid
                && d.GpsDeviceID == gpsDeviceID
          select d).FirstOrDefault();
        Assert.IsNotNull(device);
        deviceXML = device.DeviceDetailsXML;
        Assert.IsFalse(string.IsNullOrEmpty(deviceXML), "DeviceDetailsXML should have something in it");

        dp = (from d in opCtx.DevicePersonalityReadOnly
          where d.fk_DeviceID == device.ID
                && d.fk_PersonalityTypeID == (int) PersonalityTypeEnum.PL321ModuleType
          select d
          ).FirstOrDefault();
        Assert.IsNotNull(dp, "There should be an entry in DevicePersonality table");
        Assert.AreEqual(dp.Value, "PL321SR");

        current = XElement.Parse(deviceXML).Elements("Current").FirstOrDefault();
        Assert.IsNotNull(current);
        generalRegistry = current.Elements("GeneralRegistry");
        Assert.IsNotNull(generalRegistry);
        moduleTypes =
          (from e in generalRegistry select e.Elements("moduleType"));
        selection = moduleTypes.FirstOrDefault();
        Assert.IsNotNull(selection);
        newElement = selection.FirstOrDefault();

        Assert.IsNotNull(newElement);
        Assert.AreEqual(newElement.Value, "PL321SR");
      }
    }

    ///// <summary>
    /////A test for UpdateECMInfo
    /////</summary>

    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoNoECMInfoNoneTest()
    {
      var gpsDeviceID = TestData.TestPL321.GpsDeviceID;
      const DeviceTypeEnum type = DeviceTypeEnum.PL321;

      var ecmInfo = SetUpECMInfoList(new[] {(int) DatalinkEnum.None});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoNoECMInfoCDLTest()
    {
      var gpsDeviceID = TestData.TestMTS522.GpsDeviceID;
      const DeviceTypeEnum type = DeviceTypeEnum.Series522;

      var ecmInfo = SetUpECMInfoList(new[] {(int) DatalinkEnum.CDL});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);
    }

    ///// <summary>
    /////A test for UpdateECMInfo
    /////</summary>
    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoNoECMInfoJ1939Test()
    {
      var gpsDeviceID = TestData.TestMTS522.GpsDeviceID;
      const DeviceTypeEnum type = DeviceTypeEnum.Series522;

      var ecmInfo = SetUpECMInfoList(new[] {(int) DatalinkEnum.J1939});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);
    }

    ///// <summary>
    /////A test for UpdateECMInfo
    /////</summary>
    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoNoECMInfoSAEJ1939Test()
    {
      var gpsDeviceID = TestData.TestMTS522.GpsDeviceID;
      const DeviceTypeEnum type = DeviceTypeEnum.Series522;

      var ecmInfo = SetUpECMInfoList(new[] {(int) DatalinkEnum.SAEJ1939});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);
    }

    ///// <summary>
    /////A test for UpdateECMInfo
    /////</summary>
    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoNoECMInfoBothTest()
    {
      var gpsDeviceID = TestData.TestMTS522.GpsDeviceID;
      const int both = 3;
      const DeviceTypeEnum type = DeviceTypeEnum.Series522;

      var ecmInfo = SetUpECMInfoList(new[] {both});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoBothtoOneTest()
    {
      var gpsDeviceID = TestData.TestMTS522.GpsDeviceID;
      const int both = 3;
      const DeviceTypeEnum type = DeviceTypeEnum.Series522;

      var ecmInfo = SetUpECMInfoList(new[] {both});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      UpdateDatalinkIDs(gpsDeviceID, type);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);

      ecmInfo = SetUpECMInfoList(new[] {(int) DatalinkEnum.CDL});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      UpdateDatalinkIDs(gpsDeviceID, type);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);

      ecmInfo = SetUpECMInfoList(new[] {both});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      UpdateDatalinkIDs(gpsDeviceID, type);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);

      ecmInfo = SetUpECMInfoList(new[] {(int) DatalinkEnum.J1939});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      UpdateDatalinkIDs(gpsDeviceID, type);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoTestForSAEJ1939J1939()
    {
      var gpsDeviceID = TestData.TestMTS522.GpsDeviceID;
      const DeviceTypeEnum type = DeviceTypeEnum.Series522;

      var ecmInfo = SetUpECMInfoList(new[] {(int) DeviceIDData.DataLinkType.SAEJI939AndCDL});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      UpdateDatalinkIDs(gpsDeviceID, type);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);

      ecmInfo = SetUpECMInfoList(new[] {(int) DeviceIDData.DataLinkType.SAEJI939AndCDL});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      UpdateDatalinkIDs(gpsDeviceID, type);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoRemoveECMInfonoDatalinkTest()
    {
      var gpsDeviceID = TestData.TestMTS522.GpsDeviceID;
      const int both = 3;
      const DeviceTypeEnum type = DeviceTypeEnum.Series522;

      var ecmInfo = SetUpECMInfoList(new[] {both});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      UpdateDatalinkIDs(gpsDeviceID, type);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);

      var info = new ECMInfo
      {
        Device = TestData.TestMTS522,
        fk_DeviceID = TestData.TestMTS522.ID,
        DiagnosticProtocolVer = true,
        Engine1SN = "!",
        EventProtocolVer = true,
        HasSMUClock = true,
        IsSyncClockMaster = true,
        PartNumber = "2",
        SerialNumber = "3",
        SoftwareDescription = "4",
        SoftwarePartNumber = "5",
        SoftwareReleaseDate = "6",
        Transmission1SN = "7",
        ID = 447
      };
      Ctx.OpContext.ECMInfo.AddObject(info);
      Ctx.OpContext.SaveChanges();

      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      UpdateDatalinkIDs(gpsDeviceID, type);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);

      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      UpdateDatalinkIDs(gpsDeviceID, type);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoOneToBothTest()
    {
      var gpsDeviceID = TestData.TestMTS522.GpsDeviceID;
      const int both = 3;
      const DeviceTypeEnum type = DeviceTypeEnum.Series522;

      var ecmInfo = SetUpECMInfoList(new[] {(int) DatalinkEnum.J1939});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);

      ecmInfo = SetUpECMInfoList(new[] {both});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoJ1939ToCDLTest()
    {
      var gpsDeviceID = TestData.TestMTS522.GpsDeviceID;
      const DeviceTypeEnum type = DeviceTypeEnum.Series522;

      var ecmInfo = SetUpECMInfoList(new[] {(int) DatalinkEnum.J1939});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      UpdateDatalinkIDs(gpsDeviceID, type);

      AssertECMInfo(gpsDeviceID, type, ecmInfo);

      ecmInfo = SetUpECMInfoList(new[] {(int) DatalinkEnum.CDL});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      UpdateDatalinkIDs(gpsDeviceID, type);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoThroughDataIn()
    {
      var device = TestData.TestMTS522Subscribed;
      var gpsDeviceID = device.GpsDeviceID;
      const DeviceTypeEnum type = DeviceTypeEnum.Series522;
      Entity.Asset.EquipmentVIN("EV12")
        .SerialNumberVin("SN12")
        .Name("Asset12")
        .WithDevice(device)
        .WithCoreService()
        .SyncWithRpt()
        .Save();
      AssetIDCache.Init(true);
      var timeStampUtc = DateTime.UtcNow;
      var ecmInfo = SetUpECMInfoList(new[] {(int) DatalinkEnum.J1939, (int) DatalinkEnum.J1939});
      _svc.UpdateECMInfoThroughDataIn(gpsDeviceID, type, ecmInfo, DatalinkEnum.J1939, timeStampUtc);

      AssertECMInfo(gpsDeviceID, type, ecmInfo);

      var ecmInfo1 = SetUpECMInfoList(new[] {(int) DatalinkEnum.CDL});
      _svc.UpdateECMInfoThroughDataIn(gpsDeviceID, type, ecmInfo1, DatalinkEnum.CDL, timeStampUtc.AddHours(1));
      AssertECMInfo(gpsDeviceID, type, ecmInfo.Concat(ecmInfo1).ToList());

      var ecmInfo2 = SetUpECMInfoList(new[] {(int) DatalinkEnum.J1939});
      _svc.UpdateECMInfoThroughDataIn(gpsDeviceID, type, ecmInfo, DatalinkEnum.J1939, timeStampUtc.AddHours(2));

      AssertECMInfo(gpsDeviceID, type, ecmInfo2.Concat(ecmInfo1).ToList());

      var ecmInfo3 = SetUpECMInfoList(new[] { (int)DatalinkEnum.J1939 });
      _svc.UpdateECMInfoThroughDataIn(gpsDeviceID, type, ecmInfo, DatalinkEnum.J1939, timeStampUtc.AddHours(-1));

      AssertECMInfo(gpsDeviceID, type, ecmInfo2.Concat(ecmInfo1).ToList());
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoThroughDataInUsingSaeJ1939Ecm()
    {
      var device = TestData.TestMTS521Subscribed;
      var gpsDeviceID = device.GpsDeviceID;
      const DeviceTypeEnum type = DeviceTypeEnum.Series521;
      Entity.Asset.EquipmentVIN("EV1")
        .SerialNumberVin("SN1")
        .Name("Asset1")
        .WithDevice(device)
        .WithCoreService()
        .SyncWithRpt()
        .Save();
      AssetIDCache.Init(true);
      var timeStampUtc = DateTime.UtcNow;
      var ecmInfo = SetUpECMInfoList(new[] {(int) DatalinkEnum.J1939});
      _svc.UpdateECMInfoThroughDataIn(gpsDeviceID, type, ecmInfo, DatalinkEnum.J1939, timeStampUtc);

      AssertECMInfo(gpsDeviceID, type, ecmInfo);

      var ecmInfo1 = SetUpECMInfoList(new[] {(int) DatalinkEnum.SAEJ1939});
      Assert.IsNotNull(ecmInfo1);
      var firstOrDefault = ecmInfo1.FirstOrDefault();
      Assert.IsNotNull(firstOrDefault);
      firstOrDefault.mid1 = "5000780000bb9100";
      _svc.UpdateECMInfoThroughDataIn(gpsDeviceID, type, ecmInfo1, DatalinkEnum.SAEJ1939, timeStampUtc);
      AssertMidDescriptionForSAEJ1939(gpsDeviceID, type);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoMultipleECMTest()
    {
      var gpsDeviceID = TestData.TestMTS522.GpsDeviceID;
      const int both = 3;
      const DeviceTypeEnum type = DeviceTypeEnum.Series522;

      var ecmInfo = SetUpECMInfoList(new[] {(int) DatalinkEnum.CDL, (int) DatalinkEnum.J1939, both});
      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateECMInfoPL421ECMTest()
    {
      var gpsDeviceID = TestData.TestPL421.GpsDeviceID;
      const DeviceTypeEnum type = DeviceTypeEnum.PL421;
      var ecmInfo = SetUpECMInfoList(new[] {(int) DatalinkEnum.J1939});

      _svc.UpdateECMInfo(gpsDeviceID, type, ecmInfo);
      AssertECMInfo(gpsDeviceID, type, ecmInfo);
    }

    ///// <summary>
    /////A test for SetPLWithinAmericas
    /////</summary>
    [TestMethod]
    [DatabaseTest]
    public void SetPLWithinAmericasTest()
    {
      var device = new PLDevice
      {
        GlobalgramEnabled = false,
        InAmericas = false,
        IsReadOnly = true,
        ModuleCode = "TESTPLDevice",
        SatelliteNumber = 1,
        UpdateUTC = DateTime.UtcNow
      };
      Ctx.RawContext.PLDevice.AddObject(device);
      Ctx.RawContext.SaveChanges();

      var now = DateTime.UtcNow;
      var locations = new List<DataHoursLocation>();
      var loc = new DataHoursLocation
      {
        GPSDeviceID = device.ModuleCode,
        DeviceType = DeviceTypeEnum.PL321,
        Latitude = 46.073231,
        Longitude = 2.460938,
        EventUTC = now
      };
      locations.Add(loc);

      _svc.SetPLWithinAmericas(locations);

      var inAmericas = (from p in Ctx.RawContext.PLDevice
        where p.ModuleCode == device.ModuleCode
        select p.InAmericas).FirstOrDefault();
      Assert.IsFalse(inAmericas, "The Device is not in North or South America so inamericas should be false");

      locations = new List<DataHoursLocation>();
      loc = new DataHoursLocation
      {
        GPSDeviceID = device.ModuleCode,
        DeviceType = DeviceTypeEnum.PL321,
        Latitude = 38.548165,
        Longitude = -101.601562,
        EventUTC = now
      };
      locations.Add(loc);
      _svc.SetPLWithinAmericas(locations);

      inAmericas = (from p in Ctx.RawContext.PLDevice
        where p.ModuleCode == device.ModuleCode
        select p.InAmericas).FirstOrDefault();
      Assert.IsTrue(inAmericas, "The Device is in North or South America so inamericas should be true");

      locations = new List<DataHoursLocation>();
      loc = new DataHoursLocation
      {
        GPSDeviceID = device.ModuleCode,
        DeviceType = DeviceTypeEnum.PL321,
        Latitude = 38.548165,
        Longitude = -101.601562,
        EventUTC = now
      };
      locations.Add(loc);

      var loc2 = new DataHoursLocation
      {
        GPSDeviceID = device.ModuleCode,
        DeviceType = DeviceTypeEnum.PL321,
        Latitude = 46.073231,
        Longitude = 2.460938,
        EventUTC = now.AddDays(2)
      };
      locations.Add(loc2);

      _svc.SetPLWithinAmericas(locations);

      inAmericas = (from p in Ctx.RawContext.PLDevice
        where p.ModuleCode == device.ModuleCode
        select p.InAmericas).FirstOrDefault();
      Assert.IsFalse(inAmericas,
        "The Device is not in North or South America so inamericas should be false for multiple Locations");
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateMTSDeviceConfigurationTest_AllNull_SentStatusTest()
    {
      UpdateMTSDeviceConfiguration();
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateMTSDeviceConfigurationTest_AllNotNull_SentStatusTest()
    {
      UpdateMTSDeviceConfiguration(MachineStartStatus.NormalOperation, TamperResistanceStatus.TamperResistanceLevel3);
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateMTSDeviceConfigurationTest_AllNullExceptMachineStartStatus_SentStatusTest()
    {
      UpdateMTSDeviceConfiguration(MachineStartStatus.Disabled);
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateMTSDeviceConfigurationTest_AllNullExceptTamperResistanceStatus_SentStatusTest()
    {
      UpdateMTSDeviceConfiguration(tamperResistanceStatus: TamperResistanceStatus.TamperResistanceLevel1);
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateMTSDeviceConfigurationTest_AllNull_AckStatusTest()
    {
      UpdateMTSDeviceConfiguration(messageStatusEnum: MessageStatusEnum.Acknowledged);
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateMTSDeviceConfigurationTest_AllNotNull_AckStatusTest()
    {
      UpdateMTSDeviceConfiguration(MachineStartStatus.DeratedPending, TamperResistanceStatus.TamperResistanceLevel2,
        MessageStatusEnum.Acknowledged);
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateMTSDeviceConfigurationTest_AllNullExceptMachineStartStatus_AckStatusTest()
    {
      UpdateMTSDeviceConfiguration(MachineStartStatus.Disabled,
        messageStatusEnum: MessageStatusEnum.Acknowledged);
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateMTSDeviceConfigurationTest_AllNullExceptTamperResistanceStatus_AckStatusTest()
    {
      UpdateMTSDeviceConfiguration(tamperResistanceStatus: TamperResistanceStatus.TamperResistanceLevel2,
        messageStatusEnum: MessageStatusEnum.Acknowledged);
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdatePL421SMHSourceAndRuntimeHours()
    {
      var gpsDeviceID = System.IO.Path.GetRandomFileName().Substring(0, 10).ToUpper();

      var me = TestData.CustomerAdminActiveUser;
      var session = API.Session.Validate(me.SessionID);

      var pl421 = Entity.Device.PL421.OwnerBssId(TestData.TestCustomer.BSSID).GpsDeviceId(gpsDeviceID).Save();
      var testAsset = Entity.Asset.WithDevice(pl421).Name("Test Device").SyncWithRpt().Save();

      //set source as RTerm AC 30HZ
      UpdatePL421DeviceConfiguration(PrimaryDataSourceEnum.RTERM_AC_30HZ, session, gpsDeviceID,
        MessageStatusEnum.Acknowledged);
      //set runtime adjustment
      SendRuntimeCalibration(10, gpsDeviceID, session);
      var messageID = UpdatePL421DeviceConfiguration(PrimaryDataSourceEnum.J1939, session, gpsDeviceID,
        MessageStatusEnum.Acknowledged);
      var mtsOutQuery =
        (from messages in session.NHRawContext.MTSOutReadOnly
          where (messages.SerialNumber == gpsDeviceID)
                && messages.DeviceType == (int) DeviceTypeEnum.PL421
          select messages).OrderByDescending(messages => messages.ID);

      using (var dataCtx = ObjectContextFactory.NewNHContext<INH_DATA>())
      {
        var servicemeter = new DataServiceMeterAdjustment
        {
          AssetID = testAsset.AssetID,
          RuntimeAfterHours = 10,
          RuntimeBeforeHours = 5
        };

        dataCtx.DataServiceMeterAdjustment.AddObject(servicemeter);
        dataCtx.SaveChanges();
      }

      MTSUpdateDeviceConfig.UpdateDeviceStatus(messageID, DeviceTypeEnum.PL421, DateTime.Now,
        MessageStatusEnum.Acknowledged, gpsDeviceID, mtsOutQuery.First().PacketID, mtsOutQuery.First().Payload);

      var runtimeCaliberation =
        (from runtimeCaliberations in session.NHRawContext.RuntimeCalibrationReadOnly
          where runtimeCaliberations.SerialNumber == gpsDeviceID
                && runtimeCaliberations.DeviceType == (int) DeviceTypeEnum.PL421
          select runtimeCaliberations).ToList();

      NHDataSaver.DeleteServiceMeterAdjustment(testAsset.AssetID);
      var serviceMeterAdj1 =
        (from serviceMeterAdjustment in Ctx.DataContext.DataServiceMeterAdjustment
          where serviceMeterAdjustment.AssetID == testAsset.AssetID
          select serviceMeterAdjustment).ToList();

      Assert.AreEqual(0, runtimeCaliberation.Count, "The runtime delta is not reset to zero");
      Assert.AreEqual(0, serviceMeterAdj1.Count, "The runtime delta is not reset to zero");
    }

   
    [DatabaseTest]
    [TestMethod]
    public void UpdateDeviceConfigurationThroughDataInTireMonitoringEnabledTest_Success()
    {
      var device = TestData.TestPLE641;
      var gpsDeviceID = device.GpsDeviceID;
      const DeviceTypeEnum type = DeviceTypeEnum.PLE641;
      Entity.Asset.EquipmentVIN("EV12")
        .SerialNumberVin("SN12")
        .Name("Asset12")
        .WithDevice(device)
        .WithCoreService()
        .SyncWithRpt()
        .Save();

      device.fk_DeviceStateID = 3;
      Ctx.OpContext.SaveChanges();
      AssetIDCache.Init(true);
      var tmsConfig = new A5N2ConfigData.TMSConfig
      {
        IsEnabled = true,
        MessageSourceID = 1234,
        Status = MessageStatusEnum.Acknowledged
      };
      _svc.UpdateDeviceConfiguration(gpsDeviceID, type, tmsConfig);

      var updateddevice = (from d in Ctx.OpContext.DeviceReadOnly
        where d.GpsDeviceID == gpsDeviceID
        select d).First();

      Assert.IsNotNull(updateddevice, "Device should be created properly");
      Assert.IsNotNull(updateddevice.DeviceDetailsXML, "Device Details XML should have been populated");

      var xElement = XElement.Parse(updateddevice.DeviceDetailsXML).Descendants("TMSConfig").FirstOrDefault();

      Assert.IsNotNull(xElement, "xElement should have a value");
      Assert.AreEqual("TMSConfig", xElement.Name, "configdata name is not matching");
      Assert.AreEqual("true", xElement.FirstAttribute.Value, "Value should match");
      Assert.AreEqual("1234", xElement.LastAttribute.Value, "msgSrcId should match");
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateDeviceConfigurationThroughDataInTireMonitoringDisabledTest_Success()
    {
      var device = TestData.TestPLE641;
      var gpsDeviceID = device.GpsDeviceID;
      const DeviceTypeEnum type = DeviceTypeEnum.PLE641;
      Entity.Asset.EquipmentVIN("EV12")
        .SerialNumberVin("SN12")
        .Name("Asset12")
        .WithDevice(device)
        .WithCoreService()
        .SyncWithRpt()
        .Save();

      device.fk_DeviceStateID = 3;
      Ctx.OpContext.SaveChanges();
      AssetIDCache.Init(true);
      var tmsConfig = new A5N2ConfigData.TMSConfig
      {
        IsEnabled = false,
        MessageSourceID = 1234,
        Status = MessageStatusEnum.Acknowledged
      };
      _svc.UpdateDeviceConfiguration(gpsDeviceID, type, tmsConfig);

      var updateddevice = (from d in Ctx.OpContext.DeviceReadOnly
        where d.GpsDeviceID == gpsDeviceID
        select d).First();

      Assert.IsNotNull(updateddevice, "Device should be created properly");
      Assert.IsNotNull(updateddevice.DeviceDetailsXML, "Device Details XML should have been populated");

      var xElement = XElement.Parse(updateddevice.DeviceDetailsXML).Descendants("TMSConfig").FirstOrDefault();
      Assert.IsNotNull(xElement, "xElement should have a value");
      Assert.AreEqual("TMSConfig", xElement.Name, "configdata name is not matching");
      Assert.AreEqual("false", xElement.FirstAttribute.Value, "Value should match");
      Assert.AreEqual("1234", xElement.LastAttribute.Value, "msgSrcId should match");
    }

    [TestMethod]
    public void SplitJ1939NameWorking()
    {
      bool arbitraryAddressCapable;
      byte industryGroup, vehicleSystemInstance, vehicleSystem, reserved;
      byte function, functionInstance, ecuInstance;
      ushort manufactureCode;
      int identityNumber;

      const string j1939Name = "22548578304";

      ConfigStatusSvc.GetJ1939NameSplitup(j1939Name, out arbitraryAddressCapable, out industryGroup,
        out vehicleSystemInstance, out vehicleSystem, out reserved, out function, out functionInstance, out ecuInstance,
        out manufactureCode, out identityNumber);
    }

    private void SetUpForTest()
    {
      var device522 = TestData.TestMTS522;

      // Create a service provider
      var serviceProviderID = CreateServiceProvider("MTS500fw1_", 111);

      // Create two MTS500FirmwareVersions
      var firmwareVersion1 = API.Firmware.Create(Ctx.OpContext, serviceProviderID, "firmwareVersion1", "Some folder1",
        "notes notes notes");
      if (firmwareVersion1 == 0)
      {
        var firmware = (from f in Ctx.OpContext.MTS500FirmwareVersion
                        where f.ID == 0
                        select f).FirstOrDefault();
        Assert.IsNotNull(firmware);
        firmware.ID = 225;
        firmwareVersion1 = 225;
      }
      Assert.IsTrue(firmwareVersion1 > 0, "Failed to create firmwareVersion1 record");

      serviceProviderID = CreateServiceProvider("MTS500fw2_", 112);
      var firmwareVersion2 = API.Firmware.Create(Ctx.OpContext, serviceProviderID, "firmwareVersion2", "Some folder2",
        "notes notes notes");
      if (firmwareVersion2 == 0)
      {
        var firmware = (from f in Ctx.OpContext.MTS500FirmwareVersion
                        where f.ID == 0
                        select f).FirstOrDefault();
        Assert.IsNotNull(firmware);
        firmware.ID = 226;
        firmwareVersion2 = 226;
      }

      Assert.IsTrue(firmwareVersion2 > 0, "Failed to create firmwareVersion2 record");

      // Create the DeviceFirmwareVersion
      var fwVersion = new DeviceFirmwareVersion
      {
        ID = 0,
        UpdateStatusUTC = DateTime.UtcNow,
        fk_DeviceID = device522.ID,
        fk_FirmwareUpdateStatusID = (int)FirmwareUpdateStatusEnum.NeverUpdated,
        fk_MTS500FirmwareVersionIDInstalled = firmwareVersion1,
        fk_MTS500FirmwareVersionIDPending = firmwareVersion2,
        CurrentFirmwareReport = "<tag>data</tag>"
      };
      Ctx.OpContext.DeviceFirmwareVersion.AddObject(fwVersion);
      var result = Ctx.OpContext.SaveChanges();
      if (result < 0)
        throw new InvalidOperationException("Error creating Firmware Version");

      // Create a second device that will not be placed in the DeviceFirmwareVersion table.
    }

    private long CreateServiceProvider(string mts500ServerPrefix, int id)
    {
      // Create a service provider
      var fwareSvr = new ServiceProvider
      {
        ID = id,
        ProviderName = string.Format("{0}UNITTEST", mts500ServerPrefix),
        ServerIPAddress = "1.2.3.4",
        UpdateUTC = DateTime.UtcNow,
        UserName = "Test",
        Password = "Pass"
      };
      Ctx.OpContext.ServiceProvider.AddObject(fwareSvr);
      Ctx.OpContext.SaveChanges();

      var serviceProviderID = (from sp in Ctx.OpContext.ServiceProviderReadOnly
                               where sp.ProviderName.StartsWith(mts500ServerPrefix)
                               select sp.ID).FirstOrDefault<long>();
      Assert.IsTrue(serviceProviderID > 0, "Invalid service provider ID");
      return serviceProviderID;
    }

    private static void AssertMidDescriptionForSAEJ1939(string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var mid = (from e in ctx.ECMInfo
                   from device in ctx.Device
                   from m in ctx.ECMDatalinkInfo
                   where e.fk_DeviceID == device.ID && device.GpsDeviceID == gpsDeviceID
                         && device.fk_DeviceTypeID == (int)deviceType && m.fk_ECMInfoID == e.ID
                         && m.fk_DatalinkID == (int)DatalinkEnum.SAEJ1939
                   select m.fk_MIDID).ToList();
        foreach (var i in mid)
        {
          var midDesc = (from midDs in ctx.MIDDesc
                         where midDs.fk_MIDID == i
                         select midDs).ToList();
          Assert.IsNotNull(midDesc);
          var firstOrDefault = midDesc.FirstOrDefault();
          Assert.IsNotNull(firstOrDefault);
          Assert.AreEqual(firstOrDefault.Description, "Function: 120", "Description not correct");
        }
      }
    }

    private static void AssertECMInfo(string gpsDeviceID, DeviceTypeEnum deviceType, List<MTSEcmInfo> expectedECMList,
      int? count = null)
    {
      using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var deviceTypeID = (int)deviceType;
        var ecmInfoList = (from e in ctx.ECMInfo
                           from device in ctx.Device
                           where e.fk_DeviceID == device.ID && device.GpsDeviceID == gpsDeviceID
                                 && device.fk_DeviceTypeID == deviceTypeID
                           orderby e.ID
                           select e).ToList();
        Assert.AreEqual(count.HasValue ? count.Value : expectedECMList.Count(), ecmInfoList.Count,
          "Wrong number of ECMInfoRows");

        for (var i = 0; i < ecmInfoList.Count(); i++)
        {
          Assert.AreEqual(expectedECMList[i].actingMasterECM, ecmInfoList[i].IsSyncClockMaster,
            string.Format("Incorrect IsSyncClockMaster at index {0}", i));
          Assert.AreEqual(expectedECMList[i].syncSMUClockSupported, ecmInfoList[i].HasSMUClock,
            string.Format("Incorrect HasSMUClock at index {0}", i));
          Assert.AreEqual(expectedECMList[i].engineSerialNumbers[0], ecmInfoList[i].Engine1SN,
            string.Format("Incorrect EngineSerialNumber 1 at index {0}", i));
          Assert.AreEqual(
            expectedECMList[i].engineSerialNumbers.Count() == 1 ? null : expectedECMList[i].engineSerialNumbers[1],
            ecmInfoList[i].Engine2SN, string.Format("Incorrect EngineSerialNumber 2 at index {0}", i));
          Assert.AreEqual(expectedECMList[i].transmissionSerialNumbers[0], ecmInfoList[i].Transmission1SN,
            string.Format("Incorrect Transmission1SN at index {0}", i));
          Assert.AreEqual(
            expectedECMList[i].transmissionSerialNumbers.Count() == 1
              ? null
              : expectedECMList[i].transmissionSerialNumbers[1], ecmInfoList[i].Transmission2SN,
            string.Format("Incorrect Transmission2SN at index {0}", i));
          Assert.AreEqual(expectedECMList[i].eventProtocolVersion == 1, ecmInfoList[i].EventProtocolVer,
            string.Format("Incorrect EventProtocolVer at index {0}", i));
          Assert.AreEqual(expectedECMList[i].diagnosticProtocolVersion == 1, ecmInfoList[i].DiagnosticProtocolVer,
            string.Format("Incorrect DiagnosticProtocolVer at index {0}", i));
          Assert.AreEqual(expectedECMList[i].softwarePartNumber, ecmInfoList[i].SoftwarePartNumber,
            string.Format("Incorrect SoftwarePartNumber at index {0}", i));
          Assert.AreEqual(expectedECMList[i].serialNumber, ecmInfoList[i].SerialNumber,
            string.Format("Incorrect SerialNumber at index {0}", i));

          AssertECMDataLink(i, expectedECMList[i].datalink, ecmInfoList[i], expectedECMList[i]);
        }
      }
    }

    private static void AssertECMDataLink(int index, int datalinkType, ECMInfo actualECMInfo, MTSEcmInfo expectedECMInfo)
    {
      Assert.IsNotNull(actualECMInfo);
      Assert.IsNotNull(expectedECMInfo);
      Assert.IsNotNull(actualECMInfo.ECMDatalinkInfo);

      switch (datalinkType)
      {
        case 0:
          Assert.AreEqual(0, actualECMInfo.ECMDatalinkInfo.Count(),
            string.Format("Incorrect number of ECMDataLinkInfo at index {0}", index));
          break;
        case 1:
          var info1 = actualECMInfo.ECMDatalinkInfo.FirstOrDefault();
          Assert.IsNotNull(info1);
          Assert.AreEqual(1, actualECMInfo.ECMDatalinkInfo.Count(),
            string.Format("Incorrect number of ECMDataLinkInfo at index {0}", index));
          Assert.AreEqual(datalinkType, info1.fk_DatalinkID,
            string.Format("Incorrect DataLinkID at index {0}", index));
          Assert.AreEqual((short)expectedECMInfo.toolSupportChangeLevel1,
            info1.SvcToolSupportChangeLevel,
            string.Format("Incorrect SvcToolSupportChangeLevel at index {0}", index));
          Assert.AreEqual((short)expectedECMInfo.applicationLevel1,
            info1.ApplicationLevel,
            string.Format("Incorrect ApplicationLevel at index {0}", index));
          Assert.AreEqual(expectedECMInfo.mid1, info1.MID.MID1,
            string.Format("Incorrect mid at index {0}", index));
          break;
        case 2:
          var info2 = actualECMInfo.ECMDatalinkInfo.FirstOrDefault();
          Assert.IsNotNull(info2);
          Assert.AreEqual(1, actualECMInfo.ECMDatalinkInfo.Count(),
            string.Format("Incorrect number of ECMDataLinkInfo at index {0}", index));
          Assert.AreEqual(datalinkType, info2.fk_DatalinkID,
            string.Format("Incorrect DataLinkID at index {0}", index));
          Assert.AreEqual((short)expectedECMInfo.toolSupportChangeLevel1,
            info2.SvcToolSupportChangeLevel,
            string.Format("Incorrect SvcToolSupportChangeLevel at index {0}", index));
          Assert.AreEqual((short)expectedECMInfo.applicationLevel1,
            info2.ApplicationLevel,
            string.Format("Incorrect ApplicationLevel at index {0}", index));
          Assert.AreEqual(expectedECMInfo.mid1, info2.MID.MID1,
            string.Format("Incorrect mid at index {0}", index));
          break;
        case 3:
          {
            var info3 = actualECMInfo.ECMDatalinkInfo.FirstOrDefault();
            Assert.IsNotNull(info3);
            var ecmDataLinkInfo = (from e in actualECMInfo.ECMDatalinkInfo
                                   orderby e.fk_DatalinkID
                                   select e);
            Assert.AreEqual(2, actualECMInfo.ECMDatalinkInfo.Count(),
              string.Format("Incorrect number of ECMDataLinkInfo at index {0}", index));
            Assert.AreEqual(1, info3.fk_DatalinkID,
              string.Format("Incorrect DataLinkID 1 at index {0}", index));
            Assert.AreEqual((short)expectedECMInfo.toolSupportChangeLevel1,
              info3.SvcToolSupportChangeLevel,
              string.Format("Incorrect SvcToolSupportChangeLevel 1 at index {0}", index));
            Assert.AreEqual((short)expectedECMInfo.applicationLevel1, info3.ApplicationLevel,
              string.Format("Incorrect ApplicationLevel 1 at index {0}", index));
            Assert.AreEqual(expectedECMInfo.mid1, info3.MID.MID1,
              string.Format("Incorrect mid 1 at index {0}", index));

            Assert.AreEqual(2, ecmDataLinkInfo.Last().fk_DatalinkID,
              string.Format("Incorrect DataLinkID at index {0}", index));
            Assert.IsTrue(expectedECMInfo.toolSupportChangeLevel2 != null);
            Assert.AreEqual((short)expectedECMInfo.toolSupportChangeLevel2,
              ecmDataLinkInfo.Last().SvcToolSupportChangeLevel,
              string.Format("Incorrect SvcToolSupportChangeLevel 2 at index {0}", index));
            Assert.IsTrue(expectedECMInfo.applicationLevel2 != null);
            Assert.AreEqual((short)expectedECMInfo.applicationLevel2, ecmDataLinkInfo.Last().ApplicationLevel,
              string.Format("Incorrect ApplicationLevel 2 at index {0}", index));
            Assert.AreEqual(expectedECMInfo.mid2.ToString(), ecmDataLinkInfo.Last().MID.MID1,
              string.Format("Incorrect mid 2 at index {0}", index));
          }
          break;
        case 4:
          {
            var ecmDataLinkInfo = (from e in actualECMInfo.ECMDatalinkInfo
                                   orderby e.fk_DatalinkID
                                   select e).FirstOrDefault();
            Assert.IsNotNull(ecmDataLinkInfo);
            Assert.AreEqual(1, actualECMInfo.ECMDatalinkInfo.Count(),
              string.Format("Incorrect number of ECMDataLinkInfo at index {0}", index));
            Assert.AreEqual(4, ecmDataLinkInfo.fk_DatalinkID, string.Format("Incorrect DataLinkID 1 at index {0}", index));
            Assert.AreEqual(expectedECMInfo.J1939Name, ecmDataLinkInfo.MID.MID1,
              string.Format("Incorrect mid 1 at index {0}", index));
            var firstOrDefault = ecmDataLinkInfo.MID.MIDDesc.FirstOrDefault();
            Assert.IsNotNull(firstOrDefault);
            Assert.AreEqual(ecmDataLinkInfo.MID.ID, firstOrDefault.fk_MIDID,
              "There is no corresponding row in MIDDesc table");
          }
          break;
      }
    }

    private static List<MTSEcmInfo> SetUpECMInfoList(IEnumerable<int> datalinkType)
    {
      var mtsList = new List<MTSEcmInfo>();
      foreach (var linkType in datalinkType)
      {
        if (linkType == 1)
        {
          var mts = new MTSEcmInfo
          {
            actingMasterECM = true,
            applicationLevel1 = 15,
            datalink = 1,
            diagnosticProtocolVersion = 1,
            engineSerialNumbers = new[] { "12345", "3456" },
            transmissionSerialNumbers = new[] { "54321" },
            eventProtocolVersion = 1,
            mid1 = "1",
            serialNumber = "##########",
            softwarePartNumber = "87695403",
            syncSMUClockSupported = false,
            toolSupportChangeLevel1 = 27,
            toolSupportChangeLevel2 = 45
          };
          mtsList.Add(mts);
        }
        else if (linkType == 2)
        {
          var mts = new MTSEcmInfo
          {
            actingMasterECM = false,
            datalink = 2,
            diagnosticProtocolVersion = 0,
            engineSerialNumbers = new[] { "47568", "5869" },
            transmissionSerialNumbers = new[] { "86574", "5869" },
            eventProtocolVersion = 0,
            SourceAddress = 123,
            mid1 = "2",
            applicationLevel2 = 27,
            toolSupportChangeLevel2 = 18,
            serialNumber = "ABCDEFG",
            softwarePartNumber = "123456",
            syncSMUClockSupported = true
          };

          mts.SourceAddress = 123;
          mts.J1939Name = "5000780000bb9100";
          mts.ArbitraryAddressCapable = false;
          mts.IndustryGroup = 5;
          mts.VehicleSystem = 0;
          mts.ECUInstance = 0;
          mts.VehicleSystemInstance = 0;
          mts.Function = 120;
          mts.FunctionInstance = 0;
          mts.ManufacturerCode = 5;
          mts.IdentityNumber = 1806592;
          mtsList.Add(mts);
        }
        else if (linkType == 3 || linkType == 7)
        {
          var mts = new MTSEcmInfo
          {
            actingMasterECM = true,
            applicationLevel1 = 15,
            datalink = (byte)linkType,
            diagnosticProtocolVersion = 1,
            engineSerialNumbers = new[] { "12345", "3456" },
            transmissionSerialNumbers = new[] { "54321" },
            eventProtocolVersion = 1,
            mid1 = "1",
            serialNumber = "##########",
            softwarePartNumber = "87695403",
            syncSMUClockSupported = true,
            toolSupportChangeLevel1 = 27,
            mid2 = 2,
            toolSupportChangeLevel2 = 18,
            applicationLevel2 = 25,
            J1939Name = "5000780000bb9100",
            ArbitraryAddressCapable = false,
            IndustryGroup = 5,
            VehicleSystem = 0,
            VehicleSystemInstance = 0,
            Function = 120,
            ECUInstance = 0,
            FunctionInstance = 0,
            ManufacturerCode = 5,
            IdentityNumber = 1806592
          };
          mtsList.Add(mts);
        }
        else if (linkType == 4)
        {
          var mts = new MTSEcmInfo
          {
            actingMasterECM = true,
            applicationLevel1 = 15,
            datalink = 4,
            diagnosticProtocolVersion = 0,
            engineSerialNumbers = new[] { "47568", "5869" },
            transmissionSerialNumbers = new[] { "86574", "5869" },
            eventProtocolVersion = 0,
            SourceAddress = 123,
            serialNumber = "ABCDEFG",
            softwarePartNumber = "123456",
            syncSMUClockSupported = true
          };
          mts.SourceAddress = 123;
          mts.J1939Name = "5000780000bb9100";
          mts.ArbitraryAddressCapable = false;
          mts.IndustryGroup = 5;
          mts.VehicleSystem = 0;
          mts.VehicleSystemInstance = 0;
          mts.Function = 120;
          mts.ECUInstance = 0;
          mts.FunctionInstance = 0;
          mts.ManufacturerCode = 5;
          mts.IdentityNumber = 1806592;
          mtsList.Add(mts);

        }
        else
        {
          var mts = new MTSEcmInfo
          {
            actingMasterECM = true,
            applicationLevel1 = 15,
            datalink = (byte)linkType,
            diagnosticProtocolVersion = 1,
            engineSerialNumbers = new[] { "12345", "3456" },
            transmissionSerialNumbers = new[] { "54321" },
            eventProtocolVersion = 1,
            mid1 = "123",
            serialNumber = "##########",
            softwarePartNumber = "87695403",
            syncSMUClockSupported = false,
            toolSupportChangeLevel1 = 27,
            toolSupportChangeLevel2 = 45,
            SourceAddress = 2,
            J1939Name = "5000780000bb9100",
            ArbitraryAddressCapable = false,
            IndustryGroup = 5,
            VehicleSystem = 0,
            VehicleSystemInstance = 0,
            Function = 120,
            FunctionInstance = 0,
            ECUInstance = 0,
            ManufacturerCode = 5,
            IdentityNumber = 1806592
          };
          mtsList.Add(mts);
        }
      }
      return mtsList;
    }

    private void UpdateDatalinkIDs(string gpsDeviceID, DeviceTypeEnum type)
    {
      var ecm = (from e in Ctx.OpContext.ECMInfo
                 from d in Ctx.OpContext.Device
                 where d.GpsDeviceID == gpsDeviceID && d.fk_DeviceTypeID == (int)type
                       && e.fk_DeviceID == d.ID
                 select e).ToList();
      var count = 1;
      foreach (var e in ecm)
      {
        if (e.ID <= 0)
        {
          e.ID = count;
          count++;
        }
        foreach (var d in e.ECMDatalinkInfo)
        {
          if (d.ID <= 0)
          {
            d.ID = count;
            count++;
          }
        }
      }
    }

    private void UpdateMTSDeviceConfiguration(MachineStartStatus? machineStartStatus = null,
      TamperResistanceStatus? tamperResistanceStatus = null,
      MessageStatusEnum messageStatusEnum = MessageStatusEnum.Sent)
    {
      var gpsDeviceID = System.IO.Path.GetRandomFileName().Substring(0, 10).ToUpper();

      var me = TestData.CustomerAdminActiveUser;
      var session = API.Session.Validate(me.SessionID);

      SendMachineSecurityInformation(TestData.TestCustomer, gpsDeviceID, machineStartStatus, tamperResistanceStatus,
        session);

      var mtsOutMsg = (from messages in session.NHRawContext.MTSOutReadOnly
                       where (messages.SerialNumber == gpsDeviceID)
                             && messages.DeviceType == (int)DeviceTypeEnum.Series522
                       select messages).FirstOrDefault();
      Assert.IsNotNull(mtsOutMsg);
      var msg = PlatformMessage.HydratePlatformMessage(mtsOutMsg.Payload, true, false) as UserDataBaseMessage;
      Assert.IsNotNull(msg, "Message should be an MachineEventMessage");
      var machineSecuritySystemInformation = msg.Message as ConfigureGatewayMessage;
      Assert.IsNotNull(machineSecuritySystemInformation);
      var machineStartStatusConfig =
        machineSecuritySystemInformation.GetMachineSecuritySystemConfig(mtsOutMsg.ID, mtsOutMsg.SentUTC,
          messageStatusEnum);
      if (machineStartStatusConfig != null)
      {
        if (messageStatusEnum == MessageStatusEnum.Acknowledged)
          machineStartStatusConfig.packetID = 83;

        _svc.UpdateDeviceConfiguration(gpsDeviceID, DeviceTypeEnum.Series522, machineStartStatusConfig);
      }

      var device = (from d in session.NHOpContext.DeviceReadOnly
                    where d.GpsDeviceID == gpsDeviceID
                    select d).First();

      Assert.IsNotNull(device, "Device should be created properly");
      Assert.IsNotNull(device.DeviceDetailsXML, "Device Details XML should have been populated");
      var xElements = XElement.Parse(device.DeviceDetailsXML).Descendants("MachineSecuritySystemConfig");

      if (machineStartStatus.HasValue && tamperResistanceStatus.HasValue)
      {
        Assert.AreEqual(xElements.Count(), messageStatusEnum == MessageStatusEnum.Acknowledged ? 2 : 1,
          "There should be only one MachineSecuritySystemConfig node for each device");
      }

      var data = new MTSConfigData(device.DeviceDetailsXML);

      if (machineStartStatus != null)
      {
        Assert.IsTrue(data.pendingMachineSecuritySystemInformationConfig.machineStartStatus != null);
        if (messageStatusEnum == MessageStatusEnum.Acknowledged)
          Assert.AreEqual((int)data.pendingMachineSecuritySystemInformationConfig.machineStartStatus,
            (int)machineStartStatus, "Machine Start Status fields should be equal");
        else
          Assert.AreEqual((int)data.pendingMachineSecuritySystemInformationConfig.machineStartStatus,
            (int)machineStartStatus, "Machine Start Status fields should be equal");
      }

      if (tamperResistanceStatus != null)
      {
        if (messageStatusEnum == MessageStatusEnum.Acknowledged)
        {
          Assert.IsTrue(data.currentMachineSecuritySystemInformationConfig.tamperResistanceStatus != null);
          Assert.AreEqual((int)data.currentMachineSecuritySystemInformationConfig.tamperResistanceStatus,
            (int)tamperResistanceStatus, "Machine Start Status fields should be equal");
        }
        else
        {
          Assert.IsTrue(data.pendingMachineSecuritySystemInformationConfig.tamperResistanceStatus != null);
          Assert.AreEqual((int)data.pendingMachineSecuritySystemInformationConfig.tamperResistanceStatus,
            (int)tamperResistanceStatus, "Machine Start Status fields should be equal");
        }
      }
    }

    private static void SendMachineSecurityInformation(Customer customer, string gpsDeviceID,
      MachineStartStatus? machineStartStatus, TamperResistanceStatus? tamperResistanceStatus, SessionContext session)
    {
      var mts522 = Entity.Device.MTS522.OwnerBssId(customer.BSSID).GpsDeviceId(gpsDeviceID).Save();
      Entity.Asset.WithDevice(mts522).Name("Test Device").SyncWithRpt().Save();

      var device = (from d in session.NHOpContext.Device
                    where d.GpsDeviceID == gpsDeviceID
                    select d).First();

      device.DeviceDetailsXML = @"<PLConfigData><Pending /><Current /></PLConfigData>";

      session.NHOpContext.SaveChanges();
      API.MTSOutbound.SendMachineSecuritySystemInformationMessage(session.NHOpContext, 
        new[] { gpsDeviceID }, DeviceTypeEnum.Series522, machineStartStatus, tamperResistanceStatus);

      var mtsOutQuery =
        (from messages in session.NHRawContext.MTSOutReadOnly
         where (messages.SerialNumber == gpsDeviceID)
               && messages.DeviceType == (int)DeviceTypeEnum.Series522
         select messages);
      var messageCount = mtsOutQuery.Count();
      Assert.AreEqual(1, messageCount,
        "there should have been MTSOut record created for Machine Security System Information");

      var msg = PlatformMessage.HydratePlatformMessage(mtsOutQuery.First().Payload, true, false) as UserDataBaseMessage;
      Assert.IsNotNull(msg);
      var machineSecuritySystemInformation = msg.Message as ConfigureGatewayMessage;
      Assert.IsNotNull(machineSecuritySystemInformation);
      Assert.AreEqual(machineSecuritySystemInformation.TransactionType, 0x11);
      Assert.AreEqual(machineSecuritySystemInformation.TransactionSubType, 0x02);
      Assert.AreEqual(machineSecuritySystemInformation.TransactionVersion, 0x01);
      Assert.AreEqual(machineSecuritySystemInformation.PacketID, 0x1A);
    }

    private int UpdatePL421DeviceConfiguration(PrimaryDataSourceEnum primaryDataSource, SessionContext session,
      string gpsDeviceID, MessageStatusEnum messageStatusEnum = MessageStatusEnum.Sent)
    {
      int msgID;
      //USE RTerm
      SendMachineEventHeaderInformation(primaryDataSource, gpsDeviceID, out msgID, session);
      var mtsOutMsg =
        (from messages in session.NHRawContext.MTSOutReadOnly
         where (messages.SerialNumber == gpsDeviceID)
               && messages.DeviceType == (int)DeviceTypeEnum.PL421
         select messages).FirstOrDefault();
      Assert.IsNotNull(mtsOutMsg);
      var machineEventHeader = new DeviceConfigurationBaseUserDataMessage.ConfigureMachineEventHeader();
      var machineStartStatusConfig =
        machineEventHeader.GetConfig(mtsOutMsg.ID, mtsOutMsg.SentUTC, messageStatusEnum);
      if (machineStartStatusConfig != null)
      {
        _svc.UpdateDeviceConfiguration(gpsDeviceID, DeviceTypeEnum.Series522, machineStartStatusConfig);
      }

      return msgID;
    }

    private static void SendMachineEventHeaderInformation(PrimaryDataSourceEnum primaryDataSource, string gpsDeviceID,
      out int msgID, SessionContext session)
    {
      var device = (from d in session.NHOpContext.Device
                    where d.GpsDeviceID == gpsDeviceID
                    select d).First();

      device.DeviceDetailsXML = @"<PLConfigData><Pending /><Current /></PLConfigData>";

      session.NHOpContext.SaveChanges();

      API.MTSOutbound.SetMachineEventHeaderConfiguration(session.NHOpContext, 
        new[] { gpsDeviceID }, primaryDataSource, DeviceTypeEnum.PL421);

      var mtsOutQuery =
        (from messages in session.NHRawContext.MTSOutReadOnly
         where (messages.SerialNumber == gpsDeviceID)
               && messages.DeviceType == (int)DeviceTypeEnum.PL421
         select messages).OrderByDescending(messages => messages.ID);
      msgID = (int)mtsOutQuery.First().ID;
    }

    private static void SendRuntimeCalibration(long runtime, string gpsDeviceID, SessionContext session)
    {
      var device = (from d in session.NHOpContext.Device
                    where d.GpsDeviceID == gpsDeviceID
                    select d).First();

      device.DeviceDetailsXML = @"<PLConfigData><Pending /><Current /></PLConfigData>";

      session.NHOpContext.SaveChanges();

      API.MTSOutbound.CalibrateDeviceRuntime(session.NHOpContext, new[] { gpsDeviceID }, DeviceTypeEnum.PL421, runtime);
    }
  }
}