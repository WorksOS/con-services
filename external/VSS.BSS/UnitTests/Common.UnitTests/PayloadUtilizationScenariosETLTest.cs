using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
  /// <summary>
  ///This is a test class for FactPayloadCycleUtilizationDaily Missing Data/Meter Value/Spike Flags populated by the report ETLs
  ///</summary>
  [TestClass()]
  public class PayloadUtilizationScenariosETLTest : ReportLogicTestBase
  {

    [TestMethod()]
    [DatabaseTest]
    public void PayloadUtlizationTest_MultipleScenarios_bothPayloadNCycle()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-10).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, payload: 100, cycles: 20, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, runtimeHours: 88, idleHours: 10, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, payload: 200, cycles: 10, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, runtimeHours: 94, idleHours: 12, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 4, payload: 400, cycles: 40, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 4, runtimeHours: 105, idleHours: 14, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 5, payload: 300, cycles: 50, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 5, runtimeHours: 110, idleHours: 16, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 6, payload: 600, cycles: 60, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 6, runtimeHours: 118, idleHours: 18, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, payload: 700, cycles: 65, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, runtimeHours: 126, idleHours: 20, totalFuel: 12, idleFuel: null);

      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();
      List<FactPayloadCycleUtilizationDaily> fpcu = (from aud in Ctx.RptContext.FactPayloadCycleUtilizationDaily
                                                     where aud.ifk_DimAssetID == asset.AssetID
                                                      && aud.ifk_AssetPriorKeyDate != null
                                                     orderby aud.ifk_AssetKeyDate
                                                     select aud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(4, fpcu.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      // Day 1 doesn't get a FPCU row

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 2, startOfDeviceDay,
          day: 2, payloadValue: 100, cycleValue: null);

      //Day 3 had no report (no Daily Report) so no row for this day

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 3, startOfDeviceDay,
          day: 4, payloadValue: 200, cycleValue: 30);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 4, startOfDeviceDay,
          day: 5, payloadValue: null, cycleValue: 10);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 6, startOfDeviceDay,
          day: 6, payloadValue: 400, cycleValue: 15);

    }

    
    [TestMethod()]
    [DatabaseTest]
    public void PayloadUtlizationCalloutFlagsTest_MultipleScenarios_bothPayloadNCycle()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-10).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, payload: 100, cycles: 20, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, payload: 200, cycles: 10, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 4, payload: 400, cycles: 40, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 4, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 5, payload: 300, cycles: 50, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 5, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 6, payload: 600, cycles: 60, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 6, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, payload: 700, cycles: 65, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();
      List<FactPayloadCycleUtilizationDaily> fpcu = (from aud in Ctx.RptContext.FactPayloadCycleUtilizationDaily
                                                     where aud.ifk_DimAssetID == asset.AssetID
                                                      && aud.ifk_AssetPriorKeyDate != null
                                                     orderby aud.ifk_AssetKeyDate
                                                     select aud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(4, fpcu.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      // Day 1 doesn't get a FPCU row

      AssertNH_RPTFactPayloadCycleUtilizationDailyFlags(fpcu, 2, startOfDeviceDay,
          day: 2, payloadCalloutFlag: DimUtilizationCalloutTypeEnum.None, cycleCalloutFlag: DimUtilizationCalloutTypeEnum.Spike);

      //Day 3 had no report (no Daily Report) so no row for this day

      AssertNH_RPTFactPayloadCycleUtilizationDailyFlags(fpcu, 3, startOfDeviceDay,
          day: 4, payloadCalloutFlag: DimUtilizationCalloutTypeEnum.MultipleDayDelta, cycleCalloutFlag: DimUtilizationCalloutTypeEnum.MultipleDayDelta);

      AssertNH_RPTFactPayloadCycleUtilizationDailyFlags(fpcu, 4, startOfDeviceDay,
          day: 5, payloadCalloutFlag: DimUtilizationCalloutTypeEnum.Spike, cycleCalloutFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDailyFlags(fpcu, 6, startOfDeviceDay,
          day: 6, payloadCalloutFlag: DimUtilizationCalloutTypeEnum.None, cycleCalloutFlag: DimUtilizationCalloutTypeEnum.None);

    }

    
    [TestMethod()]
    [DatabaseTest]
    public void PayloadUtlizationTest_MultipleScenarios_forUtilization_onlyCycle()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-10).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, payload: null, cycles: 20, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, payload: null, cycles: 10, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 4, payload: null, cycles: 40, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 4, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 5, payload: null, cycles: 50, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 5, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 6, payload: null, cycles: 60, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 6, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, payload: null, cycles: 65, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();
      List<FactPayloadCycleUtilizationDaily> fpcu = (from aud in Ctx.RptContext.FactPayloadCycleUtilizationDaily
                                                     where aud.ifk_DimAssetID == asset.AssetID
                                                      && aud.ifk_AssetPriorKeyDate != null
                                                     orderby aud.ifk_AssetKeyDate
                                                     select aud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(4, fpcu.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      // Day 1 doesn't get a FPCU row

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 2, startOfDeviceDay,
          day: 2, payloadValue: null, cycleValue: null);

      //Day 3 had no report (no Daily Report) so no row for this day

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 3, startOfDeviceDay,
          day: 4, payloadValue: null, cycleValue: 30);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 4, startOfDeviceDay,
          day: 5, payloadValue: null, cycleValue: 10);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 6, startOfDeviceDay,
          day: 6, payloadValue: null, cycleValue: 15);

    }

    
    [TestMethod()]
    [DatabaseTest]
    public void PayloadUtlizationCalloutFlagsTest_MultipleScenarios_forUtilization_onlyCycle()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-10).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, payload: null, cycles: 20, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, payload: null, cycles: 10, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 4, payload: null, cycles: 40, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 4, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 5, payload: null, cycles: 50, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 5, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 6, payload: null, cycles: 60, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 6, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, payload: null, cycles: 65, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();
      List<FactPayloadCycleUtilizationDaily> fpcu = (from aud in Ctx.RptContext.FactPayloadCycleUtilizationDaily
                                                     where aud.ifk_DimAssetID == asset.AssetID
                                                      && aud.ifk_AssetPriorKeyDate != null
                                                     orderby aud.ifk_AssetKeyDate
                                                     select aud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(4, fpcu.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      // Day 1 doesn't get a FPCU row

      AssertNH_RPTFactPayloadCycleUtilizationDailyFlags(fpcu, 2, startOfDeviceDay,
          day: 2, payloadCalloutFlag: DimUtilizationCalloutTypeEnum.None, cycleCalloutFlag: DimUtilizationCalloutTypeEnum.Spike);

      //Day 3 had no report (no Daily Report) so no row for this day

      AssertNH_RPTFactPayloadCycleUtilizationDailyFlags(fpcu, 3, startOfDeviceDay,
          day: 4, payloadCalloutFlag: DimUtilizationCalloutTypeEnum.None, cycleCalloutFlag: DimUtilizationCalloutTypeEnum.MultipleDayDelta);

      AssertNH_RPTFactPayloadCycleUtilizationDailyFlags(fpcu, 4, startOfDeviceDay,
          day: 5, payloadCalloutFlag: DimUtilizationCalloutTypeEnum.None, cycleCalloutFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDailyFlags(fpcu, 6, startOfDeviceDay,
          day: 6, payloadCalloutFlag: DimUtilizationCalloutTypeEnum.None, cycleCalloutFlag: DimUtilizationCalloutTypeEnum.None);
    }


    [TestMethod()]
    [DatabaseTest]
    public void PayloadUtlizationTest_MultipleScenarios_forUtilization_OnlyPayload()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-10).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, payload: 100, cycles: null, utilization: 17);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, payload: 200, cycles: null, utilization: 17);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 4, payload: 400, cycles: null, utilization: 17);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 4, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 5, payload: 300, cycles: null, utilization: 17);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 5, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 6, payload: 600, cycles: null, utilization: 17);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 6, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, payload: 700, cycles: null, utilization: 17);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();
      List<FactPayloadCycleUtilizationDaily> fpcu = (from aud in Ctx.RptContext.FactPayloadCycleUtilizationDaily
                                                     where aud.ifk_DimAssetID == asset.AssetID
                                                      && aud.ifk_AssetPriorKeyDate != null
                                                     orderby aud.ifk_AssetKeyDate
                                                     select aud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(4, fpcu.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      // Day 1 doesn't get a FPCU row

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 2, startOfDeviceDay,
          day: 2, payloadValue: 100, cycleValue: null);

      //Day 3 had no report (no Daily Report) so no row for this day

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 3, startOfDeviceDay,
          day: 4, payloadValue: 200, cycleValue: null);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 4, startOfDeviceDay,
          day: 5, payloadValue: null, cycleValue: null);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 6, startOfDeviceDay,
          day: 6, payloadValue: 400, cycleValue: null);
    }


    [TestMethod()]
    [DatabaseTest]
    public void PayloadUtlizationCalloutFlagsTest_MultipleScenarios_forUtilization_OnlyPayload()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-10).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, payload: 100, cycles: null, utilization: 17);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, payload: 200, cycles: null, utilization: 17);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 4, payload: 400, cycles: null, utilization: 17);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 4, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 5, payload: 300, cycles: null, utilization: 17);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 5, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 6, payload: 600, cycles: null, utilization: 17);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 6, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, payload: 700, cycles: null, utilization: 17);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();
      List<FactPayloadCycleUtilizationDaily> fpcu = (from aud in Ctx.RptContext.FactPayloadCycleUtilizationDaily
                                                     where aud.ifk_DimAssetID == asset.AssetID
                                                      && aud.ifk_AssetPriorKeyDate != null
                                                     orderby aud.ifk_AssetKeyDate
                                                     select aud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(4, fpcu.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      // Day 1 doesn't get a FPCU row

      AssertNH_RPTFactPayloadCycleUtilizationDailyFlags(fpcu, 2, startOfDeviceDay,
          day: 2, payloadCalloutFlag: DimUtilizationCalloutTypeEnum.None, cycleCalloutFlag: DimUtilizationCalloutTypeEnum.None);

      //Day 3 had no report (no Daily Report) so no row for this day

      AssertNH_RPTFactPayloadCycleUtilizationDailyFlags(fpcu, 3, startOfDeviceDay,
          day: 4, payloadCalloutFlag: DimUtilizationCalloutTypeEnum.MultipleDayDelta, cycleCalloutFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDailyFlags(fpcu, 4, startOfDeviceDay,
          day: 5, payloadCalloutFlag: DimUtilizationCalloutTypeEnum.Spike, cycleCalloutFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDailyFlags(fpcu, 6, startOfDeviceDay,
          day: 6, payloadCalloutFlag: DimUtilizationCalloutTypeEnum.None, cycleCalloutFlag: DimUtilizationCalloutTypeEnum.None);
    }
    
    [TestMethod()]
    [DatabaseTest]
    public void PayloadUtlizationTest_DataForADayReceivedLater_forUtilization_onlyCycle()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-10).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, payload: null, cycles: 20, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, payload: null, cycles: 10, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 4, payload: null, cycles: 40, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 4, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 5, payload: null, cycles: 50, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 5, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();
      List<FactPayloadCycleUtilizationDaily> fpcu = (from aud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
                                                     where aud.ifk_DimAssetID == asset.AssetID
                                                      && aud.ifk_AssetPriorKeyDate != null
                                                     orderby aud.ifk_AssetKeyDate
                                                     select aud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(3, fpcu.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      // Day 1 doesn't get a FPCU row
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 2, startOfDeviceDay,
          day: 2, payloadValue: null, cycleValue: null);

      //Day 3 had no report (no Daily Report) so no row for this day

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 4, startOfDeviceDay,
          day: 4, payloadValue: null, cycleValue: 30);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 5, startOfDeviceDay,
          day: 5, payloadValue: null, cycleValue: 10);

      // Day 3 data received after Day 5
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 3, payload: null, cycles: 20, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 3, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      // Run the ETL's again
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();
      fpcu = (from aud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
              where aud.ifk_DimAssetID == asset.AssetID
                && aud.ifk_AssetPriorKeyDate != null
              orderby aud.ifk_AssetKeyDate
              select aud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(4, fpcu.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      // Re assert the FactPayLoad values
      // Day 1 doesn't get a FPCU row

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 2, startOfDeviceDay,
          day: 2, payloadValue: null, cycleValue: null);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 3, startOfDeviceDay,
          day: 3, payloadValue: null, cycleValue: 10);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 4, startOfDeviceDay,
          day: 4, payloadValue: null, cycleValue: 20);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 5, startOfDeviceDay,
          day: 5, payloadValue: null, cycleValue: 10);
    }
    
    [TestMethod()]
    [DatabaseTest]
    public void PayloadUtlizationTest_DataReceivedWithChangedUtilizationType()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-10).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, payload: null, cycles: 20, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, payload: null, cycles: 10, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 4, payload: 20, cycles: 40, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 4, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 5, payload: 40, cycles: 50, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 5, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();
      List<FactPayloadCycleUtilizationDaily> fpcu = (from aud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
                                                     where aud.ifk_DimAssetID == asset.AssetID
                                                      && aud.ifk_AssetPriorKeyDate != null
                                                     orderby aud.ifk_AssetKeyDate
                                                     select aud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(3, fpcu.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      // Day 1 doesn't get a FPCU row
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 2, startOfDeviceDay,
          day: 2, payloadValue: null, cycleValue: null);

      // Day 4 does'nt get a FPCU row for Payload after Utilization type changes to 19
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 4, startOfDeviceDay,
      day: 4, payloadValue: null, cycleValue: 30, checkPayload: false);

      // Day 5 will have FPCU rows for both Payload and Cycles
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 5, startOfDeviceDay,
          day: 5, payloadValue: 20, cycleValue: 10);
    }
    
    [TestMethod()]
    [DatabaseTest]
    public void PayloadUtlizationTest_DataReceivedWithChangedUtilizationTypeFollowedByAnOlderDaysDataWithPrevUtilizationType()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-10).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, payload: null, cycles: 20, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, payload: null, cycles: 10, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      // Missing data for Day 3 with utilization type 2
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 4, payload: 20, cycles: 40, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 4, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 5, payload: 40, cycles: 50, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 5, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();
      List<FactPayloadCycleUtilizationDaily> fpcu = (from aud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
                                                     where aud.ifk_DimAssetID == asset.AssetID
                                                      && aud.ifk_AssetPriorKeyDate != null
                                                     orderby aud.ifk_AssetKeyDate
                                                     select aud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(3, fpcu.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      // Day 1 doesn't get a FPCU row
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 2, startOfDeviceDay,
          day: 2, payloadValue: null, cycleValue: null);

      // Day 4 does'nt get a FPCU row for Payload after Utilization type changes to 19
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 4, startOfDeviceDay,
      day: 4, payloadValue: null, cycleValue: 30, checkPayload: false);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 5, startOfDeviceDay,
          day: 5, payloadValue: 20, cycleValue: 10);

      // Day 3 missing data with utilization type 2 is received late
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 3, startOfDeviceDay,
         day: 3, payload: null, cycles: 20, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 3, startOfDeviceDay,
          day: 3, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      //Re-run the Sync to RPT and the FactPayload populate ETL
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();
      fpcu = (from aud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
              where aud.ifk_DimAssetID == asset.AssetID
                && aud.ifk_AssetPriorKeyDate != null
              orderby aud.ifk_AssetKeyDate
              select aud).ToList<FactPayloadCycleUtilizationDaily>();

      // Re-assert FPCU records
      Assert.AreEqual(4, fpcu.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      //Re-assert FPCU data
      // Day 1 doesn't get a FPCU row
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 2, startOfDeviceDay,
          day: 2, payloadValue: null, cycleValue: null);

      // Day 3 now gets a row - validate it
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 3, startOfDeviceDay,
          day: 3, payloadValue: null, cycleValue: 10);

      // Day 4 does'nt get a FPCU row for Payload after Utilization type changes to 19. Cycle Value will be adjusted based on Day 3 value.
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 4, startOfDeviceDay,
      day: 4, payloadValue: null, cycleValue: 20, checkPayload: false);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 5, startOfDeviceDay,
          day: 5, payloadValue: 20, cycleValue: 10);
    }
    
    [TestMethod()]
    [DatabaseTest]
    public void PayloadUtlizationTest_DataReceivedWithChangedUtilizationTypeFollowedByAnotherChangeToOldUtilizationType()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-10).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, payload: null, cycles: 20, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 1, startOfDeviceDay,
          day: 1, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, payload: null, cycles: 10, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 2, startOfDeviceDay,
          day: 2, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      // Missing data for Day 3 with utilization type 2
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 4, payload: 20, cycles: 40, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 4, startOfDeviceDay,
          day: 4, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 5, payload: 40, cycles: 50, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 5, startOfDeviceDay,
          day: 5, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      //Change in utilization type back to 2
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, payload: null, cycles: 60, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 6, startOfDeviceDay,
          day: 6, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 7, startOfDeviceDay,
          day: 7, payload: null, cycles: 95, utilization: 2);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 7, startOfDeviceDay,
          day: 7, runtimeHours: 88, idleHours: 20, totalFuel: 12, idleFuel: null);

      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();
      List<FactPayloadCycleUtilizationDaily> fpcu = (from aud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
                                                     where aud.ifk_DimAssetID == asset.AssetID
                                                      && aud.ifk_AssetPriorKeyDate != null
                                                     orderby aud.ifk_AssetKeyDate
                                                     select aud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(3, fpcu.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      // Day 1 doesn't get a FPCU row
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 2, startOfDeviceDay,
          day: 2, payloadValue: null, cycleValue: null);

      // Day 4 does'nt get a FPCU row for Payload after Utilization type changes to 19
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 4, startOfDeviceDay,
      day: 4, payloadValue: null, cycleValue: 30, checkPayload: false);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 5, startOfDeviceDay,
          day: 5, payloadValue: 20, cycleValue: 10);

      /** missing payload when it is now expecting a dailyReport i.e p + C
       * // Cycle difference will consider the previously reported utlization type prior to 19
       // jcm 20140328 this is a bug in the old ETL. This value should be 10. 
       AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 6, startOfDeviceDay,
           day: 6, payloadValue: null, cycleValue: 50);

       AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcu, 7, startOfDeviceDay,
           day: 7, payloadValue: null, cycleValue: 35);
       * ****/
    }

    
    [TestMethod()]
    [DatabaseTest]
    public void PayloadCycleUtilizationDailyTest_HappyPath()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime firstDeviceDay = testStartDeviceDay.AddHours(1);
      DateTime secondDeviceDay = testStartDeviceDay.AddDays(1).AddHours(10);

      // setup first day
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, firstDeviceDay,
          day: 0, payload: 100, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA(asset.AssetID, firstDeviceDay,
          day: 0, runtimeHours: 8, engineIdleHours: 1, consumptionGallons: 10, idleFuelGallons: 1, runtimeOffsetMs: 0);

      // setup our tests

      // hour 1
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(1),
          day: 0, payload: 100, cycles: null, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(1),
          day: 0, runtimeHours: 10, engineIdleHours: null, consumptionGallons: null, idleFuelGallons: null, runtimeOffsetMs: 0);

      // hour 2
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(2),
          day: 0, payload: null, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(2),
          day: 0, runtimeHours: null, engineIdleHours: 2, consumptionGallons: 12, idleFuelGallons: 2, runtimeOffsetMs: 0);

      // hour 3
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(3),
          day: 0, payload: 200, cycles: 20, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(3),
          day: 0, runtimeHours: 10, engineIdleHours: 2, consumptionGallons: 12, idleFuelGallons: 2, runtimeOffsetMs: 0);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily      
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();

      List<FactPayloadCycleUtilizationDaily> fpcuds = (from fpcud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
                                                       where fpcud.ifk_DimAssetID == asset.AssetID
                                                        && fpcud.ifk_AssetPriorKeyDate != null
                                                       orderby fpcud.ifk_AssetKeyDate
                                                       select fpcud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(1, fpcuds.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, secondDeviceDay, hour: 3,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);
    }


    [TestMethod()]
    [DatabaseTest]
    public void PayloadCycleUtilizationDailyTest_NoPayloadDailyReport()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime firstDeviceDay = testStartDeviceDay.AddHours(1);
      DateTime secondDeviceDay = testStartDeviceDay.AddDays(1).AddHours(10);

      // setup first day
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, firstDeviceDay,
          day: 0, payload: 100, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA(asset.AssetID, firstDeviceDay,
          day: 0, runtimeHours: 8, engineIdleHours: 1, consumptionGallons: 10, idleFuelGallons: 1, runtimeOffsetMs: 0);

      // setup our tests

      // hour 1
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(1),
          day: 0, payload: 100, cycles: null, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(1),
          day: 0, runtimeHours: 10, engineIdleHours: null, consumptionGallons: null, idleFuelGallons: null, runtimeOffsetMs: 0);

      // hour 2
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(2),
          day: 0, payload: null, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(2),
          day: 0, runtimeHours: null, engineIdleHours: 2, consumptionGallons: 12, idleFuelGallons: 2, runtimeOffsetMs: 0);

      // hour 3
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(3),
          day: 0, payload: null, cycles: 20, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(3),
          day: 0, runtimeHours: 10, engineIdleHours: 2, consumptionGallons: 12, idleFuelGallons: 2, runtimeOffsetMs: 0);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily      
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();

      List<FactPayloadCycleUtilizationDaily> fpcuds = (from fpcud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
                                                       where fpcud.ifk_DimAssetID == asset.AssetID
                                                        && fpcud.ifk_AssetPriorKeyDate != null
                                                       orderby fpcud.ifk_AssetKeyDate
                                                       select fpcud).ToList<FactPayloadCycleUtilizationDaily>();
      // This should return zero as, once a 'Payload and Cycles 'daily report' is expected (which we have for day1) 
      //   then days with no dailyReport are ignored.        
      Assert.AreEqual(0, fpcuds.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");
    }

    
    [TestMethod()]
    [DatabaseTest] 
    public void PayloadCycleUtilizationDailyTest_NoUtilizationDailyReport()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime firstDeviceDay = testStartDeviceDay.AddHours(1);
      DateTime secondDeviceDay = testStartDeviceDay.AddDays(1).AddHours(10);

      // setup first day
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, firstDeviceDay,
        day: 0, payload: 100, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA(asset.AssetID, firstDeviceDay,
          day: 0, runtimeHours: 8, engineIdleHours: 1, consumptionGallons: 10, idleFuelGallons: 1, runtimeOffsetMs: 0);

      // setup our tests

      // hour 1
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(1),
          day: 0, payload: 100, cycles: null, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(1),
          day: 0, runtimeHours: 10, engineIdleHours: null, consumptionGallons: null, idleFuelGallons: null, runtimeOffsetMs: 0);

      // hour 2
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(2),
          day: 0, payload: null, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(2),
          day: 0, runtimeHours: null, engineIdleHours: 2, consumptionGallons: 12, idleFuelGallons: 2, runtimeOffsetMs: 0);

      // hour 3
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(3),
          day: 0, payload: 200, cycles: 20, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(3),
          day: 0, runtimeHours: 10, engineIdleHours: null, consumptionGallons: null, idleFuelGallons: null, runtimeOffsetMs: 0);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily      
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();

      List<FactPayloadCycleUtilizationDaily> fpcuds = (from fpcud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
                                                       where fpcud.ifk_DimAssetID == asset.AssetID
                                                        && fpcud.ifk_AssetPriorKeyDate != null
                                                       orderby fpcud.ifk_AssetKeyDate
                                                       select fpcud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(1, fpcuds.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, secondDeviceDay, hour: 3,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue);
    }

    
    [TestMethod()]
    [DatabaseTest]
    public void PayloadCycleUtilizationDailyTest_UtilizationDailyReportEarlierThanPayloadDailyReport()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime firstDeviceDay = testStartDeviceDay.AddHours(1);
      DateTime secondDeviceDay = testStartDeviceDay.AddDays(1).AddHours(10);
      toleranceMS = GetToleranceMS();

      // setup first day
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, firstDeviceDay,
        day: 0, payload: 100, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA(asset.AssetID, firstDeviceDay,
          day: 0, runtimeHours: 8, engineIdleHours: 1, consumptionGallons: 10, idleFuelGallons: 1, runtimeOffsetMs: -(toleranceMS + 1000));

      // setup our tests

      // hour 1
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(1),
          day: 0, payload: 100, cycles: null, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(1),
          day: 0, runtimeHours: 10, engineIdleHours: null, consumptionGallons: null, idleFuelGallons: null, runtimeOffsetMs: -(toleranceMS + 1000));

      // hour 2
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(2),
          day: 0, payload: null, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(2),
          day: 0, runtimeHours: 12, engineIdleHours: 2, consumptionGallons: 12, idleFuelGallons: 2, runtimeOffsetMs: -(toleranceMS + 1000));

      // hour 3
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(3),
          day: 0, payload: 200, cycles: 20, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(3),
          day: 0, runtimeHours: null, engineIdleHours: 4, consumptionGallons: null, idleFuelGallons: null, runtimeOffsetMs: -(toleranceMS + 1000));

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily      
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();

      List<FactPayloadCycleUtilizationDaily> fpcuds = (from fpcud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
                                                       where fpcud.ifk_DimAssetID == asset.AssetID
                                                        && fpcud.ifk_AssetPriorKeyDate != null
                                                       orderby fpcud.ifk_AssetKeyDate
                                                       select fpcud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(1, fpcuds.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, secondDeviceDay, hour: 3,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue);
    }

    
    [TestMethod()]
    [DatabaseTest]
    public void PayloadCycleUtilizationDailyTest_PayloadDailyReportEarlierThanUtilizationDailyReport()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime firstDeviceDay = testStartDeviceDay.AddHours(1);
      DateTime secondDeviceDay = testStartDeviceDay.AddDays(1).AddHours(10);
      toleranceMS = GetToleranceMS();

      // setup first day
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, firstDeviceDay,
        day: 0, payload: 100, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA(asset.AssetID, firstDeviceDay,
          day: 0, runtimeHours: 8, engineIdleHours: 1, consumptionGallons: 10, idleFuelGallons: 1, runtimeOffsetMs: (toleranceMS + 1000));

      // setup our tests

      // hour 1
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(1),
          day: 0, payload: 100, cycles: null, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(1),
          day: 0, runtimeHours: 10, engineIdleHours: null, consumptionGallons: null, idleFuelGallons: null, runtimeOffsetMs: (toleranceMS + 1000));

      // hour 2
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(2),
          day: 0, payload: 200, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(2),
          day: 0, runtimeHours: 12, engineIdleHours: 2, consumptionGallons: 12, idleFuelGallons: 2, runtimeOffsetMs: (toleranceMS + 1000));

      // hour 3
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(3),
          day: 0, payload: null, cycles: 20, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(3),
          day: 0, runtimeHours: 10, engineIdleHours: 4, consumptionGallons: 12, idleFuelGallons: 2, runtimeOffsetMs: (toleranceMS + 1000));

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily      
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();

      List<FactPayloadCycleUtilizationDaily> fpcuds = (from fpcud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
                                                       where fpcud.ifk_DimAssetID == asset.AssetID
                                                        && fpcud.ifk_AssetPriorKeyDate != null
                                                       orderby fpcud.ifk_AssetKeyDate
                                                       select fpcud).ToList<FactPayloadCycleUtilizationDaily>();
      Assert.AreEqual(1, fpcuds.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      //jcm will look for the P&C daily report (if expected), NOT the latest in the day
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, secondDeviceDay, hour: 2,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue);
    }


    [TestMethod()]
    [DatabaseTest]
    public void PayloadCycleUtilizationDailyTest_AssetOnlyReportsPayload()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime firstDeviceDay = testStartDeviceDay.AddHours(1);
      DateTime secondDeviceDay = testStartDeviceDay.AddDays(1).AddHours(10);

      // setup first day
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, firstDeviceDay,
        day: 0, payload: 100, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA(asset.AssetID, firstDeviceDay,
          day: 0, runtimeHours: 8, engineIdleHours: 1, consumptionGallons: 10, idleFuelGallons: 1, runtimeOffsetMs: 0);

      // setup our tests

      // hour 1
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(1),
          day: 0, payload: 100, cycles: null, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(1),
          day: 0, runtimeHours: 10, engineIdleHours: null, consumptionGallons: null, idleFuelGallons: null, runtimeOffsetMs: 0);

      // hour 2
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(2),
          day: 0, runtimeHours: null, engineIdleHours: 2, consumptionGallons: 12, idleFuelGallons: 2, runtimeOffsetMs: 0);

      // hour 3
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(3),
          day: 0, payload: 200, cycles: null, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(3),
          day: 0, runtimeHours: 10, engineIdleHours: 4, consumptionGallons: 12, idleFuelGallons: 2, runtimeOffsetMs: 0);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily      
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();

      List<FactPayloadCycleUtilizationDaily> fpcuds = (from fpcud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
                                                       where fpcud.ifk_DimAssetID == asset.AssetID
                                                        && fpcud.ifk_AssetPriorKeyDate != null
                                                       orderby fpcud.ifk_AssetKeyDate
                                                       select fpcud).ToList<FactPayloadCycleUtilizationDaily>();
      // jcm yes I beleive the count should indeed by zero as there is no P&C daily report for secondsDeviceDay
      Assert.AreEqual(0, fpcuds.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      //AssertNH_RPTFactPayloadCycleUtilizationFlags_Refactor(fpcuds, secondDeviceDay, hour: 3,
      //  payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  payloadPerCycleCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  payloadPerRuntimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  cyclesPerRuntimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  payloadPerWorkingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  cyclesPerWorkingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  payloadPerTotalFuelCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  cyclesPerTotalFuelCalloutTypeID: DimUtilizationCalloutTypeEnum.None);
    }


    [TestMethod()]
    [DatabaseTest]
    public void PayloadCycleUtilizationDailyTest_AssetOnlyReportsCycle()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime firstDeviceDay = testStartDeviceDay.AddHours(1);
      DateTime secondDeviceDay = testStartDeviceDay.AddDays(1).AddHours(10);

      // setup first day
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, firstDeviceDay,
        day: 0, payload: 100, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA(asset.AssetID, firstDeviceDay,
          day: 0, runtimeHours: 8, engineIdleHours: 1, consumptionGallons: 10, idleFuelGallons: 1, runtimeOffsetMs: 0);

      // setup our tests

      // hour 1
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(1),
          day: 0, payload: null, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(1),
          day: 0, runtimeHours: 10, engineIdleHours: null, consumptionGallons: null, idleFuelGallons: null, runtimeOffsetMs: 0);

      // hour 2
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(2),
          day: 0, runtimeHours: null, engineIdleHours: 2, consumptionGallons: 12, idleFuelGallons: 2, runtimeOffsetMs: 0);

      // hour 3
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, secondDeviceDay.AddHours(3),
          day: 0, payload: null, cycles: 20, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATA_WithOptionalHoursLocation(asset.AssetID, secondDeviceDay.AddHours(3),
          day: 0, runtimeHours: 10, engineIdleHours: 4, consumptionGallons: 12, idleFuelGallons: 2, runtimeOffsetMs: 0);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily      
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();

      List<FactPayloadCycleUtilizationDaily> fpcuds = (from fpcud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
                                                       where fpcud.ifk_DimAssetID == asset.AssetID
                                                        && fpcud.ifk_AssetPriorKeyDate != null
                                                       orderby fpcud.ifk_AssetKeyDate
                                                       select fpcud).ToList<FactPayloadCycleUtilizationDaily>();
      // jcm yes I beleive the count should indeed by zero as there is no P&C daily report for secondsDeviceDay
      Assert.AreEqual(0, fpcuds.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      //AssertNH_RPTFactPayloadCycleUtilizationFlags_Refactor(fpcuds, secondDeviceDay, hour: 3,
      //  payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  payloadPerCycleCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  payloadPerRuntimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  cyclesPerRuntimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  payloadPerWorkingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  cyclesPerWorkingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  payloadPerTotalFuelCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
      //  cyclesPerTotalFuelCalloutTypeID: DimUtilizationCalloutTypeEnum.None);
    }

    [Ignore]
    [TestMethod()]
    [DatabaseTest]
    public void PayloadCycleUtilizationCalloutFlagsTest_MultipleScenarios()
    {
      var asset = SetupDefaultAsset();

      DateTime dateSetup = DateTime.UtcNow.AddDays(-28).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay.AddHours(10);


      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 1, payload: 100, cycles: null, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 2, payload: 200, cycles: null, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 3, payload: null, cycles: null, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 4, payload: 400, cycles: null, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 5, payload: 500, cycles: 10, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 6, payload: 600, cycles: 20, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 7, payload: 700, cycles: null, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 8, payload: 800, cycles: 40, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 9, payload: 900, cycles: 50, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 9, runtimeHours: 8, idleHours: null, totalFuel: null, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 10, payload: 1000, cycles: 60, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 10, runtimeHours: 16, idleHours: null, totalFuel: null, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 11, payload: 1100, cycles: 70, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 12, payload: 1200, cycles: 80, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 12, runtimeHours: 32, idleHours: null, totalFuel: null, idleFuel: null);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 13, payload: 1300, cycles: 90, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 13, runtimeHours: 40, idleHours: 4, totalFuel: 6, idleFuel: 3);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 14, payload: 1400, cycles: 100, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 14, runtimeHours: 48, idleHours: 8, totalFuel: 12, idleFuel: 6);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 15, payload: 1500, cycles: 110, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 15, runtimeHours: 56, idleHours: null, totalFuel: 18, idleFuel: 9);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 16, payload: 1600, cycles: 120, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 16, runtimeHours: 64, idleHours: 16, totalFuel: 24, idleFuel: 12);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 17, payload: 1700, cycles: 130, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 17, runtimeHours: 72, idleHours: 20, totalFuel: 30, idleFuel: 15);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 18, payload: null, cycles: 140, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 18, runtimeHours: 80, idleHours: 24, totalFuel: 36, idleFuel: 18);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 19, payload: 1900, cycles: 150, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 19, runtimeHours: 88, idleHours: 28, totalFuel: 42, idleFuel: 21);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 20, payload: 2000, cycles: 160, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay.AddMinutes(10),
          day: 20, runtimeHours: 96, idleHours: 32, totalFuel: 48, idleFuel: 24);

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 21, payload: 2100, cycles: 170, utilization: (long)NHDataHelper.UtilizationDescriptorEnum.SavePayloadAndCycles);
      SetupNH_DATAForDay(asset.AssetID, startOfDeviceDay,
          day: 21, runtimeHours: 104, idleHours: 36, totalFuel: 54, idleFuel: 27);


      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily      
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();

      List<FactAssetUtilizationDaily> fauds = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                               where aud.ifk_DimAssetID == asset.AssetID
                                                && aud.fk_AssetPriorKeyDate != null
                                               orderby aud.fk_AssetKeyDate
                                               select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(11, fauds.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      List<FactPayloadCycleUtilizationDaily> fpcuds = (from fpcud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
                                                       where fpcud.ifk_DimAssetID == asset.AssetID
                                                              && fpcud.ifk_AssetPriorKeyDate != null
                                                       orderby fpcud.ifk_AssetKeyDate
                                                       select fpcud).ToList<FactPayloadCycleUtilizationDaily>();
      //jcm this count should be 16. 
      // day 1,  2 and 4 get a row because at  this stage a P&C daily report is NOT expected.
      // day 3 no row
      // day 5 is where P&C daily reports start to be expected. Days 7,13,14,18 should be missing due to no daily report.

      //cwh this count should be 17 - day 1,3 get no reports, day 7,18 don't either because a full-rpt was expected, but 1 p/c was missing
      // day 2,4 get reports though only 1 p/c reported; afterwards, both are expected in order to generate a full p/c report

      Assert.AreEqual(17, fpcuds.Count(), "Incorrect FactPayloadCycleUtilizationDaily record count.");

      // Day 1 doesn't get any FAUD/FPCUD rows

      // Day 2 had no report (no Daily Report) so no row for this day
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 2,
       payloadMeterTonne: 200, cycleMeterCount: null,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: null,
       runtimeHours: null, workingHours: null,
       totalFuelConsumedGallons: null, workingFuelConsumedGallons: null);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 2,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        // jcm I changed this callout to match what tests in UtilizationMissingDataFlagETLTest” expect
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 3 had no Payload/Util reports (no Daily Report) so no FAUD/FPCUD rows for this day

      // Day 4 had no report (no Daily Report) so no row for this day
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 4,
       payloadMeterTonne: 400, cycleMeterCount: null,
       payloadMeterDeltaTonne: 200, cycleMeterDeltaCount: null,
       runtimeHours: null, workingHours: null,
       totalFuelConsumedGallons: null, workingFuelConsumedGallons: null);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 4,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 5 had no report (no Daily Report) so no row for this day
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 5,
       payloadMeterTonne: 500, cycleMeterCount: 10,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: null,
       runtimeHours: null, workingHours: null,
       totalFuelConsumedGallons: null, workingFuelConsumedGallons: null);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 5,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 6 had no report (no Daily Report) so no row for this day
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 6,
       payloadMeterTonne: 600, cycleMeterCount: 20,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: 10,
       runtimeHours: null, workingHours: null,
       totalFuelConsumedGallons: null, workingFuelConsumedGallons: null);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 6,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 7 had no Payload/Util reports (no Daily Report) so no FAUD/FPCUD rows for this day   

      // Day 8 had no report (no Daily Report) so no row for this day
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 8,
       payloadMeterTonne: 800, cycleMeterCount: 40,
       payloadMeterDeltaTonne: 200, cycleMeterDeltaCount: 20,
       runtimeHours: null, workingHours: null,
       totalFuelConsumedGallons: null, workingFuelConsumedGallons: null);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 8,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 9 had no report (no Daily Report) so no row for this day
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 9,
       payloadMeterTonne: 900, cycleMeterCount: 50,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: 10,
       runtimeHours: null, workingHours: null,
       totalFuelConsumedGallons: null, workingFuelConsumedGallons: null);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 9,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 10
      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay, day: 10,
        runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: null, idleFuelDelta: null, workingFuelDelta: null);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay, day: 10,
        runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        totalFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 10,
       payloadMeterTonne: 1000, cycleMeterCount: 60,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: 10,
       runtimeHours: 8, workingHours: null,
       totalFuelConsumedGallons: null, workingFuelConsumedGallons: null);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 10,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 11 had no report (no Daily Report) so no row for this day
      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 11,
       payloadMeterTonne: 1100, cycleMeterCount: 70,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: 10,
       runtimeHours: null, workingHours: null,
       totalFuelConsumedGallons: null, workingFuelConsumedGallons: null);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 11,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 12
      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay, day: 12,
        runtimeHoursDelta: 16, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: null, idleFuelDelta: null, workingFuelDelta: null);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay, day: 12,
        runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        totalFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 12,
       payloadMeterTonne: 1200, cycleMeterCount: 80,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: 10,
       runtimeHours: 16, workingHours: null,
       totalFuelConsumedGallons: null, workingFuelConsumedGallons: null);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 12,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 13
      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay, day: 13,
        runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: null, idleFuelDelta: null, workingFuelDelta: null);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay, day: 13,
        runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        totalFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 13,
       payloadMeterTonne: 1300, cycleMeterCount: 90,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: 10,
       runtimeHours: 8, workingHours: null,
       totalFuelConsumedGallons: null, workingFuelConsumedGallons: null);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 13,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 14
      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay, day: 14,
        runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: 4, totalFuelDelta: 6, idleFuelDelta: null, workingFuelDelta: 3);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay, day: 14,
        runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        totalFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 14,
       payloadMeterTonne: 1400, cycleMeterCount: 100,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: 10,
       runtimeHours: 8, workingHours: 4,
       totalFuelConsumedGallons: 6,
       workingFuelConsumedGallons: 3);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 14,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 15
      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay, day: 15,
        runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: null, totalFuelDelta: 6, idleFuelDelta: null, workingFuelDelta: 3);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay, day: 15,
        runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        totalFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 15,
       payloadMeterTonne: 1500, cycleMeterCount: 110,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: 10,
       runtimeHours: 8, workingHours: null,
       totalFuelConsumedGallons: 6,
       workingFuelConsumedGallons: 3);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 15,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 16
      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay, day: 16,
        runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: 8, totalFuelDelta: 6, idleFuelDelta: null, workingFuelDelta: 3);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay, day: 16,
        runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        totalFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 16,
       payloadMeterTonne: 1600, cycleMeterCount: 120,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: 10,
      runtimeHours: 8, workingHours: 8,
       totalFuelConsumedGallons: 6,
       workingFuelConsumedGallons: 3);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 16,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 17
      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay, day: 17,
        runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: 4, totalFuelDelta: 6, idleFuelDelta: null, workingFuelDelta: 3);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay, day: 17,
        runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        totalFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 17,
       payloadMeterTonne: 1700, cycleMeterCount: 130,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: 10,
       runtimeHours: 8, workingHours: 4,
       totalFuelConsumedGallons: 6,
       workingFuelConsumedGallons: 3);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 17,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);

      // Day 18 had no Payload report so no FPCUD row for this day
      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay, day: 18,
        runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: 4, totalFuelDelta: 6, idleFuelDelta: null, workingFuelDelta: 3);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay, day: 18,
        runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        totalFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None);

      // Day 19
      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay, day: 19,
        runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: 4, totalFuelDelta: 6, idleFuelDelta: null, workingFuelDelta: 3);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay, day: 19,
        runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        totalFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 19,
       payloadMeterTonne: 1900, cycleMeterCount: 150,
       payloadMeterDeltaTonne: 200, cycleMeterDeltaCount: 20,
       runtimeHours: 16, workingHours: 8,
       totalFuelConsumedGallons: 12,
       workingFuelConsumedGallons: 6);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 19,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta);

      // Day 20
      // jcm: this day FAUD was created with EventUTC + 10 minutes. The AssertNH_RPTFactUtilizationDaily() checkes eventUTC NOT startOFDeviceDay
      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay.AddMinutes(10), day: 20,
        runtimeHoursDelta: 8, idleHoursDelta: 4, workingHoursDelta: 4, totalFuelDelta: 6, idleFuelDelta: null, workingFuelDelta: 3);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay.AddMinutes(10), day: 20,
        runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        totalFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 20,
       payloadMeterTonne: 2000, cycleMeterCount: 160,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: 10,
       runtimeHours: null, workingHours: null,
       totalFuelConsumedGallons: null,
       workingFuelConsumedGallons: null);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 20,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue);

      // Day 21
      AssertNH_RPTFactUtilizationDaily(fauds, startOfDeviceDay, day: 21,
        runtimeHoursDelta: 8, idleHoursDelta: null, workingHoursDelta: 4, totalFuelDelta: 6, idleFuelDelta: null, workingFuelDelta: 3);
      AssertNH_RPTFactUtilizationDailyFlags(fauds, startOfDeviceDay, day: 21,
        runtimeHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        workingHoursDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        totalFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None,
        idleFuelDeltaFlag: DimUtilizationCalloutTypeEnum.None);

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcuds, startOfDeviceDay, day: 21,
       payloadMeterTonne: 2100, cycleMeterCount: 170,
       payloadMeterDeltaTonne: 100, cycleMeterDeltaCount: 10,
       runtimeHours: 16, workingHours: 8,
       totalFuelConsumedGallons: 12,
       workingFuelConsumedGallons: 6);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcuds, startOfDeviceDay, day: 21,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MultipleDayDelta);

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
