using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass]
  public class A5N210SecondWindowTests : ReportLogicTestBase
  {
    [TestMethod]
    [DatabaseTest]
    public void SimultaneousRuntimeShouldBeProcessed()
    {
      Customer customer = TestData.TestDealer;
      var asset = SetupDefaultAsset(Entity.Device.PLE641.OwnerBssId(customer.BSSID).Save());

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
        day: 16, runtimeHours: 128, idleHours: 40, totalFuel: 22, idleFuel: 12);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
        day: 17, runtimeHours: 136, idleHours: null, totalFuel: 24, idleFuel: 13); // runtimeOffsetMs: 0

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();

      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
        where aud.ifk_DimAssetID == asset.AssetID
              && aud.fk_AssetPriorKeyDate != null
        orderby aud.fk_AssetKeyDate
        select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: 2,
        idleFuelDelta: 1, workingFuelDelta: 1);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue);
    }

    [TestMethod]
    [DatabaseTest]
    public void OffsetRuntimeNonA5N2ShouldNotBeProcessed()
    {
      // day 17 has a 2ms offset between the EP and HL. This is not a valid daily report for a non- A5N2 device 
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 16, runtimeHours: 128, idleHours: 40, totalFuel: 22, idleFuel: 12);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 17, runtimeHours: 136, idleHours: null, totalFuel: 24, idleFuel: 13, runtimeOffsetMs: 2000);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();

      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                               where aud.ifk_DimAssetID == asset.AssetID
                                                && aud.fk_AssetPriorKeyDate != null
                                               orderby aud.fk_AssetKeyDate
                                               select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(0, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");
    }

    [TestMethod]
    [DatabaseTest]
    public void OffsetRuntimeOfPlus2SecShouldBeProcessed()
    {
      // same as prior test, however it is an A5N2 device and the HL is offset from EP by 2 seconds.
      // This should fail to generate a daily report for tday 17 for a non-A5N2 device
      Customer customer = TestData.TestDealer;
      var asset = SetupDefaultAsset(Entity.Device.PLE641.OwnerBssId(customer.BSSID).Save());

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 16, runtimeHours: 128, idleHours: 40, totalFuel: 22, idleFuel: 12);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 17, runtimeHours: 136, idleHours: null, totalFuel: 24, idleFuel: 13, runtimeOffsetMs: 2000);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();

      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                               where aud.ifk_DimAssetID == asset.AssetID
                                                && aud.fk_AssetPriorKeyDate != null
                                               orderby aud.fk_AssetKeyDate
                                               select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: 2,
        idleFuelDelta: 1, workingFuelDelta: 1);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue);
    }

    [TestMethod]
    [DatabaseTest]
    public void OffsetRuntimeOfMinus3SecShouldBeProcessed()
    {
      Customer customer = TestData.TestDealer;
      var asset = SetupDefaultAsset(Entity.Device.PLE641.OwnerBssId(customer.BSSID).Save());

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 16, runtimeHours: 128, idleHours: 40, totalFuel: 22, idleFuel: 12);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 17, runtimeHours: 136, idleHours: null, totalFuel: 24, idleFuel: 13, runtimeOffsetMs: -3000);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();

      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                               where aud.ifk_DimAssetID == asset.AssetID
                                                && aud.fk_AssetPriorKeyDate != null
                                               orderby aud.fk_AssetKeyDate
                                               select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: 2,
        idleFuelDelta: 1, workingFuelDelta: 1);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue);
    }

    [TestMethod]
    [DatabaseTest]
    public void ClosestOffsetRuntimeShouldBeUsedWhenAhead()
    {
      Customer customer = TestData.TestDealer;
      var asset = SetupDefaultAsset(Entity.Device.PLE641.OwnerBssId(customer.BSSID).Save());

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 16, runtimeHours: 128, idleHours: 40, totalFuel: 22, idleFuel: 12);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
        day: 17, runtimeHours: 136, idleHours: null, totalFuel: 24, idleFuel: 13, runtimeOffsetMs: -2000);

      // stray HL that shouldn't be processed
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.TelematicsSync,
        startOfDeviceDay.AddDays(17).AddSeconds(3), runtimeHours: 135, latitude: GMTlatitude,
        longitude: GMTlongitude);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                               where aud.ifk_DimAssetID == asset.AssetID
                                                && aud.fk_AssetPriorKeyDate != null
                                               orderby aud.fk_AssetKeyDate
                                               select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: 2,
        idleFuelDelta: 1, workingFuelDelta: 1);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue);
    }

    [TestMethod]
    [DatabaseTest]
    public void ClosestOffsetRuntimeShouldBeUsedWhenBehind()
    {
      Customer customer = TestData.TestDealer;
      var asset = SetupDefaultAsset(Entity.Device.PLE641.OwnerBssId(customer.BSSID).Save());

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
        day: 16, runtimeHours: 128, idleHours: 40, totalFuel: 22, idleFuel: 12);

      // stray HL that shouldn't be processed
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.TelematicsSync,
        startOfDeviceDay.AddDays(17).AddSeconds(-3), runtimeHours: 135, latitude: GMTlatitude,
        longitude: GMTlongitude);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
        day: 17, runtimeHours: 136, idleHours: null, totalFuel: 24, idleFuel: 13, runtimeOffsetMs: 2000);
      
      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
        where aud.ifk_DimAssetID == asset.AssetID
              && aud.fk_AssetPriorKeyDate != null
        orderby aud.fk_AssetKeyDate
        select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: 2,
        idleFuelDelta: 1, workingFuelDelta: 1);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue);
    }

    [TestMethod]
    [DatabaseTest]
    public void OffsetOfExactly5SecAheadShouldBeProcessed()
    {
      Customer customer = TestData.TestDealer;
      var asset = SetupDefaultAsset(Entity.Device.PLE641.OwnerBssId(customer.BSSID).Save());

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);
      toleranceMS = GetToleranceMS();

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 16, runtimeHours: 128, idleHours: 40, totalFuel: 22, idleFuel: 12);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
        day: 17, runtimeHours: 136, idleHours: null, totalFuel: 24, idleFuel: 13, runtimeOffsetMs: -toleranceMS);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                               where aud.ifk_DimAssetID == asset.AssetID
                                                && aud.fk_AssetPriorKeyDate != null
                                               orderby aud.fk_AssetKeyDate
                                               select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: 2,
        idleFuelDelta: 1, workingFuelDelta: 1);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue);
    }

    [TestMethod]
    [DatabaseTest]
    public void OffsetOf1SecBehindToleranceShouldNotBeProcessed()
    {
      Customer customer = TestData.TestDealer;
      var asset = SetupDefaultAsset(Entity.Device.PLE641.OwnerBssId(customer.BSSID).Save());

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);
      toleranceMS = GetToleranceMS();

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 16, runtimeHours: 128, idleHours: 40, totalFuel: 22, idleFuel: 12);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
        day: 17, runtimeHours: 136, idleHours: null, totalFuel: 24, idleFuel: 13, runtimeOffsetMs: -(toleranceMS + 1000));

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                               where aud.ifk_DimAssetID == asset.AssetID
                                                && aud.fk_AssetPriorKeyDate != null
                                               orderby aud.fk_AssetKeyDate
                                               select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(0, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");
    }

    [TestMethod]
    [DatabaseTest]
    public void OffsetOf1SecAheadOfToleranceShouldNotBeProcessed()
    {
      Customer customer = TestData.TestDealer;
      var asset = SetupDefaultAsset(Entity.Device.PLE641.OwnerBssId(customer.BSSID).Save());

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);
      toleranceMS = GetToleranceMS();

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 16, runtimeHours: 128, idleHours: 40, totalFuel: 22, idleFuel: 12);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
        day: 17, runtimeHours: 136, idleHours: null, totalFuel: 24, idleFuel: 13, runtimeOffsetMs: (toleranceMS + 1000));

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                               where aud.ifk_DimAssetID == asset.AssetID
                                                && aud.fk_AssetPriorKeyDate != null
                                               orderby aud.fk_AssetKeyDate
                                               select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(0, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");
    }

    [TestMethod]
    [DatabaseTest]
    public void WithMultipleMessagesInWindowLatestEngineParamsShouldBePairedWithClosestHoursLocation()
    {
      Customer customer = TestData.TestDealer;
      var asset = SetupDefaultAsset(Entity.Device.PLE641.OwnerBssId(customer.BSSID).Save());

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 16, runtimeHours: 128, idleHours: 40, totalFuel: 22, idleFuel: 12);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay.AddSeconds(-3),
        day: 17, runtimeHours: 134, idleHours: null, totalFuel: 23, idleFuel: 12, runtimeOffsetMs: -2000);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
        day: 17, runtimeHours: 136, idleHours: null, totalFuel: 24, idleFuel: 13, runtimeOffsetMs: -1000);

      // stray HL that shouldn't be processed
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.TelematicsSync,
        startOfDeviceDay.AddDays(17).AddSeconds(4), runtimeHours: 141, latitude: GMTlatitude,
        longitude: GMTlongitude);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                               where aud.ifk_DimAssetID == asset.AssetID
                                                && aud.fk_AssetPriorKeyDate != null
                                               orderby aud.fk_AssetKeyDate
                                               select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: 2,
        idleFuelDelta: 1, workingFuelDelta: 1);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue);
    }
    [TestMethod]
    [DatabaseTest]
    public void WhenClosestHoursLocationCrossesMidnightBoundaryItShouldStillBeUsed()
    {
      Customer customer = TestData.TestDealer;
      var asset = SetupDefaultAsset(Entity.Device.PLE641.OwnerBssId(customer.BSSID).Save());

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(23).AddMinutes(59).AddSeconds(58);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 16, runtimeHours: 128, idleHours: 40, totalFuel: 22, idleFuel: 12);

      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
        day: 17, runtimeHours: 136, idleHours: null, totalFuel: 24, idleFuel: 13);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                               where aud.ifk_DimAssetID == asset.AssetID
                                                && aud.fk_AssetPriorKeyDate != null
                                               orderby aud.fk_AssetKeyDate
                                               select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: 2,
        idleFuelDelta: 1, workingFuelDelta: 1);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay,
        day: 17, runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue);
    }


    #region private
    private long toleranceMS;
    private int GetToleranceMS()
    {
        return int.Parse(
          (from config in Ctx.RptContext.DimConfigurationReadOnly
           where config.Name == "EventOffsetSeconds"
           select config.Value).Single()) * 1000;
    }
    #endregion private

  }
}
