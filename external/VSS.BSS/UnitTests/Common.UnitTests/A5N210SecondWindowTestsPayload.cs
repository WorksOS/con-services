using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass]
  public class A5N210SecondWindowTestsPayload : ReportLogicTestBase
  {
    private Asset asset;
    private DateTime dateSetup;
    private DateTime testStartDeviceDay;
    private DateTime startOfDeviceDay;
    private int tolerance;

    public void Initialize(Device device = null)
    {
      asset = SetupDefaultAsset(device ?? TestData.TestPLE641);

      dateSetup = DateTime.UtcNow.AddDays(-10).Date;
      testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      startOfDeviceDay = testStartDeviceDay.AddHours(10);

      tolerance =
        int.Parse(
          (from config in Ctx.RptContext.DimConfigurationReadOnly
           where config.Name == "EventOffsetSeconds"
           select config.Value).Single());

      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
        day: 1, payload: 100, cycles: 20, utilization: 19);
      SetupNH_DATA_Engine_ForDay(asset.AssetID, 0, startOfDeviceDay,
        day: 1, runtimeHours: 88, idleHours: 10, totalFuel: 12, idleFuel: null);
      SetupNH_DATA_Payload_ForDay(asset.AssetID, 0, startOfDeviceDay,
        day: 2, payload: 200, cycles: 30, utilization: 19);
    }

    [TestMethod]
    [DatabaseTest]
    public void SimultaneousEPShouldBeMatched()
    {
      Initialize();

      SetupNH_DATA_Engine_ForDay(asset.AssetID, 0, startOfDeviceDay,
          day: 2, runtimeHours: 94, idleHours: 12, totalFuel: 12, idleFuel: null);

      List<FactPayloadCycleUtilizationDaily> fpcus = ExecuteETL();      

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcus, 0, startOfDeviceDay,
          day: 2, payloadValue: 100, cycleValue: 10);    
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcus, startOfDeviceDay, day: 2,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);       
    }

    [TestMethod]
    [DatabaseTest]
    public void OffsetEPShouldNotBeMatchedForNonA5N2Device()
    {
      Initialize(TestData.TestMTS522);

      int offsetMs = tolerance * 1000 / 2;

      SetupNH_DATA_Engine_ForDay(asset.AssetID, offsetMs, startOfDeviceDay,
          day: 2, runtimeHours: 94, idleHours: 12, totalFuel: 12, idleFuel: null);

      List<FactPayloadCycleUtilizationDaily> fpcus = ExecuteETL();

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcus, 0, startOfDeviceDay,
        day: 2, payloadValue: 100, cycleValue: 10);   
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcus, startOfDeviceDay, day: 2,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue);  
    }

    [TestMethod]
    [DatabaseTest]
    public void EPBehindButInWindowShouldBeMatched()
    {
      Initialize();

      int offsetMs = tolerance * 1000 / 2;

      SetupNH_DATA_Engine_ForDay(asset.AssetID, offsetMs, startOfDeviceDay,
          day: 2, runtimeHours: 94, idleHours: 12, totalFuel: 12, idleFuel: null);

      List<FactPayloadCycleUtilizationDaily> fpcus = ExecuteETL();

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcus, 0, startOfDeviceDay,
        day: 2, payloadValue: 100, cycleValue: 10);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcus, startOfDeviceDay, day: 2,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None); 
    }

    [TestMethod]
    [DatabaseTest]
    public void EPAheadButInWindowShouldBeMatched()
    {
      Initialize();

      int offsetMs = -tolerance * 1000 / 2;

      SetupNH_DATA_Engine_ForDay(asset.AssetID, offsetMs, startOfDeviceDay,
          day: 2, runtimeHours: 94, idleHours: 12, totalFuel: 12, idleFuel: null);

      List<FactPayloadCycleUtilizationDaily> fpcus = ExecuteETL();

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcus, 0, startOfDeviceDay,
        day: 2, payloadValue: 100, cycleValue: 10);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcus, startOfDeviceDay, day: 2,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);
    }

    [TestMethod]
    [DatabaseTest]
    public void EPBehindButAtWindowBoundaryShouldBeMatched()
    {
      Initialize();

      int offsetMs = tolerance * 1000;

      SetupNH_DATA_Engine_ForDay(asset.AssetID, offsetMs, startOfDeviceDay,
          day: 2, runtimeHours: 94, idleHours: 12, totalFuel: 12, idleFuel: null);

      List<FactPayloadCycleUtilizationDaily> fpcus = ExecuteETL();

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcus, 0, startOfDeviceDay,
        day: 2, payloadValue: 100, cycleValue: 10);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcus, startOfDeviceDay, day: 2,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);
    }

    [TestMethod]
    [DatabaseTest]
    public void EPAheadButAtWindowBoundaryShouldBeMatched()
    {
      Initialize();

      int offsetMs = -tolerance * 1000;

      SetupNH_DATA_Engine_ForDay(asset.AssetID, offsetMs, startOfDeviceDay,
          day: 2, runtimeHours: 94, idleHours: 12, totalFuel: 12, idleFuel: null);

      List<FactPayloadCycleUtilizationDaily> fpcus = ExecuteETL();

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcus, 0, startOfDeviceDay,
        day: 2, payloadValue: 100, cycleValue: 10);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcus, startOfDeviceDay, day: 2,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None);
    }

    [TestMethod]
    [DatabaseTest]
    public void EPBehindAndOutsideWindowShouldNotBeMatched()
    {
      Initialize();

      int offsetMs = tolerance * 1000 * 2;

      SetupNH_DATA_Engine_ForDay(asset.AssetID, offsetMs, startOfDeviceDay,
          day: 2, runtimeHours: 94, idleHours: 12, totalFuel: 12, idleFuel: null);

      List<FactPayloadCycleUtilizationDaily> fpcus = ExecuteETL();

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcus, 0, startOfDeviceDay,
        day: 2, payloadValue: 100, cycleValue: 10);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcus, startOfDeviceDay, day: 2,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue); 
    }

    [TestMethod]
    [DatabaseTest]
    public void EPAheadAndOutsideWindowShouldNotBeMatched()
    {
      Initialize();

      int offsetMs = -tolerance * 1000 * 2;

      SetupNH_DATA_Engine_ForDay(asset.AssetID, offsetMs, startOfDeviceDay,
          day: 2, runtimeHours: 94, idleHours: 12, totalFuel: 12, idleFuel: null);

      List<FactPayloadCycleUtilizationDaily> fpcus = ExecuteETL();

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcus, 0, startOfDeviceDay,
        day: 2, payloadValue: 100, cycleValue: 10);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcus, startOfDeviceDay, day: 2,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.MissingMeterValue); 
    }

    [TestMethod]
    [DatabaseTest]
    public void WhenMultipleEPArePresentTheClosestShouldBeUsed()
    {
      Initialize();

      int offsetMs = tolerance * 1000 / 2;

      SetupNH_DATA_Engine_ForDay(asset.AssetID, offsetMs, startOfDeviceDay,
          day: 2, runtimeHours: 100, idleHours: 20, totalFuel: 15, idleFuel: null);

      offsetMs = -tolerance * 1000 / 3; // this one's closer

      SetupNH_DATA_Engine_ForDay(asset.AssetID, offsetMs, startOfDeviceDay,
          day: 2, runtimeHours: 94, idleHours: 12, totalFuel: 12, idleFuel: null);

      List<FactPayloadCycleUtilizationDaily> fpcus = ExecuteETL();

      AssertNH_RPTFactPayloadCycleUtilizationDaily(fpcus, 0, startOfDeviceDay,
        day: 2, payloadValue: 100, cycleValue: 10);
      AssertNH_RPTFactPayloadCycleUtilizationFlags(fpcus, startOfDeviceDay, day: 2,
        payloadMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        cycleMeterDeltaCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        runtimeHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingHoursCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        totalFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None,
        workingFuelConsumedGallonsCalloutTypeID: DimUtilizationCalloutTypeEnum.None); 
    }

    private List<FactPayloadCycleUtilizationDaily> ExecuteETL()
    {
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      ExecutePayloadCalloutPopulationScript();

      List<FactPayloadCycleUtilizationDaily> fpcu =
        (from fpcud in Ctx.RptContext.FactPayloadCycleUtilizationDailyReadOnly
         where fpcud.ifk_DimAssetID == asset.AssetID
               && fpcud.ifk_AssetPriorKeyDate != null
         orderby fpcud.ifk_AssetKeyDate
         select fpcud).ToList();

      return fpcu;
    }
  }
}
