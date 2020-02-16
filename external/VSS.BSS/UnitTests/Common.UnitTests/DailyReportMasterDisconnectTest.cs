using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{

  /// <summary>
  ///This is a test class for DailyReportMasterDisconnectTest and is intended
  ///to contain all DailyReportMasterDisconnectTest Unit Tests
  ///</summary>
  [TestClass()]
  public class DailyReportMasterDisconnectTest : ReportLogicTestBase
  { 

    [TestMethod()]
    [DatabaseTest]
      [Ignore]
    public void DailyReportMDTest_NormalConditionWithVariances()
    {
      // test symptom of MasterDisconnect i.e. day/s missing; bounding DailyReports (+/- 1 hour); multiple reports on subsequent day; runtime increments
      Device device = TestData.TestPL321;
      Customer customer = (from c in Ctx.OpContext.Customer where c.BSSID == device.OwnerBSSID select c).FirstOrDefault();
      Asset asset = Entity.Asset.SerialNumberVin("ABC123").WithDevice(device).WithCoreService().WithDefaultAssetUtilizationSettings().SyncWithRpt().Save();
      long assetID = asset.AssetID;
      var burnRates = Entity.AssetBurnRates.ForAsset(assetID)
         .EstimatedIdleBurnRateGallonsPerHour(5.0)
         .EstimatedWorkingBurnRateGallonsPerHour(10.0).Save();
      var workDef = Entity.AssetWorkingDefinition.ForAsset(assetID)
         .WorkDefinition(WorkDefinitionEnum.MeterDelta) 
         .Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 531;
      double? latitude = 20; // GMT - so we don't have to worry about timezone offsets
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(13).AddMinutes(30);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      double? engineIdleHours = 338;
      double? idleFuelGallons = 433;
      double? consumptionGallons = 2874;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(1);
      runtimeHours = 535;
      eventUTC = startOfDeviceDay.AddHours(9).AddMinutes(4);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 340;
      idleFuelGallons = 439;
      consumptionGallons = 2880;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(2); //11th
      runtimeHours = 540;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(58);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 345;
      idleFuelGallons = 442;
      consumptionGallons = 2890;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(4); //4th day early
      runtimeHours = 556;
      eventUTC = startOfDeviceDay.AddHours(4).AddMinutes(11);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 349;
      idleFuelGallons = 445;
      consumptionGallons = 2900;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(4); //4th dailyReport time
      runtimeHours = 559;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(12);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 354;
      idleFuelGallons = 452;
      consumptionGallons = 2914;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);


      startOfDeviceDay = testStartDeviceDay.AddDays(5); //15th 
      runtimeHours = 569;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(24);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 363;
      idleFuelGallons = 466;
      consumptionGallons = 2923;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();


      List<HoursLocation> hl = (from hl1 in Ctx.RptContext.HoursLocationReadOnly
                                where hl1.ifk_DimAssetID == asset.AssetID
                                orderby hl1.EventUTC
                                select hl1).ToList<HoursLocation>();
      Assert.AreEqual(6, hl.Count(), "Incorrect HoursLocation record 1st count."); 

      ExecuteMeterDeltaTransformScript();
      ExecuteMeterDeltaTransformScript(); // needs to be run twice, the 2nd run will process the emulated day

      // make sure we have picked up the new emulated raw event
      hl = (from hl1 in Ctx.RptContext.HoursLocationReadOnly
            where hl1.ifk_DimAssetID == asset.AssetID
            orderby hl1.EventUTC
            select hl1).ToList<HoursLocation>();
      Assert.AreEqual(7, hl.Count(), "Incorrect HoursLocation record 2nd count."); // should be 1 extra for the emulated one

      // ensure we have the 4 normal days PLUS the emulated day between days 2 and 4
      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                              where aud.ifk_DimAssetID == asset.AssetID
                                               && aud.fk_AssetPriorKeyDate != null
                                              orderby aud.fk_AssetKeyDate
                                              select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(5, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartDeviceDay.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day0"));
      Assert.AreEqual(4, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day0"));
      Assert.AreEqual(2, util[0].IdleHours, string.Format("Idle hours incorrect for Day0"));
      Assert.AreEqual(testStartDeviceDay.AddDays(1).AddHours(9).AddMinutes(4), util[0].EventUTC, string.Format("source EventUTC wrong for Day0"));

      Assert.AreEqual(testStartDeviceDay.AddDays(2).KeyDate(), util[1].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day1"));
      Assert.AreEqual(5, util[1].RuntimeHours, string.Format("Runtime hours incorrect for Day1"));
      Assert.AreEqual(5, util[1].IdleHours, string.Format("Idle hours incorrect for Day1"));
      Assert.AreEqual(testStartDeviceDay.AddDays(2).AddHours(7).AddMinutes(58), util[1].EventUTC, string.Format("source EventUTC wrong for Day1"));

      // this is the event which was moved from early on the 5th day to this 4th day
      Assert.AreEqual(testStartDeviceDay.AddDays(3).KeyDate(), util[2].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day2"));
      Assert.AreEqual(16, util[2].RuntimeHours, string.Format("Runtime hours incorrect for Day2"));
      Assert.AreEqual(4, util[2].IdleHours, string.Format("Idle hours incorrect for Day2"));
      Assert.AreEqual(testStartDeviceDay.AddDays(3).AddHours(7).AddMinutes(58), util[2].EventUTC, string.Format("source EventUTC wrong for Day2"));

      // this should now delta from the emulated day on day4 instead of previously it would have deltas with day3
      Assert.AreEqual(testStartDeviceDay.AddDays(4).KeyDate(), util[3].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day3"));
      Assert.AreEqual(3, util[3].RuntimeHours, string.Format("Runtime hours incorrect for Day3"));
      Assert.AreEqual(5, util[3].IdleHours, string.Format("Idle hours incorrect for Day3"));
      Assert.AreEqual(testStartDeviceDay.AddDays(4).AddHours(7).AddMinutes(12), util[3].EventUTC, string.Format("source EventUTC wrong for Day3"));

      Assert.AreEqual(testStartDeviceDay.AddDays(5).KeyDate(), util[4].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day4"));
      Assert.AreEqual(10, util[4].RuntimeHours, string.Format("Runtime hours incorrect for Day4"));
      Assert.AreEqual(9, util[4].IdleHours, string.Format("Idle hours incorrect for Day4"));
      Assert.AreEqual(testStartDeviceDay.AddDays(5).AddHours(7).AddMinutes(24), util[4].EventUTC, string.Format("source EventUTC wrong for Day4"));
    }

    [TestMethod()]
    [DatabaseTest]
      [Ignore]
    public void DailyReportMDTest_NormalConditionWithFalsePositives()
    {    
      // test symptom of MasterDisconnect i.e. day/s missing; bounding DailyReports (+/- 1 hour); multiple reports on subsequent day; runtime does NOT increment
      Device device = TestData.TestPL321;
      Customer customer = (from c in Ctx.OpContext.Customer where c.BSSID == device.OwnerBSSID select c).FirstOrDefault();
      Asset asset = Entity.Asset.SerialNumberVin("ABC123").WithDevice(device).WithCoreService().WithDefaultAssetUtilizationSettings().SyncWithRpt().Save();
      long assetID = asset.AssetID;
      var burnRates = Entity.AssetBurnRates.ForAsset(assetID)
         .EstimatedIdleBurnRateGallonsPerHour(5.0)
         .EstimatedWorkingBurnRateGallonsPerHour(10.0).Save();
      var workDef = Entity.AssetWorkingDefinition.ForAsset(assetID)
         .WorkDefinition(WorkDefinitionEnum.MeterDelta)
         .Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 531;
      double? latitude = 20; // GMT - so we don't have to worry about timezone offsets
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(13).AddMinutes(30);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      double? engineIdleHours = 338;
      double? idleFuelGallons = 433;
      double? consumptionGallons = 2874;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(1);
      runtimeHours = 535;
      eventUTC = startOfDeviceDay.AddHours(9).AddMinutes(4);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 340;
      idleFuelGallons = 439;
      consumptionGallons = 2880;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(2); //11th
      runtimeHours = 540;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(58);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 345;
      idleFuelGallons = 442;
      consumptionGallons = 2890;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(4); //4th day early - runtime does NOT increment
      runtimeHours = 540;
      eventUTC = startOfDeviceDay.AddHours(4).AddMinutes(11);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 349;
      idleFuelGallons = 445;
      consumptionGallons = 2900;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(4); //4th dailyReport time
      runtimeHours = 559;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(12);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 354;
      idleFuelGallons = 452;
      consumptionGallons = 2914;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);


      startOfDeviceDay = testStartDeviceDay.AddDays(5); //15th 
      runtimeHours = 569;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(24);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 363;
      idleFuelGallons = 466;
      consumptionGallons = 2923;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();


      List<HoursLocation> hl = (from hl1 in Ctx.RptContext.HoursLocationReadOnly
                                where hl1.ifk_DimAssetID == asset.AssetID
                                orderby hl1.EventUTC
                                select hl1).ToList<HoursLocation>();
      Assert.AreEqual(6, hl.Count(), "Incorrect HoursLocation record 1st count.");

      ExecuteMeterDeltaTransformScript();
      ExecuteMeterDeltaTransformScript(); // needs to be run twice, the 2nd run will process the emulated day

      // make sure we have picked up the new emulated raw event
      hl = (from hl1 in Ctx.RptContext.HoursLocationReadOnly
            where hl1.ifk_DimAssetID == asset.AssetID
            orderby hl1.EventUTC
            select hl1).ToList<HoursLocation>();
      Assert.AreEqual(6, hl.Count(), "Incorrect HoursLocation record 2nd count."); // no emulated event here

      // ensure we have the 4 normal days PLUS the emulated day between days 2 and 4
      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                              where aud.ifk_DimAssetID == asset.AssetID
                                               && aud.fk_AssetPriorKeyDate != null
                                              orderby aud.fk_AssetKeyDate
                                              select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(4, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartDeviceDay.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day0"));
      Assert.AreEqual(4, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day0"));
      Assert.AreEqual(2, util[0].IdleHours, string.Format("Idle hours incorrect for Day0"));
      Assert.AreEqual(testStartDeviceDay.AddDays(1).AddHours(9).AddMinutes(4), util[0].EventUTC, string.Format("source EventUTC wrong for Day0"));

      Assert.AreEqual(testStartDeviceDay.AddDays(2).KeyDate(), util[1].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day1"));
      Assert.AreEqual(5, util[1].RuntimeHours, string.Format("Runtime hours incorrect for Day1"));
      Assert.AreEqual(5, util[1].IdleHours, string.Format("Idle hours incorrect for Day1"));
      Assert.AreEqual(testStartDeviceDay.AddDays(2).AddHours(7).AddMinutes(58), util[1].EventUTC, string.Format("source EventUTC wrong for Day1"));

      // this is the event which should NOT be moved from early on the 5th day to the 4th day
      Assert.AreEqual(testStartDeviceDay.AddDays(4).KeyDate(), util[2].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day2"));
      Assert.AreEqual(19, util[2].RuntimeHours, string.Format("Runtime hours incorrect for Day2"));
      Assert.AreEqual(9, util[2].IdleHours, string.Format("Idle hours incorrect for Day2"));
      Assert.AreEqual(testStartDeviceDay.AddDays(4).AddHours(7).AddMinutes(12), util[2].EventUTC, string.Format("source EventUTC wrong for Day2"));
   
      Assert.AreEqual(testStartDeviceDay.AddDays(5).KeyDate(), util[3].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day4"));
      Assert.AreEqual(10, util[3].RuntimeHours, string.Format("Runtime hours incorrect for Day4"));
      Assert.AreEqual(9, util[3].IdleHours, string.Format("Idle hours incorrect for Day4"));
      Assert.AreEqual(testStartDeviceDay.AddDays(5).AddHours(7).AddMinutes(24), util[3].EventUTC, string.Format("source EventUTC wrong for Day3"));       
    }

    [TestMethod()]
    [DatabaseTest]
      [Ignore]
    public void DailyReportMDTest_ErrorConditionWithTwoDayGap()
    {     
      // test symptom of MasterDisconnect i.e. day/s missing; bounding DailyReports (+/- 1 hour); multiple reports on subsequent day; runtime increments
      Device device = TestData.TestPL321;
      Customer customer = (from c in Ctx.OpContext.Customer where c.BSSID == device.OwnerBSSID select c).FirstOrDefault();
      Asset asset = Entity.Asset.SerialNumberVin("ABC123").WithDevice(device).WithCoreService().WithDefaultAssetUtilizationSettings().SyncWithRpt().Save();
      long assetID = asset.AssetID;
      var burnRates = Entity.AssetBurnRates.ForAsset(assetID)
         .EstimatedIdleBurnRateGallonsPerHour(5.0)
         .EstimatedWorkingBurnRateGallonsPerHour(10.0).Save();
      var workDef = Entity.AssetWorkingDefinition.ForAsset(assetID)
         .WorkDefinition(WorkDefinitionEnum.MeterDelta)
         .Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 531;
      double? latitude = 20; // GMT - so we don't have to worry about timezone offsets
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(13).AddMinutes(30);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      double? engineIdleHours = 338;
      double? idleFuelGallons = 433;
      double? consumptionGallons = 2874;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(1);
      runtimeHours = 535;
      eventUTC = startOfDeviceDay.AddHours(9).AddMinutes(4);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 340;
      idleFuelGallons = 439;
      consumptionGallons = 2880;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(2); //11th
      runtimeHours = 540;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(58);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 345;
      idleFuelGallons = 442;
      consumptionGallons = 2890;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(5); //4th day early
      runtimeHours = 556;
      eventUTC = startOfDeviceDay.AddHours(4).AddMinutes(11);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 349;
      idleFuelGallons = 445;
      consumptionGallons = 2900;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(5); //4th dailyReport time
      runtimeHours = 559;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(12);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 354;
      idleFuelGallons = 452;
      consumptionGallons = 2914;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);


      startOfDeviceDay = testStartDeviceDay.AddDays(6); //15th 
      runtimeHours = 569;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(24);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 363;
      idleFuelGallons = 466;
      consumptionGallons = 2923;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();


      List<HoursLocation> hl = (from hl1 in Ctx.RptContext.HoursLocationReadOnly
                                where hl1.ifk_DimAssetID == asset.AssetID
                                orderby hl1.EventUTC
                                select hl1).ToList<HoursLocation>();
      Assert.AreEqual(6, hl.Count(), "Incorrect HoursLocation record 1st count.");

      ExecuteMeterDeltaTransformScript();
      ExecuteMeterDeltaTransformScript(); // needs to be run twice, the 2nd run will process the emulated day

      // make sure we have picked up the new emulated raw event
      hl = (from hl1 in Ctx.RptContext.HoursLocationReadOnly
            where hl1.ifk_DimAssetID == asset.AssetID
            orderby hl1.EventUTC
            select hl1).ToList<HoursLocation>();
      Assert.AreEqual(7, hl.Count(), "Incorrect HoursLocation record 2nd count."); // should be 1 extra for the emulated one

      // ensure we have the 4 normal days PLUS the emulated day between days 2 and 4
      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                              where aud.ifk_DimAssetID == asset.AssetID
                                               && aud.fk_AssetPriorKeyDate != null
                                              orderby aud.fk_AssetKeyDate
                                              select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(5, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartDeviceDay.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day0"));
      Assert.AreEqual(4, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day0"));
      Assert.AreEqual(2, util[0].IdleHours, string.Format("Idle hours incorrect for Day0"));
      Assert.AreEqual(testStartDeviceDay.AddDays(1).AddHours(9).AddMinutes(4), util[0].EventUTC, string.Format("source EventUTC wrong for Day0"));

      Assert.AreEqual(testStartDeviceDay.AddDays(2).KeyDate(), util[1].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day1"));
      Assert.AreEqual(5, util[1].RuntimeHours, string.Format("Runtime hours incorrect for Day1"));
      Assert.AreEqual(5, util[1].IdleHours, string.Format("Idle hours incorrect for Day1"));
      Assert.AreEqual(testStartDeviceDay.AddDays(2).AddHours(7).AddMinutes(58), util[1].EventUTC, string.Format("source EventUTC wrong for Day1"));

      // this is the event which was moved from early on the 5th day to this 4th day
      Assert.AreEqual(testStartDeviceDay.AddDays(3).KeyDate(), util[2].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day2"));
      Assert.AreEqual(16, util[2].RuntimeHours, string.Format("Runtime hours incorrect for Day2"));
      Assert.AreEqual(4, util[2].IdleHours, string.Format("Idle hours incorrect for Day2"));
      Assert.AreEqual(testStartDeviceDay.AddDays(3).AddHours(7).AddMinutes(58), util[2].EventUTC, string.Format("source EventUTC wrong for Day2"));

      // this should now delta from the emulated day on day4 instead of previously it would have deltas with day3
      Assert.AreEqual(testStartDeviceDay.AddDays(5).KeyDate(), util[3].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day3"));
      Assert.AreEqual(3, util[3].RuntimeHours, string.Format("Runtime hours incorrect for Day3"));
      Assert.AreEqual(5, util[3].IdleHours, string.Format("Idle hours incorrect for Day3"));
      Assert.AreEqual(testStartDeviceDay.AddDays(5).AddHours(7).AddMinutes(12), util[3].EventUTC, string.Format("source EventUTC wrong for Day3"));

      Assert.AreEqual(testStartDeviceDay.AddDays(6).KeyDate(), util[4].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day4"));
      Assert.AreEqual(10, util[4].RuntimeHours, string.Format("Runtime hours incorrect for Day4"));
      Assert.AreEqual(9, util[4].IdleHours, string.Format("Idle hours incorrect for Day4"));
      Assert.AreEqual(testStartDeviceDay.AddDays(6).AddHours(7).AddMinutes(24), util[4].EventUTC, string.Format("source EventUTC wrong for Day4"));
    }

    [TestMethod()]
    [DatabaseTest]
      [Ignore]
    public void DailyReportMDTest_ErrorConditionWithTwoDayGapNonPL321Device()
    {
      // test symptom of MasterDisconnect i.e. day/s missing; bounding DailyReports (+/- 1 hour); multiple reports on subsequent day; runtime increments
      Device device = TestData.TestMTS522;
      Customer customer = (from c in Ctx.OpContext.Customer where c.BSSID == device.OwnerBSSID select c).FirstOrDefault();
      Asset asset = Entity.Asset.SerialNumberVin("ABC123").WithDevice(device).WithCoreService().WithDefaultAssetUtilizationSettings().SyncWithRpt().Save();
      long assetID = asset.AssetID;
      var burnRates = Entity.AssetBurnRates.ForAsset(assetID)
         .EstimatedIdleBurnRateGallonsPerHour(5.0)
         .EstimatedWorkingBurnRateGallonsPerHour(10.0).Save();
      var workDef = Entity.AssetWorkingDefinition.ForAsset(assetID)
         .WorkDefinition(WorkDefinitionEnum.MeterDelta)
         .Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 531;
      double? latitude = 20; // GMT - so we don't have to worry about timezone offsets
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(13).AddMinutes(30);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      double? engineIdleHours = 338;
      double? idleFuelGallons = 433;
      double? consumptionGallons = 2874;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(1);
      runtimeHours = 535;
      eventUTC = startOfDeviceDay.AddHours(9).AddMinutes(4);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 340;
      idleFuelGallons = 439;
      consumptionGallons = 2880;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(2); //11th
      runtimeHours = 540;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(58);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 345;
      idleFuelGallons = 442;
      consumptionGallons = 2890;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(5); //4th day early
      runtimeHours = 556;
      eventUTC = startOfDeviceDay.AddHours(4).AddMinutes(11);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 349;
      idleFuelGallons = 445;
      consumptionGallons = 2900;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(5); //4th dailyReport time
      runtimeHours = 559;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(12);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 354;
      idleFuelGallons = 452;
      consumptionGallons = 2914;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);


      startOfDeviceDay = testStartDeviceDay.AddDays(6); //15th 
      runtimeHours = 569;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(24);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 363;
      idleFuelGallons = 466;
      consumptionGallons = 2923;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();


      List<HoursLocation> hl = (from hl1 in Ctx.RptContext.HoursLocationReadOnly
                                where hl1.ifk_DimAssetID == asset.AssetID
                                orderby hl1.EventUTC
                                select hl1).ToList<HoursLocation>();
      Assert.AreEqual(6, hl.Count(), "Incorrect HoursLocation record 1st count.");

      ExecuteMeterDeltaTransformScript();
      ExecuteMeterDeltaTransformScript(); // needs to be run twice, the 2nd run will process the emulated day

      // make sure we have picked up the new emulated raw event
      hl = (from hl1 in Ctx.RptContext.HoursLocationReadOnly
            where hl1.ifk_DimAssetID == asset.AssetID
            orderby hl1.EventUTC
            select hl1).ToList<HoursLocation>();
      Assert.AreEqual(7, hl.Count(), "Incorrect HoursLocation record 2nd count."); // should be 1 extra for the emulated one

      // ensure we have the 4 normal days PLUS the emulated day between days 2 and 4
      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                              where aud.ifk_DimAssetID == asset.AssetID
                                               && aud.fk_AssetPriorKeyDate != null
                                              orderby aud.fk_AssetKeyDate
                                              select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(5, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartDeviceDay.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day0"));
      Assert.AreEqual(4, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day0"));
      Assert.AreEqual(2, util[0].IdleHours, string.Format("Idle hours incorrect for Day0"));
      Assert.AreEqual(testStartDeviceDay.AddDays(1).AddHours(9).AddMinutes(4), util[0].EventUTC, string.Format("source EventUTC wrong for Day0"));

      Assert.AreEqual(testStartDeviceDay.AddDays(2).KeyDate(), util[1].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day1"));
      Assert.AreEqual(5, util[1].RuntimeHours, string.Format("Runtime hours incorrect for Day1"));
      Assert.AreEqual(5, util[1].IdleHours, string.Format("Idle hours incorrect for Day1"));
      Assert.AreEqual(testStartDeviceDay.AddDays(2).AddHours(7).AddMinutes(58), util[1].EventUTC, string.Format("source EventUTC wrong for Day1"));

      // this is the event which was moved from early on the 5th day to this 4th day
      Assert.AreEqual(testStartDeviceDay.AddDays(3).KeyDate(), util[2].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day2"));
      Assert.AreEqual(16, util[2].RuntimeHours, string.Format("Runtime hours incorrect for Day2"));
      Assert.AreEqual(4, util[2].IdleHours, string.Format("Idle hours incorrect for Day2"));
      Assert.AreEqual(testStartDeviceDay.AddDays(3).AddHours(7).AddMinutes(58), util[2].EventUTC, string.Format("source EventUTC wrong for Day2"));

      // this should now delta from the emulated day on day4 instead of previously it would have deltas with day3
      Assert.AreEqual(testStartDeviceDay.AddDays(5).KeyDate(), util[3].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day3"));
      Assert.AreEqual(3, util[3].RuntimeHours, string.Format("Runtime hours incorrect for Day3"));
      Assert.AreEqual(5, util[3].IdleHours, string.Format("Idle hours incorrect for Day3"));
      Assert.AreEqual(testStartDeviceDay.AddDays(5).AddHours(7).AddMinutes(12), util[3].EventUTC, string.Format("source EventUTC wrong for Day3"));

      Assert.AreEqual(testStartDeviceDay.AddDays(6).KeyDate(), util[4].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day4"));
      Assert.AreEqual(10, util[4].RuntimeHours, string.Format("Runtime hours incorrect for Day4"));
      Assert.AreEqual(9, util[4].IdleHours, string.Format("Idle hours incorrect for Day4"));
      Assert.AreEqual(testStartDeviceDay.AddDays(6).AddHours(7).AddMinutes(24), util[4].EventUTC, string.Format("source EventUTC wrong for Day4"));
    }


    [TestMethod()]
    [DatabaseTest]
    [Ignore]
    public void DailyReportMDTest_ErrorConditionWithMonthDayGap()
    {
      // test symptom of MasterDisconnect i.e. day/s missing; bounding DailyReports (+/- 1 hour); multiple reports on subsequent day; runtime increments
      Device device = TestData.TestPL321;
      Customer customer = (from c in Ctx.OpContext.Customer where c.BSSID == device.OwnerBSSID select c).FirstOrDefault();
      Asset asset = Entity.Asset.SerialNumberVin("ABC123").WithDevice(device).WithCoreService().WithDefaultAssetUtilizationSettings().SyncWithRpt().Save();
      long assetID = asset.AssetID;
      var burnRates = Entity.AssetBurnRates.ForAsset(assetID)
         .EstimatedIdleBurnRateGallonsPerHour(5.0)
         .EstimatedWorkingBurnRateGallonsPerHour(10.0).Save();
      var workDef = Entity.AssetWorkingDefinition.ForAsset(assetID)
         .WorkDefinition(WorkDefinitionEnum.MeterDelta)
         .Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = DateTime.UtcNow.AddDays(-58).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 531;
      double? latitude = 20; // GMT - so we don't have to worry about timezone offsets
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(13).AddMinutes(30);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      double? engineIdleHours = 338;
      double? idleFuelGallons = 433;
      double? consumptionGallons = 2874;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(1);
      runtimeHours = 535;
      eventUTC = startOfDeviceDay.AddHours(9).AddMinutes(4);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 340;
      idleFuelGallons = 439;
      consumptionGallons = 2880;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(2); //11th
      runtimeHours = 540;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(58);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 345;
      idleFuelGallons = 442;
      consumptionGallons = 2890;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(35); //4th day early
      runtimeHours = 556;
      eventUTC = startOfDeviceDay.AddHours(4).AddMinutes(11);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 349;
      idleFuelGallons = 445;
      consumptionGallons = 2900;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(35); //4th dailyReport time
      runtimeHours = 559;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(12);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 354;
      idleFuelGallons = 452;
      consumptionGallons = 2914;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);


      startOfDeviceDay = testStartDeviceDay.AddDays(36); //15th 
      runtimeHours = 569;
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(24);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 363;
      idleFuelGallons = 466;
      consumptionGallons = 2923;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();


      List<HoursLocation> hl = (from hl1 in Ctx.RptContext.HoursLocationReadOnly
                                where hl1.ifk_DimAssetID == asset.AssetID
                                orderby hl1.EventUTC
                                select hl1).ToList<HoursLocation>();
      Assert.AreEqual(6, hl.Count(), "Incorrect HoursLocation record 1st count.");

      ExecuteMeterDeltaTransformScript();
      ExecuteMeterDeltaTransformScript(); // needs to be run twice, the 2nd run will process the emulated day

      // make sure we have picked up the new emulated raw event
      hl = (from hl1 in Ctx.RptContext.HoursLocationReadOnly
            where hl1.ifk_DimAssetID == asset.AssetID
            orderby hl1.EventUTC
            select hl1).ToList<HoursLocation>();
      Assert.AreEqual(7, hl.Count(), "Incorrect HoursLocation record 2nd count."); // should be 1 extra for the emulated one

      // ensure we have the 4 normal days PLUS the emulated day between days 2 and 4
      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                              where aud.ifk_DimAssetID == asset.AssetID
                                               && aud.fk_AssetPriorKeyDate != null
                                              orderby aud.fk_AssetKeyDate
                                              select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(5, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartDeviceDay.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day0"));
      Assert.AreEqual(4, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day0"));
      Assert.AreEqual(2, util[0].IdleHours, string.Format("Idle hours incorrect for Day0"));
      Assert.AreEqual(testStartDeviceDay.AddDays(1).AddHours(9).AddMinutes(4), util[0].EventUTC, string.Format("source EventUTC wrong for Day0"));

      Assert.AreEqual(testStartDeviceDay.AddDays(2).KeyDate(), util[1].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day1"));
      Assert.AreEqual(5, util[1].RuntimeHours, string.Format("Runtime hours incorrect for Day1"));
      Assert.AreEqual(5, util[1].IdleHours, string.Format("Idle hours incorrect for Day1"));
      Assert.AreEqual(testStartDeviceDay.AddDays(2).AddHours(7).AddMinutes(58), util[1].EventUTC, string.Format("source EventUTC wrong for Day1"));

      // this is the event which was moved from early on the 5th day to this 4th day
      Assert.AreEqual(testStartDeviceDay.AddDays(3).KeyDate(), util[2].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day2"));
      Assert.AreEqual(16, util[2].RuntimeHours, string.Format("Runtime hours incorrect for Day2"));
      Assert.AreEqual(4, util[2].IdleHours, string.Format("Idle hours incorrect for Day2"));
      Assert.AreEqual(testStartDeviceDay.AddDays(3).AddHours(7).AddMinutes(58), util[2].EventUTC, string.Format("source EventUTC wrong for Day2"));

      // this should now delta from the emulated day on day4 instead of previously it would have deltas with day3
      Assert.AreEqual(testStartDeviceDay.AddDays(35).KeyDate(), util[3].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day3"));
      Assert.AreEqual(3, util[3].RuntimeHours, string.Format("Runtime hours incorrect for Day3"));
      Assert.AreEqual(5, util[3].IdleHours, string.Format("Idle hours incorrect for Day3"));
      Assert.AreEqual(testStartDeviceDay.AddDays(35).AddHours(7).AddMinutes(12), util[3].EventUTC, string.Format("source EventUTC wrong for Day3"));

      Assert.AreEqual(testStartDeviceDay.AddDays(36).KeyDate(), util[4].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day4"));
      Assert.AreEqual(10, util[4].RuntimeHours, string.Format("Runtime hours incorrect for Day4"));
      Assert.AreEqual(9, util[4].IdleHours, string.Format("Idle hours incorrect for Day4"));
      Assert.AreEqual(testStartDeviceDay.AddDays(36).AddHours(7).AddMinutes(24), util[4].EventUTC, string.Format("source EventUTC wrong for Day4"));
    }

  }
}
