using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass()]
  public class ReportLogicTestPL : ReportLogicTestBase 
  {

    [DatabaseTest]
    [TestMethod]
    public void SimpleMeterDelta()
    {
      // PL121 uses the MeterDelta method. In this scenario there are engineParameter events including idle info.
      //   DeviceDate          Runtime  IdleTime IdleFuel TotalFuel ExpectedRT   Runtime   Idle    Working  Running   Working  Efficiency% 
      //                       Meter    Meter    Meter    Meter     Hours        Hours     Hours   Hours    Utilzn    Utilzn 
      //   5 Dec 2010 08:00    1000     900      1200     16000     0(Sunday)     
      //   6 Dec 2010 08:00    1010     903      1214     16030     8            10        3       7        125       87.5     70  
      //   7 Dec 2010 08:00    1026     913      1250     16080     8            16        10      6        200       75       37.5   
      //   8 Dec 2010 08:00    1032     915      1260     16100     8            6         2       4        75        50       66.66667   

      var target = new EquipmentAPI(); 
      var customer = TestData.TestCustomer;
			var user = TestData.TestCustomerUser;
			var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.PL121.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                 .WithCoreService()
                 .Save();

      Helpers.NHRpt.DimTables_Populate();
      var dimAsset = (from da in Ctx.RptContext.DimAssetReadOnly
                      where da.AssetID == asset.AssetID
                      select da).FirstOrDefault();
      Assert.IsNotNull(dimAsset, "DimAsset not created in NH_RPT");

      List<DimAssetExpectedRuntimeHours> dimAssetExpectedRuntimeHourses  = (from dau in Ctx.RptContext.DimAssetExpectedRuntimeHoursReadOnly
                          where dau.ifk_DimAssetID == asset.AssetID
                          select dau).ToList<DimAssetExpectedRuntimeHours>();
      Assert.IsNotNull(dimAssetExpectedRuntimeHourses, "DimAssetExpectedRuntimeHours not created in NH_RPT");
      
      
      // create events in NH_DATA
      long? runtimeHours = 1000;
      double? latitude = 53.756058;
      double? longitude = 8.021608;
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Sunday - DateTime.UtcNow.DayOfWeek).AddDays(-14)).Date;
      DateTime? eventUTC = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day, 8, 0, 0);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      double engineIdleHours = 900;
      double idleFuelGallons = 1200;
      double consumptionGallons = 16000;
      double percentRemaining = 10;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, consumptionGallons, engineIdleHours, idleFuelGallons, percentRemaining );

      runtimeHours = 1010;
      eventUTC = dateSetup.AddDays(1);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 903;
      idleFuelGallons= 1214;
      consumptionGallons = 16030;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, consumptionGallons, engineIdleHours, idleFuelGallons, percentRemaining);

      runtimeHours = 1026;
      eventUTC = dateSetup.AddDays(2);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 913;
      idleFuelGallons= 1250;
      consumptionGallons = 16080;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, consumptionGallons, engineIdleHours, idleFuelGallons, percentRemaining);

      runtimeHours = 1032;
      eventUTC = dateSetup.AddDays(3);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 915;
      idleFuelGallons= 1260;
      consumptionGallons = 16100;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, consumptionGallons, engineIdleHours, idleFuelGallons, percentRemaining);
   

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      List<HoursLocation> hl = (from rhl in Ctx.RptContext.HoursLocationReadOnly
                                where rhl.ifk_DimAssetID == asset.AssetID
                                orderby rhl.fk_AssetKeyDate
                                select rhl).ToList<HoursLocation>();
      Assert.AreEqual(4, hl.Count(), "Incorrect HoursLocation record count.");

      List<EngineParameters> ep = (from rep in Ctx.RptContext.EngineParametersReadOnly
                                   where rep.ifk_DimAssetID == asset.AssetID
                                   orderby rep.fk_AssetKeyDate
                                   select rep).ToList<EngineParameters>();
      Assert.AreEqual(4, ep.Count(), "Incorrect EngineParameters record count.");

      ExecuteMeterDeltaTransformScript();
      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                            && aud.fk_AssetPriorKeyDate != null
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(3, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(dateSetup.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("Date wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(10, util[0].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(3, util[0].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(7, util[0].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[0].fk_AssetKeyDate));

      Assert.AreEqual(dateSetup.AddDays(2).KeyDate(), util[1].fk_AssetKeyDate, string.Format("Date wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(16, util[1].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(10, util[1].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(6, util[1].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[1].fk_AssetKeyDate));

      Assert.AreEqual(dateSetup.AddDays(3).KeyDate(), util[2].fk_AssetKeyDate, string.Format("Date wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(6, util[2].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(2, util[2].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(4, util[2].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
}

    [DatabaseTest]
    [Ignore]
    [TestMethod]
    public void TimeZoneCheck()
    {
      // check that the timezoneID is correct in HoursLocation, and that that ID is carried over into EngineParameters row
      //   DeviceDate                  event  timezone  eventUTC                  EventDeviceTime             AssetKeyDate 
      //                               type    
      //   5 [LastMonth] YYYY 08:00    hl     EST       5 [LastMonth] YYYY 03:00  5 [LastMonth] YYYY 03:00    YYYY1205         TzID=GMT (Africa)
      //                               ep     EST       5 [LastMonth] YYYY 03:00  5 [LastMonth] YYYY 03:00    YYYY1205         
      //   6 [LastMonth] YYYY 08:00    ep     EST       6 [LastMonth] YYYY 03:00  6 [LastMonth] YYYY 03:00    YYYY1206 
      //
      //   8 [LastMonth] YYYY 08:00    hl     GMT       8 [LastMonth] YYYY 03:00  7 [LastMonth] YYYY 22/23:00    YYYY1207         TzID=EST (Canada)
      //                               ep     GMT       8 [LastMonth] YYYY 03:00  7 [LastMonth] YYYY 22/23:00    YYYY1207
      //   9 [LastMonth] YYYY 08:00    ep     GMT       9 [LastMonth] YYYY 01:00  8 [LastMonth] YYYY 20/21:00    YYYY1208

      var target = new EquipmentAPI();
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.PL121.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                 .WithCoreService().Save();
      Helpers.NHRpt.DimTables_Populate();
 
      // create events in NH_DATA
      int year = DateTime.UtcNow.Year;
      int month = DateTime.UtcNow.Month;
      long? runtimeHours = 1000;
      double? latitude = 20; // Africa/Nouakchott, a GMT time zone, with no Daylight Saving. ie utc == local.
      double? longitude = -8.021608;
      DateTime testStartEventUTC = new DateTime(year, month, 05, 03, 00, 00);
      testStartEventUTC = testStartEventUTC.AddMonths(-1);
      DateTime? eventUTC = testStartEventUTC;

      // first location, on the 5th of the month, is within a GMT timezone.
      DataHoursLocation hl1 = Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      double engineIdleHours = 900;
      double idleFuelGallons = 1200;
      double consumptionGallons = 16000;
      double percentRemaining = 10;
      DataEngineParameters ep1a = Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, consumptionGallons, engineIdleHours, idleFuelGallons, percentRemaining);

      engineIdleHours = 903;
      idleFuelGallons = 1214;
      consumptionGallons = 16030;
      eventUTC = testStartEventUTC.AddDays(1);
      DataEngineParameters ep1b = Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, consumptionGallons, engineIdleHours, idleFuelGallons, percentRemaining);

      // Next location is in America/Montreal, EST, Daylight saving starts 2nd Sunday in March, ends 1st Sunday in November. bias=-300mins, daylightBias=60mins
      runtimeHours = 1026;
      latitude = 52; // EST, Montreal.
      longitude = -78;
      eventUTC = testStartEventUTC.AddDays(3);
      NamedTimeZone EST = new NamedTimeZone("Eastern Standard Time");
      // Next location occurs on the 8th of the month.
      DataHoursLocation hl2 = Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 913;
      idleFuelGallons = 1250;
      consumptionGallons = 16080;
      DataEngineParameters ep2a = Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, consumptionGallons, engineIdleHours, idleFuelGallons, percentRemaining);

      engineIdleHours = 915;
      idleFuelGallons = 1260;
      consumptionGallons = 16100;
      eventUTC = testStartEventUTC.AddDays(4).AddHours(-2);
      DataEngineParameters ep2b = Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, consumptionGallons, engineIdleHours, idleFuelGallons, percentRemaining);


      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      List<HoursLocation> hl = (from rhl in Ctx.RptContext.HoursLocationReadOnly
                                where rhl.ifk_DimAssetID == asset.AssetID
                                orderby rhl.fk_AssetKeyDate
                                select rhl).ToList<HoursLocation>();
      Assert.AreEqual(2, hl.Count(), "Incorrect HoursLocation record count.");

      List<EngineParameters> ep = (from rep in Ctx.RptContext.EngineParametersReadOnly
                                   where rep.ifk_DimAssetID == asset.AssetID
                                   orderby rep.fk_AssetKeyDate
                                   select rep).ToList<EngineParameters>();
      Assert.AreEqual(4, ep.Count(), "Incorrect EngineParameters record count.");

      Assert.AreEqual(testStartEventUTC.KeyDate(), hl[0].fk_AssetKeyDate, string.Format("Date wrong for Day:{0}", hl[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartEventUTC, hl[0].EventUTC, string.Format("EventUTC wrong for Day:{0}", hl[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartEventUTC, hl[0].EventDeviceTime, string.Format("EventDeviceTime wrong for Day:{0}", hl[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartEventUTC.KeyDate(), ep[0].fk_AssetKeyDate, string.Format("Date wrong for Day:{0}", ep[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartEventUTC, ep[0].EventUTC, string.Format("EventUTC wrong for Day:{0}", ep[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartEventUTC, ep[0].EventDeviceTime, string.Format("EventDeviceTime wrong for Day:{0}", ep[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartEventUTC.AddDays(1).KeyDate(), ep[1].fk_AssetKeyDate, string.Format("Date wrong for Day:{0}", ep[1].fk_AssetKeyDate));
      Assert.AreEqual(testStartEventUTC.AddDays(1), ep[1].EventUTC, string.Format("EventUTC wrong for Day:{0}", ep[1].fk_AssetKeyDate));
      Assert.AreEqual(testStartEventUTC.AddDays(1), ep[1].EventDeviceTime, string.Format("EventDeviceTime wrong for Day:{0}", ep[1].fk_AssetKeyDate));

      DateTime controlHLLocal = EST.ToLocalTime(hl2.EventUTC);
      DateTime controlEPaLocal = EST.ToLocalTime(ep2a.EventUTC);
      DateTime controlEPbLocal = EST.ToLocalTime(ep2b.EventUTC);

      Assert.AreEqual(controlHLLocal.KeyDate(), hl[1].fk_AssetKeyDate, string.Format("Date wrong for Day:{0}", hl[1].fk_AssetKeyDate));
      Assert.AreEqual(hl2.EventUTC, hl[1].EventUTC, string.Format("EventUTC wrong for Day:{0}", hl[1].fk_AssetKeyDate));
      Assert.AreEqual(controlHLLocal, hl[1].EventDeviceTime, string.Format("EventDeviceTime wrong for Day:{0}", hl[1].fk_AssetKeyDate));
      Assert.AreEqual(controlEPaLocal.KeyDate(), ep[2].fk_AssetKeyDate, string.Format("Date wrong for Day:{0}", ep[2].fk_AssetKeyDate));
      Assert.AreEqual(ep2a.EventUTC, ep[2].EventUTC, string.Format("EventUTC wrong for Day:{0}", ep[2].fk_AssetKeyDate));
      Assert.AreEqual(controlEPaLocal, ep[2].EventDeviceTime, string.Format("EventDeviceTime wrong for Day:{0}", ep[2].fk_AssetKeyDate));
      Assert.AreEqual(controlEPbLocal.KeyDate(), ep[3].fk_AssetKeyDate, string.Format("Date wrong for Day:{0}", ep[3].fk_AssetKeyDate));
      Assert.AreEqual(ep2b.EventUTC, ep[3].EventUTC, string.Format("EventUTC wrong for Day:{0}", ep[3].fk_AssetKeyDate));
      Assert.AreEqual(controlEPbLocal, ep[3].EventDeviceTime, string.Format("EventDeviceTime wrong for Day:{0}", ep[3].fk_AssetKeyDate));
    }


    [DatabaseTest]
    [TestMethod]
   public void RuntimeMeterReset()
    {
      // PL121 uses the MeterDelta method. This scenario tests that manual adjustments to runtime are reflected in AUD.
      //   DeviceDate          RuntimeMeter      Adjustment           RuntimeHours
      //                                           From   To
      //   6  Dec 2010 08:00    1000 
      //   7  Dec 2010 08:00    1010                                   10 
      //   7  Dec 2010 09:00                                                             EngineStart
      //   7  Dec 2010 17:00                                                             EngineStop
      //   7  Dec 2010 23:00                                                             EngineStart
      //   8  Dec 2010 09:00                        1020,  10 
      //   8  Dec 2010 16:00                                                             EngineStop
      //   9  Dec 2010 09:10                          15,  40 
      //   9  Dec 2010 20:00                                                             EngineStart
      //   10 Dec 2010 08:00      50                                   25 
      //   10 Dec 2010 22:00                                                             EngineStop
      //   11 Dec 2010 09:00                          50,  1000 
      //   11 Dec 2010 12:00                                                             EngineStart
      //   12 Dec 2010 08:00    1000                                   0  
      //   13 Dec 2010 12:00    1002                                   2 
      //   13 Dec 2010 14:00                        1004,  0 
      //   14 Dec 2010 08:00      14                                   16
      //   15 Dec 2010 12:00                        15,  16 
      //   15 Dec 2010 12:00      16                                   1 
      //   16 Dec 2010 08:00      22                                   6 

      var target = new EquipmentAPI(); 
      var customer = TestData.TestCustomer;
			var user = TestData.TestCustomerUser;
			var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.PL121.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService()
                  .Save();
      
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      long? runtimeHours = 1000; 
      double? latitude = 53.756058;
      double? longitude = 2.021608;
     // create events in NH_DATA need to get timezone of GMT-5 to reproduce local time conversions
      DateTime dateSetup = new DateTime((DateTime.UtcNow.Month < 3 ? DateTime.UtcNow.Year - 1 : DateTime.UtcNow.Year), 02, 01, 00, 00, 00);
      DateTime testStartEventUTC = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day, 8, 0, 0);
      DateTime? eventUTC = testStartEventUTC;
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      runtimeHours = 1010;
      eventUTC = testStartEventUTC.AddDays(1);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);
      eventUTC = testStartEventUTC.AddDays(1).AddHours(1);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = testStartEventUTC.AddDays(1).AddHours(9);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID,  eventUTC.Value, false );
      eventUTC = testStartEventUTC.AddDays(1).AddHours(15);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID,  eventUTC.Value, true );

      eventUTC = testStartEventUTC.AddDays(2).AddHours(1);
      Helpers.NHData.ServiceMeterAdjustment_Add(asset.AssetID, eventUTC.Value, 1020, 10); // from 1020 to 10
      eventUTC = testStartEventUTC.AddDays(2).AddHours(8);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID,  eventUTC.Value, false );

      eventUTC = testStartEventUTC.AddDays(3).AddHours(1);
      Helpers.NHData.ServiceMeterAdjustment_Add(asset.AssetID, eventUTC.Value, 15, 40);
      eventUTC = testStartEventUTC.AddDays(3).AddHours(12);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID,  eventUTC.Value, true );

      runtimeHours = 50;
      eventUTC = testStartEventUTC.AddDays(4);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);
      eventUTC = testStartEventUTC.AddDays(4).AddHours(14);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID,  eventUTC.Value, false );

      eventUTC = testStartEventUTC.AddDays(11).AddHours(1);
      Helpers.NHData.ServiceMeterAdjustment_Add(asset.AssetID, eventUTC.Value, 50, 1000);
      eventUTC = testStartEventUTC.AddDays(11).AddHours(4);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID,  eventUTC.Value, true );

      runtimeHours = 1000;
      eventUTC = testStartEventUTC.AddDays(12);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      runtimeHours = 1002;
      eventUTC = testStartEventUTC.AddDays(13);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);
      eventUTC = testStartEventUTC.AddDays(13).AddHours(1);
      Helpers.NHData.ServiceMeterAdjustment_Add(asset.AssetID, eventUTC.Value, 1004, 0); 

      runtimeHours = 14;
      eventUTC = testStartEventUTC.AddDays(14);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      eventUTC = testStartEventUTC.AddDays(15).AddHours(4);
      Helpers.NHData.ServiceMeterAdjustment_Add(asset.AssetID, eventUTC.Value, 15, 16); 
      runtimeHours = 16;
      eventUTC = testStartEventUTC.AddDays(15).AddHours(4);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      runtimeHours = 22;
      eventUTC = testStartEventUTC.AddDays(16);
      
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);
     
      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                           && aud.fk_AssetPriorKeyDate != null
                                        orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(7, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(10, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.IsNotNull(util[0].ifk_DimTimeZoneID, string.Format("timezoneID incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(25, util[1].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(0, util[2].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(2, util[3].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(16, util[4].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.AreEqual(1, util[5].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[5].fk_AssetKeyDate));
      Assert.AreEqual(6, util[6].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[6].fk_AssetKeyDate));

      // AO comes from EngineStartStops, not EngineParameters
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[0].fk_AssetKeyDate, 2, 0, 0, 9);  // 7th Dec
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[0].fk_AssetKeyDate + 1, 1, 0, 0, 16);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[0].fk_AssetKeyDate + 2, 1, 0, 0, 4);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[0].fk_AssetKeyDate + 3, 1, 0, 0, 22); 
      // 11th Dec is an 'inProgress' event,so will not be in FactAssetOperation table
    }

    [DatabaseTest]
    [TestMethod]
    public void ZeroExpectedHours()
    {
      // PL121 uses the MeterDelta method. Test that when Expected hours is set to Zero...the utilization division shows '-' using the -888 indicator
      //   DeviceDate          Runtime  IdleTime IdleFuel TotalFuel ExpectedRT   Runtime   Idle    Working  Running   Working  Efficiency%
      //                       Meter    Meter    Meter    Meter     Hours        Hours     Hours   Hours    Utiln %    Utiln%
      //   27 Mar 2010 08:00    1000     900      1200     16000     0  (sat)     --------------------------------------------------------------- 
      //   28 Mar 2010 08:00    1010     903      1214     16030     0  (sun)         10        3       7        -888      -888      70 
      //   29 Mar 2010 08:00    1020     906      1228     16044     8  (mon)         10        3       7        125        87.5     70 
    
      var target = new EquipmentAPI(); 
      var customer = TestData.TestCustomer;
			var user = TestData.TestCustomerUser;
			var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.PL121.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService()
                  .Save();
      
      Helpers.NHRpt.DimTables_Populate();
      
      
      // create events in NH_DATA
      long? runtimeHours = 1000;
      double? latitude = 53.756058;
      double? longitude = 8.021608;
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Saturday - DateTime.UtcNow.DayOfWeek).AddDays(-14)).Date;
      DateTime? eventUTC = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day, 8, 0, 0);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      double? engineIdleHours = 900;
      double? idleFuelGallons = 1200;
      double? consumptionGallons = 16000;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      runtimeHours = 1010;
      eventUTC = eventUTC.Value.AddDays(1);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 903;
      idleFuelGallons = 1214;
      consumptionGallons = 16030;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      runtimeHours = 1020;
      eventUTC = eventUTC.Value.AddDays(1);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 906;
      idleFuelGallons = 1228;
      consumptionGallons = 16044;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                           && aud.fk_AssetPriorKeyDate != null
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(2, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(10, util[0].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(3, util[0].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(7, util[0].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[0].fk_AssetKeyDate));

      Assert.AreEqual(10, util[1].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(3, util[1].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(7, util[1].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
    }

    [DatabaseTest]
    [TestMethod]
    public void MissingVSZeroDays()
    {
      //  If a delta between today and any prior day cannot be cald, then no row should appear in AUD i.e. if today has no RT info or no prior day found.
      //  Days where there is a Runtime event but is contains no difference from the prior day should have a row of all zeros in AUD
      //
      //   DeviceDate          Runtime  IdleTime IdleFuel  ExpectedRT   Runtime   Idle    Working   PriorStartOfDeviceDay
      //                       Meter    Meter    Meter     Hours        Hours     Hours   Hours   
      //   5 Dec 2010 08:00    1000     900      1200      0(Sunday)    --------------no row-----------------------------
      //   6 Dec 2010 08:00    1010     903      1214      8            10        3       7         1 Feb 2009    
      //   7 Dec 2010 08:00    N/A      N/A      N/A       8            --------------no row-----------------------------
      //   8 Dec 2010 08:00    1026     910      1220     8            16        7        3        2 Feb 2009  
      //   9 Dec 2010 08:00    1026     910      1220      8            0         0        0        4 Feb 2009 

      var target = new EquipmentAPI(); 
      var customer = TestData.TestCustomer;
			var user = TestData.TestCustomerUser;
			var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.PL121.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService()
                  .Save();
      Helpers.NHRpt.DimTables_Populate();

       // create events in NH_DATA
      long? runtimeHours = 1000;
      double? latitude = 53.756058;
      double? longitude = 8.021608;
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Sunday - DateTime.UtcNow.DayOfWeek).AddDays(-14)).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day, 8, 0, 0);
      DateTime? eventUTC = testStartDeviceDay;
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      double? engineIdleHours = 900;
      double? idleFuelGallons = 1200;
      double? consumptionGallons = 16000;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      runtimeHours = 1010;
      eventUTC = testStartDeviceDay.AddDays(1);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 903;
      idleFuelGallons = 1214;
      consumptionGallons = 16030;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      runtimeHours = 1026;
      eventUTC = testStartDeviceDay.AddDays(3);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 910;
      idleFuelGallons = 1220;
      consumptionGallons = 16050;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      runtimeHours = 1026;
      eventUTC = testStartDeviceDay.AddDays(4);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 910;
      idleFuelGallons = 1220;
      consumptionGallons = 16130;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                           && aud.fk_AssetPriorKeyDate != null
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(3, util.Count(), "Incorrect FactAssetUtilizationDaily records.");

      Assert.AreEqual(testStartDeviceDay.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartKeyDate wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.KeyDate(), util[0].fk_AssetPriorKeyDate, string.Format("PriorStartKeyDate wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(10, util[0].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(3, util[0].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(7, util[0].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[0].fk_AssetKeyDate));

      Assert.AreEqual(testStartDeviceDay.AddDays(3).KeyDate(), util[1].fk_AssetKeyDate, string.Format("StartKeyDate wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.AddDays(1).KeyDate(), util[1].fk_AssetPriorKeyDate, string.Format("PriorStartKeyDate wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(16, util[1].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(7, util[1].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(9, util[1].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[1].fk_AssetKeyDate));

      Assert.AreEqual(testStartDeviceDay.AddDays(4).KeyDate(), util[2].fk_AssetKeyDate, string.Format("StartKeyDate wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.AddDays(3).KeyDate(), util[2].fk_AssetPriorKeyDate, string.Format("PriorStartKeyDate wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(0, util[2].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(0, util[2].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(0, util[2].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
    }

    [DatabaseTest]
    [TestMethod]
    public void OutOfOrderEvents()
    {
      /// If events arrive late, and need to be inserted before an existing AUD, then the subsequent AUD RT and PriorDeviceDay needs to be recalc
      /// Consider also when they arrive in same run and in a subsequent run.
      //   DeviceDate          Runtime   ExpectedRT   Runtime   PriorStartOfDeviceDay
      //                       Meter     Hours        Hours     
      //   1 Feb 2010 08:00    1000      8(Monday)    -------------------------------
      //   2 Feb 2010 08:00    1010      8            10        1 Feb 2010    
      //   3 Feb 2010 08:00    1015      8            5         2 Feb 2010    
      //   4 Feb 2010 08:00    1026      8            11        3 Feb 2010  
      //   5 Feb 2010 08:00    1032      8            6         4 Feb 2010 
      //   6 Feb 2010 08:00    1032      0            0         5 Feb 2010 -- this was bug -- zero runtimes on insert
      //   8 Feb 2010 08:00    1032      8(Monday)    0         6 Feb 2010 -- this was bug -- ExpectedRT change on insert
      //
      // Scenario: Insert 1,3,4,5,8; RunETL; Insert 2,6; RunETL;

       var target = new EquipmentAPI(); 
      var customer = TestData.TestCustomer;
			var user = TestData.TestCustomerUser;
			var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.PL121.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService()
                  .Save();
      
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      long? runtimeHours = 1000;
      double? latitude = 53.756058;
      double? longitude = 8.021608;
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Monday - DateTime.UtcNow.DayOfWeek).AddDays(-14)).Date;
      DateTime? eventUTC = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day, 8, 0, 0);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      runtimeHours = 1015;
      eventUTC = eventUTC.Value.AddDays(2);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      runtimeHours = 1026;
      eventUTC = eventUTC.Value.AddDays(1);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);
      
      runtimeHours = 1032;
      eventUTC = eventUTC.Value.AddDays(1);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      runtimeHours = 1032;
      eventUTC = eventUTC.Value.AddDays(3);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);
 
      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                           && aud.fk_AssetPriorKeyDate != null
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(4, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(dateSetup.AddDays(2).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartKeyDate wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(dateSetup.KeyDate(), util[0].fk_AssetPriorKeyDate, string.Format("PriorStartKeyDate wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(15, util[0].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[0].fk_AssetKeyDate));

      Assert.AreEqual(dateSetup.AddDays(7).KeyDate(), util[3].fk_AssetKeyDate, string.Format("StartKeyDate wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(dateSetup.AddDays(4).KeyDate(), util[3].fk_AssetPriorKeyDate, string.Format("PriorStartKeyDate wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(0, util[3].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[3].fk_AssetKeyDate));

      runtimeHours = 1010;
      eventUTC = eventUTC.Value.AddDays(-6);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);
      
      runtimeHours = 1032;
      eventUTC = eventUTC.Value.AddDays(4);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
              where aud.ifk_DimAssetID == asset.AssetID
               && aud.fk_AssetPriorKeyDate != null
              orderby aud.fk_AssetKeyDate
              select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(6, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(dateSetup.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartKeyDate wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(dateSetup.KeyDate(), util[0].fk_AssetPriorKeyDate, string.Format("PriorStartKeyDate wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(10, util[0].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[0].fk_AssetKeyDate));

      Assert.AreEqual(dateSetup.AddDays(2).KeyDate(), util[1].fk_AssetKeyDate, string.Format("StartKeyDate wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(dateSetup.AddDays(1).KeyDate(), util[1].fk_AssetPriorKeyDate, string.Format("PriorStartKeyDate wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(5, util[1].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[1].fk_AssetKeyDate));

      Assert.AreEqual(dateSetup.AddDays(3).KeyDate(), util[2].fk_AssetKeyDate, string.Format("StartKeyDate wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(dateSetup.AddDays(2).KeyDate(), util[2].fk_AssetPriorKeyDate, string.Format("PriorStartKeyDate wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(11, util[2].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[2].fk_AssetKeyDate));

      Assert.AreEqual(dateSetup.AddDays(4).KeyDate(), util[3].fk_AssetKeyDate, string.Format("StartKeyDate wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(dateSetup.AddDays(3).KeyDate(), util[3].fk_AssetPriorKeyDate, string.Format("PriorStartKeyDate wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(6, util[3].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[3].fk_AssetKeyDate));

      Assert.AreEqual(dateSetup.AddDays(5).KeyDate(), util[4].fk_AssetKeyDate, string.Format("StartKeyDate wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.AreEqual(dateSetup.AddDays(4).KeyDate(), util[4].fk_AssetPriorKeyDate, string.Format("PriorStartKeyDate wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.AreEqual(0, util[4].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[4].fk_AssetKeyDate));

      Assert.AreEqual(dateSetup.AddDays(7).KeyDate(), util[5].fk_AssetKeyDate, string.Format("StartKeyDate wrong for Day:{0}", util[5].fk_AssetKeyDate));
      Assert.AreEqual(dateSetup.AddDays(5).KeyDate(), util[5].fk_AssetPriorKeyDate, string.Format("PriorStartKeyDate wrong for Day:{0}", util[5].fk_AssetKeyDate));
      Assert.AreEqual(0, util[5].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
   }

    [DatabaseTest]
    [TestMethod]
  [Ignore]
    public void MultipleEventsInADay()
    {
      //   Should use the last event in a day
      //   DeviceDate          Runtime   Idle   Runtime   IdleHours
      //                       Meter     Meter  Hours     
      //   23 Mar 2010 20:00   100       12     -------------------
      //   24 Mar 2010 20:00   100       12     0         0
      //   25 Mar 2010 23:00   110       15     10        3    
      //   28 Mar 2010 23:00   148       26     38        11    
      //   29 Mar 2010 08:00   156       28     -------------------    
      //   29 Mar 2010 23:00   164       30     16        4    

      var target = new EquipmentAPI(); 
      var customer = TestData.TestCustomer;
			var user = TestData.TestCustomerUser;
			var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.PL121.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                 .WithCoreService().Save();

      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Wednesday - DateTime.UtcNow.DayOfWeek).AddDays(-14)).Date;
      DateTime testStartEventUTC = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartEventUTC;
      long? runtimeHours = 100;
      double? latitude = 20; // UTC - 0
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(20);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      double? engineIdleHours = 12;
      double? idleFuelGallons = 50;
      double? consumptionGallons = 400;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      startOfDeviceDay = testStartEventUTC.AddDays(1);
      runtimeHours = 100;
      eventUTC = startOfDeviceDay.AddHours(20);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 12;
      idleFuelGallons = 50;
      consumptionGallons = 400;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      startOfDeviceDay = testStartEventUTC.AddDays(2);
      runtimeHours = 110;
      eventUTC = startOfDeviceDay.AddHours(23);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 15;
      idleFuelGallons = 60;
      consumptionGallons = 480;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      startOfDeviceDay = testStartEventUTC.AddDays(5);
      runtimeHours = 148;
      eventUTC = startOfDeviceDay.AddHours(23);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 26;
      idleFuelGallons = 95;
      consumptionGallons = 670;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      startOfDeviceDay = testStartEventUTC.AddDays(6);
      runtimeHours = 156;
      eventUTC = startOfDeviceDay.AddHours(8);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 28;
      idleFuelGallons = 108;
      consumptionGallons = 710;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      runtimeHours = 164;
      eventUTC = startOfDeviceDay.AddHours(23);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 30;
      idleFuelGallons = 120;
      consumptionGallons = 760;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );
      
      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      
      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                           && aud.fk_AssetPriorKeyDate != null
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(4, util.Count(), "Incorrect FactAssetUtilizationDaily records.");

      Assert.AreEqual(testStartEventUTC.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("fk_AssetKeyDate wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(0, util[0].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(0, util[0].IdleHours, string.Format("Runtime hours wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartEventUTC.AddDays(2).KeyDate(), util[1].fk_AssetKeyDate, string.Format("fk_AssetKeyDate wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(10, util[1].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(3, util[1].IdleHours, string.Format("Runtime hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(testStartEventUTC.AddDays(5).KeyDate(), util[2].fk_AssetKeyDate, string.Format("fk_AssetKeyDate wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(38, util[2].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(11, util[2].IdleHours, string.Format("Runtime hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(testStartEventUTC.AddDays(6).KeyDate(), util[3].fk_AssetKeyDate, string.Format("fk_AssetKeyDate wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(16, util[3].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(4, util[3].IdleHours, string.Format("Runtime hours wrong for Day:{0}", util[3].fk_AssetKeyDate));
  }

  
    [DatabaseTest]
    [TestMethod]
    public void PL321DualEngine()
    {
      //  Ignore idleTime/Gals from DuelEngine events (has events from different engines with their own series)
      //   DeviceDate          Runtime   Idle   Runtime   IdleHours WorkingHours
      //                       Meter     Meter  Hours     
      //   23 Mar 2010 20:00   100       12     -------------------
      //   24 Mar 2010 20:00   100       12     0         0
      //   25 Mar 2010 23:00   110       15     10        -999      
      //   25 Mar 2010 23:00   110       2      10        -999      10
      //   28 Mar 2010 23:00   148       4      38        -999  
      //   28 Mar 2010 23:00   148       26     38        -999      38 
      //   29 Mar 2010 08:00   156       28     -------------------    
      //   29 Mar 2010 23:00   164       30     16        -999 
      //   29 Mar 2010 23:00   164       null   16        -999      16

      var target = new EquipmentAPI(); 
      var customer = TestData.TestCustomer;
			var user = TestData.TestCustomerUser;
			var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.PL321.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                 .WithCoreService().MakeCode("CAT").ModelName("637G").ProductFamily("WHEEL TRACTOR SCRAPERS").ModelVariant(2).Save();

      //asset.Model = "637G";
      Helpers.NHRpt.DimTables_Populate();

       // create events in NH_DATA
      long? runtimeHours = 100;
      double? latitude = 20; // UTC - 0
      double? longitude = -8.021608;
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Tuesday - DateTime.UtcNow.DayOfWeek).AddDays(-14)).Date;
      DateTime testStartEventUTC = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day, 20, 00, 00);
      DateTime? t0 = testStartEventUTC;
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, t0.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      double? engineIdleHours = 12;
      double? idleFuelGallons = 50;
      double? consumptionGallons = 400;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  t0.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      DateTime? t1 = testStartEventUTC.AddDays(1);
      runtimeHours = 100;
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, t1.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 12;
      idleFuelGallons = 50;
      consumptionGallons = 450;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  t1.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      DateTime? t2 = testStartEventUTC.AddDays(2).AddHours(3);
      runtimeHours = 110;
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, t2.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 15;
      idleFuelGallons = 60;
      consumptionGallons = 480;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  t2.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      engineIdleHours = 2;
      idleFuelGallons = 56000;
      consumptionGallons = 480;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  t2.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      DateTime? t3 = testStartEventUTC.AddDays(5).AddHours(3);
      runtimeHours = 148;
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, t3.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 26;
      idleFuelGallons = 95;
      consumptionGallons = 670;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  t3.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      engineIdleHours = 4;
      idleFuelGallons = 5700;
      consumptionGallons = 670;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  t3.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      DateTime? t4 = testStartEventUTC.AddDays(6).AddHours(-12);
      runtimeHours = 156;
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, t4.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 28;
      idleFuelGallons = 108;
      consumptionGallons = 710;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  t4.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      DateTime? t5 = testStartEventUTC.AddDays(6).AddHours(3);
      runtimeHours = 164;
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, t5.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 30;
      idleFuelGallons = 120;
      consumptionGallons = 760;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  t5.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      engineIdleHours = null;
      idleFuelGallons = 5800;
      consumptionGallons = 760;
      Helpers.NHData.EngineParameters_Add(asset.AssetID,  t5.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null );

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      
      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(5, util.Count(), "Incorrect FactAssetUtilizationDaily records.");

      Assert.AreEqual(t1.KeyDate(), util[1].fk_AssetKeyDate, string.Format("fk_AssetKeyDate wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(0, util[1].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.IsNull(util[1].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.IsNull(util[1].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(50, util[1].TotalFuelConsumedGallons, string.Format("TotalFuelConsumed wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.IsNull(util[1].IdleFuelConsumedGallons, string.Format("IdleFuelConsumed wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.IsNull(util[1].IdleHoursMeter, string.Format("IdleHoursMeter wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(null, util[1].WorkingFuelConsumedGallons, string.Format("WorkingFuelConsumed wrong for Day:{0}", util[1].fk_AssetKeyDate));

      Assert.AreEqual(t2.KeyDate(), util[2].fk_AssetKeyDate, string.Format("fk_AssetKeyDate wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(10, util[2].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.IsNull(util[2].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.IsNull(util[2].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(30, util[2].TotalFuelConsumedGallons, string.Format("TotalFuelConsumed wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.IsNull(util[2].IdleFuelConsumedGallons, string.Format("IdleFuelConsumed wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.IsNull(util[2].IdleHoursMeter, string.Format("IdleHoursMeter wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.IsNull(util[2].WorkingFuelConsumedGallons, string.Format("WorkingFuelConsumed wrong for Day:{0}", util[2].fk_AssetKeyDate));

      Assert.AreEqual(t3.KeyDate(), util[3].fk_AssetKeyDate, string.Format("fk_AssetKeyDate wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(38, util[3].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.IsNull(util[3].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.IsNull(util[3].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(190, util[3].TotalFuelConsumedGallons, string.Format("TotalFuelConsumed wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.IsNull(util[3].IdleFuelConsumedGallons, string.Format("IdleFuelConsumed wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.IsNull(util[3].IdleHoursMeter, string.Format("IdleHoursMeter wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.IsNull(util[3].WorkingFuelConsumedGallons, string.Format("WorkingFuelConsumed wrong for Day:{0}", util[3].fk_AssetKeyDate));

      Assert.AreEqual(t4.KeyDate(), util[4].fk_AssetKeyDate, string.Format("fk_AssetKeyDate wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.AreEqual(16, util[4].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.IsNull(util[4].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.IsNull(util[4].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[4].fk_AssetKeyDate));     
      Assert.AreEqual(90, util[4].TotalFuelConsumedGallons, string.Format("TotalFuelConsumed wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.IsNull(util[4].IdleFuelConsumedGallons, string.Format("IdleFuelConsumed wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.IsNull(util[4].IdleHoursMeter, string.Format("IdleHoursMeter wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.IsNull(util[4].WorkingFuelConsumedGallons, string.Format("WorkingFuelConsumed wrong for Day:{0}", util[4].fk_AssetKeyDate));
    }


    [DatabaseTest]
    [TestMethod]
    public void ZeroLifetimeMeterValues()
    {
      // TFS16080
      // All Lifetime Meter values of 0 should be treated as null values.  There is a known ECM issue in which "0" is sent instead of "Null". (SMH, Idle Fuel, Idle Time, Lifetime Fuel)

      var target = new EquipmentAPI();
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.PL121.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService()
                  .Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      // day1
      double? runtimeHours = 1000.120;
      double? latitude = 53.756058;
      double? longitude = 8.021608;
      double? engineIdleHours = 900.200;
      double? idleFuelGallons = 1200.280;
      double? consumptionGallons = 1600.33;
      double? percentRemaining = null;
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Sunday - DateTime.UtcNow.DayOfWeek).AddDays(-14)).Date;
      DateTime testStartEventUTC = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day, 8, 0, 0);
      DateTime? eventUTC = testStartEventUTC;
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // day2
      runtimeHours = 0;
      engineIdleHours = 0;
      idleFuelGallons = 0;
      consumptionGallons = 1600.35;
      eventUTC = testStartEventUTC.AddDays(1);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // day3
      runtimeHours = 1000.131;
      engineIdleHours = 900.070;
      idleFuelGallons = 1200.292;
      consumptionGallons = 0;
      eventUTC = testStartEventUTC.AddDays(2);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      
      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<HoursLocation> hl = (from aud in Ctx.RptContext.HoursLocationReadOnly
                                              where aud.ifk_DimAssetID == asset.AssetID
                                              orderby aud.fk_AssetKeyDate
                                              select aud).ToList<HoursLocation>();
      Assert.AreEqual(3, hl.Count(), "Incorrect HoursLocation record count.");

      List<EngineParameters> ep = (from aud in Ctx.RptContext.EngineParametersReadOnly
                                where aud.ifk_DimAssetID == asset.AssetID
                                orderby aud.fk_AssetKeyDate
                                   select aud).ToList<EngineParameters>();
      Assert.AreEqual(3, ep.Count(), "Incorrect EngineParameters record count.");

      Assert.AreEqual(testStartEventUTC.KeyDate(), hl[0].fk_AssetKeyDate, string.Format("Date wrong for Day1"));
      Assert.AreEqual(1000.120, hl[0].RuntimeHoursMeter, string.Format("RuntimeHoursMeter wrong for Day1"));
      Assert.AreEqual(latitude, hl[0].Latitude, string.Format("Latitude wrong for Day1"));
      Assert.AreEqual(900.200, ep[0].IdleHours, string.Format("IdleHours wrong for Day1"));
      Assert.AreEqual(1200.280, ep[0].IdleGallonsCuml, string.Format("IdleGallonsCuml wrong for Day1"));
      Assert.AreEqual(1600.33, ep[0].TotalGallonsCuml, string.Format("TotalGallonsCuml wrong for Day1"));

      Assert.AreEqual(testStartEventUTC.AddDays(1).KeyDate(), hl[1].fk_AssetKeyDate, string.Format("Date wrong for Day2"));
      Assert.IsNull(hl[1].RuntimeHoursMeter, string.Format("RuntimeHoursMeter wrong for Day2"));
      Assert.AreEqual(latitude, hl[1].Latitude, string.Format("Latitude wrong for Day2"));
      Assert.IsNull(ep[1].IdleHours, string.Format("IdleHours wrong for Day2"));
      Assert.IsNull(ep[1].IdleGallonsCuml, string.Format("IdleGallonsCuml wrong for Day2"));
      Assert.AreEqual(1600.35, ep[1].TotalGallonsCuml, string.Format("TotalGallonsCuml wrong for Day2"));

      Assert.AreEqual(testStartEventUTC.AddDays(2).KeyDate(), hl[2].fk_AssetKeyDate, string.Format("Date wrong for Day3"));
      Assert.AreEqual(1000.131, hl[2].RuntimeHoursMeter, string.Format("RuntimeHoursMeter wrong for Day3"));
      Assert.AreEqual(latitude, hl[2].Latitude, string.Format("Latitude wrong for Day3"));
      Assert.AreEqual(900.070, ep[2].IdleHours, string.Format("IdleHours wrong for Day3"));
      Assert.AreEqual(1200.292, ep[2].IdleGallonsCuml, string.Format("IdleGallonsCuml wrong for Day3"));
      Assert.IsNull(ep[2].TotalGallonsCuml, string.Format("TotalGallonsCuml wrong for Day3"));
    }


    #region Spikes
    [DatabaseTest]
    [TestMethod]
    public void AssetSpikesAndPartial()
    {
      // PL121 uses the MeterDelta method. This scenario tests that spikes and partially reported AssetUtilization data are flagged as such
      //   DeviceDate          Runtime  IdleTime IdleFuel TotalFuel ExpectedRT   Runtime   Idle    Working  Running   Working  Efficiency%
      //                       Meter    Meter    Meter    Meter     Hours        Hours     Hours   Hours    Utiln %    Utiln%
      //  5  Dec 2010 08:00    1000     900      1200     16000     0(Sunday)     
      //  6  Dec 2010 08:00    1010     903      1214     16030     8            10        3       7        125        87.5    70 
      //  7  Dec 2010 08:00    1017     N/A      N/A      N/A       --------------------------------------------------------------- 
      //  8  Dec 2010 08:00    1026     960      5490     N/A       8            16        -999    16       NULL*      NULL*   100 
      //  10 Dec 2010 08:00    1035     967      5510     N/A       8(Fri  )     9         7       2        NULL*      NULL*   22.222 
      //                                                            * Total/idle covered >1 day but expectedHours is for today
      //  11 Dec 2010 08:00    N/A      1000     7650     N/A       ---------------------------------------------------------------
      //  12 Dec 2010 08:00    1045     1005     7662     N/A       0(Sun)      10         38      -28      NULL      NULL     -280
      //  13 Dec 2010 08:00    90       N/A      N/A      N/A       ---------------------------------------------------------------$
      //                                                            $if a device has EP events, ONLY use those (furthermore, only if they are paired with a RT)
      //  14 Dec 2010 08:00    210      5277     7670     18000     8(Tue)      -999     -999     -999     -999       -999    -999
      //  15 Dec 2010 08:00    230      5280     7700     18200     8           20       3        17       250        212.5   85
      //  16 Dec 2010 08:00    241      5282     7704     18210     8           11       2        9        137.5      112.5   81
      //  16 Dec 2010 08:12    245      N/A      N/A      N/A       ---------------------------------------------------------------$ 
      //  17 Dec 2010 08:00    249      N/A      N/A      N/A       ---------------------------------------------------------------$
      //  17 Dec 2010 08:12    253      5286     7704     18210     8           12       4        8         150        100    66.66667
      //  18 Dec 2010 08:11    277      5286     7704     18400     0 (Sat)     24       0        24       -888      -888     100  
      //                                                                    * round this and prior EventUTC as CAT rounds their RTHours

      var target = new EquipmentAPI();
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.PL121.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService()
                  .Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      long? runtimeHours = 1000;
      double? latitude = 53.756058;
      double? longitude = 8.021608;
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Sunday - DateTime.UtcNow.DayOfWeek).AddDays(-14)).Date;
      DateTime testStartEventUTC = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day, 8, 0, 0);
      DateTime? eventUTC = testStartEventUTC;
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      double? engineIdleHours = 900;
      double? idleFuelGallons = 1200;
      double? consumptionGallons = 16000;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 1010;
      eventUTC = testStartEventUTC.AddDays(1);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 903;
      idleFuelGallons = 1214;
      consumptionGallons = 16030;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 1017;
      eventUTC = testStartEventUTC.AddDays(2);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      runtimeHours = 1026;
      eventUTC = testStartEventUTC.AddDays(3);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 960;
      idleFuelGallons = 5490;
      consumptionGallons = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 1035;
      eventUTC = testStartEventUTC.AddDays(5);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 967;
      idleFuelGallons = 5510;
      consumptionGallons = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      eventUTC = testStartEventUTC.AddDays(6);
      engineIdleHours = 1000;
      idleFuelGallons = 7650;
      consumptionGallons = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 1045;
      eventUTC = testStartEventUTC.AddDays(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 1005;
      idleFuelGallons = 7662;
      consumptionGallons = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 90;
      eventUTC = testStartEventUTC.AddDays(8);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      runtimeHours = 210;
      eventUTC = testStartEventUTC.AddDays(9);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 5277;
      idleFuelGallons = 7670;
      consumptionGallons = 18000;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 230;
      eventUTC = testStartEventUTC.AddDays(10);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 5280;
      idleFuelGallons = 7700;
      consumptionGallons = 18200;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 241;
      eventUTC = testStartEventUTC.AddDays(11);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 5282;
      idleFuelGallons = 7704;
      consumptionGallons = 18210;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 245;
      eventUTC = testStartEventUTC.AddDays(11).AddMinutes(12);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      runtimeHours = 249;
      eventUTC = testStartEventUTC.AddDays(12);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      runtimeHours = 253;
      eventUTC = testStartEventUTC.AddDays(12).AddMinutes(12);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 5286;
      idleFuelGallons = 7704;
      consumptionGallons = 18210;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 277;
      eventUTC = testStartEventUTC.AddDays(13).AddMinutes(12);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 5286;
      idleFuelGallons = 7704;
      consumptionGallons = 18400;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                              where aud.ifk_DimAssetID == asset.AssetID
                                               && aud.fk_AssetPriorKeyDate != null
                                              orderby aud.fk_AssetKeyDate
                                              select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(9, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartEventUTC.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("Date wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(10, util[0].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(3, util[0].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(7, util[0].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(16, util[1].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.IsNull(util[1].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(3, util[1].ifk_IdleHoursCalloutTypeID, string.Format("Idle hours calloutType wrong for Day:{0}", util[1].fk_AssetKeyDate));


      Assert.AreEqual(null, util[1].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(9, util[2].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(7, util[2].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(2, util[2].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(10, util[3].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(38, util[3].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(-28, util[3].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.IsNull(util[4].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.AreEqual(3, util[4].ifk_RuntimeHoursCalloutTypeID, string.Format("Runtime hours wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.IsNull(util[4].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.AreEqual(3, util[4].ifk_IdleHoursCalloutTypeID, string.Format("Idle hours wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.IsNull(util[4].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.AreEqual(20, util[5].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[5].fk_AssetKeyDate));
      Assert.AreEqual(3, util[5].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[5].fk_AssetKeyDate));
      Assert.AreEqual(17, util[5].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[5].fk_AssetKeyDate));
      Assert.AreEqual(11, util[6].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[6].fk_AssetKeyDate));
      Assert.AreEqual(2, util[6].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[6].fk_AssetKeyDate));
      Assert.AreEqual(9, util[6].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[6].fk_AssetKeyDate));
      Assert.AreEqual(12, util[7].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[7].fk_AssetKeyDate));
      Assert.AreEqual(4, util[7].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[7].fk_AssetKeyDate));
      Assert.AreEqual(8, util[7].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[7].fk_AssetKeyDate));
      Assert.AreEqual(24, util[8].RuntimeHours, string.Format("Runtime hours wrong for Day:{0}", util[8].fk_AssetKeyDate));
      Assert.AreEqual(0, util[8].IdleHours, string.Format("Idle hours wrong for Day:{0}", util[8].fk_AssetKeyDate));
      Assert.AreEqual(24, util[8].WorkingHours, string.Format("Working hours wrong for Day:{0}", util[8].fk_AssetKeyDate));

    }

    [DatabaseTest]
    [TestMethod]
    public void FuelSpikesAndPartial()
    {
      // PL121 uses the MeterDelta method. This scenario tests that spikes and partially reported FuelUtilization data are flagged as such
      //   DeviceDate        Runtime  IdleTime IdleFuel TotalFuel Runtime Idle  Working  Total Idling  Working  Average
      //                     Meter    Meter    Meter    Meter     Hours   Hours Hours    Fuel  Fuel    Fuel     BurnRate 
      //  6  Dec 2010 08:00  1000     900      1200     16000      (monday)
      //  7  Dec 2010 08:00  1010     903      1214     16030     10        3       7    30    14      16       3    
      //  8  Dec 2010 08:00  1017     N/A      N/A      16050     7         NULL    7    20    NULL    NULL     2.85714
      //  9  Dec 2010 08:00  1026     908      5490     16130     9         NULL    9    80    -999    NULL     8.88  Idle calcd over 2 days, but is too large
      //                                                                    idleHours, fuel, working cant be calcd as event on 3rd has null idle data         
      //  10 Dec 2010 08:00  1034     915      5510     N/A       8         7       1    NULL  NULL    NULL     NULL
      //  11 Dec 2010 08:00  1043     915      7650(s)  N/A       9         0       9    NULL  NULL    NULL     NULL    
      //  12 Dec 2010 08:00  1053     916      7662     90000     10        1       9    -999  -999    -999     -999 
      //                                                                    totalfuel, working cant be calcd as event on 8th has null Totalfuel data 
      //  13 Dec 2010 08:00  1061     N/A      N/A      10000(s)  8         NULL    8    -999  NULL    -999     -999    
      //  14 Dec 2010 08:00  1070     920      90000(s) 40000(s)  9         2       7    -999  -999    -999     -999   
      //  15 Dec 2010 08:00  1080     923      90030    40070     10        3       7    70    30      40       7               

      var target = new EquipmentAPI();
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.PL121.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService()
                  .Save();

      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      long? runtimeHours = 1000;
      double? latitude = 53.756058;
      double? longitude = 8.021608;
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Monday - DateTime.UtcNow.DayOfWeek).AddDays(-14)).Date;
      DateTime testStartEventUTC = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day, 8, 0, 0);
      DateTime? eventUTC = testStartEventUTC;
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      double? engineIdleHours = 900;
      double? idleFuelGallons = 1200;
      double? consumptionGallons = 16000;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 1010;
      eventUTC = testStartEventUTC.AddDays(1);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 903;
      idleFuelGallons = 1214;
      consumptionGallons = 16030;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 1017;
      eventUTC = testStartEventUTC.AddDays(2);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = null;
      idleFuelGallons = null;
      consumptionGallons = 16050;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 1026;
      eventUTC = testStartEventUTC.AddDays(3);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 908;
      idleFuelGallons = 5490;
      consumptionGallons = 16130;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 1034;
      eventUTC = testStartEventUTC.AddDays(4);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 915;
      idleFuelGallons = 5510;
      consumptionGallons = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 1043;
      eventUTC = testStartEventUTC.AddDays(5);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 915;
      idleFuelGallons = 7650;
      consumptionGallons = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 1053;
      eventUTC = testStartEventUTC.AddDays(6);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 916;
      idleFuelGallons = 7662;
      consumptionGallons = 90000;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 1061;
      eventUTC = testStartEventUTC.AddDays(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = null;
      idleFuelGallons = null;
      consumptionGallons = 10000;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 1070;
      eventUTC = testStartEventUTC.AddDays(8);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 920;
      idleFuelGallons = 90000;
      consumptionGallons = 40000;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      runtimeHours = 1080;
      eventUTC = testStartEventUTC.AddDays(9);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      engineIdleHours = 923;
      idleFuelGallons = 90030;
      consumptionGallons = 40070;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                              where aud.ifk_DimAssetID == asset.AssetID
                                               && aud.fk_AssetPriorKeyDate != null
                                              orderby aud.fk_AssetKeyDate
                                              select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(9, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(30, util[0].TotalFuelConsumedGallons, string.Format("TotalFuelConsumed wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(14, util[0].IdleFuelConsumedGallons, string.Format("IdleFuelConsumed wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(16, util[0].WorkingFuelConsumedGallons, string.Format("WorkingFuelConsumed wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(3, util[0].AverageBurnRateGallonsPerHour, string.Format("AverageBurnRate wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(20, util[1].TotalFuelConsumedGallons, string.Format("TotalFuelConsumed wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.IsNull(util[1].IdleFuelConsumedGallons, string.Format("IdleFuelConsumed wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.IsNull(util[1].WorkingFuelConsumedGallons, string.Format("WorkingFuelConsumed wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(2.857, util[1].AverageBurnRateGallonsPerHour, string.Format("AverageBurnRate wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(80, util[2].TotalFuelConsumedGallons, string.Format("TotalFuelConsumed wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.IsNull(util[2].IdleFuelConsumedGallons, string.Format("IdleFuelConsumed wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(3, util[2].ifk_IdleFuelConsumedGallonsCalloutTypeID, string.Format("IdleFuelConsumed wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.IsNull(util[2].WorkingFuelConsumedGallons, string.Format("WorkingFuelConsumed wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(8.889,util[2].AverageBurnRateGallonsPerHour, string.Format("AverageBurnRate wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.IsNull(util[3].TotalFuelConsumedGallons, string.Format("TotalFuelConsumed wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.IsNull(util[3].IdleFuelConsumedGallons, string.Format("IdleFuelConsumed wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.IsNull(util[3].WorkingFuelConsumedGallons, string.Format("WorkingFuelConsumed wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.IsNull(util[3].AverageBurnRateGallonsPerHour, string.Format("AverageBurnRate wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.IsNull(util[4].TotalFuelConsumedGallons, string.Format("TotalFuelConsumed wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.IsNull(util[4].IdleFuelConsumedGallons, string.Format("IdleFuelConsumed wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.IsNull(util[4].WorkingFuelConsumedGallons, string.Format("WorkingFuelConsumed wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.IsNull(util[4].AverageBurnRateGallonsPerHour, string.Format("AverageBurnRate wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.IsNull(util[5].TotalFuelConsumedGallons, string.Format("TotalFuelConsumed wrong for Day:{0}", util[5].fk_AssetKeyDate));
      Assert.AreEqual(3, util[5].ifk_TotalFuelConsumedGallonsCalloutTypeID, string.Format("TotalFuelConsumed calloutType wrong for Day:{0}", util[5].fk_AssetKeyDate));
      Assert.IsNull(util[5].IdleFuelConsumedGallons, string.Format("IdleFuelConsumed wrong for Day:{0}", util[5].fk_AssetKeyDate));
      Assert.AreEqual(3, util[5].ifk_IdleFuelConsumedGallonsCalloutTypeID, string.Format("IdleFuelConsumed calloutType wrong for Day:{0}", util[5].fk_AssetKeyDate));
      Assert.IsNull(util[5].WorkingFuelConsumedGallons, string.Format("WorkingFuelConsumed wrong for Day:{0}", util[5].fk_AssetKeyDate));
      Assert.IsNull(util[5].AverageBurnRateGallonsPerHour, string.Format("AverageBurnRate wrong for Day:{0}", util[5].fk_AssetKeyDate));
      Assert.IsNull(util[6].TotalFuelConsumedGallons, string.Format("TotalFuelConsumed wrong for Day:{0}", util[6].fk_AssetKeyDate));
      Assert.AreEqual(3, util[6].ifk_TotalFuelConsumedGallonsCalloutTypeID, string.Format("TotalFuelConsumed calloutType wrong for Day:{0}", util[6].fk_AssetKeyDate));
      Assert.IsNull(util[6].IdleFuelConsumedGallons, string.Format("IdleFuelConsumed wrong for Day:{0}", util[6].fk_AssetKeyDate));
      Assert.AreEqual(1, util[6].ifk_IdleFuelConsumedGallonsCalloutTypeID, string.Format("IdleFuelConsumed calloutType wrong for Day:{0}", util[6].fk_AssetKeyDate));
      Assert.IsNull(util[6].WorkingFuelConsumedGallons, string.Format("WorkingFuelConsumed wrong for Day:{0}", util[6].fk_AssetKeyDate));
      Assert.IsNull(util[6].AverageBurnRateGallonsPerHour, string.Format("AverageBurnRate wrong for Day:{0}", util[6].fk_AssetKeyDate));
      Assert.IsNull(util[7].TotalFuelConsumedGallons, string.Format("TotalFuelConsumed wrong for Day:{0}", util[7].fk_AssetKeyDate));
      Assert.AreEqual(3, util[7].ifk_TotalFuelConsumedGallonsCalloutTypeID, string.Format("TotalFuelConsumed calloutType wrong for Day:{0}", util[7].fk_AssetKeyDate));
      Assert.IsNull(util[7].IdleFuelConsumedGallons, string.Format("IdleFuelConsumed wrong for Day:{0}", util[7].fk_AssetKeyDate));
      Assert.AreEqual(3, util[7].ifk_IdleFuelConsumedGallonsCalloutTypeID, string.Format("IdleFuelConsumed calloutType wrong for Day:{0}", util[7].fk_AssetKeyDate));
      Assert.IsNull(util[7].WorkingFuelConsumedGallons, string.Format("WorkingFuelConsumed wrong for Day:{0}", util[7].fk_AssetKeyDate));
      Assert.IsNull(util[7].AverageBurnRateGallonsPerHour, string.Format("AverageBurnRate wrong for Day:{0}", util[7].fk_AssetKeyDate));
      Assert.AreEqual(70, util[8].TotalFuelConsumedGallons, string.Format("TotalFuelConsumed wrong for Day:{0}", util[8].fk_AssetKeyDate));
      Assert.AreEqual(30, util[8].IdleFuelConsumedGallons, string.Format("IdleFuelConsumed wrong for Day:{0}", util[8].fk_AssetKeyDate));
      Assert.AreEqual(40, util[8].WorkingFuelConsumedGallons, string.Format("WorkingFuelConsumed wrong for Day:{0}", util[8].fk_AssetKeyDate));
      Assert.AreEqual(7, util[8].AverageBurnRateGallonsPerHour, string.Format("AverageBurnRate wrong for Day:{0}", util[8].fk_AssetKeyDate));
    }


    [DatabaseTest]
    [TestMethod]
    public void SmallNegativeSpikes()
    {
      // TFS16080
      // Very small (tenths) negative values resulting in a spike that invalidates a value that is needed for daily calculation.
      // These are caused by the meters reporting in different units and should be treated as zero rather than a spike.
      //  a) If Idle Hours delta is -.15, treat as a 0 delta value and anything greater should be a spike. Ditto for RTime?
      //  b) If Idle Fuel delta is -.10, treat as a 0 delta value and anything greater should be a spike.
      //  c) If Lifetime Fuel delta is -.10, treat as a 0 delta value and anything greater should be a spike.

      //   DeviceDate  Runtime   IdleTime IdleFuel TotalFuel 
      //               Meter     Meter    Meter    Meter     
      //  day1         1000.120  900.200  1200.280 1600.33   
      //  day2         1000.130  900.210  1200.290 1600.35  -- ok all incrementing slightly   
      //  day3         1000.131  900.070  1200.292 1600.40  -- 1600.45  -- idleHours -.14 should NOT be spike, should be zero //Too much fuel burned per runtime hours
      //  day4         1000.132  900.212  1200.940 1600.30  -- 1600.35  -- totalFuel -.10 should be spike
      //  day5         1000.133  900.062  1200.942 1600.37  -- idleTHours -.15 should be spike
      //  day6         1000.134  900.063  1200.842 1600.39  -- idleTFuel -.10 should be spike
      //  day7         1000.154  900.230  1201.306 1601.89  -- 1605.12  -- all is well with the universe...
      //  day8         1000.024  901.100  1201.320 1602.76  -- 1605.99  -- rtHours -.13 should NOT be spike, should be zero
      //  day9          999.874  901.102  1201.323 1603.76  -- 1606.99  -- rtHours -.15 should  be spike
      //  day10        1000.210  901.156  1201.233 1603.892 -- 1607.122 -- idleFuel -.09 should NOT be spike, should be zero
      //  day11        1000.240  901.166  1201.240 1603.793 -- 1607.032 -- totalFuel -.09 should NOT be spike, should be zero

      var target = new EquipmentAPI();
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.PL121.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService()
                  .Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      // day1
      double? runtimeHours = 1000.120;
      double? latitude = 53.756058;
      double? longitude = 8.021608;
      double? engineIdleHours = 900.200;
      double? idleFuelGallons = 1200.280;
      double? consumptionGallons = 1600.33;
      double? percentRemaining = null;
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Sunday - DateTime.UtcNow.DayOfWeek).AddDays(-14)).Date;
      DateTime testStartEventUTC = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day, 8, 0, 0);
      DateTime? eventUTC = testStartEventUTC;
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // day2
      runtimeHours = 1000.130;
      engineIdleHours = 900.210;
      idleFuelGallons = 1200.290;
      consumptionGallons = 1600.35;
      eventUTC = testStartEventUTC.AddDays(1);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // day3
      runtimeHours = 1000.131;
      engineIdleHours = 900.070;
      idleFuelGallons = 1200.292;
      consumptionGallons = 1600.40; //1600.45;
      eventUTC = testStartEventUTC.AddDays(2);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // day4
      runtimeHours = 1000.132;
      engineIdleHours = 900.212;
      idleFuelGallons = 1200.940;
      consumptionGallons = 1600.30; //1600.35;
      eventUTC = testStartEventUTC.AddDays(3);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // day5
      runtimeHours = 1000.133;
      engineIdleHours = 900.062;
      idleFuelGallons = 1200.942;
      consumptionGallons = 1600.37;
      eventUTC = testStartEventUTC.AddDays(4);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // day6
      runtimeHours = 1000.134;
      engineIdleHours = 900.063;
      idleFuelGallons = 1200.842;
      consumptionGallons = 1600.39;
      eventUTC = testStartEventUTC.AddDays(5);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // day7
      runtimeHours = 1000.154;
      engineIdleHours = 900.230;
      idleFuelGallons = 1201.306;
      consumptionGallons = 1601.89; //1605.12;
      eventUTC = testStartEventUTC.AddDays(6);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // day8
      runtimeHours = 1000.024;
      engineIdleHours = 901.100;
      idleFuelGallons = 1201.320;
      consumptionGallons = 1602.76; //1605.99;
      eventUTC = testStartEventUTC.AddDays(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // day9
      runtimeHours = 999.874;
      engineIdleHours = 901.102;
      idleFuelGallons = 1201.323;
      consumptionGallons = 1603.76; //1606.99;
      eventUTC = testStartEventUTC.AddDays(8);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // day10
      runtimeHours = 1000.210;
      engineIdleHours = 901.156;
      idleFuelGallons = 1201.233;
      consumptionGallons = 1603.892; //1607.122;
      eventUTC = testStartEventUTC.AddDays(9);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // day11
      runtimeHours = 1000.240;
      engineIdleHours = 901.166;
      idleFuelGallons = 1201.240;
      consumptionGallons = 1603.793; // 1607.032;
      eventUTC = testStartEventUTC.AddDays(10);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);



      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                              where aud.ifk_DimAssetID == asset.AssetID
                                               && aud.fk_AssetPriorKeyDate != null
                                              orderby aud.fk_AssetKeyDate
                                              select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(10, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      // day2
      Assert.AreEqual(testStartEventUTC.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("Date wrong for Day2"));
      Assert.AreEqual(0.010, Math.Round(util[0].RuntimeHours.Value, 3), string.Format("RuntimeHours delta  wrong for Day2"));
      Assert.AreEqual(0.010, Math.Round(util[0].IdleHours.Value, 3), string.Format("IdleHours delta wrong for Day2"));
      Assert.AreEqual(0.010, Math.Round(util[0].IdleFuelConsumedGallons.Value, 3), string.Format("IdleFueldelta  wrong for Day2"));
      Assert.AreEqual(0.020, Math.Round(util[0].TotalFuelConsumedGallons.Value, 3), string.Format("TotalFuel delta wrong for Day2"));

      // day3
      Assert.AreEqual(testStartEventUTC.AddDays(2).KeyDate(), util[1].fk_AssetKeyDate, string.Format("Date wrong for Day3"));
      Assert.AreEqual(0.001, Math.Round(util[1].RuntimeHours.Value, 3), string.Format("RuntimeHours delta wrong for Day3"));
      Assert.AreEqual(0.000, Math.Round(util[1].IdleHours.Value, 3), string.Format("IdleHours delta wrong for Day3"));
      Assert.AreEqual(0.002, Math.Round(util[1].IdleFuelConsumedGallons.Value, 3), string.Format("IdleFuel delta wrong for Day3"));
      Assert.AreEqual(0.050, Math.Round(util[1].TotalFuelConsumedGallons.Value, 3), string.Format("TotalFuel delta wrong for Day3"));

      // day4
      Assert.AreEqual(testStartEventUTC.AddDays(3).KeyDate(), util[2].fk_AssetKeyDate, string.Format("Date wrong for Day4"));
      Assert.AreEqual(0.001, Math.Round(util[2].RuntimeHours.Value, 3), string.Format("RuntimeHours delta wrong for Day4"));
      Assert.AreEqual(0.142, Math.Round(util[2].IdleHours.Value, 3), string.Format("IdleHours delta wrong for Day4"));
      Assert.AreEqual(0.648, Math.Round(util[2].IdleFuelConsumedGallons.Value, 3), string.Format("IdleFuel delta wrong for Day4"));
      Assert.IsNull(util[2].TotalFuelConsumedGallons, string.Format("TotalFuel delta wrong for Day4"));
      Assert.AreEqual(3, util[2].ifk_TotalFuelConsumedGallonsCalloutTypeID, string.Format("TotalFuel calloutType wrong for Day4"));

      // day5
      Assert.AreEqual(testStartEventUTC.AddDays(4).KeyDate(), util[3].fk_AssetKeyDate, string.Format("Date wrong for Day5"));
      Assert.AreEqual(0.001, Math.Round(util[3].RuntimeHours.Value, 3), string.Format("RuntimeHours delta wrong for Day5"));
      Assert.IsNull(util[3].IdleHours, string.Format("IdleHours delta wrong for Day5"));
      Assert.AreEqual(3, util[3].ifk_IdleHoursCalloutTypeID, string.Format("IdleHours calloutType wrong for Day5"));
      Assert.AreEqual(0.002, Math.Round(util[3].IdleFuelConsumedGallons.Value, 3), string.Format("IdleFuel delta wrong for Day5"));
      Assert.AreEqual(0.070, Math.Round(util[3].TotalFuelConsumedGallons.Value, 3), string.Format("TotalFuel delta wrong for Day5"));

      // day6
      Assert.AreEqual(testStartEventUTC.AddDays(5).KeyDate(), util[4].fk_AssetKeyDate, string.Format("Date wrong for Day6"));
      Assert.AreEqual(0.001, Math.Round(util[4].RuntimeHours.Value, 3), string.Format("RuntimeHours delta wrong for Day6"));
      Assert.AreEqual(0.001, Math.Round(util[4].IdleHours.Value, 3), string.Format("IdleHours delta wrong for Day6"));
      Assert.IsNull(util[4].IdleFuelConsumedGallons, string.Format("IdleFuel delta wrong for Day6"));
      Assert.AreEqual(3, util[4].ifk_IdleFuelConsumedGallonsCalloutTypeID, string.Format("IdleFuel callout Type wrong for Day6"));
      Assert.AreEqual(0.020, Math.Round(util[4].TotalFuelConsumedGallons.Value, 3), string.Format("TotalFuel delta wrong for Day6"));

      // day7
      Assert.AreEqual(testStartEventUTC.AddDays(6).KeyDate(), util[5].fk_AssetKeyDate, string.Format("Date wrong for Day7"));
      Assert.AreEqual(0.020, Math.Round(util[5].RuntimeHours.Value, 3), string.Format("RuntimeHours delta wrong for Day7"));
      Assert.AreEqual(0.167, Math.Round(util[5].IdleHours.Value, 3), string.Format("IdleHours delta wrong for Day7"));
      Assert.AreEqual(0.464, Math.Round(util[5].IdleFuelConsumedGallons.Value, 3), string.Format("IdleFuel delta wrong for Day7"));
      Assert.AreEqual(1.50, Math.Round(util[5].TotalFuelConsumedGallons.Value, 3), string.Format("TotalFuel delta wrong for Day7"));

      // day8
      Assert.AreEqual(testStartEventUTC.AddDays(7).KeyDate(), util[6].fk_AssetKeyDate, string.Format("Date wrong for Day8"));
      Assert.AreEqual(0.000, Math.Round(util[6].RuntimeHours.Value, 3), string.Format("RuntimeHours delta wrong for Day8"));
      Assert.AreEqual(0.870, Math.Round(util[6].IdleHours.Value, 3), string.Format("IdleHours delta wrong for Day8"));
      Assert.AreEqual(0.014, Math.Round(util[6].IdleFuelConsumedGallons.Value, 3), string.Format("IdleFuel delta wrong for Day8"));
      Assert.AreEqual(0.87, Math.Round(util[6].TotalFuelConsumedGallons.Value, 3), string.Format("TotalFuel delta wrong for Day8"));

      // day9
      Assert.AreEqual(testStartEventUTC.AddDays(8).KeyDate(), util[7].fk_AssetKeyDate, string.Format("Date wrong for Day9"));
      Assert.IsNull(util[7].RuntimeHours, string.Format("RuntimeHours delta wrong for Day9"));
      Assert.AreEqual(3, util[7].ifk_RuntimeHoursCalloutTypeID, string.Format("RuntimeHours calloutType wrong for Day9"));
      Assert.AreEqual(0.002, Math.Round(util[7].IdleHours.Value, 3), string.Format("IdleHours delta wrong for Day9"));
      Assert.AreEqual(0.003, Math.Round(util[7].IdleFuelConsumedGallons.Value, 3), string.Format("IdleFuel delta wrong for Day9"));
      Assert.AreEqual(1.00, Math.Round(util[7].TotalFuelConsumedGallons.Value, 3), string.Format("TotalFuel delta wrong for Day9"));

      // day10
      Assert.AreEqual(testStartEventUTC.AddDays(9).KeyDate(), util[8].fk_AssetKeyDate, string.Format("Date wrong for Day10"));
      Assert.AreEqual(0.336, Math.Round(util[8].RuntimeHours.Value, 3), string.Format("RuntimeHours delta wrong for Day10"));
      Assert.AreEqual(0.054, Math.Round(util[8].IdleHours.Value, 3), string.Format("IdleHours delta wrong for Day10"));
      Assert.AreEqual(0.000, Math.Round(util[8].IdleFuelConsumedGallons.Value, 3), string.Format("IdleFuel delta wrong for Day10"));
      Assert.AreEqual(0.132, Math.Round(util[8].TotalFuelConsumedGallons.Value, 3), string.Format("TotalFuel delta wrong for Day10"));

      // day11
      Assert.AreEqual(testStartEventUTC.AddDays(10).KeyDate(), util[9].fk_AssetKeyDate, string.Format("Date wrong for Day11"));
      Assert.AreEqual(0.030, Math.Round(util[9].RuntimeHours.Value, 3), string.Format("RuntimeHours delta wrong for Day11"));
      Assert.AreEqual(0.010, Math.Round(util[9].IdleHours.Value, 3), string.Format("IdleHours delta wrong for Day11"));
      Assert.AreEqual(0.007, Math.Round(util[9].IdleFuelConsumedGallons.Value, 3), string.Format("IdleFuel delta wrong for Day11"));
      Assert.AreEqual(0.000, Math.Round(util[9].TotalFuelConsumedGallons.Value, 3), string.Format("TotalFuel delta wrong for Day11"));

    }

    #endregion

    [TestMethod()]
    [DatabaseTest]
      [Ignore]
    public void UtilizationETLTest_Bug17778_DeviceSupportsIdleButHasNeverReportedIdleHasDailyReport()
    {
      var assetSupportsIdle = SetupDefaultAsset(TestData.TestMTS522);

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      // Add a DataEngineParameter entry with an IdleFuelgallons of 0 to make sure we have one. The sproc changes to null so not a valid test.
      var day1EngineParameters = new DataEngineParameters();
      day1EngineParameters.EventUTC = testStartDeviceDay;
      day1EngineParameters.AssetID = assetSupportsIdle.AssetID;
      day1EngineParameters.EngineIdleHours = 0;
      day1EngineParameters.ConsumptionGallons = 0;
      day1EngineParameters.LevelPercent = 75;
      day1EngineParameters.IdleFuelGallons = 0;
      Ctx.DataContext.DataEngineParameters.AddObject(day1EngineParameters);
      Ctx.DataContext.SaveChanges();

      SetupNH_DATAForDay(assetSupportsIdle.AssetID, startOfDeviceDay,
          day: 1, runtimeHours: 8, idleHours: null, totalFuel: null, idleFuel: null);

      SetupNH_DATAForDay(assetSupportsIdle.AssetID, startOfDeviceDay,
          day: 2, runtimeHours: 17, idleHours: null, totalFuel: null, idleFuel: null);

      SetupNH_DATAForDay(assetSupportsIdle.AssetID, startOfDeviceDay,
          day: 3, runtimeHours: 24, idleHours: null, totalFuel: null, idleFuel: null);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      
      List<FactAssetUtilizationDaily> fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                               where aud.ifk_DimAssetID == assetSupportsIdle.AssetID
                                                && aud.fk_AssetPriorKeyDate != null
                                               orderby aud.fk_AssetKeyDate
                                               select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(2, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      // Assert that daily report is created even though device supports idle but no idle value has ever been reported
      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay,
          day: 2, runtimeHoursDelta: 9, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: null, idleFuelDelta: null, workingFuelDelta: null);
      
      var assetDoesNotSupportIdle = SetupDefaultAsset(TestData.TestPL121, "ABC121");

      SetupNH_DATAForDay(assetDoesNotSupportIdle.AssetID, startOfDeviceDay,
          day: 1, runtimeHours: 8, idleHours: null, totalFuel: null, idleFuel: null);

      SetupNH_DATAForDay(assetDoesNotSupportIdle.AssetID, startOfDeviceDay,
          day: 2, runtimeHours: 17, idleHours: null, totalFuel: null, idleFuel: null);

      SetupNH_DATAForDay(assetDoesNotSupportIdle.AssetID, startOfDeviceDay,
          day: 3, runtimeHours: 24, idleHours: null, totalFuel: null, idleFuel: null);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
               where aud.ifk_DimAssetID == assetDoesNotSupportIdle.AssetID
                 && aud.fk_AssetPriorKeyDate != null
                orderby aud.fk_AssetKeyDate
                select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(2, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      // Assert that daily report is created for device that doesn't support idle but no idle value has ever been reported
      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay,
          day: 2, runtimeHoursDelta: 9, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: null, idleFuelDelta: null, workingFuelDelta: null);
    }
  }
}
